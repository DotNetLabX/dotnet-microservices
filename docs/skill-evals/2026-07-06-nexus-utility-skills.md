# Skill Evaluation — nexus utility skills (improve-skills, evaluate-skill) vs research canon

**Evaluator:** architect (session model), 2026-07-06
**Scope:** shipped nexus 1.23.1 cache, read in full: `improve-skills/SKILL.md`, `improve-skills/scripts/skill-lint.mjs`, `improve-skills/references/proven-patterns.md`, `improve-skills/references/skill-recipe.md`, `evaluate-skill/SKILL.md`, `evaluate-skill/references/rubric.md`.
**Benchmark:** the externally verified authoring canon in `docs/kb/research/dotnet-agent-skill-estates.md` (24 claims, 3-vote adversarial verification) — Anthropic progressive-disclosure guidance, scripts/references/assets anatomy, degrees-of-freedom model, executable-gates-over-attestation.
**Run evidence:** none consulted — this is a **spec-level** evaluation (text vs canon diff), per evaluate-skill step 2's honesty rule. Run-fitness of these skills was separately exercised during the 2026-07-05 skill build (13 lint runs, several inline rubric passes) with no failures attributable to the gaps below.
**Channel:** shipped plugin skills → findings route to `docs/plugin-feedback/nexus-1.23.1-2026-07-06.md` (ADR-1); application happens in the plugin dev repo.

## F1: lint's dangling-reference check misses `scripts/` and `assets/`
**Severity:** Medium
**Layer:** 0 (skill-lint.mjs E6)
**Claim vs reality:** E6 verifies cited files exist, but its regex is scoped to `references/` and `workflows/` only. A SKILL.md citing `scripts/foo.mjs` or `assets/template.docx` that doesn't exist passes lint — the AP5 fictional-infrastructure class the estate itself warns about. improve-skills' own SKILL.md cites `scripts/skill-lint.mjs`, so the pattern is live and unchecked. Canon names scripts/ and assets/ as first-class skill anatomy.
**Fix:** extend the E6 pattern to `(?:references|workflows|scripts|assets)/` — one-token change, update the header comment.

## F2: no body-size signal in the lint
**Severity:** Medium
**Layer:** 0 (skill-lint.mjs) + rubric Layer 2.3
**Claim vs reality:** canon prescribes a <500-line SKILL.md body with split-when-approaching; the lint checks description length (W1/W2) but says nothing about body size, and rubric 2.3 ("right weight") relies on unaided judgment. An oversized body is exactly the defect a warning catches cheapest.
**Fix:** add **W3 (WARN, not ERROR)**: SKILL.md body (after frontmatter) over 500 lines → "consider a progressive-disclosure split (references/), or record why this skill justifies its length." WARN preserves the consuming-project stance that the limit is a flag to justify, not a kill.

## F3: rubric Layer 0 item 6 has no executor
**Severity:** Medium
**Layer:** rubric (Layer 0 vs Layer 4 drift)
**Claim vs reality:** rubric Layer 0 is titled "Mechanical (scripted: improve-skills' skill-lint.mjs)" and lists item 6 "if the project keeps a skills index, the skill is listed and the row matches" — but skill-lint.mjs has no index check. A check claimed as scripted with no script is AP1 (dead-letter enforcement) inside the estate's own quality doc, and it duplicates Layer 4.3 (index/frontmatter sync) — AP3.
**Fix:** delete item 6 from Layer 0; Layer 4.3 already owns index sync as a judgment check. (Alternative — implementing the check in the lint — rejected: index location/format is project-specific, not mechanically derivable.)

## F4: authoring recipe never offers `scripts/` (or `assets/`) as skill anatomy
**Severity:** Medium
**Layer:** skill-recipe.md §2 + improve-skills scaffold step 3
**Claim vs reality:** the element menu offers references/, workflows/, templates, state files — but no "bundled executable script" element, even though P1 (deterministic post-condition scripts) is the estate's top proven pattern and improve-skills itself ships one. Scaffold step 3 says "add `workflows/` or `references/` only if variant-aware or template-bearing" — an author following it has no sanctioned home for a P1 script. Canon: scripts/ is first-class and runnable **without loading into context** (context-cheap enforcement).
**Fix:** add a menu bullet — "**Bundled executable `scripts/`** — the home of P1 deterministic post-condition checks; runnable without being read into context; lint gains coverage via F1" — and widen scaffold step 3 to `workflows/`, `references/`, or `scripts/`.

## F5: degrees-of-freedom axis absent from the authoring guidance
**Severity:** Low
**Layer:** skill-recipe.md §2 (owner), rubric Layer 2 (reference)
**Claim vs reality:** canon's documented craft mechanism — match each instruction's specificity to its fragility: high freedom (heuristic prose), medium (parameterized template/pseudocode), low ("run exactly this, don't modify") — appears nowhere. The estate applies it implicitly (P1 scripts = low freedom; heavy/light archetype is a related but different axis: skill weight, not per-step specificity).
**Fix:** one bullet in the recipe's element menu naming the three tiers and the matching rule; rubric Layer 2.2 gains a pointing clause ("is each fragile step pinned to low freedom?") — one owner, one reference (AP3).

## F6: no nesting-depth signal for references
**Severity:** Low
**Layer:** 0 (skill-lint.mjs)
**Claim vs reality:** canon requires references one level deep from SKILL.md — nested reference chains cause partial/incomplete reads. Nothing checks whether a `references/*.md` file itself points at further `references/` files.
**Fix:** add **W4 (WARN)**: a cited `references/*.md` whose content itself matches the E6 reference pattern → "nested reference chain — canon wants references one level deep from SKILL.md."

## Checked clean (coverage record)

- **P1 = the executable-gate canon.** The strongest public exemplar (morphir's pre-commit scripts) validates proven-patterns P1 outright; self-attestation checklists (awesome-copilot) are the weaker model the estate already avoids. No change.
- **Heavy/light archetype ≈ progressive disclosure in practice** (thin orchestrator + references/); recipe §2 explicitly reasons about it.
- **Scope fence** is rubric Layer 1.5 and a recipe menu element — matches public best practice (morphir Do/Avoid lists).
- **Description budget** W1/W2 aligns with canon's L1 metadata economy; frontmatter cheat-sheet carries a dated live-docs verification (2026-07-04) — P9 compliant.
- **ADR-1 channel split** (cache read-only, feedback file, dev-repo carve-out) — intact and consistently stated in both skills.
- **AP1–AP7** — no canon contradiction found; the research corroborates rather than challenges the anti-pattern list.

## Verdict

**fix-then-accept.** Six findings, all small consolidating moves (two regex/warning additions, one deletion, two menu bullets, one pointing clause); no Critical/High — nothing in the utilities produced wrong output during the 2026-07-05 build round. Apply in the plugin dev repo via the release-plugin flow, then proceed to the nexus-dotnet skill upstream with the upgraded gates.
