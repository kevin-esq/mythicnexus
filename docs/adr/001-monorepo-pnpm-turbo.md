# ADR 001: Monorepo with pnpm and Turborepo

## Status

Accepted.

## Context

A single repository is required for the Next.js frontend, the .NET backend, and future shared packages, with coordinated `dev` and `build` workflows.

## Decision

Adopt **pnpm workspaces** (`apps/*`, `packages/*`) and **Turborepo** for `dev`, `build`, and `lint`, with a single root `pnpm-lock.yaml`.

## Consequences

- **Positive**: deduplicated installs, Turbo build cache, unified commands from the repository root.
- **Negative**: contributors must use the pnpm version declared in `packageManager`; native dependency builds (`sharp`, etc.) may require `pnpm approve-builds` under pnpm 11 policies.

## References

- [docs/tech-decisions.md](../tech-decisions.md)
