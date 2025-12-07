using Review.Domain.Articles;
using Review.Domain.Assets.Enums;
using Review.Domain.Assets.ValueObjects;

namespace Review.Domain.Assets;

public partial class Asset : AggregateRoot
{
    public AssetName Name { get; private set; } = null!;
    public AssetNumber Number { get; private set; } = null!;

    //insight - keep the following properties as enums because they change quite rarely
    public AssetState State { get; private set; }

    public AssetType Type { get; private set; }
    public virtual AssetTypeDefinition TypeDefinition { get; private set; } = null!;
    
    public int ArticleId { get; private set; }
    public virtual Article Article { get; private set; } = null!;

    public File File { get; set; } = null!; 
}
