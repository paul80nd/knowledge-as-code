// ---------------------------------------------------------------------------
// Corpus discovery and loading
// ---------------------------------------------------------------------------

namespace kac.core;

// The corpus as one loaded thing: the schema it is judged against, what it holds, and every record
// parsed. Every entry point begins by building one of these, so there is a single account of what "the
// corpus" is rather than one per command that can drift out of step.
public sealed class LoadedCorpus
{
    public required Schema Schema;

    // What this corpus records about itself: where it stands against the framework it took, and which
    // types it declares. `Adopted` below is the resolved answer, which is what the rest of the tool reads.
    public required CorpusDescriptor Descriptor;

    // Every path the corpus holds, and a way to read one.
    public required Tree Tree;

    // The types this corpus took. Resolved once, here, because it decides both what is generated and what
    // the corpus is held to having built — two entry points asking separately could answer differently.
    public required List<TypeSchema> Adopted;

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
}

public static class Corpus
{
    private static readonly string[] SkipDirs = [".git", ".idea", ".claude"];

    // Every file the corpus contains, before any exclusion — what `Tree` is built over.
    //
    // git ls-files respects .gitignore, .git/info/exclude and global excludes, and never lists .git/
    // itself — exactly the "respect .gitignore" requirement; the walk is the non-git fallback.
    private static List<string> AllFiles(string repoRoot) =>
        GitFiles.Tracked(repoRoot) ?? GitFiles.Walk(repoRoot, "*.md", SkipDirs);

    // A corpus at a path. The listing is taken once and carried on the result as a `Tree`: everything
    // downstream asks it what the corpus holds, and a second `git ls-files` costs more than every check in
    // the tool put together.
    //
    // This is the one place a path becomes a corpus. Everything below it is decided from values.
    public static LoadedCorpus Load(string repoRoot) =>
        Load(
            new Tree(
                new HashSet<string>(AllFiles(repoRoot).Select(f => f.Replace('\\', '/')), StringComparer.Ordinal),
                rel => Files.ReadLf(Path.Combine(repoRoot, rel)),
                rel => File.Exists(Path.Combine(repoRoot, rel))),
            Schema.Load(repoRoot),
            CorpusDescriptor.Load(repoRoot));

    // The listing, the schema it is judged against, and what the corpus records about itself — everything
    // an entry point needs before it can ask a question, and the whole of what this reads. A caller with a
    // corpus nobody wrote to disk hands over the same three things, so a check can be written against one.
    public static LoadedCorpus Load(Tree tree, Schema schema, CorpusDescriptor descriptor)
    {
        var docs = new List<Doc>();
        var skipped = 0;
        foreach (var rel in Discover(tree, schema))
        {
            var doc = Doc.Parse(rel, tree.Read(rel), schema);
            if (doc is null)
            {
                skipped++;
                continue;
            }

            docs.Add(doc);
        }

        return new LoadedCorpus
        {
            Schema = schema,
            Descriptor = descriptor,
            Tree = tree,
            Adopted = Adopted(schema, tree, descriptor),
            Docs = docs,
            Templates = DiscoverTemplates(tree, schema),
            SkippedNoFrontmatter = skipped
        };
    }

    // The types this corpus holds, in schema order. The schema declares every type the tool manages and a
    // corpus adopts as many as it has use for, so everything generated about the taxonomy reads this and
    // describes the corpus instead of the framework's full range.
    //
    // `.corpus.yaml` answers where it declares `types:`, because adoption is a decision the corpus
    // records and the pages follow the decision. Otherwise the filesystem answers: a type counts as
    // adopted where both halves are there, the page and the folder, which is the bar CheckTypeSetup holds
    // a type to. That reading cannot tell a type nobody wanted from one somebody has not finished adding,
    // which is what `types:` exists to say. It stands until the corpus declares, so taking a newer
    // framework never means editing the descriptor in the same breath.
    public static List<TypeSchema> Adopted(Schema schema, Tree tree, CorpusDescriptor descriptor)
    {
        var declared = descriptor.Types;

        return
        [
            .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value)
                .Where(t => declared?.Contains(t.Key, StringComparer.Ordinal) ?? StoodUp(t, tree))
        ];
    }

    // Whether both halves of a type are in the corpus. A half-built type is not adopted: generating a row
    // for it would answer a defect with a link resolving to whichever of the two exists.
    //
    // Asked of the listing, as `CheckTypeSetup` asks it, so that one voice does not generate an index into
    // a folder the other reports as absent.
    public static bool StoodUp(TypeSchema t, Tree tree) =>
        !string.IsNullOrEmpty(t.Page)
        && tree.Exists(t.Page)
        && tree.HasFolder(t.Folder);

    // The template of every collection type that has one. Asked with `OnDisk` rather than of the listing,
    // as type-setup asks it: the question is whether the file a contributor would copy is there, and a
    // type whose template is untracked has a different problem from one with none.
    //
    // A type with no template is skipped in silence — its absence is type-setup's to report, and a type
    // nobody has stood up yet is a valid, quiet state.
    private static List<string> DiscoverTemplates(Tree tree, Schema schema)
    {
        var result = new List<string>();
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var rel = $"{(string.IsNullOrEmpty(t.Folder) ? key : t.Folder)}/{Artefact.Template}";
            if (tree.OnDisk(rel)) result.Add(rel);
        }

        return result;
    }

    // Which of the files the corpus holds are records to validate: markdown, inside a folder the schema
    // maps to a type.
    //
    // The glob names every path and the test below decides which are markdown, because the extension is
    // matched however it is cased and a pattern is not. Corpus order is the listing's own, which `Match`
    // gives in ordinal order.
    private static List<string> Discover(Tree tree, Schema schema)
    {
        // Type pages at the repo root — adrs.md, services.md, data.md, … Each is prose about its
        // records and is checked separately, as a page.
        var typePages = new HashSet<string>(
            schema.ByFolder.Values.Select(t => t.Page).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        var typeFolders = new HashSet<string>(schema.ByFolder.Keys, StringComparer.OrdinalIgnoreCase);

        var result = new List<string>();
        foreach (var rel in tree.Match("**/*"))
        {
            if (!rel.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;
            if (IsExcluded(rel, typePages, typeFolders)) continue;
            result.Add(rel);
        }

        return result;
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
