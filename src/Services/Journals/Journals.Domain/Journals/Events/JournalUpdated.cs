using Blocks.Domain;

namespace Journals.Domain.Journals.Events;

public record JournalUpdated(Journal Journal) : IDomainEvent;
