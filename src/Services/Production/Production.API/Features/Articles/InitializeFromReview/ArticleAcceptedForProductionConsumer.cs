using Articles.Abstractions.Enums;
using Articles.IntegrationEvents.Contracts.Articles;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using FileStorage.Contracts;
using MassTransit;
using Production.Domain.Articles;
using Production.Domain.Assets;
using Production.Domain.Shared;
using Production.Domain.Shared.Enums;
using Production.Persistence;
using Production.Persistence.Repositories;

namespace Production.API.Features.Articles.InitializeFromReview;

public sealed class ArticleAcceptedForProductionConsumer(
    ProductionDbContext dbContext,
    ArticleRepository articleRepository,
    Repository<Person> personRepository,
    Repository<Journal> journalRepository,
    AssetTypeRepository assetTypeRepository,
    IFileService<ReviewFileStorageOptions> reviewFileService,
    IFileService azureBlobFileService
) : IConsumer<ArticleAcceptedForProductionEvent>
{
    public async Task Consume(ConsumeContext<ArticleAcceptedForProductionEvent> context)
    {
        var articleDto = context.Message.Article;
        var ct = context.CancellationToken;

        if (await articleRepository.ExistsAsync(articleDto.Id))
            return;

        var journal = await GetOrCreateJournal(articleDto);
        var actors = await CreateActors(articleDto, ct);
        var assets = await CreateAssets(articleDto, ct);

        var action = new ArticleAction
        {
            ArticleId = articleDto.Id,
            ActionType = ArticleActionType.AcceptForProduction,
            CreatedById = articleDto.SubmittedBy.UserId ?? 0,
        };

        var article = Article.FromReview(articleDto, actors, assets, action);
        await articleRepository.AddAsync(article);

        await dbContext.SaveChangesAsync(ct);
    }

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

    private async Task<IEnumerable<ArticleActor>> CreateActors(ArticleDto articleDto, CancellationToken ct)
    {
        var actors = new List<ArticleActor>();
        foreach (var actorDto in articleDto.Actors)
        {
            if (actorDto.Role != UserRoleType.AUT && actorDto.Role != UserRoleType.CORAUT)
                continue;

            var person = await personRepository.FindByIdAsync(actorDto.Person.Id);
            if (person is null)
            {
                person = new Author
                {
                    Id = actorDto.Person.Id,
                    FirstName = actorDto.Person.FirstName,
                    LastName = actorDto.Person.LastName,
                    Email = actorDto.Person.Email,
                    Affiliation = actorDto.Person.Affiliation ?? string.Empty,
                    UserId = actorDto.Person.UserId,
                };
                await personRepository.AddAsync(person);
            }

            actors.Add(new ArticleActor { PersonId = person.Id, Role = actorDto.Role });
        }

        return actors;
    }

    private async Task<IEnumerable<Asset>> CreateAssets(ArticleDto articleDto, CancellationToken ct)
    {
        var assets = new List<Asset>();
        foreach (var assetDto in articleDto.Assets)
        {
            var assetType = assetTypeRepository.GetById(assetDto.Type);
            var asset = Asset.CreateFromReview(articleDto.Id, assetType, (byte)(assetDto.Number - 1));

            var (fileStream, fileMetadata) = await reviewFileService.DownloadAsync(assetDto.File.FileServerId, ct);

            var uploadedMetadata = await azureBlobFileService.UploadAsync(
                new FileUploadRequest(fileMetadata.StoragePath, fileMetadata.FileName, fileMetadata.ContentType, fileMetadata.FileSize),
                fileStream,
                ct: ct);

            asset.CreateAndAddFile(uploadedMetadata, assetType);

            assets.Add(asset);
        }

        return assets;
    }
}
