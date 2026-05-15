import { apiRequest } from "./client";
import type { ApiEnvelope, CharacterDetail, CharacterListItem, PagedResult } from "./types";

export type ListCharactersParams = { page?: number; pageSize?: number };

export async function listCharacters(campaignId: string, params: ListCharactersParams = {}) {
  const page = params.page ?? 1;
  const pageSize = params.pageSize ?? 20;
  const qs = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  return apiRequest<ApiEnvelope<PagedResult<CharacterListItem>>>(
    `/api/campaigns/${campaignId}/characters?${qs.toString()}`,
  );
}

export async function createCharacter(
  campaignId: string,
  input: {
    name: string;
    level?: number | null;
    race?: string | null;
    class?: string | null;
    backstory?: string | null;
    notes?: string | null;
  },
) {
  return apiRequest<ApiEnvelope<CharacterDetail>>(`/api/campaigns/${campaignId}/characters`, {
    method: "POST",
    body: JSON.stringify(input),
  });
}

export async function getCharacter(id: string) {
  return apiRequest<ApiEnvelope<CharacterDetail>>(`/api/characters/${id}`);
}

export async function patchCharacter(
  id: string,
  input: {
    name?: string | null;
    level?: number | null;
    race?: string | null;
    class?: string | null;
    backstory?: string | null;
    notes?: string | null;
  },
) {
  return apiRequest<ApiEnvelope<CharacterDetail>>(`/api/characters/${id}`, {
    method: "PATCH",
    body: JSON.stringify(input),
  });
}

export async function deleteCharacter(id: string) {
  await apiRequest<null>(`/api/characters/${id}`, { method: "DELETE" });
}
