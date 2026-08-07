// ---------------------------------------------------------------------------
// Corpus discovery
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Corpus
{
    private static readonly string[] SkipDirs = [".git", ".idea", ".claude"];

    // Every file the corpus contains, before any exclusion. Used to ask whether a folder is really
    // there: an empty directory git has never seen is not part of the corpus, and treating it as one
    // is what made the answer depend on which machine was asking.
    public static List<string> AllFiles(string repoRoot) =>
        GitFiles.Tracked(repoRoot) ?? GitFiles.Walk(repoRoot, "*.md", SkipDirs);

    public static List<string> Discover(string repoRoot, Schema schema, List<string> paths)
    {
        // git ls-files respects .gitignore, .git/info/exclude and global excludes, and never lists
        // .git/ itself — exactly the "respect .gitignore" requirement; the walk is the non-git fallback.
        var files = AllFiles(repoRoot);

        // Type pages at the repo root — adrs.md, services.md, data.md, … A collection type's page is
        // prose about its records and is checked separately, as a page. A single-document type's page
        // *is* its record, so it stays in the corpus and is validated like any other document.
        var typePages = new HashSet<string>(
            schema.ByFolder.Values.Where(t => !t.IsSingleDocument)
                .Select(t => t.Page).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        var typeFolders = new HashSet<string>(schema.ByFolder.Keys, StringComparer.OrdinalIgnoreCase);

        // A single-document type's page, which is a record living at the repo root rather than inside
        // a type folder — so it has to be let through the folder test at the end of IsExcluded.
        var recordPages = new HashSet<string>(
            schema.ByFolder.Values.Where(t => t.IsSingleDocument)
                .Select(t => t.Page).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        var pathFilter = paths.Select(p => p.Replace('\\', '/').TrimEnd('/')).ToList();

        var result = new List<string>();
        foreach (var raw in files)
        {
            var rel = raw.Replace('\\', '/');
            if (!rel.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;
            if (IsExcluded(rel, typePages, typeFolders, recordPages)) continue;
            if (pathFilter.Count > 0 && !pathFilter.Any(p => rel == p || rel.StartsWith(p + "/"))) continue;
            result.Add(rel);
        }

        return [.. result.OrderBy(r => r, StringComparer.Ordinal)];
    }

    private static bool IsExcluded(string rel, HashSet<string> typePages, HashSet<string> typeFolders,
        HashSet<string> recordPages)
    {
        var parts = rel.Split('/');
        var top = parts[0];

        if (SkipDirs.Contains(top)) return true;
        if (top is "knowledge-as-code" or "_plan" or "_reports") return true;

        var name = parts[^1];
        if (name.Equals("template.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.Equals("INDEX.md", StringComparison.OrdinalIgnoreCase)) return true;

        // Root-level orientation / generated / type pages.
        if (parts.Length == 1)
        {
            if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("CLAUDE.md", StringComparison.OrdinalIgnoreCase)) return true;
            if (typePages.Contains(name)) return true;
            if (recordPages.Contains(name)) return false;
        }

        // Only look inside folders that map to a type; everything else (legacy root
        // docs being reconciled elsewhere) is left alone.
        return !typeFolders.Contains(top);
    }
}
