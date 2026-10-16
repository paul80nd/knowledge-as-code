namespace kac.core;

/// <summary>
/// The one account of which files carry generated blocks and which blocks each one carries. <c>generate</c> writes the
/// blocks and <c>validate</c> checks the markers, and both read this. The names and the renderers are declared
/// together, and <c>Blocks</c> projects the names out without calling a renderer.
/// </summary>
public static class GeneratedFiles
{
    /// <summary>
    /// A file carrying generated blocks, as <c>validate</c> needs it. <c>MarkersRequired</c> is false where deleting
    /// the markers is how a corpus declines the block, which is true of <c>README.md</c> alone.
    /// <c>tooling/CLAUDE.md</c> says why. <c>FromFramework</c> is true for a page <c>kac update</c> writes whole, so a
    /// lost marker comes back with it.
    /// </summary>
    public readonly record struct BlockFile(string Path, IReadOnlyList<string> Blocks, bool MarkersRequired,
        bool FromFramework);

    /// <summary>
    /// Which blocks each file carries. Reads no disk and renders nothing. Takes the adopted types, resolved once in
    /// <c>Corpus.Adopted</c>.
    /// </summary>
    public static List<BlockFile> Blocks(IReadOnlyList<TypeSchema> adopted) =>
    [
        .. Declare(adopted).Select(spec =>
            new BlockFile(spec.Path, [.. spec.Blocks.Select(b => b.Name)], spec.MarkersRequired,
                spec.Path.StartsWith(FrameworkFolder, StringComparison.Ordinal)))
    ];

    /// <summary>
    /// One generated file: where it goes, what the corpus holds there now, and what it should hold. <c>Current</c> is
    /// null where the corpus holds nothing at that path yet.
    /// </summary>
    public readonly record struct GeneratedFile(string Path, string? Current, string Wanted)
    {
        public bool Stale => Current != Wanted;
    }

    /// <summary>
    /// Every generated file and what it should hold, in the order <c>generate</c> writes them. Reads nothing and writes
    /// nothing. A file the corpus does not hold is skipped, except an <c>_index.md</c>, which is written because each
    /// type page links to one.
    /// </summary>
    public static List<GeneratedFile> Plan(Schema schema, IReadOnlyList<TypeSchema> adopted,
        IEnumerable<Doc> docs, Tree tree)
    {
        // A document whose folder maps to no schema is not indexed. `validate` reports it.
        var byType = new Dictionary<string, List<Doc>>();
        foreach (var doc in docs)
        {
            if (doc.Type is null) continue;
            (byType.TryGetValue(doc.Type.Folder, out var list) ? list : byType[doc.Type.Folder] = []).Add(doc);
        }

        var plan = new List<GeneratedFile>();

        // Written whole, so it carries no markers and `Blocks` does not name it.
        foreach (var t in adopted)
        {
            if (string.IsNullOrEmpty(t.Folder)) continue;
            if (!tree.HasFolder(t.Folder)) continue;
            var rel = $"{t.Folder}/{Artefact.Index}";
            var records = byType.TryGetValue(t.Folder, out var found) ? found : [];
            plan.Add(new GeneratedFile(rel, tree.Exists(rel) ? tree.Read(rel) : null, Generator.IndexPage(t, records)));
        }

        // Every block a file carries, spliced into one text and written once, so two blocks in one file cannot
        // overwrite each other. A file carrying no marker resolves to itself.
        foreach (var spec in Declare(adopted))
        {
            if (!tree.Exists(spec.Path)) continue;

            var current = tree.Read(spec.Path);
            var wanted = current;
            foreach (var block in spec.Blocks) wanted = Markers.SpliceBlock(wanted, block.Name, block.Render(schema));
            plan.Add(new GeneratedFile(spec.Path, current, wanted));
        }

        return plan;
    }

    /// <summary>
    /// Write every entry whose content has moved, and return what was written. <c>generate</c> and <c>update</c> both
    /// end here.
    /// </summary>
    public static List<string> Write(string corpusRoot, IEnumerable<GeneratedFile> plan)
    {
        var written = new List<string>();
        foreach (var file in plan)
        {
            if (!file.Stale) continue;
            File.WriteAllText(Path.Combine(corpusRoot, file.Path), file.Wanted);
            written.Add(file.Path);
        }

        return written;
    }

    /// <summary>The overlay folder <c>kac update</c> writes from the template into every corpus.</summary>
    private const string FrameworkFolder = "knowledge-as-code/";

    private readonly record struct Block(string Name, Func<Schema, string> Render);

    private readonly record struct FileSpec(string Path, bool MarkersRequired, Block[] Blocks);

    /// <summary>
    /// How far a block's own file sits below the corpus root, as the prefix a relative link from it climbs by.
    /// </summary>
    private static string Up(string path) =>
        string.Concat(Enumerable.Repeat("../", path.Count(c => c == '/')));

    /// <summary>The declaration itself. Everything above reads this and nothing else.</summary>
    private static List<FileSpec> Declare(IReadOnlyList<TypeSchema> adopted)
    {
        // The schema and checks blocks derive from the schema alone, so every adopted type gets them whether or not it
        // holds records yet.
        var specs = (from t in adopted
            where !string.IsNullOrEmpty(t.Page)
            select new FileSpec(t.Page, true,
            [
                new Block($"schema-{t.Key}", schema => Generator.SchemaTable(t, schema, Up(t.Page))),
                new Block($"checks-{t.Key}", s => ChecksTable.Render(s, t, Up(t.Page)))
            ])).ToList();

        // The pages that describe the taxonomy to a reader. Each lists only the types this corpus adopted.
        // `metadata.md` also carries the universal field table and the universal checks.
        specs.Add(new FileSpec("knowledge-as-code/metadata.md", true,
        [
            new Block("schema-universal", Generator.UniversalSchemaTable),
            new Block("checks-universal", ChecksTable.RenderUniversal),
            new Block("types-metadata", _ => Generator.MetadataStrip(adopted, Up("knowledge-as-code/metadata.md")))
        ]));

        specs.Add(new FileSpec("knowledge-as-code/taxonomy.md", true,
        [
            new Block("types-placement", _ => Generator.PlacementTable(adopted, Up("knowledge-as-code/taxonomy.md"))),
            new Block("types-detail",
                schema => Generator.TypeCatalogue(schema.Tiers, adopted, Up("knowledge-as-code/taxonomy.md"))),
            new Block("types-versus", _ => Generator.Disambiguations(adopted)),
            new Block("types-graph", _ => Generator.RelationDiagram(adopted)),
            new Block("types-edges", _ => Generator.RelationTable(adopted))
        ]));

        specs.Add(new FileSpec("knowledge-as-code/lineage.md", true,
        [
            new Block("types-lineage", _ => Generator.LineageTable(adopted, Up("knowledge-as-code/lineage.md"))),
            new Block("types-collisions", _ => Generator.Collisions(adopted))
        ]));

        specs.Add(new FileSpec("README.md", false,
        [
            new Block("types-index",
                _ => Generator.TypesIndex(adopted, "knowledge-as-code/taxonomy.md", Up("README.md")))
        ]));

        return specs;
    }
}
