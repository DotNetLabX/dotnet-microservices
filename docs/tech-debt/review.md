# Tech-Debt Registry — review

**Species:** tech-debt triage registry (`mine-verify-repo`) — sibling to `docs/reference-model.md`. Rows never deleted; dispositions flip; append-only changelog.
**Scope:** `src/Services/Review/**`. **Provenance:** td-run-1, commit `96dde60`, mined + skeptic-verified 2026-07-11.
**Hotspot priority:** strict hotspot filter did not fire (see `metrics.md`); fallback area rank **2/11** (largest service by files/NLOC).
**Triage:** architect-adjudicated 2026-07-11 (advisory); **owner ratification pending**.

| Id | Lens | Sev | Finding (symbol + condition) | Evidence (re-executed 2026-07-11) | Status | Disposition |
|----|------|-----|------------------------------|-----------------------------------|--------|-------------|
| rev-1 | code-quality | low | `Review.API/FileStorage/SubmissionFileService.cs` contains only a commented-out class declaration — dead file | file body = usings + 2 commented lines | verified | accepted |
| rev-2 | code-quality | low | `ReviewFileService` subclass declared but never referenced; DI registers the generic `FileService` default instead — dead subclass | `grep -rn "ReviewFileService" src/Services/Review --include=*.cs` → declaration only | verified | accepted |
| rev-3 | code-quality | low | Typo `Resonse` propagated across exactly 6 files in the GetArticle and GetArticleInvitations slices | `grep -rln "Resonse" src/Services/Review` → 6 files | verified | accepted |
| rev-4 | architecture | low | Nested `Directory.Packages.props` at Review.API — the only nested CPM file in the repo — re-declares three centrally managed versions (same values today) and adds 2 Roslyn packages absent from root; nearest-file-wins shadowing is a future-drift hazard *(provenance: also a reference-model run-2 lead)* | `find src -iname Directory.Packages.props` → 2; nested file re-pins Carter 9.0.0, EFCore.Design 9.0.8, OpenApi 9.0.8 | verified | accepted |
| rev-5 | architecture | medium | `ReviewInvitation.Status` is `public { get; set; }` while `Accept()`/`Decline()` guard `Status != Open` + expiry — the guard is bypassable by direct assignment | `ReviewInvitation.cs:27` + `Behaviors/ReviewInvitation.cs:10,:13,:22` | verified | accepted |
| rev-6 | architecture | medium | `ArticleApprovedForReviewConsumer.CreateAssets` uploads files in a per-asset loop before a single terminal `SaveChangesAsync` with no try/catch or `TryDeleteAsync` compensation — unlike `UploadFileCommandHandler`, which wraps the same sequence in try/catch + compensating delete; failure mid-loop orphans uploaded bytes and MassTransit redelivery re-uploads | consumer `:64-87` (no try) vs `UploadFiles/_Shared/UploadFileCommandHandler.cs:22-33` | verified | accepted |
| rev-7 | performance | low | `ArticleRepository.Query()` unconditionally Includes `Actors.Person` **and** `Assets`; 5 command handlers via `GetByIdOrThrowAsync` pay the Assets join even when unused | repo `Query()` + 5 handler hits (AssignEditor, UploadFile, Accept/DeclineInvitation, InviteReviewer) | verified | accepted |
| rev-8 | performance | low | Read-only `GetArticleQueryHandler` uses the tracked 4-Include query; the only `AsNoTracking` in Review.Persistence is cached reference data | `grep -rn "AsNoTracking" src/Services/Review --include=*.cs` → 1 hit (`AssetRepository.cs:26`) | verified | accepted |
| rev-9 | test-coverage | medium | `ReviewInvitation.Accept()/Decline()` is the sole enforcement of invitation status + expiry, sits behind a public setter (rev-5), zero tests (Review.Domain.Tests is a csproj-less husk — glb-4) | `Behaviors/ReviewInvitation.cs:8-23`; Glob `**/*Tests*.csproj` → none | verified | deferred (owner-ratified 2026-07-11: nets mined on demand via `mine-verify-cover`) |
| rev-10 | test-coverage | low | `ArticleStateMachine.CanFire` drives every legal Article stage transition from cache-loaded rows; the permit table is untested | `ArticleStateMachine.cs:19,:24-26` | verified | deferred (owner-ratified 2026-07-11: nets mined on demand via `mine-verify-cover`) |
| rev-11 | test-coverage | low | `ArticleAccessChecker.HasAccessAsync` (CCN 6 per `metrics.md`) is the single role/resource gate for protected Review endpoints; its 4 branches are untested | 4-branch body; `metrics.md` CCN row | verified | deferred (owner-ratified 2026-07-11: nets mined on demand via `mine-verify-cover`) |

| rev-12 | architecture | medium | `Review.Domain/Assets/Asset.cs` `File` is `public { get; set; }` — same shape as sub-6 (Submission's Asset); repo-wide `.File =` assignment grep finds zero external writers, so the tightening is compile-safe | `grep -n "File File" src/Services/Review/Review.Domain/Assets/Asset.cs` → `:21 public File File { get; set; } = null!;` | verified | accepted (in scope: adhoc-DomainInvariantGuards) |

## Changelog

- 2026-07-11 — td-run-1 (commit `96dde60`): initial registry, 11 rows (all CONFIRMED).
- 2026-07-11 — follow-up discovery appended: rev-12 (architect Phase-1 sweep for adhoc-DomainInvariantGuards). Registry: 12 rows.
