# Template: reference-data shadow-row consumer pair

Foreign reference data that changes independently of the article flow (a Journal's name, abbreviation) is replicated eagerly by a **dedicated `JournalCreated` / `JournalUpdated` consumer pair** in each consuming write service (Submission, Review) — so downstream reads never need a synchronous call (datasharing-2). This is distinct from the article-event-embedded `GetOrCreate` hydration in `write-side-consumer.md`.

Idempotency here is the **silent** family (consumers-2): create skips if the row already exists; update upserts. A redelivery is normal and harmless, so nothing is thrown. Injection is a single concrete `Repository<Journal>` — no DbContext needed for a one-entity shadow row.

Feature-folder convention: one folder per event, `Features/{Journal}Created/` and `Features/{Journal}Updated/`.

## Created — silent skip-if-exists

From `Submission.Application/Features/JournalCreated/JournalCreatedConsumer.cs`:

```csharp
using MassTransit;
using Articles.IntegrationEvents.Contracts.Journals;

namespace Submission.Application.Features.JournalCreated;

public class JournalCreatedConsumer(Repository<Journal> _journalRepository) : IConsumer<JournalCreatedEvent>
{
    public async Task Consume(ConsumeContext<JournalCreatedEvent> context)
    {
        var journalDto = context.Message.Journal;

        // Idempotency: skip if already exists.
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
```

## Updated — upsert (idempotent by construction)

From `Submission.Application/Features/JournalUpdated/JournalUpdatedConsumer.cs`. `UpsertAsync` inserts if missing, otherwise copies values onto the tracked row — so a duplicate or an out-of-order delivery both converge to the same state:

```csharp
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
```

`UpsertAsync` lives on `Repository` in `Blocks.EntityFrameworkCore`: `FindByIdAsync` then either `AddAsync` or `Entry(existing).CurrentValues.SetValues(entity)`.
