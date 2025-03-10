using Blocks.Core;
using Blocks.Domain;

namespace Blocks.EntityFrameworkCore;

public static partial class DbContextExtensions
{
		public static async Task<int> DispatchDomainEventsAsync(this DbContext ctx, IDomainEventPublisher eventPublisher, CancellationToken ct = default)
		{
				var aggregates = ctx.ChangeTracker
						//.Entries<IAggregateRoot>(); this is not always safe
						.Entries()
						.Select(a => a.Entity)
						.OfType<IAggregateRoot>()
						.Where(a => a.DomainEvents.Any())
						.ToList();
				
				if(aggregates.IsEmpty())
						return 0;

				var domainEvents = aggregates
						.SelectMany(a => a.DomainEvents)
						//.Select(x => (IEvent)x) //there is a bug in FastEndpoints regarding interface inheritance, therefore we need IEvent here
						.ToList();

				aggregates.ToList()
						.ForEach(a => a.ClearDomainEvents());

				foreach (var domainEvent in domainEvents)
						await eventPublisher.PublishAsync(domainEvent, ct);

				return domainEvents.Count;
		}
}
