using Blocks.Entitities;
using Blocks.Core.Cache;
using ArticleTimeline.Domain.Enums;

namespace ArticleTimeline.Domain;

public class TimelineTemplate : ICacheable, IMetadataEntity
{
    public SourceType SourceType { get; init; }
    public string SourceId { get; init; } = null!;
    public string TitleTemplate { get; init; } = default!;
    public string DescriptionTemplate { get; init; } = default!;
}
