using Newtonsoft.Json;

namespace Review.Domain.Assets.ValueObjects;

public class AssetName: StringValueObject
{
		[JsonConstructor]
		private AssetName(string value) => Value = value;

		internal static AssetName FromSubmission(string name)
		{
				if (string.IsNullOrWhiteSpace(name))
						throw new ArgumentException("Asset name is required.", nameof(name));

				return new AssetName(name.Trim());
		}

		public static AssetName Create(AssetTypeDefinition assetType, int assetCount)
		{
				if (assetType.AllowsMultipleAssets)
						return new AssetName($"{assetType.Name.ToString()}_{assetCount + 1}");
				else
						return new AssetName(assetType.Name.ToString());
		}
}
