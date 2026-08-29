namespace kac.core;

// The corpus as one loaded thing: the schema it is judged against, what it holds, and every record
// parsed. A verb asking about records begins by building one of these, so there is a single account of
// what "the corpus" is and no per-command copy of it to drift. `checks`, `bundle` and `update` ask about
// something else and each says so where it skips this.
public sealed class LoadedCorpus
{
    public required Schema Schema;

    // What this corpus records about itself: where it stands against the framework it took, and which
    // types it declares. `Adopted` below is the resolved answer, which is what the rest of the tool reads.
    public required CorpusDescriptor Descriptor;

    // Every path the corpus holds, and a way to read one.
    public required Tree Tree;

    // The types this corpus took. Resolved once, here, because it decides both what is generated and what
    // the corpus is held to having built. Two entry points asking separately could answer differently.
    public required List<TypeSchema> Adopted;

    // The records: every discovered document that carries frontmatter, in corpus order.
    public required List<Doc> Docs;

    // The template each stood-up collection type carries, in corpus order. Held beside the records and
    // never among them, because a template is checked and is not one. Discovered here, so the count the
    // summary reports and the files the validator reads are one list and not two walks that could
    // disagree.
    public required List<string> Templates;

    // Discovered but not migrated. Reported, so a corpus part-way through adoption reads as part-way
    // through and never as smaller than it is.
    public required int SkippedNoFrontmatter;

    // What this corpus consumes, as `.imports/` holds it.
    //
    // The one member carrying a default, because consuming nothing is what a corpus does unless it says
    // otherwise. Every other member here is a fact about any corpus at all.
    public ImportGraph Imports = ImportGraph.None;
}

public static class Corpus
{
    private static readonly string[] SkipDirs = [".git", ".idea", ".claude"];

    // The descriptor a corpus is found by. `kac` walks up for this rather than for `.schema/`, because a
    // repository may author one schema above several corpora.
    public const string Descriptor = ".corpus.yaml";

    // The corpus `start` sits in: the nearest folder at or above it carrying a `.corpus.yaml`, or null
    // where there is none. Every verb but one is answered from what this finds, so where the tool's own
    // files sit says nothing about which corpus it reads. `new` is the one that refuses what this finds.
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

    // What `Tree` is built over. `GitFiles` says why the listing is the git one where there is a git one.
    //
    // The two branches do not answer the same question, and the difference is silent: the walk lists
    // markdown alone, so in a corpus git has never seen, a link to an image or a YAML file resolves to
    // nothing. It is the fallback rather than the path anything is proven on.
    private static List<string> AllFiles(string corpusRoot) =>
        GitFiles.Tracked(corpusRoot) ?? GitFiles.Walk(corpusRoot, "*.md", SkipDirs);

    // A corpus at a path. The listing is taken once and carried on the result as a `Tree`: everything
    // downstream asks it what the corpus holds, and a second `git ls-files` costs more than every check in
    // the tool put together.
    //
    // This is the one place a path becomes a corpus. Everything below it is decided from values.
    //
    // The schema is read from wherever `Schema.FindRoot` lands, which is the corpus itself in a standalone
    // one. Falling back to the corpus root leaves a corpus with no schema anywhere above it failing on the
    // file it cannot open, as it did before the walk existed. `kac` declines such a corpus ahead of this.
    public static LoadedCorpus Load(string corpusRoot)
    {
        var descriptor = CorpusDescriptor.Load(corpusRoot);

        return Load(
            new Tree(
                new HashSet<string>(AllFiles(corpusRoot).Select(f => f.Replace('\\', '/')), StringComparer.Ordinal),
                rel => Files.ReadLf(Path.Combine(corpusRoot, rel)),
                rel => File.Exists(Path.Combine(corpusRoot, rel))),
            Schema.Load(Schema.FindRoot(corpusRoot) ?? corpusRoot),
            descriptor,
            Imports.Load(corpusRoot, descriptor.Consumes));
    }

    // The listing, the schema it is judged against, and what the corpus records about itself. That is
    // everything an entry point needs before it can ask a question, and the whole of what this reads.
    // A caller with a corpus nobody wrote to disk hands over the same three things, so a check can be
    // written against one.
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
    private static List<TypeSchema> Adopted(Schema schema, Tree tree, CorpusDescriptor descriptor)
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

    // The template of every collection type that has one. Asked with `OnDisk`, as type-setup asks it:
    // the question is whether the file a contributor would copy is there, and a type whose template is
    // untracked has a different problem from one with none.
    //
    // A type with no template is skipped in silence. Its absence is type-setup's to report, and a type
    // nobody has stood up yet is a valid, quiet state.
    private static List<string> DiscoverTemplates(Tree tree, Schema schema)
    {
        var result = new List<string>();
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var rel = $"{t.Folder}/{Artefact.Template}";
            if (tree.OnDisk(rel)) result.Add(rel);
        }

        return result;
    }

    // Which of the files the corpus holds are records to validate: markdown, inside a folder the schema
    // maps to a type.
    //
    // The glob names every path and the test below decides which are markdown. The extension is matched
    // however it is cased, which a pattern cannot do. Corpus order is the listing's own, which `Match`
    // gives in ordinal order.
    private static List<string> Discover(Tree tree, Schema schema)
    {
        // Type pages at the corpus root: adrs.md, services.md, data.md, … Each is prose about its
        // records and is checked separately, as a page.
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

        // A record lives in the folder of the type it belongs to, so those folders are the whole of what
        // is read. A root document that is not a type page, and a folder no type claims, are outside what
        // the schema describes and nothing here judges them.
        return !typeFolders.Contains(top);
    }
}
