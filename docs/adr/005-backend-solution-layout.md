# ADR 005: Backend solution layout and developer ergonomics

## Status

Accepted.

## Context

The backend started with a flatter layout under `apps/api/`. We want a **Visual Studio–friendly** structure: one **`.sln`** at `apps/api/`, **all production `.csproj` files under `apps/api/src/`** (including the web host), **test projects under `apps/api/tests/`**, and solution **folders** (`src`, `tests`) for navigation. Contributors also run the API from **pnpm**, **VS Code**, and **CLI**; optional **`apps/api/.env`** must still resolve when the process working directory or build output path differs.

## Decision

1. **Single solution** — `apps/api/MythicNexus.Api.sln` builds every C# project; no standalone host project file at `apps/api/` root (only the solution + package metadata live there).
2. **Production under `src/`** — `MythicNexus.Api` (SDK Web), `MythicNexus.Domain`, `MythicNexus.Application`, `MythicNexus.Infrastructure`, each in its own folder under `apps/api/src/`. Vertical modules stay on the host under `MythicNexus.Api/Modules/` (modular monolith; see [ADR 003](./003-aspnet-modular-monolith.md)).
3. **`LocalEnvLoader`** — keep loading `apps/api/.env` for local secrets by probing multiple paths (including upward from `AppContext.BaseDirectory`) so a host built under `src/MythicNexus.Api/bin/...` still finds `.env` next to the solution when appropriate.
4. **Tooling alignment** — `apps/api/package.json` (`dev`, `build`, `test`), `apps/api/turbo.json` outputs for `src/**` and `tests/**`, root `turbo.json` / `package.json` **`pnpm test`**, and `.vscode` launch/tasks point at the host under `src/MythicNexus.Api/`.

## Consequences

- **Positive**: matches common VS habits; clearer separation of product code vs tests; CLI and IDE entry points stay consistent.
- **Negative**: paths in docs and scripts must stay in sync when moving projects (prefer updating ADR + architecture + skills together).

## References

- [docs/architecture.md](../architecture.md)
- [ADR 003](./003-aspnet-modular-monolith.md) — modular monolith (what); this ADR — repo layout (where).
- [ADR 004](./004-backend-testing-layout.md) — test projects under `tests/`.
