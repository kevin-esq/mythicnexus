"use client";

import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { getCampaign } from "@/lib/api/campaigns";
import { useAuth } from "@/lib/auth/AuthContext";

export default function CampaignOverviewPage() {
  const params = useParams();
  const id = typeof params?.id === "string" ? params.id : "";
  const { accessToken, hydrated } = useAuth();

  const q = useQuery({
    queryKey: ["campaign", id, accessToken],
    queryFn: async () => (await getCampaign(id)).data,
    enabled: hydrated && !!accessToken && !!id,
  });

  if (!id) return null;
  if (q.isPending) return <p className="text-sm text-zinc-400">Cargando…</p>;
  if (q.isError || !q.data) return null;

  const c = q.data;
  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-4">
      <h2 className="text-xl font-semibold text-zinc-50">Resumen</h2>
      <dl className="grid gap-3 text-sm">
        <div>
          <dt className="text-zinc-500">Nombre</dt>
          <dd className="text-zinc-200">{c.name}</dd>
        </div>
        <div>
          <dt className="text-zinc-500">Descripción</dt>
          <dd className="text-zinc-300">{c.description?.trim() ? c.description : "Sin descripción."}</dd>
        </div>
      </dl>
    </div>
  );
}
