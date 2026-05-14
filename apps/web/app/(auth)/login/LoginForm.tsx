"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { ApiError } from "@/lib/api/client";
import { useAuth } from "@/lib/auth/AuthContext";

function loginErrorMessage(err: ApiError): string {
  if (err.code === "auth.email_not_confirmed") {
    return "Confirma tu email antes de entrar. Revisa la carpeta local email-outbox del API o reenvía el enlace desde registro.";
  }
  if (err.code === "auth.account_locked") {
    return "Cuenta bloqueada por varios intentos fallidos. Espera unos minutos e inténtalo de nuevo.";
  }
  return err.message;
}

export function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { login } = useAuth();

  const registered = searchParams.get("registered") === "1";
  const verified = searchParams.get("emailVerified");
  const hintEmail = searchParams.get("email") ?? "";

  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [pending, setPending] = useState(false);

  async function onSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const form = e.currentTarget;
    const fd = new FormData(form);
    const email = String(fd.get("email") ?? "").trim();
    setError(null);
    setPending(true);
    try {
      await login({ email, password });
      router.replace("/dashboard");
    } catch (err) {
      if (err instanceof ApiError) {
        setError(loginErrorMessage(err));
      } else {
        setError("No se pudo iniciar sesión. Intenta de nuevo.");
      }
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 p-8 shadow-xl shadow-black/40">
      <h1 className="text-xl font-semibold text-zinc-50">Iniciar sesión</h1>
      <p className="mt-2 text-sm text-zinc-400">Accede con tu cuenta MythicNexus.</p>
      {registered ? (
        <p className="mt-4 rounded-md border border-violet-900/50 bg-violet-950/40 px-3 py-2 text-sm text-violet-100">
          Cuenta creada. Revisa el correo de confirmación (en desarrollo: archivos en{" "}
          <code className="text-xs">apps/api/src/MythicNexus.Api/email-outbox</code>) y luego entra aquí.
        </p>
      ) : null}
      {verified === "1" ? (
        <p className="mt-4 rounded-md border border-emerald-900/50 bg-emerald-950/40 px-3 py-2 text-sm text-emerald-100">
          Email confirmado. Ya puedes iniciar sesión.
        </p>
      ) : null}
      {verified === "0" ? (
        <p className="mt-4 rounded-md border border-amber-900/50 bg-amber-950/40 px-3 py-2 text-sm text-amber-100">
          El enlace de confirmación no es válido o expiró. Solicita uno nuevo desde registro.
        </p>
      ) : null}
      {error ? (
        <p className="mt-4 rounded-md border border-red-900/50 bg-red-950/40 px-3 py-2 text-sm text-red-200" role="alert">
          {error}
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
            defaultValue={hintEmail}
            className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
            placeholder="tu@email.com"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm text-zinc-300">
          Contraseña
          <input
            type="password"
            name="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-zinc-50 outline-none ring-violet-500 focus:ring-2"
          />
        </label>
        <button
          type="submit"
          disabled={pending}
          className="mt-2 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-violet-500 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {pending ? "Entrando…" : "Entrar"}
        </button>
      </form>
      <p className="mt-4 text-center text-sm">
        <Link href="/forgot-password" className="font-medium text-violet-400 hover:text-violet-300">
          ¿Olvidaste tu contraseña?
        </Link>
      </p>
      <p className="mt-6 text-center text-sm text-zinc-500">
        ¿No tienes cuenta?{" "}
        <Link href="/register" className="font-medium text-violet-400 hover:text-violet-300">
          Registrarse
        </Link>
      </p>
    </div>
  );
}
