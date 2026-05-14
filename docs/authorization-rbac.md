# Authorization: tenant vs campaign RBAC

MythicNexus separates **organizational** permissions (the SaaS workspace) from **in-campaign** participation (the RPG table). They are **different layers**; one does not replace the other.

## Mental model

```text
User
  → Tenant membership (TenantRole)     … billing, invites, workspace admin
  → Campaign membership (CampaignRole) … play at the table, lore, sessions
  → Resource ownership                 … e.g. Character.OwnerUserId
```

A **`TenantRole.Viewer`** is **not** “globally read-only in the product”. It means **no workspace administration**. The same user can still be `CampaignRole.Player` and create or edit **their** character, subject to campaign rules.

## Domain types (Level 1 — workspace)

| Concept | Purpose |
| -------- | -------- |
| `Tenant` | Workspace / org boundary. |
| `TenantMembership` | `(TenantId, UserId, TenantRole)` — many users per tenant, many memberships per user across tenants (future). |
| `TenantRole` | `Owner`, `Admin`, `Member`, `Viewer` — billing, invites, org-wide campaign admin. |

`User.TenantId` remains the **primary / home** tenant for onboarding and JWT `tenant_id`. Authoritative roles for authorization live on **`TenantMembership`**.

## Domain types (Level 2 — campaign)

| Concept | Purpose |
| -------- | -------- |
| `Campaign` | Scoped to a tenant via **`Campaign.TenantId`**. |
| `CampaignMember` | `(CampaignId, UserId, CampaignRole)`. |
| `CampaignRole` | `DungeonMaster`, `CoDungeonMaster`, `Player`, `Viewer`. |

Legacy fallback: if no `CampaignMember` row exists but `Campaign.OwnerUserId` matches, the API treats the user as **`DungeonMaster`** (migration seeds a row for every existing owner).

## Character ownership

`Character` has **`OwnerUserId`** in addition to `CampaignId`. Sheets, private notes, and audits key off the owner. **Do not** infer ownership only from “character is in campaign X”.

Intended flow for `POST /campaigns/{id}/characters` (when implemented):

1. Resolve `CampaignRole` for the current user in that campaign.
2. Require `CampaignCapabilityRules.CanCreateCharacter(role)`.
3. Set `Character.OwnerUserId = currentUserId` (or a documented DM-on-behalf rule).

## Application layer (policy-style, not stringly-typed `if`s)

Pure rules (easy to unit test):

- `TenantCapabilityRules` — `CanManageWorkspace`, `CanInviteUsers`, `CanManageBilling`, `CanManageAllCampaignsInTenant`, etc.
- `CampaignCapabilityRules` — `CanCreateCharacter`, `CanManageSession`, `CanEditSharedLore`, **`CanDeleteCampaign(tenantRole, campaignRole)`** (combines both layers).

Database-backed services (scoped, use EF):

- `ITenantPermissionService` / `TenantPermissionService`
- `ICampaignPermissionService` / `CampaignPermissionService`

**Endpoints and command handlers** should call these services (or thin wrappers), not scatter `switch` on raw enums. When you adopt ASP.NET Core **resource-based** authorization, implement `IAuthorizationHandler` that delegates to the same services so policies and handlers stay one source of truth.

### Example: delete campaign

`CampaignCapabilityRules.CanDeleteCampaign` allows:

- **Tenant** `Owner` / `Admin` — org-wide delete; or
- **Tenant** `Member` **and** in-campaign **`DungeonMaster` or `CoDungeonMaster`** — DM can remove their table without being billing owner.

It **blocks** a **tenant `Viewer`** even if they were somehow marked DM in a campaign (mirrors “Kevin” scenario).

## Registration

On `Register`, after creating the tenant and user, the API inserts **`TenantMembership`** with **`TenantRole.Owner`** for that user and tenant.

## Migrations and data

Migration **`TenantCampaignRbacAndCharacterOwnership`**:

- Adds `TenantMemberships`, `CampaignMembers`.
- Adds `Campaigns.TenantId` (backfilled from `OwnerUserId` → `Users.TenantId`).
- Adds `Characters.OwnerUserId` (backfilled from campaign owner).
- Seeds membership rows for existing users and campaign owners.

## Related

- [Security & authentication](./security-auth.md)
- [ADR 006 — Tenant and campaign authorization](./adr/006-tenant-and-campaign-authorization.md)
