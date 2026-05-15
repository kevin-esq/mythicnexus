# MythicNexus architecture

## Vision

MythicNexus is an AI-powered RPG knowledge and campaign platform focused on content organization, semantic search, collaborative storytelling, and intelligent campaign memory.

Near-term delivery is **not** AI-heavy: first ship a **searchable knowledge platform** (clean domain, PostgreSQL, full-text search, imports). Add embeddings, agents, and semantic features after the core data model and search are solid.

## Repository layout

- `apps/web` — Next.js (App Router), TypeScript, Tailwind CSS
- `apps/api` — ASP.NET Core 9: **`MythicNexus.Api.sln`** at this folder; **production projects under `apps/api/src/`**, **test projects under `apps/api/tests/`**, matching a typical Visual Studio layout. Optional `apps/api/.env` for connection strings and JWT overrides.
- `packages/` — shared TypeScript libraries (empty until needed)
- `infrastructure/` — notes for local and cloud infrastructure; root `docker-compose.yml` runs Postgres locally
- `docs/` — vision, roadmap, architecture, and technical decisions

## Backend layout (`apps/api`)

| Project | Path |
|--------|------|
| **MythicNexus.Api** (host) | `apps/api/src/MythicNexus.Api` — `Program.cs`, `Modules/`, `Http/`, `appsettings*`, `Properties/` |
| **MythicNexus.Domain** | `apps/api/src/MythicNexus.Domain` — entities only |
| **MythicNexus.Application** | `apps/api/src/MythicNexus.Application` — use cases (auth, validation, error codes) |
| **MythicNexus.Infrastructure** | `apps/api/src/MythicNexus.Infrastructure` — EF Core, migrations, `LocalEnvLoader`, middleware |

**Tests** (`apps/api/tests/`): xUnit mirror — `MythicNexus.Domain.Tests`, `MythicNexus.Application.Tests`, `MythicNexus.Infrastructure.Tests`, `MythicNexus.Api.IntegrationTests` (host boot with `Testing` + `appsettings.Testing.json`). Run: `dotnet test apps/api/MythicNexus.Api.sln` from the repo root.

Open **`apps/api/MythicNexus.Api.sln`** in Visual Studio. From CLI: `dotnet build apps/api/MythicNexus.Api.sln`. (You can add an optional **`.slnx`** from Visual Studio if you prefer the JSON solution format; the repo ships **`.sln`** for broad `dotnet` CLI compatibility.)

## Domain sketch (current entities)

- **User** — identity with unique `Username`; primary workspace via `TenantId`; **tenant memberships** (`TenantMembership` + `TenantRole`) for org RBAC; **campaign memberships** (`CampaignMember` + `CampaignRole`) for table-level play; owns campaigns; authors lore (`CreatedBy` on `LoreEntry`).
- **Tenant** — workspace; has many `TenantMemberships` and **campaigns** (`Campaign.TenantId`).
- **Campaign** — belongs to a **tenant**; has **members** (`CampaignMember`); container for characters, lore, tags, and **lore relations** (graph edges scoped to the campaign).
- **Character** — belongs to a campaign and an **owning user** (`OwnerUserId`); optional `Race`, `Class`, `Backstory`, `Notes`.
- **LoreEntry** — `Title`, `Slug` (unique per campaign), optional `Summary`, `ContentMarkdown`, `CreatedByUserId`; many-to-many with tags.
- **LoreRelation** — directed edge between two lore entries in the same campaign (`RelationType` string, e.g. `references`, `contradicts`); unique on `(CampaignId, Source, Target, RelationType)`.
- **Tag** — scoped per campaign; many-to-many with lore entries.

Later: `TimelineEvent`, `World`, **source-agnostic content import** (providers → normalizers → jobs; optional D&D 5e API seeds only—see [ADR 007](./adr/007-external-content-ingestion-strategy.md)), and semantic search over this graph.

## Related documents

- [Roadmap](./roadmap.md) — phases and MVP scope
- [Technical decisions](./tech-decisions.md) — stack and open choices
- [Local database & migrations](./database-local.md) — Docker Postgres, `dotnet ef`, verify applied migrations
- [Security & authentication](./security-auth.md) — passwords, lockout, verification, rate limits, audit, tenants
- [Tenant vs campaign RBAC](./authorization-rbac.md) — workspace roles, campaign roles, character ownership
- [Architecture Decision Records (ADRs)](./adr/) — monorepo (001), PostgreSQL (002), modular API (003), [testing layout](./adr/004-backend-testing-layout.md), [solution layout & ergonomics](./adr/005-backend-solution-layout.md), [tenant vs campaign authorization](./adr/006-tenant-and-campaign-authorization.md), [external content ingestion](./adr/007-external-content-ingestion-strategy.md)
