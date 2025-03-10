using FastEndpoints;
using FileStorage.Contracts;
using Articles.Security;
using Articles.Abstractions.Enums;
using Microsoft.AspNetCore.Authorization;
using Production.Persistence.Repositories;
using Production.Application.StateMachines;
using Production.API.Features.Assets.UploadFiles._Shared;

namespace Production.API.Features.Assets.UploadFiles.UploadDraftFile;

[Authorize(Roles = Role.Typesetter)]
[HttpPost("articles/{articleId:int}/assets/draft/files:upload")]
[AllowFileUploads]
[Tags("Files")]
public class UploadDraftFileEndpoint(ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, IFileService fileService, AssetStateMachineFactory factory)
		: UploadFileEndpoint<UploadDraftFileCommand>(articleRepository, assetTypeRepository, fileService, factory)
{

		protected override ArticleStage NextStage => ArticleStage.DraftProduction;
}
