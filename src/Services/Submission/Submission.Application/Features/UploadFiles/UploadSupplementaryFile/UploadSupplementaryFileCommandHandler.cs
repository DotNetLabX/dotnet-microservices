using FileStorage.Contracts;
using Submission.Application.Features.UploadFiles.Shared;

namespace Submission.Application.Features.UploadFiles.UploadAuthorFile;

public class UploadSupplementaryFileCommandHandler(
    ArticleRepository articleRepository,
		AssetTypeRepository assetTypeRepository,
		IFileService fileService,
		ArticleStateMachineFactory stateMachineFactory)
    : UploadFileCommandHandler<UploadSupplementaryFileCommand>(articleRepository, assetTypeRepository, fileService, stateMachineFactory)
{
}
