# ADR 004: Backend test project layout (xUnit)

## Status

Accepted.

## Context

Production projects and the host live under `apps/api/src/` and the solution layout are described in [ADR 005](./005-backend-solution-layout.md). The API is split into **Domain**, **Application**, **Infrastructure**, and **MythicNexus.Api** (host). We need repeatable automated tests without coupling every run to a live PostgreSQL instance, while still exercising the real HTTP pipeline for smoke and API-level checks.

## Decision

1. Place **test projects** under `apps/api/tests/`, registered in `MythicNexus.Api.sln` under a `tests` solution folder, mirroring responsibilities:
   - **`MythicNexus.Domain.Tests`** — pure domain/unit tests.
   - **`MythicNexus.Application.Tests`** — application rules (e.g. FluentValidation) against referenced application code.
   - **`MythicNexus.Infrastructure.Tests`** — persistence-focused tests using **EF Core InMemory** (not a substitute for PostgreSQL semantics; use for fast, isolated DbContext behavior).
   - **`MythicNexus.Api.IntegrationTests`** — **`Microsoft.AspNetCore.Mvc.Testing`** + **`WebApplicationFactory<Program>`** for end-to-end HTTP against the host.
2. Expose the minimal hosting entry point to the factory via **`public partial class Program`** in the API project (alongside top-level `Program.cs`).
3. Run integration tests with **`ASPNETCORE_ENVIRONMENT=Testing`** and host-specific **`appsettings.Testing.json`** (connection string + `Jwt` values satisfying `ValidateOnStart`), instead of relying on in-memory configuration merging with `WebApplicationBuilder` in all cases.

## Consequences

- **Positive**: clear place to add tests per layer; CI can run `dotnet test` / `pnpm test` on the solution; integration tests do not require Postgres for basic host and anonymous routes.
- **Negative**: InMemory EF does not validate Npgsql-specific behavior; PostgreSQL-focused tests still need Docker/Testcontainers or a dedicated DB when we add them.
- **Operational**: when new required configuration is added to the host startup path, **`appsettings.Testing.json`** must stay valid or integration tests will fail early (intentional).

## References

- [docs/architecture.md](../architecture.md)
- [ADR 003](./003-aspnet-modular-monolith.md)
- [ADR 005](./005-backend-solution-layout.md)
