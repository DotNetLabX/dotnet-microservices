using Review.Domain.Invitations.Enums;

namespace Review.Persistence.Repositories;

public class ReviewInvitationRepository(ReviewDbContext dbContext)
		: Repository<ReviewInvitation>(dbContext)
{
		public async Task<ReviewInvitation> GetByTokenOrThrowAsync(string token, CancellationToken ct = default)
				=> await Query()
						.SingleOrThrowAsync(i => i.Token.Value == token && i.Status == InvitationStatus.Open, ct);

		public async Task<List<ReviewInvitation>> GetByArticleIdAsync(int articleId, CancellationToken ct = default)
				=> await Query()
						.Where(x => x.ArticleId == articleId)
						.ToListAsync(ct);

		public async Task<bool> OpenInvitationExistsAsync(int articleId, int? userId, string? email, CancellationToken ct)
		{
				return await _dbContext.ReviewInvitations
						.AnyAsync(x =>
								x.ArticleId == articleId &&
								x.Status == InvitationStatus.Open &&
								(
										(userId != null && x.UserId == userId) ||
										(userId == null && x.Email.Value == email!)
								),
								ct);
		}
}
