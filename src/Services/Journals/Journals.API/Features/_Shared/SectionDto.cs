namespace Journals.API.Features.Shared;

public record SectionDto(
		int Id, 
		string Name, 
		string Description)
{
		[AdaptIgnore] 
		public List<EditorDto> Editors { get; set; } = default!;
}