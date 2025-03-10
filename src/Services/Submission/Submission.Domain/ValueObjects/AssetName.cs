using Newtonsoft.Json;

namespace Submission.Domain.ValueObjects;

public class AssetName: StringValueObject
{
		[JsonConstructor]
		private AssetName(string value) => Value = value;

		public static AssetName Create(AssetTypeDefinition assetType, int assetCount)
		{
				if (assetType.AllowsMultipleAssets)
						return new AssetName($"{assetType.Name.ToString()}_{assetCount + 1}");
				else
						return new AssetName(assetType.Name.ToString());
		}
}
