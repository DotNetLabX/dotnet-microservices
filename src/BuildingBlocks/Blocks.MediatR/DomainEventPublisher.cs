using Blocks.Domain;
using MediatR;

namespace Blocks.MediatR;

public sealed class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
		public Task PublishAsync(IDomainEvent @event, CancellationToken ct = default)
				=> mediator.Publish(@event, ct);
}
