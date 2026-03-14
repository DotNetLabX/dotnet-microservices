# Auth Service

**Endpoint framework:** FastEndpoints (partial MediatR)
**Database:** SQL Server (AuthDb) with ASP.NET Identity
**Port:** 4401 / 4451

## Domain model

- **Aggregate:** User (extends `IdentityUser<int>`, implements `IAggregateRoot` manually — cannot extend `AggregateRoot<T>`)
- **Entities:** Person, RefreshToken
- **No separate Application layer**

## Auth

- JWT configuration in `Articles.Security`
- Role constants in `Articles.Security/Role.cs`

## gRPC server

PersonGrpcService — exposes person data to other services (GetPersonById, GetPersonByUserId, GetPersonByEmail, GetOrCreatePersonAsync)

## Existing features

**Users:** CreateAccount, Login, Me, RefreshToken, SendChangePasswordLink, SetPassword
**Persons:** gRPC server only
