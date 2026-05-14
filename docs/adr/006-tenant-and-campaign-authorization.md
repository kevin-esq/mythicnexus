# ADR 006: Tenant vs campaign authorization (two-layer RBAC)

- **Status**: Accepted  
- **Date**: 2026-05-14  

## Context

MythicNexus is a multi-tenant SaaS-shaped product with collaborative RPG campaigns. A single global “role” per user conflates:

- **Workspace** concerns: billing, inviting org members, deleting any campaign in the tenant, audit.
- **Table** concerns: playing a PC, editing a character sheet, running a session, DM notes.

Real products (Notion, Discord, Jira) separate **organization roles** from **resource-scoped roles**.

## Decision

1. Introduce **`TenantMembership`** + **`TenantRole`** for workspace-level RBAC.
2. Introduce **`CampaignMember`** + **`CampaignRole`** for in-campaign participation, independent of tenant role.
3. Add **`Character.OwnerUserId`** for per-resource ownership and future private sheets / auditing.
4. Add **`Campaign.TenantId`** so every campaign belongs to exactly one tenant for consistent workspace checks.
5. Encode permission matrices in **`TenantCapabilityRules`** and **`CampaignCapabilityRules`**, with **`ITenantPermissionService`** / **`ICampaignPermissionService`** loading roles from the database. Handlers and endpoints consume those services instead of ad-hoc string or role checks.

## Consequences

- **Positive**: Clear extension path for hidden lore, DM-only notes, NPC ownership, and cross-tenant users (multiple `TenantMembership` rows).
- **Positive**: JWT can stay minimal; authoritative checks remain on the server from current DB state.
- **Negative**: More tables and joins; every campaign-mutating command must load tenant + campaign context (acceptable for correctness).
- **Migration**: Existing rows are backfilled so production-like databases remain consistent.

## Related documentation

- [authorization-rbac.md](../authorization-rbac.md)  
- [security-auth.md](../security-auth.md)  
