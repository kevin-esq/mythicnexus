const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5118";

export const ACCESS_TOKEN_KEY = "mythicnexus_access_token";

export function getStoredAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function setStoredAccessToken(token: string | null) {
  if (typeof window === "undefined") return;
  if (token) localStorage.setItem(ACCESS_TOKEN_KEY, token);
  else localStorage.removeItem(ACCESS_TOKEN_KEY);
}

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly body: unknown;

  constructor(status: number, code: string, message: string, body?: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.body = body;
  }
}

function readErrorFromPayload(json: unknown): { code: string; message: string } {
  if (!json || typeof json !== "object") {
    return { code: "unknown", message: "Request failed" };
  }

  const o = json as Record<string, unknown>;
  const err = o.error as Record<string, unknown> | undefined;
  if (err && typeof err === "object") {
    const code = typeof err.code === "string" ? err.code : "unknown";
    const message = typeof err.message === "string" ? err.message : "Request failed";
    return { code, message };
  }

  const title = typeof o.title === "string" ? o.title : undefined;
  const detail = typeof o.detail === "string" ? o.detail : undefined;
  const ext = o.extensions as Record<string, unknown> | undefined;
  const extCode = ext && typeof ext.code === "string" ? ext.code : undefined;
  const code = extCode ?? (typeof o.type === "string" && o.type.includes("/errors/") ? o.type.split("/errors/").pop() ?? "unknown" : "unknown");
  const message = detail ?? title ?? "Request failed";
  return { code, message };
}

export async function apiRequest<T>(path: string, init: RequestInit & { accessToken?: string | null } = {}): Promise<T> {
  const { accessToken, ...rest } = init;
  const token = accessToken !== undefined ? accessToken : getStoredAccessToken();

  const headers = new Headers(rest.headers);
  headers.set("Accept", "application/json");
  if (rest.body && typeof rest.body === "string" && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const res = await fetch(`${API_BASE}${path}`, { ...rest, headers });
  const text = await res.text();
  let json: unknown = null;
  if (text) {
    try {
      json = JSON.parse(text) as unknown;
    } catch {
      json = { raw: text };
    }
  }

  if (!res.ok) {
    const { code, message } = readErrorFromPayload(json);
    throw new ApiError(res.status, code, message, json);
  }

  return json as T;
}
