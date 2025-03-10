namespace Review.Application.Features.Articles.AssignEditor;

public class AssignEditorCommandHandler(ArticleRepository _articleRepository, ReviewDbContext _dbContext)
    : IRequestHandler<AssignEditorCommand, IdResponse>
{
    public async Task<IdResponse> Handle(AssignEditorCommand command, CancellationToken ct)
    {
				var editor = await _dbContext.Editors.FindByIdOrThrowAsync(command.EditorId);

				var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);

        article.AssignEditor(editor, command);

        await _articleRepository.SaveChangesAsync(ct);

        return new IdResponse(article.Id);
    }
}
