using Microsoft.AspNetCore.Authorization;
using Production.API.Features.Assets.RequestAssets._Shared;
using Production.Application.StateMachines;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.RequestAssets.Final;

[Authorize(Roles = Role.ProdAdmin)]
//insight about custom verbs
[HttpPut("articles/{articleId:int}/assets/final:request")]
[Tags("Assets")]
public class RequestFinalAssetsEndpoint(ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, AssetStateMachineFactory factory)
        : RequestAssetsEndpointBase<RequestFinalAssetsCommand>(articleRepository, assetTypeRepository, factory)
{
    //configure
}
