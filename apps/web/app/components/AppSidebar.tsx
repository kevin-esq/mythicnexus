import Link from "next/link";

const nav = [
  { href: "/dashboard", label: "Inicio" },
  { href: "/dashboard/campaigns", label: "Campañas" },
  { href: "/dashboard/lore", label: "Lore" },
];

export function AppSidebar() {
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
      <div className="mt-auto flex flex-col gap-2 border-t border-zinc-800 pt-4 text-sm">
        <Link href="/login" className="text-zinc-400 hover:text-white">
          Iniciar sesión
        </Link>
        <Link href="/register" className="text-zinc-400 hover:text-white">
          Registrarse
        </Link>
      </div>
    </aside>
  );
}
