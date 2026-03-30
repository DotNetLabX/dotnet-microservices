using MediatR;
using Blocks.Core.Security;
using Blocks.Domain;

namespace Blocks.MediatR.Behaviours;

public class AssignUserIdBehavior<TRequest, TResponse>
    (IClaimsProvider _claimsProvider) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableAction
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is IAuditableAction action)
        {
            var userId = _claimsProvider.TryGetUserId();
            if (userId is not null)
                action.CreatedById = userId.Value;
        }

        return await next();
    }
}