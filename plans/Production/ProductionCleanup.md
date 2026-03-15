# Production Service Cleanup

## Summary

Fix DDD modeling violations, naming typos, and dead code in the Production service identified during architecture review.

## Affected services

- **Production** — all changes are scoped to this service
- **Root CLAUDE.md** — guardrail wording update (#7)

## Domain model changes

- `AssetAction` base class: `AggregateRoot` → `Entity`

## File list

### Step 1 — AssetAction: AggregateRoot → Entity
- `src/Services/Production/Production.Domain/Assets/AssetAction.cs` — change base class

### Step 2 — Fix `Submited` → `Submitted` typo
- `src/Services/Production/Production.Domain/Articles/Article.cs` — rename properties `SubmitedById` → `SubmittedById`, `SubmitedBy` → `SubmittedBy`, `SubmitedOn` → `SubmittedOn`
- `src/Services/Production/Production.Application/Dtos/ArticleSummaryDto.cs` — rename `SubmitedOn` → `SubmittedOn`, `SubmitedBy` → `SubmittedBy`
- `src/Services/Production/Production.Persistence/EntityConfigurations/ArticleEntityConfiguration.cs` — update property references
- New migration via `dotnet ef migrations add RenameSubmittedColumns`

### Step 3 — Fix `*EndpointEndpoint` class names
- `src/Services/Production/Production.API/Features/Articles/GetArticle/GetArticleSummaryEndpointEndpoint.cs` — rename class to `GetArticleSummaryEndpoint`, rename file to `GetArticleSummaryEndpoint.cs`
- `src/Services/Production/Production.API/Features/Articles/GetArticle/GetArticleAssetsEndpointEndpoint.cs` — rename class to `GetArticleAssetsEndpoint`, rename file to `GetArticleAssetsEndpoint.cs`

### Step 4 — Fix `BaseEnpoint.cs` filename
- `src/Services/Production/Production.API/Features/_Shared/BaseEnpoint.cs` — rename file to `BaseEndpoint.cs`

### Step 5 — Delete dead `Orders/` code
- Delete `src/Services/Production/Production.Domain/Orders/` (entire folder: Address.cs, Contact.cs, Customer.cs, Order.cs, PhoneNumber.cs)
- Delete `src/Services/Production/Production.Persistence/EntityConfigurations/Orders/` (entire folder: CustomerEntityConfiguration.cs)
- Remove commented-out `CustomerEntityConfiguration` line from `src/Services/Production/Production.Persistence/ProductionDbContext.cs`

### Step 6 — Update guardrail wording in root CLAUDE.md
- Change `No interfaces without multiple implementations` to `No interfaces without multiple implementations for repositories`

## Numbered steps

### Step 1 — Fix AssetAction base class
- Open `src/Services/Production/Production.Domain/Assets/AssetAction.cs`
- Change `public partial class AssetAction : AggregateRoot` to `public partial class AssetAction : Entity`
- Remove the comment `//insight - modification never happens for an action`
- No migration needed — `AggregateRoot` and `Entity` both map to the same table structure (Id column)

### Step 2 — Fix `Submited` typo + migration
- In `Article.cs`: rename `SubmitedById` → `SubmittedById`, `SubmitedBy` → `SubmittedBy`, `SubmitedOn` → `SubmittedOn`
- In `ArticleSummaryDto.cs`: rename the two record properties
- In `ArticleEntityConfiguration.cs`: update all `e.SubmitedOn`, `e.SubmitedBy`, `e.SubmitedById` references
- Check Mapster mappings in `MappingConfig.cs` — if `Submited` is referenced there, update too
- Run: `dotnet ef migrations add RenameSubmittedColumns -p src/Services/Production/Production.Persistence -s src/Services/Production/Production.API`
- Verify the generated migration contains `RenameColumn` calls (not drop+create)
- If EF generates drop+create instead of rename, manually write the migration with `migrationBuilder.RenameColumn()`

### Step 3 — Fix EndpointEndpoint class names
- Rename class `GetArticleSummaryEndpointEndpoint` → `GetArticleSummaryEndpoint` in `GetArticleSummaryEndpointEndpoint.cs`
- Rename the file to `GetArticleSummaryEndpoint.cs`
- Rename class `GetArticleAssetsEndpointEndpoint` → `GetArticleAssetsEndpoint` in `GetArticleAssetsEndpointEndpoint.cs`
- Rename the file to `GetArticleAssetsEndpoint.cs`
- No other files reference these classes by name (FastEndpoints discovers them by convention)

### Step 4 — Fix BaseEnpoint filename
- Rename file `Features/_Shared/BaseEnpoint.cs` → `Features/_Shared/BaseEndpoint.cs`
- Class inside is already named `BaseEndpoint` — no code change needed

### Step 5 — Delete dead Orders code
- Delete entire folder `src/Services/Production/Production.Domain/Orders/`
- Delete entire folder `src/Services/Production/Production.Persistence/EntityConfigurations/Orders/`
- In `ProductionDbContext.cs`, remove the commented-out line: `//modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());`

### Step 6 — Update CLAUDE.md guardrail
- In root `CLAUDE.md`, find `No interfaces without multiple implementations`
- Change to `No interfaces without multiple implementations for repositories`

## Cross-service impacts

None. All changes are internal to Production. The `Submited` → `Submitted` rename affects only the Production API contract (DTOs) — no other service consumes these DTOs directly.

## Migration notes

- **Step 2** requires a new EF migration in Production.Persistence, applied against Production.API
- Command: `dotnet ef migrations add RenameSubmittedColumns -p src/Services/Production/Production.Persistence -s src/Services/Production/Production.API`
- Then: `dotnet ef database update -p src/Services/Production/Production.Persistence -s src/Services/Production/Production.API`

## Open questions

None.
