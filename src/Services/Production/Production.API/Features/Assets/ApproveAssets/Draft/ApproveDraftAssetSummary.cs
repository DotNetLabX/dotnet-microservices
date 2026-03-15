using FastEndpoints;
using Production.API.Features.Shared;

namespace Production.API.Features.Assets.ApproveAssets.Draft;

public class ApproveDraftAssetSummary : Summary<ApproveDraftAssetEndpoint>
{
    public ApproveDraftAssetSummary()
    {
        Summary = "Approves a draft asset";
        Description = "Approves a draft asset, transitioning the article to the FinalProduction stage";
        Response<AssetActionResponse>(200, "Draft asset was successfully approved");
    }
}
