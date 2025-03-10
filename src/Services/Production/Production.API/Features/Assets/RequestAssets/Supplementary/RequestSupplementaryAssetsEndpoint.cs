using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Production.API.Features.Assets.RequestAssets._Shared;
using Production.Application.StateMachines;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.RequestAssets.Supplementary;

[Authorize(Roles = Role.Typesetter)]
[HttpPut("articles/{articleId:int}/assets/supplementary:request")]
[Tags("Assets")]
public class RequestSupplementaryAssetsEndpoint(ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, AssetStateMachineFactory factory)
    : RequestAssetsEndpointBase<RequestSupplementaryAssetsCommand>(articleRepository, assetTypeRepository, factory)
{
}
