using EmailService.Contracts;
using SendGrid;
using SendGridEmailAddress = SendGrid.Helpers.Mail.EmailAddress;
using SendGridMessage = SendGrid.Helpers.Mail.SendGridMessage;

namespace EmailService.SendGrid
{
		internal static class Extensions
		{
				public static SendGridMessage ToSendGridMessage(this EmailMessage emailMessage)
				{
						var message = new SendGridMessage()
						{
								Subject = emailMessage.Subject,
								From = emailMessage.From.ToSendGridEmailAddress(),								
						};
						message.AddTos(emailMessage.To.Select(t => t.ToSendGridEmailAddress()).ToList());
						message.AddContent(emailMessage.Content.Type.ToMimeType() , emailMessage.Content.Value);
						return message;
				}

				public static SendGridEmailAddress ToSendGridEmailAddress(this EmailAddress emailMessage)
						=> new SendGridEmailAddress(emailMessage.Name, emailMessage.Address);

				public static string ToMimeType(this ContentType contentType)
				{
						switch(contentType)
						{ 
								case ContentType.Text : return MimeType.Text;
								case ContentType.Html : return MimeType.Html;
								default: return MimeType.Text;
						}								
				}
		}
}
