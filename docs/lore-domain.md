# Lore domain (design reference)

This document is the **human-readable** companion to [ADR 008 — Lore domain model](./adr/008-lore-domain-model.md). It describes how MythicNexus treats **lore as a knowledge graph inside a campaign**, without binding the product to any single RPG ruleset.

## Goals

- **Campaign-scoped knowledge**: every lore entry, tag, and relation belongs to exactly one `CampaignId` (and therefore one tenant via the campaign).
- **Interconnected entries**: directed edges, typed relations, and **backlinks** derived from those edges.
- **Progressive disclosure**: drafts, visibility hints, and authorization layers work together; visibility is **metadata**, not a substitute for server-side permission checks.

## Current model (baseline)

The codebase already includes:

- **`LoreEntry`**: `CampaignId`, `Title`, `Slug`, optional `Summary`, `ContentMarkdown`, `CreatedByUserId`, timestamps, soft delete (`DeletedAt`), audit (`UpdatedByUserId`).
- **`LoreRelation`**: **directed** edge `Source` → `Target` within a campaign, plus `RelationType` (today a string; see ADR 008 for enum evolution).
- **`Tag`**: **campaign-scoped** (`CampaignId` + `Name`), many-to-many with lore entries.

Unique slug per campaign is already enforced at the persistence layer (`(CampaignId, Slug)`).

## Target `LoreEntry` shape (knowledge node)

A lore entry should read as a **knowledge node**, not a disposable text note. Beyond the fields already in code, the model should converge on:

| Field | Role |
|-------|------|
| `Id` | Primary key |
| `CampaignId` | **Always** present; tenancy and permissions scope from here |
| `Title` | Display name |
| `Slug` | Stable URL segment; unique with `CampaignId` |
| `Summary` | Short blurb for lists and cards |
| `ContentMarkdown` | Body (editor-backed markdown) |
| `Status` | `Draft` / `Published` / `Archived` |
| `Visibility` | `Public` / `CampaignMembers` / `DungeonMastersOnly` (metadata; policies enforce access) |
| `CreatedByUserId` / `UpdatedByUserId` | Audit |
| `CreatedAt` / `UpdatedAt` / `DeletedAt` | Timestamps + soft delete |

**Future-proof (documented, not required in v1):** optional `Excerpt` (curated preview) and PostgreSQL `tsvector` / **`SearchVector`** when the search sprint lands—avoid premature columns until FTS is implemented.

## Planned extensions (next implementation passes)

| Area | Intent |
|------|--------|
| **Status** | `Draft`, `Published`, `Archived` on `LoreEntry` for prep, release, and moderation. |
| **Visibility** | `LoreVisibility` on `LoreEntry` as **metadata** (`Public` / `CampaignMembers` / `DungeonMastersOnly` style); **real access** still enforced by campaign + lore permission services. |
| **Relation typing** | Replace free-form `RelationType` string with a **`LoreRelationType` enum** (or a constrained vocabulary mapped to enum) for queries, UI, and future search. |
| **Backlinks (MVP)** | Compute **incoming** relations with a normal query over `LoreRelation` where `TargetLoreEntryId = id`. **No** dedicated backlink materialization table until usage proves it. |
| **Mentions / wiki links** | Future: `[[slug]]` (or similar) should resolve to **structured references** that create or update `LoreRelation` rows—not a one-off regex hack without slug integrity. |
| **Search (later sprint)** | PostgreSQL **full-text** over title + markdown + tags; optional `tsvector` / excerpt columns when that sprint lands. |

## UX and frontend (after backend contracts)

- **Markdown-first editor** (e.g. **TipTap**) for `ContentMarkdown`; defer collaborative/CRDT editing.
- **Related entries** panel driven by outgoing + incoming relations (UI may present inverse labels even though storage is directed).
- **Clean URLs**: prefer `/dashboard/campaigns/{id}/lore/{slug}` (or a future campaign slug segment) over opaque numeric-only routes in the product shell.

## Non-goals (near term)

- Vector databases, embeddings, AI summarization, semantic search **before** structured lore + Postgres FTS.
- D&D-specific entity types in the core domain (spells, monsters as first-class tables). External corpora stay behind the ingestion boundary described in [ADR 007](./adr/007-external-content-ingestion-strategy.md).

## Implementation order (recommended)

1. Evolve **domain + EF** (`Status`, `Visibility`, relation enum).
2. **Lore module** endpoints + **policies** (reuse campaign membership and tenant context).
3. **Tests** (permission matrix, slug uniqueness, relation integrity).
4. **Frontend**: real lore list/detail/editor and relation/tag UI on the existing campaign shell.

See ADR 008 for formal decisions and consequences.
