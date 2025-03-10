namespace Submission.Domain.Enums;

public static class AssetTypeCategories
{
		public static HashSet<AssetType> ManuscriptAsset = new()
		{
				AssetType.Manuscript
		};

		public static HashSet<AssetType> SupplementaryAssets = new()
		{
				AssetType.Figure,
				AssetType.DataSheet,
				AssetType.SupplementaryFile,
		};
}