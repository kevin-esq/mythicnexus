"use client";

import Link from "next/link";
import { useState } from "react";
import { forgotPassword } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [done, setDone] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setPending(true);
    try {
      await forgotPassword(email);
      setDone(true);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("No se pudo enviar la solicitud.");
      }
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 p-8 shadow-xl shadow-black/40">
      <h1 className="text-xl font-semibold text-zinc-50">Recuperar contraseña</h1>
      <p className="mt-2 text-sm text-zinc-400">
        Si existe una cuenta con ese email, recibirás instrucciones. En desarrollo, mira la carpeta{" "}
        <code className="text-xs text-zinc-500">email-outbox</code> del API.
      </p>
      {error ? (
        <p className="mt-4 rounded-md border border-red-900/50 bg-red-950/40 px-3 py-2 text-sm text-red-200" role="alert">
          {error}
        </p>
      ) : null}
      {done ? (
        <p className="mt-4 rounded-md border border-emerald-900/50 bg-emerald-950/40 px-3 py-2 text-sm text-emerald-100">
          Si la cuenta existe, se generó un mensaje con el enlace de restablecimiento.
        </p>
      ) : (
        <form className="mt-6 flex flex-col gap-4" onSubmit={onSubmit}>
          <label className="flex flex-col gap-1 text-sm text-zinc-300">
            Email
            <input
              type="email"
              name="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
            />
          </label>
          <button
            type="submit"
            disabled={pending}
            className="mt-2 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-violet-500 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {pending ? "Enviando…" : "Enviar enlace"}
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
