# Upload Refactoring — Final Cleanup

## Summary

Three remaining cleanup items from the upload feature review: eliminate duplicate `IAssetActionCommand` interface, add file size validation to upload validators, and verify timeline propagation for uploads.

## Affected services

- **Production** — all changes are here

## Steps

### Step 1 — Replace `IAssetActionCommand` with `IAssetAction`

Both interfaces are identical (`IArticleAction<AssetActionType>`). `IAssetAction` lives in the Domain layer (correct location), `IAssetActionCommand` is a duplicate in the API layer.

**Files to modify:**

- `src/Services/Production/Production.API/Features/_Shared/AssetActionCommand.cs`
  - Delete `IAssetActionCommand` interface (lines 8-11)
  - Change `AssetActionCommand<TResponse>` to implement `IAssetAction` instead of `IAssetActionCommand`
  - Remove the `//todo - do I need this interface?` comment
  - Keep `IAssetActionResponse` — it's used by `AssetActionCommand` and `AssetActionResponse`
- `src/Services/Production/Production.API/Features/_Shared/AssetBaseEndpoint.cs`
  - Constraint already uses `IAssetAction` (`where TCommand : IAssetAction`) — no change needed

**Pattern:** Follow existing — `AssetCommand<TResponse>` in `ArticleCommand.cs` already implements `IAssetAction`.

### Step 2 — Add file size validation to `UploadFileValidator`

Currently no validator checks `IFormFile.Length` against `AssetTypeDefinition.MaxFileSizeInMB`. The old commented-out code had this but depended on the removed `AssetProvider` pattern.

**Approach:** Inject `AssetTypeRepository` into `UploadFileValidator` and validate file size against the asset type's `MaxFileSizeInMB`.

**Files to modify:**

- `src/Services/Production/Production.API/Features/Assets/UploadFiles/_Shared/UploadFileCommand.cs`
  - Add `AssetTypeRepository` as constructor parameter to `UploadFileValidator<T>`
  - Add rule: `RuleFor(r => r.File)` — check `file.Length <= assetType.MaxFileSizeInMB * 1024 * 1024`
  - Lookup uses `AssetType` from the command, resolve via `AssetTypeRepository.GetById()`
  - Use existing `ValidatorsMessagesConstants.InvalidFileSize` message
  - Remove the `//RuleFor(r => r.File.Length)` comment placeholder

**Note:** FastEndpoints resolves validator constructor dependencies from DI automatically, so `AssetTypeRepository` injection will work without additional registration.

### Step 3 — Verify timeline propagation for uploads (no code changes)

**Verified — uploads DO propagate to ArticleTimeline.** The flow is:

1. `UploadFileEndpoint.HandleAsync` → `asset.CreateAndAddFile(metadata, assetType, command)`
2. `Asset.CreateAndAddFile` → `SetState(Uploaded, action)` → `AddAction(action)`
3. `AddAction` → `AddDomainEvent(new AssetActionExecuted(action, stage, type, number, file))`
4. `TransactionalDispatchDomainEventsInterceptor` dispatches after SaveChanges
5. `AddTimelineWhenAssetActionExecutedHandler` handles the event with `SourceType=ActionExecuted`, `SourceId="Upload"`
6. Timeline template resolves: `"<<UserName>> uploaded the <<UploadedFile>>"`
7. Visibility: POF, TSOF, CORAUT roles can see upload entries

**No changes needed** — the flow works correctly after the `SetState` fix we already made in `CreateAndAddFile`.

## Open questions

None.
