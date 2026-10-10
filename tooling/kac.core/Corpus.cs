namespace kac.core;

// The single account of what "the corpus" is. `checks`, `bundle` and `update` ask about something else
// and each says so where it skips this.
public sealed class LoadedCorpus
{
    public required Schema Schema;

    // `Adopted` below is the resolved answer, which the rest of the tool reads.
    public required CorpusDescriptor Descriptor;

    public required Tree Tree;

    // Resolved once, here: two entry points asking separately could answer differently.
    public required List<TypeSchema> Adopted;

    public required List<Doc> Docs;

    // Discovered here, so the count the summary reports and the files the validator reads are one list.
    public required List<string> Templates;

    public required int SkippedNoFrontmatter;

    public ImportGraph Imports = ImportGraph.None;
}

public static class Corpus
{
    private static readonly string[] SkipDirs = [".git", ".idea", ".claude"];

    // `kac` walks up for this rather than for `.schema/`, because one schema may sit above several corpora.
    public const string Descriptor = ".corpus.yaml";

    // `new` is the one verb that refuses what this finds.
    public static string? FindRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, Descriptor))) return dir.FullName;
            dir = dir.Parent;
        }

        return null;
    }

    // `GitFiles` says why the listing is the git one where there is one. The walk lists markdown alone, so
    // in a corpus git has never seen a link to an image resolves to nothing.
    private static List<string> AllFiles(string corpusRoot) =>
        GitFiles.Tracked(corpusRoot) ?? GitFiles.Walk(corpusRoot, "*.md", SkipDirs);

    // The listing is taken once: a second `git ls-files` costs more than every check put together.
    public static LoadedCorpus Load(string corpusRoot)
    {
        var descriptor = CorpusDescriptor.Load(corpusRoot);

        return Load(
            new Tree(
                new HashSet<string>(AllFiles(corpusRoot).Select(f => f.Replace('\\', '/')), StringComparer.Ordinal),
                rel => Files.ReadLf(Path.Combine(corpusRoot, rel)),
                rel => File.Exists(Path.Combine(corpusRoot, rel))),
            Schema.LoadNearest(corpusRoot, descriptor.Enums),
            descriptor,
            Imports.Load(corpusRoot, descriptor.Consumes));
    }

    // The schema handed over must be built with the descriptor's `enums:`, as the overload above builds it.
    // One built without them gives a field with no range and no finding to say so.
    public static LoadedCorpus Load(
        Tree tree, Schema schema, CorpusDescriptor descriptor, ImportGraph? imports = null)
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
            SkippedNoFrontmatter = skipped,
            Imports = imports ?? ImportGraph.None
        };
    }

    // `types:` answers where the corpus declares it. Otherwise a type counts as adopted where its page and
    // folder are both there, the bar CheckTypeSetup holds a type to.
    private static List<TypeSchema> Adopted(Schema schema, Tree tree, CorpusDescriptor descriptor)
    {
        var declared = descriptor.Types;

        return
        [
            .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value)
                .Where(t => declared?.Contains(t.Key, StringComparer.Ordinal) ?? StoodUp(t, tree))
        ];
    }

    // Asked of the listing, as `CheckTypeSetup` asks it, so generate and validate agree.
    public static bool StoodUp(TypeSchema t, Tree tree) =>
        !string.IsNullOrEmpty(t.Page)
        && tree.Exists(t.Page)
        && tree.HasFolder(t.Folder);

    // Asked with `OnDisk`, as type-setup asks it. A type with no template is type-setup's to report.
    private static List<string> DiscoverTemplates(Tree tree, Schema schema)
    {
        var result = new List<string>();
        foreach (var (_, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var rel = $"{t.Folder}/{Artefact.Template}";
            if (tree.OnDisk(rel)) result.Add(rel);
        }

        return result;
    }

    // The extension is matched however it is cased, which a glob cannot do.
    private static List<string> Discover(Tree tree, Schema schema)
    {
        var typePages = new HashSet<string>(
            schema.ByFolder.Values.Select(t => t.Page).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        var typeFolders = new HashSet<string>(schema.ByFolder.Keys, StringComparer.OrdinalIgnoreCase);

        return
        [
            .. tree.Match("**/*")
                .Where(rel => rel.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                .Where(rel => !IsExcluded(rel, typePages, typeFolders))
        ];
    }

    private static bool IsExcluded(string rel, HashSet<string> typePages, HashSet<string> typeFolders)
    {
        var parts = rel.Split('/');
        var top = parts[0];

        if (SkipDirs.Contains(top)) return true;
        if (top == "knowledge-as-code") return true;

        if (Artefact.IsReserved(rel)) return true;

        var name = parts[^1];

        if (parts.Length == 1)
        {
            if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("CLAUDE.md", StringComparison.OrdinalIgnoreCase)) return true;
            if (typePages.Contains(name)) return true;
        }

        return !typeFolders.Contains(top);
    }
}
