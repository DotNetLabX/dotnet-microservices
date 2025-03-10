namespace Journals.API.Features.Shared;

public record JournalDto(
		int Id, 
		string Abbreviation, 
		string Name,
		string NormalizedName,
		string Description, 
		string ISSN,
		int ArticlesCount,
		IEnumerable<SectionDto> Sections)
{
		//insight about the difference between the two approaches AdaptIgnore vs MapToConstructor
		//[AdaptIgnore] 
		public EditorDto ChiefEditor { get; set; } = null!;

		[AdaptIgnore]
		public List<EditorDto> Editors { get; set; } = new();
}
