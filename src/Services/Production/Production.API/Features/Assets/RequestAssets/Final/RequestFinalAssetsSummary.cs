using FastEndpoints;
using Production.API.Features.Assets.RequestAssets._Shared;

namespace Production.API.Features.Assets.RequestAssets.Final;

public class RequestFinalAssetsSummary : Summary<RequestFinalAssetsEndpoint>
{
    public RequestFinalAssetsSummary()
    {
        Summary = "Requests final assets for an article";
        Description = "Creates or transitions final assets (PDF, HTML, Epub) to Requested state";
        Response<RequestAssetsResponse>(200, "Final assets were successfully requested");
    }
}
