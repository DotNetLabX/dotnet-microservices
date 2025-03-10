using Auth.Grpc;
using Blocks.Mapster;
using Review.Domain.Reviewers;

namespace Review.Application.Features.Invitations.AcceptInvitation;

public class AcceptInvitationCommandHandler(ArticleRepository _articleRepository, ReviewerRepository _reviewerRepository, ReviewInvitationRepository _reviewInivtiationRepository, IPersonService _personClient)
    : IRequestHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    public async Task<AcceptInvitationResponse> Handle(AcceptInvitationCommand command, CancellationToken ct)
    {
        var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);

				var invitation = await _reviewInivtiationRepository.GetByTokenOrThrowAsync(command.Token);

				Reviewer? reviewer = default!;
				if (invitation.UserId != null)
						reviewer = await GetOrCreateReviewerByUserId(command, article, invitation.UserId.Value, ct);
				else
						reviewer = await GetOrCreateReviewerByEmail(command, article, invitation, ct);

				// keeping those 2 domain methods separately, allows us to assign reviewers directly without an invitation
				invitation.Accept();

				article.AssignReviewer(reviewer, command);

        await _articleRepository.SaveChangesAsync();

        return new AcceptInvitationResponse(article.Id, invitation.Id, reviewer.Id);
    }

		private async Task<Reviewer> GetOrCreateReviewerByUserId(AcceptInvitationCommand command, Article article, int userId, CancellationToken ct)
		{
				var reviewer = await _reviewerRepository.GetByUserIdAsync(userId);
				if (reviewer is null)
				{
						var response = await _personClient.GetPersonByUserIdAsync(new GetPersonByUserIdRequest { UserId = userId});
						
						reviewer = await CreateReviewerFromPerson(response.PersonInfo, article, command, ct);
				}
				command.CreatedById = userId;
				return reviewer;
		}

		private async Task<Reviewer> GetOrCreateReviewerByEmail(AcceptInvitationCommand command, Article article, ReviewInvitation invitation, CancellationToken ct)
		{
				var reviewer = await _reviewerRepository.GetByEmailAsync(invitation.Email, ct);
				if (reviewer is null)
				{
						//todo - here we need an user not a person. Implement a new User gRPC service. 
						var response = await _personClient.GetOrCreatePersonAsync(
								invitation.AdaptWith<CreatePersonRequest>(
										request => request.Affiliation = command.Affiliation)
								);

						reviewer = await CreateReviewerFromPerson(response.PersonInfo, article, command, ct);
				}
				command.CreatedById = reviewer.Id; //replace it with userId after implementing grpc user creation
				return reviewer;
		}

		private async Task<Reviewer> CreateReviewerFromPerson(PersonInfo personInfo, Article article, IArticleAction command, CancellationToken ct)
		{
				var reviewer = Reviewer.Create(personInfo, new HashSet<int> { article.JournalId }, command);
				await _reviewerRepository.AddAsync(reviewer, ct);
				return reviewer;
		}
}
