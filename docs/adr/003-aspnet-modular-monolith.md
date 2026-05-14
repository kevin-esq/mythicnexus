# ADR 003: Modular monolith API on ASP.NET Core

## Status

Accepted.

## Context

The backend must grow by domain (campaigns, lore, search, users) without becoming a “big ball of mud” or splitting into premature microservices.

## Decision

Maintain a **single deployable** (modular monolith) with **vertical modules** under `apps/api/src/MythicNexus.Api/Modules/*`, each registering services and endpoints; **Domain**, **Application**, and **Infrastructure** live as separate projects under `apps/api/src/` (`MythicNexus.Domain`, `MythicNexus.Application`, `MythicNexus.Infrastructure`).

## Consequences

- **Positive**: clear boundaries, simple deployment, optional extraction to external services later.
- **Negative**: discipline is required to avoid incorrect cross-module dependencies (enforce via code review).

## References

- [docs/architecture.md](../architecture.md)
- [ADR 005](./005-backend-solution-layout.md) — physical `apps/api/src/` layout and solution shape
