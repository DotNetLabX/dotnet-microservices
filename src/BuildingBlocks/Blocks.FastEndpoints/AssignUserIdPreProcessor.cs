using Blocks.Core.Security;
using Blocks.Domain;
using FastEndpoints;

namespace Blocks.FastEnpoints;

public class AssignUserIdPreProcessor : IGlobalPreProcessor
{
		public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
		{
				if (context.Request is IAuditableAction articleCommand)
				{
						var claimsProvider = context.HttpContext.Resolve<IClaimsProvider>();
						articleCommand.CreatedById = claimsProvider.GetUserId();
				}

				return Task.CompletedTask;
		}
}
