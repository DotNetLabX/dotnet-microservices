namespace Production.Domain.Assets;

public class AssetCurrentFileLink : IAssociationEntity
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
    public int FileId { get; set; }
    public File File { get; set; } = null!;
}
