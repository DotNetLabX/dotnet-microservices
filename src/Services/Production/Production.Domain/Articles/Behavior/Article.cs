using Articles.Abstractions.Events;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Blocks.Domain;
using Production.Domain.Articles.Events;
using Production.Domain.Assets;
using Production.Domain.StateMachines;

namespace Production.Domain.Articles;

public partial class Article
{
    public static Article FromReview(ArticleDto articleDto, IEnumerable<ArticleActor> actors, IEnumerable<Asset> assets, IArticleAction action)
    {
        var article = new Article
        {
            Id = articleDto.Id,
            Title = articleDto.Title,
            Doi = articleDto.Doi ?? string.Empty,
            JournalId = articleDto.Journal.Id,
            SubmittedById = articleDto.SubmittedBy.Id,
            SubmittedOn = articleDto.SubmittedOn,
            AcceptedOn = articleDto.AcceptedOn ?? DateTime.UtcNow,
        };

        article._actors.AddRange(actors);
        article._assets.AddRange(assets);

        // Stage defaults to None (0), transitions via SetStage to raise ArticleStageChanged(None → Accepted)
        article.SetStage(articleDto.Stage, action);
        article.AddDomainEvent(new ArticleAcceptedForProduction(article, action));

        return article;
    }


    public void SetStage(ArticleStage newStage, global::Articles.Abstractions.IArticleAction action)
    {
        if (newStage == Stage)
            return;

        var currentStage = Stage;
        Stage = newStage;
        LastModifiedOn = action.CreatedOn;
        LastModifiedById = action.CreatedById;

        _stageHistories.Add(new StageHistory { ArticleId = Id, StageId = newStage, StartDate = DateTime.UtcNow });
        AddDomainEvent(
            new ArticleStageChanged(currentStage, newStage, action)
            );
    }

    public void AssignTypesetter(Typesetter typesetter, ArticleStateMachineFactory stateMachineFactory, IArticleAction action)
    {
        if (!stateMachineFactory(Stage).CanFire(action.ActionType))
            throw new DomainException("Action not allowed for current stage");

        if (Typesetter is not null)
            throw new TypesetterAlreadyAssignedException("Typesetter already assigned");

        _actors.Add(new ArticleActor() { PersonId = typesetter.Id, Role = UserRoleType.TSOF });

        AddDomainEvent(new TypesetterAssigned(typesetter.Id, typesetter.UserId!.Value, action));

        SetStage(ArticleStage.InProduction, action);
    }

    public Asset CreateAsset(AssetTypeDefinition type, byte assetNumber = 0)
    {
        if (_assets.Exists(a => a.Type == type.Id && a.Number == assetNumber))
            throw new DomainException("Asset already exists");

        var asset = Asset.Create(this, type, assetNumber);
        _assets.Add(asset);        
        return asset;
    }
}
