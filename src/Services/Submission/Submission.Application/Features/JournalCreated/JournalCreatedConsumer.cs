using MassTransit;
using Articles.IntegrationEvents.Contracts.Journals;

namespace Submission.Application.Features.JournalCreated;

public class JournalCreatedConsumer(Repository<Journal> _journalRepository) : IConsumer<JournalCreatedEvent>
{
		public async Task Consume(ConsumeContext<JournalCreatedEvent> context)
		{
				var journalDto = context.Message.Journal;

				// Idempotency: skip if already exists
				var existing = await _journalRepository.GetByIdAsync(journalDto.Id);
				if (existing is not null)
						return;

				var journal = new Journal
				{
						Id = journalDto.Id,
						Name = journalDto.Name,
						Abbreviation = journalDto.Abbreviation						
				};

				await _journalRepository.AddAsync(journal);
				await _journalRepository.SaveChangesAsync();
		}
}
