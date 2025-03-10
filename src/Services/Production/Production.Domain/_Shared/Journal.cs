using Production.Domain.Articles;

namespace Production.Domain.Shared;

public partial class Journal : Entity
{
    //insight - difference between required, null!, default!
    public required string Name { get; set; }

    public string Abbreviation { get; set; } = null!;

    //public string ShortName { get; set; } = null!;

    public int DefaultTypesetterId { get; set; }
    public Typesetter DefaultTypesetter { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; } = new List<Article>();
}
