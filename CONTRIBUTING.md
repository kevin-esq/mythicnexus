# Contributing to MythicNexus

## Branch strategy

- **`main`**: stable, release-ready code.
- **`develop`**: integration branch when you use a GitFlow-style flow (optional for solo work).
- **`feature/*`**, **`fix/*`**, **`docs/*`**: short-lived branches scoped to one concern.

Keep pull requests **small and focused** (one feature or fix per PR when possible).

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/) so history and changelogs stay readable:

| Prefix      | Use for                                                          |
| ----------- | ---------------------------------------------------------------- |
| `feat:`     | New user-facing behavior or API surface                          |
| `fix:`      | Bug fixes                                                        |
| `docs:`     | Documentation only                                               |
| `refactor:` | Internal change without behavior change                          |
| `test:`     | Tests only                                                       |
| `chore:`    | Tooling, CI, formatting, deps bumps without product logic change |
| `ci:`       | GitHub Actions / pipeline only                                   |

Examples:

- `feat(api): add campaign members endpoint`
- `fix(web): validate login form before submit`
- `docs: expand Supabase setup in database-local`
- `ci: run dotnet test against Postgres service`

## Pull requests

1. Open a PR against `main` (or `develop` if your team uses it).
2. Fill in **`.github/pull_request_template.md`** (summary, changes, technical notes, testing).
3. Ensure **CI** is green (`.github/workflows/ci.yml`).
4. Link related issues with `Closes #123` when applicable.

## Local development

- **Monorepo**: `pnpm` at the root; `turbo` orchestrates `apps/web` and `apps/api`.
- **API**: .NET 9 solution under `apps/api/MythicNexus.Api.sln`. See `.cursor/skills/mythicnexus-backend/SKILL.md` and `docs/database-local.md`.
- **Database**: Docker Postgres at the repo root or hosted PostgreSQL (e.g. Supabase). Migrations live in `MythicNexus.Infrastructure`.

## Security

Do **not** commit secrets (`.env`, production connection strings, JWT keys). Use `*.example` files and CI secrets for deployment.
