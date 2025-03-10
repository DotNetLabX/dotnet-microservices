using Blocks.Core.Cache;
using Review.Domain.Shared.Enums;

namespace Review.Domain.Articles;

public class ArticleStageTransition : IMetadataEntity, ICacheable
{
    public ArticleStage CurrentStage { get; set; }
    public ArticleActionType ActionType { get; set; }
    public ArticleStage DestinationStage { get; set; }
}
