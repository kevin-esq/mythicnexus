import { Suspense } from "react";
import { ResetPasswordForm } from "./ResetPasswordForm";

export default function ResetPasswordPage() {
  return (
    <Suspense
      fallback={
        <div className="w-full max-w-sm rounded-xl border border-zinc-800 bg-zinc-900/60 px-8 py-12 text-center text-sm text-zinc-400">
          Cargando…
        </div>
      }
    >
      <ResetPasswordForm />
    </Suspense>
  );
}
