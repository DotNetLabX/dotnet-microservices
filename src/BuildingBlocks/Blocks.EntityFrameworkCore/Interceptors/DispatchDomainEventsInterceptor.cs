using Blocks.Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Blocks.EntityFrameworkCore.Interceptors;

public class DispatchDomainEventsInterceptor(IDomainEventPublisher _publisher) : SaveChangesInterceptor
{
		public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
		{
				result = await base.SavedChangesAsync(eventData, result, ct);
				
				if (eventData.Context is not null)
						await eventData.Context.DispatchDomainEventsAsync(_publisher, ct);

				return result;
		}
}
