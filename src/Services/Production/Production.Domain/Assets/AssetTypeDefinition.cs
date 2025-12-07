using Blocks.Core.Cache;
using Production.Domain.Assets.ValueObjects;


namespace Production.Domain.Assets;

//insight - mix enums & tables togheter
public partial class AssetTypeDefinition : EnumEntity<AssetType>, ICacheable
{
    public AssetCategory DefaultCategoryId { get; init; }
    public AllowedFileExtensions AllowedFileExtensions { get; init; } = null!;
    public string DefaultFileExtension { get; init; } = default!;
    public byte MaxNumber { get; init; }
    public byte MaxFileSizeInMB{ get; init; }
}
