---
name: plan-feature
description: Creates an architecture plan for a new feature. Use when planning a feature, endpoint, or capability before implementation. Produces a plan file in plans/ directory.
disable-model-invocation: true
---

# Plan Feature

Plan a feature using the implementation plan format below. Save the plan to `plans/$ARGUMENTS.md`.

## Implementation plan format

### 1. Summary
1-2 lines: what's being built and why.

### 2. Affected services
Which services are impacted and why each one.

### 3. Domain model changes
New/modified aggregates, entities, value objects, events.

### 4. File list
Every file to create or modify, with full paths relative to `src/`.

### 5. Numbered implementation steps
Each step is one focused task the Developer can execute as a single prompt. Each step specifies:
- What to do (not how to code it)
- Which files are involved
- What pattern to follow (reference an existing file as example)
- Any dependencies on previous steps

### 6. Cross-service impacts
gRPC contract changes, new integration events, new consumers.

### 7. Migration notes
If DB changes are needed, which service and what command.

### 8. Open questions
Anything that needs human input before proceeding (don't guess).

## Before planning, verify

- Which service owns the feature?
- Is this a command or query?
- Does it require cross-service communication (gRPC or integration events)?
- Which DB is used by that service?
- What endpoint framework does the service use?

If any of these are unclear, ask before producing the plan.
