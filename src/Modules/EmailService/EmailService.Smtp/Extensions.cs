using EmailService.Contracts;
using MimeKit;
using MimeKit.Text;

namespace EmailService.Smtp;

internal static class Extensions
{
		public static  MailboxAddress ToMailboxAddress(this EmailAddress emailAddress)
				=> new MailboxAddress(emailAddress.Name, emailAddress.Address);

		public static MimeMessage ToMailKitMessage(this EmailMessage emailMessage)
		{
				var message = new MimeMessage();
				message.Subject = emailMessage.Subject;
				message.From.Add(emailMessage.From.ToMailboxAddress());
				message.To.AddRange(emailMessage.To.Select(t => t.ToMailboxAddress()).ToList());

				var bodyBuilder = new BodyBuilder
				{
						HtmlBody = emailMessage.Content.Value
				};
				message.Body = bodyBuilder.ToMessageBody();

				//message.Body = new TextPart(TextFormat.Html)
				//{
				//		Text = emailMessage.Content.Value
				//};

				return message;
		}
}
