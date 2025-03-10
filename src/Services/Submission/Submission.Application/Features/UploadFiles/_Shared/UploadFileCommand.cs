using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Blocks.Http.Abstractions;
using ValidationResult = FluentValidation.Results.ValidationResult;
using ValidationMessages = Submission.Application.Features.Shared.ValidationMessages;

namespace Submission.Application.Features.UploadFiles.Shared;

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

		public override ArticleActionType ActionType => ArticleActionType.UploadAsset;
}

public abstract class UploadFileValidator<TUploadFileCommand> : ArticleCommandValidator<TUploadFileCommand>
				where TUploadFileCommand : UploadFileCommand
{
    private readonly AssetTypeRepository _assetTypeRepository;
		private AssetTypeDefinition _assetTypeDefinition = null!;

		public UploadFileValidator(AssetTypeRepository assetTypeRepository)
		{
				_assetTypeRepository = assetTypeRepository;

				RuleFor(r => r.AssetType).Must(IsAssetTypeAllowed)
						.WithMessage(x => ValidationMessages.InvalidAssetType.FormatWith(_assetTypeDefinition.Name));

				RuleFor(x => x.File)
						.NotNullWithMessage();
				
				When(x => x.File != null, () =>
				{
						RuleFor(x => x.File)
								.Must(IsFileSizeValid).WithMessage(x => ValidationMessages.InvalidFileSize.FormatWith(_assetTypeDefinition.MaxFileSizeInMB))
								.Must(IsFileExtensionValid).WithMessage(x => ValidationMessages.InvalidFileExtension.FormatWith(_assetTypeDefinition.Name, x.File.GetExtension()));
				});
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
