// {Svc}.Domain/Entities/{Aggregate}.cs — EF-backed variant, STATE partial.
// What the aggregate HAS. No behavior methods here (those live in Behaviors/{Aggregate}.cs).
// Lifted from src/Services/Submission/Submission.Domain/Entities/Article.cs (domain-2).

namespace {Svc}.Domain.Entities;

public partial class {Aggregate} : AggregateRoot   // AggregateRoot = Blocks.Entities, TPrimaryKey = int (domain-1)
{
    internal {Aggregate}() { }                      // EF materialization ctor; keep internal

    // Scalar state — init for immutable-after-create, set only where a behavior must mutate it.
    public required string Title { get; init; }
    public {Enum} Stage { get; set; }

    // Owned reference (FK relationship configured in persistence).
    public int {Parent}Id { get; init; }
    public required {Parent} {Parent} { get; init; } = null!;

    // Child collections: private backing field, exposed read-only. Mutated ONLY by behavior methods.
    private readonly List<{Child}> _{child}s = new();
    public IReadOnlyList<{Child}> {Child}s => _{child}s.AsReadOnly();
}
