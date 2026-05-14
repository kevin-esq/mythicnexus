# ADR 002: PostgreSQL as the sole database engine; Supabase as a hosted option

## Status

Accepted.

## Context

The product requires relational data, advanced search (full-text in the MVP), and an evolutive model. SQLite is not a fit.

## Decision

Use **PostgreSQL** exclusively. Local development may use **Docker Compose** at the repository root; shared or cloud environments may use **Supabase** (managed Postgres) with the same API and EF Core migrations.

## Consequences

- **Positive**: single SQL dialect, mature Npgsql driver, optional `pgvector` on the same engine later.
- **Negative**: every environment needs a valid connection string (local or remote); Supabase requires TLS (`SSL Mode=Require`).

## References

- [docker-compose.yml](../../docker-compose.yml)
- [docs/tech-decisions.md](../tech-decisions.md)
