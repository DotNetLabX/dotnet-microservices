using Auth.Grpc;

namespace Review.Application.Features.Invitations.AcceptInvitation;

public class DeclineInvitationCommandHandler(ArticleRepository _articleRepository, ReviewInvitationRepository _reviewInivtiationRepository)
    : IRequestHandler<DeclineInvitationCommand, DeclineInvitationResponse>
{
    public async Task<DeclineInvitationResponse> Handle(DeclineInvitationCommand command, CancellationToken ct)
    {
        var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);

				var invitation = await _reviewInivtiationRepository.GetByTokenOrThrowAsync(command.Token);

        invitation.Decline();

        await _articleRepository.SaveChangesAsync();

        return new DeclineInvitationResponse(article.Id, invitation.Id);
    }
}
