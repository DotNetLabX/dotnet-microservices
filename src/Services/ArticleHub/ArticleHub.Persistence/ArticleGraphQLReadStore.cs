using GraphQL;
using GraphQL.Client.Http;
using Blocks.Core.GraphQL;
using ArticleHub.Domain.Dtos;
using FluentValidation;
using FluentValidation.Results;

namespace ArticleHub.Persistence;

public class ArticleGraphQLReadStore(GraphQLHttpClient client)
{
		private readonly GraphQLHttpClient _client = client;

		// Shared fragment (reuse in all Gets))
		private const string ArticleFragment = @"
				fragment ArticleDto on Article {
					id
					title
					doi
					stage
					submittedOn
					acceptedOn
					publishedOn
					journal { id abbreviation name }
					submittedBy: person { id email firstName lastName userId }
					actors:articleActors {
						role
						person {id userId email firstName lastName }
					}
				}";

		public async Task<QueryResult<ArticleDto>> GetArticlesAsync(object filter, int limit = 20, int offset = 0, CancellationToken ct = default)
		{
				var req = new GraphQLRequest
				{
						OperationName = "GetArticles",
						Query = ArticleFragment + @"
						query GetArticles($filter: ArticleBoolExp, $limit: Int = 20, $offset: Int = 0) {
							items: article(where: $filter, limit: $limit, offset: $offset) {
								...ArticleDto
							}
						}",
						Variables = new { filter, limit, offset }
				};

				var res = await _client.SendQueryAsync<QueryResult<ArticleDto>>(req, ct);
				if (res.Errors?.Length > 0) //todo create a custom exception for GraphQL errors
						throw new ValidationException("GraphQL error", res.Errors.Select(e => new ValidationFailure("GraphQL", e.Message)));

				return res.Data ?? new QueryResult<ArticleDto>(new());
		}

		public async Task<ArticleDto?> GetArticleById(int id, CancellationToken ct = default)
		{
				var req = new GraphQLRequest
				{
						OperationName = "GetArticleById",
						Query = ArticleFragment + @"
query GetArticleById($id: Int!) {
  item: articleByPk(id: $id) {
    ...ArticleDto
  }
}",
						Variables = new { id }
				};

				var res = await _client.SendQueryAsync<SingleResult<ArticleDto>>(req, ct);
				if (res.Errors?.Length > 0)
						throw new ValidationException("GraphQL error", res.Errors.Select(e => new ValidationFailure("GraphQL", e.Message)));

				return res.Data?.Item;
		}
}
