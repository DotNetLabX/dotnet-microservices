# Tech-Debt Metric Layer (C1)

- **Run id:** td-run-1
- **Date:** 2026-07-10 (metrics computed 2026-07-11)
- **Commit:** `96dde60` (branch `main`)
- **Method:** deterministic — every number below was computed by running a command; nothing estimated.

## Preflight

| Tool | Result |
|---|---|
| lizard | 1.23.0 — OK (expected 1.23.0) |
| java | NOT FOUND (`java: command not found`) |
| Code Maat | UNAVAILABLE (jar not present, no JVM) → **git-only fallback used** for revisions and coupling |
| git log source | `git log --all --numstat --date=short --pretty=format:'--%h--%ad--%aN' --no-renames` |
| lizard source | `lizard src -l csharp --csv -x "*/obj/*" -x "*/bin/*" -x "*/Migrations/*" -x "*/StrykerOutput/*"` (exclusions verified: 0 excluded-path rows in CSV) |

Scope filter applied everywhere: `src/**/*.cs`, excluding `obj/`, `bin/`, `Migrations/`, `StrykerOutput/`.
Project lifetime: first commit 2025-03-10 → last commit 2026-07-07 (~16 months).

## Bot filter

`git shortlog -sne --all`:

| Commits | Author |
|---|---|
| 19 | ldumit \<dumitrescu.laurentiu@gmail.com\> |
| 3 | Laurentiu Dumitrescu \<dumitrescu.laurentiu@gmail.com\> |
| 2 | laurentiu \<dumitrescu.laurentiu@gmail.com\> |

- **Commits analyzed: 24. Commits excluded by bot filter: 0** (no bot authors present).
- All 3 aliases share one email → **one human maintainer**.
- Commit shape: only 13 of 24 commits touch any tracked `.cs` file; 3 are bulk commits touching 704 / 557 / 554 files (initial import 2025-03-10, two repo-wide passes 2025-12-07); the remaining 11 commits are docs/skills-only.

## Hotspot filter (strict) — DID NOT FIRE

- 741 tracked `.cs` files with ≥1 revision. Per-file revision counts: μ = 2.668, σ = 0.983 → threshold μ+3σ = **5.62**, AND required > 16 changes (1/month over 16-month lifetime).
- **Maximum per-file revision count in the repo is 5. Zero files qualify. The hotspot filter yielded no hotspots — stated explicitly, not silently replaced.**
- Fallback ranking used instead (per runbook): complexity-weighted size (total function NLOC, boosted by presence of CCN≥10 functions), revision count as tiebreaker.
- Context: the 3 bulk commits give every long-lived file a baseline of ~3 revisions; only counts ≥ 4 indicate any focused, intentional change.

## Per-area ranking (fallback ranking; verdicts reflect it)

NLOC = sum of function NLOC from lizard (declaration-only files contribute 0 — 421 of 741 tracked files contain measurable functions). Revisions = sum of per-file revision counts in the area (bulk-inflated, comparative use only).

| # | Area | Files (w/ fns) | NLOC | Max CCN | Fns CCN≥10 | Revisions | Verdict |
|---|---|---|---|---|---|---|---|
| 1 | buildingblocks | 81 | 1566 | 10 | 1 | 440 | warm |
| 2 | review | 89 | 1424 | 6 | 0 | 384 | warm |
| 3 | submission | 77 | 1100 | 6 | 0 | 354 | warm |
| 4 | production | 69 | 977 | 6 | 0 | 335 | warm |
| 5 | auth | 37 | 580 | 5 | 0 | 142 | warm |
| 6 | journals | 28 | 314 | 4 | 0 | 118 | cold |
| 7 | modules-filestorage | 6 | 307 | 6 | 0 | 31 | cold |
| 8 | articlehub | 16 | 271 | 4 | 0 | 78 | cold |
| 9 | modules-articletimeline | 11 | 185 | 3 | 0 | 60 | cold |
| 10 | modules-email | 7 | 91 | 3 | 0 | 32 | cold |
| 11 | apigateway | 0 | 0 | 0 | 0 | 3 | cold |

- **No area is "hot"**: the strict hotspot filter fired on zero files and no function exceeds CCN 10. Verdicts are relative within this small, healthy repo.
- Functions with CCN ≥ 15: **0 repo-wide**. Functions with CCN ≥ 10: **1 repo-wide**.
- apigateway is YARP config + top-level `Program.cs`; lizard sees no named functions there.

## Top 25 functions by CCN

| CCN | NLOC | Function | File |
|---|---|---|---|
| 10 | 45 | HasuraMetadataInitService::ExecuteAsync | src/BuildingBlocks/Blocks.Hasura/HasuraMetadataInitService.cs |
| 8 | 19 | RequestContextMiddleware::InvokeAsync | src/BuildingBlocks/Blocks.AspNetCore/Middlewares/RequestContextMiddleware.cs |
| 8 | 11 | FormFileExtensions::GetContentType | src/BuildingBlocks/Blocks.AspNetCore/Extensions/FormFileExtensions.cs |
| 7 | 32 | RequestDiagnosticsMiddleware::InvokeAsync | src/BuildingBlocks/Blocks.AspNetCore/Middlewares/RequestDiagnosticsMiddleware.cs |
| 7 | 14 | ServiceCollectionExtensions::InheritsFrom | src/BuildingBlocks/Blocks.Core/Extensions/ServiceCollectionExtensions.cs |
| 7 | 14 | Entity::Equals | src/BuildingBlocks/Blocks.Domain/Entities/Entity.cs |
| 6 | 40 | ArticleApprovedForReviewConsumer::CreateActors | src/Services/Review/Review.Application/Features/Articles/InitializeFromSubmission/ArticleApprovedForReviewConsumer.cs |
| 6 | 26 | FileService::UploadInternalAsync | src/Modules/FileService/FileService.MongoGridFS/FileService.cs |
| 6 | 12 | ReviewInvitationRepository::OpenInvitationExistsAsync | src/Services/Review/Review.Persistence/Repositories/ReviewInvitationRepository.cs |
| 6 | 12 | ArticleAccessChecker::HasAccessAsync | src/Services/Submission/Submission.Application/ArticleAccessChecker.cs |
| 6 | 11 | ArticleAccessChecker::HasAccessAsync | src/Services/Production/Production.Application/ArticleAccessChecker.cs |
| 6 | 11 | ArticleAccessChecker::HasAccessAsync | src/Services/Review/Review.Application/ArticleAccessChecker.cs |
| 5 | 25 | FileService::UploadInternalAsync | src/Modules/FileService/FileService.AzureBlob/FileService.cs |
| 5 | 25 | FileService::UploadInternalAsync | src/Modules/FileService/FileStorage.MinIO/FileService.cs |
| 5 | 25 | ArticleAcceptedForProductionConsumer::CreateActors | src/Services/Production/Production.API/Features/Articles/InitializeFromReview/ArticleAcceptedForProductionConsumer.cs |
| 5 | 22 | CreateUserEndpoint::HandleAsync | src/Services/Auth/Auth.API/Features/Users/CreateAccount/CreateUserEndpoint.cs |
| 5 | 21 | HasuraMetadataService::TrackAllRelationshipsAsync | src/BuildingBlocks/Blocks.Hasura/HasuraMetadataService.cs |
| 5 | 20 | GlobalExceptionMiddleware::InvokeAsync | src/BuildingBlocks/Blocks.AspNetCore/Middlewares/GlobalExceptionMiddleware.cs |
| 5 | 16 | EnumerableExtensions::ToString\<T\> | src/BuildingBlocks/Blocks.Core/Extensions/EnumerableExtension.cs |
| 5 | 16 | UserRole::Create | src/Services/Auth/Auth.Domain/Users/Behaviors/UserRole.cs |
| 5 | 15 | CreateReviewerCommandHandler::Handle | src/Services/Review/Review.Application/Features/Reviewers/CreateReviewer/CreateReviewerCommandHandler.cs |
| 5 | 13 | ManualGenerateIdScope::ManualGenerateIdScope | src/BuildingBlocks/Blocks.EntityFrameworkCore/Seeding/ManualGenerateIdScope.cs |
| 5 | 12 | TransactionalDispatchDomainEventsInterceptor::SavingChangesAsync | src/BuildingBlocks/Blocks.EntityFrameworkCore/Interceptors/TransactionalDispatchDomainEventsInterceptor.cs |
| 5 | 10 | ReflectionExtensions::GetValue | src/BuildingBlocks/Blocks.Core/Extensions/ReflectionExtensions.cs |
| 5 | 9 | RequestContextMiddleware::ResolveCorrelationId | src/BuildingBlocks/Blocks.AspNetCore/Middlewares/RequestContextMiddleware.cs |

Deterministic observation for miners (from function names/paths alone, no judgment): `ArticleAccessChecker::HasAccessAsync` appears 3× (Submission/Review/Production, CCN 6 each), `FileService::UploadInternalAsync` 3× (MongoGridFS/AzureBlob/MinIO), `CreateActors` consumer logic 2× (Review/Production) — near-identical CCN/NLOC suggests cross-service duplication as a mining lead.

## Per-file revisions — top 20

Absolute churn (added+deleted lines) shown as context only — **absolute churn is a poor predictor of debt; do not rank by it**.

| Revs | Churn | File |
|---|---|---|
| 5 | 682 | src/Services/Submission/Submission.Domain/Behaviours/Article.cs |
| 5 | 208 | src/Services/Production/Production.Domain/Articles/Behavior/Article.cs |
| 5 | 137 | src/BuildingBlocks/Blocks.Domain/ValueObjects/ValueObject.cs |
| 5 | 110 | src/Services/Submission/Submission.Domain/Behaviours/File.cs |
| 5 | 81 | src/BuildingBlocks/Blocks.EntityFrameworkCore/EntityConfigurations/AuditedEntityConfiguration.cs |
| 5 | 75 | src/Services/Journals/Journals.Persistence/DependencyInjection.cs |
| 5 | 42 | src/BuildingBlocks/Blocks.Domain/ValueObjects/StringValueObject.cs |
| 5 | 40 | src/BuildingBlocks/Blocks.Domain/ValueObjects/SingleValueObject.cs |
| 4 | 716 | src/Services/Review/Review.Domain/Articles/Behaviors/Article.cs |
| 4 | 339 | src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs |
| 4 | 335 | src/BuildingBlocks/Blocks.AspNetCore/Grpc/GrpcClientRegistrationExtensions.cs |
| 4 | 332 | src/Services/Review/Review.Domain/Assets/Behavior/Asset.cs |
| 4 | 330 | src/Services/Auth/Auth.API/DependencyInjection.cs |
| 4 | 314 | src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/Repository.cs |
| 4 | 296 | src/Modules/FileService/FileService.MongoGridFS/FileStorageRegistration.cs |
| 4 | 272 | src/Services/Auth/Auth.Application/TokenFactory.cs |
| 4 | 261 | src/Services/Production/Production.API/DependencyInjection.cs |
| 4 | 244 | src/BuildingBlocks/Blocks.Redis/Extensions.cs |
| 4 | 229 | src/Modules/ArticleTimeline/ArticleTimeline.Application/ArticleTimeline.Application/EventHandlers/AddTimelineEventHandler.cs |
| 4 | 225 | src/Services/Submission/Submission.Application/Features/UploadFiles/_Shared/UploadFileCommand.cs |

Note: the domain behavior partials (`Article.cs`, `Asset.cs`, `File.cs` across Submission/Review/Production) dominate the genuinely-revised files — the write-side domain layer is where real change concentrates.

## Change coupling — FLAGGED WEAK

**coupling signal WEAK: 24 commits, several repo-wide bulk commits inflate co-occurrence.** Three commits touched 704 / 557 / 554 tracked files each, so 153,182 file pairs pass the co-occurrence ≥ 3 threshold — at threshold 3 the signal is pure bulk-commit noise. Pairs below have co-occurrence ≥ 4 (requires at least one non-bulk co-change); treat even these as weak leads, not evidence.

| Co-occ | File A | File B |
|---|---|---|
| 5 | Blocks.Domain/ValueObjects/SingleValueObject.cs | Blocks.Domain/ValueObjects/StringValueObject.cs |
| 5 | Production.Domain/Articles/Behavior/Article.cs | Submission.Domain/Behaviours/Article.cs |
| 4 | Blocks.Redis/Extensions.cs | Blocks.Redis/IntIdGenerationStrategy .cs |
| 4 | Blocks.Redis/Extensions.cs | Blocks.Redis/Repository.cs |
| 4 | Blocks.Redis/Extensions.cs | Journals.Persistence/DependencyInjection.cs |
| 4 | Blocks.Redis/Extensions.cs | Journals.Persistence/JournalDbContext.cs |
| 4 | Blocks.Redis/IntIdGenerationStrategy .cs | Blocks.Redis/Repository.cs |
| 4 | Blocks.Redis/Repository.cs | Journals.Persistence/DependencyInjection.cs |
| 4 | Journals.Persistence/DependencyInjection.cs | Journals.Persistence/JournalDbContext.cs |
| 4 | Blocks.Domain/Entities/AggregateRoot.cs | Blocks.Domain/Entities/Entity.cs |

(A wide band of further pairs sits at exactly 4, dominated by Blocks.Domain base types co-changing with everything — consistent with bulk refactor commits, not runtime coupling.) Deterministic side-note: `IntIdGenerationStrategy .cs` has a space before its extension (actual filename on disk).

## Degraded signals

- **ownership signal DEGRADED: single maintainer (3 aliases, 1 person) — column dropped**
- **coupling signal WEAK: 24 commits, several repo-wide bulk commits inflate co-occurrence** (3 commits touch 704/557/554 files; 153,182 pairs pass the ≥3 threshold)
- **Code Maat DEGRADED: java not installed, jar absent — git-only fallback (numstat log) used for revisions and coupling**
- **hotspot filter DID NOT FIRE: 0 files exceed μ+3σ = 5.62 AND > 16 changes (max per-file revisions = 5) — fallback ranking (complexity-weighted size, revisions as tiebreaker) used for the area verdicts**
- **churn CONTEXT-ONLY: absolute added+deleted lines reported but not used for ranking (poor predictor)**
- **revision counts INFLATED: 3 bulk commits give long-lived files a baseline of ~3 revisions; only counts ≥ 4 reflect focused change**

## Recommended scan list (top 5 by honest fallback ranking)

1. **buildingblocks** — largest function mass (1566 NLOC, 81 files), the repo's only CCN≥10 function (HasuraMetadataInitService::ExecuteAsync), 6 of the top-6 CCN functions, and every service depends on it (highest blast radius).
2. **review** — biggest service by files and NLOC (89 files, 1424 NLOC), holds the most complex consumer/actor-creation logic and 716-churn Article behavior partial.
3. **submission** — genuine change concentrates in its domain behaviors (Article.cs: 5 revs / 682 churn, File.cs: 5 revs); it is the head of the article lifecycle write path.
4. **production** — mirrors submission/review with 4–5-rev domain behavior partials and duplicated ArticleAccessChecker/CreateActors variants — the strongest cross-service duplication lead.
5. **auth** — security-critical surface (TokenFactory: 4 revs, CreateUserEndpoint CCN 5, UserRole::Create CCN 5) at moderate size; complexity is low but the domain warrants a lens pass.
