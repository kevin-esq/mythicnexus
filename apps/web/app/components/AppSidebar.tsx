"use client";

import Link from "next/link";
import { useAuth } from "@/lib/auth/AuthContext";

const nav = [
  { href: "/dashboard", label: "Inicio" },
  { href: "/dashboard/campaigns", label: "Campañas" },
  { href: "/dashboard/lore", label: "Lore" },
];

export function AppSidebar() {
  const { user, logout } = useAuth();

  return (
    <aside className="flex w-56 shrink-0 flex-col border-r border-zinc-800 bg-zinc-950/80 px-4 py-6">
      <Link href="/dashboard" className="mb-8 text-lg font-semibold tracking-tight text-zinc-50">
        MythicNexus
      </Link>
      <nav className="flex flex-col gap-1">
        {nav.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            className="rounded-md px-3 py-2 text-sm text-zinc-300 transition hover:bg-zinc-800 hover:text-white"
          >
            {item.label}
          </Link>
        ))}
      </nav>
      <div className="mt-auto flex flex-col gap-3 border-t border-zinc-800 pt-4 text-sm">
        <div className="px-1 text-xs text-zinc-500">
          <p className="truncate font-medium text-zinc-300">{user?.username}</p>
          <p className="truncate text-zinc-500">{user?.email}</p>
          {user && !user.emailConfirmed ? (
            <p className="mt-2 rounded border border-amber-800/50 bg-amber-950/30 px-2 py-1 text-amber-100">
              Email sin confirmar: revisa el buzón local del API o reenvía desde registro.
            </p>
          ) : null}
        </div>
        <button
          type="button"
          onClick={() => logout()}
          className="rounded-md px-3 py-2 text-left text-zinc-400 transition hover:bg-zinc-800 hover:text-white"
        >
          Cerrar sesión
        </button>
      </div>
    </aside>
  );
}
