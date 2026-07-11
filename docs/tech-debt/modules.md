# Tech-Debt Registry — modules

**Species:** tech-debt triage registry (`mine-verify-repo`) — sibling to `docs/reference-model.md`. Rows never deleted; dispositions flip; append-only changelog.
**Scope:** `src/Modules/**`. **Note:** the modules areas ranked cold (see `metrics.md`) and were **not** given a full miner pass this run; this file holds the one verified lead routed from the reference-model run reports. A future run may scan the areas properly.
**Provenance:** td-run-1, commit `96dde60`, skeptic-verified 2026-07-11.
**Triage:** architect-adjudicated 2026-07-11 (advisory); **owner ratification pending**.

| Id | Lens | Sev | Finding (symbol + condition) | Evidence (re-executed 2026-07-11) | Status | Disposition |
|----|------|-----|------------------------------|-----------------------------------|--------|-------------|
| mod-1 | code-quality | low | `SendGridEmailService` logging is malformed twice: `LogError($"unknow error")` (pointless `$`, typo) and `LogError($"the mail service returned status code response.StatusCode")` — the variable sits inside the string literal without braces, so it never interpolates and the literal text is logged *(provenance: reference-model run-2 lead; project also has zero consumers — glb-2)* | `SendGridEmailService.cs:20,:25` quoted | verified | accepted |

## Changelog

- 2026-07-11 — td-run-1 (commit `96dde60`): initial registry, 1 row (routed lead, verified). Areas not fully mined this run (cold rank).
