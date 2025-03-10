using Articles.Abstractions.Enums;

namespace Articles.IntegrationEvents.Contracts.Articles.Dtos;

public record ArticleDto(
		int Id, 
		string Title,
		string Scope,
		string? Doi,
		ArticleType Type,
		ArticleStage Stage, 
		JournalDto Journal,
		PersonDto SubmittedBy, 
		DateTime SubmittedOn, 
		DateTime? AcceptedOn, 
		DateTime? PublishedOn,
		List<ActorDto> Actors,
		List<AssetDto> Assets
		);