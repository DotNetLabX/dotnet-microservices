namespace Auth.API.Features.Users.SendChangePasswordLink
{
		public record SendChangePasswordLinkCommand(string Email);
		public record SendChangePasswordLinkResponse(string Email, string Code);
}
