namespace Blocks.Domain;

public interface IDomainEventPublisher
{
		Task PublishAsync(IDomainEvent @event, CancellationToken ct = default);
}