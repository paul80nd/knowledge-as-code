using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace kac.core;

/// <summary>Named relative to the export root, not to the corpus.</summary>
public sealed record ExportFile(string Path, string Content);

/// <summary>
/// Named before anything is written, so a test can ask what an export would contain without a filesystem.
/// <c>Withheld</c> and <c>Unread</c> are what the output cannot say about itself, so the run reports them. A plan with
/// any <c>Refused</c> carries no files, and lists every reason at once.
/// </summary>
public sealed record ExportPlan(
    IReadOnlyList<ExportFile> Files,
    IReadOnlyList<ExportedType> Types,
    IReadOnlyList<string> Withheld,
    IReadOnlyList<string> Unread,
    IReadOnlyList<string> Refused);

/// <summary>Everything that differs between two runs over one commit, so the exporter reads no clock.</summary>
public sealed record ExportRun(string GeneratedAt, DateOnly Today, string? Commit, bool? Dirty);

/// <summary>
/// Reads each type's <c>export:</c> block and holds no list of its own. See <c>docs/reference/export.md</c>.
/// </summary>
public static class Exporter
{
    /// <summary>
    /// Versioned apart from the corpus. <c>Bundler</c> refuses an export at another version, so moving it means
    /// rebuilding every export on disk. <c>docs/reference/export.md</c> says what moves it.
    /// </summary>
    public const int FormatVersion = 4;

    public const string ManifestFile = "manifest.json";

    /// <summary>A line without it is this corpus's own, as a bare <c>pol-VURM</c> is in a citation.</summary>
    public const string ShortcodeKey = "shortcode";

    /// <summary>
    /// <c>type</c> narrows what is written, never what is read. Two runs from one tree write identical bytes but for
    /// the timestamp. <c>inherited</c> is what <c>.imports/</c> holds. <c>docs/reference/export.md</c> says what
    /// merging settles.
    /// </summary>
    public static ExportPlan Plan(LoadedCorpus corpus, Publishing? publishing, string? type, ExportRun run,
        IReadOnlyList<InheritedCorpus>? inherited = null, IReadOnlyList<string>? missing = null)
    {
        var consumed = Wanted(inherited ?? [], type);

        var refused = Refusals(corpus, inherited ?? [], missing ?? [], type);
        refused.AddRange(Disagreements(corpus, consumed, type));
        if (refused.Count > 0) return new ExportPlan([], [], [], [], refused);

        var files = new List<ExportFile>();
        var types = new List<ExportedType>();
        var withheld = new List<string>();
        var unread = new List<string>();
        var owed = new List<(string Type, string Record, string Path)>();
        var lost = new List<string>();

        foreach (var key in Keys(corpus, consumed, type))
        {
            var local = corpus.Adopted.FirstOrDefault(t => t.Key == key && t.Export is not null);
            var lines = new List<string>();
            var filed = new HashSet<string>(StringComparer.Ordinal);
            var written = new HashSet<string>(StringComparer.Ordinal);
            var owedHere = new List<(string Type, string Record, string Path)>();
            var records = 0;

            if (local is { Export: { } export })
            {
                var held = corpus.Docs.Where(d => d.Type?.Key == key)
                    .ToLookup(d => Travels(d, corpus.Descriptor, run.Today));

                withheld.AddRange(held[false].Select(Id).OrderBy(id => id, StringComparer.Ordinal));

                var own = Ordered([.. held[true]]);
                records += own.Count;

                foreach (var doc in own)
                    files.Add(new ExportFile(Filed(key, null, $"{Id(doc)}.json"),
                        Serialize(Record(doc, local, corpus.Schema, corpus.Tree, publishing))));

                if (export.Parts.Length > 0 && local.Parts is not null)
                    foreach (var line in PartLines(own, local, corpus.Tree, publishing, unread))
                    {
                        lines.Add(line);
                        owedHere.AddRange(Owed(key, line, KeyFrom(export, PartLineSource.RecordId), key));
                    }
            }

            // Consumed corpora after this one's own, each in its producer's order. Sorting across corpora would
            // interleave them. A corpus consuming a producer and that producer's own producer meets the grandparent's
            // record files and part lines twice, and writes each once.
            foreach (var from in consumed)
            {
                if (from.Types.FirstOrDefault(t => t.Type == key) is not { } theirs) continue;

                if (Lost(theirs, from.Shortcode) is { } loss)
                {
                    lost.Add(loss);
                    continue;
                }

                foreach (var record in theirs.Records)
                {
                    var path = Filed(theirs.Dir, record.Producer, record.Name);
                    if (!filed.Add(path)) continue;

                    files.Add(new ExportFile(path, record.Content));
                    records++;
                }

                foreach (var line in theirs.PartLines.Select(line => Stamped(line, from.Shortcode, theirs)))
                {
                    if (!written.Add(line)) continue;

                    lines.Add(line);
                    owedHere.AddRange(Owed(key, line, theirs.RecordKey, theirs.Dir));
                }
            }

            if (records == 0) continue;

            owed.AddRange(owedHere);

            var partsFile = lines.Count > 0 ? PartsPath(key, local, consumed) : null;
            if (partsFile is not null)
                files.Add(new ExportFile(partsFile, string.Concat(lines.Select(l => l + "\n"))));

            var frameworks = FrameworkLines(local, corpus, publishing);
            var frameworksFile = frameworks.Count > 0 && local?.Export is { Frameworks: { Length: > 0 } named }
                ? $"{key}/{named}"
                : null;
            if (frameworksFile is not null)
                files.Add(new ExportFile(frameworksFile, string.Concat(frameworks.Select(l => l + "\n"))));

            types.Add(Entry(key, local, corpus.Schema, consumed, records, lines.Count, partsFile) with
            {
                FrameworksFile = frameworksFile
            });
        }

        var orphans = Orphans(files, owed);
        orphans.InsertRange(0, lost);
        if (orphans.Count > 0) return new ExportPlan([], [], [], [], orphans);

        files.Add(new ExportFile(ManifestFile,
            Serialize(new ExportManifest(
                FormatVersion,
                corpus.Descriptor.Name,
                corpus.Descriptor.Shortcode,
                corpus.Descriptor.ContentVersion,

                // The descriptor's `upstream.template-version`. Renaming the published key needs a `FormatVersion`
                // move.
                corpus.Descriptor.TemplateVersion,
                run.Commit,
                run.Dirty,
                run.GeneratedAt,
                About(corpus.Descriptor),
                Addresses(corpus.Descriptor, publishing),
                corpus.Descriptor.ReposBase,
                Tracker.Own(corpus.Descriptor),
                Tracker.Framework(corpus.Descriptor),
                Sources(consumed),
                types))));

        return new ExportPlan([.. files.OrderBy(f => f.Path, StringComparer.Ordinal)], types, withheld,
            [.. unread.Distinct(StringComparer.Ordinal).OrderBy(u => u, StringComparer.Ordinal)], []);
    }

    /// <summary>
    /// The corpora this one consumes and the ones they consumed in turn, so a chain of any depth resolves.
    /// </summary>
    private static List<ExportSource> Sources(List<InheritedCorpus> consumed)
    {
        var found = new Dictionary<string, ExportSource>(StringComparer.Ordinal);

        foreach (var from in consumed)
        {
            found.TryAdd(from.Shortcode,
                new ExportSource(from.Shortcode, from.Corpus, from.ContentVersion, from.Publishing,
                    from.ReposBase, from.Tracker));

            foreach (var theirs in from.Sources) found.TryAdd(theirs.Shortcode, theirs);
        }

        return [.. found.Values.OrderBy(s => s.Shortcode, StringComparer.Ordinal)];
    }

    /// <summary>Ordered by shortcode, so two runs over one tree write the same bytes.</summary>
    private static List<InheritedCorpus> Wanted(IReadOnlyList<InheritedCorpus> inherited, string? type) =>
    [
        .. inherited
            .Select(c => type is null
                ? c
                : c with { Types = [.. c.Types.Where(t => t.Type == type)] })
            .Where(c => c.Types.Count > 0 || type is null)
            .OrderBy(c => c.Shortcode, StringComparer.Ordinal)
    ];

    /// <summary>
    /// Includes types this corpus holds only because a consumed corpus exported them, so a citation of a clause in a
    /// policy this corpus never adopted still resolves.
    /// </summary>
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

    /// <summary>
    /// What stops an export before a file is planned. A missing restore is a hole in the export, not a smaller one: see
    /// <c>docs/use/imports.md</c>. An unknown envelope is refused because its key names cannot be trusted.
    /// </summary>
    private static List<string> Refusals(LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited,
        IReadOnlyList<string> missing, string? type)
    {
        var found = new List<string>();

        if (type is not null && corpus.Adopted.All(t => t.Key != type))
            found.Add($"this corpus has not adopted a type called '{type}'. "
                      + $"It holds {string.Join(", ", corpus.Adopted.Select(t => t.Key))}.");

        var unknown = corpus.Descriptor.ExportExclude
            .Where(e => !CorpusDescriptor.Excludable.Contains(e, StringComparer.Ordinal)).ToList();
        if (unknown.Count > 0)
            found.Add($".corpus.yaml excludes {string.Join(", ", unknown)}, which an export cannot act on. "
                      + $"It excludes {string.Join(" or ", CorpusDescriptor.Excludable)}.");

        if (missing.Count > 0)
            found.Add($"nothing is restored for {string.Join(", ", missing)}, which this corpus consumes "
                      + "and an export carries. Run kac restore.");

        var stale = inherited.Where(c => c.FormatVersion != FormatVersion).ToList();
        if (stale.Count > 0)
            found.Add(
                $"{string.Join(", ", stale.Select(c => $"{c.Shortcode} is at export format {c.FormatVersion}"))}, "
                + $"and this build reads {FormatVersion}. Re-export and re-pack it, then run kac restore.");

        return found;
    }

    /// <summary>
    /// Two corpora exporting one type at two shapes, or its sections at two fidelities. Either would merge into one
    /// file that answers two ways, invisibly, so the run stops here.
    /// </summary>
    private static List<string> Disagreements(LoadedCorpus corpus, List<InheritedCorpus> consumed, string? type)
    {
        var found = new List<string>();
        var seen = new Dictionary<string, ExportSource>(StringComparer.Ordinal);

        // A third corpus consumed twice, at two versions, would resolve links through whichever account won.
        foreach (var source in consumed.SelectMany(c =>
                     c.Sources.Prepend(new ExportSource(
                         c.Shortcode, c.Corpus, c.ContentVersion, c.Publishing, c.ReposBase,
                         c.Tracker))))
        {
            if (seen.TryAdd(source.Shortcode, source)) continue;
            if (Addressed(seen[source.Shortcode]) == Addressed(source)) continue;

            found.Add($"'{source.Shortcode}' arrives twice and differently: at "
                      + $"{seen[source.Shortcode].ContentVersion} and at {source.ContentVersion}. A line "
                      + "naming it could resolve either way.");
        }

        // The tracker and repository base are left out: no line resolves through either, and an older export
        // states neither.
        static ExportSource Addressed(ExportSource source) =>
            source with { Tracker = Tracker.None, ReposBase = null };

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
                              + $"{shape} by {where}. A merged file cannot hold both. Bring both to one shape, "
                              + "or drop the type from this corpus.");
                else if (!Same(first.Sections, sections))
                    found.Add($"'{key}' carries its sections at one fidelity in {first.Where} and another in "
                              + $"{where}. A merged file cannot promise both. Bring both to one fidelity, or "
                              + "drop the type from this corpus.");
            }

            found.AddRange(InForceDisagreement(corpus, consumed, key));
        }

        return found;
    }

    /// <summary>
    /// The manifest entry states one in-force status for every record under the type. A corpus stating none, such as
    /// a producer exported by an older <c>kac</c>, agrees with any.
    /// </summary>
    private static IEnumerable<string> InForceDisagreement(
        LoadedCorpus corpus, List<InheritedCorpus> consumed, string key)
    {
        var stated = new List<(string Where, string InForce)>();

        if (corpus.Adopted.FirstOrDefault(t => t.Key == key && t.Export is not null) is { } local
            && corpus.Schema.EffectiveField(local, "status")?.InForce is { } own)
            stated.Add((corpus.Descriptor.Shortcode ?? "this corpus", own));

        foreach (var from in consumed)
            if (from.Types.FirstOrDefault(t => t.Type == key)?.InForce is { } theirs)
                stated.Add((from.Shortcode, theirs));

        if (stated.Count == 0) yield break;

        var first = stated[0];
        foreach (var (where, inForce) in stated.Skip(1).Where(s => !string.Equals(s.InForce, first.InForce, StringComparison.Ordinal)))
            yield return $"'{key}' is in force at '{first.InForce}' in {first.Where} and at '{inForce}' in "
                         + $"{where}. A merged file cannot state both. Bring both to one in-force status, or drop "
                         + "the type from this corpus.";
    }

    /// <summary>A part line naming a record whose file the plan does not write.</summary>
    private static List<string> Orphans(
        List<ExportFile> files, List<(string Type, string Record, string Path)> owed)
    {
        var paths = files.Select(f => f.Path).ToHashSet(StringComparer.Ordinal);

        return
        [
            .. owed.Where(o => !paths.Contains(o.Path)).Select(o =>
                $"'{o.Type}' has a part line for {o.Record}, and the export writes no {o.Path}. "
                + "Re-export and re-pack the corpus that sent the line, then run kac restore.")
        ];
    }

    /// <summary>
    /// The refusal for a consumed type that sent fewer record files than its manifest counts, or none while its part
    /// lines say it has records. Null where every counted file arrived, and for an empty type, which the export leaves
    /// out. <c>Records</c> is what arrived from this producer alone, so this corpus's own records never fill the count.
    /// </summary>
    private static string? Lost(InheritedType theirs, string shortcode)
    {
        var fault = theirs switch
        {
            { Records.Count: > 0 and var n } when n < theirs.StatedRecords =>
                $"with {n} of the {theirs.StatedRecords} record files its manifest counts",
            { Records.Count: > 0 } => null,
            { PartLines.Count: > 0 and var n } => $"with {Count(n, "part line")} and no record files",
            { StatedRecords: > 0 and var n } => $"with no record files, and its manifest counts {Count(n, "record")}",
            { StatedParts: > 0 and var n } => $"with no record files, and its manifest counts {Count(n, "part line")}",
            _ => null
        };

        return fault is null
            ? null
            : $"'{theirs.Type}' arrives from {shortcode} {fault}. "
              + $"Re-export and re-pack {shortcode}, then run kac restore.";

        static string Count(int n, string noun) => $"{n} {noun}{(n == 1 ? "" : "s")}";
    }

    /// <summary>
    /// The record file a part line names, in the folder <c>dir</c> its records are filed under. None where the line
    /// does not parse or names no record.
    /// </summary>
    private static IEnumerable<(string Type, string Record, string Path)> Owed(
        string type, string line, string? recordKey, string dir)
    {
        if (recordKey is null || JsonRead.Parse(line) is not { } read
                              || JsonRead.Str(read[recordKey]) is not { } record)
            yield break;

        var shortcode = JsonRead.Str(read[ShortcodeKey]);
        var id = shortcode is not null && record.StartsWith($"{shortcode}:", StringComparison.Ordinal)
            ? record[(shortcode.Length + 1)..]
            : record;

        yield return (type, record, Filed(dir, shortcode, $"{id}.json"));
    }

    /// <summary>
    /// Where a record file lands: in <c>dir</c> for this corpus's own, and one folder deeper, named for the corpus that
    /// wrote it, for an inherited one. See the chain section of docs/reference/export.md.
    /// </summary>
    private static string Filed(string dir, string? producer, string name) =>
        producer is null ? $"{dir}/{name}" : $"{dir}/{producer}/{name}";

    private static bool Same(
        IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b) =>
        a.Count == b.Count && a.All(e => b.TryGetValue(e.Key, out var v) && v == e.Value);

    /// <summary>
    /// None where the type declares no file for them. This corpus's own clauses alone, because an inherited clause left
    /// its <c>Alignment</c> cell behind. See <c>docs/reference/export.md</c>.
    /// </summary>
    private static List<string> FrameworkLines(TypeSchema? local, LoadedCorpus corpus, Publishing? publishing)
    {
        if (local?.Export is not { Frameworks.Length: > 0 }) return [];

        return
        [
            .. Reports.FrameworkRows(corpus, []).Select(row => Serialize(new JsonObject
            {
                ["framework"] = row.Framework,
                ["reference"] = row.Reference,
                ["standing"] = row.Standing,
                ["clauses"] = new JsonArray([.. row.Clauses.Select(c => (JsonNode?)JsonValue.Create(c))]),
                ["records"] = new JsonArray([.. row.Records.Select(r => (JsonNode?)JsonValue.Create(r))]),
                ["path"] = row.Path,
                ["anchor"] = row.Anchor,
                ["links"] = LineLinks(publishing, row.Path, row.Anchor)
            }))
        ];
    }

    /// <summary>This corpus's name for a part where it adopted the type, and the producer's where it did not.</summary>
    private static string? PartsPath(string key, TypeSchema? local, List<InheritedCorpus> consumed)
    {
        if (local?.Parts is { } spec) return $"{key}/{spec.Noun}s.jsonl";

        return consumed.SelectMany(c => c.Types).FirstOrDefault(t => t.Type == key)?.PartsFile;
    }

    /// <summary>
    /// Counts what this corpus wrote and what it inherited together. A type this corpus did not adopt states the
    /// in-force status its producer published.
    /// </summary>
    private static ExportedType Entry(string key, TypeSchema? local, Schema schema, List<InheritedCorpus> consumed,
        int records, int parts, string? partsFile)
    {
        if (local?.Export is { } export)
            return new ExportedType(
                key, export.Version, records, parts, key, partsFile,
                partsFile is null ? null : KeyFrom(export, PartLineSource.RecordId),
                partsFile is null ? null : KeyFrom(export, PartLineSource.PartKey),
                partsFile is null ? null : KeyFrom(export, PartLineSource.PartId),
                partsFile is null ? null : KeyFrom(export, PartLineSource.PartSeeAlso),
                export.Sections.ToDictionary(e => e.Section, e => e.Fidelity, StringComparer.Ordinal),
                InForce: schema.EffectiveField(local, "status")?.InForce);

        var theirs = consumed.SelectMany(c => c.Types).First(t => t.Type == key);

        return new ExportedType(
            key, theirs.ShapeVersion, records, parts, theirs.Dir, partsFile,
            theirs.RecordKey, theirs.PartKey, theirs.IdKey, theirs.SeeAlsoKey, theirs.Sections,
            InForce: theirs.InForce);
    }

    /// <summary>
    /// Writes the producer's shortcode onto every id key and onto the line. A value already carrying a prefix keeps it,
    /// so a grandparent arrives labelled once.
    /// </summary>
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

        // A line arriving with a shortcode names a corpus further up the chain. Overwriting it would send its
        // links to the wrong repository.
        read[ShortcodeKey] ??= JsonValue.Create(shortcode);
        return Serialize(read);
    }

    /// <summary>How a consumer names an id it inherited: the producer's shortcode, then the id.</summary>
    /// <remarks>An id already carrying one was written further up the chain and keeps the name it arrived
    /// under. Public because a report reads inherited parts and has to name them the way an export does.
    /// See Reports.cs.</remarks>
    public static string Scoped(string id, string shortcode) =>
        id.Contains(':', StringComparison.Ordinal) ? id : $"{shortcode}:{id}";

    /// <summary>
    /// Deletes the export, then writes, so a deleted record cannot stay readable. <c>docs/use/cli/export.md</c> says
    /// why. Deletes <c>Dist.Export</c> and never <c>.dist/</c>, because the bundle sits beside it.
    /// </summary>
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

    /// <summary>
    /// Every record travels unless the corpus excludes drafts or overdue records. <c>docs/use/cli/export.md</c> says
    /// why carrying both is the default.
    /// </summary>
    private static bool Travels(Doc doc, CorpusDescriptor descriptor, DateOnly today)
    {
        if (descriptor.ExportExclude.Count == 0) return true;

        if (descriptor.ExportExclude.Contains(CorpusDescriptor.ExcludeDraft, StringComparer.Ordinal)
            && string.Equals(doc.FrontScalar("status"), "draft", StringComparison.OrdinalIgnoreCase))
            return false;

        // A missing or unparseable `review-by` is never overdue. That is the validator's to report.
        return !descriptor.ExportExclude.Contains(CorpusDescriptor.ExcludeOverdue, StringComparer.Ordinal)
               || !DateOnly.TryParse(doc.FrontScalar("review-by"), out var due)
               || due >= today;
    }

    /// <summary>
    /// Roots by id, each root's chain depth-first beneath it. A record narrowing something outside the set is a root.
    /// See <c>docs/reference/export.md</c>. Two records sharing an id keep the first, and <c>id-unique</c> reports it.
    /// </summary>
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

        // A cycle leaves its members unreachable from any root, so they follow in id order. The validator reports
        // the cycle.
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

    private static ExportRecord Record(Doc doc, TypeSchema t, Schema schema, Tree tree, Publishing? publishing)
    {
        var export = t.DeclaredExport;
        var link = publishing?.Link(doc.Rel);

        var fields = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);
        foreach (var name in export.Fields)
            fields[name] = name == Generator.Title
                ? JsonValue.Create(Absent(doc.H1))
                : Value(doc.FrontNode(name), schema.EffectiveField(t, name));

        var edits = Edits(doc, tree, publishing);
        var sections = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var (name, fidelity) in export.Sections)
            if (doc.Sections.FirstOrDefault(s =>
                    string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase)) is { } section)
                sections[name] = Carry(Rewritten(doc.Text, section.BodyStart, section.BodyEnd, edits), fidelity);

        return new ExportRecord(t.Key, doc.Rel, Trust(doc, export), fields, sections, Links(link));
    }

    /// <summary>
    /// Fixed here, not declared per type: a type writing <c>verified</c> has taken the Open Knowledge Format's field.
    /// </summary>
    private const string VerifiedField = "verified";

    private const string ActorKey = "by";
    private const string HumanPrefix = "human:";

    /// <summary>
    /// <c>reports</c> is the only type declaring both today. A type declaring only <c>verified</c> counts every entry.
    /// </summary>
    private const string GeneratedField = "generated";
    private const string MomentKey = "at";

    /// <summary>
    /// The Open Knowledge Format's three tiers: no entries is <c>unverified</c>, agents alone <c>machine-confirmed</c>,
    /// and one <c>human:</c> actor <c>human-reviewed</c>. Null where the export declares no <c>verified</c>. A
    /// verification taken before <c>generated.at</c> is left out. See docs/use/cli/report.md. One mapping is the
    /// one-entry case.
    /// </summary>
    private static string? Trust(Doc doc, ExportSpec export)
    {
        if (!export.Fields.Contains(VerifiedField, StringComparer.Ordinal)) return null;

        var generated = Moment(doc.FrontNode(GeneratedField) as YamlMappingNode);

        IEnumerable<YamlNode> entries = doc.FrontNode(VerifiedField) switch
        {
            YamlSequenceNode seq => seq.Children,
            YamlMappingNode map => [map],
            _ => []
        };

        var verified = false;
        foreach (var item in entries)
        {
            if (item is not YamlMappingNode entry) continue;
            if (Yaml.Get(entry, ActorKey) is not YamlScalarNode { Value: { Length: > 0 } actor }) continue;
            if (generated is { } at && Moment(entry) < at) continue;

            if (actor.StartsWith(HumanPrefix, StringComparison.Ordinal)) return "human-reviewed";
            verified = true;
        }

        return verified ? "machine-confirmed" : "unverified";
    }

    /// <summary>
    /// Null where the event states no instant, or one that will not parse. Such an entry still counts toward a tier,
    /// because <c>timestamp-format</c> already reports it.
    /// </summary>
    private static DateTimeOffset? Moment(YamlMappingNode? entry) =>
        entry is not null
        && Yaml.Get(entry, MomentKey) is YamlScalarNode { Value: { Length: > 0 } moment }
        && DateTimeOffset.TryParse(moment, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var at)
            ? at
            : null;

    /// <summary>
    /// The source from <c>from</c> to <c>to</c> with <c>edits</c> applied, so its link reference definitions are gone
    /// and each reference link is inline.
    /// </summary>
    private static string Rewritten(string text, int from, int to, List<Edit> edits)
    {
        var kept = new StringBuilder();
        var at = from;

        foreach (var (start, end, replacement) in edits.Where(e => e.Start >= from && e.Start < to))
        {
            // An edit starting inside one already applied is dropped, so no source text is written twice.
            if (start < at) continue;
            kept.Append(text[at..start]).Append(replacement);
            at = Math.Min(end, to);
        }

        return kept.Append(text[at..to]).ToString();
    }

    /// <summary>
    /// Every edit exported prose takes, in source order: a section, a part's lead and asides, and its bullets. A link
    /// reference definition sits at the foot, so inside the last section, and is cut. The span after each reference
    /// link's text, <c>][label]</c>, <c>][]</c> or <c>]</c>, becomes <c>](target "title")</c>, copied from its
    /// definition's source. A link's target that <c>Addressed</c> resolves is written as that address, in an inline
    /// link too, and an image's never is. All are read from the AST, so a look-alike in a code span or a fenced block is
    /// left as written.
    /// </summary>
    private static List<Edit> Edits(Doc doc, Tree tree, Publishing? publishing)
    {
        var edits = doc.DefinitionSpans.Select(d => new Edit(d.Start, d.End, "")).ToList();

        foreach (var link in doc.Ast.Descendants<LinkInline>())
        {
            if (link.Reference is not { } definition)
            {
                if (!link.IsAutoLink && !link.IsImage && Addressed(doc, link.Url, tree, publishing) is { } address)
                    edits.Add(new Edit(link.UrlSpan.Start, link.UrlSpan.End + 1, address));
                continue;
            }

            // Empty link text closes straight after the opening `[`, or `![` for an image.
            var close = link.LastChild is { } last ? last.Span.End + 1 : link.Span.Start + (link.IsImage ? 2 : 1);
            var target = (link.IsImage ? null : Addressed(doc, definition.Url, tree, publishing))
                         ?? Source(doc.Text, definition.UrlSpan);
            var title = string.IsNullOrEmpty(definition.Title) ? "" : " " + Source(doc.Text, definition.TitleSpan);

            edits.Add(new Edit(close, link.Span.End + 1, $"]({target}{title})"));
        }

        return [.. edits.OrderBy(e => e.Start)];

        static string Source(string text, SourceSpan span) => text.Substring(span.Start, span.Length);
    }

    /// <summary>
    /// The address <c>publishing</c> gives the Markdown file a relative <c>target</c> in <c>doc</c> resolves to, with
    /// the target's anchor. Null where the corpus has no address, and for an absolute or anchor-only target or one
    /// naming no Markdown file the corpus has. See <c>docs/reference/export.md</c>.
    /// </summary>
    private static string? Addressed(Doc doc, string? target, Tree tree, Publishing? publishing)
    {
        if (publishing is null || target is not { Length: > 0 } || LinkChecks.IsExternal(target)) return null;
        if (LinkChecks.Resolve(tree, doc.Rel, target) is not { } rel
            || !rel.EndsWith(".md", StringComparison.Ordinal))
            return null;

        var hash = target.IndexOf('#');
        var address = publishing.Link(rel, hash >= 0 ? target[(hash + 1)..] : null);

        // A space would end the destination early, and angle brackets keep it whole.
        return address.Contains(' ', StringComparison.Ordinal) ? $"<{address}>" : address;
    }

    /// <summary>The source from <c>Start</c> up to <c>End</c>, exclusive, written as <c>Replacement</c>.</summary>
    private sealed record Edit(int Start, int End, string Replacement);

    /// <summary>
    /// <c>docs/reference/export.md</c> says what each fidelity promises. Unwrapped first, so a summary is the author's
    /// paragraph.
    /// </summary>
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

    /// <summary>
    /// In the shape its type declared: a list field is always an array and an object field an object, so a consumer
    /// reads one key one way. See <c>docs/reference/export.md</c>.
    /// </summary>
    private static JsonNode? Value(YamlNode? node, FieldSpec? spec)
    {
        if (spec?.Type == "object") return Mapping(node, spec);
        if (spec?.Type != "list") return JsonValue.Create(Absent((node as YamlScalarNode)?.Value));

        // A list written as one scalar is the one-entry case, as `Doc.FrontList` reads it. An empty list is null,
        // as every absence is.
        IEnumerable<YamlNode> items = node switch
        {
            YamlSequenceNode seq => seq.Children,
            null => [],
            _ => [node]
        };

        var array = new JsonArray();
        foreach (var item in items)
            if (Entry(item, spec) is { } entry)
                array.Add(entry);

        return array.Count > 0 ? array : null;
    }

    /// <summary>An entry written in the other shape is dropped. <c>entry-shape</c> reports it.</summary>
    private static JsonNode? Entry(YamlNode node, FieldSpec spec) =>
        spec.Of != "object"
            ? JsonValue.Create(Absent((node as YamlScalarNode)?.Value))
            : Mapping(node, spec);

    /// <summary>
    /// Each key is read back through <c>Value</c>, so a nested list travels as an array. An object nobody wrote is
    /// null, as every absence is.
    /// </summary>
    private static JsonNode? Mapping(YamlNode? node, FieldSpec spec)
    {
        if (node is not YamlMappingNode map || spec.Entry is null) return null;

        var obj = new JsonObject();
        foreach (var key in spec.Entry) obj[key.Name] = Value(Yaml.Get(map, key.Name), key);

        return obj;
    }

    /// <summary>Blank and missing both arrive as <c>null</c>. See <c>docs/reference/export.md</c>.</summary>
    private static string? Absent(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>
    /// Takes the source's wrap column out. Blank lines stay, and a block a joiner would mangle is left as written. See
    /// <c>docs/reference/export.md</c>.
    /// </summary>
    private static string Unwrap(string text)
    {
        var blocks = Blocks(FromFirstText(text.Replace("\r\n", "\n")));

        return string.Join("\n\n", blocks.Select(p =>
        {
            var block = FromFirstText(p);
            var lines = block.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            if (lines.Any(Structural)) return block;

            var indent = block[..(block.Length - block.TrimStart().Length)];
            return indent + string.Join(" ", lines);
        }));

        // A blank line inside a fence splits nothing, so the fence is one block to its closing line or the text's end.
        static List<string> Blocks(string text)
        {
            var blocks = new List<string>();
            (char Mark, int Length)? open = null;
            foreach (var piece in text.Split("\n\n"))
            {
                if (open is null) blocks.Add(piece);
                else blocks[^1] += "\n\n" + piece;
                open = OpenAfter(piece, open);
            }

            return blocks;
        }

        // CommonMark 0.31.2, 4.5: only the opening mark, repeated at least as often, closes a fence.
        static (char Mark, int Length)? OpenAfter(string block, (char Mark, int Length)? open)
        {
            foreach (var line in block.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0))
                if (open is not { } fence)
                {
                    if (Fence(line)) open = (line[0], line.Length - line.TrimStart(line[0]).Length);
                }
                else if (line.Length >= fence.Length && line.All(c => c == fence.Mark))
                    open = null;

            return open;
        }

        // Keeps the first line's indent, which places a block inside a list item.
        static string FromFirstText(string block)
        {
            var lines = block.Split('\n');
            var first = Array.FindIndex(lines, l => !string.IsNullOrWhiteSpace(l));
            return first < 0 ? "" : string.Join('\n', lines[first..]).TrimEnd();
        }

        // A bullet and a heading need the marker and then a space, which tells `- item` from an em-dash clause.
        static bool Structural(string line) =>
            Bullet(line) || Heading(line) || line[0] is '>' or '|' || Fence(line) || Ordered(line);

        // CommonMark 0.31.2, 4.5: three or more backticks or tildes. A backtick in a backtick fence's info string
        // makes the line a code span.
        static bool Fence(string line) =>
            line.StartsWith("~~~", StringComparison.Ordinal)
            || (line.StartsWith("```", StringComparison.Ordinal) && !line.TrimStart('`').Contains('`'));

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

    /// <summary>
    /// Nothing is filled in for an empty key: a licence or author nobody named is a claim about a person.
    /// </summary>
    private static ExportAbout About(CorpusDescriptor descriptor) =>
        new(descriptor.DisplayName, descriptor.Description,
            descriptor.AuthorName is null && descriptor.AuthorUrl is null
                ? null
                : new ExportAuthor(descriptor.AuthorName, descriptor.AuthorUrl),
            descriptor.License);

    /// <summary>Null where the type's line has no key for this source.</summary>
    private static string? KeyFrom(ExportSpec export, string source) =>
        export.Line.FirstOrDefault(l => string.Equals(l.Source, source, StringComparison.Ordinal)).Key;

    /// <summary>
    /// One line per part of this corpus's own records. <c>docs/reference/export.md</c> says why JSONL. The type
    /// declares what a line holds, so a second type exporting parts needs no C#.
    /// </summary>
    private static List<string> PartLines(
        List<Doc> records, TypeSchema t, Tree tree, Publishing? publishing, List<string> unread)
    {
        var spec = t.DeclaredParts;
        var byPath = records.ToDictionary(d => d.Rel, StringComparer.Ordinal);
        var lines = new List<string>();
        var footnotes = t.DeclaredFields.Select(f => f.MirrorsCitations).OfType<string>().ToList();

        foreach (var doc in records)
        {
            var id = Id(doc);
            var edits = Edits(doc, tree, publishing);
            foreach (var row in Ordered(doc.Parts, spec))
            {
                if (row.Id is not { Length: > 0 } partId) continue;

                var (lead, asides) = Split(doc, row, edits, spec.Asides, footnotes);
                var part = new Part(doc, row, partId, id, lead, Bullets(doc, row, edits, spec), asides,
                    SeeAlso(doc, row, byPath, tree), Citations(doc, row, footnotes));

                var line = new JsonObject();
                foreach (var (key, source) in t.DeclaredExport.Line) line[key] = Value(source, part, t, publishing);

                lines.Add(Serialize(line));
                unread.AddRange(Unread(doc, row, partId, byPath, tree));
            }
        }

        return lines;
    }

    /// <summary>
    /// Heading parts sort on the heading. Table rows keep their author's order, which carries the binding levels. See
    /// <c>docs/reference/export.md</c>.
    /// </summary>
    private static IEnumerable<PartRow> Ordered(IEnumerable<PartRow> parts, PartSpec spec) =>
        spec.Source == PartSpec.Table
            ? parts
            : parts.OrderBy(p => p.Text, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Id, StringComparer.Ordinal);

    private sealed record Part(
        Doc Doc,
        PartRow Row,
        string Id,
        string Record,
        string? Lead,
        IReadOnlyList<Bullet> Bullets,
        IReadOnlyDictionary<string, string> Asides,
        IReadOnlyList<string>? SeeAlso,
        IReadOnlyDictionary<string, IReadOnlyList<string>> Citations);

    /// <summary>
    /// A source outside <c>PartLineSource</c> writes null. <c>SchemaChecks</c> has already refused one.
    /// </summary>
    private static JsonNode? Value(string source, Part part, TypeSchema t, Publishing? publishing)
    {
        if (PartLineSource.Argument(source, PartLineSource.FrontPrefix) is { } field)
            return JsonValue.Create(Absent(part.Doc.FrontScalar(field)));

        if (PartLineSource.Argument(source, PartLineSource.ColumnPrefix) is { } header)
            return JsonValue.Create(Absent(part.Row.Cells?.GetValueOrDefault(header)));

        if (PartLineSource.Argument(source, PartLineSource.CitationPrefix) is { } label)
            return part.Citations.GetValueOrDefault(label) is { Count: > 0 } cited
                ? new JsonArray([.. cited.Select(v => (JsonNode?)JsonValue.Create(v))])
                : null;

        if (PartLineSource.Argument(source, PartLineSource.AsidePrefix) is { } aside)
            return JsonValue.Create(part.Asides.GetValueOrDefault(aside));

        return source switch
        {
            PartLineSource.PartId => JsonValue.Create($"{part.Record}.{part.Id}"),
            PartLineSource.PartKey => JsonValue.Create(part.Id),
            PartLineSource.PartText => JsonValue.Create(part.Row.Text),
            PartLineSource.PartLead => JsonValue.Create(part.Lead),
            PartLineSource.PartLevel => JsonValue.Create(t.DeclaredParts.Modal(part.Row.Text)),
            PartLineSource.PartBullets => part.Bullets.Count == 0
                ? null
                : new JsonArray([.. part.Bullets.Select(b => (JsonNode?)new JsonObject
                {
                    ["level"] = JsonValue.Create(b.Level),
                    ["text"] = JsonValue.Create(b.Text)
                })]),
            PartLineSource.PartSeeAlso => part.SeeAlso is null
                ? null
                : new JsonArray([.. part.SeeAlso.Select(v => (JsonNode?)JsonValue.Create(v))]),

            PartLineSource.PartAnchor => JsonValue.Create(t.DeclaredParts.Anchor(part.Row)),
            PartLineSource.PartLinks => LineLinks(publishing, part.Doc.Rel, t.DeclaredParts.Anchor(part.Row)),

            PartLineSource.RecordId => JsonValue.Create(part.Record),
            PartLineSource.RecordType => JsonValue.Create(t.Key),
            PartLineSource.RecordPath => JsonValue.Create(part.Doc.Rel),
            _ => null
        };
    }

    /// <summary>
    /// Keyed by label. Matched by line, because a footnote is found per document and a part is a span. Read from the
    /// footnote, not the links, because <c>[pol-INTC].HOLDS</c> links only the policy. A coverage report reads it too.
    /// </summary>
    internal static IReadOnlyDictionary<string, IReadOnlyList<string>> Citations(
        Doc doc, PartRow part, IReadOnlyList<string> labels)
    {
        var found = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        if (labels.Count == 0) return found;

        var from = LineAt(doc.Text, part.BodyStart);
        var to = LineAt(doc.Text, part.BodyEnd);

        foreach (var label in labels)
        {
            var cited = doc.CitationFootnotes.GetValueOrDefault(label, [])
                .Where(c => c.Line >= from && c.Line <= to)
                .SelectMany(c => c.Citations).ToList();

            if (cited.Count > 0) found[label] = cited;
        }

        return found;
    }

    /// <summary>One-based, as a footnote records its line.</summary>
    private static int LineAt(string text, int offset) =>
        text.AsSpan()[..Math.Min(offset, text.Length)].Count('\n') + 1;

    /// <summary>
    /// Full part ids, or null where the part points at none. Read from link anchors and never inferred: a link naming a
    /// record and no part is reported by <c>Unread</c>. See <c>docs/reference/export.md</c>.
    /// </summary>
    private static IReadOnlyList<string>? SeeAlso(Doc doc, PartRow part, Dictionary<string, Doc> byPath, Tree tree)
    {
        var found = new List<string>();

        foreach (var (target, anchor) in CrossReferences(doc, part, byPath, tree))
        {
            if (anchor is null) continue;
            if (target.Parts.FirstOrDefault(p => string.Equals(p.Anchor, anchor, StringComparison.Ordinal))
                is not { Id: { } partId }) continue;

            var full = $"{Id(target)}.{partId}";
            if (!found.Contains(full, StringComparer.Ordinal)) found.Add(full);
        }

        found.Sort(StringComparer.Ordinal);
        return found.Count > 0 ? found : null;
    }

    /// <summary>
    /// A link naming another exported record without naming a part inside it, as <c>&lt;record&gt;.&lt;part&gt; -&gt;
    /// &lt;record&gt;</c>.
    /// </summary>
    private static IEnumerable<string> Unread(Doc doc, PartRow part, string partId,
        Dictionary<string, Doc> byPath, Tree tree) =>
        CrossReferences(doc, part, byPath, tree)
            .Where(r => r.Anchor is null)
            .Select(r => $"{Id(doc)}.{partId} -> {Id(r.Target)}");

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

    /// <summary>
    /// The lead, and each labelled aside the type's <c>parts.asides:</c> names, in the source's markdown with
    /// <c>edits</c> applied. A label the body lacks is absent, so its key writes null. A reconciling footnote is
    /// dropped first.
    /// </summary>
    private static (string? Lead, IReadOnlyDictionary<string, string> Asides) Split(
        Doc doc, PartRow part, List<Edit> edits, IReadOnlyList<string> labels, IReadOnlyList<string> footnotes)
    {
        var blocks = Rewritten(doc.Text, part.BodyStart, part.BodyStart + part.Body(doc.Text).Length, edits)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(Unwrap)
            .Where(b => b.Length > 0 && !footnotes.Any(f => b.StartsWith($"_**{f}:**", StringComparison.Ordinal)))
            .ToList();

        var markers = labels.ToDictionary(l => l, l => $"**{l}:**", StringComparer.Ordinal);

        var lead = blocks.FirstOrDefault(b =>
            !markers.Values.Any(m => b.StartsWith(m, StringComparison.Ordinal)));

        var asides = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (label, marker) in markers)
        {
            if (blocks.FirstOrDefault(b => b.StartsWith(marker, StringComparison.Ordinal)) is not { } found)
                continue;
            if (Absent(found[marker.Length..].Trim()) is { } text) asides[label] = text;
        }

        return (Absent(lead), asides);
    }

    /// <summary>
    /// The modal is a bold run naming a declared level whole, so <c>MUST NOT</c> is never read as <c>MUST</c>.
    /// </summary>
    private static IReadOnlyList<Bullet> Bullets(Doc doc, PartRow part, List<Edit> edits, PartSpec spec) =>
    [
        .. Md.BulletSources(doc.Ast, (from, to) => Rewritten(doc.Text, from, to, edits), part.BodyStart, part.BodyEnd)
            .Select(b => new Bullet(
                b.Bold.FirstOrDefault(r => spec.Levels.Contains(r, StringComparer.Ordinal)), b.Markdown))
    ];

    private sealed record Bullet(string? Level, string Text);

    /// <summary>All null where the corpus has no address the tool can build on.</summary>
    private static ExportPublishing Addresses(CorpusDescriptor descriptor, Publishing? publishing)
    {
        var target = descriptor.PublishingTarget ?? Publishing.None;

        return new ExportPublishing(
            target, publishing?.Template(), publishing is null ? null : descriptor.Base,
            publishing?.PathPrefix, publishing?.Ref);
    }

    private static ExportLinks? Links(string? link) =>
        link is null ? null : new ExportLinks(link);

    /// <summary>
    /// The <c>links</c> a record file has, built for one part. Null where <c>publishing</c> or <c>path</c> is null.
    /// </summary>
    private static JsonNode? LineLinks(Publishing? publishing, string? path, string? anchor) =>
        path is null
            ? null
            : JsonSerializer.SerializeToNode(Links(publishing?.Link(path, anchor)), KacJson.Relaxed.ExportLinks);

    private static string Id(Doc doc) => doc.FrontScalar("id") ?? doc.Rel;

    private static string Serialize(ExportManifest m) =>
        JsonSerializer.Serialize(m, KacJson.Relaxed.ExportManifest) + "\n";

    /// <summary>
    /// Read through the context that wrote it, so the two ends cannot drift. Null where the text is missing or does not
    /// parse.
    /// </summary>
    public static ExportManifest? ReadManifest(string? json)
    {
        if (json is null) return null;

        try
        {
            return JsonSerializer.Deserialize(json, KacJson.Relaxed.ExportManifest) is { } manifest
                ? Sound(manifest)
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// A missing key arrives as null whatever the record declares. Settled here so every reader below takes the record
    /// at its word: an entry naming no type is dropped, a type naming no directory is read under its own name, and a
    /// block naming no publishing target publishes nowhere.
    /// </summary>
    private static ExportManifest Sound(ExportManifest m)
    {
        var publishing = Sound(m.Publishing);

        return m with
        {
            About = Read(m.About) ?? new ExportAbout(null, null, null, null),
            Publishing = publishing,
            Tracker = Sound(m.Tracker, publishing),
            Framework = Sound(m.Framework, null),
            Sources =
            [
                .. (Read(m.Sources) ?? []).Where(s => Read(s)?.Shortcode is { Length: > 0 }).Select(Sound)
            ],
            Types = [.. (Read(m.Types) ?? []).Where(t => Read(t)?.Type is not null).Select(Sound)]
        };
    }

    private static ExportSource Sound(ExportSource s)
    {
        var publishing = Sound(s.Publishing);

        return s with { Publishing = publishing, Tracker = Sound(s.Tracker, publishing) };
    }

    private static ExportPublishing Sound(ExportPublishing? p) =>
        Read(p) is not { } stated ? new ExportPublishing(Publishing.None, null, null, null, null)
        : Read(stated.Target) is null ? stated with { Target = Publishing.None }
        : stated;

    /// <summary>
    /// A missing block and a target of <c>none</c> arrive as one record. <c>from</c> is the publishing block a missing
    /// tracker is derived from, and null for the framework's.
    /// </summary>
    private static ExportTracker Sound(ExportTracker? t, ExportPublishing? from) =>
        Read(t) is { } stated ? Tracker.For(stated.Target, stated.Base, stated.Area)
        : from is null ? Tracker.None
        : Tracker.From(from);

    /// <summary>A type naming no directory is read under its own name.</summary>
    private static ExportedType Sound(ExportedType t) => t with
    {
        Dir = Read(t.Dir) ?? t.Type,
        Sections = Read(t.Sections) ?? new Dictionary<string, string>(StringComparer.Ordinal)
    };

    /// <summary>
    /// The value typed as the deserialiser can leave it: a key the document lacks arrives as null whatever the record
    /// declares, so the null checks in <c>Sound</c> stay real questions to a reader and to an analyser.
    /// </summary>
    private static T? Read<T>(T value) => value;

    private static string Serialize(ExportRecord r) =>
        JsonSerializer.Serialize(r, KacJson.Relaxed.ExportRecord) + "\n";

    /// <summary>One object per line, because a JSONL line is the unit a grep returns.</summary>
    private static string Serialize(JsonObject line) =>
        JsonSerializer.Serialize(line, KacJson.Line.JsonObject);
}
