"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { resetPassword } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";

export default function ResetPasswordPage() {
  const router = useRouter();

  const [token, setToken] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);
  const [done, setDone] = useState(false);

  useEffect(() => {
    const p = new URLSearchParams(window.location.search);
    const t = p.get("token");
    if (t) {
      setToken(t);
    }
  }, []);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setPending(true);
    try {
      await resetPassword({ token, newPassword: password });
      setDone(true);
      setTimeout(() => router.replace("/login"), 2000);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("No se pudo actualizar la contraseña.");
      }
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 p-8 shadow-xl shadow-black/40">
      <h1 className="text-xl font-semibold text-zinc-50">Nueva contraseña</h1>
      <p className="mt-2 text-sm text-zinc-400">
        Elige una contraseña fuerte (mín. 12 caracteres, mayúsculas, minúsculas, número y símbolo).
      </p>
      {error ? (
        <p className="mt-4 rounded-md border border-red-900/50 bg-red-950/40 px-3 py-2 text-sm text-red-200" role="alert">
          {error}
        </p>
      ) : null}
      {done ? (
        <p className="mt-4 rounded-md border border-emerald-900/50 bg-emerald-950/40 px-3 py-2 text-sm text-emerald-100">
          Contraseña actualizada. Redirigiendo al inicio de sesión…
        </p>
      ) : (
        <form className="mt-6 flex flex-col gap-4" onSubmit={onSubmit}>
          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            Token (por si el enlace no lo rellenó)
            <input
              type="text"
              name="token"
              autoComplete="off"
              required
              value={token}
              onChange={(e) => setToken(e.target.value)}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 font-mono text-xs text-zinc-50 outline-none ring-violet-500 focus:ring-2"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            Nueva contraseña
            <input
              type="password"
              name="password"
              autoComplete="new-password"
              required
              minLength={12}
              maxLength={200}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
            />
          </label>
          <button
            type="submit"
            disabled={pending || !token}
            className="mt-2 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-violet-500 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {pending ? "Guardando…" : "Guardar contraseña"}
          </button>
        </form>
      )}
      <p className="mt-6 text-center text-sm text-zinc-500">
        <Link href="/login" className="font-medium text-violet-400 hover:text-violet-300">
          Volver al inicio de sesión
        </Link>
      </p>
    </div>
  );
}
