export type AuthTokenPayload = {
  accessToken: string;
};

export type RegisterResponseData = {
  requiresEmailVerification: boolean;
  accessToken: string | null;
  message: string;
};

export type UserMe = {
  id: string;
  tenantId: string;
  email: string;
  username: string;
  emailConfirmed: boolean;
  createdAt: string;
};

export type ApiEnvelope<T> = { data: T };

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type CampaignListItem = {
  id: string;
  name: string;
  description: string | null;
  ownerUserId: string;
  createdAt: string;
  updatedAt: string;
};

export type CampaignDetail = {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  ownerUserId: string;
  createdAt: string;
  updatedAt: string;
};

export type CampaignMember = {
  userId: string;
  username: string;
  email: string;
  role: number;
  roleName: string;
  joinedAt: string;
};

export type CharacterListItem = {
  id: string;
  name: string;
  ownerUserId: string;
  level: number;
  race: string | null;
  class: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CharacterDetail = {
  id: string;
  campaignId: string;
  ownerUserId: string;
  name: string;
  level: number;
  race: string | null;
  class: string | null;
  backstory: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
};
