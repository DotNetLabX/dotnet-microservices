# Create Aggregate

## Pattern

**Reference:** `src/Services/Submission/Submission.Domain/Entities/Article.cs` (state) + `src/Services/Submission/Submission.Domain/Behaviours/Article.cs` (behavior)

### State file: `{ServiceName}.Domain/{AggregateName}/{AggregateName}.cs`

```csharp
public partial class {AggregateName} : AggregateRoot
{
    // Required properties
    public required string {Property} { get; init; }

    // Backing collections (private list, public readonly)
    private readonly List<{ChildEntity}> _{items} = new();
    public IReadOnlyList<{ChildEntity}> {Items} => _{items}.AsReadOnly();

    // Navigation properties
    public int {ForeignKeyId} { get; init; }
}
```

### Behavior file: `{ServiceName}.Domain/{AggregateName}/Behaviors/{AggregateName}.cs`

```csharp
public partial class {AggregateName}
{
    public void {DomainMethod}({Parameters})
    {
        // Validate business rules
        // Mutate state
        // Raise domain event
        AddDomainEvent(new {EventName}(this, action));
    }
}
```

## Folder structure

```
{ServiceName}.Domain/
├── {AggregateName}/
│   ├── {AggregateName}.cs              (state — properties, collections)
│   ├── Behaviors/
│   │   └── {AggregateName}.cs          (behavior — domain methods, partial class)
│   ├── Events/
│   │   └── {EventName}.cs             (domain event records)
│   └── ValueObjects/
│       └── {ValueObjectName}.cs       (value objects)
```

## EF Core entity configuration

Create in `{ServiceName}.Persistence/EntityConfigurations/`:

```csharp
internal class {AggregateName}EntityConfiguration : AuditedEntityConfiguration<{AggregateName}>
{
    public override void Configure(EntityTypeBuilder<{AggregateName}> builder)
    {
        base.Configure(builder);
        builder.Property(e => e.{Property}).HasMaxLength(MaxLength.C64).IsRequired();
        // OwnsOne for value objects
        builder.OwnsOne(e => e.{ValueObject}, b => { ... });
    }
}
```
