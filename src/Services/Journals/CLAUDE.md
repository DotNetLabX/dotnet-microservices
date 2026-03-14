# Journals Service

**Endpoint framework:** FastEndpoints (no MediatR)
**Database:** Redis (Redis.OM) — NOT SQL Server
**Port:** 4402 / 4452

## Domain model

- **Entity:** Journal (extends `Blocks.Redis.Entity`, NOT `AggregateRoot`)
- Redis.OM decorators: `[Document]`, `[Indexed]`, `[Searchable]`
- **Repository:** `Repository<T>` from Blocks.Redis (not EF Core)
- **ID generation:** Redis string increment

## Endpoint pattern

Handler logic lives directly in endpoint class (combined pattern). No separate handler files.

## gRPC server

JournalGrpcService — exposes journal data (GetJournalById, IsEditorAssignedToJournal)

## gRPC clients

IPersonService (Auth)

## Existing features

**Journals:** Create, GetById, Update
**Sections:** Create, GetById, Update
**Editors:** GetBySectionId
