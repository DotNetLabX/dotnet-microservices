# adhoc-DomainInvariantGuards — Questions

## Q1: Step 1 breaks the build — `Journal.CreateArticle` sets `Article.Stage` via object initializer from a different class
**From:** developer
**To:** architect
**Status:** Resolved
**Step:** Step 1 — Submission: close the setters
**File:** `src/Services/Submission/Submission.Domain/Behaviors/Journal.cs:15`

**Context:** Step 1's grounding claims "Sanctioned mutation paths stay as-is: `Behaviors/Article.cs SetStage` and the asset file-creation behavior — both partial-class members of the same types, unaffected by the accessibility change," and the plan-wide context note claims "the only `.Stage =` assignment outside domain internals repo-wide is ArticleHub's read model."

That claim is dot-anchored and misses a bare (non-dot-prefixed) object-initializer assignment. `Journal.CreateArticle` (`Submission.Domain/Entities/Journal.cs`, behavior in `Behaviors/Journal.cs:7-24`) constructs the Article like this:

```csharp
var article = new Article
{
    Title = title,
    Type = Type,
    Scope = scope,
    Journal = this,
    Stage = ArticleStage.Created,   // Behaviors/Journal.cs:15
    CreatedById = action.CreatedById,
    CreatedOn = action.CreatedOn
};
```

`Journal` and `Article` are different classes (both happen to live in the `Submission.Domain.Entities` namespace, but C# `private set` is scoped to the declaring class, not the namespace or assembly). Once `Article.Stage` becomes `private set` per Step 1, this object-initializer line fails to compile — CS0272 ("cannot access private set"). I re-ran the grep repo-wide with a non-dot-anchored pattern (`\bStage\s*=\s*ArticleStage\.`) and this is the only additional hit beyond `Behaviors/Article.cs`'s own `SetStage`. Sole caller of `Journal.CreateArticle` is `CreateArticleCommandHandler.cs:15`.

Comparable code elsewhere in scope is fine: Production's `Article.FromReview` factory (Step 3's exemplar) does **not** set `Stage` in its initializer — it defaults to `None` and calls `SetStage` afterward, which is exactly why Step 3 has no equivalent problem. Submission's `Journal.CreateArticle` is the one site that assigns the initial stage inline instead of going through the guarded mutator.

**Question:** How should `Journal.CreateArticle` set the article's initial `Created` stage once `Article.Stage` is `private set`? This blocks Step 1's own acceptance criterion ("`dotnet build src/Articles.sln` exit 0").

**Recommendation:** Two options, ranked:
- **(A, smaller diff, no behavior change)** Add an `internal` initialization seam on `Article` itself — e.g. `internal void InitializeStage(ArticleStage stage) => Stage = stage;` as a member of `Article`'s own partial class (so `private set` stays satisfied), called from `Journal.CreateArticle` as `article.InitializeStage(ArticleStage.Created)` after construction instead of in the initializer. No new domain event, no new constructor parameter threaded through `CreateArticleCommandHandler`, stays inside the plan's declared "accessibility tightenings only... no new types" scope (this is a new *method*, not a new type).
- **(B, matches the Production.FromReview precedent, but changes behavior)** Don't set `Stage` in the initializer (defaults to `None`), then call `article.SetStage(ArticleStage.Created, action, stateMachineFactory)` right after construction — the seed data already has a `None → Created` row for the `CreateArticle` action type (per the `add-state-machine` skill's own lifted example, which is this exact service). This is more idiomatically consistent with the codebase's established "materialize then transition" pattern but (1) requires threading `ArticleStateMachineFactory` into `Journal.CreateArticle`'s signature and its one caller, and (2) newly raises an `ArticleStageChanged` domain event on article creation, which ArticleTimeline's domain-event handlers don't fire on today — a behavior change beyond pure accessibility tightening.

**Confidence:** medium — (A) is safer and smaller for an ad-hoc scope-bounded pass, but (B) is the pattern the codebase already uses elsewhere (Production) and the one a future reader would expect for consistency. Real trade-off; wants your call.

### Answer
**Architect Phase-1 resolution (recommendation — Status stays Open until the Phase-2 plan patch; no owner flag on this one).**

**Take Option A, but in its exemplar-consistent form: a same-class static factory, not a bespoke `InitializeStage` mutator.** Reject Option B.

Why a factory rather than `InitializeStage`: both "correct" siblings initialize `Stage` inside a **static factory that is a member of `Article`'s own partial class** — Review's `Article.FromSubmission` (`Review.Domain/Articles/Behaviors/Article.cs:154-166`) sets `Stage = articleDto.Stage` directly in the initializer (compiles because it is *inside* Article), and Production's `Article.FromReview` (`Production.Domain/Articles/Behaviors/Article.cs:12-33`) defaults to `None` then transitions. `Journal.CreateArticle` is the lone site that assigns the initial stage from a *sibling* class. The fix that converges Submission onto the pattern the whole pass exists to establish is to move construction into `Article`'s own class:

```csharp
// Submission.Domain/Entities/Behaviors/Article.cs (Article partial class) — new static factory
public static Article Create(string title, ArticleType type, string scope, Journal journal, IArticleAction action)
{
    var article = new Article
    {
        Title = title, Type = type, Scope = scope, Journal = journal,
        Stage = ArticleStage.Created,          // now compiles — inside Article's own class
        CreatedById = action.CreatedById, CreatedOn = action.CreatedOn
    };
    article.AddDomainEvent(new ArticleCreated(article, action));
    return article;
}
// Journal.CreateArticle body becomes: var article = Article.Create(title, Type, scope, this, action); _articles.Add(article); return article;
```

This is **behavior-identical** (same properties, same single `ArticleCreated` event, same `_articles.Add`) and adds **no new type** — a static method on an existing partial class. It also honors CLAUDE.md's "No inventing patterns — use what exists": the static-factory idiom is pervasive here (`Journal.Create`, `Asset.Create`, `Asset.CreateFromReview`, `Article.FromReview`, `Article.FromSubmission`); an `InitializeStage` setter-seam exists nowhere and would be a bespoke invention. The developer's `InitializeStage` is an acceptable minimal-diff fallback if appetite demands the very smallest change, but the factory is my recommendation.

**Reject Option B (route creation through `SetStage`).** It is a scope expansion beyond this accessibility-only pass and it contradicts the plan's own **ratified Decisions #3** ("initialization is not a transition; ... `SetState` stays transitions-only"). Concretely B would (1) require a `None → Created` seed transition row — the exact "force-add an init transition row" alternative Decisions #3 rejected; (2) newly raise an `ArticleStageChanged` domain event on creation (a behavior change ArticleTimeline handlers don't see today); and (3) thread `ArticleStateMachineFactory` through `Journal.CreateArticle` + its caller and switch the action from `IArticleAction` to `IArticleAction<ArticleActionType>`. All three exceed "accessibility tightenings only." If the owner ever wants creation to fire `ArticleStageChanged` for timeline consistency, that is a separate opt-in feature, not this pass.

Confidence: **high** — the philosophical question (init vs transition) is already settled by the plan's ratified Decisions #3, and the factory shape is the in-repo precedent, so this is the architect's call within ratified scope, not a toss-up.

**Resolved into plan (2026-07-11, architect):** Step 1.2 (`Article.Create` factory), Decisions #4, Context note corrected. Answered by: architect (within ratified scope — no owner call required).

---

## Q2: Step 4's guard relocation needs a Domain-layer type that doesn't exist yet for Asset — `AssetStateMachineFactory` lives in Application, and `Production.Domain` has no reference to `Production.Application`
**From:** developer
**To:** architect
**Status:** Resolved
**Step:** Step 4 — Production: move the Asset transition guard into the domain
**File:** `src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs:61` (target), `src/Services/Production/Production.Application/StateMachines/AssetStateMachine.cs:10` (current delegate location), `src/Services/Production/Production.Domain/StateMachines/IArticleStateMachine.cs` (the Article precedent)

**Context:** Step 4 says to relocate `AssetBaseEndpoint.CheckAndThrowStateTransition`'s body into `Asset.SetState` (a Domain-layer method), factory-keyed: `_stateMachineFactory(asset.State).CanFire(asset.Article.Stage, asset.Type, actionType)` → adapted to `this.`/`action.ActionType`. The step's disposition is "guard-in-behavior phase only (machine + table already exist)," i.e. it assumes the `add-state-machine` skill's Phase 1-3 are already done for Asset and only Phase 4 (the guard call in the aggregate) remains.

I invoked `nexus-dotnet:add-state-machine` to confirm the pattern before implementing. Its Phase 2 is explicit: **"The interface, the factory delegate, and the guard extension live together in the Domain layer."** That's exactly how Article's machine is wired here: `IArticleStateMachine` + `public delegate IArticleStateMachine ArticleStateMachineFactory(ArticleStage articleStage)` are both defined in `Production.Domain/StateMachines/IArticleStateMachine.cs` (Domain layer), and `Production.Application/StateMachines/ArticleStateMachine.cs` implements that Domain interface — which is exactly why `Behaviors/Article.cs:52 AssignTypesetter(..., ArticleStateMachineFactory stateMachineFactory, ...)` (a Domain method) can take the factory as a parameter and compile.

Asset's machine is **not** wired the same way. `public delegate AssetStateMachine AssetStateMachineFactory(AssetState assetState);` is declared directly in `Production.Application/StateMachines/AssetStateMachine.cs:10` — **Application layer**, with no Domain-layer interface at all (`IAssetStateMachine` doesn't exist anywhere in the repo — confirmed via repo-wide grep). I also checked `Production.Domain.csproj`: its `ProjectReference`s are BuildingBlocks + `FileStorage.Contracts` only — **no reference to `Production.Application`**. So `Asset.SetState` (Domain) cannot accept an `AssetStateMachineFactory` parameter as currently typed; the type isn't visible from Domain, and even if it were, Domain depending on Application inverts the layering CLAUDE.md's Clean Architecture guardrail requires.

In short: Step 4's premise ("machine + table already exist," only the guard-in-behavior phase remains) holds for the transition table and the Application-layer machine class, but **not** for the Domain-layer interface/delegate seam — that piece of Phase 2 was done for Article and never done for Asset. This is a real gap, not a style issue: the guard cannot physically move into `Asset.SetState` without it.

**Question:** Should I do the missing Phase 2 work for Asset — add `IAssetStateMachine` + `AssetStateMachineFactory` to `Production.Domain/StateMachines/` (new file, mirroring `IArticleStateMachine.cs`), make `AssetStateMachine` (Application) implement it, and repoint the DI registration + the 5 endpoint call sites' `using` from `Production.Application.StateMachines` to `Production.Domain.StateMachines` for the factory type — before doing Step 4's guard relocation? This is a **new type** in Domain, which the plan's "Domain Model Changes" section says is out of scope ("Accessibility tightenings only... no new types") — I read that line as scoped to the entity/value-object model (Article/Asset/ReviewInvitation), not this state-machine seam, but flagging since it's a literal conflict with the plan's own words.

**Recommendation:** Do the Phase 2 work (mirror the Article pattern exactly). It's the skill-mandated shape, it already has a working precedent in the same file family (`IArticleStateMachine.cs` sits one folder over), and there's no smaller alternative that keeps the guard inside the domain: the guard cannot be "pre-computed" in the endpoint and passed down as a bool without recreating the exact bypass-route problem td prod-5 exists to close.
**Confidence:** high — clear basis (the skill's own binding rule + the Article precedent in this same repo); the only friction is the plan's "no new types" wording, which I believe under-scoped this seam rather than deliberately excluding it, but that's your call to confirm.

### Answer
**Architect Phase-1 resolution (recommendation — Status stays Open: carries one owner-visible flag, see below).**

**Endorsed: do the missing Phase 2 for Asset, mirroring the Article seam exactly.** There is no feasible alternative that keeps the guard authoritative in the domain — I verified `Production.Domain.csproj` references BuildingBlocks + `FileStorage.Contracts` only (no Application ref), so `Asset.SetState` (Domain) cannot see an Application-layer factory type, and pre-computing the guard result in the endpoint and passing a `bool` down would recreate the exact bypass prod-5 exists to close. Concrete change set:
- **New** `src/Services/Production/Production.Domain/StateMachines/IAssetStateMachine.cs` — `interface IAssetStateMachine { bool CanFire(ArticleStage, AssetType, AssetActionType); }` + `public delegate IAssetStateMachine AssetStateMachineFactory(AssetState assetState);` (mirrors `IArticleStateMachine.cs` one folder over). All three param types are Domain-visible (`ArticleStage` from Abstractions; `AssetType`/`AssetActionType`/`AssetState` from `Production.Domain.Assets.Enums`), so this compiles in Domain.
- `Production.Application/StateMachines/AssetStateMachine.cs` — `class AssetStateMachine : IAssetStateMachine` (add the interface; it already has the matching `CanFire`), and **delete** the delegate declaration at `:10` (it moves to Domain).
- `Asset.SetState` gains an `AssetStateMachineFactory` parameter + the guard, threaded through its internal callers `CancelRequest` (`Asset.cs:69`) and `CreateAndAddFile` (`:77`, only the `action != null` branch — the `action == null` materialization branch stays a direct `State = Uploaded`, consistent with Decisions #3). Throw `DomainException` (not `BadRequestException`) — the plan's wire note already covers the 400-preserved/message-changed effect.

**Correction to the recommendation's blast-radius — it is NOT "5 endpoint call sites."** The delegate `AssetStateMachineFactory` is injected across the whole Asset-endpoint family. Files whose `using Production.Application.StateMachines` must swap to `Production.Domain.StateMachines` (grep-verified, plan-time): `AssetBaseEndpoint.cs`, `RequestAssetsEndpointBase.cs`, `UploadFileEndpoint.cs`, `ApproveDraftAssetEndpoint.cs`, `UploadSupplementaryFileEndpoint.cs`, `RequestFinalAssetsEndpoint.cs`, `RequestSupplementaryAssetsEndpoint.cs`, `CancelRequestFinalAssetsEndpoint.cs`, `UploadFinalFileEndpoint.cs`, `UploadDraftFileEndpoint.cs` = **10 API files** (none reference the concrete `AssetStateMachine`, so it is a clean swap, not an add). The DI file needs **no** using change — `Production.Application/DependencyInjection.cs:5-6` already imports both namespaces and already registers `ArticleStateMachineFactory` (Domain) the identical way. Net: 1 new Domain file + 1 Application edit + 10 API using-swaps + the `Asset.SetState`/caller threading. Phase 2 will update Step 4's caller list and acceptance greps to this reality.

**OWNER-VISIBLE FLAG (routed to team lead):** this literally conflicts with the plan's "Domain Model Changes: ... no new types" line. My read is that "no new types" was scoped to the domain *model* (aggregates / entities / value objects), and the state-machine seam is the sanctioned mechanism for the already-ratified prod-5 — the Article side proves it (`IArticleStateMachine` lives in Domain for exactly this reason). prod-5 ("guard in the domain, not the API layer") is **infeasible without** at least this one Domain interface, so this is not a real fork — but because it is a literal conflict with owner-ratified plan text, the owner should confirm the reading (proceed) rather than have me silently override it. Phase 2 will amend the "no new types" wording to carve out the seam explicitly once confirmed.

Confidence: **high** on the technical resolution; the only open item is owner confirmation of the scope-wording reading.

**Resolved into plan (2026-07-11):** owner confirmed "proceed with the seam" (relayed via team lead) — the "no new types" line is scoped to the domain model; the state-machine seam is the sanctioned prod-5 mechanism. Patched: Step 4.1 (create `IAssetStateMachine` + relocate delegate + 10-file using-swap), Skill-Mapping/Domain-Model-Changes carve-out, Decisions #5. Answered by: owner (via team lead) on the scope-wording; architect on the technical shape.
