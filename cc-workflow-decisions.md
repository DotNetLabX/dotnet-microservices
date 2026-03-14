# Claude Code Workflow Decisions

> Captured from thread discussion — March 12, 2026

## File Structure

### Root CLAUDE.md (minimal, auto-loads every message in both terminals)
- Project description (2-3 lines)
- Repo structure
- BuildingBlocks overview (needed by both agents)
- Core principles (DDD, vertical slice, CQRS — names only)
- Compaction rules
- "Don't re-read files" rule: "After reading a file, trust your context. Do not re-read files unless you have been told they changed or you explicitly modified them. After editing a file, do not read it back to verify — trust your edit was applied correctly."

### Convention Files (split from original claude.md, 3-4 files max)
- Stored in `conventions/` folder
- Exact split TBD — using CC to break up the existing claude.md
- Each agent file imports only the convention files it needs via `@` references

### Agent Role Files (loaded manually at session start)
- `agents/Architect.md` — role definition + `@` references to architecture-related convention files only
- `agents/Developer.md` — role definition + `@` references to architecture + coding standards convention files
- These are NOT in `.claude/agents/` — they are regular files, not auto-discovered subagents
- `@` references inside agent files work behaviorally via Read tool (not native import), confirmed acceptable — loads once at session start

### Service-Level CLAUDE.md (auto-loads when CC works in that service folder)
```
Services/Submission/CLAUDE.md   ← DB choice, domain model, endpoints, service-specific rules
Services/Auth/CLAUDE.md         ← JWT config, user model, auth-specific patterns
Services/Production/CLAUDE.md   ← Production-specific context
```

### Subagents (Section 8 only)
```
.claude/agents/                 ← auto-discovered by CC, separate from agents/ folder
```

### .claudeignore
```
bin/
obj/
Migrations/
.vs/
*.lock
docker-compose.override.yml
```

## Two-Terminal Setup

### Terminal 1 — Architect
- Command: `claude --model opus`
- First message: "read agents/Architect.md"
- Purpose: discuss architecture, plan features, never write code
- Windows Terminal profile: `CC Architect`

### Terminal 2 — Developer
- Command: `claude --model opusplan`
- First message: "read agents/Developer.md"
- Purpose: implement from plan, don't debate architecture
- `opusplan` = Opus for planning (Shift+Tab), Sonnet for implementation — automatic switching
- Windows Terminal profile: `CC Developer`

### Windows Terminal Profiles
- Starting directory: repo root (need access to BuildingBlocks, gRPC contracts, etc.)
- Command line: `powershell -NoExit -Command "claude --model opus"` (or opusplan)
- Don't change Default profile — keep it for normal terminal work

### Working in a Specific Service
- Start CC from repo root
- First prompt scopes it: "We're working on the Production service"
- Service-level CLAUDE.md auto-loads when CC reads/edits files in that folder
- Root CLAUDE.md + service CLAUDE.md both load (hierarchical)

## Session Management

- `/rename FeatureName-ServiceName` — immediately after starting a session
- `claude --resume` — interactive session picker (shows named sessions)
- `claude --resume session-name` — resume specific session
- `claude --continue` — resume most recent session
- One session per feature, can span 1-3 days
- Sessions are local files at `~/.claude/projects/` — don't sync across machines
- Git-committed plan files are the cross-machine continuity layer

## Cost Efficiency (in order of impact)

1. **Minimal CLAUDE.md + scoped service files** — saves tokens on every message
2. **`.claudeignore`** — prevents CC from reading junk files
3. **Specific prompts** — vague requests = expensive exploration
4. **`/clear` between features** — stops context bloat
5. **`opusplan` for Developer** — automatic Opus/Sonnet switching
6. **`/rename` + `--resume`** — session management across days
7. **Escape when CC goes wrong** — stops token burn immediately
8. **Plan mode (`Shift+Tab`) for complex tasks** — prevents expensive wrong-direction work

### Marginal (nice to know, not essential)
- `DISABLE_NON_ESSENTIAL_MODEL_CALLS=1` — disables flavor text, minimal savings
- Effort levels — medium is fine for both terminals
- `CLAUDE_CODE_SUBAGENT_MODEL=haiku` — Explore subagent already defaults to Haiku

## In-Session Commands

| Command | When to use |
|---|---|
| `/rename` | Immediately at session start |
| `/clear` | Between features — fresh context |
| `/compact` | Within a long feature, conversation getting heavy |
| `/compact Keep domain model decisions and current file changes` | Compact with preservation instructions |
| `/context` | Check what's consuming context window |
| `/model` + left/right arrows | Change effort level mid-session |
| `Shift+Tab` | Toggle plan mode (plan before executing) |
| `Escape` | Stop CC immediately when going wrong direction |

## External Tools

| Command | Purpose |
|---|---|
| `npx ccusage@latest session` | Per-session cost tracking |
| `npx ccusage@latest daily` | Daily cost report |
| `npx ccusage@latest blocks` | 5-hour billing window tracking |

Runs from any terminal, any directory. Not inside CC. Reads from `~/.claude/projects/`.

## Settings (in `~/.claude/settings.json`)

```json
{
  "env": {
    "CLAUDE_CODE_EFFORT_LEVEL": "medium",
    "DISABLE_NON_ESSENTIAL_MODEL_CALLS": "1"
  }
}
```

## Key Concepts Clarified

### Agents vs Subagents vs Agent Teams
- **Interactive sessions with role files** = what we call "agents" (not CC's official term) — two terminals, you interact directly, roles enforced via loaded agent files
- **Subagents** (`.claude/agents/`) = delegated workers spawned by main session, non-interactive, isolated context, return summary and disappear
- **Agent Teams** = multiple CC instances coordinating in parallel, each with own terminal pane (needs tmux/iTerm2), automated orchestration — Section 8 topic

### @import Behavior
- Official: only works in CLAUDE.md and files imported by CLAUDE.md (recursive, max 5 levels)
- Behavioral: `@` references in agent files work via Read tool — CC sees the reference and reads the file
- For our use case (loading once at session start): acceptable, no meaningful overhead

### Subscription Limits
- Max is NOT unlimited — has 5-hour rolling windows + weekly caps
- Opus burns through limits faster than Sonnet (~1.7x)
- Students will replicate everything — cost awareness is a design constraint, not just a topic
- Pro ($20/mo) will hit limits fast on a microservices project with two terminals
- Consider API via Goose as cheaper alternative for students

### Codex Portability
- Convention file content transfers directly to Codex
- Codex uses AGENTS.md, no `@import` support, no role files per session
- Hierarchical AGENTS.md per directory works same as CC's CLAUDE.md hierarchy
- Role separation = first prompt only, no file-based enforcement

### CC Re-reads Files
- Confirmed: CC re-reads same files repeatedly within a session — biggest token waste
- CLAUDE.md rule helps but not bulletproof (CC can ignore instructions as context fills up)
- `read-once` hook exists (60-90% reduction) — not using for now
- Track re-reads via JSONL at `~/.claude/projects/<project>/<session-id>.jsonl`

## Course Implications

- Sections 1-7: two terminals, manual orchestration with role files — "chatting with enforced roles"
- Section 8: introduce subagents and agent teams as automation layer
- Cost section: address subscription limits, show ccusage, compare subscription vs API pricing
- Convention portability section: same content, different wrapper (CLAUDE.md → AGENTS.md)
- All decisions in this file = teachable moments for the course
