// ---------------------------------------------------------------------------
// What `kac index` writes, and where
// ---------------------------------------------------------------------------

namespace kac.core;

// The one account of which files carry generated blocks and which blocks each one carries.
//
// Two commands read it and both need the same answer: `index` writes the blocks, and `validate` holds
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
    // `MarkersRequired` is false where deleting the markers is how a corpus declines the block rather than
    // how it breaks it. That is true of `README.md` alone: the file belongs to the corpus, which decides
    // where the block sits and whether it wants one. Everywhere else the file arrives from the framework
    // with its markers, and a marker that has gone is a block that silently stopped being written.
    public readonly record struct BlockFile(string Path, IReadOnlyList<string> Blocks, bool MarkersRequired);

    // Which blocks each file carries. Reads no disk and renders nothing, so a caller can ask what a corpus
    // ought to hold without building a single table.
    //
    // Takes the adopted types rather than the descriptor: adoption is a question the corpus answers from
    // `.corpus.yaml` or, undeclared, from the filesystem — see `Corpus.Adopted` — and resolving it here
    // would put a directory read behind a list of names.
    public static List<BlockFile> Blocks(IReadOnlyList<TypeSchema> adopted) =>
    [
        .. Declare(adopted).Select(spec =>
            new BlockFile(spec.Path, [.. spec.Blocks.Select(b => b.Name)], spec.MarkersRequired))
    ];

    // Every generated file and the full content it should hold, in the order `index` writes them.
    //
    // A file that is not on disk is skipped rather than created: the generator populates structure the
    // corpus has declared and never invents it. `validate` is the one voice that says an adopted type is
    // not set up, so a missing page is reported there rather than papered over here.
    public static List<(string Path, string Content)> Targets(LoadedCorpus corpus)
    {
        var (schema, repoRoot) = (corpus.Schema, corpus.RepoRoot);

        // The types this corpus took, which is what everything below is generated from. The schema
        // declares the framework's full range and a corpus adopts as much of it as it has use for, so
        // generating per schema type would write pages for types the corpus declined — files no list of
        // its types names, and which `index --check` then holds it to keeping fresh.
        var adopted = Corpus.Adopted(schema, repoRoot, corpus.Descriptor);

        // Grouped by type. A document whose folder maps to no schema has nothing to be indexed under;
        // validate is the voice that says so.
        var byType = new Dictionary<string, List<Doc>>();
        foreach (var doc in corpus.Docs)
        {
            if (doc.Type is null) continue;
            (byType.TryGetValue(doc.Type.Folder, out var list) ? list : byType[doc.Type.Folder] = []).Add(doc);
        }

        var targets = new List<(string, string)>();

        // Every adopted type gets an index, populated or not — each type page links to one, so a
        // withheld file is a dead link rather than a tidy absence. Written whole, so it carries no
        // markers and `Blocks` does not name it.
        foreach (var t in adopted)
        {
            if (string.IsNullOrEmpty(t.Folder)) continue;
            if (!Directory.Exists(Path.Combine(repoRoot, t.Folder))) continue;
            var docs = byType.TryGetValue(t.Folder, out var found) ? found : [];
            targets.Add((Path.Combine(repoRoot, t.Folder, Artefact.Index), Generator.IndexPage(t, docs)));
        }

        // Every block a file carries, spliced into one text and offered as one target — a file is written
        // once, so two blocks in the same file cannot each overwrite the other's work. A file carrying no
        // marker resolves to itself, which is what lets a corpus decline the one block it may.
        foreach (var spec in Declare(adopted))
        {
            var full = Path.Combine(repoRoot, spec.Path);
            if (!File.Exists(full)) continue;

            var text = Files.ReadLf(full);
            foreach (var block in spec.Blocks) text = Generator.SpliceBlock(text, block.Name, block.Render(schema));
            targets.Add((full, text));
        }

        return targets;
    }

    // Write every target whose content has moved, and answer with what was written. A file already
    // holding what it should is left alone rather than rewritten, so a regeneration that changes nothing
    // touches nothing and says so.
    //
    // `index` and `mechanism --sync` both end here, so a sync writes what an index would write and the
    // two cannot come to different files.
    public static List<string> Write(IEnumerable<(string Path, string Content)> targets)
    {
        var written = new List<string>();
        foreach (var (path, content) in targets)
        {
            if (File.Exists(path) && Files.ReadLf(path) == content) continue;
            File.WriteAllText(path, content);
            written.Add(path);
        }

        return written;
    }

    private readonly record struct Block(string Name, Func<Schema, string> Render);

    private readonly record struct FileSpec(string Path, bool MarkersRequired, Block[] Blocks);

    // The declaration itself. Everything above reads this and nothing else, so the block a corpus is held
    // to is by construction the block it is written.
    private static List<FileSpec> Declare(IReadOnlyList<TypeSchema> adopted)
    {
        var specs = new List<FileSpec>();

        // The schema and checks blocks derive from the schema alone, so every adopted type gets them
        // whether or not it holds records yet. Restricting this to populated types would leave the markers
        // on an empty page holding hand-written text nothing checks, to be overwritten by whoever adds the
        // type's first record — surfacing the drift at the least convenient moment.
        foreach (var t in adopted)
        {
            if (string.IsNullOrEmpty(t.Page)) continue;
            specs.Add(new FileSpec(t.Page, true,
            [
                new Block($"schema-{t.Key}", schema => Generator.SchemaTable(t, schema)),
                new Block($"checks-{t.Key}", s => Generator.ChecksTable(s, t))
            ]));
        }

        // The pages that describe the taxonomy to a reader rather than to the tool. Each lists types, and
        // the list is the half that was wrong in every corpus that adopted some of them — so none can name
        // a type whose page is not there to open. `metadata.md` also carries the universal field table,
        // which is the schema's alone.
        specs.Add(new FileSpec("knowledge-as-code/metadata.md", true,
        [
            new Block("schema-universal", Generator.UniversalSchemaTable),
            new Block("types-metadata", _ => Generator.MetadataStrip(adopted))
        ]));

        specs.Add(new FileSpec("knowledge-as-code/taxonomy.md", true,
        [
            new Block("types-placement", _ => Generator.PlacementTable(adopted)),
            new Block("types-detail", schema => Generator.TypeCatalogue(schema.Tiers, adopted)),
            new Block("types-versus", _ => Generator.Disambiguations(adopted)),
            new Block("types-graph", _ => Generator.RelationDiagram(adopted)),
            new Block("types-edges", _ => Generator.RelationTable(adopted))
        ]));

        specs.Add(new FileSpec("knowledge-as-code/lineage.md", true,
        [
            new Block("types-lineage", _ => Generator.LineageTable(adopted)),
            new Block("types-collisions", _ => Generator.Collisions(adopted))
        ]));

        specs.Add(new FileSpec("README.md", false,
        [
            new Block("types-index", _ => Generator.TypesIndex(adopted, "knowledge-as-code/taxonomy.md"))
        ]));

        return specs;
    }
}
