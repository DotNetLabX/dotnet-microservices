using Blocks.Redis;
using Redis.OM.Modeling;

namespace Journals.Domain.Journals;

[Document(StorageType = StorageType.Hash, Prefixes = new[] { nameof(Editor) })]
//[Document(StorageType = StorageType.Hash)]
public partial class Editor : Entity
{
		// Editor.Id = Editor.UserId. Editors are Users therefore they don't need their own IDs

		[Indexed]
    public required string FullName { get; init; }

		[Indexed]
		public required string Email{ get; init; }

		[Indexed]
		public required string Affiliation { get; init; }

		[Indexed]
		public required int PersonId{ get; init; }
}
