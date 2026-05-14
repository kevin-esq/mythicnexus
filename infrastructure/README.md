# Infrastructure

Local dependencies are defined at the repository root in `docker-compose.yml` (PostgreSQL 16 for MythicNexus).

From the repo root:

```bash
docker compose up -d postgres
```

Apply EF Core migrations (requires the tool restore once per clone). Prefer setting **`ASPNETCORE_ENVIRONMENT=Development`** so the API picks up `appsettings.Development.json` for local Docker.

```bash
dotnet tool restore
dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

Details, `migrations list`, and troubleshooting: **[docs/database-local.md](../docs/database-local.md)**.

## Environment files

- **Web** (`apps/web`): copy `.env.example` or `.env.local.example` to `.env.local` for `NEXT_PUBLIC_*` variables (see `docs/tech-decisions.md`). Point `NEXT_PUBLIC_API_URL` at the API that uses your Supabase Postgres (local `http://localhost:5118` with the **Supabase** launch profile, or a deployed URL).
- **API** (`apps/api`): copy `.env.example` to **`apps/api/.env`** and set `ConnectionStrings__Default` for Supabase (see [docs/database-local.md](../docs/database-local.md) — “Remote databases”). From the repo root, `LocalEnvLoader` picks up `apps/api/.env` automatically.
- **Run API against Supabase DB**: `dotnet run --project apps/api/src/MythicNexus.Api --launch-profile Supabase` (after `dotnet ef database update` with `ASPNETCORE_ENVIRONMENT=Supabase`).
- Development defaults for **local Docker** Postgres remain in `appsettings.Development.json`.
