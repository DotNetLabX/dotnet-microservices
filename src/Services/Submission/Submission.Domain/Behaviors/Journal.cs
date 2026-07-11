using Journals.Grpc;

namespace Submission.Domain.Entities;

public partial class Journal
{
    public Article CreateArticle(string title, ArticleType Type, string scope, IArticleAction action)
    {
        var article = Article.Create(title, Type, scope, this, action);
        _articles.Add(article);

        return article;
    }

    public static Journal Create(JournalInfo journalInfo, IArticleAction action)
    {
        var journal = new Journal
        {
            Id = journalInfo.Id,
            Abbreviation = journalInfo.Abbreviation,
            Name = journalInfo.Name
        };

        return journal;
    }
}
