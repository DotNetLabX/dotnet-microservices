using Blocks.Entitities;
using Newtonsoft.Json;
using Production.Domain.Assets;

namespace Production.Domain.Assets.ValueObjects;

//insight - explain why class and not record. the record provides its own set of ToString, GethashCode implementations and it doesn't work with inheritence
public class AssetNumber : SingleValueObject<byte>
{
		[JsonConstructor]
		private AssetNumber(byte value) => Value = value;

		public static AssetNumber FromNumber(byte number,  AssetTypeDefinition assetType)
		{
				ArgumentOutOfRangeException.ThrowIfGreaterThan(number, assetType.MaxNumber, "Asset type max number already reached");
				return new AssetNumber(++number);
		}

		// added for Link queries
		public static implicit operator byte(AssetNumber assetNumber) => assetNumber.Value;
}
