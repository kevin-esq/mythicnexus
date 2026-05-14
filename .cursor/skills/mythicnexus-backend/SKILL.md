---
name: mythicnexus-backend
description: >-
  Guides ASP.NET Core 9 minimal API in apps/api: layered projects (Domain,
  Application, Infrastructure), EF Core 9 + Npgsql, FluentValidation,
  LocalEnvLoader/DotNetEnv, migrations, and Postgres (Docker or Supabase). Use
  when changing Program.cs, DbContext, entities, endpoints, or database config.
---

# MythicNexus — backend (`apps/api`)

## Stack and layout

- **.NET 9** (`global.json` / `TargetFramework`). **Minimal hosting model** (`src/MythicNexus.Api/Program.cs`).
- **Solution** — `apps/api/MythicNexus.Api.sln`. **Production code under `apps/api/src/`**: `MythicNexus.Api` (Web host), `MythicNexus.Domain`, `MythicNexus.Application`, `MythicNexus.Infrastructure`. **Tests under `apps/api/tests/`** (xUnit): `MythicNexus.Domain.Tests`, `MythicNexus.Application.Tests`, `MythicNexus.Infrastructure.Tests` (EF InMemory), `MythicNexus.Api.IntegrationTests` (`WebApplicationFactory<Program>`, environment `Testing` + `appsettings.Testing.json` on the API project).
  - **`MythicNexus.Domain`** — entities (`MythicNexus.Domain.Entities`), no infra packages.
  - **`MythicNexus.Infrastructure`** — `MythicNexusDbContext`, `Configurations/`, EF migrations, `LocalEnvLoader`, `CorrelationIdMiddleware`, `AddInfrastructurePersistence`.
  - **`MythicNexus.Application`** — auth use cases (DTOs, validators, `AuthService`, JWT signing, `ErrorCodes`), `AddApplication`; references Infrastructure for `MythicNexusDbContext` (pragmatic until repositories/UoW).
  - **`MythicNexus.Api` host** — OpenAPI, JWT bearer *validation*, CORS, rate limits, `GlobalExceptionHandler`, `Modules/**` endpoint maps.

When adding a bounded context: prefer new types in **Application** + persistence in **Infrastructure**; keep **Domain** free of web/EF concerns. Formal layout and testing decisions: [ADR 005](docs/adr/005-backend-solution-layout.md), [ADR 004](docs/adr/004-backend-testing-layout.md) (paths from repo root).

## Configuration and secrets

- Call **`LocalEnvLoader.Load()`** **before** `WebApplication.CreateBuilder(args)` so `.env` variables participate in configuration.
- Use **`ConnectionStrings__Default`** in **`apps/api/.env`** (gitignored). `LocalEnvLoader` checks **`apps/api/.env` first** when the current directory is the repo root. Template: **`apps/api/.env.example`**.
- **`appsettings.json`**: no production connection string; **`appsettings.Development.json`** supplies a local Docker fallback (`mythicnexus`). For **Supabase Postgres** only, use **`ASPNETCORE_ENVIRONMENT=Supabase`** + **`appsettings.Supabase.json`** (no default connection string) and the **`Supabase`** launch profile — see **`docs/database-local.md`**.
- **Supabase**: host `db.<ref>.supabase.co`, user `postgres`, database is typically **`postgres`** (the Supabase project display name is not the database name). **TLS**: `SSL Mode=Require` (and `Trust Server Certificate=true` on Windows dev if required).

## EF Core and migrations

- **`dotnet-ef`** is declared in **root** `.config/dotnet-tools.json`. After clone: `dotnet tool restore` from the repository root.
- Migrations live in **`MythicNexus.Infrastructure`**; startup/design-time entry is **`MythicNexus.Api`** (host). The host references **`Microsoft.EntityFrameworkCore.Design`** (private assets) so `dotnet ef` works with that startup project.
- For local Docker Postgres, set **`ASPNETCORE_ENVIRONMENT=Development`** when running `dotnet ef` so `appsettings.Development.json` supplies `ConnectionStrings:Default`. For **Supabase**, use **`ASPNETCORE_ENVIRONMENT=Supabase`** (and `apps/api/.env` with your cloud connection string). See **`docs/database-local.md`** for full commands, `migrations list`, and troubleshooting.
  - `dotnet ef migrations add <Name> --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj --output-dir Persistence/Migrations`
  - `dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj`
- Avoid hand-editing the model snapshot except to resolve merge conflicts; prefer additive migrations.

## Key packages

Host: `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.OpenApi`, `Microsoft.EntityFrameworkCore.Design` (private assets; EF CLI startup project). Infrastructure: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design` (private assets), `DotNetEnv`. Application: `FluentValidation`, `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`.

## Tests

- From `apps/api`: `pnpm test` or `dotnet test MythicNexus.Api.sln -c Release`.
- Integration tests use **`ASPNETCORE_ENVIRONMENT=Testing`**; keep **`appsettings.Testing.json`** on the host in sync with required `ConnectionStrings:Default` and `Jwt` (secret ≥ 32 chars for `ValidateOnStart`).

## Security and auth

- See **`docs/security-auth.md`** (password policy, lockout, email verification, forgot password, rate limits, login audit, tenants, JWT claims). Local email drops: **`Email:LocalOutbox`** → `email-outbox` under the API content root until SMTP is configured.
- **Two-layer RBAC**: **`docs/authorization-rbac.md`** and **`docs/adr/006-tenant-and-campaign-authorization.md`** — `TenantMembership` / `CampaignMember`, `TenantCapabilityRules` + `CampaignCapabilityRules`, `ITenantPermissionService` / `ICampaignPermissionService` in `MythicNexus.Application/Authorization/`. Prefer these over scattered role checks in endpoints.

## Quality and API surface

- OpenAPI in Development and **Supabase** local profile (`MapOpenApi`).
- Validate request bodies with FluentValidation as endpoints grow.
- Keep HTTP routes under `/api/...` prefixes consistent with each module.

## Related skills

- Monorepo, Turbo, Supabase MCP: `mythicnexus-overview`.
- Web UI and public env vars: `mythicnexus-frontend`.
