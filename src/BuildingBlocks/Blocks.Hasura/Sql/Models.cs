namespace Blocks.Hasura.Query;

public class RunSqlArgs : IRequest
{
		public string Source { get; set; } = "default";
		public string Sql { get; set; } = default!;
		public bool Cascade { get; set; } = false;
		public bool ReadOnly { get; set; } = true;

		public string Type => "run_sql";
}

public class RunSqlResponse
{
		// Result is a 2D array of strings where row 0 is the header
		public List<List<string>> Result { get; set; } = new();
		public IReadOnlyList<string> Header => Result.Count > 0 ? Result[0] : Array.Empty<string>();

		public IEnumerable<IReadOnlyList<string>> Values => Result.Skip(1); //skip header
}