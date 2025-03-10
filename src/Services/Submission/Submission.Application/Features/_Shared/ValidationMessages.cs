namespace Submission.Application.Features.Shared;

public static class ValidationMessages
{
    public static readonly string InvalidAssetType = "The {0} asset type is invalid for this endpoint.";
    public static readonly string InvalidFileSize = "The file size exceeds the maximum limit; The maximum allowed file size for this asset is {0}MB.";
    public static readonly string InvalidFileExtension = "The {0} does not support the {1} extension.";
}        
