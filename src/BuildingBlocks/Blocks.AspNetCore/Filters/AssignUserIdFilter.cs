using Blocks.Core.Security;
using Blocks.Domain;
using Microsoft.AspNetCore.Http;

namespace Blocks.AspNetCore;

public sealed class AssignUserIdFilter(IClaimsProvider _claimsProvider) : IEndpointFilter
{
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
		{
				foreach (var arg in ctx.Arguments)
				{
						if (arg is IAuditableAction action && action.CreatedById == default)
						{
								var userId = _claimsProvider.TryGetUserId();
								if (userId is not null)
										action.CreatedById = userId.Value;
						}
				}
				return await next(ctx);
		}
}