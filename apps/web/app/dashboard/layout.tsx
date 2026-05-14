import { AppSidebar } from "../components/AppSidebar";

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-0 flex-1">
      <AppSidebar />
      <main className="min-h-0 flex-1 overflow-auto bg-zinc-950 px-8 py-8">{children}</main>
    </div>
  );
}
