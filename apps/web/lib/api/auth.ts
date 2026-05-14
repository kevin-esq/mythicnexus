import { apiRequest } from "./client";
import type { ApiEnvelope, AuthTokenPayload, RegisterResponseData, UserMe } from "./types";

export type RegisterInput = { email: string; username: string; password: string };
export type LoginInput = { email: string; password: string };

export async function registerUser(input: RegisterInput) {
  return apiRequest<ApiEnvelope<RegisterResponseData>>("/api/users/register", {
    method: "POST",
    body: JSON.stringify(input),
    accessToken: null,
  });
}

export async function loginUser(input: LoginInput) {
  return apiRequest<ApiEnvelope<AuthTokenPayload>>("/api/users/login", {
    method: "POST",
    body: JSON.stringify(input),
    accessToken: null,
  });
}

export async function fetchCurrentUser(accessToken: string) {
  return apiRequest<ApiEnvelope<UserMe>>("/api/users/me", {
    method: "GET",
    accessToken,
  });
}

export async function forgotPassword(email: string) {
  return apiRequest<ApiEnvelope<{ message: string }>>("/api/users/forgot-password", {
    method: "POST",
    body: JSON.stringify({ email }),
    accessToken: null,
  });
}

export async function resetPassword(input: { token: string; newPassword: string }) {
  return apiRequest<ApiEnvelope<{ message: string }>>("/api/users/reset-password", {
    method: "POST",
    body: JSON.stringify(input),
    accessToken: null,
  });
}

export async function resendVerificationEmail(email: string) {
  return apiRequest<ApiEnvelope<{ message: string }>>("/api/users/resend-verification", {
    method: "POST",
    body: JSON.stringify({ email }),
    accessToken: null,
  });
}
