using Journals.Domain.Journals.Enums;

namespace Journals.API.Features.Shared;

public class SectionEditorDto
{
		public int EditorId { get; set; }

		public EditorRole EditorRole { get; set; }
}
