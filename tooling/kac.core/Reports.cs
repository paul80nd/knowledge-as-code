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
    /// <summary>Every line ends with a line feed, although the body is built with <c>AppendLine</c>.</summary>
    public string Text => $"---\n{Frontmatter}---\n\n# {Title}\n\n{Body}".ReplaceLineEndings("\n");
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
    IReadOnlyList<string> Pairs);

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
    /// The field and value marking a control that checks nothing, which the <c>Controls</c> column leaves out. Named
    /// here, because no declaration says which value of a field means unenforced.
    /// </summary>
    private const string Mechanism = "mechanism";
    private const string NotEnforced = "not-enforced";

    /// <summary>The field a type's <c>in-force:</c> value is declared on and a record's state is read from.</summary>
    private const string Status = "status";

    /// <summary>
    /// The column a clause states its framework mapping in. <c>AlignmentRollup</c> reads the same one.
    /// </summary>
    private const string AlignmentColumn = "Alignment";

    /// <summary>
    /// The type a report record is, which <c>Frontmatter</c> writes and whose folder a local link climbs out of.
    /// </summary>
    private const string ReportType = "report";

    /// <summary>The keys a clause line states its wording and its level under.</summary>
    private const string ClauseKey = "clause";
    private const string LevelKey = Exporter.LevelKey;

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

        foreach (var source in inherited)
        foreach (var type in source.Types.Where(CarriesClauses))
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
                JsonRead.Str(json[LevelKey]), JsonRead.Str(json[ClauseKey]) ?? "", null, [],
                source.Shortcode));
        }

        return found;
    }

    /// <summary>
    /// Whether an imported type's parts are clauses, read from the keys its producer wrote on each line. A consumer
    /// declining the type has no schema for it, and a type renamed on either side keeps the keys.
    /// <c>policy-lookup</c> reads the same two keys.
    /// </summary>
    private static bool CarriesClauses(InheritedType type) =>
        type.PartLines.Any(line => JsonRead.Parse(line) is { } json
                                   && json.ContainsKey(ClauseKey) && json.ContainsKey(LevelKey));

    /// <summary>
    /// The type whose parts are clauses in this corpus's own records, found through what it declares, so a corpus that
    /// renamed it still reports.
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
        var rules = RuleCovers(corpus, inherited);
        var departures = Edges(corpus, inherited, DepartsFrom, inForceOnly: true);
        var controls = Edges(corpus, inherited, Verifies, inForceOnly: true, leaveOut: (Mechanism, NotEnforced));
        var pairs = Pairs(clauses);
        var pages = Pages.For(corpus, inherited);

        var rows = clauses.Select(c =>
        {
            var coveredBy = CoveredBy(c.Id, covers, rules);
            return new CoverageRow(c, coveredBy,
                departures.GetValueOrDefault(c.Id) ?? [],
                [
                    .. coveredBy.SelectMany(by => ControlsFor(by, controls))
                        .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                ],
                pairs.GetValueOrDefault(c.Id) ?? []);
        }).ToList();

        const string listed = " The controls index of each corpus lists every control it wrote.";

        var body = new StringBuilder();
        Limits(body, corpus, inherited, pages,
            "A clause uncovered here may well be covered in a corpus consuming this one, and every "
            + "consumer answers for its own coverage.",
            "No column here says a clause is verified. `Controls` lists each control naming a rule in "
            + "`Covered by`, and a control may check one obligation of that rule and leave the rest."
            + InForce(corpus, inherited, Verifies) switch
            {
                [] => "",
                [var inForce] => $" The `Controls` column lists a control only while its `status` is `{inForce}` "
                                 + $"and its `{Mechanism}` is not `{NotEnforced}`.{listed}",
                _ => " The `Controls` column lists a control only while its `status` is the value its type declares "
                     + $"in `in-force:` and its `{Mechanism}` is not `{NotEnforced}`.{listed}"
            }
            + InForce(corpus, inherited, DepartsFrom) switch
            {
                [] => "",
                [var inForce] => $" The `Deviations` column lists a deviation only while its `status` is `{inForce}`.",
                _ => " The `Deviations` column lists a deviation only while its `status` is the value its type "
                     + "declares in `in-force:`."
            });
        CoverageTotals(body, rows, pages);
        CoverageSections(body, rows, pages);
        Judgement(body, "The tool prints two verdicts and a person writes the rest.",
            "It prints `covered` and `uncovered`, and splitting `uncovered` into a gap and something out "
            + "of scope is a judgement about this estate. That judgement, and the `Note` beside it, "
            + "belong to whoever confirms this report.");
        pages.Define(body);

        return new ReportPlan(Coverage, "Clause coverage", Frontmatter(stamp, Coverage),
            body.ToString().TrimEnd() + "\n");
    }

    private static ReportPlan FrameworksPlan(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, ReportStamp stamp)
    {
        var rows = FrameworkRows(corpus, inherited);
        var pages = Pages.For(corpus, inherited);

        var body = new StringBuilder();
        Limits(body, corpus, inherited, pages,
            "An `Alignment` cell stays in the corpus that wrote it, so the citations counted below are "
            + "the ones written here. A reference an imported policy cites belongs to that producer's "
            + "own report.",
            "No column here says a clause meets the reference it cites. A citation records that the "
            + "clause named it. Whoever confirms this report decides whether the clause covers that "
            + "control or one corner of it.");
        FrameworkTotals(body, rows);
        FrameworkSections(body, rows, pages);
        Judgement(body, "A reference on one clause is one citation from losing its coverage.",
            "The tool counts the citations it can see. Whether a control still has honest coverage, and "
            + "whether an uncited reference should be removed from the register, belongs to whoever "
            + "confirms this report.");
        pages.Define(body);

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
    /// Every distinct <c>in-force:</c> value the coverage filter applies to a type declaring the given field: this
    /// corpus's own schema, and each inherited type of that name as its producer's manifest states it.
    /// </summary>
    private static List<string> InForce(LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, string field)
    {
        var types = corpus.Schema.ByFolder.Values.Where(t => t.Fields.ContainsKey(field)).ToList();
        var keys = types.Select(t => t.Key).ToHashSet(StringComparer.Ordinal);

        return
        [
            .. types.Select(t => corpus.Schema.EffectiveField(t, Status)?.InForce)
                .Concat(inherited.SelectMany(s => s.Types).Where(t => keys.Contains(t.Type)).Select(t => t.InForce))
                .OfType<string>().Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
        ];
    }

    /// <summary>
    /// Every id a field of the given name names, keyed by the id it points at, across every adopted type. Inherited
    /// records count, and both halves of an inherited edge are scoped to the corpus that wrote the record, so the join
    /// meets on both sides. <paramref name="inForceOnly"/> drops a record whose <c>status</c> is not the one its type
    /// names in <c>in-force:</c>, and keeps every record of a type that names none. <paramref name="leaveOut"/> drops a
    /// record whose field of that name states that value.
    /// </summary>
    private static Dictionary<string, List<string>> Edges(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, string field, bool inForceOnly = false,
        (string Field, string Value)? leaveOut = null)
    {
        var found = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void Add(string target, string id) =>
            (found.TryGetValue(target, out var list) ? list : found[target] = []).Add(id);

        bool Lapsed(string? inForce, string? status) =>
            inForceOnly && inForce is not null && !string.Equals(status, inForce, StringComparison.Ordinal);

        bool LeftOut(Func<string, string?> read) =>
            leaveOut is var (name, value) && string.Equals(read(name), value, StringComparison.Ordinal);

        foreach (var doc in corpus.Docs)
        {
            if (doc.Type is not { } type || !type.Fields.ContainsKey(field)) continue;
            if (doc.FrontScalar("id") is not { Length: > 0 } id) continue;
            if (Lapsed(corpus.Schema.EffectiveField(type, Status)?.InForce, doc.FrontScalar(Status))) continue;
            if (LeftOut(doc.FrontScalar)) continue;

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
            if (Lapsed(type.InForce, JsonRead.Str(fields[Status]))) continue;
            if (LeftOut(name => JsonRead.Str(fields[name]))) continue;

            foreach (var target in targets)
                if (JsonRead.Str(target) is { Length: > 0 } value)
                    Add(Exporter.Scoped(value, record.Producer),
                        Exporter.Scoped(id, record.Producer));
        }

        return found;
    }

    /// <summary>
    /// Each standard covering the clause, named by the rules whose <c>Covers</c> line cites it. A standard none of
    /// whose rules cites it is named whole, which is how a record exported without its rules is read.
    /// </summary>
    private static List<string> CoveredBy(
        string clause, Dictionary<string, List<string>> covers, Dictionary<string, List<string>> rules)
    {
        var named = rules.GetValueOrDefault(clause) ?? [];

        return
        [
            .. (covers.GetValueOrDefault(clause) ?? []).SelectMany(standard =>
                named.Where(r => string.Equals(Citation.Read(r).Whole, standard, StringComparison.Ordinal))
                    .Order(StringComparer.Ordinal)
                    .DefaultIfEmpty(standard))
        ];
    }

    /// <summary>
    /// The controls naming this rule, and those naming its standard whole, as an export from before rule ids may. A
    /// standard named whole folds every control naming one of its rules into it.
    /// </summary>
    private static IEnumerable<string> ControlsFor(string coveredBy, Dictionary<string, List<string>> controls)
    {
        var cited = Citation.Read(coveredBy);

        return controls
            .Where(e => string.Equals(e.Key, coveredBy, StringComparison.Ordinal)
                        || string.Equals(e.Key, cited.Whole, StringComparison.Ordinal)
                        || (cited.Part is null
                            && string.Equals(Citation.Read(e.Key).Whole, cited.Whole, StringComparison.Ordinal)))
            .SelectMany(e => e.Value);
    }

    /// <summary>
    /// Every rule a <c>Covers</c> line ties to a clause, keyed by the clause. The label is the one the field
    /// <c>implements</c> mirrors, and an inherited rule carries it under the key this schema's export line names.
    /// </summary>
    private static Dictionary<string, List<string>> RuleCovers(
        LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited)
    {
        var found = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void Add(string clause, string rule) =>
            (found.TryGetValue(clause, out var list) ? list : found[clause] = []).Add(rule);

        foreach (var doc in corpus.Docs)
        {
            if (Label(doc.Type) is not { } label) continue;
            if (doc.FrontScalar("id") is not { Length: > 0 } record) continue;

            foreach (var row in doc.Parts)
                if (row.Id is { Length: > 0 } part)
                    foreach (var clause in Exporter.Citations(doc, row, [label]).GetValueOrDefault(label, []))
                        Add(clause, $"{record}.{part}");
        }

        foreach (var source in inherited)
        foreach (var type in source.Types)
        {
            if (corpus.Schema.ByFolder.GetValueOrDefault(type.Type) is not { } local) continue;
            if (Label(local) is not { } label || local.Export is not { } export) continue;

            var key = export.Line.FirstOrDefault(l => string.Equals(l.Source,
                PartLineSource.CitationPrefix + label, StringComparison.Ordinal)).Key;
            if (key is null) continue;

            foreach (var line in type.PartLines)
            {
                if (JsonRead.Parse(line) is not { } json) continue;
                if (JsonRead.Str(json[type.IdKey ?? "id"]) is not { } id) continue;
                if (json[key] is not JsonArray clauses) continue;

                // A line further up the chain names its own corpus, and its citations were written there.
                var scope = JsonRead.Str(json[Exporter.ShortcodeKey]) ?? source.Shortcode;
                foreach (var clause in clauses)
                    if (JsonRead.Str(clause) is { Length: > 0 } value)
                        Add(Exporter.Scoped(value, scope), Exporter.Scoped(id, source.Shortcode));
            }
        }

        return found;
    }

    /// <summary>The <c>Covers</c> label a type's <c>implements</c> mirrors, or null where it mirrors none.</summary>
    private static string? Label(TypeSchema? type) =>
        type?.Fields.GetValueOrDefault(Implements)?.MirrorsCitations;

    /// <summary>
    /// The clause a clause is likely paired with: the same key in another policy. A candidate, which whoever confirms
    /// the report decides.
    /// </summary>
    private static Dictionary<string, List<string>> Pairs(IReadOnlyList<ClauseFact> clauses)
    {
        var byKey = clauses.GroupBy(c => c.Key, StringComparer.Ordinal).Where(g => g.Count() > 1);
        var found = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var group in byKey)
        foreach (var clause in group)
            found[clause.Id] =
                [.. group.Where(o => o.Id != clause.Id).Select(o => o.Id).Order(StringComparer.Ordinal)];

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
        sb.Append($"type: {ReportType}\n");
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
        StringBuilder body, LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited, Pages pages,
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
            // The version stays in `sources` alone, so the body cannot disagree with the frontmatter.
            foreach (var source in inherited)
                body.AppendLine($"* `{source.Shortcode}`, {source.Corpus ?? "unnamed"}.");
            body.AppendLine();

            var unlinked = pages.Unlinked.Select(s => $"`{s}`").ToList();
            if (unlinked.Count > 0)
            {
                var names = unlinked.Count == 1
                    ? unlinked[0]
                    : $"{string.Join(", ", unlinked[..^1])} or {unlinked[^1]}";
                body.AppendLine($"Nothing imported from {names} states the page it was published at, so "
                                + $"{(unlinked.Count == 1 ? "its" : "their")} ids are not linked.");
                body.AppendLine();
            }
        }

        body.AppendLine(closing);
        body.AppendLine();
    }

    private static void CoverageTotals(StringBuilder body, IReadOnlyList<CoverageRow> rows, Pages pages)
    {
        body.AppendLine("## Totals");
        body.AppendLine();
        body.AppendLine("| Policy | Clauses | Covered | Uncovered |");
        body.AppendLine("|--------|---------|---------|-----------|");

        foreach (var group in rows.GroupBy(r => r.Clause.Record, StringComparer.Ordinal)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
            body.AppendLine($"| {pages.Cite(group.Key)} | {group.Count()} "
                            + $"| {group.Count(r => r.CoveredBy.Count > 0)} "
                            + $"| {group.Count(r => r.CoveredBy.Count == 0)} |");

        body.AppendLine($"| **Total** | **{rows.Count}** | **{rows.Count(r => r.CoveredBy.Count > 0)}** "
                        + $"| **{rows.Count(r => r.CoveredBy.Count == 0)}** |");
        body.AppendLine();
    }

    private static void CoverageSections(StringBuilder body, IReadOnlyList<CoverageRow> rows, Pages pages)
    {
        body.AppendLine("## Clauses");
        body.AppendLine();

        foreach (var group in rows.GroupBy(r => r.Clause.Record, StringComparer.Ordinal)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            body.AppendLine($"### {pages.Heading(group.Key)}");
            body.AppendLine();
            body.AppendLine(
                "| Clause | Level | Covered by | Deviations | Controls | Pair candidate | Verdict | Note |");
            body.AppendLine(
                "|--------|-------|------------|------------|----------|----------------|---------|------|");

            foreach (var row in group)
                body.AppendLine($"| `{row.Clause.Key}` | {row.Clause.Level ?? ""} "
                                + $"| {pages.Cite(row.CoveredBy)} | {pages.Cite(row.Deviations)} "
                                + $"| {pages.Cite(row.Controls)} | {pages.Cite(row.Pairs)} "
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

    private static void FrameworkSections(StringBuilder body, IReadOnlyList<FrameworkRow> rows, Pages pages)
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
                                + $"| {pages.Cite(row.Clauses)} | {pages.Cite(row.Records)} | |");

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

    /// <summary>
    /// The page of every record a report can cite, keyed by the id the report cites it by, and the ones it has cited
    /// so far. A record with no page is cited as bare code. <c>docs/use/cli/report.md</c> says where each link leads.
    /// <c>Unlinked</c> lists each corpus that wrote inherited records and stated a page for none of them.
    /// </summary>
    private sealed class Pages(Dictionary<string, string> addresses, IReadOnlyList<string> unlinked)
    {
        private readonly SortedDictionary<string, string> cited = new(StringComparer.Ordinal);

        public IReadOnlyList<string> Unlinked => unlinked;

        /// <summary>
        /// A local page is relative to the folder a report record is filed in. A schema declaring no report type
        /// names no such folder, so this corpus's own records are left unlinked.
        /// </summary>
        public static Pages For(LoadedCorpus corpus, IReadOnlyList<InheritedCorpus> inherited)
        {
            var found = new Dictionary<string, string>(StringComparer.Ordinal);

            if (corpus.Schema.ByFolder.Values.FirstOrDefault(t => t.TypeName == ReportType) is
                { Folder.Length: > 0 } report)
            {
                var up = string.Concat(report.Folder.Split('/').Select(_ => "../"));

                foreach (var doc in corpus.Docs)
                    if (doc.FrontScalar("id") is { Length: > 0 } id)
                        found.TryAdd(id, up + doc.Rel.Replace('\\', '/'));
            }

            // Keyed by the corpus that wrote each record: a chain can pass on a producer that publishes nowhere inside
            // an import that publishes its own pages.
            var producers = new List<(string Producer, bool Linked)>();

            foreach (var source in inherited)
            foreach (var type in source.Types)
            foreach (var record in type.Records)
            {
                var (id, page) = Published(record);
                producers.Add((record.Producer, page is not null));
                if (id is not null && page is not null) found.TryAdd(Exporter.Scoped(id, record.Producer), page);
            }

            return new Pages(found,
            [
                .. producers.GroupBy(p => p.Producer, StringComparer.Ordinal)
                    .Where(g => !g.Any(p => p.Linked)).Select(g => g.Key)
            ]);
        }

        /// <summary>The link covers the record alone, and a cited part follows it as code.</summary>
        public string Cite(string id)
        {
            var citation = Citation.Read(id);
            if (!TryCite(citation.Whole)) return $"`{id}`";

            var link = $"[`{citation.Whole}`][{citation.Whole}]";
            return citation.Part is null ? link : $"{link}`.{citation.Part}`";
        }

        public string Cite(IReadOnlyList<string> ids) => string.Join(", ", ids.Select(Cite));

        public string Heading(string record) => TryCite(record) ? $"[{record}][{record}]" : record;

        /// <summary>One definition per record the report cited, written at its foot.</summary>
        public void Define(StringBuilder body)
        {
            foreach (var (label, page) in cited) body.AppendLine($"[{label}]: {page}");
        }

        /// <summary>False where the record has no page. True records it for <c>Define</c>.</summary>
        private bool TryCite(string record)
        {
            if (!addresses.TryGetValue(record, out var page)) return false;
            cited[record] = page;
            return true;
        }

        /// <summary>
        /// The id an exported record states and the page its producer published it at. Either may be null.
        /// </summary>
        private static (string? Id, string? Page) Published(InheritedRecord record)
        {
            var json = JsonRead.Parse(record.Content);
            return (JsonRead.Str(JsonRead.Object(json?["fields"])?["id"]),
                JsonRead.Str(JsonRead.Object(json?["links"])?["human"]));
        }
    }
}
