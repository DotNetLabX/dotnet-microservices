---
name: persistence-patterns
description: EF Core persistence for this repo — DbContext setup, the two-level entity-configuration ladder (audited aggregates, opt-in RowVersion), the repository-as-unit-of-work base, JSON seeding, and the type-keyed reference-data cache. Use when adding or changing a DbContext, an entity configuration, a repository, seed data, or caching in a service's `.Persistence` project, and to pick the storage engine (SQL Server / PostgreSQL / Redis.OM) for a service's role.
---

# Persistence Patterns

Four rules carry the persistence layer. Break one and the code is wrong here, not just unidiomatic:

1. **The storage engine follows the service's role** — SQL Server for write-side services, PostgreSQL for the read projection, Redis.OM for CRUD-shaped aggregates. The last of those gets *none* of the EF machinery below (persistence-4).
2. **The repository IS the unit of work.** `SaveChangesAsync`, `ClearTracking`, and the range operations all live on the repository base. There is no `IUnitOfWork` anywhere, and you do not add one (persistence-1).
3. **Audit is mandatory for aggregates; concurrency is opt-in and OFF.** An aggregate's config extends `AuditedEntityConfiguration`; the `RowVersion` token exists but defaults to disabled and has zero opt-ins to date (persistence-2).
4. **Caching is for small reference tables only** — marked with `ICacheable`, keyed by CLR type, whole-table, no eviction (persistence-5).

## Pick the storage engine first (persistence-4)

This decision precedes every phase below — it selects which recipe you follow.

| Service role | Engine | Provider call | Table naming | Follow |
|---|---|---|---|---|
| Write-side (Auth, Submission, Review, Production) — real invariants, aggregates, domain events | SQL Server | `UseSqlServer(...)` | `UseEntityTypeNamesAsTables()` bare (namingx-1) | Phases 1–5 |
| Read projection (ArticleHub) — denormalized view fed by consumers | PostgreSQL | `UseNpgsql(...)` | `UseEntityTypeNamesAsTables(new SnakeCaseNameRewriter(...))` (namingx-1) | Phases 1–5, plain data-bag entities, no cache loader |
| CRUD-shaped aggregate (Journals) — no write-side invariants | Redis.OM | none — no EF provider | Redis keys, no tables | the Redis.OM variant section only |

Reserve EF's full machinery for services with real write-side invariants. A CRUD-shaped aggregate gets a document store without the ceremony (persistence-4).

## Phase 1 — the DbContext (conventions-3, namingx-1/2)

Name it `{Service}DbContext` — mirror the owner (`SubmissionDbContext`, `AuthDbContext`, `ArticleHubDbContext`). Never a generic `AppDbContext`. (Journals is the sanctioned singular-name exception, `JournalDbContext`, because it is Redis, not EF — conventions-3.) Derive from the shared `ApplicationDbContext<TDbContext>` base, which supplies the cache accessors used in Phase 5 — this is the shape for four of the five EF contexts (Submission, Review, Production, ArticleHub).

**Auth is the base-class exception.** Because its `User` aggregate extends `IdentityUser<int>`, `AuthDbContext` derives `IdentityDbContext<User, Role, int>` — **not** `ApplicationDbContext<T>` — takes no `IMemoryCache`, and does not call `UseEntityTypeNamesAsTables` (its tables come from ASP.NET Identity + plain EF PascalCase defaults — namingx-1). Follow the `ApplicationDbContext<T>` template below unless the service is Identity-backed, in which case start from `IdentityDbContext<User, Role, int>` and add only your non-Identity `DbSet`s:

```csharp
// Auth.Persistence/AuthDbContext.cs — Identity-backed, no ApplicationDbContext base, no cache
public class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<User, Role, int>(options)
{
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }   // Users/Roles already in the base
    public virtual DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }
}
```

```csharp
// Submission.Persistence/SubmissionDbContext.cs — SQL Server write-side
public partial class SubmissionDbContext(DbContextOptions<SubmissionDbContext> options, IMemoryCache cache)
    : ApplicationDbContext<SubmissionDbContext>(options, cache)
{
    #region Entities
    public virtual DbSet<Article> Articles { get; set; }
    public virtual DbSet<AssetTypeDefinition> AssetTypes { get; set; }
    // ...one DbSet per aggregate/entity, in a #region Entities block
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        modelBuilder.UseEntityTypeNamesAsTables();          // bare → PascalCase tables
    }

    public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        this.UnTrackCacheableEntities();                    // keep ICacheable rows out of the tracker (Phase 5)
        return await base.SaveChangesAsync(ct);
    }
}
```

`UseEntityTypeNamesAsTables()` exists to name each table after its TPH **root** type (it walks to the root base type), not to pick a case style — Auth reaches PascalCase from plain EF defaults with no call at all (namingx-2). The snake_case rewriter is passed **only** by the PostgreSQL context, to match Postgres's own convention; the three SQL Server contexts call it bare (namingx-1):

```csharp
// ArticleHub.Persistence/ArticleHubDbContext.cs — PostgreSQL read projection
using EFCore.NamingConventions.Internal;
using System.Globalization;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.UseEntityTypeNamesAsTables(new SnakeCaseNameRewriter(CultureInfo.InvariantCulture));
    modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
}
```

**Registration** lives in the service's `Persistence/DependencyInjection.cs`. The `ISaveChangesInterceptor` line wires domain-event dispatch, which is a separate concern (persistence-3) — copy it as-is:

```csharp
services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
services.AddDbContext<SubmissionDbContext>((provider, options) =>
{
    options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
    options.UseSqlServer(connectionString);                // UseNpgsql(...) for the read projection
});

services.AddScoped(typeof(Repository<>));                  // the open generic (Phase 3)
services.AddDerivedTypesOf(typeof(Repository<>));          // ArticleRepository, PersonRepository, ...
services.AddScoped<AssetTypeRepository>();                 // cached repository (Phase 5)
services.AddHostedService<DatabaseCacheLoader>();          // cache prewarm (Phase 5)
```

## Phase 2 — entity configurations (persistence-2, conventions-4)

Every configuration inherits from one rung of the shared ladder. The rung you pick declares what the entity **is**:

| Entity kind | Base class | What the base gives you |
|---|---|---|
| Plain child entity / owned table | `EntityConfiguration<T>` (int key) or `EntityConfiguration<T, TKey>` | `HasKey`, id generated-or-not toggle, master-data seeding |
| **Aggregate root** | `AuditedEntityConfiguration<T>` | the above **plus** audit columns (mandatory) and the opt-in `RowVersion` token |
| Enum-backed reference entity | `EnumEntityConfiguration<T, TEnum>` | unique `Name` index, `Name`/`Description` columns with enum conversion |
| Composite-key metadata table | `MetadataConfiguration<T>` | `ToTable` + seeding; you set the composite `HasKey` in the derived config |

The ladder itself (from `Blocks.EntityFrameworkCore`), so you know exactly what `base.Configure` does before you call it:

```csharp
// EntityConfiguration<T> : EntityConfiguration<T, int> — the single-arg rung adds the id toggle
protected virtual bool HasGeneratedId => true;            // override => false for seeded / externally-keyed ids
public override void Configure(EntityTypeBuilder<T> builder)
{
    if (HasGeneratedId) builder.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnOrder(0);
    else                builder.Property(e => e.Id).ValueGeneratedNever().HasColumnOrder(0);
    base.Configure(builder);
}

// EntityConfiguration<T, TKey> — the root rung: key + free seeding on every Configure()
public virtual void Configure(EntityTypeBuilder<T> builder)
{
    builder.HasKey(e => e.Id);
    builder.SeedFromJsonFile();                            // Phase 4 — automatic master-data seed
}

// AuditedEntityConfiguration<T, TKey> : EntityConfiguration<T, TKey> — constrained to IAggregateRoot
protected virtual string DefaultDateSql => "GETUTCDATE()";
protected virtual bool HasConcurrencyToken => false;      // opt-in, default OFF — do not flip without a real concurrency need
public override void Configure(EntityTypeBuilder<T> builder)
{
    base.Configure(builder);
    builder.Property(e => e.CreatedOn).IsRequired().HasDefaultValueSql(DefaultDateSql);
    builder.Property(e => e.CreatedById).IsRequired();
    builder.Property(e => e.LastModifiedOn);
    builder.Property(e => e.LastModifiedById);
    if (HasConcurrencyToken)
        builder.Property<byte[]>("RowVersion").IsRowVersion();
}
```

`DefaultDateSql` is a `virtual` hook, and its default `"GETUTCDATE()"` is **SQL Server T-SQL** applied through `HasDefaultValueSql` (which lives in EFCore.Relational — the token is a raw dialect string EF does not translate). It is correct only because the four audited contexts are all SQL Server. If you ever put audited aggregates on a different engine, override `DefaultDateSql` with that dialect's expression (Postgres would be `now() at time zone 'utc'`). Note this override does **not** exist in the repo today: ArticleHub (PostgreSQL) is a read projection whose entities are not audited aggregates — it extends plain `EntityConfiguration<T>`, sets `HasGeneratedId => false`, and gives its date columns `HasColumnType("timestamp without time zone")` with no default-value SQL.

A real aggregate configuration — note it extends `AuditedEntityConfiguration<Article>` (audit is not optional for an aggregate) and every string length is a `MaxLength` constant, **never** a numeric literal (conventions-4):

```csharp
// Submission.Persistence/EntityConfigurations/ArticleEntityConfiguration.cs
public class ArticleEntityConfiguration : AuditedEntityConfiguration<Article>
{
    public override void Configure(EntityTypeBuilder<Article> builder)
    {
        base.Configure(builder);                           // key + audit + seeding, already applied
        builder.HasIndex(e => e.Title);
        builder.Property(e => e.Title).HasMaxLength(MaxLength.C256).IsRequired();
        builder.Property(e => e.Scope).HasMaxLength(MaxLength.C2048).IsRequired();
        builder.Property(e => e.Stage).HasEnumConversion().IsRequired();
        // relationships: HasOne/HasMany with explicit OnDelete...
    }
}
```

`MaxLength` (in `Blocks.Core`) is a fixed ladder of power-of-two constants — `C8, C16, C32, C64, C128, C256, C512, C1024, C2048`. Column lengths are a shared vocabulary; if you write `HasMaxLength(500)` you are wrong (conventions-4). Enum-backed reference entities (`AssetTypeDefinition`, `Stage`) extend `EnumEntityConfiguration<T, TEnum>` and get their `Name`/`Description` columns for free; add only the entity-specific properties.

## Phase 3 — repositories (persistence-1)

The base `RepositoryBase<TContext, TEntity, TKey>` (in `Blocks.EntityFrameworkCore`) is where the unit-of-work surface lives — `SaveChangesAsync`, `ClearTracking`, `AddRange`/`UpdateRange`/`RemoveRange`, plus the query surface (`Query()`, `QueryNotTracked()`). You do not wrap it in an `IUnitOfWork`; one seam per aggregate is the whole design (persistence-1).

One deliberate oddity to preserve: `DeleteByIdAsync` drops to raw SQL rather than the EF `State = Deleted` trick — because that trick requires instantiating an empty entity, which is impossible when the entity has required properties. Keep the raw form and its insight comment:

```csharp
public virtual async Task<bool> DeleteByIdAsync(TKey id, CancellationToken ct = default)
{
    // insight - executing an SQL is the most eficient way
    // insight - setting the state to Deleted doesn't work
    // because it requires to instantiate an empty entity first which is not possible if the entity has required properties
    var rows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM {TableName} WHERE Id = {id}", ct);
    return rows > 0;
}
```

A second base-method caveat: `UpsertAsync` updates an existing row via `_dbContext.Entry(existingEntity).CurrentValues.SetValues(entity)` (Repository.cs). `SetValues(object)` copies only the source's **mapped scalar properties**, by name — it does not touch navigation properties, child collections, or backing-field-only state. So upserting an aggregate refreshes its scalar columns but silently leaves related entities untouched; if the update must replace a child collection you handle that yourself (the Redis side keeps a delete-then-insert workaround for the same limitation). Use `UpsertAsync` for flat rows; reach for an explicit load-and-mutate when relationships are in play.

Each service binds the base to its own context with a one-liner open generic, then adds domain-specific repositories on top:

```csharp
// Submission.Persistence/Repositories/Repository.cs — bind base to this service's context
public class Repository<TEntity>(SubmissionDbContext dbContext)
    : RepositoryBase<SubmissionDbContext, TEntity>(dbContext)
        where TEntity : class, IEntity<int>;

// ArticleRepository.cs — a domain-specific repository: override Query() to shape the aggregate graph,
// and add named query methods. No IArticleRepository — a domain-specific repo has one implementation (repo guardrail).
public class ArticleRepository(SubmissionDbContext dbContext) : Repository<Article>(dbContext)
{
    public override IQueryable<Article> Query()
        => base.Entity.Include(e => e.Actors).ThenInclude(e => e.Person).Include(e => e.Assets);

    public async Task<Article?> GetFullArticleByIdAsync(int id, CancellationToken ct = default)
        => await Query().Include(e => e.Journal).Include(e => e.SubmittedBy)
                        .SingleOrDefaultAsync(e => e.Id == id, ct);
}
```

`AddDerivedTypesOf(typeof(Repository<>))` in DI registers every such derived repository automatically — do not hand-register each one (the commented-out `AddScoped<ArticleRepository>` lines in the DI file are dead and stay dead).

For the load-or-fail case, don't null-check the result of a get: use the shared throwing extensions in `RepositoryExtensions` (Blocks.EntityFrameworkCore), which wrap the repository/`DbSet`/`IQueryable` and raise a `NotFoundException`/`BadRequestException` for you — `FindByIdOrThrowAsync(id)`, `GetByIdOrThrowAsync(id)`, `SingleOrThrowAsync(predicate)`, `ExistsOrThrowAsync(id)`, `EnsureNotExistsOrThrowAsync(id)`. These are the persistence-side spelling of the guards in the error-handling skill; prefer them over an inline `?? throw`.

## Phase 4 — seeding

Two seeding paths, chosen by where the data belongs:

- **Master / reference data → design-time `HasData`, automatic.** Drop a `Data/Master/{EntityName}.json` file and the base `Configure`'s `builder.SeedFromJsonFile()` picks it up into a migration. You write nothing — seeding is free the moment the config inherits the ladder. Composite-key metadata tables get the same via `MetadataConfiguration`.
- **Test / runtime data → `context.SeedFromJsonFile<T>()`** (the DbContext extension), reading `Data/Test/{EntityName}.json` and calling `SaveChanges`, skipping if the table is non-empty. Used for dev fixtures, not shipped reference data.

Do not hand-write `HasData(...)` calls; the JSON-file convention is the seam.

**Two gotchas that make seeding silently do nothing:**

1. **The JSON file must be copied to the output directory** — both readers resolve it as `AppContext.BaseDirectory/Data/{Master|Test}/{Entity}.json`, and both return early (no error) when the file is absent. So every seed file needs a `.Persistence.csproj` entry with `CopyToOutputDirectory` set to `PreserveNewest` — `<None Update="Data\Master\X.json">` for a file that lives in the project, or `<Content Include="..\..\..\SharedData\Master\X.json" Link="Data\Master\X.json">` for one shared across services. Add the file without the csproj entry and seeding no-ops on a clean checkout.
2. **Runtime seeding of explicit-id rows needs `IDENTITY_INSERT`.** `SeedFromJsonFile<T>` wraps its `SaveChanges` in `UseManualGenerateId<T>(disableAutoGenerate)`, whose `ManualGenerateIdScope` toggles `SET IDENTITY_INSERT [schema].[table] ON/OFF` around the insert so seeded rows keep their JSON ids. That is **SQL Server-specific** T-SQL — the runtime `Data/Test` path is a SQL Server convenience, not portable to the Postgres or Redis services (which seed their own way; Redis resets a sequence seed instead — see the Redis variant).

## Phase 5 — caching (persistence-5)

Only small, rarely-changing reference tables are cached. Mark the entity `ICacheable` (the marker interface in `Blocks.Core.Cache`) and read it through one of the type-keyed accessors. The key is the CLR type's `FullName` — whole-table, process-wide, no eviction, and by construction it can hold nothing per-request or per-instance:

```csharp
// Blocks.Core/Cache/MemoryCacheExtensions.cs — the type-only key
public static T GetOrCreateByType<T>(this IMemoryCache cache, Func<ICacheEntry, T> factory)
    => cache.GetOrCreate(typeof(T).FullName!, factory)!;
```

Read paths, in order of preference:

- `dbContext.GetAllCached<TEntity>()` / `GetByIdCached<TEntity, TId>(id)` — on the `ApplicationDbContext` base, for read-through from any handler.
- A `CachedRepository<TDbContext, TEntity, TId>` subclass when a reference table wants its own repository type — e.g. `AssetTypeRepository : CachedRepository<SubmissionDbContext, AssetTypeDefinition, AssetType>`. It exposes `GetAll()` / `GetById(id)` off the same cache.
- `DatabaseCacheLoader : IHostedService` prewarms chosen tables at startup (`dbContext.GetAllCached<ArticleStageTransition>()`); register it with `AddHostedService`.

Because these rows are cached, the context's `SaveChangesAsync` override calls `this.UnTrackCacheableEntities()` so they never enter the change tracker (Phase 1). Do not cache anything that is not stable reference data.

## The Redis.OM variant — none of the above (persistence-4)

Journals is a CRUD-shaped aggregate with no write-side invariants, so it gets a document store and skips the entire EF stack: **no `DbContext` base, no interceptors, no configuration ladder, no repository-as-UoW, no migrations.** Do not reach for any Phase 1–5 machinery here.

- The "context" is a thin wrapper over `RedisConnectionProvider` exposing collections — not an EF `DbContext`:

```csharp
// Journals.Persistence/JournalDbContext.cs
public class JournalDbContext(RedisConnectionProvider provider)
{
    public IRedisCollection<Journal> Journals => provider.RedisCollection<Journal>();
    public IRedisCollection<Editor> Editors => provider.RedisCollection<Editor>();
    public RedisConnectionProvider Provider => provider;
}
```

- Entities extend `Blocks.Redis.Entity` (an `int Id` with `[RedisIdField]`/`[Indexed]`) — **not** `AggregateRoot` — and carry Redis.OM decorators (`[Document]`, `[Indexed]`, `[Searchable]`).
- Persistence goes through `Blocks.Redis.Repository<T>` (plain CRUD; ids via a Redis `StringIncrement` sequence).
- DI registers the provider and the open generic repository — no `AddDbContext`:

```csharp
// Journals.Persistence/DependencyInjection.cs
services.AddSingleton(new RedisConnectionProvider(connectionString));
services.AddSingleton<IConnectionMultiplexer>(redis);
services.AddSingleton<JournalDbContext>();
services.AddScoped(typeof(Repository<>));                  // Blocks.Redis.Repository<>, not the EF one
```

- Seeding is runtime JSON via `RedisConnectionProvider.SeedFromJson<T>` plus a sequence-seed reset, not `HasData`.

## Binding rules (each is a wrong-code check, not a preference)

1. DbContext is named `{Service}DbContext`; never `AppDbContext` (conventions-3).
2. Aggregate configs extend `AuditedEntityConfiguration` — audit columns are mandatory; leave `HasConcurrencyToken` OFF unless the service genuinely needs optimistic concurrency (persistence-2).
3. String column lengths come from `MaxLength.C*` constants — a numeric literal like `HasMaxLength(500)` is wrong (conventions-4).
4. Table naming matches the engine: `UseEntityTypeNamesAsTables()` bare for SQL Server; the `SnakeCaseNameRewriter` is passed **only** by the PostgreSQL context (namingx-1). The helper normalizes TPH-root names; it is not a case-style switch (namingx-2).
5. The repository is the unit of work — `SaveChangesAsync`/`ClearTracking` live on the base and there is no `IUnitOfWork` (persistence-1). Separately, don't give a domain-specific repository its own interface (`IArticleRepository`) — the repo guardrail allows an interface only where implementations are plural; `IRepository<T>` is the one shared base seam.
6. Cache only small reference tables, via `ICacheable` and the type-keyed accessors (persistence-5).
7. Engine follows role — do not put EF machinery on a Redis.OM service, or a Redis document model on a real write-side aggregate (persistence-4).

## What this skill does NOT do

- **Domain modeling** (aggregate behavior split, value objects, state machine) — separate concern.
- **Domain-event dispatch interceptors** — the `ISaveChangesInterceptor` wiring shown in Phase 1 belongs to persistence-3; this skill only shows where to register it.
- **Migrations commands** — see the project `CLAUDE.md` `dotnet ef` recipes.
- **gRPC / integration-event data replication** — how foreign reference rows arrive is a boundaries concern, not persistence.
