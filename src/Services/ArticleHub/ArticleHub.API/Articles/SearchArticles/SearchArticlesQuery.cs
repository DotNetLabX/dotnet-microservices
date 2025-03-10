using System.Text.Json.Serialization;

namespace ArticleHub.API.Articles.SearchArticles;

public class SearchArticlesQuery
{
		public required object Filter { get; init; }
		public Pagination Pagination { get; init; } = new();
}

public sealed class Pagination
{
		private const int MaxLimit = 100;

		[JsonConstructor]
		public Pagination(int limit = 20, int offset = 0)
				=> (Limit, Offset) = (limit > MaxLimit ? MaxLimit : limit, offset < 0 ? 0 : offset);

		public int Limit { get; } 
		public int Offset { get; } 
}