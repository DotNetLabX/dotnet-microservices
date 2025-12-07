using Blocks.Domain;
using FileStorage.Contracts;
using Mapster;
using Production.Domain.Articles;
using Production.Domain.Assets.Enums;
using Production.Domain.Assets.Events;
using Production.Domain.Assets.ValueObjects;

namespace Production.Domain.Assets;

public partial class Asset
{
    private Asset() {/* use factory method*/}

    public string GenerateStorageFilePath(string fileName) 
        => $"Articles/{ArticleId}/{Name}/{Number}/{CalculateNextVersion()}/{fileName}";
    
    public byte CalculateNextVersion() => (byte)(CurrentVersion + 1);
    public byte CurrentVersion => CurrentFile?.Version.Value ?? 0;

    public string GenerateFileName(string fileExtension)
    {
        var assetName = Name.Value.Replace("'", "").Replace(" ", "-");
        var assetNumber = Number > 0 ? Number.ToString() : string.Empty;
        return $"{assetName}{assetNumber}.{fileExtension}";
    }

    public bool IsNewVersion => this.CurrentFileLink != null;

    public bool IsFileRequested => this.State == AssetState.Requested;

    public static Asset Create(Article article, AssetTypeDefinition type, byte assetNumber = 0)
    {
        //insight - value objects for AssetName & AssetNumber, encapsulate validation            
        return new Asset()
        {
            ArticleId = article.Id,
            Article = article,
            Number = AssetNumber.FromNumber(assetNumber, type),
            Name = AssetName.FromAssetType(type),
            Type = type.Name,
            //TypeDefinition = type,
            CategoryId = type.DefaultCategoryId,
            State = AssetState.None
        };
    }

    public void SetState(AssetState newState, IArticleAction<AssetActionType> action)
    {
        this.State = newState;
        this.LastModifiedOn = DateTime.UtcNow;
        this.LastModifiedById = action.CreatedById;
        AddAction(action);
    }

    public void CancelRequest(IArticleAction<AssetActionType> action)
    {
        if (this.State != AssetState.Requested)
            throw new DomainException("Wrong status");

        //this.Status = newStatus;
        this.LastModifiedOn = DateTime.UtcNow;
        this.LastModifiedById = action.CreatedById;
        AddAction(action);
    }

    public File CreateAndAddFile(FileMetadata uploadResponse, AssetTypeDefinition assetTypeDefinition)
    {
        var file = File.CreateFile(uploadResponse, this, assetTypeDefinition);

        _files.Add(file);
        CurrentFileLink = new AssetCurrentFileLink() { File = file };
        State = AssetState.Uploaded;
        return file;
    }

    private void AddAction(IArticleAction<AssetActionType> action)
    {
        _actions.Add(action.Adapt<AssetAction>());
        AddDomainEvent(new AssetActionExecuted(action, this.Article.Stage, this.Type, this.Number, this.CurrentFile));
    }
}
