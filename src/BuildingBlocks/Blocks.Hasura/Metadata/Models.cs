namespace Blocks.Hasura.Metadata;

public class TableRef
{
		public string Schema { get; set; } = "public";
		public string Name { get; set; } = default!;
}

public class PgTrackTableRequest : IRequest
{
		public string Source { get; set; } = "default";
		public TableRef Table { get; set; } = default!;

		public string Type => "pg_track_table";
}

public class Relationship
{
		public string Type { get; set; } = default!;
		public RelEndpoint From { get; set; } = default!;
		public RelEndpoint To { get; set; } = default!;
}

public class RelEndpoint
{
		public TableRef Table { get; set; } = default!;
		public List<string> Columns { get; set; } = new();
		public string ConstraintName { get; set; } = default!;
}

public class PgSuggestRelationshipsRequest : IRequest
{
		public string Type => "pg_suggest_relationships";
		public bool OmitTracked { get; set; } = true;
		public string Source { get; set; } = "default";
}

public class PgSuggestRelationshipsResponse
{
		public List<Relationship> Relationships { get; set; } = new();
}

public class PgCreateObjectRelationshipRequest : PgCreateRelationshipRequest
{
		public override string Type => "pg_create_object_relationship";
		public ObjectRelationshipUsing Using { get; set; } = default!;
}

public class PgCreateArrayRelationshipRequest : PgCreateRelationshipRequest
{
		public override string Type => "pg_create_array_relationship";
		public ArrayRelationshipUsing Using { get; set; } = default!;
}

public abstract class PgCreateRelationshipRequest : IRequest
{
		public string Source { get; set; } = "default";
		public TableRef Table { get; set; } = default!;
		public string Name { get; set; } = default!;

		public abstract string Type { get; }
}

public class ObjectRelationshipUsing
{
		public string[] ForeignKeyConstraintOn { get; set; } = default!;
}

public class ArrayRelationshipUsing
{
		public ForeignKeyRef ForeignKeyConstraintOn { get; set; } = default!;
}

public class ForeignKeyRef
{
		public TableRef Table { get; set; } = default!;
		public string[] Columns { get; set; } = Array.Empty<string>();
}

public class PgBulkArgs
{
		public List<object> Operations { get; set; } = new();
}