using Auth.Application;
using Auth.Persistence.Repositories;
using Blocks.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.API.Features.Users.RefreshToken;

[AllowAnonymous]
[HttpPost("/tokens/refresh")]
[Tags("Auth")]
public class RefreshTokenEndpoint(UserRepository _userRepository, UserManager<User> _userManager, TokenFactory _tokenFactory)
				: Endpoint<RefreshTokenCommand, RefreshTokenResponse>
{
		public override async Task HandleAsync(RefreshTokenCommand command, CancellationToken ct)
		{
				var user = await _userRepository.GetByRefreshTokenAsync(command.RefreshToken, ct);
				
				if (user is null)
						throw new UnauthorizedException("Invalid refresh token.");

				var roles = await _userManager.GetRolesAsync(user);

				var jwtToken = _tokenFactory.GenerateJWTToken(user.Id.ToString(), user.Person.FullName, user.Email!, roles, Array.Empty<Claim>());
				// you can also generate a new refresh token here and invalidate the old one when near expiry

				await Send.OkAsync(new RefreshTokenResponse(user.Email!, jwtToken, command.RefreshToken), ct);
		}
}
