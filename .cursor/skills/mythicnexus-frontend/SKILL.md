---
name: mythicnexus-frontend
description: >-
  Guides Next.js App Router work in apps/web: route groups, dashboard shell,
  Tailwind v4, TanStack Query providers, env vars, and API base URL patterns.
  Use when editing React pages, layouts, client providers, or web env config.
---

# MythicNexus — frontend (`apps/web`)

## Stack

- **Next.js** (App Router), **React 19**, **TypeScript** `strict`, **Tailwind CSS v4** (`@import "tailwindcss"` in `app/globals.css`).
- **TanStack Query**: `QueryClientProvider` in `app/providers.tsx`; wrap client routes that fetch remote data.
- Path alias **`@/*`** maps to the web app root (e.g. imports under `@/app/...` per repository convention).

## Routes and layout

- **`app/layout.tsx`**: root layout, dark theme (`className="dark"` on `<html>`), Geist fonts, `<Providers>`.
- **`app/(auth)/`**: lightweight layout for sign-in and sign-up (no sidebar).
- **`app/dashboard/`**: layout with **`AppSidebar`** and main content.
- Marketing/landing: `app/page.tsx`.

When adding pages, reuse existing Tailwind spacing and color tokens (`bg-zinc-950`, `zinc-800` borders, violet accents) until a component library such as shadcn/ui is introduced.

## Environment variables

- Local overrides: **`.env.local`** (gitignored). Templates: `.env.example`, `.env.local.example`.
- Backend URL: **`NEXT_PUBLIC_API_URL`** (e.g. `http://localhost:5118`, matching the API HTTP profile in `launchSettings.json`).
- Expose only `NEXT_PUBLIC_*` variables to the client bundle.

## Data fetching

- Prefer **TanStack Query** (`useQuery` / `useMutation`) for HTTP calls; build URLs with `process.env.NEXT_PUBLIC_API_URL` and versioned `/api/...` paths when available.
- Default to Server Components; add `"use client"` only for state, events, or Query hooks.

## Quality

- Run `pnpm lint` via the `web` workspace or Turbo from the root.
- Define SEO `metadata` in layouts when adding major sections.

## Related skills

- Monorepo, secrets, Supabase MCP: `mythicnexus-overview`.
- HTTP contracts and backend errors: `mythicnexus-backend`.
