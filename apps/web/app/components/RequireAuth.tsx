"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/lib/auth/AuthContext";

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { hydrated, accessToken, isSessionReady, sessionError, logout } = useAuth();

  useEffect(() => {
    if (!hydrated) return;
    if (!accessToken) {
      router.replace("/login");
    }
  }, [hydrated, accessToken, router]);

  if (!hydrated) {
    return (
      <div className="flex flex-1 items-center justify-center px-6 py-16 text-sm text-zinc-400">
        Cargando sesión…
      </div>
    );
  }

  if (!accessToken) {
    return (
      <div className="flex flex-1 items-center justify-center px-6 py-16 text-sm text-zinc-400">
        Redirigiendo al inicio de sesión…
      </div>
    );
  }

  if (sessionError) {
    return (
      <div className="mx-auto max-w-md rounded-xl border border-zinc-800 bg-zinc-900/60 px-6 py-8 text-center text-sm text-zinc-300">
        <p className="text-zinc-100">No se pudo validar la sesión.</p>
        <button
          type="button"
          onClick={() => {
            logout();
            router.replace("/login");
          }}
          className="mt-4 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500"
        >
          Volver a iniciar sesión
        </button>
      </div>
    );
  }

  if (!isSessionReady) {
    return (
      <div className="flex flex-1 items-center justify-center px-6 py-16 text-sm text-zinc-400">
        Verificando usuario…
      </div>
    );
  }

  return <>{children}</>;
}
