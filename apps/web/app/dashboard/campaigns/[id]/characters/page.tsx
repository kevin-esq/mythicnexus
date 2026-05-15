"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { createCharacter, listCharacters } from "@/lib/api/characters";
import { ApiError } from "@/lib/api/client";
import { useAuth } from "@/lib/auth/AuthContext";

export default function CampaignCharactersPage() {
  const params = useParams();
  const campaignId = typeof params?.id === "string" ? params.id : "";
  const { accessToken, hydrated } = useAuth();
  const qc = useQueryClient();
  const [name, setName] = useState("");
  const [level, setLevel] = useState("1");
  const [race, setRace] = useState("");
  const [cls, setCls] = useState("");
  const [error, setError] = useState<string | null>(null);

  const listQ = useQuery({
    queryKey: ["campaign", campaignId, "characters", accessToken],
    queryFn: async () => (await listCharacters(campaignId)).data,
    enabled: hydrated && !!accessToken && !!campaignId,
  });

  const createM = useMutation({
    mutationFn: async () => {
      setError(null);
      const lv = Number.parseInt(level, 10);
      await createCharacter(campaignId, {
        name,
        level: Number.isFinite(lv) ? lv : 1,
        race: race.trim() || null,
        class: cls.trim() || null,
      });
    },
    onSuccess: async () => {
      setName("");
      setRace("");
      setCls("");
      setLevel("1");
      await qc.invalidateQueries({ queryKey: ["campaign", campaignId, "characters"] });
    },
    onError: (e: unknown) => {
      setError(e instanceof ApiError ? e.message : "No se pudo crear el personaje.");
    },
  });

  if (!campaignId) return null;

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-8">
      <h2 className="text-xl font-semibold text-zinc-50">Personajes</h2>

      <form
        className="flex flex-col gap-3 rounded-lg border border-zinc-800 bg-zinc-900/40 p-4"
        onSubmit={(e) => {
          e.preventDefault();
          createM.mutate();
        }}
      >
        <p className="text-sm font-medium text-zinc-200">Nuevo personaje</p>
        <div className="grid gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-xs text-zinc-400 sm:col-span-2">
            Nombre
            <input
              required
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-xs text-zinc-400">
            Nivel
            <input
              type="number"
              min={1}
              max={40}
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={level}
              onChange={(e) => setLevel(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-xs text-zinc-400">
            Clase
            <input
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={cls}
              onChange={(e) => setCls(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-xs text-zinc-400 sm:col-span-2">
            Raza
            <input
              className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
              value={race}
              onChange={(e) => setRace(e.target.value)}
            />
          </label>
        </div>
        {error ? <p className="text-sm text-red-400">{error}</p> : null}
        <button
          type="submit"
          disabled={createM.isPending}
          className="self-start rounded-md bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-white disabled:opacity-50"
        >
          Crear
        </button>
      </form>

      {listQ.isLoading ? <p className="text-sm text-zinc-400">Cargando…</p> : null}
      {listQ.isError ? (
        <p className="text-sm text-red-400">{listQ.error instanceof ApiError ? listQ.error.message : "Error."}</p>
      ) : null}
      {listQ.data?.items.length ? (
        <ul className="divide-y divide-zinc-800 rounded-lg border border-zinc-800">
          {listQ.data.items.map((ch) => (
            <li key={ch.id}>
              <Link
                href={`/dashboard/campaigns/${campaignId}/characters/${ch.id}`}
                className="flex flex-wrap items-baseline justify-between gap-2 px-4 py-3 text-sm transition hover:bg-zinc-900/80"
              >
                <span className="font-medium text-zinc-100">{ch.name}</span>
                <span className="text-xs text-zinc-500">
                  Nv. {ch.level}
                  {ch.class ? ` · ${ch.class}` : ""}
                  {ch.race ? ` · ${ch.race}` : ""}
                </span>
              </Link>
            </li>
          ))}
        </ul>
      ) : listQ.data ? (
        <p className="text-sm text-zinc-500">Aún no hay personajes en esta campaña.</p>
      ) : null}
    </div>
  );
}
