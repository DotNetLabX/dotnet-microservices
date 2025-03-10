using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Blocks.Exceptions;
using EmailService.Contracts;
using Blocks.AspNetCore;
using Flurl;
using Auth.Persistence.Repositories;

namespace Auth.API.Features.Users.SendChangePasswordLink;

[AllowAnonymous]
[HttpPost("/password/forgot")]
[Tags("Password")]
public class SendChangePasswordLinkEndpoint(UserRepository _userRepository, UserManager<User> _userManager, IEmailService _emailService, IHttpContextAccessor _httpContextAccessor, IOptions<EmailOptions> _emailOptions, IWebHostEnvironment _env)
		: Endpoint<SendChangePasswordLinkCommand, SendChangePasswordLinkResponse>
{
		public override async Task HandleAsync(SendChangePasswordLinkCommand command, CancellationToken ct)
		{
				var user = await _userRepository.GetByEmailAsync(command.Email);
				if (user == null)
						throw new BadRequestException($"User doesn't exist");

				var ressetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);

				var emailMessage = BuildEmailMessage(user, ressetPasswordToken);
				await _emailService.SendEmailAsync(emailMessage, ct);

				if (_env.IsDevelopment())
						await Send.OkAsync(new SendChangePasswordLinkResponse(command.Email, ressetPasswordToken));
				else
						await Send.ResponseAsync(null, StatusCodes.Status202Accepted, ct);
		}

		private EmailMessage BuildEmailMessage(User user, string token)
		{
				const string SetPasswordEmail =
						@"Dear {0}, Please set your password using the following URL: {1}";

				//todo - add presentation application URl to appsettings
				var url =
						_httpContextAccessor.HttpContext?.Request.BaseUrl()
						.AppendPathSegment("set-first-password")
						.SetQueryParams(new { token });

				return new EmailMessage(
						"Confirmation",
						new Content(ContentType.Html, string.Format(SetPasswordEmail, user.FullName, url)),
						new EmailAddress("articles", _emailOptions.Value.EmailFromAddress),
						new List<EmailAddress> { new EmailAddress(user.FullName, user.Email!) }
						);
		}
}
