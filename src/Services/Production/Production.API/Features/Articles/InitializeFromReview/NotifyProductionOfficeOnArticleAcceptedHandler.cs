using EmailService.Contracts;
using MediatR;
using Microsoft.Extensions.Options;
using Production.Domain.Articles;
using Production.Domain.Articles.Events;
using Production.Persistence;

namespace Production.API.Features.Articles.InitializeFromReview;

public class NotifyProductionOfficeOnArticleAcceptedHandler(
    ProductionDbContext dbContext,
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions
) : INotificationHandler<ArticleAcceptedForProduction>
{
    public async Task Handle(ArticleAcceptedForProduction notification, CancellationToken ct)
    {
        var productionOfficers = dbContext.ProductionOfficers.ToList();

        foreach (var officer in productionOfficers)
        {
            var emailMessage = BuildNotificationEmail(notification.Article.Title, officer, emailOptions.Value.EmailFromAddress);
            await emailService.SendEmailAsync(emailMessage);
        }
    }

    private static EmailMessage BuildNotificationEmail(string articleTitle, ProductionOfficer officer, string fromEmailAddress)
    {
        const string body = "Dear {0},<br/>A new article has been accepted for production: <strong>{1}</strong>.<br/>Please assign a typesetter.";

        return new EmailMessage(
            "New Article Accepted for Production – Assign Typesetter",
            new Content(ContentType.Html, string.Format(body, officer.FullName, articleTitle)),
            new EmailAddress("articles", fromEmailAddress),
            new List<EmailAddress> { new EmailAddress(officer.FullName, officer.Email) }
        );
    }
}
