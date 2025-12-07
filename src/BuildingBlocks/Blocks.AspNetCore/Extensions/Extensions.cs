using Microsoft.AspNetCore.Http;

namespace Blocks.AspNetCore;

public static class Extensions
{
    public static string? BaseUrl(this HttpRequest req)
    {
        if (req == null) return null;
        var uriBuilder = new UriBuilder(req.Scheme, req.Host.Host, req.Host.Port ?? -1);
        if (uriBuilder.Uri.IsDefaultPort)
        {
            uriBuilder.Port = -1;
        }

        return uriBuilder.Uri.AbsoluteUri;
    }
}
