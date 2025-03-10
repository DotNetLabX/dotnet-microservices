using Articles.Abstractions.Enums;
using Blocks.Entitities;
using Blocks.Core.Cache;
using System.Collections.Immutable;
using Production.Domain.Assets.Enums;

namespace Production.Domain.Assets;

public class AssetStateTransitionCondition : Entity, ICacheable
{
		public ArticleStage ArticleStage { get; set; }
		public IReadOnlyList<AssetType> AssetTypes { get; init; } = ImmutableList<AssetType>.Empty;
		public IReadOnlyList<AssetActionType> ActionTypes { get; init; } = ImmutableList<AssetActionType>.Empty;
}
