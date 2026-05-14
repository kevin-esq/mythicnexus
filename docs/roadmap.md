# Roadmap

## Foundation sprint (now)

- Monorepo with pnpm and Turborepo
- Next.js app shell, dark UI baseline, auth route placeholders
- ASP.NET Core 9 API with modular folders, EF Core, PostgreSQL
- Docker Compose for local Postgres
- Architecture and tech decision notes

## MVP: searchable knowledge platform

1. **Auth** — register, login, sessions or tokens (decision in `docs/tech-decisions.md`)
2. **Campaigns** — create campaigns, notes, characters
3. **Lore** — markdown entries, tags, relationships
4. **Search** — PostgreSQL full-text search on lore and notes
5. **Import** — markdown / JSON ingestion

Defer until after MVP: embeddings, agents, heavy AI features.
