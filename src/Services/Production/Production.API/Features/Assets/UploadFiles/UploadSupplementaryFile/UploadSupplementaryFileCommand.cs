using Articles.Abstractions.Enums;
using Production.API.Features.Assets.UploadFiles._Shared;
using Production.Domain.Assets.Enums;
using System.ComponentModel.DataAnnotations;

namespace Production.API.Features.Assets.UploadFiles.UploadSupplementaryFile;

public record UploadSupplementaryFileCommand : UploadFileCommand
{
		[Required]
		public byte AssetNumber { get; set; }

		internal override byte GetAssetNumber() => AssetNumber;
}

public abstract class UploadSupplementaryFileValidator : UploadFileValidator<UploadSupplementaryFileCommand>
{
		public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.SupplementaryAssets;
}