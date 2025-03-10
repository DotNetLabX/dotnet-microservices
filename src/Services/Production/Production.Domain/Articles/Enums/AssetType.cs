namespace Production.Domain.Assets.Enums;

public static class AssetTypeCategories
{
		public static HashSet<AssetType> DraftAssets = new()
		{
				AssetType.TypesetDraft
		};
		public static HashSet<AssetType> FinalAssets = new()
		{
				AssetType.FinalPdf,
				AssetType.FinalHtml,
				AssetType.FinalEpub
		};
		public static HashSet<AssetType> SupplementaryAssets = new()
		{
				AssetType.Manuscript,
				AssetType.Figure,
				AssetType.SupplementaryFile,
				AssetType.DataSheet
		};
}