namespace kac.core;

public static class GitFiles
{
    /// <summary>
    /// Null when git is unavailable or this is not a repository, and the caller falls back to <c>Walk</c>. A file the
    /// index holds and the working tree no longer does is dropped, so both listings answer the same.
    /// </summary>
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

    public static List<string> Walk(string root, string pattern, params string[] skipDirs) =>
    [
        .. Directory
            .EnumerateFiles(root, pattern, SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
            .Where(rel => !skipDirs.Any(d => rel == d || rel.StartsWith(d + "/")))
    ];
}
