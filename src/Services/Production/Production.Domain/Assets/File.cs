using Production.Domain.Assets.ValueObjects;

namespace Production.Domain.Assets;

public partial class File : AggregateRoot
{
    //insight - difference between required, null!, default!
    public string OriginalName { get; init; } = default!;
    
    //insight - server id, create the file server server both mongo & azure blob
    public string FileServerId { get; init; } = default!;

    public long Size { get; set; }

    public FileVersion Version { get; init; } = null!;

    public required FileName Name { get; init; }

    public FileExtension Extension { get; init; } = default!;

    public int AssetId { get; private set; }

    public virtual Asset? Asset { get; private set; }
}
