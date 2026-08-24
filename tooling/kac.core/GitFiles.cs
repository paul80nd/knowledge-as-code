// File listing: shared by corpus discovery and by what an update reads of either tree

namespace kac.core;

public static class GitFiles
{
    // Tracked + untracked-but-not-ignored files, relative and forward-slashed, or null when git is
    // unavailable or this is not a repo (the caller then falls back to Walk). git ls-files respects
    // .gitignore, .git/info/exclude and global excludes, and never lists .git/ itself.
    //
    // A file the index still holds and the working tree no longer does is dropped, so both listings
    // answer the same question: what the corpus holds right now. Every caller goes on to read what it
    // is handed, and a deleted file has nothing to read.
    public static List<string>? Tracked(string root)
    {
        if (Git.Run(root, "ls-files --cached --others --exclude-standard") is not { } listing) return null;
        return
        [
            .. listing
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(rel => File.Exists(Path.Combine(root, rel)))
        ];
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
