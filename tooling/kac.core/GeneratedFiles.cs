namespace kac.core;

// The one account of which files carry generated blocks and which blocks each one carries.
//
// Two commands read it and both need the same answer: `generate` writes the blocks, and `validate` holds
// each file to still carrying the markers to write between. A block named in one of those places and not
// the other is written and never checked, or checked for and never written, and neither state announces
// itself.
//
// The names and the renderers are declared together, so the two cannot come apart. `Blocks` projects the
// names out without calling a renderer, which is what lets `validate` ask what a file should carry
// without building any of it.
public static class GeneratedFiles
{
    // A file carrying generated blocks, as `validate` needs it: the path, the blocks, and whether the
    // markers have to be there.
    //
    // `MarkersRequired` is false where deleting the markers is how a corpus declines the block, which is
    // true of `README.md` alone. `tooling/CLAUDE.md` says why that file is the exception.
    public readonly record struct BlockFile(string Path, IReadOnlyList<string> Blocks, bool MarkersRequired);

    // Which blocks each file carries. Reads no disk and renders nothing, so a caller can ask what a corpus
    // ought to hold without building a single table.
    //
    // Takes the adopted types and not the descriptor. Adoption is resolved once, when the corpus is
    // loaded (see `Corpus.Adopted`), so what is generated and what the corpus is held to having built
    // cannot be answered differently.
    public static List<BlockFile> Blocks(IReadOnlyList<TypeSchema> adopted) =>
    [
        .. Declare(adopted).Select(spec =>
            new BlockFile(spec.Path, [.. spec.Blocks.Select(b => b.Name)], spec.MarkersRequired))
    ];

    // One generated file: where it goes, what the corpus holds there now, and what it should hold.
    // `Current` is null where the corpus holds nothing at that path yet. That is a file to write, and
    // not one that has drifted.
    public readonly record struct GeneratedFile(string Path, string? Current, string Wanted)
    {
        public bool Stale => Current != Wanted;
    }

    // Every generated file and what it should hold, in the order `generate` writes them.
    //
    // Reads nothing and writes nothing: everything it needs arrives in its arguments, so what `generate`
    // comes to is decidable from a listing and a set of records. `Write` is the half that acts.
    //
    // A file the corpus does not hold is skipped. The generator populates structure the corpus has
    // declared and never invents it, and `validate` is the one voice that says an adopted type is not
    // set up. An `_index.md` is the exception: it is written whether or not it is there, because each
    // type page links to one and a withheld file leaves a dead link.
    public static List<GeneratedFile> Plan(Schema schema, IReadOnlyList<TypeSchema> adopted,
        IEnumerable<Doc> docs, Tree tree)
    {
        // Grouped by type. A document whose folder maps to no schema has nothing to be indexed under;
        // validate is the voice that says so.
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

        // Every block a file carries, spliced into one text and offered as one entry. A file is written
        // once, so two blocks in the same file cannot each overwrite the other's work. A file carrying no
        // marker resolves to itself, which is what lets a corpus decline the one block it may.
        foreach (var spec in Declare(adopted))
        {
            if (!tree.Exists(spec.Path)) continue;

            var current = tree.Read(spec.Path);
            var wanted = current;
            foreach (var block in spec.Blocks) wanted = Generator.SpliceBlock(wanted, block.Name, block.Render(schema));
            plan.Add(new GeneratedFile(spec.Path, current, wanted));
        }

        return plan;
    }

    // Write every entry whose content has moved, and answer with what was written. A file already holding
    // what it should is left alone, so a regeneration that changes nothing touches nothing and says so.
    // The plan decided all of it, so this asks the disk nothing.
    //
    // `generate` and `update` both end here, so an update writes what a generation would write and
    // the two cannot come to different files.
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

    private readonly record struct Block(string Name, Func<Schema, string> Render);

    private readonly record struct FileSpec(string Path, bool MarkersRequired, Block[] Blocks);

    // How far a block's own file sits below the corpus root, as the prefix a link from it climbs by. A
    // generated link is written relative so it resolves wherever the corpus sits, whether that is the root
    // of a wiki or a subfolder of a documentation site.
    private static string Up(string path) =>
        string.Concat(Enumerable.Repeat("../", path.Count(c => c == '/')));

    // The declaration itself. Everything above reads this and nothing else, so the block a corpus is held
    // to is by construction the block it is written.
    private static List<FileSpec> Declare(IReadOnlyList<TypeSchema> adopted)
    {
        // The schema and checks blocks derive from the schema alone, so every adopted type gets them
        // whether or not it holds records yet. Restricting this to populated types would leave the markers
        // on an empty page holding hand-written text nothing checks, to be overwritten by whoever adds the
        // type's first record, which surfaces the drift at the least convenient moment.
        var specs = (from t in adopted
            where !string.IsNullOrEmpty(t.Page)
            select new FileSpec(t.Page, true,
            [
                new Block($"schema-{t.Key}", schema => Generator.SchemaTable(t, schema, Up(t.Page))),
                new Block($"checks-{t.Key}", s => Generator.ChecksTable(s, t))
            ])).ToList();

        // The pages that describe the taxonomy to a reader rather than to the tool. Each lists types, and
        // the list is the half that was wrong in every corpus that adopted some of them. So none can
        // name a type whose page is not there to open. `metadata.md` also carries the universal field table,
        // which is the schema's alone.
        specs.Add(new FileSpec("knowledge-as-code/metadata.md", true,
        [
            new Block("schema-universal", Generator.UniversalSchemaTable),
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
