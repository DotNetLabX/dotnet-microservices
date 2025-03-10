using FileStorage.Contracts;
using Production.Persistence.Repositories;
using Production.Application.StateMachines;
using Production.API.Features.Assets.UploadFiles._Shared;

namespace Production.API.Features.Assets.UploadFiles.UploadFinalFile;

[Authorize(Roles = Role.Typesetter)]
[HttpPost("articles/{articleId:int}/assets/final/files:upload")]
[AllowFileUploads]
[Tags("Files")]
public class UploadFinalFileEndpoint(
    ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, IFileService fileService, AssetStateMachineFactory factory)
    : UploadFileEndpoint<UploadFinalFileCommand>(articleRepository, assetTypeRepository, fileService, factory)
{
}
