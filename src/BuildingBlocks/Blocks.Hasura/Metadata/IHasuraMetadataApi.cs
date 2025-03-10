using Refit;

namespace Blocks.Hasura.Metadata;

public interface IHasuraMetadataApi
{
		[Post("/v1/metadata")]
		Task<PgSuggestRelationshipsResponse> SuggestAsync([Body] SingleHasuraRequest request, CancellationToken ct = default);

		[Post("/v1/metadata")]
		Task<object> BulkAsync([Body] BulkHasuraRequest request, CancellationToken ct = default);

		[Post("/v1/metadata")]
		Task<object> SingleAsync([Body] SingleHasuraRequest request, CancellationToken ct = default);

		[Post("/v1/metadata")]
		Task<object> TrackTableAsync([Body] SingleHasuraRequest request, CancellationToken ct = default);
}