using Articles.Abstractions.Enums;

namespace Production.Application.Dtos;

public record ArticleSummaryDto(
		int id,
		string Title, 
		string Doi, 
		ArticleStage Stage, 
		JournalDto Journal,
		int VolumeId,
		DateTime SubmitedOn,
		PersonDto SubmitedBy,
		DateTime PublishedOn,
		PersonDto PublishedBy,
		List<ContributorDto> Contributors
);