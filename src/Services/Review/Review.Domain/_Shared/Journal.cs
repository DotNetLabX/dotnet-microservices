using Review.Domain.Articles;
using Review.Domain.Reviewers;

namespace Review.Domain.Shared;

public partial class Journal : Entity
{
    //todo - add ChiefEditor and sync it togheter with the Name and Abbreviation
    public required string Name { get; set; }
    public required string Abbreviation { get; set; }

    
    private readonly List<Article> _articles = new();
    public IReadOnlyList<Article> Articles => _articles.AsReadOnly();
    public void AddArticle(Article article) => _articles.Add(article);

    public IReadOnlyCollection<ReviewerSpecialization> Reviewers { get; set; } = new HashSet<ReviewerSpecialization>();
}
