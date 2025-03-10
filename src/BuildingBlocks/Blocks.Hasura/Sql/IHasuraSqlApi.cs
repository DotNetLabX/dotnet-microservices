using Refit;

namespace Blocks.Hasura.Query;

public interface IHasuraSqlApi
{
		[Post("/v2/query")]
		Task<RunSqlResponse> RunSqlAsync([Body] SingleHasuraRequest request, CancellationToken ct = default);

		// Fallback for older versions of Hasura
		[Post("/v1/query")]
		Task<RunSqlResponse> RunSqlV1Async([Body] SingleHasuraRequest request, CancellationToken ct = default);
}