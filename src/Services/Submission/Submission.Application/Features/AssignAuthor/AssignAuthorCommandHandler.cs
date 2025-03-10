namespace Submission.Application.Features.AssignAuthor;

public class AssignAuthorCommandHandler(ArticleRepository _articleRepository, SubmissionDbContext _dbContext)
		: IRequestHandler<AssignAuthorCommand, IdResponse>
{
		public async Task<IdResponse> Handle(AssignAuthorCommand command, CancellationToken cancellationToken)
		{
				var author = await _dbContext.Authors.FindByIdOrThrowAsync(command.AuthorId);

				var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);

				article.AssignAuthor(author, command.ContributionAreas, command.IsCorrespondingAuthor, command);

				await _articleRepository.SaveChangesAsync();

				return new IdResponse(article.Id);
		}
}
