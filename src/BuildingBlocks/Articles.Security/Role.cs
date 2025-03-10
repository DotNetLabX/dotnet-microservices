using Articles.Abstractions.Enums;

namespace Articles.Security;


public static class Role
{
		public const string UserAdmin = nameof(UserRoleType.USERADMIN);

		public const string EditorAdmin = nameof(UserRoleType.EOF);
		public const string Author = nameof(UserRoleType.AUT);

		public const string Editor = nameof(UserRoleType.REVED);
		public const string Reviewer = nameof(UserRoleType.REV);

		public const string ProdAdmin = nameof(UserRoleType.POF);
		public const string Typesetter = nameof(UserRoleType.TSOF);

}