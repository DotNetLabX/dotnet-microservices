using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Blocks.Core;
using FileStorage.Contracts;
using Review.Domain.Articles;
using Review.Domain.Assets.Enums;
using Review.Domain.Assets.Events;
using Review.Domain.Assets.ValueObjects;

namespace Review.Domain.Assets;

public partial class Asset
{
    private Asset() {/* use factory method*/}

    public string GenerateStorageFilePath(string fileName) 
        => $"Articles/{ArticleId}/{Name}/{fileName}";

    //insight - use internal factory method so that the Asset can be created only in the Domain
    internal static Asset Create(Article article, AssetTypeDefinition type, int assetCount, IArticleAction action)
    {
        //insight - value objects for AssetName & AssetNumber, encapsulate validation            
        return new Asset()
        {
            ArticleId = article.Id,
            Article = article,
            Name = AssetName.Create(type, assetCount),
            Number = AssetNumber.Create(type, assetCount),
            Type = type.Name,
            TypeDefinition = type,
            State = AssetState.None,
            CreatedById = action.CreatedById,
            CreatedOn = action.CreatedOn
        };
    }


    public File CreateFile(FileMetadata fileMetadata, AssetTypeDefinition assetType, IArticleAction action)
    {
        File = File.CreateFile(fileMetadata, this, assetType);

        State = AssetState.Uploaded;

        this.AddDomainEvent(new FileUploaded(this, action));
        return File;
    }

    public File CreateFile(FileMetadata fileMetadata, AssetTypeDefinition assetType)
    {
        File = File.CreateFile(fileMetadata, this, assetType);

        State = AssetState.Uploaded;

        return File;
    }

    public static Asset CreateFromSubmission(AssetDto assetDto, ArticleDto articleDto)
    {
        //insight - value objects for AssetName & AssetNumber, encapsulate validation            
        var asset = new Asset()
        {
            ArticleId = articleDto.Id,
            Name = AssetName.FromSubmission(assetDto.Name),
            Number = AssetNumber.FrmSubmission(assetDto.Number),
            Type = assetDto.Type,
            State = AssetState.Uploaded,
            CreatedOn = articleDto.SubmittedOn,
            CreatedById = articleDto.SubmittedBy.Id
        };

        return asset;
    }
}
