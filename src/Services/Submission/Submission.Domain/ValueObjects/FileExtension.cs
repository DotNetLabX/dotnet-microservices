using Blocks.Core;
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
        var extension = Path.GetExtension(fileName).Remove(0, 1); //removing the '.'
				Guard.ThrowIfNullOrWhiteSpace(extension);
				Guard.ThrowIfNotEqual(
            assetType.AllowedFileExtensions.IsValidExtension(extension), true); 

				return new FileExtension(extension);
    }
}
