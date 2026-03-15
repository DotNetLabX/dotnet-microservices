# Approve / Request / CancelRequest Refactoring

## Summary

Fix bugs and inconsistencies in the three asset-action feature groups: ApproveDraftAsset, RequestAssets (Final/Supplementary), and CancelRequestFinalAssets. The CancelRequest endpoint has an inverted condition bug that skips the wrong assets, bypasses the domain method, and lacks state machine validation.

## Affected services

- **Production** — all changes are internal

## Current problems

### CancelRequestFinalAssetsEndpoint (critical)

1. **Inverted condition bug** — `if (asset?.State == AssetState.Requested) continue` skips assets that ARE in Requested state. A cancel-request operation should skip assets that are NOT requested. The condition should be `!=`.
2. **Domain method ignored** — `Asset.CancelRequest()` exists but the endpoint calls `asset.SetState(AssetState.Uploaded, command)` directly, bypassing the domain validation.
3. **No state machine validation** — extends `BaseEndpoint` instead of `AssetBaseEndpoint`, so `CheckAndThrowStateTransition()` is never called.
4. **Dead command** — `CancelRequestAuthorFilesCommand` is defined but never used by any endpoint.

### ApproveDraftAssetEndpoint (minor)

5. **Inconsistent loading** — uses `AssetRepository.GetByIdAsync` while all other asset endpoints use `ArticleRepository.GetByIdWithAssetsAsync`. Works, but deviates from the pattern.
6. **No Summary** — missing FastEndpoints Summary class (compare with AssignTypesetter).

### RequestAssets endpoints (minor)

7. **No Summaries** — RequestFinalAssets, RequestSupplementaryAssets, CancelRequestFinalAssets all lack Summary classes.

### Domain — Asset.CancelRequest (fix)

8. **Incomplete domain method** — `CancelRequest()` validates state but never actually sets `State = Uploaded`. It only updates timestamps and adds the action. It should call `SetState(AssetState.Uploaded, action)` or set the state explicitly.

## Domain model changes

- `Asset.CancelRequest()` — fix to actually transition state to `AssetState.Uploaded` (currently only validates + adds action, doesn't change state)

## File list

| # | File | Action |
|---|------|--------|
| 1 | `src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs` | Modify — fix `CancelRequest()` |
| 2 | `src/Services/Production/Production.API/Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsEndpoint.cs` | Modify — fix bug, use domain method, add state machine validation |
| 3 | `src/Services/Production/Production.API/Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsCommand.cs` | Modify — delete `CancelRequestAuthorFilesCommand` |
| 4 | `src/Services/Production/Production.API/Features/Assets/ApproveAssets/Draft/ApproveDraftAssetEndpoint.cs` | Modify — switch to ArticleRepository for consistency |
| 5 | `src/Services/Production/Production.API/Features/Assets/ApproveAssets/Draft/ApproveDraftAssetSummary.cs` | Create |
| 6 | `src/Services/Production/Production.API/Features/Assets/RequestAssets/Final/RequestFinalAssetsSummary.cs` | Create |
| 7 | `src/Services/Production/Production.API/Features/Assets/RequestAssets/Supplementary/RequestSupplementaryAssetsSummary.cs` | Create |
| 8 | `src/Services/Production/Production.API/Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsSummary.cs` | Create |

## Numbered steps

### Step 1 — Fix Asset.CancelRequest domain method

**File:** `src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs`

Replace the `CancelRequest` method body to actually transition state. It should:
- Keep the `State != Requested` guard
- Call `SetState(AssetState.Uploaded, action)` instead of manually setting timestamps + AddAction (since `SetState` already does both)

This makes `CancelRequest` a proper domain operation: validate precondition → delegate to `SetState`.

**Pattern:** Same as how `CreateAndAddFile` delegates to `SetState(AssetState.Uploaded, action)`.

### Step 2 — Fix CancelRequestFinalAssetsEndpoint

**File:** `src/Services/Production/Production.API/Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsEndpoint.cs`

Changes:
- Extend `AssetBaseEndpoint<CancelRequestFinalAssetsCommand, RequestAssetsResponse>` instead of `BaseEndpoint` — this brings in `CheckAndThrowStateTransition` and `_stateMachineFactory`
- Add `ArticleRepository` as constructor parameter (same as `RequestAssetsEndpointBase` does)
- Fix the loop: invert the condition to `if (asset?.State != AssetState.Requested) continue` — skip assets that are NOT requested
- Call `asset.CancelRequest(command)` (the now-fixed domain method) instead of `asset.SetState(AssetState.Uploaded, command)`
- Add `CheckAndThrowStateTransition(asset, command.ActionType)` before the domain call
- Save via `_articleRepository.SaveChangesAsync()` instead of `_assetRepository`
- Remove `AssetRepository` from constructor — no longer needed

**Pattern:** Follow `RequestAssetsEndpointBase` for the overall structure.

**Depends on:** Step 1

### Step 3 — Remove dead CancelRequestAuthorFilesCommand

**File:** `src/Services/Production/Production.API/Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsCommand.cs`

Delete the `CancelRequestAuthorFilesCommand` record — no endpoint uses it and it inherits `RequestMultipleAssetsCommand` without overriding `ActionType`, so it would default to `AssetActionType.Request` which is wrong for a cancel operation.

### Step 4 — Switch ApproveDraftAssetEndpoint to ArticleRepository

**File:** `src/Services/Production/Production.API/Features/Assets/ApproveAssets/Draft/ApproveDraftAssetEndpoint.cs`

Changes:
- Add `ArticleRepository` as constructor parameter
- Load article via `_articleRepository.GetByIdWithAssetsAsync(command.ArticleId)`
- Find the asset from `_article.Assets.Single(a => a.Id == command.AssetId)`
- Remove `AssetRepository` from constructor
- Save via `_articleRepository.SaveChangesAsync()`
- Remove the commented-out `stateMachineFactory.ValidateStageTransition` line (dead code)

This aligns with how all other asset endpoints load data: article first, then find asset within it.

**Pattern:** Follow `RequestAssetsEndpointBase.HandleAsync` for the load pattern.

### Step 5 — Add Summary classes

Create four Summary classes following the `AssignTypesetterSummary` pattern:

**File:** `ApproveDraftAssetSummary.cs`
- Summary: "Approves a draft asset"
- Description: "Approves a draft asset, transitioning the article to the FinalProduction stage"
- Response: `AssetActionResponse`, 200

**File:** `RequestFinalAssetsSummary.cs`
- Summary: "Requests final assets for an article"
- Description: "Creates or transitions final assets (PDF, HTML, Epub) to Requested state"
- Response: `RequestAssetsResponse`, 200

**File:** `RequestSupplementaryAssetsSummary.cs`
- Summary: "Requests supplementary assets for an article"
- Description: "Creates or transitions supplementary assets to Requested state"
- Response: `RequestAssetsResponse`, 200

**File:** `CancelRequestFinalAssetsSummary.cs`
- Summary: "Cancels a request for final assets"
- Description: "Reverts requested final assets back to Uploaded state"
- Response: `RequestAssetsResponse`, 200

## Cross-service impacts

None. All changes are internal to Production.

## Migration notes

None. No schema changes.

## Open questions

None — resolved:
1. Remove the commented-out `stateMachineFactory.ValidateStageTransition` line in `ApproveDraftAssetEndpoint` (Step 4).
2. No supplementary cancel-request endpoint needed — this is a course, not every feature needs to be implemented.
