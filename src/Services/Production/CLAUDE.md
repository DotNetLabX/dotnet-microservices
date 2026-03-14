# Production Service

**Endpoint framework:** FastEndpoints (no MediatR)
**Database:** SQL Server (ProductionDb) + Azure Blob Storage (files)
**Port:** 4406 / 4456

## Domain model

- **Aggregate:** Article (partial class split — state in `Article.cs`, behavior in `Behaviors/Article.cs`)
- **No separate Application layer** — features live in API project

## Endpoint pattern

Attribute-based `[HttpPost]`/`[HttpGet]`, handler logic directly in `HandleAsync`. Extends `Endpoint<TRequest, TResponse>`.

## Validators

Extend `BaseValidator<T>` (custom, wraps FastEndpoints `Validator<T>`)

## Domain event dispatch

Uses `TransactionalDispatchDomainEventsInterceptor` (wraps save + dispatch in transaction)

## File storage

Azure Blob

## Existing features

**Articles:** AssignTypesetter, GetArticle (Summary + Assets)
**Assets:** ApproveAssets, DownloadFile, RequestAssets (Final/Supplementary/Cancel), UploadFiles (Draft/Final/Supplementary)
