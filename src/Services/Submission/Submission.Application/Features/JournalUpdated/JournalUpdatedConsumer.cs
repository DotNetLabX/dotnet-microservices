using MassTransit;
using Articles.IntegrationEvents.Contracts.Journals;

namespace Submission.Application.Features.JournalUpdated;

public class JournalUpdatedConsumer(Repository<Journal> _journalRepository) : IConsumer<JournalUpdatedEvent>
{
		public async Task Consume(ConsumeContext<JournalUpdatedEvent> context)
		{
				var journalDto = context.Message.Journal;
				
				var journal = new Journal
				{
						Id = journalDto.Id,
						Name = journalDto.Name,
						Abbreviation = journalDto.Abbreviation
				};

				await _journalRepository.UpsertAsync(journal);
				await _journalRepository.SaveChangesAsync();
		}
}
