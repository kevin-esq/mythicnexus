"use client";

import { useParams, useRouter } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { deleteCampaign, getCampaign, patchCampaign } from "@/lib/api/campaigns";
import { ApiError } from "@/lib/api/client";
import type { CampaignDetail } from "@/lib/api/types";
import { useAuth } from "@/lib/auth/AuthContext";

function CampaignSettingsForm({ campaign, campaignId }: { campaign: CampaignDetail; campaignId: string }) {
  const router = useRouter();
  const qc = useQueryClient();
  const [name, setName] = useState(campaign.name);
  const [description, setDescription] = useState(campaign.description ?? "");
  const [error, setError] = useState<string | null>(null);

  const saveM = useMutation({
    mutationFn: () => patchCampaign(campaignId, { name, description: description.trim() || null }),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["campaign", campaignId] });
      setError(null);
    },
    onError: (e: unknown) => {
      setError(e instanceof ApiError ? e.message : "No se pudo guardar.");
    },
  });

  const delM = useMutation({
    mutationFn: () => deleteCampaign(campaignId),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["campaigns"] });
      router.push("/dashboard/campaigns");
    },
    onError: (e: unknown) => {
      setError(e instanceof ApiError ? e.message : "No se pudo archivar la campaña.");
    },
  });

  return (
    <>
      <form
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault();
          saveM.mutate();
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
          Descripción
          <textarea
            rows={4}
            className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </label>
        {error ? <p className="text-sm text-red-400">{error}</p> : null}
        <button
          type="submit"
          disabled={saveM.isPending}
          className="self-start rounded-md bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-white disabled:opacity-50"
        >
          Guardar cambios
        </button>
      </form>

      <div className="border-t border-zinc-800 pt-6">
        <p className="text-sm text-zinc-400">Archivar oculta la campaña (soft delete). Los datos permanecen en el servidor.</p>
        <button
          type="button"
          disabled={delM.isPending}
          className="mt-3 rounded-md border border-red-900/60 px-4 py-2 text-sm text-red-300 hover:bg-red-950/40 disabled:opacity-50"
          onClick={() => {
            if (window.confirm("¿Archivar esta campaña?")) {
              delM.mutate();
            }
          }}
        >
          Archivar campaña
        </button>
      </div>
    </>
  );
}

export default function CampaignSettingsPage() {
  const params = useParams();
  const campaignId = typeof params?.id === "string" ? params.id : "";
  const { accessToken, hydrated } = useAuth();

  const q = useQuery({
    queryKey: ["campaign", campaignId, accessToken],
    queryFn: async () => (await getCampaign(campaignId)).data,
    enabled: hydrated && !!accessToken && !!campaignId,
  });

  if (!campaignId) return null;
  if (q.isLoading) return <p className="text-sm text-zinc-400">Cargando…</p>;
  if (q.isError || !q.data) {
    return <p className="text-sm text-red-400">No se pudo cargar la campaña.</p>;
  }

  const c = q.data;

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6">
      <h2 className="text-xl font-semibold text-zinc-50">Ajustes de campaña</h2>
      <CampaignSettingsForm key={`${campaignId}-${c.updatedAt}`} campaign={c} campaignId={campaignId} />
    </div>
  );
}
