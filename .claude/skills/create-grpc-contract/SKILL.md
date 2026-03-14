---
name: create-grpc-contract
description: Creates a gRPC code-first contract, server implementation, client registration, and updates callers. Use when adding service-to-service synchronous communication.
---

# Create gRPC Contract

Creates a complete gRPC code-first contract with server and client.

## Steps

1. **Create the contract** — follow `workflows/CreateContract.md`
2. **Create the server implementation** — follow `workflows/CreateServer.md`
3. **Register the client** in consuming services — follow `workflows/CreateClient.md`
4. **Verify the build:** `dotnet build`

## Arguments

Pass the service contract name: `/create-grpc-contract ArticleQueryService`

## Existing contracts

| Contract | Host | Location |
|----------|------|----------|
| IPersonService | Auth | `Articles.Grpc.Contracts/Auth/PersonContracts.cs` |
| IJournalService | Journals | `Articles.Grpc.Contracts/Journals/JournalContracts.cs` |
| IArticleQueryService | Submission | `Articles.Grpc.Contracts/Submission/ArticleRequest.cs` (placeholder) |
