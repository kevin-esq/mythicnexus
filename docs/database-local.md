# Local database (PostgreSQL + EF Core)

This guide covers the **Docker Postgres** instance at the repo root and applying **EF Core migrations** to match the current domain model.

## 1. Start PostgreSQL

From the repository root:

```bash
docker compose up -d postgres
```

Defaults match [docker-compose.yml](../docker-compose.yml) and the API’s [appsettings.Development.json](../apps/api/src/MythicNexus.Api/appsettings.Development.json): database `mythicnexus`, user `mythic`, password `mythic`, port `5432`.

## 2. Restore the EF CLI tool

Once per clone (tool version is pinned in [.config/dotnet-tools.json](../.config/dotnet-tools.json)):

```bash
dotnet tool restore
```

## 3. Apply migrations

Migrations live in **`MythicNexus.Infrastructure`** (`Persistence/Migrations/`). The **startup project** is **`MythicNexus.Api`** so configuration (connection string, JWT for design-time if needed) loads from `appsettings*.json` and optional `apps/api/.env`.

Use **Development** when pointing at local Docker:

**Windows (PowerShell)**

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

**macOS / Linux (bash)**

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

After a successful run, the database schema matches the latest migration.

## 4. Verify what is applied

```bash
dotnet ef migrations list --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

With `ASPNETCORE_ENVIRONMENT=Development` set as above:

- Migrations **without** `(Pending)` are recorded in `__EFMigrationsHistory` and applied.
- `(Pending)` means `database update` has not been run (or the DB is new).

## Current migrations (as of this doc)

| Migration                         | Purpose (summary)        |
| --------------------------------- | -------------------------- |
| `20260514110413_InitialCreate`    | Initial schema / tables    |
| `20260514125049_DomainFoundationLoreGraph` | Lore graph, campaigns, characters |
| `20260514140856_SecurityTenancyAuthHardening` | Tenants, login audit, email/reset tokens, lockout & email flags on `Users` |
| `20260514143036_TenantCampaignRbacAndCharacterOwnership` | Tenant/campaign RBAC, `Character.OwnerUserId`, `Campaign.TenantId` |
| `20260514152106_CampaignCoreSoftDeleteAudit` | Soft delete + audit columns on campaigns, characters, lore; `Character.Level`; tenant-scoped list indexes |

New migrations should be added with `dotnet ef migrations add …` from the repo root using the same `--project` / `--startup-project` pair; see the backend skill in `.cursor/skills/mythicnexus-backend/SKILL.md`.

## Remote databases (e.g. Supabase)

Identity and registration are **stored in your PostgreSQL database** (EF tables). Supabase provides that Postgres host; we do **not** use Supabase Auth for sign-up.

### A. Connection string

1. In Supabase: **Project Settings → Database** — copy the **URI** or build the parameters (host `db.<ref>.supabase.co`, database usually **`postgres`**, user **`postgres`**, TLS required).
2. Copy [`apps/api/.env.example`](../../apps/api/.env.example) to **`apps/api/.env`** (same folder). Set **`ConnectionStrings__Default=…`** (Npgsql format). On Windows dev you often need **`Trust Server Certificate=true`** with **`SSL Mode=Require`** (see example file).

`LocalEnvLoader` loads **`apps/api/.env` first** when you run `dotnet` / `dotnet ef` from the **repository root**, so you do not have to `cd apps/api`.

### B. Schema (migrations)

Apply migrations **once** per Supabase project (same commands as local Docker, but set the environment so **`appsettings.Development.json` does not supply a localhost connection**):

**Windows (PowerShell)**

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Supabase"
dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

**macOS / Linux**

```bash
export ASPNETCORE_ENVIRONMENT=Supabase
dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

The host loads **`appsettings.Supabase.json`** (JWT, auth URLs, email outbox) and the **connection string only from** `apps/api/.env` (or real environment variables in CI).

### C. Run the API

From the repo root:

```bash
dotnet run --project apps/api/src/MythicNexus.Api --launch-profile Supabase
```

Keep the **web** pointed at that API (`apps/web/.env.local` → `NEXT_PUBLIC_API_URL=http://localhost:5118` unless your API is deployed elsewhere). Register in the UI: rows appear in Supabase **Table Editor** under `Users`, `Tenants`, `TenantMemberships`, etc.

### Troubleshooting (Supabase)

| Issue | What to check |
| ----- | ------------- |
| **SSL / certificate** | `SSL Mode=Require`; on Windows add `Trust Server Certificate=true` for dev. |
| **Wrong host** | Prefer direct `db.*.supabase.co` for long-lived ASP.NET; pooler hostnames differ (often port **6543**). |
| **“Connection string not configured”** with profile `Supabase` | Missing or unloaded `apps/api/.env` — path must be `apps/api/.env` from repo root, or set `ConnectionStrings__Default` in the shell. |

## Troubleshooting

| Issue | What to check |
| ----- | ------------- |
| **Cannot connect** | `docker compose ps`; firewall; port `5432` not used by another service. |
| **`dotnet ef` says Design package missing on startup project** | The host **`MythicNexus.Api`** references `Microsoft.EntityFrameworkCore.Design` (private assets) so the CLI can use it as startup project. Restore/build again. |
| **Wrong database / credentials** | `appsettings.Development.json` vs `apps/api/.env` — env vars override JSON when both are loaded. |

## Related

- [infrastructure/README.md](../infrastructure/README.md) — short copy-paste commands
- [ADR 002 — PostgreSQL / Supabase](./adr/002-postgresql-supabase.md)
