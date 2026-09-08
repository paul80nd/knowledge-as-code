using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

// One corpus a report answers for, and the version of it the run read. The version is what
// `report-stale` compares against, so a report says which content it is true of rather than only when it
// was taken. OKF carries no version on a source, and `docs/design/reports.md` says why this one does.
public sealed record ReportSource(string Resource, string? Version);

// What produced a report and when, as the frontmatter of the record it becomes.
//
// `By` takes OKF's `<producer>/<version>` form, so a later run by an agent names itself the same way the
// tool does.
public sealed record ReportStamp(string By, string At, IReadOnlyList<ReportSource> Sources);

// What a report comes to, decided before anything is written, as `ExportPlan` and `BundlePlan` are. A
// test asks what a report would say without a filesystem, and the command prints what it answers.
//
// The mechanical half fills what the corpus states. Every judgement is left open, because the tool
// cannot tell a gap from a thing that does not exist here. `docs/cli/report.md` carries that division.
public sealed record ReportPlan(string Name, string Title, string Frontmatter, string Body)
{
    public string Text => $"---\n{Frontmatter}---\n\n# {Title}\n\n{Body}";
}

// One policy clause, whoever wrote it, as a report reads it.
//
// `Alignment` is the raw cell and stands null for an inherited clause, whose export left the column
// behind. `Shortcode` is null for a clause this corpus wrote.
public sealed record ClauseFact(
    string Id,
    string Record,
    string Key,
    string? Level,
    string Text,
    string? Alignment,
    IReadOnlyList<string> AlignmentLabels,
    string? Shortcode);

// One row of the coverage report: a clause and every edge the corpus draws back to it.
public sealed record CoverageRow(
    ClauseFact Clause,
    IReadOnlyList<string> CoveredBy,
    IReadOnlyList<string> Deviations,
    IReadOnlyList<string> Controls,
    string? Pair);

// One row of the framework report: a reference into a framework, and what cites it.
//
// `Standing` is the heading the register files the framework under, verbatim, and null where the
// register places it nowhere. The tool states the corpus's own word and never rules on what it means, so
// a corpus renaming its standings reports in the vocabulary it wrote.
//
// `Path` and `Anchor` address the register entry itself, so a reader holding this row can reach the
// page that placed the framework. Both stand null where the register places it nowhere.
public sealed record FrameworkRow(
    string Framework,
    string? Reference,
    string? Standing,
    IReadOnlyList<string> Clauses,
    IReadOnlyList<string> Records,
    string? Path,
    string? Anchor);

// The reports `kac report` can print, and what each comes to.
//
// A report reads the corpus and everything under `.imports/`. A producer cannot see what its consumers
// implement, so an uncovered clause means uncovered here, and every report states that limit itself.
public static class Reports
{
    public const string Coverage = "coverage";
    public const string Frameworks = "frameworks";

    public static readonly IReadOnlyList<string> Names = [Coverage, Frameworks];

    // The three fields a coverage report walks back. Named here rather than read from the schema,
    // because no declaration says which field covers an obligation and which departs from one: a type
    // declares a `ref:` and a `part-required:`, and only this report knows what each edge means.
    // `AlignmentRollup` names `aligns-with` for the same reason.
    private const string Implements = "implements";
    private const string DepartsFrom = "departs-from";
    private const string Verifies = "verifies";

    // The column a clause states its framework mapping in. `AlignmentRollup` reads the same one.
    private const string AlignmentColumn = "Alignment";

    public static ReportPlan? Plan(
        string name, LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, ReportStamp stamp) =>
        name switch
        {
            Coverage => CoveragePlan(corpus, inherited, stamp),
            Frameworks => FrameworksPlan(corpus, inherited, stamp),
            _ => null
        };

    // Every policy clause the corpus can see, its own first and then each corpus it consumes, in the
    // order a reader meets them.
    public static List<ClauseFact> Clauses(LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited)
    {
        var found = new List<ClauseFact>();

        foreach (var doc in ClauseDocs(corpus))
        {
            var record = doc.FrontScalar("id");
            if (record is null) continue;

            foreach (var row in doc.Parts)
            {
                if (row.Id is not { Length: > 0 } key) continue;

                var cell = row.Cells?.GetValueOrDefault(AlignmentColumn);
                var labels = row.CellLinks?.GetValueOrDefault(AlignmentColumn) ?? [];

                found.Add(new ClauseFact($"{record}.{key}", record, key,
                    doc.Type?.DeclaredParts.Modal(row.Text), row.Text, cell, labels, null));
            }
        }

        // Only the clause-bearing type. Every type carrying parts exports a flat file, so reading them
        // all would file a glossary's terms as obligations.
        var clauseType = ClauseType(corpus)?.Key;

        foreach (var source in inherited)
        foreach (var type in source.Types.Where(t => t.Type == clauseType))
        foreach (var line in type.PartLines)
        {
            if (JsonRead.Parse(line) is not { } json) continue;

            var id = JsonRead.Str(json[type.IdKey ?? "id"]);
            var record = JsonRead.Str(json[type.RecordKey ?? "record"]);
            var key = JsonRead.Str(json[type.PartKey ?? "part"]);
            if (id is null || record is null || key is null) continue;

            // Scoped here rather than in the export, because `.imports/` holds what the producer
            // published and the shortcode is the consumer's name for it.
            found.Add(new ClauseFact(
                Exporter.Scoped(id, source.Shortcode), Exporter.Scoped(record, source.Shortcode), key,
                JsonRead.Str(json["level"]), JsonRead.Str(json["clause"]) ?? "", null, [],
                source.Shortcode));
        }

        return found;
    }

    // The type whose parts are clauses, found through what it declares rather than by its name, so a
    // corpus that renamed it still reports.
    //
    // Read from the whole schema rather than from what this corpus adopted. A consumer inheriting every
    // policy it answers to adopts the type itself, and would otherwise report on none of them.
    private static TypeSchema? ClauseType(LoadedCorpus corpus) =>
        corpus.Schema.ByFolder.Values.FirstOrDefault(t => t.Parts is { } parts
            && parts.Columns.Any(c => string.Equals(c, AlignmentColumn, StringComparison.OrdinalIgnoreCase)));

    // This corpus's own records of that type.
    private static IEnumerable<Doc> ClauseDocs(LoadedCorpus corpus)
    {
        var type = ClauseType(corpus);
        return type is null ? [] : corpus.Docs.Where(d => d.Type?.Key == type.Key);
    }

    private static ReportPlan CoveragePlan(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, ReportStamp stamp)
    {
        var clauses = Clauses(corpus, inherited);
        var covers = Edges(corpus, inherited, Implements);
        var departures = Edges(corpus, inherited, DepartsFrom);
        var controls = Edges(corpus, inherited, Verifies);
        var pairs = Pairs(clauses);

        var rows = clauses.Select(c => new CoverageRow(c,
            covers.GetValueOrDefault(c.Id) ?? [],
            departures.GetValueOrDefault(c.Id) ?? [],
            [.. (covers.GetValueOrDefault(c.Id) ?? []).SelectMany(s => controls.GetValueOrDefault(s) ?? [])
                .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
            pairs.GetValueOrDefault(c.Id))).ToList();

        var body = new StringBuilder();
        Limits(body, corpus, inherited);
        CoverageTotals(body, rows);
        CoverageSections(body, rows);
        Judgement(body, "Every `uncovered` row needs a verdict.",
            "The tool prints `covered` and `uncovered` and stops. Splitting `uncovered` into a gap and "
            + "something out of scope is a judgement about this estate, and it belongs to whoever "
            + "confirms this report.");

        return new ReportPlan(Coverage, "Clause coverage", Frontmatter(stamp), body.ToString().TrimEnd() + "\n");
    }

    private static ReportPlan FrameworksPlan(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, ReportStamp stamp)
    {
        var rows = FrameworkRows(corpus, inherited);

        var body = new StringBuilder();
        Limits(body, corpus, inherited);
        FrameworkTotals(body, rows);
        FrameworkSections(body, rows);
        Judgement(body, "A reference on one clause is one citation from losing its coverage.",
            "The tool counts the citations it can see. Whether a control still has honest coverage, and "
            + "whether an uncited reference should be removed from the register, belongs to whoever "
            + "confirms this report.");

        return new ReportPlan(Frameworks, "Framework coverage", Frontmatter(stamp),
            body.ToString().TrimEnd() + "\n");
    }

    // Every reference the clause tables cite, keyed so a reference six clauses reach is one row.
    //
    // Local clauses alone. An inherited clause left its `Alignment` cell behind, and `docs/design/export.md`
    // says why. A consumer reads its producer's own report for that half.
    public static List<FrameworkRow> FrameworkRows(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited)
    {
        var register = new Register(corpus.Tree, []);
        var byReference = new Dictionary<string, (string Framework, string? Reference, List<string> Clauses)>(
            StringComparer.Ordinal);

        foreach (var doc in ClauseDocs(corpus))
        {
            var record = doc.FrontScalar("id");
            if (record is null) continue;

            foreach (var row in doc.Parts)
            {
                if (row.Id is not { Length: > 0 } key) continue;
                if (row.Cells?.GetValueOrDefault(AlignmentColumn) is not { Length: > 0 } cell) continue;

                var labels = row.CellLinks?.GetValueOrDefault(AlignmentColumn) ?? [];

                foreach (var (framework, reference) in Alignment.References(cell, labels))
                {
                    var joined = Alignment.Join(framework, reference);
                    if (!byReference.TryGetValue(joined, out var entry))
                        byReference[joined] = entry = (framework, reference, []);
                    entry.Clauses.Add($"{record}.{key}");
                }
            }
        }

        return
        [
            .. byReference.OrderBy(e => e.Value.Framework, StringComparer.Ordinal)
                .ThenBy(e => e.Value.Reference ?? "", Comparer<string>.Create(Natural.Compare))
                .Select(e => Row(e.Value.Framework, e.Value.Reference, e.Value.Clauses,
                    Placed(register, corpus, e.Value.Framework)))
        ];
    }

    // One reference, with the policies holding its clauses taken from the clause ids themselves. A
    // clause id opens with the record that wrote it, so the set of policies is already in hand.
    private static FrameworkRow Row(string framework, string? reference, List<string> clauses,
        (string? Standing, string? Path, string? Anchor) placed) =>
        new(framework, reference, placed.Standing, clauses,
            [.. clauses.Select(c => c[..c.LastIndexOf('.')]).Distinct(StringComparer.Ordinal)],
            placed.Path, placed.Anchor);

    // Where the register puts a framework: the word it is filed under, the page carrying the entry, and
    // the anchor a link reaches it by. Read through a document citing it, because the label is defined
    // as a link and only a citing document holds that definition.
    //
    // The register is built with no standings, because this asks it to place a framework and never to
    // weigh one. Which standings bind is `alignment-rollup`'s judgement, and a report repeating it would
    // tie what it prints to a rule's configuration it has no other use for.
    private static (string? Standing, string? Path, string? Anchor) Placed(
        Register register, LoadedCorpus corpus, string framework)
    {
        foreach (var doc in ClauseDocs(corpus))
            if (register.Reaches(doc, framework) is ({ } page, { } anchor))
                return (register.Filed(doc, framework), page, anchor);

        return (null, null, null);
    }

    // Every id a field of the given name names, keyed by the id it points at. One pass over every
    // adopted type, so a corpus declaring the field on two types reports both.
    //
    // Inherited records count. A corpus consuming the policies it answers to consumes the standards
    // discharging them, and a report reading only local records would call two thirds of its own
    // coverage a gap. `.schema/standards.yaml` says why the field travels.
    private static Dictionary<string, List<string>> Edges(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, string field)
    {
        var found = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void Add(string target, string id) =>
            (found.TryGetValue(target, out var list) ? list : found[target] = []).Add(id);

        foreach (var doc in corpus.Docs)
        {
            if (doc.Type?.Fields.ContainsKey(field) != true) continue;
            if (doc.FrontScalar("id") is not { Length: > 0 } id) continue;

            foreach (var target in doc.FrontList(field)) Add(target, id);
        }

        foreach (var source in inherited)
        foreach (var type in source.Types)
        foreach (var record in type.Records)
        {
            if (JsonRead.Parse(record.Content) is not { } json) continue;
            if (JsonRead.Object(json["fields"]) is not { } fields) continue;
            if (JsonRead.Str(fields["id"]) is not { Length: > 0 } id) continue;
            if (fields[field] is not JsonArray targets) continue;

            foreach (var target in targets)
                if (JsonRead.Str(target) is { Length: > 0 } value)
                    Add(Exporter.Scoped(value, source.Shortcode),
                        Exporter.Scoped(id, source.Shortcode));
        }

        return found;
    }

    // The clause a clause is likely paired with: the same key in another policy, which is how this
    // corpus writes one obligation stated from both sides.
    //
    // A candidate rather than a fact. Two policies may reach for one word by coincidence, so the report
    // offers the match and whoever confirms it decides.
    private static Dictionary<string, string> Pairs(IReadOnlyList<ClauseFact> clauses)
    {
        var byKey = clauses.GroupBy(c => c.Key, StringComparer.Ordinal).Where(g => g.Count() > 1);
        var found = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var group in byKey)
        foreach (var clause in group)
            found[clause.Id] = string.Join(", ",
                group.Where(o => o.Id != clause.Id).Select(o => o.Id).Order(StringComparer.Ordinal));

        return found;
    }

    private static string Frontmatter(ReportStamp stamp)
    {
        var sb = new StringBuilder();
        sb.Append("id: rpt-\n");
        sb.Append("type: report\n");
        sb.Append("tier: descriptive\n");
        sb.Append("owner: human:\n");
        sb.Append($"generated: {{ at: {stamp.At}, by: {stamp.By} }}\n");
        sb.Append("sources:\n");

        foreach (var source in stamp.Sources)
            sb.Append($"  - {{ resource: {source.Resource}, version: \"{source.Version ?? ""}\" }}\n");

        sb.Append("confirmed: []\n");
        return sb.ToString();
    }

    // What the report can and cannot see, stated in the report rather than left to a reader. A producer
    // publishing policies cannot see what implements them elsewhere, so an uncovered clause here means
    // uncovered here.
    private static void Limits(
        StringBuilder body, LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited)
    {
        var name = corpus.Descriptor.Name ?? "this corpus";

        body.AppendLine("## Limits");
        body.AppendLine();
        body.AppendLine($"This reads `{name}` and what it imports. A clause uncovered here may well be covered in a "
                        + "corpus consuming this one, and every consumer answers for its own coverage.");
        body.AppendLine();

        if (inherited.Count > 0)
        {
            body.AppendLine("Imported:");
            body.AppendLine();
            foreach (var source in inherited)
                body.AppendLine($"* `{source.Shortcode}`, {source.Corpus ?? "unnamed"}, at "
                                + $"{source.ContentVersion ?? "an unstated version"}.");
            body.AppendLine();
        }

        body.AppendLine("No column here says a clause is verified. A control names a standard and not a rule, so it "
                        + "vouches for a whole document whatever it checks inside it.");
        body.AppendLine();
    }

    private static void CoverageTotals(StringBuilder body, IReadOnlyList<CoverageRow> rows)
    {
        body.AppendLine("## Totals");
        body.AppendLine();
        body.AppendLine("| Policy | Clauses | Covered | Uncovered |");
        body.AppendLine("|--------|---------|---------|-----------|");

        foreach (var group in rows.GroupBy(r => r.Clause.Record, StringComparer.Ordinal)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
            body.AppendLine($"| `{group.Key}` | {group.Count()} | {group.Count(r => r.CoveredBy.Count > 0)} "
                            + $"| {group.Count(r => r.CoveredBy.Count == 0)} |");

        body.AppendLine($"| **Total** | **{rows.Count}** | **{rows.Count(r => r.CoveredBy.Count > 0)}** "
                        + $"| **{rows.Count(r => r.CoveredBy.Count == 0)}** |");
        body.AppendLine();
    }

    private static void CoverageSections(StringBuilder body, IReadOnlyList<CoverageRow> rows)
    {
        body.AppendLine("## Clauses");
        body.AppendLine();

        foreach (var group in rows.GroupBy(r => r.Clause.Record, StringComparer.Ordinal)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            body.AppendLine($"### {group.Key}");
            body.AppendLine();
            body.AppendLine("| Clause | Level | Covered by | Deviations | Controls | Pair candidate | Verdict |");
            body.AppendLine("|--------|-------|------------|------------|----------|----------------|---------|");

            foreach (var row in group)
                body.AppendLine($"| `{row.Clause.Key}` | {row.Clause.Level ?? ""} "
                                + $"| {Cite(row.CoveredBy)} | {Cite(row.Deviations)} | {Cite(row.Controls)} "
                                + $"| {(row.Pair is null ? "" : $"`{row.Pair}`")} "
                                + $"| {(row.CoveredBy.Count > 0 ? "covered" : "uncovered")} |");

            body.AppendLine();
        }
    }

    private static void FrameworkTotals(StringBuilder body, IReadOnlyList<FrameworkRow> rows)
    {
        body.AppendLine("## Totals");
        body.AppendLine();
        body.AppendLine("| Framework | Standing | References | Cited once |");
        body.AppendLine("|-----------|----------|------------|------------|");

        foreach (var group in rows.GroupBy(r => r.Framework, StringComparer.Ordinal)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
            body.AppendLine($"| {group.Key} | {group.First().Standing ?? ""} | {group.Count()} "
                            + $"| {group.Count(r => r.Clauses.Count == 1)} |");

        body.AppendLine($"| **Total** | | **{rows.Count}** | **{rows.Count(r => r.Clauses.Count == 1)}** |");
        body.AppendLine();
    }

    private static void FrameworkSections(StringBuilder body, IReadOnlyList<FrameworkRow> rows)
    {
        body.AppendLine("## References");
        body.AppendLine();

        foreach (var group in rows.GroupBy(r => r.Framework, StringComparer.Ordinal)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            body.AppendLine($"### {group.Key}");
            body.AppendLine();
            body.AppendLine("| Reference | Clauses | Policies | Note |");
            body.AppendLine("|-----------|---------|----------|------|");

            foreach (var row in group)
                body.AppendLine($"| {row.Reference ?? "the framework entire"} | {Cite(row.Clauses)} "
                                + $"| {Cite(row.Records)} | |");

            body.AppendLine();
        }
    }

    // What the report leaves open, said once at the foot so nobody takes an empty column for an answer.
    private static void Judgement(StringBuilder body, string lead, string detail)
    {
        body.AppendLine("## What this leaves open");
        body.AppendLine();
        body.AppendLine($"**{lead}** {detail}");
        body.AppendLine();
    }

    private static string Cite(IReadOnlyList<string> ids) =>
        ids.Count == 0 ? "" : string.Join(", ", ids.Select(i => $"`{i}`"));
}
