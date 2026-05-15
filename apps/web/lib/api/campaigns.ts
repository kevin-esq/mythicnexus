import { apiRequest } from "./client";
import type { ApiEnvelope, CampaignDetail, CampaignListItem, CampaignMember, PagedResult } from "./types";

export type ListCampaignsParams = { page?: number; pageSize?: number };

export async function listCampaigns(params: ListCampaignsParams = {}) {
  const page = params.page ?? 1;
  const pageSize = params.pageSize ?? 20;
  const qs = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  return apiRequest<ApiEnvelope<PagedResult<CampaignListItem>>>(`/api/campaigns?${qs.toString()}`);
}

export async function createCampaign(input: { name: string; description?: string | null }) {
  return apiRequest<ApiEnvelope<CampaignDetail>>("/api/campaigns", {
    method: "POST",
    body: JSON.stringify({ name: input.name, description: input.description ?? null }),
  });
}

export async function getCampaign(id: string) {
  return apiRequest<ApiEnvelope<CampaignDetail>>(`/api/campaigns/${id}`);
}

export async function patchCampaign(id: string, input: { name?: string | null; description?: string | null }) {
  return apiRequest<ApiEnvelope<CampaignDetail>>(`/api/campaigns/${id}`, {
    method: "PATCH",
    body: JSON.stringify(input),
  });
}

export async function deleteCampaign(id: string) {
  await apiRequest<null>(`/api/campaigns/${id}`, { method: "DELETE" });
}

export async function listCampaignMembers(campaignId: string) {
  return apiRequest<ApiEnvelope<CampaignMember[]>>(`/api/campaigns/${campaignId}/members`);
}

export async function addCampaignMember(
  campaignId: string,
  input: { role: number; username?: string; userId?: string },
) {
  const payload: { role: number; username?: string; userId?: string } = { role: input.role };
  if (input.userId?.trim()) {
    payload.userId = input.userId.trim();
  } else if (input.username?.trim()) {
    payload.username = input.username.trim();
  }
  return apiRequest<ApiEnvelope<CampaignMember>>(`/api/campaigns/${campaignId}/members`, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function removeCampaignMember(campaignId: string, userId: string) {
  await apiRequest<null>(`/api/campaigns/${campaignId}/members/${userId}`, { method: "DELETE" });
}
