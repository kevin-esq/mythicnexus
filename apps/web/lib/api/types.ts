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
