using Production.Domain.Assets.Enums;

namespace Production.Domain.Assets;

public partial class AssetAction : Entity
{
    public int AssetId { get; set; }

    //insight - difference between default! & null!
    public string Comment { get; set; } = default!;

    public AssetActionType TypeId { get; set; }

    public virtual Asset Asset { get; set; } = null!;
}
