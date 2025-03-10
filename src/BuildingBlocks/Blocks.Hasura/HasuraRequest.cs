namespace Blocks.Hasura;

public interface IRequest 
{
		string Type { get; }
}

public class SingleHasuraRequest(IRequest args) : HasuraRequest(args)
{
		public override string Type { get; } = args.Type;
}

public class BulkHasuraRequest(List<IRequest> args) : HasuraRequest(args)
{
		public override string Type => "bulk";
}

public abstract class HasuraRequest : IRequest
{
		public HasuraRequest(IRequest args) => this.Args = args;
		public HasuraRequest(List<IRequest> args) => this.Args = args;

		public string Source { get; set; } = "default";
		public abstract string Type { get; }
		public object Args { get; }
}