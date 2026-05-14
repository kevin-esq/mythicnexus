import { Suspense } from "react";
import { LoginForm } from "./LoginForm";

export default function LoginPage() {
  return (
    <Suspense
      fallback={
        <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 px-8 py-12 text-center text-sm text-zinc-400">
          Cargando…
        </div>
      }
    >
      <LoginForm />
    </Suspense>
  );
}
