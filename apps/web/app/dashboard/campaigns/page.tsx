import { CampaignsListClient } from "./CampaignsListClient";

export default function CampaignsPage() {
  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-2">
      <h1 className="text-2xl font-semibold text-zinc-50">Campañas</h1>
      <p className="text-sm text-zinc-400">Workspace por tenant: crear, listar y abrir campañas conectadas a la API.</p>
      <div className="mt-6">
        <CampaignsListClient />
      </div>
    </div>
  );
}
