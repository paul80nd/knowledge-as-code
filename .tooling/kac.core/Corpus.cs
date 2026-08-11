// ---------------------------------------------------------------------------
// Corpus discovery and loading
// ---------------------------------------------------------------------------

namespace kac.core;

// The corpus as one loaded thing: the schema it is judged against, the file listing the shape checks
// ask about, and every record parsed. Every entry point begins by building one of these, so there is
// a single account of what "the corpus" is rather than one per command that can drift out of step.
public sealed class LoadedCorpus
{
    public required string RepoRoot;
    public required Schema Schema;

    // Every file, before exclusion — what CheckTypeSetup asks about which folders exist.
    public required List<string> Files;

    // The records: every discovered document that carries frontmatter, in corpus order.
    public required List<Doc> Docs;

    // The template each stood-up collection type carries, in corpus order. Held beside the records
    // rather than among them: a template is checked, and is not one. Discovered here so that the count
    // the summary reports and the files the validator reads are the same list rather than two walks
    // that could disagree about what the corpus holds.
    public required List<string> Templates;

    // Discovered but not migrated. Reported rather than dropped, so a corpus part-way through
    // adoption reads as part-way through rather than as smaller than it is.
    public required int SkippedNoFrontmatter;

    // The subtrees this load was narrowed to, empty for the whole repo. Held because it decides more
    // than which documents were read: a run narrowed to one document is not asking about the shape of
    // the corpus, and the checks that answer that are skipped.
    public required List<string> Paths;
}

public static class Corpus
{
    private static readonly string[] SkipDirs = [".git", ".idea", ".claude"];

    // Every file the corpus contains, before any exclusion. Used to ask whether a folder is really
    // there: an empty directory git has never seen is not part of the corpus, and counting it as one
    // makes the answer depend on which machine is asking.
    //
    // git ls-files respects .gitignore, .git/info/exclude and global excludes, and never lists .git/
    // itself — exactly the "respect .gitignore" requirement; the walk is the non-git fallback.
    public static List<string> AllFiles(string repoRoot) =>
        GitFiles.Tracked(repoRoot) ?? GitFiles.Walk(repoRoot, "*.md", SkipDirs);

    // Load the schema, list the files, and parse every record — everything an entry point needs
    // before it can ask a question. The listing is taken once and carried on the result: discovery
    // and the type-setup check both want it, and a second `git ls-files` costs more than every check
    // in the tool put together.
    public static LoadedCorpus Load(string repoRoot, List<string> paths)
    {
        var schema = Schema.Load(repoRoot);
        var files = AllFiles(repoRoot);

        var docs = new List<Doc>();
        var skipped = 0;
        foreach (var rel in Discover(files, schema, paths))
        {
            var doc = Doc.Parse(rel, File.ReadAllText(Path.Combine(repoRoot, rel)), schema);
            if (doc is null)
            {
                skipped++;
                continue;
            }

            docs.Add(doc);
        }

        return new LoadedCorpus
        {
            RepoRoot = repoRoot,
            Schema = schema,
            Files = files,
            Docs = docs,
            Templates = DiscoverTemplates(repoRoot, schema, paths),
            SkippedNoFrontmatter = skipped,
            Paths = paths
        };
    }

    // The types this corpus actually holds, in schema order. The schema declares every type the tool
    // manages; a corpus adopts as many of them as it has use for, and this is the difference between the
    // two. Everything generated about the taxonomy reads it, so a corpus's own pages describe the corpus
    // rather than the framework's full range.
    //
    // Stood up means both halves are there — the page and the folder of records — which is the same bar
    // CheckTypeSetup holds a type to, because a type is set up as both or as neither. A half-built type
    // is left out here and reported there: generating a row for it would answer a defect with a link that
    // resolves to one of the two files that exist.
    public static List<TypeSchema> StoodUp(Schema schema, string repoRoot) =>
    [
        .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value)
            .Where(t => !string.IsNullOrEmpty(t.Page)
                        && File.Exists(Path.Combine(repoRoot, t.Page))
                        && (t.IsSingleDocument || Directory.Exists(Path.Combine(repoRoot, t.Folder))))
    ];

    // The template of every collection type that has one. Asked of the filesystem rather than of the
    // file listing, as type-setup asks it: the question is whether the file a contributor would copy is
    // there, and a type whose template is untracked has a different problem from one with none.
    //
    // A type with no template is skipped in silence — its absence is type-setup's to report, and a type
    // nobody has stood up yet is a valid, quiet state.
    private static List<string> DiscoverTemplates(string repoRoot, Schema schema, List<string> paths)
    {
        var pathFilter = paths.Select(p => p.Replace('\\', '/').TrimEnd('/')).ToList();

        var result = new List<string>();
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (t.IsSingleDocument) continue; // one document, and it is the page — nothing to copy

            var rel = $"{(string.IsNullOrEmpty(t.Folder) ? key : t.Folder)}/{Artefact.Template}";
            if (pathFilter.Count > 0 && !pathFilter.Any(p => rel == p || rel.StartsWith(p + "/"))) continue;
            if (File.Exists(Path.Combine(repoRoot, rel))) result.Add(rel);
        }

        return result;
    }

    // Which of the listed files are records to validate: markdown, inside a folder the schema maps to
    // a type, and within the given subtrees.
    private static List<string> Discover(List<string> files, Schema schema, List<string> paths)
    {
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
        if (top == "knowledge-as-code") return true;

        // The framework's own files, wherever they sit: the generated index and the template inside a
        // type folder, and the scaffolding directories at the root.
        if (Artefact.IsReserved(rel)) return true;

        var name = parts[^1];

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
