# MythicNexus

A **knowledge and campaign platform** for tabletop role-playing: lore organization, campaigns, search, and campaign memory. The longer-term vision includes assisted AI; the **MVP** prioritizes a clean domain, PostgreSQL, and **full-text search**—not agents or embeddings in the first iteration.

## Vision (summary)

See [docs/architecture.md](docs/architecture.md) and [docs/roadmap.md](docs/roadmap.md).

## Tech stack

| Area       | Technology                                                                                                                          |
| ---------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| Monorepo   | [pnpm](https://pnpm.io/) workspaces + [Turborepo](https://turbo.build/)                                                             |
| Web        | [Next.js](https://nextjs.org/) 16 (App Router), React 19, TypeScript, Tailwind CSS v4, [TanStack Query](https://tanstack.com/query) |
| API        | [ASP.NET Core](https://dotnet.microsoft.com/) 9, minimal APIs, [EF Core](https://learn.microsoft.com/ef/core/) 9 + Npgsql           |
| Validation | [FluentValidation](https://docs.fluentvalidation.net/)                                                                              |
| Database   | PostgreSQL ([Docker](docker-compose.yml) locally and/or remote [Supabase](https://supabase.com/))                                   |
| EF CLI     | `dotnet-ef` ([.config/dotnet-tools.json](.config/dotnet-tools.json))                                                                |

## Monorepo layout

```txt
apps/web        → Next.js frontend
apps/api        → .NET 9 backend: solution `MythicNexus.Api.sln`, code under `apps/api/src/`, xUnit tests under `apps/api/tests/`
packages/       → shared TypeScript libraries (empty until introduced)
docs/           → architecture, roadmap, decisions, ADRs
infrastructure/ → local infrastructure notes and commands
```

## Architecture

- **API**: host `src/MythicNexus.Api` (Program, modules, HTTP); libraries `MythicNexus.Domain`, `MythicNexus.Application`, `MythicNexus.Infrastructure` (EF, migrations).
- **Decisions**: [docs/tech-decisions.md](docs/tech-decisions.md) and [docs/adr/](docs/adr/).

## Prerequisites

- **Node.js 20+** (recommended) and **pnpm** 9+ (the repository pins `packageManager` in `package.json`).
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

## Getting started

```bash
pnpm install
dotnet tool restore
```

### Database

- **Local**: `docker compose up -d postgres` (credentials in [docker-compose.yml](docker-compose.yml), aligned with `appsettings.Development.json` on the API). Full steps, verification, and troubleshooting: **[docs/database-local.md](docs/database-local.md)**.
- **Remote**: set the connection string in `apps/api/.env` (copy from [apps/api/.env.example](apps/api/.env.example)). Do **not** commit `.env`.

### Migrations

```bash
dotnet tool restore
# Use Development so the API loads appsettings.Development.json (local Docker).
# PowerShell: $env:ASPNETCORE_ENVIRONMENT = "Development"
# bash:       export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --project apps/api/src/MythicNexus.Infrastructure/MythicNexus.Infrastructure.csproj --startup-project apps/api/src/MythicNexus.Api/MythicNexus.Api.csproj
```

See **[docs/database-local.md](docs/database-local.md)** for `migrations list`, current migration names, and Supabase notes.

### Development

```bash
pnpm dev
```

- Web: [http://localhost:3000](http://localhost:3000)
- API: [http://localhost:5118](http://localhost:5118) (HTTP profile in `launchSettings.json`)

### Build, test, and lint

```bash
pnpm build
pnpm test
pnpm lint
```

## Environment variables

| Application | File                  | Example                                     |
| ----------- | --------------------- | ------------------------------------------- |
| API         | `apps/api/.env`       | `ConnectionStrings__Default=...`            |
| Web         | `apps/web/.env.local` | `NEXT_PUBLIC_API_URL=http://localhost:5118` |

Templates: `*.env.example` files under each app.

## Git branching

- `main` — stable / release.
- `develop` — integration.
- `feature/*` — feature work.

Protect `main` on GitHub (pull requests required; restrict direct pushes). Conventions, PR checklist, and CI: **[CONTRIBUTING.md](CONTRIBUTING.md)**.

## Roadmap

See [docs/roadmap.md](docs/roadmap.md): foundation sprint, MVP “searchable knowledge,” then AI / semantic search.

## License

See [package.json](package.json) (`license` field).
