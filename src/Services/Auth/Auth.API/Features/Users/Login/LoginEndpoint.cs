using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Blocks.Exceptions;
using Auth.Application;
using Auth.Persistence.Repositories;

namespace Auth.API.Features.Users.Login;

[AllowAnonymous]
[HttpPost("/tokens")]
[Tags("Auth")]
public class LoginEndpoint(UserManager<User> _userManager, SignInManager<User> _signInManager, PersonRepository _personRepository, TokenFactory _tokenFactory) 
		: Endpoint<LoginCommand, LoginResponse>
{
		public override async Task HandleAsync(LoginCommand command, CancellationToken ct)
		{
				var person = Guard.NotFound(
						await _personRepository.GetByEmailAsync(command.Email, ct)
				);

				var user = Guard.NotFound(person.User);

				var signInResult = await _signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: false); // turn lockoutOnFailure to true if you want to block the account after a few tries
				if (!signInResult.Succeeded)
						throw new BadRequestException($"Invalid credentials for {command.Email}");

				var userRoles = await _userManager.GetRolesAsync(user);

				var jwtToken = _tokenFactory.GenerateJWTToken(user.Id.ToString(), user.Person.FullName, command.Email, userRoles, Array.Empty<Claim>());
				var refreshToken = _tokenFactory.GenerateRefreshToken();

				user.AssignRefreshToken(refreshToken);
				await _userManager.UpdateAsync(user);
				var userResult = await _userManager.UpdateAsync(user);
				if (!userResult.Succeeded)
						throw new BadRequestException("Could not persist refresh token.");

				await Send.OkAsync(new LoginResponse(command.Email, jwtToken, refreshToken.Token));
		}
}
