using Articles.Abstractions.Enums;
using Blocks.Exceptions;
using Production.Application.StateMachines;
using Production.Domain.Articles;
using Production.Domain.Assets;
using Production.Domain.Assets.Enums;
using Production.Domain.Shared;
using Production.Persistence.Repositories;

namespace Production.API.Features.Shared;

public abstract class AssetBaseEndpoint<TCommand, TResponse>(AssetTypeRepository assetTypeRepository, AssetStateMachineFactory stateMachineFactory)
		: Endpoint<TCommand, TResponse>
		where TCommand : IAssetAction
{
		protected readonly AssetTypeRepository _assetTypeRepository = assetTypeRepository;
		protected readonly AssetStateMachineFactory _stateMachineFactory = stateMachineFactory;
		protected Article _article = null!;

		protected virtual ArticleStage NextStage => _article!.Stage; // by default the stage doesn't change

		protected virtual void CheckAndThrowStateTransition(Asset asset, AssetActionType actionType)
		{
				if (!_stateMachineFactory(asset.State).CanFire(asset.Article.Stage, asset.Type, actionType))
						throw new BadRequestException("Action not allowed");
		}

		protected Asset CreateAsset(AssetType assetType, byte assetNumber)
		{
				var assetTypeEntity = _assetTypeRepository.GetById(assetType);
				return _article.CreateAsset(assetTypeEntity, assetNumber);
		}
}
