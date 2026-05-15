"use client";

import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { addCampaignMember, listCampaignMembers, removeCampaignMember } from "@/lib/api/campaigns";
import { ApiError } from "@/lib/api/client";
import { CAMPAIGN_ROLE_OPTIONS, campaignRoleLabel } from "@/lib/campaignRoles";
import { useAuth } from "@/lib/auth/AuthContext";

export default function CampaignMembersPage() {
  const params = useParams();
  const campaignId = typeof params?.id === "string" ? params.id : "";
  const { accessToken, hydrated, user } = useAuth();
  const qc = useQueryClient();
  const [username, setUsername] = useState("");
  const [role, setRole] = useState<number>(2);
  const [formError, setFormError] = useState<string | null>(null);

  const listQ = useQuery({
    queryKey: ["campaign", campaignId, "members", accessToken],
    queryFn: async () => (await listCampaignMembers(campaignId)).data,
    enabled: hydrated && !!accessToken && !!campaignId,
  });

  const addM = useMutation({
    mutationFn: async () => {
      setFormError(null);
      const trimmed = username.trim();
      if (trimmed.length < 2) {
        throw new Error("Escribe un nombre de usuario (mínimo 2 caracteres).");
      }
      await addCampaignMember(campaignId, { username: trimmed, role });
    },
    onSuccess: async () => {
      setUsername("");
      await qc.invalidateQueries({ queryKey: ["campaign", campaignId, "members"] });
    },
    onError: (e: unknown) => {
      setFormError(e instanceof ApiError ? e.message : e instanceof Error ? e.message : "Error al invitar.");
    },
  });

  const removeM = useMutation({
    mutationFn: (uid: string) => removeCampaignMember(campaignId, uid),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["campaign", campaignId, "members"] });
    },
  });

  if (!campaignId) return null;

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-8">
      <div>
        <h2 className="text-xl font-semibold text-zinc-50">Miembros</h2>
        <p className="mt-1 text-sm text-zinc-400">
          Solo usuarios que ya pertenezcan al workspace pueden unirse. Escribe su{" "}
          <span className="text-zinc-300">nombre de usuario</span> (el tuyo es{" "}
          <code className="rounded bg-zinc-900 px-1 text-xs text-zinc-300">{user?.username ?? "…"}</code>
          ). La persona recibirá un correo en el <span className="text-zinc-300">outbox local del API</span> (carpeta{" "}
          <code className="text-xs text-zinc-500">email-outbox</code>); las notificaciones en la app vendrán después.
        </p>
      </div>

      <form
        className="flex flex-col gap-3 rounded-lg border border-zinc-800 bg-zinc-900/40 p-4"
        onSubmit={(e) => {
          e.preventDefault();
          addM.mutate();
        }}
      >
        <p className="text-sm font-medium text-zinc-200">Invitar miembro</p>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-end">
          <label className="flex flex-1 flex-col gap-1 text-xs text-zinc-400">
            Nombre de usuario
            <input
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="ej. MariaMistico"
              autoComplete="off"
              spellCheck={false}
            />
          </label>
          <label className="flex w-full flex-col gap-1 text-xs text-zinc-400 sm:w-44">
            Rol en campaña
            <select
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={role}
              onChange={(e) => setRole(Number(e.target.value))}
            >
              {CAMPAIGN_ROLE_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </label>
          <button
            type="submit"
            disabled={addM.isPending}
            className="rounded-md bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-white disabled:opacity-50"
          >
            Añadir
          </button>
        </div>
        {formError ? <p className="text-sm text-red-400">{formError}</p> : null}
      </form>

      {listQ.isLoading ? <p className="text-sm text-zinc-400">Cargando miembros…</p> : null}
      {listQ.isError ? (
        <p className="text-sm text-red-400">{listQ.error instanceof ApiError ? listQ.error.message : "Error al cargar."}</p>
      ) : null}
      {listQ.data ? (
        <ul className="divide-y divide-zinc-800 rounded-lg border border-zinc-800">
          {listQ.data.map((m) => (
            <li key={m.userId} className="flex flex-wrap items-center justify-between gap-2 px-4 py-3 text-sm">
              <div>
                <p className="font-medium text-zinc-100">{m.username}</p>
                <p className="text-xs text-zinc-500">{m.email}</p>
                <p className="text-xs text-zinc-400">{campaignRoleLabel(m.role)}</p>
              </div>
              <button
                type="button"
                className="rounded-md border border-zinc-700 px-2 py-1 text-xs text-zinc-300 hover:bg-zinc-900"
                disabled={removeM.isPending}
                onClick={() => {
                  if (m.userId === user?.id) {
                    window.alert("No puedes eliminarte a ti mismo desde aquí.");
                    return;
                  }
                  if (window.confirm(`¿Quitar a ${m.username} de la campaña?`)) {
                    removeM.mutate(m.userId);
                  }
                }}
              >
                Quitar
              </button>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  );
}
