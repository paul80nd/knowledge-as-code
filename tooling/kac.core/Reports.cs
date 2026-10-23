using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// One corpus a report answers for, and the version of it the run read, which <c>report-stale</c> compares against.
/// <c>docs/use/cli/report.md</c> says why a source carries a version.
/// </summary>
public sealed record ReportSource(string Resource, string? Version);

/// <summary>
/// What produced a report and when, as the frontmatter of the record it becomes. <c>By</c> takes any of OKF's actor
/// forms. <c>docs/use/cli/report.md</c> carries the handover.
/// </summary>
public sealed record ReportStamp(string By, string At, IReadOnlyList<ReportSource> Sources)
{
    /// <summary>This tool's stamp, from the version string the assembly was built with.</summary>
    /// <remarks>
    /// The build metadata <c>--version</c> carries is cut off, because <c>&lt;producer&gt;/&lt;version&gt;</c> admits
    /// a release and not a commit, so two reports differ only where their content does.
    /// </remarks>
    public static ReportStamp ForTool(string toolVersion, string at, IReadOnlyList<ReportSource> sources)
        => new($"kac/{toolVersion.Split('+')[0]}", at, sources);
}

/// <summary>
/// What a report comes to, decided before anything is written, as <c>ExportPlan</c> and <c>BundlePlan</c> are. Every
/// judgement is left open. <c>docs/use/cli/report.md</c> carries that division.
/// </summary>
public sealed record ReportPlan(string Name, string Title, string Frontmatter, string Body)
{
    public string Text => $"---\n{Frontmatter}---\n\n# {Title}\n\n{Body}";
}

/// <summary>
/// One policy clause, whoever wrote it, as a report reads it. <c>Alignment</c> is null for an inherited clause, whose
/// export left the column behind. <c>Shortcode</c> is null for a clause this corpus wrote.
/// </summary>
public sealed record ClauseFact(
    string Id,
    string Record,
    string Key,
    string? Level,
    string Text,
    string? Alignment,
    IReadOnlyList<string> AlignmentLabels,
    string? Shortcode);

/// <summary>One row of the coverage report: a clause and every edge the corpus draws back to it.</summary>
public sealed record CoverageRow(
    ClauseFact Clause,
    IReadOnlyList<string> CoveredBy,
    IReadOnlyList<string> Deviations,
    IReadOnlyList<string> Controls,
    string? Pair);

/// <summary>
/// One row of the framework report: a reference into a framework, and what cites it. <c>Standing</c> is the heading the
/// register files the framework under, verbatim. <c>Standing</c>, <c>Path</c> and <c>Anchor</c> are null where the
/// register places it nowhere.
/// </summary>
public sealed record FrameworkRow(
    string Framework,
    string? Reference,
    string? Standing,
    IReadOnlyList<string> Clauses,
    IReadOnlyList<string> Records,
    string? Path,
    string? Anchor);

/// <summary>
/// The reports <c>kac report</c> can print. A report reads the corpus and everything under <c>.imports/</c>, so an
/// uncovered clause means uncovered here, and every report states that limit.
/// </summary>
public static class Reports
{
    public const string Coverage = "coverage";
    public const string Frameworks = "frameworks";

    public static readonly IReadOnlyList<string> Names = [Coverage, Frameworks];

    /// <summary>
    /// The three fields a coverage report walks back. Named here, because no declaration says which field covers an
    /// obligation and which departs from one. <c>AlignmentRollup</c> names <c>aligns-with</c> for the same reason.
    /// </summary>
    private const string Implements = "implements";
    private const string DepartsFrom = "departs-from";
    private const string Verifies = "verifies";

    /// <summary>
    /// The column a clause states its framework mapping in. <c>AlignmentRollup</c> reads the same one.
    /// </summary>
    private const string AlignmentColumn = "Alignment";

    /// <summary>
    /// What a report calls a corpus whose descriptor states no <c>corpus:</c>. Only a printed report meets it.
    /// </summary>
    private const string Unnamed = "this corpus";

    /// <summary>The corpora a report answers for, this one first and then each corpus it consumes.</summary>
    public static List<ReportSource> Sources(CorpusDescriptor descriptor, IReadOnlyList<InheritedCorpus> inherited) =>
    [
        new(descriptor.Name ?? Unnamed, descriptor.ContentVersion),
        .. inherited.Select(i => new ReportSource(i.Corpus ?? i.Shortcode, i.ContentVersion))
    ];

    public static ReportPlan? Plan(
        string name, LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, ReportStamp stamp) =>
        name switch
        {
            Coverage => CoveragePlan(corpus, inherited, stamp),
            Frameworks => FrameworksPlan(corpus, inherited, stamp),
            _ => null
        };

    /// <summary>Every policy clause the corpus can see, its own first and then each corpus it consumes.</summary>
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

        // Only the clause-bearing type. Reading every type carrying parts would file a glossary's terms as obligations.
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

            // Scoped here, because `.imports/` holds what the producer published and the shortcode is the consumer's
            // name for it.
            found.Add(new ClauseFact(
                Exporter.Scoped(id, source.Shortcode), Exporter.Scoped(record, source.Shortcode), key,
                JsonRead.Str(json["level"]), JsonRead.Str(json["clause"]) ?? "", null, [],
                source.Shortcode));
        }

        return found;
    }

    /// <summary>
    /// The type whose parts are clauses, found through what it declares, so a corpus that renamed it still reports.
    /// Read from the whole schema, because a consumer inheriting every policy may not adopt the type itself.
    /// </summary>
    private static TypeSchema? ClauseType(LoadedCorpus corpus) =>
        corpus.Schema.ByFolder.Values.FirstOrDefault(t => t.Parts is { } parts
                                                          && parts.Columns.Any(c =>
                                                              string.Equals(c, AlignmentColumn,
                                                                  StringComparison.OrdinalIgnoreCase)));

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
        var departures = Edges(corpus, inherited, DepartsFrom, inForceOnly: true);
        var controls = Edges(corpus, inherited, Verifies);
        var pairs = Pairs(clauses);

        var rows = clauses.Select(c => new CoverageRow(c,
            covers.GetValueOrDefault(c.Id) ?? [],
            departures.GetValueOrDefault(c.Id) ?? [],
            [
                .. (covers.GetValueOrDefault(c.Id) ?? []).SelectMany(s => controls.GetValueOrDefault(s) ?? [])
                .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
            ],
            pairs.GetValueOrDefault(c.Id))).ToList();

        var body = new StringBuilder();
        Limits(body, corpus, inherited,
            "A clause uncovered here may well be covered in a corpus consuming this one, and every "
            + "consumer answers for its own coverage.",
            "No column here says a clause is verified. A control names a standard and not a rule, so it "
            + "vouches for a whole document whatever it checks inside it."
            + (DeviationInForce(corpus) is { } inForce
                ? $" The `Deviations` column lists a deviation only while its `status` is `{inForce}`."
                : ""));
        CoverageTotals(body, rows);
        CoverageSections(body, rows);
        Judgement(body, "The tool prints two verdicts and a person writes the rest.",
            "It prints `covered` and `uncovered`, and splitting `uncovered` into a gap and something out "
            + "of scope is a judgement about this estate. That judgement, and the `Note` beside it, "
            + "belong to whoever confirms this report.");

        return new ReportPlan(Coverage, "Clause coverage", Frontmatter(stamp, Coverage),
            body.ToString().TrimEnd() + "\n");
    }

    private static ReportPlan FrameworksPlan(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, ReportStamp stamp)
    {
        var rows = FrameworkRows(corpus, inherited);

        var body = new StringBuilder();
        Limits(body, corpus, inherited,
            "An `Alignment` cell stays in the corpus that wrote it, so the citations counted below are "
            + "the ones written here. A reference an imported policy cites belongs to that producer's "
            + "own report.",
            "No column here says a clause meets the reference it cites. A citation records that the "
            + "clause named it. Whoever confirms this report decides whether the clause covers that "
            + "control or one corner of it.");
        FrameworkTotals(body, rows);
        FrameworkSections(body, rows);
        Judgement(body, "A reference on one clause is one citation from losing its coverage.",
            "The tool counts the citations it can see. Whether a control still has honest coverage, and "
            + "whether an uncited reference should be removed from the register, belongs to whoever "
            + "confirms this report.");

        return new ReportPlan(Frameworks, "Framework coverage", Frontmatter(stamp, Frameworks),
            body.ToString().TrimEnd() + "\n");
    }

    /// <summary>
    /// Every reference the clause tables cite, keyed so a reference six clauses reach is one row. Local clauses alone,
    /// because an inherited clause left its <c>Alignment</c> cell behind. See <c>docs/reference/export.md</c>.
    /// </summary>
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

    /// <summary>
    /// One reference, with the policies taken from the clause ids, which open with the record that wrote them.
    /// </summary>
    private static FrameworkRow Row(string framework, string? reference, List<string> clauses,
        (string? Standing, string? Path, string? Anchor) placed) =>
        new(framework, reference, placed.Standing, clauses,
            [.. clauses.Select(c => c[..c.LastIndexOf('.')]).Distinct(StringComparer.Ordinal)],
            placed.Path, placed.Anchor);

    /// <summary>
    /// Where the register puts a framework: the word it is filed under, the page carrying the entry, and the anchor.
    /// Read through a document citing it, because only a citing document holds the label's definition. The register is
    /// built with no standings, because this places a framework and never weighs one.
    /// </summary>
    private static (string? Standing, string? Path, string? Anchor) Placed(
        Register register, LoadedCorpus corpus, string framework)
    {
        foreach (var doc in ClauseDocs(corpus))
            if (register.Reaches(doc, framework) is ({ } page, { } anchor))
                return (register.Filed(doc, framework), page, anchor);

        return (null, null, null);
    }

    /// <summary>
    /// The status the type declaring <c>departs-from</c> puts a deviation in force at, or null where none states one.
    /// </summary>
    private static string? DeviationInForce(LoadedCorpus corpus) =>
        corpus.Schema.ByFolder.Values.Where(t => t.Fields.ContainsKey(DepartsFrom))
            .Select(t => corpus.Schema.EffectiveField(t, "status")?.InForce)
            .FirstOrDefault(v => v is not null);

    /// <summary>
    /// Every id a field of the given name names, keyed by the id it points at, across every adopted type. Inherited
    /// records count, and both halves of an inherited edge are scoped to the corpus that wrote the record, so the join
    /// meets on both sides. <c>inForceOnly</c> drops a record whose <c>status</c> is not the one its type names in
    /// <c>in-force:</c>, and keeps every record of a type that names none.
    /// </summary>
    private static Dictionary<string, List<string>> Edges(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, string field, bool inForceOnly = false)
    {
        var found = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void Add(string target, string id) =>
            (found.TryGetValue(target, out var list) ? list : found[target] = []).Add(id);

        bool Lapsed(string? inForce, string? status) =>
            inForceOnly && inForce is not null && !string.Equals(status, inForce, StringComparison.Ordinal);

        foreach (var doc in corpus.Docs)
        {
            if (doc.Type is not { } type || !type.Fields.ContainsKey(field)) continue;
            if (doc.FrontScalar("id") is not { Length: > 0 } id) continue;
            if (Lapsed(corpus.Schema.EffectiveField(type, "status")?.InForce, doc.FrontScalar("status"))) continue;

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
            if (Lapsed(type.InForce, JsonRead.Str(fields["status"]))) continue;

            foreach (var target in targets)
                if (JsonRead.Str(target) is { Length: > 0 } value)
                    Add(Exporter.Scoped(value, record.Producer),
                        Exporter.Scoped(id, record.Producer));
        }

        return found;
    }

    /// <summary>
    /// The clause a clause is likely paired with: the same key in another policy. A candidate, which whoever confirms
    /// the report decides.
    /// </summary>
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

    /// <summary>
    /// What the tool can answer, and a blank for everything else. <c>status</c> is <c>draft</c>, and
    /// <c>generated.by</c> names this run. Every blank is a YAML null, because <c>owner: human:</c> does not parse and
    /// would fail <c>frontmatter-parses</c> over the whole document. <c>docs/use/cli/report.md</c> carries the
    /// handover.
    /// </summary>
    private static string Frontmatter(ReportStamp stamp, string report)
    {
        var sb = new StringBuilder();
        sb.Append("id:\n");
        sb.Append("type: report\n");
        sb.Append("tier: descriptive\n");
        sb.Append("status: draft\n");
        sb.Append("owner:\n");
        sb.Append("generated:\n");
        sb.Append($"  at: {stamp.At}\n");
        sb.Append($"  by: {stamp.By}\n");
        sb.Append($"  report: {report}\n");
        sb.Append($"  tool: {stamp.By}\n");
        sb.Append("sources:\n");

        foreach (var source in stamp.Sources)
            sb.Append($"  - {{ resource: {source.Resource}, version: \"{source.Version ?? ""}\" }}\n");

        sb.Append("verified:\n");
        return sb.ToString();
    }

    /// <summary>
    /// What the report can and cannot see, stated in the report. Each report passes its own two paragraphs.
    /// </summary>
    private static void Limits(
        StringBuilder body, LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited,
        string reach, string closing)
    {
        var name = corpus.Descriptor.Name ?? Unnamed;

        body.AppendLine("## Limits");
        body.AppendLine();
        body.AppendLine($"This reads `{name}` and what it imports. {reach}");
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

        body.AppendLine(closing);
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
            body.AppendLine(
                "| Clause | Level | Covered by | Deviations | Controls | Pair candidate | Verdict | Note |");
            body.AppendLine(
                "|--------|-------|------------|------------|----------|----------------|---------|------|");

            foreach (var row in group)
                body.AppendLine($"| `{row.Clause.Key}` | {row.Clause.Level ?? ""} "
                                + $"| {Cite(row.CoveredBy)} | {Cite(row.Deviations)} | {Cite(row.Controls)} "
                                + $"| {(row.Pair is null ? "" : $"`{row.Pair}`")} "
                                + $"| {(row.CoveredBy.Count > 0 ? "covered" : "uncovered")} | |");

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
            body.AppendLine(Filed(group.First()));
            body.AppendLine();
            body.AppendLine("| Reference | Citations | Clauses | Policies | Note |");
            body.AppendLine("|-----------|-----------|---------|----------|------|");

            foreach (var row in group)
                body.AppendLine($"| {row.Reference ?? "the framework entire"} | {row.Clauses.Count} "
                                + $"| {Cite(row.Clauses)} | {Cite(row.Records)} | |");

            body.AppendLine();
        }
    }

    /// <summary>
    /// Where the register files a framework, said again above its own table. The link climbs one folder because a
    /// report sits in <c>reports/</c> and the register is addressed from the corpus root.
    /// </summary>
    private static string Filed(FrameworkRow row)
    {
        if (row.Path is not { } path || row.Anchor is not { } anchor)
            return "The register places this framework nowhere, so it carries no standing.";

        var rel = path.Replace('\\', '/');
        var link = $"[`{rel[(rel.LastIndexOf('/') + 1)..]}`](../{rel}#{anchor})";

        return row.Standing is { Length: > 0 } standing
            ? $"Filed under `{standing}` in {link}."
            : $"{link} holds this framework under no standing.";
    }

    /// <summary>
    /// What the report leaves open, said once at the foot so nobody takes an empty column for an answer.
    /// </summary>
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
