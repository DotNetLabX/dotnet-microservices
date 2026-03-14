# Review Service

**Endpoint framework:** Carter + MediatR
**Database:** SQL Server (ReviewDb) + MongoDB (GridFS for files, dual storage with factory)
**Port:** 4405 / 4455

## Domain model

- **Aggregate:** Article (partial class split — state in `Article.cs`, behavior in `Behaviors/Article.cs`)
- **Application layer:** Separate project (`Review.Application`)

## Endpoint pattern

`ICarterModule` with `AddRoutes(IEndpointRouteBuilder)`. Endpoints dispatch to MediatR via `ISender`.

## MediatR pipeline behaviors

AssignUserIdBehavior, ValidationBehavior, LoggingBehavior

## File storage

MongoGridFS (singleton + scoped with factory)

## Existing features

**Articles:** AcceptArticle, AssignEditor, GetArticle, RejectArticle
**Assets:** DownloadFile, UploadManuscriptFile, UploadReviewReport
**Invitations:** AcceptInvitation, DeclineInvitation, GetArticleInvitations, InviteReviewer

## gRPC clients

IPersonService (Auth)
