using System.Diagnostics;

// ---------------------------------------------------------------------------
// File listing — shared by corpus discovery and the mechanism check
// ---------------------------------------------------------------------------

namespace kac.core;

public static class GitFiles
{
    // Tracked + untracked-but-not-ignored files, relative and forward-slashed, or null when git is
    // unavailable or this is not a repo (the caller then falls back to Walk). git ls-files respects
    // .gitignore, .git/info/exclude and global excludes, and never lists .git/ itself.
    public static List<string>? Tracked(string root)
    {
        try
        {
            var psi = new ProcessStartInfo("git", "ls-files --cached --others --exclude-standard")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            if (p is null) return null;
            // Drain both streams concurrently before waiting: reading one to end while the other's
            // pipe buffer fills would deadlock.
            var stdout = p.StandardOutput.ReadToEndAsync();
            _ = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            if (p.ExitCode != 0) return null;
            return
                [.. stdout.Result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }
        catch
        {
            return null;
        }
    }

    // Fallback for a non-git tree (the test harness assembles one): every file matching `pattern`,
    // relative and forward-slashed, dropping any path at or under one of `skipDirs`.
    public static List<string> Walk(string root, string pattern, params string[] skipDirs) =>
    [
        .. Directory
            .EnumerateFiles(root, pattern, SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
            .Where(rel => !skipDirs.Any(d => rel == d || rel.StartsWith(d + "/")))
    ];
}
