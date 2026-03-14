---
name: article-lifecycle
description: Cross-service article lifecycle flow and integration event chain. Loaded when discussing or implementing features that span multiple services, involve article state transitions, or require integration events between Submission, Review, Production, and ArticleHub.
user-invocable: false
---

# Article Lifecycle — Cross-Service Flow

## Lifecycle stages

Each article progresses through stages defined by `ArticleStage` enum (in `Articles.Abstractions`):

```
Draft → Submitted → InitialApproved → UnderReview → Reviewed → AcceptedForProduction → InProduction → Published
```

## Service ownership per stage

| Stage | Owning Service | What happens |
|-------|---------------|--------------|
| Draft | Submission | Article created, authors assigned, manuscript uploaded |
| Submitted | Submission | Author submits for review |
| InitialApproved | Submission | Editor approves, triggers `ArticleApprovedForReviewEvent` |
| UnderReview | Review | Editor assigns reviewers, reviewers submit reports |
| Reviewed | Review | All reviews complete |
| AcceptedForProduction | Review | Editor accepts, triggers `ArticleAcceptedForProductionEvent` |
| InProduction | Production | Typesetter assigned, assets managed |
| Published | Production | Final assets approved, triggers `ArticlePublishedEvent` |

## Integration event flow

```
Submission
  └── ArticleApprovedForReviewEvent(ArticleDto)
        ├── → Review (creates local Article aggregate)
        └── → ArticleHub (updates read model)

Review
  ├── ArticleReviewedEvent(ArticleDto)
  │     └── → ArticleHub (updates read model)
  └── ArticleAcceptedForProductionEvent(ArticleDto)
        ├── → Production (creates local Article aggregate)
        └── → ArticleHub (updates read model)

Production
  └── ArticlePublishedEvent(ArticleDto)
        └── → ArticleHub (updates read model)

Journals
  ├── JournalCreatedEvent(JournalDto) → ArticleHub
  └── JournalUpdatedEvent(JournalDto) → ArticleHub

Auth
  └── PersonUpdatedEvent(PersonDto) → ArticleHub
```

## Event DTO payloads

All events carry a full DTO snapshot (not just IDs):

- `ArticleDto` — Id, Title, Scope, Doi, Type, Stage, Journal, Actors list, Assets list, timestamps
- `JournalDto` — Id, Name, Abbreviation, ChiefEditorUserId
- `PersonDto` — Id, FirstName, LastName, Email

Contracts live in `src/BuildingBlocks/Articles.Integration.Contracts/`.

## Key principles

1. **Each service owns its own Article aggregate** — Submission's Article is different from Review's Article
2. **Integration events carry full state** — consumers don't call back to the publisher for data
3. **ArticleHub is the single read model** — aggregates latest state from all services
4. **No service reads another service's database** — all state flows through events
5. **Consumers must be idempotent** — events may be delivered more than once

## State machine

Services use `ArticleStateMachineFactory` to validate stage transitions. Invalid transitions throw `DomainException`.
