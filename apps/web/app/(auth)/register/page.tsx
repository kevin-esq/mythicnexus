import Link from "next/link";

export default function RegisterPage() {
  return (
    <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 p-8 shadow-xl shadow-black/40">
      <h1 className="text-xl font-semibold text-zinc-50">Crear cuenta</h1>
      <p className="mt-2 text-sm text-zinc-400">Registro real se implementará con el flujo de auth del backend.</p>
      <form className="mt-6 flex flex-col gap-4" action="#" method="post">
        <label className="flex flex-col gap-1 text-sm text-zinc-300">
          Email
          <input
            type="email"
            name="email"
            autoComplete="email"
            className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
            placeholder="tu@email.com"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm text-zinc-300">
          Contraseña
          <input
            type="password"
            name="password"
            autoComplete="new-password"
            className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
          />
        </label>
        <button
          type="button"
          className="mt-2 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-violet-500"
        >
          Crear cuenta (próximamente)
        </button>
      </form>
      <p className="mt-6 text-center text-sm text-zinc-500">
        ¿Ya tienes cuenta?{" "}
        <Link href="/login" className="font-medium text-violet-400 hover:text-violet-300">
          Iniciar sesión
        </Link>
      </p>
    </div>
  );
}
