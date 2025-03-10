using FluentValidation;
using Production.API.Features.Shared;

namespace Production.API.Features.Assets.DownloadFile;

public record DownloadFileCommand(int ArticleId, int FileId);

public class DownloadFileCommandValidator : BaseValidator<DownloadFileCommand>
{
		public DownloadFileCommandValidator()
		{
				RuleFor(r => r.ArticleId).GreaterThan(0);
				RuleFor(r => r.FileId).GreaterThan(0);
		}
}