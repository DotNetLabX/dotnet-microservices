---
name: add-integration-event
description: Adds a MassTransit integration event for cross-service communication. Creates the event contract, publisher in domain event handler, and consumer in target service(s). Use when a business event needs to propagate across service boundaries.
---

# Add Integration Event

Creates a complete MassTransit integration event with contract, publisher, and consumer(s).

## Steps

1. **Create the event contract** — follow `workflows/CreateEventContract.md`
2. **Create the publisher** (domain event handler) — follow `workflows/CreatePublisher.md`
3. **Create consumer(s)** in target services — follow `workflows/CreateConsumer.md`
4. **Verify the build:** `dotnet build`

## Arguments

Pass the event name: `/add-integration-event ArticlePublished`

## Existing integration events

| Event | Publisher | Consumers |
|-------|----------|-----------|
| ArticleApprovedForReviewEvent | Submission | Review, ArticleHub |
| ArticleAcceptedForProductionEvent | Review | Production, ArticleHub |
| ArticleReviewedEvent | Review | ArticleHub |
| ArticlePublishedEvent | Production | ArticleHub |
| JournalCreatedEvent | Journals | ArticleHub |
| JournalUpdatedEvent | Journals | ArticleHub |
| PersonUpdatedEvent | Auth | ArticleHub |
