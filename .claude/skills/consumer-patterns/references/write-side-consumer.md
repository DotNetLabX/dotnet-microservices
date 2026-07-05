# Template: write-side article-creation consumer

Lifted from `Review.Application/Features/Articles/InitializeFromSubmission/ArticleApprovedForReviewConsumer.cs`. This is the richest shape — all five phases, actor + asset hydration, cross-stage file migration, and the **throw-if-exists** idempotency variant. Production's `ArticleAcceptedForProductionConsumer` is the same shape with the **ExistsAsync-then-return** variant (see the alternate line in phase 2).

Feature-folder convention: the consumer lives in `Features/{Area}/InitializeFrom{Stage}/`.

```csharp
using Articles.IntegrationEvents.Contracts.Articles;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Blocks.Domain;
using FileStorage.Contracts;
using MassTransit;
using Review.Application.FileStorage;
using Review.Domain.Assets;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Articles.InitializeFromSubmission;

// PHASE 1 — consumer class + primary-constructor dependencies.
// Persistence collaborators are CONCRETE: the service DbContext, Repository<T>, {X}Repository.
// Non-persistence collaborators ARE interfaces / delegates: IFileService, FileServiceFactory,
// ArticleStateMachineFactory. (consumers-1)
public class ArticleApprovedForReviewConsumer(
    ReviewDbContext _dbContext,
    ArticleRepository _articleRepository,
    Repository<Person> _personRepository,
    Repository<Journal> _journalRepository,
    AssetTypeDefinitionRepository _assetTypeRepository,
    IFileService _reviewFileService,
    FileServiceFactory _fileServiceFactory,
    ArticleStateMachineFactory _stateMachineFactory
    )
    : IConsumer<ArticleApprovedForReviewEvent>
{
    public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
    {
        var articleDto = context.Message.Article;

        // PHASE 2 — idempotency. THROW-IF-EXISTS: a second delivery means an article was created
        // twice, which is a bug we want surfaced. (consumers-2)
        // insight - inbox pattern vs simple business rules (check if the article already exists)
        await _articleRepository.EnsureNotExistsOrThrowAsync(articleDto.Id, context.CancellationToken);
        // ALTERNATE (Production variant — swallow the redelivery instead of throwing):
        //   if (await _articleRepository.ExistsAsync(articleDto.Id)) return;

        // PHASE 3 — local reference-data hydration (see GetOrCreateJournal / CreateActors below).
        var actors = await CreateActors(articleDto);
        var assets = await CreateAssets(articleDto, context.CancellationToken);
        var journal = await GetOrCreateJournal(articleDto);

        // PHASE 4 — domain mutation through the aggregate factory so invariants run.
        var action = new ArticleAction
        {
            ArticleId = articleDto.Id,
            ActionType = ArticleActionType.ApproveForReview,
            CreatedById = actors.Single(a => a.Role == UserRoleType.REVED).Person.UserId!.Value,
        };

        var article = Article.FromSubmission(articleDto, actors, assets, _stateMachineFactory, action);
        await _articleRepository.AddAsync(article);

        // PHASE 5 — one save. The repository is the unit of work; no separate commit. (persistence-1)
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }

    // PHASE 3 helper — GetOrCreate: query local first, materialize from the event DTO only if
    // missing. Materialize with Mapster Adapt<T>() OR by hand — both sanctioned. (consumers-3)
    private async Task<Journal> GetOrCreateJournal(ArticleDto articleDto)
    {
        var journal = await _journalRepository.FindByIdAsync(articleDto.Journal.Id);
        if (journal is null)
        {
            journal = articleDto.Journal.Adapt<Journal>();
            await _journalRepository.AddAsync(journal);
        }

        return journal;
    }

    // PHASE 3/4 helper — assets carry files, so migrate the bytes: DownloadAsync from the sending
    // stage's store, UploadAsync into this stage's store. Each service owns its storage.
    // (consumers-4 — file-storage mechanics live in the file-storage-patterns skill.)
    private async Task<IEnumerable<Asset>> CreateAssets(ArticleDto articleDto, CancellationToken ct = default)
    {
        var submissionFileService = _fileServiceFactory(FileStorageType.Submission);

        var assets = new List<Asset>();
        foreach (var assetDto in articleDto.Assets)
        {
            var assetTypeDefinition = _assetTypeRepository.GetById(assetDto.Type);
            var asset = Asset.CreateFromSubmission(assetDto, articleDto);

            var (fileStream, fileMetadata) = await submissionFileService.DownloadAsync(assetDto.File.FileServerId, ct);

            fileMetadata = await _reviewFileService.UploadAsync(
                new FileUploadRequest(fileMetadata.StoragePath, fileMetadata.FileName, fileMetadata.ContentType, fileMetadata.FileSize),
                fileStream);

            asset.CreateFile(fileMetadata, assetTypeDefinition);
            assets.Add(asset);
        }

        return assets;
    }

    private async Task<IEnumerable<ArticleActor>> CreateActors(ArticleDto articleDto)
    {
        var actors = new List<ArticleActor>();
        foreach (var actorDto in articleDto.Actors)
        {
            var person = await _personRepository.GetByIdAsync(actorDto.Person.Id);

            if (actorDto.Role == UserRoleType.AUT || actorDto.Role == UserRoleType.CORAUT)
            {
                if (person is null)
                {
                    person = actorDto.Person.Adapt<Author>();
                    await _personRepository.AddAsync(person);
                }

                actors.Add(new ArticleAuthor
                {
                    Person = person,
                    Role = actorDto.Role,
                    ContributionAreas = actorDto.ContributionAreas
                });
            }
            else if (actorDto.Role == UserRoleType.REVED)
            {
                if (person is null)
                {
                    person = actorDto.Person.Adapt<Editor>();
                    await _personRepository.AddAsync(person);
                }

                actors.Add(new ArticleActor { Person = person, Role = actorDto.Role });
            }
            else
            {
                throw new DomainException($"Unknown role for {actorDto.Person.Email}");
            }
        }

        return actors;
    }
}
```

## Hand-constructed hydration (Production variant)

When you do not want Mapster, materialize by hand — Production's `GetOrCreateJournal` does this because it also stamps a local-only `DefaultTypesetterId` the event never carries:

```csharp
private async Task<Journal> GetOrCreateJournal(ArticleDto articleDto)
{
    var journal = await journalRepository.FindByIdAsync(articleDto.Journal.Id);
    if (journal is null)
    {
        var defaultTypesetter = dbContext.Typesetters.First(t => t.IsDefault == true);
        journal = new Journal
        {
            Id = articleDto.Journal.Id,
            Name = articleDto.Journal.Name,
            Abbreviation = articleDto.Journal.Abbreviation,
            DefaultTypesetterId = defaultTypesetter.Id,
        };
        await journalRepository.AddAsync(journal);
    }

    return journal;
}
```

The idempotency helpers used above live in `Blocks.EntityFrameworkCore`:
- `EnsureNotExistsOrThrowAsync(id, ct)` — `RepositoryExtensions`, throws `BadRequestException` if the id exists.
- `ExistsAsync(id, ct)` — `Repository`, an `AnyAsync` by id.
