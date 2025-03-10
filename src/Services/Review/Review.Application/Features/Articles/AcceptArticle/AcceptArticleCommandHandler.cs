namespace Review.Application.Features.Articles.AcceptArticle;

public class AcceptArticleCommandHandler(ArticleRepository _articleRepository, ArticleStateMachineFactory _stateMachineFactory)
        : IRequestHandler<AcceptArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(AcceptArticleCommand command, CancellationToken cancellationToken)
    {
        var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);

        article.Accept(_stateMachineFactory, command);

        await _articleRepository.SaveChangesAsync();

        return new IdResponse(article.Id);
    }
}
