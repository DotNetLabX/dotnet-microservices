using Newtonsoft.Json;

namespace Submission.Domain.ValueObjects;

public class FileName: StringValueObject
{
		[JsonConstructor]
		private FileName(string value) => Value = value;

		public static FileName From(Asset asset, FileExtension extension)
		{
				var assetName = asset.Name.Value.Replace("'", "").Replace(" ", "-");
				return new FileName($"{assetName}.{extension}");
		}
}
