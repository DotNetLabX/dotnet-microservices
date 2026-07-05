// {Svc}.Domain/{Aggregate}s/{Aggregate}.cs — REDIS variant (Journals shape, persistence-4).
// A CRUD-shaped aggregate with no write-side invariants gets a document store and NONE of the EF
// machinery: no DbContext, no interceptor, no AggregateRoot base, no domain-event queue.
// Lifted from src/Services/Journals/Journals.Domain/Journals/Journal.cs.

using Blocks.Redis;                // Entity (int Id via Redis INCR)
using Redis.OM.Modeling;

namespace {Svc}.Domain.{Aggregate}s;

[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof({Aggregate}) })]
public partial class {Aggregate} : Entity   // Blocks.Redis.Entity, NOT AggregateRoot
{
    [Indexed]
    public required string Abbreviation { get; set; }

    // Searchable text + a normalized copy maintained on set, for case-insensitive search.
    private string _name = null!;
    [Searchable]
    public required string Name
    {
        get => _name;
        set { _name = value; NormalizedName = _name.ToLowerInvariant(); }
    }
    [Indexed(Sortable = true)]
    public required string NormalizedName { get; set; }

    [Indexed(JsonPath = "$.Name")]
    public List<{Child}> {Child}s { get; set; } = new();
}

// ---------------------------------------------------------------------------------------------------
// Persistence & events for the Redis variant:
//
// * Repository: inject Blocks.Redis.Repository<{Aggregate}>. AddAsync assigns the Id from a Redis INCR
//   counter when Id == 0. Use ReplaceAsync (delete+insert) when updating nested collections —
//   Redis.OM does not update child collections in place.
//
// * Domain events are raised from the ENDPOINT via FastEndpoints PublishAsync, not an aggregate queue
//   (there is no interceptor to drain one). The event record is still a plain IDomainEvent carrying the
//   full entity. Lifted from Journals.API/Features/Journals/Create/CreateJournalEndpoint.cs:
//
//       var {aggregate} = command.Adapt<{Aggregate}>();
//       await _{aggregate}Repository.AddAsync({aggregate});
//       await PublishAsync(new {Aggregate}Created({aggregate}));   // Journals.Domain event: record {Aggregate}Created({Aggregate} {Aggregate}) : IDomainEvent;
//       await Send.OkAsync(new IdResponse({aggregate}.Id));
//
// * A factory partial (Behaviors/{Child}.cs -> static Create) is the sanctioned place for construction
//   logic when materializing from another source (e.g. gRPC PersonInfo). Lifted from Editor.Create.
