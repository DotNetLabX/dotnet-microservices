# Tech-Debt Registry — global (structure graph & cross-cutting)

**Species:** tech-debt triage registry (`mine-verify-repo`) — sibling to `docs/reference-model.md`. Rows never deleted; dispositions flip; append-only changelog.
**Scope:** the whole-repo structure graph (49 csproj nodes / 145 ProjectReference edges, built deterministically — no graphify output exists) + solution files + gateway topology.
**Provenance:** td-run-1, commit `96dde60`, mined + skeptic-verified 2026-07-11.
**Clean results (verified-absent, recorded so nobody re-litigates):** 0 dependency cycles (DFS over the full edge list) · 0 layering violations (Domain→Persistence/Application/API etc.) · 0 cross-service project references · 0 BuildingBlocks→Services/Modules edges · 0 Modules→Services edges · every csproj on disk is registered in `Articles.sln`. High in-degree tops (Blocks.Core 17, Blocks.Domain 11, Articles.Abstractions 10) are the by-design shared kernel, not god nodes.
**Triage:** architect-adjudicated 2026-07-11 (advisory); **owner ratification pending**.

| Id | Lens | Sev | Finding (symbol + condition) | Evidence (re-executed 2026-07-11) | Status | Disposition |
|----|------|-----|------------------------------|-----------------------------------|--------|-------------|
| glb-1 | structure | medium | **(corrected)** `src/Submission.sln` declares `UserServiceGrpcClient` at `ClientServices\UserServiceGrpcClient\UserServiceGrpcClient.csproj`, which does not exist on disk — `dotnet build Submission.sln` errors on the missing project (VS opens with one unloaded project). `Articles.sln` is clean — the architecture-reference lead claiming a similar dead entry there is **refuted** | per-path existence check over both slns → sole miss: the Submission.sln entry | verified (IMPRECISE→corrected) | accepted |
| glb-2 | structure | low | `EmailService.SendGrid.csproj` has in-degree 0 — a built, sln-registered pluggable provider with zero consumers (Auth.API wires Empty + Smtp only) | `grep -rl "EmailService.SendGrid.csproj" --include=*.csproj src` → empty | verified | **by-design** (ref: `reference-model.md` modules-1/2 — showcased swappable provider; selection is an explicit compile-time decision) |
| glb-3 | structure | low | `FileStorage.MinIO.csproj` has in-degree 0 — a built, sln-registered provider with zero consumers, and the implementation itself is marked `// todo - not tested` (reference-model run-3 routed it as debt: an untested prototype, not a showcased alternative) | `grep -rl "FileStorage.MinIO.csproj" --include=*.csproj src` → empty | verified | accepted |
| glb-4 | structure | low | `src/Services/Review/Review.Domain.Tests` exists on disk (`bin/`, `obj/`, `StrykerOutput/`) with no csproj — an orphaned test-project husk invisible to the graph and absent from both slns; residue proving a test suite existed and was removed | `ls` → 3 dirs, no csproj; `grep Review.Domain.Tests *.sln` → no match | verified | accepted |
| glb-5 | structure | medium | YARP gateway cluster addresses wrong for 3 of 4 routed services: submission→`4403` (**ArticleHub's live port** — submission traffic cross-routes to the wrong service), production→`4407` (dead port; actual 4406), articlehub→`4409` (dead port; actual 4403); only review→`4405` is correct *(provenance: architecture-reference §13.2 lead, sharpened — the cross-route to a live wrong service was not in the lead)* | `ApiGateway/appsettings.json` destinations vs all six `launchSettings.json` (4401–4406 verified per service) | verified | accepted |

## Changelog

- 2026-07-11 — td-run-1 (commit `96dde60`): initial registry, 5 rows (3 CONFIRMED, 1 IMPRECISE→corrected, 1 sharpened lead). Clean-graph results recorded above.
