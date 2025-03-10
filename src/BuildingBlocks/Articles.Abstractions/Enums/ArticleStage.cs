using System.ComponentModel;

namespace Articles.Abstractions.Enums;

public enum ArticleStage : int
{
		None = 0,

		//Submission
		[Description("The Author created the Article")]
		Created = 101,
		[Description("Author uploaded the Manuscript file")]
		ManuscriptUploaded = 102,
		[Description("The Manuscript was submitted by the author for editorial checks")]
		Submitted = 103,
		[Description("The Manuscript didn't pass the initial checks")]
		InitialRejected = 104,
		[Description("The Manuscript passed the initial editorial checks")]
		InitialApproved = 105,

		//Review
		[Description("Article is under review")]
		UnderReview = 201,
		[Description("All Reviewer feedback received, waiting for editor's decision")]
		ReadyForDecision = 202,
		[Description("Editor requested a revised manuscript from the author")]
		AwaitingRevision = 203, //insight: not implemented, but real, requires review rounds
		[Description("Article rejected after review")]
		Rejected = 204,
		[Description("Article accepted after review")]
		Accepted = 205,


		// Production
		[Description("Typesetter assigned to the article")]
		InProduction = 300,
		[Description("Typesetter uploaded the draft PDF for author approval")]
		DraftProduction = 301,
		[Description("Author approved the draft PDF, finalization in progress")]
		FinalProduction = 302,
		[Description("Article scheduled for online publication")]
		PublicationScheduled = 304,
		[Description("Article published")]
		Published = 305
}


public static class ArticleStages
{
		public static HashSet<ArticleStage> Public =
		[
				ArticleStage.Submitted,
				ArticleStage.UnderReview,
				ArticleStage.InProduction,
				ArticleStage.Accepted,
				ArticleStage.Rejected,
				ArticleStage.Published
		];

		public static HashSet<ArticleStage> Submission =
		[
				ArticleStage.Created,
				ArticleStage.ManuscriptUploaded,
				ArticleStage.Submitted,
				ArticleStage.InitialRejected,
				ArticleStage.InitialApproved
		];

		public static HashSet<ArticleStage> Review =
		[
				ArticleStage.UnderReview,
				ArticleStage.ReadyForDecision,
				ArticleStage.AwaitingRevision,
				ArticleStage.Accepted,
				ArticleStage.Rejected
		];

		public static HashSet<ArticleStage> Production =
		[
				ArticleStage.InProduction,
				ArticleStage.DraftProduction,
				ArticleStage.FinalProduction,
				ArticleStage.PublicationScheduled,
				ArticleStage.Published
		];
}

