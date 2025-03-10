using FileStorage.Contracts;
using Review.Application.Features.Assets.UploadFiles.Shared;

namespace Review.Application.Features.Assets.UploadFiles.UploadManuscriptFile;

public class UploadManuscriptFileCommandHandler(
        ArticleRepository articleRepository,
        AssetTypeDefinitionRepository assetTypeRepository,
        IFileService fileService,
        ArticleStateMachineFactory stateMachineFactory)
        : UploadFileCommandHandler<UploadManuscriptFileCommand>(articleRepository, assetTypeRepository, fileService, stateMachineFactory)
{
}
