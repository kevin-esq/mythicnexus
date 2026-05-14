# MythicNexus architecture

## Vision

MythicNexus is an AI-powered RPG knowledge and campaign platform focused on content organization, semantic search, collaborative storytelling, and intelligent campaign memory.

Near-term delivery is **not** AI-heavy: first ship a **searchable knowledge platform** (clean domain, PostgreSQL, full-text search, imports). Add embeddings, agents, and semantic features after the core data model and search are solid.

## Repository layout

- `apps/web` — Next.js (App Router), TypeScript, Tailwind CSS
- `apps/api` — ASP.NET Core 9 Web API, modular `src/` layout, EF Core + PostgreSQL
- `packages/` — shared TypeScript libraries (empty until needed)
- `infrastructure/` — notes for local and cloud infrastructure; root `docker-compose.yml` runs Postgres locally
- `docs/` — vision, roadmap, architecture, and technical decisions

## Backend layout (`apps/api/src`)

- `Domain/` — entities and validation rules
- `Infrastructure/` — persistence (EF Core `DbContext`, migrations)
- `Modules/` — vertical slices: Campaigns, Characters, Lore, Search, Users, AI (scaffolds; defer heavy AI until post-MVP)

## Domain sketch (initial entities)

- **User** — identity; owns campaigns
- **Campaign** — container for characters, lore, tags
- **Character** — PCs/NPCs with notes
- **LoreEntry** — markdown body, title, belongs to a campaign
- **Tag** — scoped per campaign; many-to-many with lore entries

Later: `TimelineEvent`, `World`, richer relationships, and ingestion pipelines.

## Related documents

- [Roadmap](./roadmap.md) — phases and MVP scope
- [Technical decisions](./tech-decisions.md) — stack and open choices
- [Architecture Decision Records (ADRs)](./adr/) — formal decisions (monorepo, PostgreSQL, modular API)
