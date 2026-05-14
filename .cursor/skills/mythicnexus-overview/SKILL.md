---
name: mythicnexus-overview
description: >-
  Maps the MythicNexus monorepo (pnpm, Turborepo, apps/web, apps/api, docs),
  dev commands, security rules for env files, and Supabase MCP usage. Use
  when onboarding, planning work across apps, CI, or configuring local/remote
  Postgres and Cursor tooling.
---

# MythicNexus — repository overview

## What this is

A **pnpm** + **Turborepo** monorepo for **MythicNexus** (RPG knowledge, campaigns, and search). Vision and MVP live in `docs/architecture.md` and `docs/roadmap.md`; stack choices in `docs/tech-decisions.md`.

## Layout

| Path                        | Purpose                                                                                                                                    |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `apps/web`                  | Next.js (App Router), Tailwind v4, TanStack Query                                                                                          |
| `apps/api`                  | ASP.NET Core 9, EF Core, Npgsql, vertical modules                                                                                          |
| `packages/`                 | Shared TypeScript libraries (empty until packages are added)                                                                               |
| `infrastructure/`           | Docker/Postgres notes and EF commands                                                                                                      |
| `docker-compose.yml`        | **Single** file at repo root: Postgres 16 (`mythic` / `mythicnexus`); shared services—not one compose per app unless isolation is required |
| `.config/dotnet-tools.json` | `dotnet-ef` (run `dotnet tool restore` from the **repository root**)                                                                       |

## Common commands

- Install dependencies: **`pnpm install`** at the root (workspaces `apps/*`, `packages/*`).
- Parallel dev: **`pnpm dev`** → `turbo dev` (uses `dev` scripts in `apps/web` and `apps/api/package.json`).
- Build: **`pnpm build`**.
- **pnpm 11**: if native dependency scripts fail (`sharp`, `unrs-resolver`), this repo lists `pnpm.onlyBuiltDependencies`; fresh clones may need **`pnpm approve-builds --all`**.
- **`packageManager`**: `pnpm@11.1.1` in the root `package.json` (expected by Turbo).

## Secrets and Git

- **Do not commit** `.env`, `.env.local`, or production passwords in `appsettings*.json`.
- Root `.gitignore` excludes `bin/`, `obj/`, `.turbo`, `.next`, etc.; committed templates use **`*.example`**.
- API secrets: **`apps/api/.env`** (local); web: **`apps/web/.env.local`**.

## Supabase (Cursor MCP)

- MCP server identifier in Cursor: **`plugin-supabase-supabase`** (not the short alias `supabase` if the client rejects it).
- Useful tools: `list_projects`, `get_project`, `get_project_url`, `execute_sql`; use MCP migrations only if team policy allows.
- Npgsql connection strings remain in **local `.env`**; MCP is for inspection, ad hoc SQL, and public URLs—**not** a substitute for committing secrets.

## VS Code / Cursor

- .NET debugging: `.vscode/launch.json` targets `apps/api/bin/Debug/net9.0/api.dll` with `cwd` `apps/api`.

## Related skills

- API / EF / environment: `mythicnexus-backend`.
- Next.js / UI / Query: `mythicnexus-frontend`.

## Product principles

- MVP: **searchable knowledge**; defer heavy AI until the domain and search baseline are solid (`docs/architecture.md`).
- Keep changes **scoped to the request**; follow existing style per application.
