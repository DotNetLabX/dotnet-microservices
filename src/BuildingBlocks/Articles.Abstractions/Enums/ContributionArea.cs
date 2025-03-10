namespace Articles.Abstractions.Enums;

public enum ContributionArea
{
		//mandatory
		OriginalDraft = 1,  // writing the first version
		Revision,						// improving the draft after feedback

		//optional
		Analysis,						// methods, formal analysis
		Investigation,			// experiments, data collection
		Visualization				// figures, charts
}

public static class ContributionAreaCategories
{
		public static HashSet<ContributionArea> MandatoryAreas =
		[
				ContributionArea.OriginalDraft,
				ContributionArea.Revision
		];

		public static HashSet<ContributionArea> OptionalAreas =
		[
				ContributionArea.Analysis,
				ContributionArea.Investigation,
				ContributionArea.Visualization
		];
}
