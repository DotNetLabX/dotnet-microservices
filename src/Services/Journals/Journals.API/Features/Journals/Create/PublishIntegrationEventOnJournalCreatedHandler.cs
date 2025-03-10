using MassTransit;
using Blocks.Redis;
using Articles.IntegrationEvents.Contracts.Journals;
using Journals.Domain.Journals.Events;

namespace Journals.API.Features.Journals.Create;

public class PublishIntegrationEventOnJournalCreatedHandler(Repository<Journal> _journalRepository, IPublishEndpoint _publishEndpoint)
		: IEventHandler<JournalCreated>
{
		public async Task HandleAsync(JournalCreated notification, CancellationToken ct)
		{
				var journal = await _journalRepository.GetByIdAsync(notification.Journal.Id);

				var journalDto = journal.Adapt<JournalDto>();

				await _publishEndpoint.Publish(new JournalCreatedEvent(journalDto), ct);
		}
}
