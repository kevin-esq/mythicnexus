import { AppSidebar } from "../components/AppSidebar";
import { RequireAuth } from "../components/RequireAuth";

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <RequireAuth>
      <div className="flex min-h-0 flex-1">
        <AppSidebar />
        <main className="min-h-0 flex-1 overflow-auto bg-zinc-950 px-8 py-8">{children}</main>
      </div>
    </RequireAuth>
  );
}
