using Journals.Grpc;

namespace Submission.Domain.Entities;

public partial class Journal
{
		public Article CreateArticle(string title, ArticleType Type, string scope, IArticleAction action)
		{
				var article = new Article
				{
						Title = title,
						Type = Type,
						Scope = scope,
						Journal = this,
						Stage = ArticleStage.Created,
						CreatedById = action.CreatedById,
						CreatedOn = action.CreatedOn
				};
				_articles.Add(article);

				var domainEvent = new ArticleCreated(article, action);
				article.AddDomainEvent(domainEvent);
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
