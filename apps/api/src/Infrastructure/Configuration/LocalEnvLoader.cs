using DotNetEnv;

namespace MythicNexus.Api.Infrastructure.Configuration;

public static class LocalEnvLoader
{
    /// <summary>
    /// Loads the first existing <c>.env</c> from common working directories (repo root, project folder, or next to the built assembly).
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
        yield return Path.Combine(Directory.GetCurrentDirectory(), ".env");

        var baseDir = AppContext.BaseDirectory;
        yield return Path.Combine(baseDir, ".env");
        yield return Path.Combine(baseDir, "..", "..", "..", ".env");
    }
}
