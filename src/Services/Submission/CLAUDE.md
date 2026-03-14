# Submission Service

**Endpoint framework:** Minimal APIs + MediatR
**Database:** SQL Server (SubmissionDb) + MongoDB (GridFS for files)
**Port:** 4404 / 4454

## Domain model

- **Aggregate:** Article (partial class split — state in `Article.cs`, behavior in `Behaviors/Article.cs`)
- **Key entities:** Asset, ArticleActor, ArticleAuthor
- **Application layer:** Separate project (`Submission.Application`)

## Endpoint pattern

Static class with `Map()` extension method, composed via `EndpointRegistration.MapAllEndpoints()`. Endpoints dispatch to MediatR via `ISender`.

## MediatR pipeline behaviors

AssignUserIdBehavior, ValidationBehavior, LoggingBehavior

## File storage

MongoGridFS (singleton registration)

## Existing features

ApproveArticle, AssignAuthor, CreateAndAssignAuthor, CreateArticle, DownloadFile, RejectArticle, SubmitArticle, UploadManuscriptFile, UploadSupplementaryMaterialFile

## gRPC clients

IPersonService (Auth), IJournalService (Journals)
