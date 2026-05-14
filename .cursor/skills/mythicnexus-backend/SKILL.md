---
name: mythicnexus-backend
description: >-
  Guides ASP.NET Core 9 minimal API in apps/api: modular Modules/, EF Core 9 +
  Npgsql, FluentValidation, LocalEnvLoader/DotNetEnv, migrations, and Postgres
  (Docker or Supabase). Use when changing Program.cs, DbContext, entities,
  endpoints, or database config.
---

# MythicNexus — backend (`apps/api`)

## Stack and layout

- **.NET 9** (`global.json` / `TargetFramework`). **Minimal hosting model** (top-level `src/Program.cs`).
- **`src/` structure**:
  - `Domain/Entities` — persisted model.
  - `Domain/Validation` — FluentValidation rules (registered via `AddValidatorsFromAssemblyContaining<User>()`).
  - `Infrastructure/Persistence` — `MythicNexusDbContext`, EF migrations, `Configurations/` (`IEntityTypeConfiguration<>` per aggregate).
  - `Infrastructure/Configuration` — `LocalEnvLoader` (loads `.env` before the host starts).
  - `Modules/{Campaigns,Characters,Lore,Search,Users,AI}/` — `Add*Module` + `Map*Endpoints` extensions wired in `Program.cs`.

When adding a bounded context: create a folder under `Modules/`, register services and `Map*Endpoints` in `Program.cs` following existing patterns.

## Configuration and secrets

- Call **`LocalEnvLoader.Load()`** **before** `WebApplication.CreateBuilder(args)` so `.env` variables participate in configuration.
- Use **`ConnectionStrings__Default`** in `apps/api/.env` (gitignored). Non-secret template: `.env.example`.
- **`appsettings.json`**: no production connection string; **`appsettings.Development.json`** supplies a local Docker fallback (`mythicnexus`).
- **Supabase**: host `db.<ref>.supabase.co`, user `postgres`, database is typically **`postgres`** (the Supabase project display name is not the database name). **TLS**: `SSL Mode=Require` (and `Trust Server Certificate=true` on Windows dev if required).

## EF Core and migrations

- **`dotnet-ef`** is declared in **root** `.config/dotnet-tools.json`. After clone: `dotnet tool restore` from the repository root.
- Create and apply migrations **from the root** (tool manifest location), for example:
  - `dotnet ef migrations add <Name> --project apps/api/api.csproj --startup-project apps/api/api.csproj --output-dir src/Infrastructure/Persistence/Migrations`
  - `dotnet ef database update --project apps/api/api.csproj --startup-project apps/api/api.csproj`
- Avoid hand-editing the model snapshot except to resolve merge conflicts; prefer additive migrations.

## Key packages

`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design` (private assets), `FluentValidation.AspNetCore`, `DotNetEnv`, `Microsoft.AspNetCore.OpenApi`.

## Quality and API surface

- OpenAPI in Development (`MapOpenApi`).
- Validate request bodies with FluentValidation as endpoints grow.
- Keep HTTP routes under `/api/...` prefixes consistent with each module.

## Related skills

- Monorepo, Turbo, Supabase MCP: `mythicnexus-overview`.
- Web UI and public env vars: `mythicnexus-frontend`.
