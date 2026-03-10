using EmailService.Contracts;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

//insight about the namespaces: contracts, smtp, sendgrid and the name of the classes
namespace EmailService.Smtp;

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;

    public SmtpEmailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task<bool> SendEmailAsync(EmailMessage emailMessage, CancellationToken ct = default)
    {
        var message = emailMessage.ToMailKitMessage();

        using var smtpClient = new SmtpClient();
        try
        {
            //todo - remove this line
            //smtpClient.ServerCertificateValidationCallback =
            //(sender, certificate, certChainType, errors) => true;

            await smtpClient.ConnectAsync(_emailOptions.Smtp.Host, _emailOptions.Smtp.Port, _emailOptions.Smtp.UseSSL, ct);
            await smtpClient.AuthenticateAsync(_emailOptions.Smtp.Username, _emailOptions.Smtp.Password, ct);
            await smtpClient.SendAsync(message, ct);
        }
        catch (Exception ex)
        {
            //log the exception 
            return false;
        }

        return true;
    }
}
