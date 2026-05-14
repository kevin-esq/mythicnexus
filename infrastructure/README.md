# Infrastructure

Local dependencies are defined at the repository root in `docker-compose.yml` (PostgreSQL 16 for MythicNexus).

From the repo root:

```bash
docker compose up -d postgres
```

Apply EF Core migrations (requires the tool restore once per clone):

```bash
dotnet tool restore
dotnet ef database update --project apps/api/api.csproj --startup-project apps/api/api.csproj
```

## Environment files

- **Web** (`apps/web`): copy `.env.example` or `.env.local.example` to `.env.local` for `NEXT_PUBLIC_*` variables (see `docs/tech-decisions.md`).
- **API** (`apps/api`): optional `.env` (see `.env.example`). Development defaults for PostgreSQL are in `appsettings.Development.json`.
