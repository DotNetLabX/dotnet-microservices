namespace Articles.IntegrationEvents.Contracts.Journals;
public record JournalDto(
		int Id, 
		string Name, 
		string Abbreviation, 
		int ChiefEditorUserId);
