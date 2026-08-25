using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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
public sealed record ExportPlan(
    IReadOnlyList<ExportFile> Files,
    IReadOnlyList<ExportedType> Types,
    IReadOnlyList<string> Withheld,
    IReadOnlyList<string> Unread);

// The facts that differ between two runs over one commit, gathered so the exporter takes them rather
// than reads them. A caller wanting byte-identical output holds them still. A test supplies its own
// clock instead of racing the real one.
public sealed record ExportRun(string GeneratedAt, DateOnly Today, string? Commit, bool? Dirty);

// The corpus projected as data, for a consumer that reads it rather than cloning it.
//
// This reads each type's `export:` block and holds no list of its own, so a new type needs no line here.
// `docs/cli/export.md` says what it all comes to, and why.
public static class Exporter
{
    // The shape of the output, versioned independently of anything the corpus says about itself.
    // `Bundler` refuses an export built to another shape, so moving this number means rebuilding every
    // export on disk. `docs/cli/export.md` says what moves it.
    public const int FormatVersion = 2;

    public const string ManifestFile = "manifest.json";

    // What an export comes to, given a loaded corpus and the addresses its published form has.
    //
    // `type` narrows what is written and never what is read. The corpus arrives whole.
    //
    // `run` holds everything that varies between runs, so nothing here reads a clock. Two runs from
    // the same tree produce identical bytes but for the timestamp.
    public static ExportPlan Plan(LoadedCorpus corpus, Publishing? publishing, string? type, ExportRun run)
    {
        var files = new List<ExportFile>();
        var types = new List<ExportedType>();
        var withheld = new List<string>();
        var unread = new List<string>();

        foreach (var t in corpus.Adopted)
        {
            if (t.Export is null) continue;
            if (type is not null && !string.Equals(t.Key, type, StringComparison.Ordinal)) continue;

            var held = corpus.Docs.Where(d => d.Type?.Key == t.Key)
                .ToLookup(d => Travels(d, corpus.Descriptor, run.Today));

            withheld.AddRange(held[false].Select(Id).OrderBy(id => id, StringComparer.Ordinal));

            var records = Ordered([.. held[true]]);
            if (records.Count == 0) continue;

            foreach (var doc in records)
                files.Add(new ExportFile($"{t.Key}/{Id(doc)}.json", Serialize(Record(doc, t, publishing))));

            var parts = t.Export.Parts.Length > 0 && t.Parts is not null
                ? PartsFile(records, t, corpus.Tree, unread)
                : null;
            if (parts is not null) files.Add(parts);

            // Two counts, because they answer two questions and one number cannot. `ExportedType` says
            // which is which.
            types.Add(new ExportedType(
                t.Key, t.Export.Version, records.Count,
                parts is null ? 0 : Lines(parts.Content), t.Key, parts?.Path));
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
                Addresses(corpus.Descriptor, publishing),
                types))));

        // The manifest is built last, because it reports what the rest of the run produced. Sorting
        // every file by path makes the listing a caller prints read in the order they sit on disk.
        return new ExportPlan([.. files.OrderBy(f => f.Path, StringComparer.Ordinal)], types, withheld,
            [.. unread.Distinct(StringComparer.Ordinal).OrderBy(u => u, StringComparer.Ordinal)]);
    }

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
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
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
    // What a reader may take from that order, and what they may not, is in `docs/cli/export.md`.
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

    private static ExportRecord Record(Doc doc, TypeSchema t, Publishing? publishing)
    {
        var export = t.Export!;
        var links = publishing?.Links(doc.Rel);

        var fields = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var name in export.Fields)
            fields[name] = Absent(name == Generator.Title ? doc.H1 : doc.FrontScalar(name));

        var sections = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, _) in export.Sections)
            if (doc.Sections.FirstOrDefault(s =>
                    string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase)) is { } section)
                sections[name] = Unwrap(doc.Text[section.BodyStart..section.BodyEnd]);

        return new ExportRecord(t.Key, doc.Rel, fields, sections, Links(links));
    }

    // Every absent value an export writes, spelled one way. A corpus writing `narrows:` with nothing
    // after it has not narrowed anything, so blank and missing arrive as the same `null`. What that buys
    // a consumer is in `docs/cli/export.md`.
    private static string? Absent(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    // The source's wrap column, taken back out. Blank lines are the author's and stay. A block a joiner
    // would mangle is left as written. `docs/cli/export.md` says why the export does this at
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

    // The flat file of every addressable part of a type, one part to a line. JSONL rather than pretty
    // JSON, and each line repeats what a reader would otherwise have to look up.
    // `docs/cli/export.md` says what that costs and what it buys.
    //
    // What a line holds is the type's to declare. Nothing here names a key, so a second type exporting
    // parts costs an `export.parts.line:` block and no line of C#.
    private static ExportFile? PartsFile(List<Doc> records, TypeSchema t, Tree tree, List<string> unread)
    {
        var spec = t.Parts!;
        var byPath = records.ToDictionary(d => d.Rel, StringComparer.Ordinal);
        var lines = new StringBuilder();

        foreach (var doc in records)
        {
            var id = Id(doc);
            foreach (var row in doc.Parts.OrderBy(p => p.Text, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(p => p.Id, StringComparer.Ordinal))
            {
                if (row.Id is not { Length: > 0 } partId) continue;

                var (lead, aside) = Split(doc, row, spec.Aside);
                var part = new Part(doc, row, partId, id, lead, aside, SeeAlso(doc, row, byPath, tree));

                var line = new JsonObject();
                foreach (var (key, source) in t.Export!.Line) line[key] = Value(source, part, t);

                lines.Append(Serialize(line));
                lines.Append('\n');
                unread.AddRange(Unread(doc, row, partId, byPath, tree));
            }
        }

        // The file is named for what a type calls one of its parts, so a reader told to look for terms
        // finds a glossary's terms in `terms.jsonl`. A type whose records hold no addressable part
        // writes no file at all, and the manifest names none for it.
        return lines.Length > 0 ? new ExportFile($"{t.Key}/{spec.Noun}s.jsonl", lines.ToString()) : null;
    }

    // One part, and everything a line about it is built from. Gathered once so each source below reads a
    // value rather than works one out, and so no source can reach past the part it is describing.
    private sealed record Part(
        Doc Doc, PartRow Row, string Id, string Record, string? Lead, string? Aside,
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

            // The anchor a part resolves at. A heading's slug is its id and its anchor alike, and a
            // table-sourced type authors an id no fragment resolves to. `docs/cli/export.md` carries
            // that under its known limits.
            PartLineSource.PartAnchor => JsonValue.Create(part.Id),

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
    // resolves to nothing, and `Unread` below reports it. `docs/cli/export.md` sets out why the
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
    private static (string? Lead, string? Aside) Split(Doc doc, PartRow part, string label)
    {
        var blocks = part.Body(doc.Text).ToString()
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(Unwrap)
            .Where(b => b.Length > 0)
            .ToList();

        var marker = label.Length > 0 ? $"**{label}:**" : null;
        var lead = blocks.FirstOrDefault(b => marker is null || !b.StartsWith(marker, StringComparison.Ordinal));
        var aside = marker is null
            ? null
            : blocks.FirstOrDefault(b => b.StartsWith(marker, StringComparison.Ordinal))?[marker.Length..].Trim();

        return (Absent(lead), Absent(aside));
    }

    // How many parts a flat file holds, counted off the file itself. One part is one line, so the count
    // the manifest states and the file a consumer greps cannot come apart.
    private static int Lines(string content) =>
        content.Count(c => c == '\n');

    // How this export's links are built, for the manifest: the two templates, or nulls where the corpus
    // has no address the tool can build on. Stated once here and substituted by whoever reads a line,
    // rather than resolved onto every line of the flat file.
    private static ExportPublishing Addresses(CorpusDescriptor descriptor, Publishing? publishing)
    {
        var target = descriptor.PublishingTarget ?? Publishing.None;
        var templates = publishing?.Templates();

        return new ExportPublishing(target, templates?.Human, templates?.Raw, publishing?.Ref);
    }

    private static ExportLinks? Links(PublishedLinks? links) =>
        links is null ? null : new ExportLinks(links.Human, links.Raw);

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
