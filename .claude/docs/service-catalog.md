# Service Catalog

## Overview

| Service | HTTP | HTTPS | Database | Framework | gRPC Server |
|---------|------|-------|----------|-----------|-------------|
| Auth | 4401 | 4451 | SQL Server (AuthDb) | FastEndpoints | Yes (PersonGrpcService) |
| Journals | 4402 | 4452 | Redis | FastEndpoints | Yes (JournalGrpcService) |
| ArticleHub | 4403 | 4453 | PostgreSQL (ArticleHubDb) | Carter | No |
| Submission | 4404 | 4454 | SQL Server (SubmissionDb) | Minimal APIs | No |
| Review | 4405 | 4455 | SQL Server (ReviewDb) | Carter | No |
| Production | 4406 | 4456 | SQL Server (ProductionDb) | FastEndpoints | No |

## Auth Service

**Bounded context:** User identity, authentication, person management
**Features:**
- Users: CreateAccount, Login, Me, RefreshToken, SendChangePasswordLink, SetPassword
- Persons: gRPC server only (PersonGrpcService)
**Modules used:** EmailService.Empty
**gRPC clients:** None (server only)

## Journals Service

**Bounded context:** Journal and section metadata
**Features:**
- Journals: Create, GetById, Update
- Sections: Create, GetById, Update
- Editors: GetBySectionId
**Modules used:** EmailService.Empty
**gRPC clients:** IPersonService (Auth)
**Storage:** Redis via Redis.OM Repository<T>

## Submission Service

**Bounded context:** Article creation and submission workflow
**Features:**
- ApproveArticle, AssignAuthor, CreateAndAssignAuthor, CreateArticle
- DownloadFile, RejectArticle, SubmitArticle
- UploadManuscriptFile, UploadSupplementaryMaterialFile
**Modules used:** FileService.MongoGridFS, EmailService.Empty
**gRPC clients:** IPersonService (Auth), IJournalService (Journals)

## Review Service

**Bounded context:** Peer review process
**Features:**
- Articles: AcceptArticle, AssignEditor, GetArticle, RejectArticle
- Assets: DownloadFile, UploadManuscriptFile, UploadReviewReport
- Invitations: AcceptInvitation, DeclineInvitation, GetArticleInvitations, InviteReviewer
**Modules used:** FileService.MongoGridFS, EmailService.Empty
**gRPC clients:** IPersonService (Auth)

## Production Service

**Bounded context:** Post-acceptance production and typesetting
**Features:**
- Articles: AssignTypesetter, GetArticle (Summary + Assets)
- Assets: ApproveAssets, DownloadFile, RequestAssets (Final/Supplementary/Cancel), UploadFiles (Draft/Final/Supplementary)
**Modules used:** FileService.AzureBlob
**gRPC clients:** None currently

## ArticleHub Service

**Bounded context:** Read-only aggregate view across all services
**Features:**
- GetArticle, GetTimeline, SearchArticles
**Storage:** PostgreSQL
**gRPC clients:** None
**Messaging:** MassTransit consumers for all integration events
