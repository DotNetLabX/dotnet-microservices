using Blocks.Redis;
using Redis.OM.Modeling;

namespace Journals.Domain.Journals;

[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(Journal) })]
//[Document(StorageType = StorageType.Json)]
public partial class Journal : Entity
{
    [Indexed]
    public required string Abbreviation { get; set; }

    private string _name = null!;
    [Searchable]
    public required string Name
    {
        get => _name;
        set
        {
            _name = value;
            NormalizedName = _name.ToLowerInvariant(); // Normalize on set
        }
    }
    [Indexed(Sortable = true)]
    //insight about normalizing the Name so we can do case-insensitive searches
    public required string NormalizedName { get; set; }

    [Searchable]
    public required string Description { get; set; }
    public required string ISSN { get; set; } //unique ID in the publishing world
    public int ChiefEditorId { get; set; }

    [Indexed(JsonPath = "$.Name")]
    public List<Section> Sections { get; set; } = new();

    public int ArticlesCount { get; set; }
}

