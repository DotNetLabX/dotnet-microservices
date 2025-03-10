using Blocks.Redis;
using Journals.Domain.Journals.ValueObjects;
using Redis.OM.Modeling;

namespace Journals.Domain.Journals;

[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(Section) })]
//[Document(StorageType = StorageType.Json)]
public class Section : Entity
{
		[Indexed]
		public required string Name { get; set; }
		public required string Description { get; set; }
    public List<SectionEditor> EditorRoles { get; set; } = new();
    public int ArticlesCount { get; set; }
}
