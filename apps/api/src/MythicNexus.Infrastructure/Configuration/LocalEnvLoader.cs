using DotNetEnv;

namespace MythicNexus.Infrastructure.Configuration;

public static class LocalEnvLoader
{
    /// <summary>
    /// Loads the first existing <c>.env</c> from common locations: <c>apps/api/.env</c> (monorepo root),
    /// current directory, or next to the API build output (so <c>dotnet run</c> / <c>dotnet ef</c> from the repo root picks up API secrets).
    /// </summary>
    public static void Load()
    {
        foreach (var path in GetCandidatePaths())
        {
            var full = Path.GetFullPath(path);
            if (!File.Exists(full))
                continue;

            Env.Load(full);
            return;
        }
    }

    private static IEnumerable<string> GetCandidatePaths()
    {
        var cwd = Directory.GetCurrentDirectory();

        // Monorepo: prefer API-specific secrets when running from repo root (`dotnet run` / `dotnet ef` from `/`).
        yield return Path.Combine(cwd, "apps", "api", ".env");
        yield return Path.Combine(cwd, ".env");

        var baseDir = AppContext.BaseDirectory;
        yield return Path.Combine(baseDir, ".env");

        // MythicNexus.Api/bin/Debug/net9.0 → …/apps/api (5 levels up).
        yield return Path.Combine(baseDir, "..", "..", "..", "..", "..", ".env");

        // Fallback: project folder and `src` (older layouts).
        yield return Path.Combine(baseDir, "..", "..", "..", ".env");
        yield return Path.Combine(baseDir, "..", "..", "..", "..", ".env");
    }
}
