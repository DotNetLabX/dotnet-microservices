using Auth.Grpc;
using Blocks.Domain;
using EmailService.Contracts;
using Flurl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Review.Application.Options;
using EmailAddress = EmailService.Contracts.EmailAddress;

namespace Review.Application.Features.Invitations.InviteReviewer;

public class InviteReviewerCommandHandler(
    ReviewDbContext _dbContext,
    ArticleRepository _articleRepository, 
    ReviewerRepository _reviewRepository,
    ReviewInvitationRepository _reviewInvitationRepository,
		IPersonService _personClient, 
    IEmailService _emailService,
    IOptions<EmailOptions> emailOptions, 
    IOptions<AppUrlsOptions> appUrlsOptions)
    : IRequestHandler<InviteReviewerCommand, InviteReviewerResponse>
{
    public async Task<InviteReviewerResponse> Handle(InviteReviewerCommand command, CancellationToken ct)
    {
        var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);
				var editor = await _dbContext.Editors.SingleAsync(r => r.UserId == command.CreatedById);

        if (await _reviewInvitationRepository.OpenInvitationExistsAsync(command.ArticleId, command.UserId, command.Email, ct))
						throw new DomainException("An open invitation already exists for this reviewer.");

				ReviewInvitation invitation = default!;
        if (command.UserId != null)
        {
            var reviewer = await _reviewRepository.GetByUserIdAsync(command.UserId.Value);
            if (reviewer is not null)
            {
                invitation = article.InviteReviewer(reviewer, command);
            }
            else
            {
                var personInfo = await GetPersonByUserId(command.UserId.Value, ct);
                invitation = article.InviteReviewer(personInfo.UserId, personInfo.Email, personInfo.FirstName, personInfo.LastName, command);
            }
        }
        else
        {
            invitation = article.InviteReviewer(command.UserId, command.Email, command.FirstName, command.LastName, command);
        }


				await _dbContext.SaveChangesAsync();

        await _emailService.SendEmailAsync(BuildEmailMessage(invitation, editor));

        return new InviteReviewerResponse(article.Id, invitation.Id, invitation.Token.Value);
    }

    private EmailMessage BuildEmailMessage(ReviewInvitation invitation, Editor editor)
    {
        const string InvitationEmail =
								@"Dear Contributor,<br/> 
						You've been invited by {0} to review the following article: {1}.<br/>
						Please accept or deny, the invitation will expire on {2}.<br/>
						If you don't have an account please create one using the following URL: {3}";

        var url =
								appUrlsOptions.Value.ReviewUIBaseUrl
								.AppendPathSegment($"articles/{invitation.ArticleId}/invitations/{invitation.Token}");

        return new EmailMessage(
                "Review Invitation",
                new Content(ContentType.Html, string.Format(InvitationEmail, editor.FirstName + " " + editor.LastName, url, invitation.ExpiresOn, url)),
                new EmailAddress("articles", emailOptions.Value.EmailFromAddress),
                new List<EmailAddress> { new EmailAddress(invitation.FullName, invitation.Email) }
                );
    }

		private async Task<PersonInfo> GetPersonByUserId(int userId, CancellationToken ct)
		{
				var response = await _personClient.GetPersonByUserIdAsync(new GetPersonByUserIdRequest { UserId = userId }, ct);
        return response.PersonInfo;
		}
}
