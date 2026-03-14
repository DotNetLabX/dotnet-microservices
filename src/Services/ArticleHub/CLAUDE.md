# ArticleHub Service

**Endpoint framework:** Carter (partial)
**Database:** PostgreSQL (ArticleHubDb)
**Port:** 4403 / 4453

## Purpose

Read-only aggregate view — aggregates latest state from all services via integration event consumers. No domain logic — pure projection/read model.

## Consumers

One MassTransit consumer per integration event. Updates local Article representation.

## Existing features

GetArticle, GetTimeline, SearchArticles
