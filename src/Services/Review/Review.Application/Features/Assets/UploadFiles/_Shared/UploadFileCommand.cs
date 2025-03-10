using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Blocks.Http.Abstractions;
using Review.Application.Features.Articles.Shared;
using Review.Domain.Assets;
using ValidationResult = FluentValidation.Results.ValidationResult;
using ValidationMessages = Review.Application.Features.Articles.Shared.ValidationMessages;

namespace Review.Application.Features.Assets.UploadFiles.Shared;

public abstract record UploadFileCommand : ArticleCommand
{
    /// <summary>
    /// The asset type of the file.
    /// </summary>
    [Required]
    public AssetType AssetType { get; init; }

    /// <summary>
    /// The file to be uploaded.
    /// </summary>
    [Required]
    public IFormFile File { get; init; } = null!;

    //public override ArticleActionType ActionType => ArticleActionType.UploadAsset;
}

public abstract class UploadFileValidator<TUploadFileCommand> : ArticleCommandValidator<TUploadFileCommand>
                where TUploadFileCommand : UploadFileCommand
{
    private readonly AssetTypeDefinitionRepository _assetTypeRepository;
    private AssetTypeDefinition _assetTypeDefinition = null!;

    public UploadFileValidator(AssetTypeDefinitionRepository assetTypeRepository)
    {
        _assetTypeRepository = assetTypeRepository;

        RuleFor(r => r.AssetType).Must(IsAssetTypeAllowed)
                .WithMessage(command => ValidationMessages.InvalidAssetType.FormatWith(_assetTypeDefinition.Name));
        RuleFor(command => command.File).Must(IsFileSizeValid)
                .WithMessage(command => ValidationMessages.InvalidFileSize.FormatWith(_assetTypeDefinition.MaxFileSizeInMB));
        RuleFor(command => command.File).Must(IsFileExtensionValid)
                .WithMessage(command => ValidationMessages.InvalidFileExtension.FormatWith(_assetTypeDefinition.Name, command.File.GetExtension()));
    }

    public override ValidationResult Validate(ValidationContext<TUploadFileCommand> context)
    {
        // we are overriding it to get the asset type definition before the validation
        _assetTypeDefinition = _assetTypeRepository.GetById(context.InstanceToValidate.AssetType);
        return base.Validate(context);
    }

    public override Task<ValidationResult> ValidateAsync(ValidationContext<TUploadFileCommand> context, CancellationToken cancellation = default)
    {
        // we are overriding it to get the asset type definition before the validation
        _assetTypeDefinition = _assetTypeRepository.GetById(context.InstanceToValidate.AssetType);
        return base.ValidateAsync(context, cancellation);
    }


    private bool IsAssetTypeAllowed(AssetType assetType)
            => AllowedAssetTypes.Contains(assetType);

    private bool IsFileSizeValid(IFormFile file)
            => file.Length <= _assetTypeDefinition.MaxFileSizeInBytes;

    public bool IsFileExtensionValid(IFormFile file)
            => _assetTypeDefinition.AllowedFileExtensions.IsValidExtension(file.GetExtension());

    public abstract IReadOnlyCollection<AssetType> AllowedAssetTypes { get; }
}
