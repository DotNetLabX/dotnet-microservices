using Newtonsoft.Json;
using Review.Domain.Articles;

namespace Review.Domain.Assets.ValueObjects;

public class FileName : StringValueObject
{
		[JsonConstructor]
		internal FileName(string value) => Value = value;

		public static FileName From(Asset asset, FileExtension extension)
		{
				var assetName = asset.Name.Value.Replace("'", "").Replace(" ", "-");
				return new FileName($"{assetName}.{extension}");
		}
}
