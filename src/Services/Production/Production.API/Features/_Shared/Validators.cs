using FastEndpoints;
using FluentValidation;
using FluentValidation.Results;

namespace Production.API.Features.Shared
{
		public abstract class BaseValidator<T> : Validator<T>
    {
        public override ValidationResult Validate(FluentValidation.ValidationContext<T> context)
        {
            var validationResult = base.Validate(context);

            if (!validationResult.IsValid)
            {
                //Log.Error(validationResult.Errors.ToString());
            }
            return validationResult;
        }

        public BaseValidator()
        {
            RuleFor(command => command).NotEmpty()
                .WithMessage(ValidatorsMessagesConstants.NotNull);
        }
    }

    public static class ValidatorsMessagesConstants
    {
        public const string NotNull = "Request cannot be null or empty.";
        public static readonly string NotNullOrEmpty = "{0} details cannot be null or empty.";
        public static readonly string InvalidId = "The {0} should be greater than zero.";
        public static readonly string InvalidActionMessage = "The {0} {1} action is invalid for the current stage.";
        public static readonly string InvalidContent = "The {0} cannot be null or empty.";
        public static readonly string InvalidIdForAction = "The {0} should be greater than or equal to zero.";
        public static readonly string InvalidAssetNumber = "The asset number should be greater than zero and less than or equal to {0}.";
        public static readonly string InvalidAssetType = "The {0} asset type is invalid for this endpoint.";
        public static readonly string InvalidFileSize = "The file size exceeds the maximum limit; the maximum file size is {0}MB.";
        public static readonly string InvalidExtension = "The {0} does not support the {1} extension.";
        public static readonly string AtLeastOneIdRequired = "At least one {0} is required for this action.";
        public const string InvalidJournalAbbreviation = "Invalid JournalAbbreviation.";
        public const string InvalidArticleTitle = "Invalid Article Title.";
        public const string InvalidArticleStage = "Generate an article in the intial assessment stage only.";
        public const string InvalidBatchId = "Invalid batchId.";
        public const string InvalidScheduleDate = "The scheduled date should be null or greater than today.";
        public const string InvalidAssetNumberForManuscript = "For the Manuscript, the asset number should be 0.";
        public const string InvalidAction = "The action is invalid for the current stage.";
        public static readonly string InvalidStatusActionMessage = "The {0} {1} action is invalid for the current file.";
        public const string InvaildAssetNumberForUpload = "The asset number should be 0";

    }

    public static class ValidatorsConstants
    {
        public const int Id = 0;
    }
}
