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

    // What this corpus records about itself: which types it has adopted, and where it stands against the
    // framework it took. Carried here because adoption decides what is generated and what the corpus is
    // held to having built, so every entry point needs the same answer.
    public required CorpusDescriptor Descriptor;

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
            Descriptor = CorpusDescriptor.Load(repoRoot),
            Files = files,
            Docs = docs,
            Templates = DiscoverTemplates(repoRoot, schema, paths),
            SkippedNoFrontmatter = skipped,
            Paths = paths
        };
    }

    // The types this corpus holds, in schema order. The schema declares every type the tool manages; a
    // corpus adopts as many of them as it has use for, and this is the difference between the two.
    // Everything generated about the taxonomy reads it, so a corpus's own pages describe the corpus rather
    // than the framework's full range.
    //
    // Two answers to the same question, and which one is given is the point. Where `.corpus.yaml`
    // declares `types:`, that is the answer: adoption is a decision the corpus records, and the pages
    // follow the decision. Where it does not, the answer is read off the filesystem — a type is adopted if
    // both halves are there, the page and the folder, which is the bar CheckTypeSetup holds a type to
    // because a type is stood up as both or as neither.
    //
    // The inferred answer is the weaker one: it cannot tell a type nobody wanted from a type somebody has
    // not finished adding, which is exactly what `types:` exists to say. It is the reading a corpus gets
    // until it declares, so that taking a newer framework never requires editing the descriptor in the
    // same breath.
    public static List<TypeSchema> Adopted(Schema schema, string repoRoot, CorpusDescriptor descriptor)
    {
        var declared = descriptor.Types;

        return
        [
            .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value)
                .Where(t => declared is not null
                    ? declared.Contains(t.Key, StringComparer.Ordinal)
                    : StoodUp(t, repoRoot))
        ];
    }

    // Whether both halves of a type are on disk. A half-built type is not adopted: generating a row for it
    // would answer a defect with a link resolving to whichever of the two files exists.
    public static bool StoodUp(TypeSchema t, string repoRoot) =>
        !string.IsNullOrEmpty(t.Page)
        && File.Exists(Path.Combine(repoRoot, t.Page))
        && Directory.Exists(Path.Combine(repoRoot, t.Folder));

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
        // Type pages at the repo root — adrs.md, services.md, data.md, … Each is prose about its
        // records and is checked separately, as a page.
        var typePages = new HashSet<string>(
            schema.ByFolder.Values.Select(t => t.Page).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        var typeFolders = new HashSet<string>(schema.ByFolder.Keys, StringComparer.OrdinalIgnoreCase);

        var pathFilter = paths.Select(p => p.Replace('\\', '/').TrimEnd('/')).ToList();

        var result = new List<string>();
        foreach (var raw in files)
        {
            var rel = raw.Replace('\\', '/');
            if (!rel.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;
            if (IsExcluded(rel, typePages, typeFolders)) continue;
            if (pathFilter.Count > 0 && !pathFilter.Any(p => rel == p || rel.StartsWith(p + "/"))) continue;
            result.Add(rel);
        }

        return [.. result.OrderBy(r => r, StringComparer.Ordinal)];
    }

    private static bool IsExcluded(string rel, HashSet<string> typePages, HashSet<string> typeFolders)
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
        }

        // Only look inside folders that map to a type; everything else (legacy root
        // docs being reconciled elsewhere) is left alone.
        return !typeFolders.Contains(top);
    }
}
