namespace ArticleHub.Domain.Dtos;

public record ArticleMinimalDto(
		int Id,
		string Title,
		string Doi,
		string Stage,
		DateTime SubmittedOn,
		DateTime? PublishedOn,
		DateTime? AcceptedOn,
		JournalDto Journal,
		PersonDto SubmittedBy
);