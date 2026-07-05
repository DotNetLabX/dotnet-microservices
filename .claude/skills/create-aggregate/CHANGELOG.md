# Changelog — create-aggregate

## [Unreleased]

- Action-parameter rule corrected to authoritative form: `action` is ALWAYS LAST (entity/domain args →
  domain services/delegates → `action`), citing the compliant Review/Production signatures; Submission's
  four `Article` behaviors flagged as known drift. Fixed in SKILL.md + `templates/ef-behavior.cs`.
- Phase C merge (verdict: this skill canonical, port scenario coverage):
  - Added a **Scope fence** pointing to `create-feature-slice`, `domain-event-wiring`,
    `add-integration-event`, and the `domain-patterns` plugin skill for design-depth questions.
  - Added a **Folder layout** note documenting both layouts — flat `Entities/`+`Behaviors/` (Submission,
    Auth) and aggregate-grouped `{Aggregate}s/Behaviors/` (Review, Production) — with the
    namespace-follows-folder caveat; stopped pinning `{Svc}.Domain.Entities` as the only form.
  - Added the **generic vs non-generic `IArticleAction`** note (Submission generic; Review/Production not).
  - Added a **Repositories** note: subclass `Repository<T>` + override `Query()`; assembly-scan
    `AddDerivedTypesOf` (Submission/Review) vs manual `AddScoped` (Production/Journals/ArticleHub).
  - Added the **Identity `User` special case** — implement `IAggregateRoot` manually when the type already
    inherits a framework base, citing `Auth.Domain/Users/User.cs`.
  - Added **Promote an existing entity to aggregate root in place** — type-change-not-scaffold, with the
    NOT-NULL audit-column migration consequence flagged at plan time.
