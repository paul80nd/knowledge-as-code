using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// Named relative to the export root, not to the corpus.
public sealed record ExportFile(string Path, string Content);

// What an export comes to: the files it writes, the types it found something to write for, and the
// records it was told to leave behind. Named before anything is written, as `GeneratedFiles.Plan` is.
// Deciding and doing stay apart. A test can ask what an export would contain without a filesystem.
//
// `Withheld` and `Unread` are the two things the output cannot say about itself. A corpus that excludes
// drafts publishes a smaller vocabulary than it holds. A link naming a record and no term inside it
// leaves a cross-reference the export could not carry. Neither leaves a mark in the export, so the run
// that built it is the last place anyone will see them.
//
// `Refused` holds the reasons an export cannot be written at all, and is empty for every export that
// can. A plan carrying one carries no files: two corpora disagreeing about a type is not something to
// write a smaller export around.
public sealed record ExportPlan(
    IReadOnlyList<ExportFile> Files,
    IReadOnlyList<ExportedType> Types,
    IReadOnlyList<string> Withheld,
    IReadOnlyList<string> Unread,
    IReadOnlyList<string> Refused);

// The facts that differ between two runs over one commit, gathered so the exporter takes them rather
// than reads them. A caller wanting byte-identical output holds them still. A test supplies its own
// clock instead of racing the real one.
public sealed record ExportRun(string GeneratedAt, DateOnly Today, string? Commit, bool? Dirty);

// The corpus projected as data, for a consumer that reads it rather than cloning it.
//
// This reads each type's `export:` block and holds no list of its own, so a new type needs no line here.
// `docs/design/export.md` says what it all comes to, and why.
public static class Exporter
{
    // The shape of the output, versioned independently of anything the corpus says about itself.
    // `Bundler` refuses an export built to another shape, so moving this number means rebuilding every
    // export on disk. `docs/design/export.md` says what moves it.
    public const int FormatVersion = 4;

    public const string ManifestFile = "manifest.json";

    // The key an inherited part line names its producer by, and the key into `sources` in the manifest.
    // A line without it is this corpus's own, which is the rule a citation already follows: `eng:pol-VURM`
    // names another corpus and a bare `pol-VURM` names this one.
    public const string ShortcodeKey = "shortcode";

    // What an export comes to, given a loaded corpus, the addresses its published form has, and the
    // corpora it consumes.
    //
    // `type` narrows what is written and never what is read. The corpus arrives whole.
    //
    // `run` holds everything that varies between runs, so nothing here reads a clock. Two runs from
    // the same tree produce identical bytes but for the timestamp.
    //
    // `inherited` is what `.imports/` holds, carried through so a consumer greps one file per type
    // instead of learning which corpus wrote which rule. `docs/design/export.md` says what merging
    // settles and what it refuses.
    public static ExportPlan Plan(LoadedCorpus corpus, Publishing? publishing, string? type, ExportRun run,
        IReadOnlyList<InheritedCorpus>? inherited = null)
    {
        var consumed = Wanted(inherited ?? [], type);

        // Nothing is written where two corpora disagree about a type. A merged file whose halves are
        // shaped differently reads as one file and answers two ways.
        var refused = Disagreements(corpus, consumed, type);
        if (refused.Count > 0) return new ExportPlan([], [], [], [], refused);

        var files = new List<ExportFile>();
        var types = new List<ExportedType>();
        var withheld = new List<string>();
        var unread = new List<string>();

        foreach (var key in Keys(corpus, consumed, type))
        {
            var local = corpus.Adopted.FirstOrDefault(t => t.Key == key && t.Export is not null);
            var lines = new List<string>();
            var records = 0;

            if (local is { Export: { } export })
            {
                var held = corpus.Docs.Where(d => d.Type?.Key == key)
                    .ToLookup(d => Travels(d, corpus.Descriptor, run.Today));

                withheld.AddRange(held[false].Select(Id).OrderBy(id => id, StringComparer.Ordinal));

                var own = Ordered([.. held[true]]);
                records += own.Count;

                foreach (var doc in own)
                    files.Add(new ExportFile($"{key}/{Id(doc)}.json",
                        Serialize(Record(doc, local, corpus.Schema, publishing))));

                if (export.Parts.Length > 0 && local.Parts is not null)
                    lines.AddRange(PartLines(own, local, corpus.Tree, unread));
            }

            // Each consumed corpus after this one's own, in shortcode order, and each keeping the order
            // its producer published. Sorting across corpora would interleave them and leave neither
            // corpus's own ordering readable, which is the one thing a flat file's order carries.
            foreach (var from in consumed)
            {
                if (from.Types.FirstOrDefault(t => t.Type == key) is not { } theirs) continue;

                records += theirs.Records.Count;

                foreach (var record in theirs.Records)
                    files.Add(new ExportFile($"{theirs.Dir}/{from.Shortcode}/{record.Name}", record.Content));

                lines.AddRange(theirs.PartLines.Select(line => Stamped(line, from.Shortcode, theirs)));
            }

            if (records == 0) continue;

            var partsFile = lines.Count > 0 ? PartsPath(key, local, consumed) : null;
            if (partsFile is not null)
                files.Add(new ExportFile(partsFile, string.Concat(lines.Select(l => l + "\n"))));

            types.Add(Entry(key, local, consumed, records, lines.Count, partsFile));
        }

        files.Add(new ExportFile(ManifestFile,
            Serialize(new ExportManifest(
                FormatVersion,
                corpus.Descriptor.Name,
                corpus.Descriptor.Shortcode,
                corpus.Descriptor.ContentVersion,

                // The descriptor calls this `upstream.template-version`. Renaming the published name is
                // gated behind a move of `FormatVersion` above, which a consumer reads.
                corpus.Descriptor.TemplateVersion,
                run.Commit,
                run.Dirty,
                run.GeneratedAt,
                About(corpus.Descriptor),
                Addresses(corpus.Descriptor, publishing),
                Sources(consumed),
                types))));

        // The manifest is built last, because it reports what the rest of the run produced. Sorting
        // every file by path makes the listing a caller prints read in the order they sit on disk.
        return new ExportPlan([.. files.OrderBy(f => f.Path, StringComparer.Ordinal)], types, withheld,
            [.. unread.Distinct(StringComparer.Ordinal).OrderBy(u => u, StringComparer.Ordinal)], []);
    }

    // Every corpus a line may name: the ones this corpus consumes, and the ones they consumed in turn.
    //
    // A grandparent's records arrive inside its child's export already carrying the child's account of
    // where that grandparent publishes. Carrying the list forward is what makes a chain of any depth
    // resolve, and `Disagreements` is what refuses two accounts of one corpus.
    private static List<ExportSource> Sources(List<InheritedCorpus> consumed)
    {
        var found = new Dictionary<string, ExportSource>(StringComparer.Ordinal);

        foreach (var from in consumed)
        {
            found.TryAdd(from.Shortcode,
                new ExportSource(from.Shortcode, from.Corpus, from.ContentVersion, from.Publishing));

            foreach (var theirs in from.Sources) found.TryAdd(theirs.Shortcode, theirs);
        }

        return [.. found.Values.OrderBy(s => s.Shortcode, StringComparer.Ordinal)];
    }

    // The consumed corpora this run carries, narrowed to the type asked for and ordered by shortcode so
    // two runs over one tree write the same bytes.
    private static List<InheritedCorpus> Wanted(IReadOnlyList<InheritedCorpus> inherited, string? type) =>
    [
        .. inherited
            .Select(c => type is null
                ? c
                : c with { Types = [.. c.Types.Where(t => t.Type == type)] })
            .Where(c => c.Types.Count > 0 || type is null)
            .OrderBy(c => c.Shortcode, StringComparer.Ordinal)
    ];

    // Every type this export writes something for: the ones this corpus adopted, and the ones it holds
    // only because a corpus it consumes exported them. A consumer receives the second kind whole, so a
    // rule citing a clause of a policy this corpus never adopted resolves against the policy itself.
    private static List<string> Keys(LoadedCorpus corpus, List<InheritedCorpus> consumed, string? type)
    {
        var keys = new List<string>();

        foreach (var t in corpus.Adopted)
            if (t.Export is not null && (type is null || t.Key == type))
                keys.Add(t.Key);

        foreach (var key in consumed.SelectMany(c => c.Types).Select(t => t.Type)
                     .Distinct(StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal))
            if (!keys.Contains(key, StringComparer.Ordinal) && (type is null || key == type))
                keys.Add(key);

        return keys;
    }

    // What stops the run, in the words a person acts on. Two corpora exporting one type at two shapes,
    // and two carrying its sections at two fidelities.
    //
    // Both would merge without complaint and leave a consumer reading one file that answers two ways: a
    // key present on half the lines, or a section a skill was told always travels standing empty on half
    // the records. Neither is visible in the output, so the run that built it is where it has to stop.
    private static List<string> Disagreements(LoadedCorpus corpus, List<InheritedCorpus> consumed, string? type)
    {
        var found = new List<string>();
        var seen = new Dictionary<string, ExportSource>(StringComparer.Ordinal);

        // Two corpora consumed here can each have consumed a third, at two versions. A line naming that
        // third resolves through whichever account won, so its links would land on a commit half the
        // records were never read at. One account or nothing.
        foreach (var source in consumed.SelectMany(c =>
                     c.Sources.Prepend(new ExportSource(
                         c.Shortcode, c.Corpus, c.ContentVersion, c.Publishing))))
        {
            if (seen.TryAdd(source.Shortcode, source)) continue;
            if (seen[source.Shortcode] == source) continue;

            found.Add($"'{source.Shortcode}' arrives twice and differently: at "
                      + $"{seen[source.Shortcode].ContentVersion} and at {source.ContentVersion}. A line "
                      + "naming it could resolve either way.");
        }

        foreach (var key in consumed.SelectMany(c => c.Types).Select(t => t.Type)
                     .Distinct(StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal))
        {
            if (type is not null && key != type) continue;

            var shapes = new List<(string Where, int Shape, IReadOnlyDictionary<string, string> Sections)>();

            if (corpus.Adopted.FirstOrDefault(t => t.Key == key)?.Export is { } export)
                shapes.Add((corpus.Descriptor.Shortcode ?? "this corpus", export.Version,
                    export.Sections.ToDictionary(e => e.Section, e => e.Fidelity, StringComparer.Ordinal)));

            foreach (var from in consumed)
                if (from.Types.FirstOrDefault(t => t.Type == key) is { } theirs)
                    shapes.Add((from.Shortcode, theirs.ShapeVersion, theirs.Sections));

            var first = shapes[0];

            foreach (var (where, shape, sections) in shapes.Skip(1))
            {
                if (shape != first.Shape)
                    found.Add($"'{key}' is exported at shape {first.Shape} by {first.Where} and at shape "
                              + $"{shape} by {where}. A merged file cannot hold both.");
                else if (!Same(first.Sections, sections))
                    found.Add($"'{key}' carries its sections at one fidelity in {first.Where} and another in "
                              + $"{where}. A merged file cannot promise both.");
            }
        }

        return found;
    }

    private static bool Same(
        IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b) =>
        a.Count == b.Count && a.All(e => b.TryGetValue(e.Key, out var v) && v == e.Value);

    // Where one type's parts file lands. This corpus's own name for a part decides it where the corpus
    // adopted the type, and the producer's where it did not, so a consumer reading a type it never
    // adopted finds the file that type's own manifest entry names.
    private static string? PartsPath(string key, TypeSchema? local, List<InheritedCorpus> consumed)
    {
        if (local?.Parts is { } spec) return $"{key}/{spec.Noun}s.jsonl";

        return consumed.SelectMany(c => c.Types).FirstOrDefault(t => t.Type == key)?.PartsFile;
    }

    // One type's manifest entry, counting what this corpus wrote and what it inherited together. A
    // consumer reads one number per question, and which corpus a record came from is a fact about the
    // line rather than about the type.
    private static ExportedType Entry(string key, TypeSchema? local, List<InheritedCorpus> consumed,
        int records, int parts, string? partsFile)
    {
        if (local?.Export is { } export)
            return new ExportedType(
                key, export.Version, records, parts, key, partsFile,
                partsFile is null ? null : KeyFrom(export, PartLineSource.RecordId),
                partsFile is null ? null : KeyFrom(export, PartLineSource.PartKey),
                partsFile is null ? null : KeyFrom(export, PartLineSource.PartId),
                partsFile is null ? null : KeyFrom(export, PartLineSource.PartSeeAlso),
                export.Sections.ToDictionary(e => e.Section, e => e.Fidelity, StringComparer.Ordinal));

        var theirs = consumed.SelectMany(c => c.Types).First(t => t.Type == key);

        return new ExportedType(
            key, theirs.ShapeVersion, records, parts, theirs.Dir, partsFile,
            theirs.RecordKey, theirs.PartKey, theirs.IdKey, theirs.SeeAlsoKey, theirs.Sections);
    }

    // One inherited line, with its producer's shortcode written onto every key that holds an id and onto
    // the line itself. Everything else is the producer's bytes, re-serialised because the object was
    // parsed to reach those keys.
    //
    // A value already carrying a prefix keeps the one it has. That is what makes a grandparent arrive
    // labelled once: `eng` merging `gp` stamps only its own unprefixed lines, and this corpus merging
    // `eng` leaves `gp:` alone.
    private static string Stamped(string line, string shortcode, InheritedType type)
    {
        if (JsonRead.Parse(line) is not { } read) return line;

        foreach (var key in new[] { type.IdKey, type.RecordKey })
            if (key is not null && JsonRead.Str(read[key]) is { } id)
                read[key] = JsonValue.Create(Scoped(id, shortcode));

        if (type.SeeAlsoKey is { } seeAlso && read[seeAlso] is JsonArray targets)
            read[seeAlso] = new JsonArray([
                .. targets.Select(t => (JsonNode?)JsonValue.Create(
                    JsonRead.Str(t) is { } v ? Scoped(v, shortcode) : null))
            ]);

        // A line arriving with a shortcode was written by a corpus further up the chain, and names it.
        // Overwriting it would file a grandparent's record under the corpus this one fetched it through,
        // and send every link for it to the wrong repository.
        read[ShortcodeKey] ??= JsonValue.Create(shortcode);
        return Serialize(read);
    }

    // An id as this corpus writes it: the producer's own spelling, behind the shortcode that names the
    // producer, unless it already names one.
    private static string Scoped(string id, string shortcode) =>
        id.Contains(':', StringComparison.Ordinal) ? id : $"{shortcode}:{id}";

    // Replace the export whole: delete what is there, then write. Overwriting in place would leave a
    // deleted record readable in the output indefinitely, and nothing downstream would catch it.
    // `docs/cli/export.md` says why an untracked artefact makes that the only safe order.
    //
    // What is deleted is `Dist.Export` and never `.dist/` itself, because the bundle sits beside it
    // under the same root and an export is not entitled to take it.
    public static List<string> Write(string corpusRoot, ExportPlan plan)
    {
        var root = Path.Combine(corpusRoot, Dist.Export.Replace('/', Path.DirectorySeparatorChar));
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);

        var written = new List<string>();
        foreach (var file in plan.Files)
        {
            var full = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
            Files.OpenFolderFor(full);
            File.WriteAllText(full, file.Content);
            written.Add($"{Dist.Export}/{file.Path}");
        }

        return written;
    }

    // Whether a record travels, which by default every record does. A corpus may exclude what has not
    // settled: a draft, or a record whose review date has passed. `docs/cli/export.md` says why
    // carrying both, with their state attached, is the default.
    private static bool Travels(Doc doc, CorpusDescriptor descriptor, DateOnly today)
    {
        if (descriptor.ExportExclude.Count == 0) return true;

        if (descriptor.ExportExclude.Contains(CorpusDescriptor.ExcludeDraft, StringComparer.Ordinal)
            && string.Equals(doc.FrontScalar("status"), "draft", StringComparison.OrdinalIgnoreCase))
            return false;

        // A record carrying no `review-by`, or one nobody can parse, is never overdue. An absent date
        // is the field's own problem and the validator's to report. Reading it as expired here would
        // withhold a record over a fault in its frontmatter.
        return !descriptor.ExportExclude.Contains(CorpusDescriptor.ExcludeOverdue, StringComparer.Ordinal)
               || !DateOnly.TryParse(doc.FrontScalar("review-by"), out var due)
               || due >= today;
    }

    // The records of one type: roots by id, and each root's chain depth-first beneath it.
    //
    // A record narrowing nothing is a root, as is one narrowing something outside the set: it has
    // nothing here to sit beneath. A type whose records narrow nothing at all sorts by id throughout,
    // which is the same rule with every record a root.
    //
    // What a reader may take from that order, and what they may not, is in `docs/design/export.md`.
    // The short of it: generality holds within a chain and says nothing across roots.
    //
    // Two records sharing an id would share an output filename, and the `TryAdd` below keeps the first.
    // Nothing here guards it. `id-unique` reports a duplicate as an error, so the corpus is one
    // `validate` already refuses, and a second account of that fault here would report it in worse words.
    private static List<Doc> Ordered(List<Doc> docs)
    {
        var byId = new Dictionary<string, Doc>(StringComparer.Ordinal);
        foreach (var d in docs.OrderBy(Id, StringComparer.Ordinal))
            byId.TryAdd(Id(d), d);

        var children = new Dictionary<string, List<Doc>>(StringComparer.Ordinal);
        var roots = new List<Doc>();
        foreach (var d in byId.Values)
        {
            var above = d.FrontScalar("narrows");
            if (above is { Length: > 0 } && byId.ContainsKey(above) && above != Id(d))
            {
                if (!children.TryGetValue(above, out var list)) children[above] = list = [];
                list.Add(d);
            }
            else
            {
                roots.Add(d);
            }
        }

        var ordered = new List<Doc>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var root in roots.OrderBy(Id, StringComparer.Ordinal)) Walk(root);

        // A cycle leaves its members unreachable from any root. They are still records of the corpus,
        // so the export carries them in id order, after everything the ordering could place. The
        // validator reports the cycle. An export that silently held records back would be a second,
        // quieter account of what the corpus contains.
        foreach (var d in byId.Values.OrderBy(Id, StringComparer.Ordinal))
            if (seen.Add(Id(d)))
                ordered.Add(d);

        return ordered;

        void Walk(Doc d)
        {
            if (!seen.Add(Id(d))) return;
            ordered.Add(d);
            if (!children.TryGetValue(Id(d), out var below)) return;
            foreach (var child in below.OrderBy(Id, StringComparer.Ordinal)) Walk(child);
        }
    }

    private static ExportRecord Record(Doc doc, TypeSchema t, Schema schema, Publishing? publishing)
    {
        var export = t.Export!;
        var link = publishing?.Link(doc.Rel);

        var fields = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);
        foreach (var name in export.Fields)
            fields[name] = name == Generator.Title
                ? JsonValue.Create(Absent(doc.H1))
                : Value(doc.FrontNode(name), schema.EffectiveField(t, name));

        var sections = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var (name, fidelity) in export.Sections)
            if (doc.Sections.FirstOrDefault(s =>
                    string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase)) is { } section)
                sections[name] = Carry(Body(doc, section), fidelity);

        return new ExportRecord(t.Key, doc.Rel, fields, sections, Links(link));
    }

    // A section's own words, with its link reference definitions taken out.
    //
    // The definitions sit in a block at the foot of the document, so they fall inside whichever section
    // is written last: `Exceptions` on a policy. They render as nothing, and a consumer reading the
    // section as prose would meet a run of paths a reader of the page never sees. `Doc.DefinitionSpans`
    // is what locates them, because a line that looks like one inside a fenced block is not one.
    private static string Body(Doc doc, Section section)
    {
        var kept = new StringBuilder();
        var at = section.BodyStart;

        foreach (var (start, end) in doc.DefinitionSpans
                     .Where(d => d.Start >= section.BodyStart && d.Start < section.BodyEnd)
                     .OrderBy(d => d.Start))
        {
            if (start > at) kept.Append(doc.Text[at..start]);
            at = Math.Max(at, Math.Min(end, section.BodyEnd));
        }

        return kept.Append(doc.Text[at..section.BodyEnd]).ToString();
    }

    // One section, cut to the fidelity its type declared. `docs/design/export.md` says what each promises.
    //
    // The unwrap comes first, so a summary is the paragraph the author wrote rather than the line the
    // wrap column happened to end.
    private static string? Carry(string body, string fidelity)
    {
        var text = Unwrap(body);

        return fidelity switch
        {
            ExportSpec.Reference => null,
            ExportSpec.Summary => text.Split("\n\n", 2)[0],
            _ => text
        };
    }

    // One declared field as JSON, in the shape its type declared. A field declared as a list is an array
    // however the record wrote it, so `depends-on: svc-a` and `depends-on: [svc-a]` both reach a consumer
    // as an array of one entry. Everything else is the scalar, which is what `string`, `date`, `enum` and
    // `id` all come to here.
    //
    // The declaration decides the shape because a consumer holds the declaration and reads one key one
    // way. A shape read off the document would vary record by record. `docs/design/export.md` states it.
    private static JsonNode? Value(YamlNode? node, FieldSpec? spec)
    {
        if (spec?.Type != "list") return JsonValue.Create(Absent((node as YamlScalarNode)?.Value));

        // A list written as one scalar is the one-entry case, as `Doc.FrontList` reads it. `list` refuses
        // that shape, so the corpus is one `validate` already reports, and reading it the second way here
        // keeps the two accounts of a field the same.
        //
        // An empty list arrives as `null` beside the absent field, because `Absent` spells every absence
        // one way and a consumer meeting `[]` as well would have a second question about the same record.
        IEnumerable<YamlNode> items = node switch
        {
            YamlSequenceNode seq => seq.Children,
            null => [],
            _ => [node]
        };

        var array = new JsonArray();
        foreach (var item in items)
            if (Entry(item, spec) is { } entry) array.Add(entry);

        return array.Count > 0 ? array : null;
    }

    // One entry of a list. A scalar entry is its own value, and an entry the type declares as an object
    // carries the keys that declaration names, each read back through `Value` so a key holding a list of
    // its own travels as an array too.
    //
    // An entry written in the other shape is dropped. `entry-shape` reports it, so the corpus is one
    // `validate` already refuses, and a guess here would hand a consumer a second reading of that record.
    private static JsonNode? Entry(YamlNode node, FieldSpec spec)
    {
        if (spec.Of != "object") return JsonValue.Create(Absent((node as YamlScalarNode)?.Value));
        if (node is not YamlMappingNode map || spec.Entry is null) return null;

        var obj = new JsonObject();
        foreach (var key in spec.Entry) obj[key.Name] = Value(Yaml.Get(map, key.Name), key);

        return obj;
    }

    // Every absent value an export writes, spelled one way. A corpus writing `narrows:` with nothing
    // after it has not narrowed anything, so blank and missing arrive as the same `null`. What that buys
    // a consumer is in `docs/design/export.md`.
    private static string? Absent(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    // The source's wrap column, taken back out. Blank lines are the author's and stay. A block a joiner
    // would mangle is left as written. `docs/design/export.md` says why the export does this at
    // all, and why the doubtful cases go the way they do.
    private static string Unwrap(string text)
    {
        var paragraphs = text.Replace("\r\n", "\n").Trim().Split("\n\n");

        return string.Join("\n\n", paragraphs.Select(p =>
        {
            var lines = p.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            return lines.Any(Structural) ? p.Trim() : string.Join(" ", lines);
        }));

        // Whether a line opens a block rather than continuing a sentence.
        //
        // A bullet and a heading need the marker and then a space. That space is what tells `- item`
        // from an em-dash clause, and `**Not:**` from a bullet. A quote, a table and a fence need no
        // such test, because none of their markers opens an ordinary sentence.
        static bool Structural(string line) =>
            Bullet(line) || Heading(line) || line[0] is '>' or '|' or '`' || Ordered(line);

        static bool Bullet(string line) =>
            (line[0] is '-' or '*' or '+') && line.Length > 1 && line[1] == ' ';

        // A run of hashes and then a space, so `## Scope` is a heading and `#tag` is a word.
        static bool Heading(string line)
        {
            var i = 0;
            while (i < line.Length && line[i] == '#') i++;
            return i > 0 && i < line.Length && line[i] == ' ';
        }

        // `1.` or `1)` and then a space, so a sentence that opens with a number is not a list.
        static bool Ordered(string line)
        {
            var i = 0;
            while (i < line.Length && char.IsDigit(line[i])) i++;
            return i > 0 && i + 1 < line.Length && line[i] is '.' or ')' && line[i + 1] == ' ';
        }
    }

    // What the corpus says about itself, carried through unchanged. Nothing is filled in for a key the
    // descriptor left empty: a licence nobody chose and an author nobody named are claims about a person.
    private static ExportAbout About(CorpusDescriptor descriptor) =>
        new(descriptor.DisplayName, descriptor.Description,
            descriptor.AuthorName is null && descriptor.AuthorUrl is null
                ? null
                : new ExportAuthor(descriptor.AuthorName, descriptor.AuthorUrl),
            descriptor.License);

    // What this type calls the key filled from one source, and null where its line carries none. A
    // consumer addressing a part needs the two that say which record and which part, and only the type
    // knows the words it chose for them.
    private static string? KeyFrom(ExportSpec export, string source) =>
        export.Line.FirstOrDefault(l => string.Equals(l.Source, source, StringComparison.Ordinal)).Key;

    // Every addressable part of this corpus's own records, one part to a line. JSONL rather than pretty
    // JSON, and each line repeats what a reader would otherwise have to look up.
    // `docs/design/export.md` says what that costs and what it buys.
    //
    // The lines rather than the file, because a consumed corpus's lines join them in one file and
    // `Plan` is where the two are put together.
    //
    // What a line holds is the type's to declare. Nothing here names a key, so a second type exporting
    // parts costs an `export.parts.line:` block and no line of C#.
    private static List<string> PartLines(List<Doc> records, TypeSchema t, Tree tree, List<string> unread)
    {
        var spec = t.Parts!;
        var byPath = records.ToDictionary(d => d.Rel, StringComparer.Ordinal);
        var lines = new List<string>();
        var footnotes = t.DeclaredFields.Select(f => f.MirrorsCitations).OfType<string>().ToList();

        foreach (var doc in records)
        {
            var id = Id(doc);
            foreach (var row in Ordered(doc.Parts, spec))
            {
                if (row.Id is not { Length: > 0 } partId) continue;

                var (lead, aside) = Split(doc, row, spec.Aside, footnotes);
                var part = new Part(doc, row, partId, id, lead, aside, SeeAlso(doc, row, byPath, tree));

                var line = new JsonObject();
                foreach (var (key, source) in t.Export!.Line) line[key] = Value(source, part, t);

                lines.Add(Serialize(line));
                unread.AddRange(Unread(doc, row, partId, byPath, tree));
            }
        }

        return lines;
    }

    // The order a record's parts travel in, decided by where the type takes them from.
    //
    // A heading-sourced type sorts on the heading, so a grep meets a term where a reader looking down
    // the record would find it. A table's rows are ordered by their author, and that order carries the
    // binding levels: sorting them here would leave the export and the rendered page disagreeing about
    // which obligations come first, and the page is where a person reads the policy. `clause-order`
    // holds the author to the grouping instead. `docs/design/export.md` states both rules.
    private static IEnumerable<PartRow> Ordered(IEnumerable<PartRow> parts, PartSpec spec) =>
        spec.Source == PartSpec.Table
            ? parts
            : parts.OrderBy(p => p.Text, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Id, StringComparer.Ordinal);

    // One part, and everything a line about it is built from. Gathered once so each source below reads a
    // value rather than works one out, and so no source can reach past the part it is describing.
    private sealed record Part(
        Doc Doc,
        PartRow Row,
        string Id,
        string Record,
        string? Lead,
        string? Aside,
        IReadOnlyList<string>? SeeAlso);

    // What one declared source comes to for one part. The vocabulary is `PartLineSource`, and
    // `SchemaChecks` has already refused a source outside it, so the fall-through writes null for a
    // source this build does not know rather than stopping an export over one.
    private static JsonNode? Value(string source, Part part, TypeSchema t)
    {
        if (PartLineSource.Argument(source, PartLineSource.FrontPrefix) is { } field)
            return JsonValue.Create(Absent(part.Doc.FrontScalar(field)));

        if (PartLineSource.Argument(source, PartLineSource.ColumnPrefix) is { } header)
            return JsonValue.Create(Absent(part.Row.Cells?.GetValueOrDefault(header)));

        return source switch
        {
            PartLineSource.PartId => JsonValue.Create($"{part.Record}.{part.Id}"),
            PartLineSource.PartKey => JsonValue.Create(part.Id),
            PartLineSource.PartText => JsonValue.Create(part.Row.Text),
            PartLineSource.PartLead => JsonValue.Create(part.Lead),
            PartLineSource.PartAside => JsonValue.Create(part.Aside),
            PartLineSource.PartLevel => JsonValue.Create(t.Parts!.Modal(part.Row.Text)),
            PartLineSource.PartSeeAlso => part.SeeAlso is null
                ? null
                : new JsonArray([.. part.SeeAlso.Select(v => (JsonNode?)JsonValue.Create(v))]),

            PartLineSource.PartAnchor => JsonValue.Create(t.Parts!.Anchor(part.Id)),

            PartLineSource.RecordId => JsonValue.Create(part.Record),
            PartLineSource.RecordType => JsonValue.Create(t.Key),
            PartLineSource.RecordPath => JsonValue.Create(part.Doc.Rel),
            _ => null
        };
    }

    // The parts this part points at, as full part ids, or null where it points at none.
    //
    // A cross-reference is written as a link, and a link's target is stripped out of the prose: an agent
    // reading `see [gls-search]` in the text alone is handed a bracket it cannot follow. So the ids are
    // carried beside the words rather than left inside them.
    //
    // Every id here is read, and none is inferred. These references are the `redefinitions-are-reciprocal`
    // rule showing through, and that rule is about a term and its counterpart. The link has to name the
    // counterpart, and the anchor is where it names it. A link naming a record and no term inside it
    // resolves to nothing, and `Unread` below reports it. `docs/design/export.md` sets out why the
    // guess that suggests itself is refused.
    private static IReadOnlyList<string>? SeeAlso(Doc doc, PartRow part, Dictionary<string, Doc> byPath, Tree tree)
    {
        var found = new List<string>();

        foreach (var (target, anchor) in CrossReferences(doc, part, byPath, tree))
        {
            if (anchor is null) continue;
            if (!target.Parts.Any(p => string.Equals(p.Id, anchor, StringComparison.Ordinal))) continue;

            var full = $"{Id(target)}.{anchor}";
            if (!found.Contains(full, StringComparer.Ordinal)) found.Add(full);
        }

        found.Sort(StringComparer.Ordinal);
        return found.Count > 0 ? found : null;
    }

    // The cross-references this part could not carry: a link naming another exported record without
    // naming a term inside it, as `<record>.<part> -> <record>`.
    //
    // Reported because the alternative is silence. The export omits what it cannot read, and an
    // omission in an artefact nobody reviews is invisible. So the run says which links under-specify,
    // and the author can add the anchor.
    private static IEnumerable<string> Unread(Doc doc, PartRow part, string partId,
        Dictionary<string, Doc> byPath, Tree tree) =>
        CrossReferences(doc, part, byPath, tree)
            .Where(r => r.Anchor is null)
            .Select(r => $"{Id(doc)}.{partId} -> {Id(r.Target)}");

    // Every link inside this part's body that reaches another record of the same export, with the anchor
    // it named or null where it named none. One walk, because reading a reference and reporting an
    // unreadable one are the same question answered two ways.
    private static IEnumerable<(Doc Target, string? Anchor)> CrossReferences(Doc doc, PartRow part,
        Dictionary<string, Doc> byPath, Tree tree)
    {
        foreach (var link in doc.Links)
        {
            if (link.Position < part.BodyStart || link.Position >= part.BodyEnd) continue;
            if (LinkChecks.IsExternal(link.Target)) continue;

            if (LinkChecks.Resolve(tree, doc.Rel, link.Target) is not { } rel) continue;
            if (!byPath.TryGetValue(rel, out var target) || target == doc) continue;

            var hash = link.Target.IndexOf('#');
            yield return (target, hash >= 0 ? link.Target[(hash + 1)..] : null);
        }
    }

    // A part's body split into the two pieces a type writes it in: the lead, and the labelled block
    // beneath it that the type's `parts.aside:` names. Both keep the source's markdown, unwrapped and
    // trimmed, because `full` fidelity promises the record's own words and markdown is the words it was
    // written in.
    //
    // A body with no labelled block returns a null aside, which is the common case. The label marks the
    // confusion worth heading off, and most parts have none.
    //
    // A footnote reconciling a field against what the part cites is dropped first. It is a fact about
    // coverage rather than a piece of the part, and a part that carries nothing else would otherwise
    // travel to a consumer with the footnote standing where its words belong.
    private static (string? Lead, string? Aside) Split(Doc doc, PartRow part, string label,
        IReadOnlyList<string> footnotes)
    {
        var blocks = part.Body(doc.Text).ToString()
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(Unwrap)
            .Where(b => b.Length > 0 && !footnotes.Any(f => b.StartsWith($"_**{f}:**", StringComparison.Ordinal)))
            .ToList();

        var marker = label.Length > 0 ? $"**{label}:**" : null;
        var lead = blocks.FirstOrDefault(b => marker is null || !b.StartsWith(marker, StringComparison.Ordinal));
        var aside = marker is null
            ? null
            : blocks.FirstOrDefault(b => b.StartsWith(marker, StringComparison.Ordinal))?[marker.Length..].Trim();

        return (Absent(lead), Absent(aside));
    }

    // How this export's links are built, for the manifest: the template a person's link is substituted
    // into, and the base, prefix and ref an agent fetches a record's source with. All null where the
    // corpus has no address the tool can build on. Stated once here and substituted by whoever reads a
    // line, rather than resolved onto every line of the flat file.
    private static ExportPublishing Addresses(CorpusDescriptor descriptor, Publishing? publishing)
    {
        var target = descriptor.PublishingTarget ?? Publishing.None;

        return new ExportPublishing(
            target, publishing?.Template(), publishing is null ? null : descriptor.Base,
            publishing?.PathPrefix, publishing?.Ref);
    }

    private static ExportLinks? Links(string? link) =>
        link is null ? null : new ExportLinks(link);

    private static string Id(Doc doc) => doc.FrontScalar("id") ?? doc.Rel;

    // A whole document, indented, as everything the tool writes for a person to open is. The trailing
    // newline is the corpus's rule for a text file and an export is no exception.
    private static string Serialize(ExportManifest m) =>
        JsonSerializer.Serialize(m, KacJson.Relaxed.ExportManifest) + "\n";

    private static string Serialize(ExportRecord r) =>
        JsonSerializer.Serialize(r, KacJson.Relaxed.ExportRecord) + "\n";

    // One object on one line, through the context that does not indent. A JSONL line is the unit a grep
    // hands back, so the object cannot be spread over several of them.
    private static string Serialize(JsonObject line) =>
        JsonSerializer.Serialize(line, KacJson.Line.JsonObject);
}
