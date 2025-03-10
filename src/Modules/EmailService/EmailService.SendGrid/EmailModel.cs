namespace EmailService.SendGrid;

//test model. to be removed after SendGrid tested on production 
public class EmailModel
{

    public string Email { get; set; }
    public string Subject { get; set; }
    public string HTMLContent { get; set; }
}
