namespace Articles.Abstractions.Enums;

public enum AssetType
{
		Manuscript = 1,        // Author’s original submission file

		ReviewReport = 2,      // Reviewer’s feedback on the manuscript

		TypesetDraft = 3,      // Draft PDF produced by the typesetter, sent to the author for approval

		FinalPdf = 4,          // Final published version in PDF format
		FinalHtml = 5,         // Final published version in HTML format
		FinalEpub = 6,         // Final published version in ePub format

		SupplementaryFile = 10,// Additional supporting files (appendices, extra material)
		Figure = 11,           // Images, charts, diagrams linked to the article
		DataSheet = 12         // Raw data tables or datasets provided by the author
}

public static class AssetTypes
{
		public static HashSet<AssetType> SupplementaryFiles =
		[
				AssetType.Figure,
				AssetType.DataSheet,
				AssetType.SupplementaryFile
		];

		public static HashSet<AssetType> FinalFiles =
		[
				AssetType.FinalPdf,
				AssetType.FinalHtml,
				AssetType.FinalEpub
		];
}
