# Plugin feedback — nexus 1.23.1 (core), 2026-07-06

Source: spec-level evaluation of the shipped utility skills against externally verified authoring canon (`docs/kb/research/dotnet-agent-skill-estates.md` — Anthropic skill-authoring guidance + strongest public .NET estates, 24 claims adversarially verified). Full findings doc with checked-clean coverage: `docs/skill-evals/2026-07-06-nexus-utility-skills.md`. All entries are consolidating fixes; no Critical/High.

## Entry 1 — improve-skills / scripts/skill-lint.mjs: extend E6 to scripts/ and assets/
- **Suggested target:** `skills/improve-skills/scripts/skill-lint.mjs`, E6 reference-existence check (+ header comment)
- **Action:** widen the citation regex from `(?:references|workflows)/` to `(?:references|workflows|scripts|assets)/` so a SKILL.md citing a nonexistent `scripts/*.mjs` or `assets/*` fails lint.
- **Evidence:** improve-skills' own SKILL.md cites `scripts/skill-lint.mjs` — live pattern, unchecked today; AP5 (fictional infrastructure) names exactly this defect class; Anthropic canon names `scripts/` and `assets/` as first-class skill anatomy alongside `references/`.
- **Condensed lesson:** the lint's fictional-path net must cover every folder the anatomy sanctions, or the sanctioned folders become the blind spot.

## Entry 2 — improve-skills / scripts/skill-lint.mjs: add W3 body-size warning
- **Suggested target:** `skills/improve-skills/scripts/skill-lint.mjs`, new W3 warning
- **Action:** WARN (never ERROR) when the SKILL.md body after frontmatter exceeds 500 lines: "consider a progressive-disclosure split (references/), or record why this skill justifies its length."
- **Evidence:** Anthropic canon: keep SKILL.md body under 500 lines, split when approaching; the lint has description-length warnings but no body signal; rubric 2.3 currently carries this unaided.
- **Condensed lesson:** the cheapest canon check is a warning at write time; keeping it WARN preserves a consuming project's right to deviate deliberately (opinionated estates justify length instead of being blocked).

## Entry 3 — evaluate-skill / references/rubric.md: Layer 0 item 6 has no executor
- **Suggested target:** `skills/evaluate-skill/references/rubric.md`, Layer 0 list
- **Action:** delete item 6 ("skills index listed / row matches") from Layer 0 — Layer 4.3 already owns index/registration sync as a judgment check.
- **Evidence:** Layer 0 is titled "scripted: skill-lint.mjs" but the script contains no index check — AP1 (a check claimed mechanical with no executor) inside the estate's own quality doc, plus AP3 duplication with Layer 4.3. Implementing it in the lint was considered and rejected: index location/format is project-specific.
- **Condensed lesson:** a rubric layer labeled "scripted" may only list what the script actually executes.

## Entry 4 — improve-skills / references/skill-recipe.md + SKILL.md: sanction scripts/ in the authoring path
- **Suggested target:** `skills/improve-skills/references/skill-recipe.md` §2 element menu; `skills/improve-skills/SKILL.md` scaffold step 3
- **Action:** add a menu element "Bundled executable `scripts/` — the home of P1 deterministic post-condition checks; runnable without being read into context"; widen scaffold step 3 to "add `workflows/`, `references/`, or `scripts/` …".
- **Evidence:** P1 is the estate's top proven pattern and improve-skills itself ships a script, yet the recipe offers no home for one — an author following the menu has nowhere sanctioned to put a P1 check. Canon (scripts/references/assets anatomy; morphir's executable pre-commit gates as the strongest public exemplar) confirms executable checks are the stronger verification model.
- **Condensed lesson:** the authoring menu must offer the mechanism the evidence layer says wins, or authors write prose rules where scripts belong.

## Entry 5 — improve-skills / references/skill-recipe.md (+ rubric pointer): add the degrees-of-freedom axis
- **Suggested target:** `skills/improve-skills/references/skill-recipe.md` §2 (owner); `skills/evaluate-skill/references/rubric.md` Layer 2.2 (one pointing clause)
- **Action:** one recipe bullet: per-step specificity matched to fragility — high freedom (heuristic prose), medium (parameterized template/pseudocode), low ("run exactly this; do not modify"). Rubric 2.2 gains "is each fragile step pinned to low freedom?" referencing the recipe.
- **Evidence:** Anthropic's documented degrees-of-freedom model; the estate applies it implicitly (P1 = low freedom) but never names the decision, and the heavy/light archetype is a different axis (skill weight, not per-step specificity).
- **Condensed lesson:** naming the specificity decision per step is cheap and prevents fragile steps shipping as heuristics.

## Entry 6 — improve-skills / scripts/skill-lint.mjs: add W4 nested-reference warning
- **Suggested target:** `skills/improve-skills/scripts/skill-lint.mjs`, new W4 warning
- **Action:** WARN when a cited `references/*.md` itself cites further `references/` paths — "nested reference chain; canon wants references one level deep from SKILL.md."
- **Evidence:** Anthropic canon: keep references one level deep — nested chains cause partial/incomplete reads (documented head-100 behavior).
- **Condensed lesson:** reference depth is mechanically checkable; a warning catches the chain before an agent reads half a file.
