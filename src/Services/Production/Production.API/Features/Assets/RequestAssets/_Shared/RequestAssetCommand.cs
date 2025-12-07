using Articles.Abstractions.Enums;
using FluentValidation;
using Production.API.Features.Shared;
using Production.Application.Dtos;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.RequestAssets._Shared;

public abstract record RequestAssetCommand : AssetCommand<RequestAssetsResponse>
{
    public override AssetActionType ActionType => AssetActionType.Request;

}
public abstract record RequestMultipleAssetsCommand : RequestAssetCommand
{
    //insight - about using List, Array, IEnumerable in the input/output models.
    //Preffer IEnumerable if for input since it doesn't change. And List for output since we need to add elements.
    public IEnumerable<AssetRequest> AssetRequests { get; set; } = null!;
    public record AssetRequest(AssetType AssetType, byte AssetNumber);
}

public class RequestAssetsResponse
{
    public IEnumerable<AssetMinimalDto> Assets { get; set; }
}

public abstract class RequestAssetsValidator<TRequestCommand> : ArticleCommandValidator<TRequestCommand>
        where TRequestCommand : RequestMultipleAssetsCommand
{
    public RequestAssetsValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(r => r.AssetRequests)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Request cannot be empty")
                .ForEach(r =>
                {
                    r
                    .Must(a => a.AssetNumber <= 10).WithMessage("Maximum 10 assets of the same type are allowed")
                    .Must(a => AllowedAssetTypes.Contains(a.AssetType)).WithMessage("Asset type not allowed");
                });

        //RuleFor(r => r).MustAsync(async (r, _, cancellation) => await IsActionValid(r))
        //    .WithMessage("Action not allowed");
    }

    public abstract IReadOnlyCollection<AssetType> AllowedAssetTypes { get; }
}