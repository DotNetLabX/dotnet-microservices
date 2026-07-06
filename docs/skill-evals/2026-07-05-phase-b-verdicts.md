# Phase B comparative-eval verdicts — 2026-07-05 (recovered record)

The per-pairing verdicts of the Phase B comparative evaluation (project-local clean-room skills
"A" vs shipped nexus-dotnet 1.3.1 skills "B", 10 pairings, ground truth = live repo source +
`docs/reference-model.md`). The aggregate outcome ("10/10 A wins, 8 merge / 2 replace, keep-best
merged") was recorded in commit e12cf81 and the session continuation prompt, but the per-pairing
breakdown was never written to a repo artifact. Recovered 2026-07-06 verbatim from the eval
subagent transcripts of session `5509c4c4-3636-4638-b450-56f7fc0f89e2` (subagents
`agent-aeval-*`, run 2026-07-05).

## Verdict table

| # | Local skill (A) | Shipped skill (B) | Verdict | Upstream treatment |
|---|---|---|---|---|
| 1 | add-integration-event | add-integration-event | MERGE (base = A) | patch |
| 2 | error-handling | error-handling | MERGE (A spine, fold B's 4 complements) | patch |
| 3 | create-aggregate | create-aggregate (+ domain-patterns) | MERGE (A canonical, port B's scenario coverage) | patch |
| 4 | create-feature-slice | create-feature (+ cqrs-patterns) | MERGE (onto A; retire B locally) | patch |
| 5 | create-service | create-service | MERGE (onto A's body, graft B's infrastructure step) | patch |
| 6 | domain-event-wiring | create-domain-event-handler | **keep-A — A supersedes B** | **replace** |
| 7 | authorization-security | authorization-patterns | **keep-A — supersede B** | **replace** |
| 8 | grpc-communication | create-grpc-contract | MERGE (into A; B is a pure scaffolder shadowed by A) | patch |
| 9 | service-infra-conventions | service-registration (+ central-package-management) | MERGE (merge plan; B1/B2 defects routed) | patch |
| 10 | persistence-patterns | persistence-patterns | MERGE (keep A, port B's six portable EF caveats) | patch |

Totals: **8 MERGE, 2 keep-A/supersede** — matching the "8 merge, 2 replace" aggregate.

## The two supersede verdicts, verbatim core

- **authorization** (A = `authorization-security`, B = `authorization-patterns`): "keep-A
  (supersede B for this repo) … B ships fictional identifiers (`UserRoleType.Editor/Admin`,
  `Role.Admin`), a named-policy registration the repo doesn't use (and which drops
  `RequireRole`), and mischaracterizes the single-layer case … **B contributes no correct fact A
  lacks.** Why not MERGE: nothing correct in B is worth grafting."
- **event-wiring** (A = `domain-event-wiring`, B = `create-domain-event-handler`): "keep-A — A
  supersedes B for this repo." (B's variant selection keys off the endpoint framework — refuted
  by Production: FastEndpoints endpoints, MediatR event bus.)

## Operative consequence for the upstream (plugin-repo) pass

- The 8 MERGE-verdict shipped skills have salvageable structure → **patch** them per the routed
  defect lists in `docs/plugin-feedback/nexus-dotnet-1.3.1-2026-07-05.md`.
- The 2 supersede-verdict shipped skills were judged beyond grafting → **replace** their content
  upstream, using the project-local winners (`authorization-security`, `domain-event-wiring`) as
  the base, adapted to the plugin's register (genericized identifiers where the shipped estate is
  generic).
- The routed feedback file marks all 13 entries `Action: update` — for the 2 supersede skills,
  "update" is satisfied by the replace treatment; the listed defects double as the acceptance
  checklist for whatever replaces them.
