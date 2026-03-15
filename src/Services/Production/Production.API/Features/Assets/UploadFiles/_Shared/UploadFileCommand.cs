using Blocks.Core;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Articles.Abstractions.Enums;
using Production.API.Features.Shared;
using Production.Domain.Assets.Enums;
using Production.Persistence.Repositories;

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
    public UploadFileValidator(AssetTypeRepository assetTypeRepository)
    {
        RuleFor(r => r.AssetType).Must(a => AllowedAssetTypes.Contains(a)).WithMessage("AssetType not allowed");
        RuleFor(r => r.File)
            .Must((command, file) =>
            {
                var assetType = assetTypeRepository.GetById(command.AssetType);
                return file.Length <= assetType.MaxFileSizeInMB * 1024 * 1024;
            })
            .WithMessage(command => ValidatorsMessagesConstants.InvalidFileSize
                .FormatWith(assetTypeRepository.GetById(command.AssetType).MaxFileSizeInMB));
    }

    public abstract IReadOnlyCollection<AssetType> AllowedAssetTypes { get; }
}
