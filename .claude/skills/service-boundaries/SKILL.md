---
name: service-boundaries
description: Service ownership, database choices, endpoint frameworks, and module vs microservice decisions. Loaded when determining which service owns a feature, understanding cross-service dependencies, or choosing the correct patterns for a specific service.
user-invocable: false
---

# Service Boundaries

## Service overview

| Service | Owns | DB | Endpoint Framework | gRPC Server |
|---------|------|----|--------------------|-------------|
| Auth | Users, persons, roles, JWT | SQL Server (Identity) | FastEndpoints (partial MediatR) | PersonGrpcService |
| Journals | Journal/section metadata | Redis (Redis.OM) | FastEndpoints (no MediatR) | JournalGrpcService |
| Submission | Article creation & submission | SQL Server + MongoDB (files) | Minimal APIs + MediatR | — |
| Review | Peer review process | SQL Server + MongoDB (files) | Carter + MediatR | — |
| Production | Post-acceptance typesetting | SQL Server + Azure Blob (files) | FastEndpoints (no MediatR) | — |
| ArticleHub | Read-only aggregate view | PostgreSQL | Carter (partial) | — |

## Cross-service dependencies

### gRPC calls (synchronous)

```
Submission → Auth (IPersonService: get/create person data)
Submission → Journals (IJournalService: validate journal exists)
Review → Auth (IPersonService: get person data for reviewers)
Journals → Auth (IPersonService: verify editor exists)
```

### Integration events (asynchronous)

```
Submission → Review, ArticleHub
Review → Production, ArticleHub
Production → ArticleHub
Journals → ArticleHub
Auth → ArticleHub
```

ArticleHub consumes events from ALL other services. It never publishes events.

## Module vs microservice decision

**Modules** (shared lifecycle, no independent deployment):
- EmailService — pluggable email sending (Empty/SendGrid/SMTP)
- FileService — pluggable file storage (MongoGridFS/AzureBlob/MinIO)
- ArticleTimeline — tracks stage transitions via domain events
- CacheStorage — caching utilities

**Microservices** (independent deployment, different scaling/ownership):
- Auth, Journals, Submission, Review, Production, ArticleHub

**Decision rule:** Default to module. Extract to microservice only when there's a strong reason for independent deployment, scaling, or team ownership.

## Port assignments

| Service | HTTP | HTTPS |
|---------|------|-------|
| Auth | 4401 | 4451 |
| Journals | 4402 | 4452 |
| ArticleHub | 4403 | 4453 |
| Submission | 4404 | 4454 |
| Review | 4405 | 4455 |
| Production | 4406 | 4456 |

## File storage per service

| Service | Storage | Registration |
|---------|---------|-------------|
| Submission | MongoGridFS | `AddMongoFileStorageAsSingletone` |
| Review | MongoGridFS | `AddMongoFileStorageAsSingletone` + scoped with factory |
| Production | Azure Blob | `AddAzureFileStorage` |
