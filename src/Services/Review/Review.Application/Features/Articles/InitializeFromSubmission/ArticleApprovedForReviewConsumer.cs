using Articles.IntegrationEvents.Contracts.Articles;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Blocks.Domain;
using FileStorage.Contracts;
using MassTransit;
using Review.Application.FileStorage;
using Review.Domain.Assets;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Articles.InitializeFromSubmission;

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

        //insight - inbox pattern vs simple business rules(chek if the article already exists)
        await _articleRepository.EnsureNotExistsOrThrowAsync(articleDto.Id, context.CancellationToken);
        
        var actors = await CreateActors(articleDto);

        var assets = await CreateAssets(articleDto, context.CancellationToken);

        var journal = await GetOrCreateJournal(articleDto);


        var action = new ArticleAction
        {
            ArticleId = articleDto.Id,
            ActionType = ArticleActionType.ApproveForReview,
            CreatedById = actors.Single(a => a.Role == UserRoleType.REVED).Person.UserId!.Value, // the editor who approved the article is the creator of the action
        };

        var article = Article.FromSubmission(articleDto, actors, assets, _stateMachineFactory, action);
        await _articleRepository.AddAsync(article);


        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }

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

                actors.Add(new ArticleActor
                {
                    Person = person,
                    Role = actorDto.Role
                });
            }
            else
            {
                // decide : throw or just log a warning
                throw new DomainException($"Unknow role for {actorDto.Person.Email}");
            }
        }

        return actors;
    }
}
