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

## Domain sketch (current entities)

- **User** — identity with unique `Username`; owns campaigns; can author lore entries (`CreatedBy` on `LoreEntry`).
- **Campaign** — container for characters, lore, tags, and **lore relations** (knowledge graph edges scoped to the campaign).
- **Character** — PCs/NPCs with optional `Race`, `Class`, `Backstory`, and `Notes`.
- **LoreEntry** — `Title`, `Slug` (unique per campaign), optional `Summary`, `ContentMarkdown`, `CreatedByUserId`; many-to-many with tags.
- **LoreRelation** — directed edge between two lore entries in the same campaign (`RelationType` string, e.g. `references`, `contradicts`); unique on `(CampaignId, Source, Target, RelationType)`.
- **Tag** — scoped per campaign; many-to-many with lore entries.

Later: `TimelineEvent`, `World`, ingestion pipelines, and semantic search over this graph.

## Related documents

- [Roadmap](./roadmap.md) — phases and MVP scope
- [Technical decisions](./tech-decisions.md) — stack and open choices
- [Architecture Decision Records (ADRs)](./adr/) — formal decisions (monorepo, PostgreSQL, modular API)
