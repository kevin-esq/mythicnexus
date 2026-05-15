"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { deleteCharacter, getCharacter, patchCharacter } from "@/lib/api/characters";
import { ApiError } from "@/lib/api/client";
import type { CharacterDetail } from "@/lib/api/types";
import { useAuth } from "@/lib/auth/AuthContext";

function CharacterEditForm({
  character,
  campaignId,
  characterId,
}: {
  character: CharacterDetail;
  campaignId: string;
  characterId: string;
}) {
  const router = useRouter();
  const qc = useQueryClient();
  const [name, setName] = useState(character.name);
  const [level, setLevel] = useState(String(character.level));
  const [race, setRace] = useState(character.race ?? "");
  const [cls, setCls] = useState(character.class ?? "");
  const [backstory, setBackstory] = useState(character.backstory ?? "");
  const [notes, setNotes] = useState(character.notes ?? "");
  const [error, setError] = useState<string | null>(null);

  const saveM = useMutation({
    mutationFn: async () => {
      setError(null);
      const lv = Number.parseInt(level, 10);
      await patchCharacter(characterId, {
        name,
        level: Number.isFinite(lv) ? lv : 1,
        race: race.trim() || null,
        class: cls.trim() || null,
        backstory: backstory.trim() || null,
        notes: notes.trim() || null,
      });
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["character", characterId] });
      await qc.invalidateQueries({ queryKey: ["campaign", campaignId, "characters"] });
    },
    onError: (e: unknown) => {
      setError(e instanceof ApiError ? e.message : "No se pudo guardar.");
    },
  });

  const delM = useMutation({
    mutationFn: () => deleteCharacter(characterId),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["campaign", campaignId, "characters"] });
      router.push(`/dashboard/campaigns/${campaignId}/characters`);
    },
  });

  return (
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
          className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
      </label>
      <div className="grid gap-4 sm:grid-cols-2">
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
      </div>
      <label className="flex flex-col gap-1 text-xs text-zinc-400">
        Raza
        <input
          className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
          value={race}
          onChange={(e) => setRace(e.target.value)}
        />
      </label>
      <label className="flex flex-col gap-1 text-xs text-zinc-400">
        Historia
        <textarea
          rows={4}
          className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
          value={backstory}
          onChange={(e) => setBackstory(e.target.value)}
        />
      </label>
      <label className="flex flex-col gap-1 text-xs text-zinc-400">
        Notas
        <textarea
          rows={3}
          className="rounded-md border border-zinc-700 bg-zinc-950 px-3 py-2 text-sm text-zinc-100"
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />
      </label>
      {error ? <p className="text-sm text-red-400">{error}</p> : null}
      <div className="flex flex-wrap gap-3">
        <button
          type="submit"
          disabled={saveM.isPending}
          className="rounded-md bg-zinc-100 px-4 py-2 text-sm font-medium text-zinc-900 hover:bg-white disabled:opacity-50"
        >
          Guardar
        </button>
        <button
          type="button"
          className="rounded-md border border-red-900/60 px-4 py-2 text-sm text-red-300 hover:bg-red-950/40 disabled:opacity-50"
          disabled={delM.isPending}
          onClick={() => {
            if (window.confirm("¿Archivar este personaje? (soft delete en el servidor)")) {
              delM.mutate();
            }
          }}
        >
          Archivar
        </button>
      </div>
    </form>
  );
}

export default function EditCharacterPage() {
  const params = useParams();
  const campaignId = typeof params?.id === "string" ? params.id : "";
  const characterId = typeof params?.characterId === "string" ? params.characterId : "";
  const { accessToken, hydrated } = useAuth();

  const q = useQuery({
    queryKey: ["character", characterId, accessToken],
    queryFn: async () => (await getCharacter(characterId)).data,
    enabled: hydrated && !!accessToken && !!characterId,
  });

  if (!characterId || !campaignId) return null;

  if (q.isLoading) return <p className="text-sm text-zinc-400">Cargando personaje…</p>;
  if (q.isError || !q.data) {
    return (
      <p className="text-sm text-red-400">
        {q.error instanceof ApiError ? q.error.message : "No se encontró el personaje."}
      </p>
    );
  }

  const c = q.data;

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6">
      <div className="flex items-center justify-between gap-4">
        <h2 className="text-xl font-semibold text-zinc-50">Editar personaje</h2>
        <Link href={`/dashboard/campaigns/${campaignId}/characters`} className="text-sm text-zinc-400 hover:text-zinc-200">
          Volver
        </Link>
      </div>

      <CharacterEditForm
        key={`${characterId}-${c.updatedAt}`}
        character={c}
        campaignId={campaignId}
        characterId={characterId}
      />
    </div>
  );
}
