using Articles.Abstractions;
using Articles.Abstractions.Enums;

namespace ArticleTimeline.Application.VariableResolvers;

public class TimelineResolverModel
{
		public IArticleAction Action { get; set; } = null!;
		public ArticleStage? NewStage { get; set; }
		public ArticleStage CurrentStage { get; set; }

		public AssetType? AssetType { get; set; }
		public int AssetNumber { get; set; }    
}
