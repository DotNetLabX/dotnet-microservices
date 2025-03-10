namespace Blocks.Domain;

public interface IAuditableAction
{
		int CreatedById { get; set; }
		public bool IsAuthenticated => CreatedById != default;
		DateTime CreatedOn { get; }
		public string Action { get; }
		string? Comment { get; }
}

public interface IAuditableAction<TActionType> : IAuditableAction
		where TActionType : Enum
{
		TActionType ActionType { get; }

		//insight - default implementation in interfaces
		string IAuditableAction.Action => ActionType.ToString();
}
