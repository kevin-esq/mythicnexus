import Link from "next/link";

export default function HomePage() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-8 px-6 py-24 text-center">
      <div className="max-w-xl space-y-4">
        <p className="text-sm font-medium uppercase tracking-[0.2em] text-violet-400">MythicNexus</p>
        <h1 className="text-balance text-3xl font-semibold text-zinc-50 sm:text-4xl">
          Plataforma de conocimiento y campañas para tu mesa
        </h1>
        <p className="text-pretty text-zinc-400">
          Fundación lista: monorepo, Next.js, API .NET 9 con PostgreSQL y modelo inicial. El MVP apunta a lore
          buscable, campañas y auth — sin IA hasta que el dominio y la búsqueda estén sólidos.
        </p>
      </div>
      <div className="flex flex-wrap items-center justify-center gap-3">
        <Link
          href="/dashboard"
          className="rounded-full bg-violet-600 px-6 py-2.5 text-sm font-medium text-white transition hover:bg-violet-500"
        >
          Ir al panel
        </Link>
        <Link
          href="/login"
          className="rounded-full border border-zinc-700 px-6 py-2.5 text-sm font-medium text-zinc-100 transition hover:border-zinc-500 hover:bg-zinc-900"
        >
          Iniciar sesión
        </Link>
      </div>
    </div>
  );
}
