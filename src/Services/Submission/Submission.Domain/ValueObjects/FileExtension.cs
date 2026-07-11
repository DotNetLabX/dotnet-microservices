using Blocks.Core;
using Blocks.Domain;
using Newtonsoft.Json;

namespace Submission.Domain.ValueObjects;

public class FileExtension: StringValueObject
{
    [JsonConstructor]
    private FileExtension(string value) => Value = value;

    public static FileExtension FromAssetType(AssetTypeDefinition assetType)
    {
        return new FileExtension(assetType.DefaultFileExtension);
    }

    public static FileExtension FromFileName(string fileName, AssetTypeDefinition assetType)
    {
        var extension = Path.GetExtension(fileName)?.TrimStart('.');
        if (string.IsNullOrWhiteSpace(extension))
            throw new DomainException($"File name '{fileName}' has no extension");
        Guard.ThrowIfNotEqual(
            assetType.AllowedFileExtensions.IsValidExtension(extension), true);

        return new FileExtension(extension);
    }
}
