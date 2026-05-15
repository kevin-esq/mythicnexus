"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { createCampaign, listCampaigns } from "@/lib/api/campaigns";
import { ApiError } from "@/lib/api/client";
import { useAuth } from "@/lib/auth/AuthContext";

export function CampaignsListClient() {
  const router = useRouter();
  const { accessToken, hydrated } = useAuth();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [createError, setCreateError] = useState<string | null>(null);

  const listQ = useQuery({
    queryKey: ["campaigns", page, pageSize, accessToken],
    queryFn: async () => (await listCampaigns({ page, pageSize })).data,
    enabled: hydrated && !!accessToken,
  });

  const createM = useMutation({
    mutationFn: () => createCampaign({ name, description: description.trim() || null }),
    onSuccess: async (res) => {
      setName("");
      setDescription("");
      setCreateError(null);
      await qc.invalidateQueries({ queryKey: ["campaigns"] });
      router.push(`/dashboard/campaigns/${res.data.id}`);
    },
    onError: (e: unknown) => {
      setCreateError(e instanceof ApiError ? e.message : "No se pudo crear la campaña.");
    },
  });

  if (!hydrated) {
    return <p className="text-sm text-zinc-400">Inicializando…</p>;
  }

  if (!accessToken) {
    return <p className="text-sm text-zinc-400">Inicia sesión para ver tus campañas.</p>;
  }

  const data = listQ.data;

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-10">
      <section>
        <h2 className="text-lg font-semibold text-zinc-50">Nueva campaña</h2>
        <form
          className="mt-3 flex flex-col gap-3 rounded-lg border border-zinc-800 bg-zinc-900/40 p-4"
          onSubmit={(e) => {
            e.preventDefault();
            createM.mutate();
          }}
        >
          <label className="flex flex-col gap-1 text-xs text-zinc-400">
            Nombre
            <input
              required
              minLength={2}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-xs text-zinc-400">
            Descripción (opcional)
            <textarea
              rows={2}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </label>
          {createError ? <p className="text-sm text-red-400">{createError}</p> : null}
          <button
            type="submit"
            disabled={createM.isPending}
            className="self-start rounded-md bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-white disabled:opacity-50"
          >
            Crear y abrir
          </button>
        </form>
      </section>

      <section>
        <h2 className="text-lg font-semibold text-zinc-50">Tus campañas</h2>
        {listQ.isLoading ? <p className="mt-3 text-sm text-zinc-400">Cargando…</p> : null}
        {listQ.isError ? (
          <p className="mt-3 text-sm text-red-400">
            {listQ.error instanceof ApiError ? listQ.error.message : "Error al cargar el listado."}
          </p>
        ) : null}
        {data?.items.length ? (
          <ul className="mt-3 divide-y divide-zinc-800 rounded-lg border border-zinc-800">
            {data.items.map((c) => (
              <li key={c.id}>
                <Link
                  href={`/dashboard/campaigns/${c.id}`}
                  className="block px-4 py-3 transition hover:bg-zinc-900/80"
                >
                  <p className="font-medium text-zinc-100">{c.name}</p>
                  {c.description ? <p className="mt-1 line-clamp-2 text-sm text-zinc-500">{c.description}</p> : null}
                </Link>
              </li>
            ))}
          </ul>
        ) : data ? (
          <p className="mt-3 text-sm text-zinc-500">No hay campañas todavía.</p>
        ) : null}

        {data && data.totalPages > 1 ? (
          <div className="mt-4 flex items-center justify-between text-sm text-zinc-400">
            <span>
              Página {data.page} de {data.totalPages} ({data.totalCount} en total)
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={page <= 1}
                className="rounded-md border border-zinc-700 px-3 py-1 hover:bg-zinc-900 disabled:opacity-40"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Anterior
              </button>
              <button
                type="button"
                disabled={page >= data.totalPages}
                className="rounded-md border border-zinc-700 px-3 py-1 hover:bg-zinc-900 disabled:opacity-40"
                onClick={() => setPage((p) => p + 1)}
              >
                Siguiente
              </button>
            </div>
          </div>
        ) : null}
      </section>
    </div>
  );
}
