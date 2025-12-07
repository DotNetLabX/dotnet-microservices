namespace Submission.Application.Features.SubmitArticle;

public class SubmitArticleCommandHandler(ArticleRepository _articleRepository, ArticleStateMachineFactory _stateMachineFactory)
    : IRequestHandler<SubmitArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(SubmitArticleCommand command, CancellationToken ct)
    {
        var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId, ct);

        article.Submit(command, _stateMachineFactory);

        await _articleRepository.SaveChangesAsync(ct);

        return new IdResponse(article.Id);
    }
}
