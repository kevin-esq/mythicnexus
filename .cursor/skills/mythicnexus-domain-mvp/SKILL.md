---
name: mythicnexus-domain-mvp
description: >-
  Describes MythicNexus MVP domain entities, relationships, and scope
  boundaries (campaigns, lore, tags, users). Use when designing schema, EF
  models, migrations, or API contracts for RPG knowledge features.
---

# MythicNexus — domain (MVP)

## Scope

Near-term goal: a **searchable knowledge platform** (organization, lore, campaigns, import). **Do not** prioritize embeddings, agents, or generative AI until the domain and full-text search baseline are defined (`docs/architecture.md`, `docs/roadmap.md`).

## Current entities (EF)

Defined under `apps/api/src/MythicNexus.Domain/Entities/` and configured in `MythicNexusDbContext`:

| Entity        | Role                                                                                      |
| ------------- | ----------------------------------------------------------------------------------------- |
| **User**      | Identity; owns campaigns (`OwnedCampaigns`).                                              |
| **Campaign**  | Aggregate for characters, lore, and tags.                                                 |
| **Character** | Character tied to a campaign; optional notes.                                             |
| **LoreEntry** | Title + markdown; belongs to a campaign; **many-to-many** with `Tag` via `LoreEntryTags`. |
| **Tag**       | Unique name **per campaign** (unique index on `CampaignId`, `Name`).                      |

## Modeling rules

- Keep **explicit foreign keys** and coherent delete behaviors (Cascade where defined; Restrict on campaign owner).
- Defer narrative concepts (`TimelineEvent`, `World`, etc.) until after the MVP unless the product owner explicitly expands scope.
- **Search**: plan indexes / `tsvector` in later migrations; do not embed ranking logic inside pure entities.

## API conventions

- Module-scoped prefixes under `/api/...` aligned with `Modules/*`.
- Separate request/response DTOs from entities when validation or projection requires it.

## Related skills

- Persistence and migrations: `mythicnexus-backend`.
- UI and routing: `mythicnexus-frontend`.
