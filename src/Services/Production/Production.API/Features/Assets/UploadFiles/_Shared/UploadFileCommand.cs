using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Articles.Abstractions.Enums;
using Production.API.Features.Shared;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.UploadFiles._Shared;

public abstract record UploadFileCommand : AssetActionCommand<AssetActionResponse>
{
    /// <summary>
    /// The asset type of the file.
    /// </summary>
    [Required]
    public AssetType AssetType { get; set; }

    /// <summary>
    /// The file to be uploaded.
    /// </summary>
    [Required]
    public IFormFile File { get; set; }

		public override AssetActionType ActionType => AssetActionType.Upload;

    internal virtual byte GetAssetNumber() => 0;
}

public abstract class UploadFileValidator<TUploadFileCommand> : ArticleCommandValidator<TUploadFileCommand>
				where TUploadFileCommand : UploadFileCommand
{
		public UploadFileValidator()
		{
				RuleFor(r => r.AssetType).Must(a => AllowedAssetTypes.Contains(a)).WithMessage("AssetType not allowed");
				//RuleFor(r => r.File.Length)
		}

		public abstract IReadOnlyCollection<AssetType> AllowedAssetTypes { get; }
}
