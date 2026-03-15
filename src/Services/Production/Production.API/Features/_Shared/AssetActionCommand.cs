using Production.Application.Dtos;
using Production.Domain.Assets.Enums;
using Production.Domain.Shared;

namespace Production.API.Features.Shared;

public interface IAssetActionResponse
{
}


public abstract record AssetActionCommand<TResponse> : AssetCommand<TResponse>, IAssetAction
        where TResponse : IAssetActionResponse
{
    internal int FileId { get; set; }
}

public record AssetActionResponse(AssetMinimalDto Asset) : IAssetActionResponse;