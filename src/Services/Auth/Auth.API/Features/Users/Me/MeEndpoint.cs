using Auth.Persistence.Repositories;
using Blocks.Core.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Features.Users.Me;

[Authorize]
[HttpGet("/users/me")]
[Tags("Users")]
public class MeEndpoint(UserRepository _userRepository, UserManager<User> _userManager, IClaimsProvider _claims)
		: EndpointWithoutRequest<MeResponse>
{
		public override async Task HandleAsync(CancellationToken ct)
		{
				var user = await _userRepository.GetByIdOrThrowAsync(_claims.GetUserId());

				var roles = await _userManager.GetRolesAsync(user!);

				await Send.OkAsync(new MeResponse(
						user.Id,
						user.Email!,
						user.Person.FullName,
						roles.ToList()));
		}
}