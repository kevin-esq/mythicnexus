/** Mirrors `CampaignRole` in the API (int values). */
export const CampaignRole = {
  DungeonMaster: 0,
  CoDungeonMaster: 1,
  Player: 2,
  Viewer: 3,
} as const;

export type CampaignRoleValue = (typeof CampaignRole)[keyof typeof CampaignRole];

export const CAMPAIGN_ROLE_OPTIONS: { value: CampaignRoleValue; label: string }[] = [
  { value: CampaignRole.DungeonMaster, label: "Maestro de calabozos" },
  { value: CampaignRole.CoDungeonMaster, label: "Co-DM" },
  { value: CampaignRole.Player, label: "Jugador" },
  { value: CampaignRole.Viewer, label: "Espectador" },
];

export function campaignRoleLabel(role: number): string {
  return CAMPAIGN_ROLE_OPTIONS.find((o) => o.value === role)?.label ?? `Rol ${role}`;
}
