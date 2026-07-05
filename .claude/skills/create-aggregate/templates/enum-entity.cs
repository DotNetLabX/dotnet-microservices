// {Svc}.Domain/Entities/{EnumEntity}.cs
// Behavior-driving enum metadata is modeled as a REAL seeded entity keyed by the enum value —
// EnumEntity<TEnum> : Entity<TEnum> — NOT [Description] attributes + reflection (domain-4).
// The metadata (MaxAssetCount, AllowsMultipleAssets) is queryable/joinable domain data.
// Lifted from src/Services/Submission/Submission.Domain/Entities/AssetTypeDefinition.cs and Stage.cs.

using Blocks.Core.Cache;           // ICacheable — only if this reference table is cached (persistence-5)

namespace {Svc}.Domain.Entities;

// EnumEntity<TEnum> gives you Name (the enum value) + Description for free. Add the behavior metadata.
public partial class {EnumEntity} : EnumEntity<{Enum}>, ICacheable
{
    public required byte MaxAssetCount { get; init; }

    // Derived behavior lives on the entity, not in a switch elsewhere.
    public bool AllowsMultipleAssets => MaxAssetCount > 1;
}

// A plainer enum entity — Name + Description + one extra column.
// Lifted from Submission.Domain/Entities/Stage.cs.
public partial class {Aggregate}Stage : EnumEntity<{Enum}>
{
    public string Info { get; init; } = null!;
}
