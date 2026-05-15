# Roadmap

## Current status

**Shipped (foundation):**

- Monorepo with pnpm and Turborepo (`pnpm build`, `pnpm test`, `pnpm dev`)
- Next.js app shell, dark UI baseline, login/register, protected dashboard (TanStack Query + API client)
- **Backend solution layout** — `MythicNexus.Api.sln`, all production projects under `apps/api/src/`, host `MythicNexus.Api`, solution folders and tooling (pnpm, Turbo, VS Code); optional `apps/api/.env` discovery across build/cwd paths ([ADR 005](./adr/005-backend-solution-layout.md))
- **Backend product** — modular `Modules/*`, EF Core + Npgsql, JWT auth ([ADR 003](./adr/003-aspnet-modular-monolith.md))
- **Automated tests** — xUnit under `apps/api/tests/` (layers + `WebApplicationFactory`); `Testing` + `appsettings.Testing.json` ([ADR 004](./adr/004-backend-testing-layout.md))
- Docker Compose for local Postgres; **schema** applied via EF migrations ([docs/database-local.md](./database-local.md))
- Architecture notes, tech decisions, and ADRs

## MVP: searchable knowledge platform

1. **Auth (polish & hardening)** — refresh tokens or shorter-lived access patterns, password reset, rate limits and error UX as needed (baseline register/login exists)
2. **Campaigns** — create campaigns, notes, characters
3. **Lore** — markdown entries, tags, relationships
4. **Search** — PostgreSQL full-text search on lore and notes
5. **Import** — markdown / JSON and optional **legal third-party seeds** (e.g. D&D 5e API) only behind a **source-agnostic ingestion layer**; no D&D Beyond coupling; see [ADR 007](./adr/007-external-content-ingestion-strategy.md). **Order:** campaigns → lore → search → import foundation.

Defer until after MVP: embeddings, agents, heavy AI features.
