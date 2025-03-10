namespace Blocks.Core.Security;

public interface IClaimsProvider
{
		public string GetClaimValue(string claimName);
		public string? TryGetClaimValue(string claimName);
		IEnumerable<string> GetClaimValues(string claimName);

		public int GetUserId();
		public int? TryGetUserId();
		public string GetUserEmail();
		public string GetUserName();

		IReadOnlySet<TEnum> GetUserRoles<TEnum>()
				where TEnum : struct, Enum;
		
		IReadOnlySet<string> GetUserRoles();
}
