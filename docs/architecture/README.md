## Architecture Notes (Course Scope)

- **Layering:** Domain / Application / Persistence / API. Some examples use `DbContext` reads for brevity; repositories remain the main seam.
- **DDD:** Aggregates enforce invariants; Value Objects model constrained types. We show the pattern once, then avoid repeating it everywhere.
- **CQRS:** Commands mutate via handlers; queries read via handlers/EF projections. Handlers orchestrate, repositories fetch.
- **Service Boundaries:** gRPC/integration contracts **guide** the domain; we do not compose aggregates from remote DTOs.
- **BuildingBlocks:** Cross-cutting concerns live in `Blocks.*` (logging, EF base repo, MediatR behaviors).
- **Observability:** Correlation-Id + request logging (dev-friendly tracing). Full dashboards are out of scope.
- **Config & DI:** Options + DI per layer. Slight per-service variation is intentional for teaching.
- **Repo == UoW (teaching choice):** Repos expose `SaveChangesAsync()` to reduce boilerplate and anchor saves on the main aggregate repo.

> **Course principle:** Once a pattern is demonstrated clearly, we don’t re-implement it everywhere. Repetition adds noise, not learning.
