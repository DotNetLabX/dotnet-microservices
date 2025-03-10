using Review.Application.Features.Articles.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Articles.AssignEditor;

public record AssignEditorCommand(int EditorId)
        : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.AssignEditor;
}

public class AssignEditorCommandValidator : ArticleCommandValidator<AssignEditorCommand>
{
    public AssignEditorCommandValidator()
    {
        RuleFor(c => c.EditorId).GreaterThan(0).WithMessageForInvalidId(nameof(AssignEditorCommand.EditorId));
    }
}