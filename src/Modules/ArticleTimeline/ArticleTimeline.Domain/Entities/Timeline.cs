using Blocks.Entitities;
using Articles.Abstractions.Enums;
using ArticleTimeline.Domain.Enums;

namespace ArticleTimeline.Domain;

public class Timeline : AggregateRoot
{
    public int ArticleId { get; init; }
    public ArticleStage NewStage { get; init; }
		public ArticleStage CurrentStage { get; init; }
		public SourceType SourceType { get; init; }
    public string SourceId { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;

		//public int TemplateId { get; set; }
		public TimelineTemplate Template { get; private set; } = null!;
}
