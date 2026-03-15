using Articles.Abstractions.Enums;
using Production.API.Features.Assets.UploadFiles._Shared;
using Production.Domain.Assets.Enums;
using Production.Persistence.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Production.API.Features.Assets.UploadFiles.UploadSupplementaryFile;

public record UploadSupplementaryFileCommand : UploadFileCommand
{
    [Required]
    public byte AssetNumber { get; set; }

    internal override byte GetAssetNumber() => AssetNumber;
}

public class UploadSupplementaryFileValidator : UploadFileValidator<UploadSupplementaryFileCommand>
{
    public UploadSupplementaryFileValidator(AssetTypeRepository assetTypeRepository) : base(assetTypeRepository) { }

    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.SupplementaryAssets;
}