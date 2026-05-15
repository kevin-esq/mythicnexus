"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { getCampaign } from "@/lib/api/campaigns";
import { ApiError } from "@/lib/api/client";
import { useAuth } from "@/lib/auth/AuthContext";

const subNav = (campaignId: string) =>
  [
    { href: `/dashboard/campaigns/${campaignId}` as const, label: "Resumen" },
    { href: `/dashboard/campaigns/${campaignId}/lore` as const, label: "Lore" },
    { href: `/dashboard/campaigns/${campaignId}/characters` as const, label: "Personajes" },
    { href: `/dashboard/campaigns/${campaignId}/members` as const, label: "Miembros" },
    { href: `/dashboard/campaigns/${campaignId}/search` as const, label: "Búsqueda" },
    { href: `/dashboard/campaigns/${campaignId}/settings` as const, label: "Ajustes" },
  ] as const;

export function CampaignShell({ campaignId, children }: { campaignId: string; children: React.ReactNode }) {
  const pathname = usePathname();
  const { accessToken, hydrated } = useAuth();

  const q = useQuery({
    queryKey: ["campaign", campaignId, accessToken],
    queryFn: async () => {
      const res = await getCampaign(campaignId);
      return res.data;
    },
    enabled: hydrated && !!accessToken,
    retry: (count, err) => {
      if (err instanceof ApiError && (err.status === 404 || err.status === 403)) return false;
      return count < 2;
    },
  });

  if (q.isLoading) {
    return <p className="text-sm text-zinc-400">Cargando campaña…</p>;
  }

  if (q.isError) {
    const msg = q.error instanceof ApiError ? q.error.message : "No se pudo cargar la campaña.";
    return (
      <div className="rounded-lg border border-red-900/50 bg-red-950/30 px-4 py-3 text-sm text-red-100">
        {msg}
        <div className="mt-3">
          <Link href="/dashboard/campaigns" className="text-red-200 underline">
            Volver al listado
          </Link>
        </div>
      </div>
    );
  }

  const campaign = q.data;
  if (!campaign) return null;

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-6 lg:flex-row">
      <aside className="flex w-full shrink-0 flex-col gap-1 border-zinc-800 lg:w-52 lg:border-r lg:pr-4">
        <div className="mb-2 border-b border-zinc-800 pb-3">
          <p className="text-xs font-medium uppercase tracking-wide text-zinc-500">Campaña</p>
          <h1 className="text-lg font-semibold text-zinc-50">{campaign.name}</h1>
        </div>
        <nav className="flex flex-row flex-wrap gap-1 lg:flex-col">
          {subNav(campaignId).map((item) => {
            const isOverview = item.href === `/dashboard/campaigns/${campaignId}`;
            const active = isOverview
              ? pathname === item.href
              : pathname === item.href || pathname.startsWith(`${item.href}/`);
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`rounded-md px-3 py-2 text-sm transition lg:w-full ${
                  active ? "bg-zinc-800 text-white" : "text-zinc-300 hover:bg-zinc-900 hover:text-white"
                }`}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
      </aside>
      <div className="min-h-0 flex-1">{children}</div>
    </div>
  );
}
