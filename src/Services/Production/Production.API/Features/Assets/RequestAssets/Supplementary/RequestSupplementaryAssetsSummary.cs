using FastEndpoints;
using Production.API.Features.Assets.RequestAssets._Shared;

namespace Production.API.Features.Assets.RequestAssets.Supplementary;

public class RequestSupplementaryAssetsSummary : Summary<RequestSupplementaryAssetsEndpoint>
{
    public RequestSupplementaryAssetsSummary()
    {
        Summary = "Requests supplementary assets for an article";
        Description = "Creates or transitions supplementary assets to Requested state";
        Response<RequestAssetsResponse>(200, "Supplementary assets were successfully requested");
    }
}
