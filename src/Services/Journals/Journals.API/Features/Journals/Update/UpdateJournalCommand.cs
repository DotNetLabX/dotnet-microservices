namespace Journals.API.Features.Journals.Update;

public record UpdateJournalCommand(int JournalId, string Abbreviation, string ShortName, string Name, string Description, string ISSN, int ChiefEditorId);

public class UpdateJournalCommandValidator : Validator<UpdateJournalCommand>
{
    public UpdateJournalCommandValidator()
    {
				RuleFor(r => r.JournalId).GreaterThan(0);
				RuleFor(r => r.Abbreviation).NotEmpty();
				RuleFor(r => r.Name).NotEmpty();
				RuleFor(r => r.ISSN).NotEmpty();
				RuleFor(r => r.Description).NotEmpty();
				RuleFor(r => r.ChiefEditorId).GreaterThan(0);
		}
}