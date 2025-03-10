namespace EmailService.Contracts;

//insight
// choosing between the modular monolith and microservices architecture
// in this case it could have been both
// there are a few things we need to consider when taking this decision:
//- if we need to keep track of the email we are sending
//      - then we need to store the email information we are sending into a database which will not be independent of any other service
//      - create a GraphQL enpoint which will be consumed by any service who wants to know what are the email the service sent
// The reason for keeping the EmailService as a modular monolith are:
// - each service needs to use a different smpt, from address or even a differnet service(normal smtp vs sendgrid)
// - we don't need to share anything with any other service
public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailMessage emailMessage, CancellationToken ct = default);
}
