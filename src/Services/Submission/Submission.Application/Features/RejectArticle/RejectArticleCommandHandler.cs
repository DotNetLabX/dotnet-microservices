using Submission.Application.Features.ApproveArticle;
using Submission.Domain.StateMachines;

namespace Submission.Application.Features.RejectArticle;

//public class RejectArticleCommandHandler(ArticleRepository articleRepository, ArticleStateMachineFactory stateMachineFactory)
//		: ArticleActionCommandHandler<RejectArticleCommand>(articleRepository, stateMachineFactory)
//{		
//		protected override ArticleStage NextStage => ArticleStage.InitialRejected;
//}

public class RejectArticleCommandHandler(ArticleRepository _articleRepository, ArticleStateMachineFactory _stateMachineFactory)
		: IRequestHandler<RejectArticleCommand, IdResponse>
{
		public async Task<IdResponse> Handle(RejectArticleCommand command, CancellationToken cancellationToken)
		{
				var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);

				article.Reject(command, _stateMachineFactory);

				await _articleRepository.SaveChangesAsync();

				return new IdResponse(article.Id);
		}
}
