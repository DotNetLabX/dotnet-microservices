using FastEndpoints;
using Production.API.Features.Assets.RequestAssets._Shared;

namespace Production.API.Features.Assets.RequestAssets.CancelRequest;

public class CancelRequestFinalAssetsSummary : Summary<CancelRequestFinalAssetsEndpoint>
{
    public CancelRequestFinalAssetsSummary()
    {
        Summary = "Cancels a request for final assets";
        Description = "Reverts requested final assets back to Uploaded state";
        Response<RequestAssetsResponse>(200, "Final asset requests were successfully cancelled");
    }
}
