using System.Text;
using System.Text.Json;

// ---------------------------------------------------------------------------
// Export — the corpus as something a consumer reads without cloning it
// ---------------------------------------------------------------------------

namespace kac.core;

// One file the export writes, named relative to the export root.
public sealed record ExportFile(string Path, string Content);

// What an export comes to: the files it writes, the types it found something to write for, and the
// records it was told to leave behind. Named before anything is written, as `GeneratedFiles.Plan` is,
// so that deciding and doing stay apart and a test can ask what an export would contain without a
// filesystem.
//
// `Withheld` is carried out rather than swallowed. A corpus that excludes drafts publishes a smaller
// vocabulary than it holds, and the run that built it is the last place anyone will notice.
public sealed record ExportPlan(
    IReadOnlyList<ExportFile> Files,
    IReadOnlyList<ExportedType> Types,
    IReadOnlyList<string> Withheld);

// The facts that differ between two runs over one commit, gathered so the exporter takes them rather
// than reads them. A caller wanting byte-identical output holds them still, and a test supplies its own
// clock instead of racing the real one.
public sealed record ExportRun(string GeneratedAt, DateOnly Today, string? Commit, bool? Dirty);

// The corpus projected as data, for a consumer that reads it rather than cloning it.
//
// What travels is the type's decision, declared in its `export:` block, and this reads that declaration
// rather than holding a list of its own. A type declaring no block contributes nothing, and a corpus
// that adopted no exporting type produces a manifest and an empty type list — which is a statement of
// what that corpus has, and not a failure.
public static class Exporter
{
    // The shape of the output, versioned independently of anything the corpus says about itself.
    //
    // This number moves when the files below change shape, and a consumer reads it to know whether it
    // can parse what it was handed. `content-version` in `.corpus.yaml` moves when the words change, and
    // a consumer reads that to know whether to re-read them. Neither implies the other: a corpus can
    // rewrite every definition without this moving, and this can move over a corpus nobody has edited.
    public const int FormatVersion = 1;

    // Where an export lands. Untracked and rebuilt whole, so it is never a thing to review — which is
    // also why the write below deletes before it writes.
    public const string Dir = ".dist";

    public const string ManifestFile = "manifest.json";

    // What an export comes to, given a loaded corpus and the addresses its published form has.
    //
    // `type` narrows what is written and never what is read: the corpus arrives whole, so ids resolve
    // against every record rather than against the handful a narrowed run happened to load. A question
    // about the set, answered from some of its members, is answered wrongly.
    //
    // `run` carries the facts that differ between two runs over one commit, so nothing here reads a
    // clock and two runs from the same tree produce identical bytes but for the timestamp.
    public static ExportPlan Plan(LoadedCorpus corpus, Publishing? publishing, string? type, ExportRun run)
    {
        var files = new List<ExportFile>();
        var types = new List<ExportedType>();
        var withheld = new List<string>();

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
                ? PartsFile(records, t, publishing)
                : null;
            if (parts is not null) files.Add(parts);

            types.Add(new ExportedType(t.Key, records.Count, t.Key, parts?.Path));
        }

        files.Add(new ExportFile(ManifestFile,
            Serialize(new ExportManifest(
                FormatVersion,
                corpus.Descriptor.Name,
                corpus.Descriptor.ContentVersion,
                corpus.Descriptor.MechanismVersion,
                run.Commit,
                run.Dirty,
                run.GeneratedAt,
                new ExportPublishing(
                    corpus.Descriptor.PublishingTarget ?? Publishing.None,
                    publishing is null ? null : corpus.Descriptor.HumanBase,
                    publishing is null ? null : corpus.Descriptor.RawBase,
                    publishing?.Ref),
                types))));

        // The manifest is built last, because it reports what the rest of the run produced, and sorted
        // into place so the listing a caller prints reads in the order the files sit on disk.
        return new ExportPlan([.. files.OrderBy(f => f.Path, StringComparer.Ordinal)], types, withheld);
    }

    // Replace the export whole: delete what is there, then write. An export is not reviewed and nothing
    // flags a file in it that no longer has a record behind it, so overwriting in place would leave a
    // deleted record readable in the output indefinitely — the one failure mode where an untracked
    // artefact is worse than a tracked one.
    public static List<string> Write(string repoRoot, ExportPlan plan)
    {
        var root = Path.Combine(repoRoot, Dir);
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);

        var written = new List<string>();
        foreach (var file in plan.Files)
        {
            var full = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, file.Content);
            written.Add($"{Dir}/{file.Path}");
        }

        return written;
    }

    // Whether a record travels, which by default every record does.
    //
    // A corpus may exclude what has not settled — a draft, or a record whose review date has passed —
    // and the default is to carry both with their state attached. A consumer reading `status: draft`
    // beside a `review-by` two years gone can decide what to do about it; a consumer handed a filtered
    // export sees a corpus smaller than it is and nothing saying so.
    private static bool Travels(Doc doc, CorpusDescriptor descriptor, DateOnly today)
    {
        if (descriptor.ExportExclude.Count == 0) return true;

        if (descriptor.ExportExclude.Contains(CorpusDescriptor.ExcludeDraft, StringComparer.Ordinal)
            && string.Equals(doc.FrontScalar("status"), "draft", StringComparison.OrdinalIgnoreCase))
            return false;

        // A record carrying no `review-by`, or one nobody can parse, is never overdue: an absent date is
        // the field's own problem and the validator's to report, and reading it as expired here would
        // withhold a record over a fault in its frontmatter.
        return !descriptor.ExportExclude.Contains(CorpusDescriptor.ExcludeOverdue, StringComparer.Ordinal)
               || !DateOnly.TryParse(doc.FrontScalar("review-by"), out var due)
               || due >= today;
    }

    // The records of one type, most general first.
    //
    // `narrows` orders a chain and nothing orders one chain against another, so "most general first" is
    // not the total order it sounds like. A record narrowing nothing is a root; roots sort by id, each
    // root's chain follows it, and a record narrowing something outside the set is a root too — it has
    // nothing here to sit beneath. A type whose records narrow nothing at all sorts by id throughout,
    // which is the same rule with every record a root.
    //
    // The order is what a grep of the flat file meets, so a term redefined by a narrower record is met
    // in its general form first.
    //
    // Two records sharing an id would share an output filename, and the second would replace the first.
    // Nothing here guards it: `id-unique` reports a duplicate as an error, so the corpus is one `validate`
    // already refuses, and a second account of that fault here would report it in worse words.
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
            if (above is { Length: > 0 } parent && byId.ContainsKey(parent) && parent != Id(d))
            {
                if (!children.TryGetValue(parent, out var list)) children[parent] = list = [];
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

        // A cycle leaves its members unreachable from any root. They are still records of the corpus, so
        // they are exported rather than dropped, in id order, after everything the ordering could place.
        // Reporting the cycle is the validator's, and an export that silently held records back would be
        // a second, quieter account of what the corpus contains.
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
            fields[name] = name == Generator.Title ? doc.H1 : doc.FrontScalar(name);

        var sections = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, _) in export.Sections)
            if (doc.Sections.FirstOrDefault(s =>
                    string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase)) is { } section)
                sections[name] = doc.Text[section.BodyStart..section.BodyEnd].Trim();

        return new ExportRecord(t.Key, doc.Rel, fields, sections, Links(links));
    }

    // The flat file of every part of a type, one part to a line.
    //
    // JSONL rather than pretty JSON because the file exists to be grepped, and a hit has to hand back
    // something parseable on its own: a matching line of an indented document is a fragment, and the
    // reader is left seeking outward for its braces. Each line therefore repeats the record it came
    // from and the links back to it, which costs bytes and is the whole point.
    private static ExportFile? PartsFile(List<Doc> records, TypeSchema t, Publishing? publishing)
    {
        var spec = t.Parts!;
        var lines = new StringBuilder();

        foreach (var doc in records)
        {
            var id = Id(doc);
            foreach (var part in doc.Parts.OrderBy(p => p.Text, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(p => p.Id, StringComparer.Ordinal))
            {
                if (part.Id is not { Length: > 0 } partId) continue;

                var (lead, aside) = Split(doc, part, spec.Aside);
                lines.Append(Serialize(new ExportPartLine(
                    $"{id}.{partId}", part.Text, lead, aside, t.Key, id, partId,
                    Links(publishing?.Links(doc.Rel, partId)))));
                lines.Append('\n');
            }
        }

        // The file is named for what a type calls one of its parts, so a glossary's terms are found in
        // `terms.jsonl` by whoever was told to look for terms. A type whose records hold no addressable
        // part writes no file at all, and the manifest names none for it.
        return lines.Length > 0 ? new ExportFile($"{t.Key}/{spec.Noun}s.jsonl", lines.ToString()) : null;
    }

    // A part's body split into the two pieces a type writes it in: the lead, and the labelled block
    // beneath it that the type's `parts.aside:` names. Both are the source as written, trimmed, because
    // `full` fidelity promises the record's own words and markdown is the words it was written in.
    //
    // A body with no labelled block returns a null aside, which is the common case — the label marks
    // the confusion worth heading off, and most parts have none.
    private static (string Lead, string? Aside) Split(Doc doc, PartRow part, string label)
    {
        var body = part.Body(doc.Text).ToString();
        var blocks = body
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => b.Length > 0)
            .ToList();

        var marker = label.Length > 0 ? $"**{label}:**" : null;
        var lead = blocks.FirstOrDefault(b => marker is null || !b.StartsWith(marker, StringComparison.Ordinal)) ?? "";
        var aside = marker is null
            ? null
            : blocks.FirstOrDefault(b => b.StartsWith(marker, StringComparison.Ordinal))?[marker.Length..].Trim();

        return (lead, aside);
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
    private static string Serialize(ExportPartLine p) =>
        JsonSerializer.Serialize(p, KacJson.Line.ExportPartLine);
}
