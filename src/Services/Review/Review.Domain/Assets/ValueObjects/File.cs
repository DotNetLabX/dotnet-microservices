using Review.Domain.Assets.ValueObjects;

namespace Review.Domain.Assets;

public partial class File : IDomainObject, IValueObject
{
    //insight - difference between required, null!, default!
    public required string OriginalName { get; init; } = default!;    
    //insight - server id, create the file server server both mongo & azure blob
    public required string FileServerId { get; init; } = default!;
    public required long Size { get; init; }
    //insight - differencce between required property and IsRequired Ef.Core configuration
    public required FileName Name { get; init; }
		public required FileExtension Extension { get; init; } = default!;
}