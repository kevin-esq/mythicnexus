"use client";

import { useParams } from "next/navigation";
import { CampaignShell } from "../CampaignShell";

export default function CampaignLayout({ children }: { children: React.ReactNode }) {
  const params = useParams();
  const id = typeof params?.id === "string" ? params.id : "";
  if (!id) {
    return <p className="text-sm text-zinc-400">Identificador de campaña no válido.</p>;
  }
  return <CampaignShell campaignId={id}>{children}</CampaignShell>;
}
