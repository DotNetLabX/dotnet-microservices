using Blocks.Domain;
using FastEndpoints;

namespace Blocks.FastEndpoints;

public sealed class DomainEventPublisher : IDomainEventPublisher
{
		public Task PublishAsync(IDomainEvent @event, CancellationToken ct = default)
				=> @event.PublishAsync(Mode.WaitForAll, ct);
}
