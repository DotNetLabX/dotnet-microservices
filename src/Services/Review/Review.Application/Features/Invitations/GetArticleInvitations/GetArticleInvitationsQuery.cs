using Review.Application.Dtos;

namespace Review.Application.Features.Invitations.GetArticleInvitations;

public record GetArticleInvitationsQuery(int ArticleId) : IQuery<GetArticleInvitationsResonse>;
public record GetArticleInvitationsResonse(IEnumerable<ReviewInvitationDto> Invitations);

public class GetArticleInvitationsQueryValidator : AbstractValidator<GetArticleInvitationsQuery>
{
    public GetArticleInvitationsQueryValidator()
    {
        RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(GetArticleInvitationsQuery.ArticleId));
    }
}