using Review.Application.Features.Articles.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Invitations.AcceptInvitation;

//todo - decline and accept invitation are not Audited Commands, because the endpoints are anonymous
public record DeclineInvitationCommand(string Token)
    : ArticleCommand<DeclineInvitationResponse>
{
    public override ArticleActionType ActionType => ArticleActionType.DeclineInvitation;
}

public record DeclineInvitationResponse(int ArticleId, int InvitationId);

public class DeclineInvitationCommandValidator : ArticleCommandValidator<DeclineInvitationCommand>
{
    public DeclineInvitationCommandValidator()
    {
				RuleFor(x => x.Token).NotEmpty();
		}
}
