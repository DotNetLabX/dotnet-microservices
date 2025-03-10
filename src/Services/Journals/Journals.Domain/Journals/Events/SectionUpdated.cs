using Blocks.Domain;

namespace Journals.Domain.Journals.Events;

public record SectionUpdated(Section Section) : IDomainEvent;

