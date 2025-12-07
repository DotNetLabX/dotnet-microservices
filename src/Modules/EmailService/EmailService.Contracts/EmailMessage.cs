namespace EmailService.Contracts;

public enum ContentType
{
    Text,
    Html
};

public record EmailAddress(string Name, string Address);

public record Content(ContentType Type, string Value);


public record EmailMessage(string Subject, Content Content, EmailAddress From, List<EmailAddress> To);
