using Blocks.Domain;
using Production.Domain.Assets;
using Production.Domain.Articles.Events;

namespace Production.Domain.Articles;

public partial class Article 
{
    public void SetStage<TActionType>(ArticleStage newStage, IArticleAction<TActionType> action)
        where TActionType : Enum
		{
        if (newStage == Stage)
            return;

        var currentStage = Stage;
				Stage = newStage;
        LastModifiedOn = action.CreatedOn;
				LastModifiedById = action.CreatedById;

				_stageHistories.Add(new StageHistory { ArticleId = Id, StageId = newStage, StartDate = DateTime.UtcNow });
				AddDomainEvent(
            new ArticleStageChanged<TActionType>(currentStage, newStage, action)
            );
    }

    public void AssignTypesetter(Typesetter typesetter, IArticleAction action)
    {
        if(this.Typesetter is not null)
				    throw new TypesetterAlreadyAssignedException("Typesetter aldready assigned");

				_contributors.Add(new ArticleContributor() { PersonId = typesetter.Id, Role = UserRoleType.TSOF});

        AddDomainEvent(new TypesetterAssigned(typesetter.Id, typesetter.UserId!.Value, action));
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
