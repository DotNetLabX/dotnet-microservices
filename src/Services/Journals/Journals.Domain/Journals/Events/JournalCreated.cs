using Blocks.Domain;

namespace Journals.Domain.Journals.Events;

public record JournalCreated(Journal Journal) : IDomainEvent;
