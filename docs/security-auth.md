# Security and authentication (MythicNexus API)

This document summarizes **how MythicNexus handles auth today** and how it maps to common **cybersecurity** guidance. It complements [database-local.md](./database-local.md) and [ADR 002](./adr/002-postgresql-supabase.md).

## Where data lives (not Supabase Auth)

- **Users, tenants, login audit, and tokens** are stored in **your PostgreSQL database** via **EF Core** (the same instance you configure with `ConnectionStrings:Default`).
- **Supabase** (if you use it) is only another Postgres host for that connection string. We do **not** delegate identity to Supabase Auth; you keep full control in the API schema.

## Passwords

- **Hashing**: BCrypt with an elevated work factor (12). Only the **hash** is stored.
- **Policy** (registration / reset): minimum **12** characters, at least one **uppercase**, **lowercase**, **digit**, and **symbol** (FluentValidation). This reduces guessable passwords and aligns with common organizational baselines (length over exotic rotation rules).
- **Timing**: On failed login, the code still runs BCrypt verification against a **dummy hash** when the user row is missing, so verification work is less trivially correlated with “user exists” (defense in depth; not a substitute for uniform error messages, which we also use for failed login).

## Account lockout

- After repeated **failed password checks**, the account enters **lockout** for a configurable window (`Auth:Lockout` in configuration).
- Locked accounts receive **HTTP 423** with a dedicated error code (`auth.account_locked`) so the client can show a clear message without revealing whether the email exists on **forgot-password** flows (those stay generic).

## Email verification

- New registrations set **`EmailConfirmed = false`** and receive a **one-time token** (only a **SHA-256 hash** of the opaque token is stored).
- Until confirmed, **login is rejected** with **`403`** and `auth.email_not_confirmed`.
- **Development**: `IEmailOutbox` writes **`.txt` drops** under `email-outbox` (see `Email:LocalOutbox:RelativeDirectory` and `.gitignore`). Replace with SMTP / provider later without changing the domain services.

## Forgot password

- **POST** `/api/users/forgot-password` always responds with a **generic success message** (no enumeration of registered emails).
- Internally, if a user exists, previous active reset tokens are **invalidated**, a new token is stored as a **hash**, and an outbox message is written with a link to the **web** reset page (`Auth:PublicUrls:WebBaseUrl`).

## Rate limiting

Separate **fixed-window** policies (per client IP):

| Policy            | Typical use                         | Default window / limit        |
| ----------------- | ----------------------------------- | ----------------------------- |
| `auth_login`      | `/api/users/login`                  | 10 / minute                   |
| `auth_register`   | `/api/users/register`             | 5 / 10 minutes                |
| `auth_recovery`   | forgot / reset / resend-verification | 3 / 15 minutes             |
| `auth_verify`     | GET verify-email                    | 30 / minute                   |

Tune in `Program.cs` as traffic patterns evolve.

## Login audit trail

- **`LoginAuditEvents`** stores timestamp, normalized email, success flag, optional failure reason, **IP** (from `HttpContext.Connection`), **User-Agent** (truncated), optional `UserId` / `TenantId`.
- **Geo-location** is **not** inferred automatically today; see the **Hardening roadmap** below for GeoIP / tenant admin direction.

## JWT access tokens

- Claims include **`tenant_id`** and **`email_verified`** for authorization decisions in modules.
- **Refresh tokens** and **server-side session revocation** are **not** implemented yet; treat access tokens as short-lived (see `Jwt:AccessTokenExpirationMinutes`) and add refresh / revocation when you harden sessions further.

## Authorization (two layers)

Workspace (**tenant**) roles and **campaign** roles are **separate**. A `TenantRole.Viewer` does **not** mean the user cannot play; it means **no workspace administration**. In-campaign actions use `CampaignMember` / `CampaignRole` and resource ownership (e.g. `Character.OwnerUserId`). See **[authorization-rbac.md](./authorization-rbac.md)** and **[ADR 006](./adr/006-tenant-and-campaign-authorization.md)**.

## Tenants

- Each registration creates a **`Tenant`** (workspace), sets `User.TenantId`, and inserts **`TenantMembership`** with **`TenantRole.Owner`** for that user.
- A **legacy tenant** row exists for users predating this feature (migration seed).
- **GET** `/api/tenants/current` (authenticated) returns the current user’s tenant summary — foundation for future **tenant admin** UIs and row-level isolation on campaigns/lore.

## Operational checklist

1. Set **`Auth:PublicUrls`** in every environment (API + web URLs used in emails and redirects).
2. Keep **`Jwt:SecretKey`** long and random; never commit production secrets.
3. Behind a reverse proxy, enable **forwarded headers** (`UseForwardedHeaders` + known proxy networks) so `RemoteIpAddress` and future rate limits use the **real client IP** instead of the proxy’s.

## Hardening roadmap (not implemented yet)

| Area | Direction |
| ---- | ---------- |
| **Sessions** | **Refresh tokens** stored server-side (hashed), **revocation**, **JWT rotation** (new access token per refresh, reuse detection). |
| **MFA** | **TOTP** and/or **WebAuthn** for high-value accounts and optional workspace policy. |
| **Risk / admin** | **GeoIP** enrichment on `LoginAuditEvents` (e.g. MaxMind GeoLite2) and a **tenant admin** UI for members, roles, and audit review. |
| **Hosting** | **Forwarded headers** behind TLS terminators and CDNs (see checklist above). |

## References

- OWASP Authentication Cheat Sheet (password storage, MFA, session management).
- NIST SP 800-63B (memorized secrets) — informs length/complexity trade-offs.
