"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import {
  fetchCurrentUser,
  loginUser,
  registerUser,
  resendVerificationEmail,
} from "@/lib/api/auth";
import { ApiError, getStoredAccessToken, setStoredAccessToken } from "@/lib/api/client";
import type { UserMe } from "@/lib/api/types";

type AuthContextValue = {
  hydrated: boolean;
  accessToken: string | null;
  user: UserMe | null;
  isSessionReady: boolean;
  sessionError: Error | null;
  login: (input: { email: string; password: string }) => Promise<void>;
  register: (input: { email: string; username: string; password: string }) => Promise<{ requiresEmailVerification: boolean }>;
  resendVerification: (email: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient();
  const [hydrated, setHydrated] = useState(false);
  const [accessToken, setAccessToken] = useState<string | null>(null);

  useEffect(() => {
    setAccessToken(getStoredAccessToken());
    setHydrated(true);
  }, []);

  const setSessionToken = useCallback((token: string | null) => {
    setStoredAccessToken(token);
    setAccessToken(token);
  }, []);

  const meQuery = useQuery({
    queryKey: ["auth", "me", accessToken],
    queryFn: async () => {
      if (!accessToken) return null;
      try {
        const res = await fetchCurrentUser(accessToken);
        return res.data;
      } catch (e) {
        if (e instanceof ApiError && e.status === 401) {
          setStoredAccessToken(null);
          setAccessToken(null);
        }
        throw e;
      }
    },
    enabled: hydrated && !!accessToken,
    retry: false,
  });

  const loginMutation = useMutation({
    mutationFn: loginUser,
    onSuccess: async (res) => {
      setSessionToken(res.data.accessToken);
      await queryClient.invalidateQueries({ queryKey: ["auth", "me"] });
    },
  });

  const registerMutation = useMutation({
    mutationFn: registerUser,
    onSuccess: async (res) => {
      if (res.data.accessToken) {
        setSessionToken(res.data.accessToken);
        await queryClient.invalidateQueries({ queryKey: ["auth", "me"] });
      }
    },
  });

  const login = useCallback(
    async (input: { email: string; password: string }) => {
      await loginMutation.mutateAsync(input);
    },
    [loginMutation],
  );

  const register = useCallback(
    async (input: { email: string; username: string; password: string }) => {
      const res = await registerMutation.mutateAsync(input);
      return { requiresEmailVerification: res.data.requiresEmailVerification };
    },
    [registerMutation],
  );

  const resendVerification = useCallback(async (email: string) => {
    await resendVerificationEmail(email);
  }, []);

  const logout = useCallback(() => {
    setSessionToken(null);
    queryClient.removeQueries({ queryKey: ["auth", "me"] });
  }, [queryClient, setSessionToken]);

  const isSessionReady = Boolean(
    hydrated && accessToken && !meQuery.isPending && !meQuery.isError && meQuery.data,
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      hydrated,
      accessToken,
      user: meQuery.data ?? null,
      isSessionReady,
      sessionError: meQuery.error ? (meQuery.error as Error) : null,
      login,
      register,
      resendVerification,
      logout,
    }),
    [
      hydrated,
      accessToken,
      meQuery.data,
      meQuery.error,
      meQuery.isPending,
      meQuery.isError,
      isSessionReady,
      login,
      register,
      resendVerification,
      logout,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
