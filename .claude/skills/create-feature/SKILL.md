---
name: create-feature
description: Creates a complete vertical slice feature including endpoint, command/query, handler, validator, and mappings. Use when implementing a new endpoint or feature slice. Check the service CLAUDE.md for which endpoint framework to use.
---

# Create Feature

Creates a complete vertical slice for a new feature. The endpoint framework determines which workflow to follow.

## Steps

1. **Determine the service and framework.** Check the service's CLAUDE.md for which endpoint framework to use:
   - Minimal APIs + MediatR → `workflows/CreateEndpointMinimalApi.md`
   - Carter + MediatR → `workflows/CreateEndpointCarter.md`
   - FastEndpoints (no MediatR) → `workflows/CreateEndpointFastEndpoints.md`

2. **Create the endpoint** using the appropriate workflow above.

3. **Create the command/query** — if the service uses MediatR, follow `workflows/CreateHandler.md`.

4. **Create the validator** — follow `workflows/CreateValidator.md`.

5. **Create mappings** (if needed) — follow `workflows/CreateMappings.md`.

6. **Register the endpoint:**
   - Minimal APIs: add to `EndpointRegistration.MapAllEndpoints()` in the API project
   - Carter: auto-discovered via `AddCarter()` — no manual registration
   - FastEndpoints: auto-discovered — no manual registration

7. **Verify the build:** `dotnet build`

## Arguments

Pass the feature name as argument: `/create-feature AssignReviewer`

The feature name is used for all file names: `{FeatureName}Endpoint.cs`, `{FeatureName}Command.cs`, etc.
