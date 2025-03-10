using Blocks.Hasura.Metadata;
using Blocks.Hasura.Query;
using Microsoft.Extensions.Logging;
using Humanizer;
using Refit;


namespace Blocks.Hasura;
public sealed class HasuraMetadataService(IHasuraMetadataApi _metadataApi, IHasuraSqlApi _queryApi, ILogger<HasuraMetadataService> _logger)
{
		const string source= "default";
		const string schema = "public";

		public async Task<List<TableRef>> DiscoverTablesAsync(CancellationToken ct = default)
		{
				var sql = """
                  select table_schema, table_name
                  from information_schema.tables
                  where table_type = 'BASE TABLE'
                    and table_schema = '$schema$'
                  and table_name not like 'hdb_%'
                  and table_name not like 'pg_%'
                  and table_name <> '__EFMigrationsHistory'
                  and table_name <> 'spatial_ref_sys'
                  order by 1,2;
                  """.Replace("$schema$", schema);

				var request = new SingleHasuraRequest(new RunSqlArgs
				{
						Source = source,
						Sql = sql,
						Cascade = false,
						ReadOnly = true
				});

				var response = await _queryApi.RunSqlAsync(request, ct);

				return response.Values
									 .Select(r => new TableRef { Schema = r[0], Name = r[1] })
									 .ToList();
		}

		public async Task TrackAllTablesBulkAsync(IEnumerable<TableRef> tables, CancellationToken ct = default)
		{
				var operations = tables
						.Select(t => new SingleHasuraRequest(
								new PgTrackTableRequest
								{
										Source = source,
										Table = t
								}))
						.Cast<IRequest>()
						.ToList();

				if (operations.Count == 0) return;

				var bulk = new BulkHasuraRequest(operations);
				await _metadataApi.BulkAsync(bulk, ct);
		}

		public async Task TrackAllTablesAsync(IEnumerable<TableRef> tables, CancellationToken ct = default)
		{
				foreach (var table in tables)
				{
						var request = new SingleHasuraRequest(
								new PgTrackTableRequest { Source = source, Table = table }
								);

						try
						{
								// in case the table is already tracked, Hasura returns Bad Request(400)
								await _metadataApi.SingleAsync(request, ct);
						}
						catch (ApiException ex)
						{
								 _logger.LogWarning(ex, "Failed to track {Schema}.{Name}. Body: {Body}", table.Schema, table.Name, ex.Content ?? string.Empty);
						}
				}
		}

		public async Task<IEnumerable<Relationship>> GetAllRelationshipsAsync(CancellationToken ct = default)
		{
				var request = new SingleHasuraRequest(
						new PgSuggestRelationshipsRequest { OmitTracked = true, Source = source}
						);

				var response = await _metadataApi.SuggestAsync(request, ct);
				return response.Relationships;
		}

		public async Task TrackAllRelationshipsAsync(IEnumerable<Relationship> relationships, CancellationToken ct = default)
		{
				foreach (var relation in relationships)
				{
						try
						{
								if(relation.Type == "object")
								{
									await TrackObjectRelationship(relation, ct);
								}
								else if(relation.Type == "array")
								{
									await TrackArrayRelationship(relation, ct);
								}
						}
						catch (ApiException ex)
						{
								_logger.LogWarning(ex, "Failed to track {Schema}.{Name}. Body: {Body}", relation.From.Table.Schema, relation.From.Table.Name, ex.Content ?? string.Empty);
						}
				}
		}


		public async Task TrackObjectRelationship(Relationship relationship, CancellationToken ct = default)
		{
				var relationName = ToRelationName(relationship.To.Table.Name);

				var args = new PgCreateObjectRelationshipRequest
				{
						Name = relationName,
						Table = relationship.From.Table,
						Using = new ObjectRelationshipUsing
						{
								ForeignKeyConstraintOn = relationship.From.Columns.ToArray()
						},
						Source = source
				};

				var request = new SingleHasuraRequest(args);
				await _metadataApi.SingleAsync(request, ct);
		}

		public async Task TrackArrayRelationship(Relationship relationship, CancellationToken ct = default)
		{
				var relationName = ToRelationName(relationship.To.Table.Name).Pluralize(inputIsKnownToBeSingular: false);

				var args = new PgCreateArrayRelationshipRequest
				{
						Name = relationName,
						Table = relationship.From.Table,
						Using = new ArrayRelationshipUsing
						{
								ForeignKeyConstraintOn = new ForeignKeyRef
								{
										Table = relationship.To.Table,
										Columns = relationship.To.Columns.ToArray()
								}
						},
						Source = source
				};

				var request = new SingleHasuraRequest(args);
				await _metadataApi.SingleAsync(request, ct);
		}


		private static string ToRelationName(string tableName)
		{
				// snake_case -> PascalCase -> camelCase
				return tableName.Underscore().Pascalize().Camelize();
		}
}