using Production.Domain.Articles;
using Production.Domain.Assets.Enums;
using Production.Domain.Assets.ValueObjects;

namespace Production.Domain.Assets;

public partial class Asset : AggregateRoot
{
    public AssetName Name { get; private set; } = null!;
    public AssetNumber Number { get; private set; } = null!;    
    //insight - keep them as enum because they change quite rarely
    public AssetState State { get; private set; }
    public AssetCategory CategoryId { get; private set; }

    public AssetType Type { get; private set; }
    public virtual AssetTypeDefinition TypeDefinition { get; private set; } = null!;
    
    public int ArticleId { get; private set; }
    public virtual Article Article { get; private set; } = null!;


    private readonly List<File> _files = new();
    public virtual IReadOnlyList<File> Files => _files.AsReadOnly();

    
    private readonly List<AssetAction> _actions = new ();
    public virtual IReadOnlyList<AssetAction> Actions => _actions.AsReadOnly();

    
    public virtual AssetCurrentFileLink? CurrentFileLink { get; private set; } = null!;
    public File? CurrentFile => this.CurrentFileLink?.File;
}
