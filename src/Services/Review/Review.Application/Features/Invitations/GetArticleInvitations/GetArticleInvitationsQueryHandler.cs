using Review.Application.Dtos;

namespace Review.Application.Features.Invitations.GetArticleInvitations;

public class GetArticleInvitationsQueryHandler(ArticleRepository _articleRepository, ReviewInvitationRepository _reviewInvitationRepository)
    : IRequestHandler<GetArticleInvitationsQuery, GetArticleInvitationsResponse>
{
    public async Task<GetArticleInvitationsResponse> Handle(GetArticleInvitationsQuery command, CancellationToken ct)
    {
        await _articleRepository.ExistsOrThrowAsync(command.ArticleId, ct);

        var invitations = await _reviewInvitationRepository.GetByArticleIdAsync(command.ArticleId, ct);

        return new GetArticleInvitationsResponse(
            invitations.Select(i => i.Adapt<ReviewInvitationDto>())
        );
    }
}