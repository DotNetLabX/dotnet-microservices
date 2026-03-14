# Guardrails & Workflow

## Modules vs Microservices

**Default to module** unless there's a strong reason for independent deployment/scaling/ownership.

Use a **Module** when:
- Reusable but doesn't need independent deployment
- Low/no cross-team ownership concerns
- Shares lifecycle with parent service
- Examples: EmailService, FileService, ArticleTimeline

Use a **Microservice** when:
- Needs independent scaling/deployment
- Different team ownership or data sovereignty
- Examples: Submission, Review, Production, Auth, Journals, ArticleHub

## Agent workflow

When implementing something, follow this routine:
1. Restate the change in 1-2 lines
2. Identify the bounded context/service impacted
3. List the files you will touch before editing
4. Implement minimal change
5. Ensure:
   - Build passes
   - Existing patterns are respected (FastEndpoints/Carter/MediatR/Validation/Mapster)
   - No new abstractions unless justified

## Questions to ask before assuming

If any of these are unclear, **ask first** (don't guess):
- Which service owns the feature?
- Is this a command or query?
- Does it require cross-service communication (gRPC) or integration events?
- Which DB is used by that service?
- Do we need idempotency/retries (messaging)?
- Any existing naming/location conventions for that service's slices?

## Guardrails (common mistakes to avoid)

- Don't introduce a generic service layer
- Don't add interfaces "just in case"
- Don't centralize feature code into shared buckets
- Don't bypass domain rules by pushing logic into EF configurations or endpoints
- Don't invent new patterns if the repo already has one that works
- Don't use abbreviated variable names (`req`, `cmd`, `res`, `ops`)
- Don't add unnecessary abstractions — three similar lines is better than a premature abstraction
- Don't over-engineer: only make changes that are directly requested or clearly necessary
