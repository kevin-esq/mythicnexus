# ADR 008: Lore domain model (knowledge graph in a campaign)

- **Status**: Accepted  
- **Date**: 2026-05-15  

## Context

MythicNexus is evolving from a **campaign shell** into a **multi-tenant knowledge platform**. Lore must behave like **interconnected knowledge nodes** inside a campaign: titles, slugs, markdown, tags, directed relationships, backlinks, and later search—not a flat “notes” table.

The repository already contains `LoreEntry`, `LoreRelation`, and `Tag` with campaign scoping for entries and tags, and **directed** relations between entries. This ADR **locks design intent** before the Lore Foundation sprint adds CRUD, editor UI, and search.

## Decision

1. **Campaign scope is mandatory for lore data.** Every `LoreEntry`, `LoreRelation`, and `Tag` row is tied to a **`CampaignId`**. Tenant isolation continues to flow from the campaign (never a “global lore” row in the core model).
2. **Slugs are unique per campaign**, not globally: composite uniqueness **`(CampaignId, Slug)`** (already reflected in persistence).
3. **Tags remain campaign-scoped** (`Tag.CampaignId` + name); uniqueness of tag name is **per campaign** to avoid cross-campaign pollution and to simplify filters.
4. **`LoreRelation` is stored as a directed edge only:** `SourceLoreEntryId` → `TargetLoreEntryId`, scoped by `CampaignId`, with a **typed** `LoreRelationType` (enum) replacing or strictly mapping today’s `string RelationType` for integrity and indexing. The UI may **render** inverse or symmetric language; the database does **not** duplicate an automatic reverse row unless we explicitly add a product rule later.
5. **`LoreEntry` gains lifecycle and disclosure metadata:** `LoreEntryStatus` (`Draft`, `Published`, `Archived`) and `LoreVisibility` (`Public`, `CampaignMembers`, `DungeonMastersOnly`). These fields guide UX and default queries; **authorization** (who may read/update) remains in **permission services / policies**, not in the enum alone. The long-term field set is documented in [Lore domain](../lore-domain.md) (including optional future `Excerpt` / full-text `SearchVector` when search ships).
6. **Backlinks (MVP):** compute **incoming** relations via query (`LoreRelation` where target = entry). **Do not** introduce a materialized backlink table until traffic or complexity justifies it.
7. **Wiki-style mentions later:** syntax such as `[[slug]]` must resolve through **structured reference handling** (slug validation, optional auto-relation) tied to `LoreRelation`, not a fragile standalone regex pipeline without integrity rules.
8. **Search is a separate sprint:** PostgreSQL full-text over title, markdown body, and tags; optional `tsvector` / excerpt columns when implemented. **No** vector DB or embeddings in the lore-foundation slice.
9. **Implementation order:** evolve **domain + EF migrations** first, then **API module (Lore)** with policies, then **tests**, then **frontend** (TipTap or equivalent markdown editor, related-entries panel). Defer realtime collaborative editing (CRDT/Yjs).

## Consequences

- **Positive:** Clear path to Obsidian/Notion-like behavior (graph, backlinks, tags) while staying **source-agnostic** for future imports ([ADR 007](./007-external-content-ingestion-strategy.md)).
- **Positive:** Directed graph storage keeps traversal, search, and future graph exports predictable.
- **Negative:** Some symmetric relation UX requires **two writes** or explicit product rules if we ever want mirrored edges; default is **one directed edge** plus UI sugar.
- **Migration:** Adding enums/columns to `LoreEntry` and changing `LoreRelation` typing requires a coordinated EF migration and backfill strategy for existing `RelationType` strings.

## Related documentation

- [Lore domain (narrative)](../lore-domain.md)  
- [Architecture](../architecture.md)  
- [Roadmap](../roadmap.md)  
- [ADR 007 — External content ingestion](./007-external-content-ingestion-strategy.md)  
