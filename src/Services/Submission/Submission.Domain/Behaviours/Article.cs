using Mapster;
using Blocks.Domain;
using Submission.Domain.StateMachines;

namespace Submission.Domain.Entities;

public partial class Article
{
    public void SetStage(ArticleStage newStage, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory stateMachineFactory)
    {
        stateMachineFactory.ValidateStageTransition(Stage, action.ActionType);

        if (newStage == Stage) // there is no transition to be done
            return;

        var currentStage = Stage;
        Stage = newStage;
        LastModifiedOn = action.CreatedOn;
        LastModifiedById = action.CreatedById;

        _stageHistories.Add(
            new StageHistory { ArticleId = Id, StageId = newStage, StartDate = DateTime.UtcNow });
        
        AddDomainEvent(
            new ArticleStageChanged(currentStage, newStage, action));
    }

    public void AssignAuthor(Author author, HashSet<ContributionArea> contributionAreas, bool isCorrespondingAuthor, IArticleAction<ArticleActionType> action)
    {
        // todo - The actions which don't generate stage transition needs to be restricted on the stage.
        // For instance we can only assign author in Created and ManuscriptUploaded stages.
        var role = isCorrespondingAuthor ? UserRoleType.CORAUT : UserRoleType.AUT;        
        
        if (_actors.Exists(a => a.PersonId == author.Id && a.Role == role))
            throw new DomainException($"Author {author.Email} is already assigned to the article");

        _actors.Add(new ArticleAuthor()
        {
            ContributionAreas = contributionAreas,
            Person = author,
            //PersonId = author.Id, 
            Role = role
        });

        AddDomainEvent(
            new AuthorAssigned(author, action));
        AddAction(action);
    }

    public void Approve(Person editor, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory _stateMachineFactory)
    {
        _actors.Add(new ArticleActor
        {
            Person = editor,
            Role = UserRoleType.REVED
        });

        SetStage(ArticleStage.InitialApproved, action, _stateMachineFactory);
        
        AddDomainEvent(new ArticleApproved(this, action));
    }

    public void Reject(IArticleAction<ArticleActionType> action, ArticleStateMachineFactory _stateMachineFactory)
    {
        SetStage(ArticleStage.InitialRejected, action, _stateMachineFactory);

        AddDomainEvent(new ArticleRejected(this, action));
    }

    public void Submit(IArticleAction<ArticleActionType> action, ArticleStateMachineFactory _stateMachineFactory)
    {
        // check if all mandatory contribution areas are covered
        var contributionAreas = _actors.OfType<ArticleAuthor>()
            .SelectMany(author => author.ContributionAreas)
            .ToHashSet();

        var missingMandatoryAreas = ContributionAreaCategories.MandatoryAreas
            .Except(contributionAreas)
            .ToList();
        
        if (missingMandatoryAreas.Count > 0)
            throw new DomainException($"Cannot submit article: Missing mandatory contribution areas: {string.Join(", ", missingMandatoryAreas)}");

        SubmittedById = action.CreatedById;
        SubmittedOn = action.CreatedOn;

        // todo add an ArticleSubmitted domain event
        SetStage(ArticleStage.Submitted, action, _stateMachineFactory);
    }

    public Asset CreateAsset(AssetTypeDefinition type, IArticleAction<ArticleActionType> action)
    {
        var assetCount = _assets
            .Where(a => a.Type == type.Id)
            .Count();

        if (assetCount >= type.MaxAssetCount)
            throw new DomainException($"The maximum number of files, {type.MaxAssetCount}, allowed for {type.Name.ToString()} was already reached");

        var asset = Asset.Create(this, type, assetCount, action);
        _assets.Add(asset);

        AddAction(action);
        return asset;
    }

    public Asset GetOrCreateAsset(AssetTypeDefinition assetType, IArticleAction<ArticleActionType> action)
    {
        Asset? asset = default;

        if (!assetType.AllowsMultipleAssets) // if the asset type doesn't support multiple assets, we are overriding the single one.
            asset = this.Assets.SingleOrDefault(a => a.Type == assetType.Id);

        if (asset is null)
            asset = this.CreateAsset(assetType, action);

        return asset;
    }

    private void AddAction(IArticleAction<ArticleActionType> action)
    {
        _actions.Add(action.Adapt<ArticleAction>());
        AddDomainEvent(new ArticleActionExecuted(this, action));
    }
}
