# ADR 007: External content ingestion (source-agnostic, no D&D Beyond coupling)

- **Status**: Accepted  
- **Date**: 2026-05-14  

## Context

Third-party RPG corpora (SRDs, community datasets, APIs) are useful **bootstrap** material, but they are not the product. A common failure mode is heavy investment in scraping, syncing, or vendor-specific schemas **before** the domain model, tenancy, campaigns, and lore graph are stable—resulting in “lots of data, weak product.”

[D&D 5e API](https://www.dnd5eapi.co/) (and similar **OGL-friendly** sources) can seed examples (spells, monsters, classes, races) without claiming MythicNexus is a D&D rules engine.

**D&D Beyond** is explicitly **out of scope** for near-term integration: licensing, ToS, HTML instability, and scraping risk outweigh value while campaign and lore cores are still maturing.

## Decision

1. **Stay source-agnostic in the domain.** Persist knowledge as generic constructs already in the model (e.g. `LoreEntry`, tags, relations, campaign scope)—**not** as `DnDSpell`, `DnDMonster`, or other vendor-shaped aggregate roots.
2. **Do not couple the web app or core API handlers directly to external HTTP APIs** for rules content. Any optional fetch belongs in a dedicated **import / ingestion boundary** (future `ContentImport` module: providers → parsers → normalizers → persistence/jobs).
3. **Optional bootstrap only:** when implemented, use D&D 5e API (or other **explicitly legal** datasets) as **seed data** behind that boundary—normalized into our tables or intermediate DTOs—never as a hard runtime dependency for core flows.
4. **Explicit non-goals (now):** D&D Beyond integration, mass scraping, embeddings/vector DB as prerequisites for ingestion, and “rules engine completeness” as a product goal.
5. **Sequencing:** ship **Campaign core** and **Lore core** (markdown, tags, relationships, search) **before** building the ingestion pipeline. Track implementation work on branch `feature/content-import-foundation` when the time comes (CLI such as `dotnet run -- import:dnd5e` is acceptable as tooling, not as product UX in v1).

## Consequences

- **Positive:** Clear story for Pathfinder, homebrew, Markdown, PDFs, and custom worlds later—all through the same normalization funnel.
- **Positive:** Hiring and architecture narratives emphasize **platform engineering** (providers, jobs, provenance) rather than “wrapper around one API.”
- **Negative:** Early users do not get a giant preloaded spell DB; that is acceptable until lore and search prove value.
- **Operational:** Seed/import jobs should record **provenance** (source id, URL, license note) on imported rows when the pipeline exists—deferred to the ingestion sprint.

## Related documentation

- [Roadmap](../roadmap.md) — order: campaigns → lore → search → import  
- [Architecture](../architecture.md) — domain sketch and “later” ingestion note  
- [ADR 002](./002-postgresql-supabase.md) — persistence host  

## Reference architecture (future module)

When implemented, prefer a layout similar to:

```txt
Modules/ContentImport/
├── Providers/      # e.g. Dnd5eApiProvider, MarkdownProvider, JsonProvider
├── Parsers/        # raw payload → intermediate models
├── Normalizers/    # intermediate → MythicNexus-shaped records
└── Jobs/           # idempotent runs, import tracking
```

This ADR does **not** require that folder to exist until the ingestion sprint starts.
