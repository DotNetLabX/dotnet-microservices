using MassTransit;
using Blocks.Redis;
using Articles.IntegrationEvents.Contracts.Journals;
using Journals.Domain.Journals.Events;

namespace Journals.API.Features.Journals.Update;

public class PublishIntegrationEventOnJournalUpdatedHandler(Repository<Journal> _journalRepository, IPublishEndpoint _publishEndpoint)
		: IEventHandler<JournalUpdated>
{
		public async Task HandleAsync(JournalUpdated notification, CancellationToken ct)
		{
				var journal = await _journalRepository.GetByIdAsync(notification.Journal.Id);

				var journalDto = journal.Adapt<JournalDto>();

				await _publishEndpoint.Publish(new JournalUpdatedEvent(journalDto), ct);
		}
}
