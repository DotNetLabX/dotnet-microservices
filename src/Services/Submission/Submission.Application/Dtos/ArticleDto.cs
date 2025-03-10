namespace Submission.Application.Dtos;

public record ArticleDto(
		int id,
		string Title,
		string Scope,
		ArticleType Type,
		ArticleStage Stage,
		JournalDto Journal,
		DateTime? SubmittedOn,
		PersonDto? SubmittedBy,
		List<ActorDto> Actors,
		List<AssetDto> Assets
);