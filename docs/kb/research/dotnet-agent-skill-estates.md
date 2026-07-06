# dotnet-agent-skill-estates

## What do the strongest public .NET/C# AI-agent skill/instruction sets do structurally that a phased, variant-aware repo-local skill estate might be missing?

**Verdict:** No public .NET estate uses task-phases or per-service variants — that design is structurally distinctive; the borrowable material is coverage dimensions (upgrade/migration playbooks, observability, executable pre-commit verification gates, multi-layer testing with coverage targets, build/release/packaging/perf) plus two under-used Anthropic levers (degrees-of-freedom matching, bundled executable `scripts/`).
**Evidence tier:** read-docs
**As-of:** 2026-07-06
**Validity scope:** the sampled repos' main branches as of July 2026 (Aaronontheweb/dotnet-skills, github/awesome-copilot, Aaronontheweb/dotnet-cursor-rules, finos/morphir-dotnet, microsoft/Generative-AI-for-beginners-dotnet, dotnet/skills) and Anthropic's current skill-authoring docs
**Status:** current
**Reconfirm trigger:** a major restructure/release of dotnet/skills or Aaronontheweb/dotnet-skills, or a revision of Anthropic's skill-authoring best-practices doc
**Corroboration:** 22 sources fetched across 6 search angles; 24 of 25 top claims survived 3-vote adversarial verification (Opus verifiers); the key structural claims are corroborated by 2+ independent primary sources

## Verdict

None of the strongest public .NET agent-instruction estates use phases or per-service variants — they scope by file-glob or directory precedence. The phased, variant-aware design is genuinely distinctive; what the public estates offer to borrow is coverage breadth (upgrades, observability, testing, build/release/perf) and executable verification gates, not structure.

## Finding

- The strongest public .NET estates split into two craft families: comprehensive reference-document skills/rules gated by file-type scope fences (Aaronontheweb/dotnet-skills — 30+ Claude Code skills + 5 agents; github/awesome-copilot instructions; Aaronontheweb/dotnet-cursor-rules) and single-file AGENTS.md operating manuals (finos/morphir-dotnet — mature; Microsoft's Generative-AI-for-beginners-dotnet — flat/educational) [1].
- The official dotnet/skills repo (released March 2026) is organized as ~15 installable plugins (dotnet, dotnet-data, dotnet-diag, dotnet-nuget, dotnet-upgrade, dotnet-maui, dotnet-test, dotnet-aspnetcore, dotnet-blazor, …), each holding multiple SKILL.md skills [8].
- No sampled estate uses task-phases or per-service variants; skills are pattern-catalog reference documents (10–40KB, "Pattern 1..N / Common Pitfalls") or flat monolithic AGENTS.md files [1][6].
- Scope fences in public estates are mechanical file/dir matches, not semantic task/variant activation: Copilot YAML `applyTo` globs, Cursor `.mdc` `globs` front-matter, morphir Do/Avoid-or-escalate lists, Codex nested-AGENTS.md nearest-wins precedence [3][4][5][7].
- Two verification-gate models exist publicly: awesome-copilot's self-attestation MANDATORY checklists ("I have verified…") and morphir's executable pre-commit commands (scripts/format-all, scripts/test-all, scripts/check-contracts, coverage ≥80% must-not-decrease) — the executable model is the closest public analog to verification greps and is mechanically stronger than self-attestation [3][5].
- Dedicated upgrade/version-migration playbooks ship as first-class instruction files: a general dotnet-upgrade specialist playbook and a targeted MAUI 9→10 migration with P0/P1/P2 breaking-change tiers and bulk-migration regex tooling [3].
- Observability discipline appears in the mature AGENTS.md (morphir: logs to stderr only via Serilog, stdout reserved for command output) and is entirely absent from the educational one — though the 2-1 verifier split notes morphir's rule is arguably CLI output hygiene rather than telemetry-grade observability [5][6].
- Cursor rules surface six build/release/ops/perf dimensions beyond feature code: NuGet package publishing, dotnet-tool authoring, CI/CD with code signing (GitHub Actions & Azure DevOps), .NET SDK/solution management, C# style+testing, and BenchmarkDotNet perf-regression testing [4].
- Mature testing coverage is multi-tool and multi-layer: morphir maps TUnit (unit), Verify (snapshot), Reqnroll BDD (acceptance), FsCheck (property-based) to explicit layers with ≥80% overall / >90% unit coverage targets [5].
- Anthropic's canonical guidance prescribes three-level progressive disclosure (metadata always loaded ~100 words; SKILL.md body <500 lines on trigger; bundled files on demand), references one level deep, mutually-exclusive paths split into separate files, and the scripts/references/assets directory anatomy [9][10][11].
- Anthropic's degrees-of-freedom model (high = text heuristics, medium = pseudocode/parameterized scripts, low = "run exactly this script") is the documented craft mechanism behind lifted templates + verification greps, and a lever most public reference-doc estates do not consciously vary [9].
- A negative sub-claim was REFUTED 0-3: do not assert that Aaronontheweb/dotnet-skills contains no verification/shell-grep commands [1].
- No sampled estate elaborates EF Core migrations as a first-class coverage dimension — it surfaced only as an absence, so there is no positive public exemplar to borrow from [no source found].

## Fix

- Treat the phased/variant structure as validated — no public estate matches it; do not restructure toward the pattern-catalog or monolithic-AGENTS.md models [1][6].
- Candidate coverage additions for the repo-local estate, in observed-value order: an upgrade/version-migration playbook skill, executable verification gates bundled as `scripts/` (beyond inline greps), a testing-strategy skill with explicit layer/tool/coverage mapping, an observability/logging-discipline skill, and build/release surfaces (Dockerfile/CI, NuGet if ever published, BenchmarkDotNet if perf matters) [3][4][5][9].
- Apply the two under-used Anthropic levers when touching skills next: state each phase's degree of freedom explicitly, and move runnable verification into bundled scripts where a grep is not enough [9][10].

## Alternatives

- Adopting a public estate wholesale (e.g. dotnet/skills or dotnet-skills as a plugin) — rejected: they are generic pattern catalogs; repo-exact skills beat them on fit, which the Phase B comparative evals already demonstrated locally [1][8].
- Switching to the comprehensive reference-document model (10–40KB per skill, dotnet-skills' deliberate choice) — rejected: Anthropic prescribes progressive disclosure with <500-line bodies, and whether the big-document model measurably outperforms is an open question [1][9].

## Caveat

The "scope fence analog" framing is interpretive: applyTo/globs and directory nesting are mechanical matches, not the semantic variant activation used locally. The observability finding rests on a 2-1 vote and a generous label. Most repo-specific findings rest on the repo itself as a single primary source (authoritative for "what it contains" but single-fetch, except dotnet-skills which was cross-checked via the GitHub contents API). All repos read at main as of ~July 2026; contents can change.

## Fallback

If a gap dimension needs a positive exemplar later, run a targeted follow-up dive on that dimension alone — especially executable verification patterns (architecture-fitness tests, contract-roundtrip gates) and EF-migration playbooks, neither of which produced a strong public exemplar in this sample.

## Sources

[1] https://github.com/Aaronontheweb/dotnet-skills
[2] https://codewithmukesh.com/resources/dotnet-claude-kit/
[3] https://github.com/github/awesome-copilot/tree/main/instructions
[4] https://github.com/Aaronontheweb/dotnet-cursor-rules
[5] https://github.com/finos/morphir-dotnet/blob/main/AGENTS.md
[6] https://github.com/microsoft/Generative-AI-for-beginners-dotnet/blob/main/AGENTS.md
[7] https://developers.openai.com/codex/guides/agents-md
[8] https://github.com/dotnet/skills
[9] https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices
[10] https://www.anthropic.com/engineering/equipping-agents-for-the-real-world-with-agent-skills
[11] https://github.com/anthropics/claude-code/blob/main/plugins/plugin-dev/skills/skill-development/SKILL.md

## Recommendation

Proceed with the upstream of the 13 local skills unchanged — the research validates their structure rather than challenging it. Add the five gap dimensions (upgrade playbook, executable verify scripts, testing strategy, observability, build/release) as backlog rows in docs/skill-backlog.md rather than blocking the upstream on them. Next probe if reopened: a targeted dive on executable verification patterns in agent estates.
