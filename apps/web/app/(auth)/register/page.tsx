"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { ApiError } from "@/lib/api/client";
import { useAuth } from "@/lib/auth/AuthContext";

export default function RegisterPage() {
  const router = useRouter();
  const { register, resendVerification } = useAuth();
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);
  const [resent, setResent] = useState(false);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setPending(true);
    try {
      const result = await register({ email, username, password });
      if (result.requiresEmailVerification) {
        router.replace(`/login?registered=1&email=${encodeURIComponent(email)}`);
        return;
      }
      router.replace("/dashboard");
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("No se pudo crear la cuenta. Intenta de nuevo.");
      }
    } finally {
      setPending(false);
    }
  }

  async function onResend() {
    setError(null);
    setPending(true);
    try {
      await resendVerification(email);
      setResent(true);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("No se pudo reenviar el correo.");
      }
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 p-8 shadow-xl shadow-black/40">
      <h1 className="text-xl font-semibold text-zinc-50">Crear cuenta</h1>
      <p className="mt-2 text-sm text-zinc-400">
        Registro con email, usuario y contraseña fuerte (mín. 12 caracteres, mayúsculas, minúsculas, número y símbolo).
      </p>
      {error ? (
        <p className="mt-4 rounded-md border border-red-900/50 bg-red-950/40 px-3 py-2 text-sm text-red-200" role="alert">
          {error}
        </p>
      ) : null}
      {resent ? (
        <p className="mt-4 rounded-md border border-emerald-900/50 bg-emerald-950/40 px-3 py-2 text-sm text-emerald-100">
          Si la cuenta existe y falta confirmar, hemos generado otro enlace (revisa la carpeta local{" "}
          <code className="text-xs">email-outbox</code> del API en desarrollo).
        </p>
      ) : null}
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
            placeholder="tu@email.com"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm text-zinc-300">
          Usuario
          <input
            type="text"
            name="username"
            autoComplete="username"
            required
            minLength={2}
            maxLength={80}
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
            placeholder="tu_nombre"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm text-zinc-300">
          Contraseña
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
          <span className="text-xs text-zinc-500">
            Mínimo 12 caracteres; incluye mayúscula, minúscula, número y símbolo.
          </span>
        </label>
        <button
          type="submit"
          disabled={pending}
          className="mt-2 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-violet-500 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {pending ? "Creando…" : "Crear cuenta"}
        </button>
      </form>
      <p className="mt-4 text-center text-xs text-zinc-500">
        ¿No llegó el enlace?{" "}
        <button
          type="button"
          className="font-medium text-violet-400 hover:text-violet-300 disabled:opacity-50"
          disabled={pending || !email}
          onClick={() => void onResend()}
        >
          Reenviar confirmación
        </button>
      </p>
      <p className="mt-6 text-center text-sm text-zinc-500">
        ¿Ya tienes cuenta?{" "}
        <Link href="/login" className="font-medium text-violet-400 hover:text-violet-300">
          Iniciar sesión
        </Link>
      </p>
    </div>
  );
}
