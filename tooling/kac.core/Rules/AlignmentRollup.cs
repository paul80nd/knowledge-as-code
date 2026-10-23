using YamlDotNet.RepresentationModel;

namespace kac.core;

public sealed class AlignmentRollup : ICorpusRule
{
    public RuleId RuleId => new("alignment-rollup");

    private static readonly CheckId Reports = new("alignment-rollup");
    private static readonly CheckId Unstated = new("framework-posture");
    private static readonly CheckId Unreferenced = new("framework-uncited");
    private static readonly CheckId Unlinked = new("alignment-unlinked");

    public IReadOnlyList<CheckId> Emits => [Reports, Unstated, Unreferenced, Unlinked];

    private const string Field = "aligns-with";

    private const string Postures = "postures";

    public IReadOnlyList<string> Reads => [Postures];

    private static List<string> Binding(RuleSpec spec) => Yaml.StrList(spec.Config.GetValueOrDefault(Postures));

    /// <summary>
    /// A posture no register page heads a standing with. Judged only against the pages the records link to, so a
    /// corpus citing no framework is told nothing.
    /// </summary>
    public IEnumerable<string> Misconfigured(CorpusRuleContext ctx)
    {
        if (Column(ctx.Type) is not { } column) yield break;

        var binding = Binding(ctx.Spec);
        var register = new Register(ctx.Tree, binding);
        var headed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var doc in ctx.Records)
        foreach (var row in doc.Parts)
        foreach (var label in row.CellLinks?.GetValueOrDefault(column) ?? [])
            if (register.Reaches(doc, label) is var (page, _))
                headed.UnionWith(register.Standings(page));

        if (headed.Count == 0) yield break;

        foreach (var posture in binding.Where(p => !headed.Contains(p)))
            yield return $"'{Postures}' lists '{posture}', and no register page the records cite has a "
                         + $"'## {posture}' heading. Spell it as the register heads it, or add the heading.";
    }

    private static string? Column(TypeSchema type) =>
        type.Parts?.Columns.FirstOrDefault(c => string.Equals(c, "Alignment", StringComparison.OrdinalIgnoreCase));

    public void Check(CorpusRuleContext ctx)
    {
        if (Column(ctx.Type) is not { } column) return;

        var register = new Register(ctx.Tree, Binding(ctx.Spec));
        var reached = new Dictionary<string, (Doc First, HashSet<string> Cited)>(StringComparer.Ordinal);

        foreach (var doc in ctx.Records)
        {
            var cited = FromClauses(doc, column, register, ctx, reached, out var unread);
            var declared = FromFrontmatter(doc, out var fieldLine);

            if (cited.Count == 0 && declared.Count == 0) continue;

            foreach (var (reference, line) in Missing(cited, declared))
                ctx.Err(doc, Reports,
                    $"'{reference}' is cited in the clause table and missing from '{Field}'.", line);

            foreach (var (reference, _) in Missing(declared, cited))
                if (!Unread(reference, unread))
                    ctx.Err(doc, Reports,
                        $"'{Field}' claims '{reference}', and no clause cites it.", fieldLine);
        }

        Uncited(register, reached, ctx);
    }

    private static void Uncited(Register register,
        Dictionary<string, (Doc First, HashSet<string> Cited)> reached, CorpusRuleContext ctx)
    {
        foreach (var (page, (doc, cited)) in reached.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        foreach (var (anchor, entry) in register.Entries(page))
            if (!cited.Contains(anchor))
                ctx.Err(doc, Unreferenced,
                    $"'{entry.Heading}' is filed on '{page}' and no clause cites it.", null);
    }

    private static bool Unread(string reference, IReadOnlySet<string> labels) =>
        labels.Any(label => reference.Length == label.Length
            ? string.Equals(reference, label, StringComparison.Ordinal)
            : reference.Length > label.Length
              && reference[label.Length] == '.'
              && reference.AsSpan(0, label.Length).SequenceEqual(label));

    private static IEnumerable<(string Reference, int? Line)> Missing(
        IReadOnlyDictionary<string, int?> side, IReadOnlyDictionary<string, int?> other) =>
        side.Where(pair => !other.ContainsKey(pair.Key)).Select(pair => (pair.Key, pair.Value));

    private static Dictionary<string, int?> FromClauses(Doc doc, string column, Register register,
        CorpusRuleContext ctx, Dictionary<string, (Doc First, HashSet<string> Cited)> reached,
        out IReadOnlySet<string> unread)
    {
        var found = new Dictionary<string, int?>(StringComparer.Ordinal);
        var said = new HashSet<string>(StringComparer.Ordinal);
        var undefined = new HashSet<string>(StringComparer.Ordinal);
        unread = undefined;

        foreach (var row in doc.Parts)
        {
            if (row.Cells?.GetValueOrDefault(column) is not { Length: > 0 } cell) continue;

            var labels = row.CellLinks?.GetValueOrDefault(column) ?? [];

            foreach (var label in Alignment.UndefinedLabels(cell))
                if (undefined.Add(label))
                    ctx.Err(doc, Unlinked,
                        $"'[{label}]' is cited here and has no link definition, so the cell states no "
                        + "mapping. Define the label against the register entry.", row.Line);

            foreach (var (framework, reference) in Alignment.References(cell, labels))
            {
                if (register.Reaches(doc, framework) is var (page, anchor))
                {
                    if (!reached.TryGetValue(page, out var seen))
                        reached[page] = seen = (doc, new HashSet<string>(StringComparer.Ordinal));
                    seen.Cited.Add(anchor);
                }

                switch (register.Standing(doc, framework))
                {
                    case Posture.Binding:
                        found.TryAdd(Alignment.Join(framework, reference), row.Line);
                        break;

                    case Posture.Unstated when said.Add(framework):
                        ctx.Err(doc, Unstated,
                            $"'{framework}' is cited here and nothing says what our standing against it "
                            + $"is. {register.Wanted}", row.Line);
                        break;
                }
            }
        }

        return found;
    }

    private static Dictionary<string, int?> FromFrontmatter(Doc doc, out int? fieldLine)
    {
        var found = new Dictionary<string, int?>(StringComparer.Ordinal);
        fieldLine = doc.FrontStartLine;

        if (doc.Front is null) return found;

        foreach (var kv in doc.Front.Children)
        {
            if (((YamlScalarNode)kv.Key).Value != Field) continue;
            // The key's line. A block sequence's value starts on its first entry.
            fieldLine = Yaml.LineOf(kv.Key, doc.FrontStartLine);
            if (kv.Value is not YamlSequenceNode seq) continue;

            foreach (var item in seq.Children)
            {
                if (item is not YamlMappingNode map) continue;
                if (Yaml.Get(map, "framework") is not YamlScalarNode { Value: { Length: > 0 } framework })
                    continue;

                var line = Yaml.LineOf(item, doc.FrontStartLine);
                var clauses = Yaml.Get(map, "clauses") as YamlSequenceNode;

                if (clauses is null || clauses.Children.Count == 0)
                {
                    found.TryAdd(framework, line);
                    continue;
                }

                foreach (var clause in clauses.Children.OfType<YamlScalarNode>())
                    if (clause.Value is { Length: > 0 } reference)
                        found.TryAdd(Alignment.Join(framework, reference), line);
            }
        }

        return found;
    }
}

internal sealed record RegisterEntry(string Heading, string Standing);

public enum Posture
{
    Binding,

    Loose,

    Unstated
}

internal sealed class Register(Tree tree, IReadOnlyList<string> postures)
{
    private readonly Dictionary<string, Dictionary<string, RegisterEntry>> _byPage =
        new(StringComparer.Ordinal);

    public string Wanted => postures.Count == 0
        ? "The rule names no standings, so nothing it cites can bind."
        : $"File it under {Join(postures)}, or under a standing that binds nothing.";

    /// <summary>
    /// The level-two heading the label's register entry sits under. Null where the label links no entry, or the entry
    /// sits under no level-two heading.
    /// </summary>
    public string? Filed(Doc doc, string label)
    {
        if (Anchor(doc, label) is not ({ } page, { } anchor)) return null;
        return Placed(page).GetValueOrDefault(anchor)?.Standing;
    }

    /// <summary>
    /// Null where no link with this label has a <c>#</c> fragment and resolves to a page. A pair is returned whether or
    /// not an entry is filed at the anchor.
    /// </summary>
    public (string Page, string Anchor)? Reaches(Doc doc, string label) =>
        Anchor(doc, label) is ({ } page, { } anchor) ? (page, anchor) : null;

    public IEnumerable<KeyValuePair<string, RegisterEntry>> Entries(string page) =>
        Placed(page).OrderBy(kv => kv.Key, StringComparer.Ordinal);

    /// <summary>Every level-two heading on the page, whether or not an entry is filed under it.</summary>
    public IEnumerable<string> Standings(string page) =>
        Md.Headings(tree.Read(page)).Where(h => h.Level == 2).Select(h => h.Text);

    public Posture Standing(Doc doc, string label) => Filed(doc, label) switch
    {
        null => Posture.Unstated,
        var standing when postures.Contains(standing, StringComparer.OrdinalIgnoreCase) => Posture.Binding,
        _ => Posture.Loose
    };

    private (string? Page, string? Anchor) Anchor(Doc doc, string label)
    {
        foreach (var link in doc.Links)
        {
            if (!string.Equals(link.Label, label, StringComparison.OrdinalIgnoreCase)) continue;

            var hash = link.Target.IndexOf('#');
            if (hash < 0 || hash == link.Target.Length - 1) continue;

            if (LinkChecks.Resolve(tree, doc.Rel, link.Target) is { } page)
                return (page, link.Target[(hash + 1)..]);
        }

        return (null, null);
    }

    private Dictionary<string, RegisterEntry> Placed(string page)
    {
        if (_byPage.TryGetValue(page, out var known)) return known;

        var placed = new Dictionary<string, RegisterEntry>(StringComparer.Ordinal);
        var standing = "";

        foreach (var (level, text) in Md.Headings(tree.Read(page)))
            if (level <= 2) standing = level == 2 ? text : "";
            else if (standing.Length > 0 && Md.Slug(text) is { Length: > 0 } slug)
                placed.TryAdd(slug, new RegisterEntry(text, standing));

        _byPage[page] = placed;
        return placed;
    }

    private static string Join(IReadOnlyList<string> words) => words.Count == 1
        ? $"'{words[0]}'"
        : string.Join(", ", words.Take(words.Count - 1).Select(w => $"'{w}'")) + $" or '{words[^1]}'";
}

public static class Alignment
{
    public static string Join(string framework, string? reference) =>
        reference is { Length: > 0 } ? $"{framework}.{reference}" : framework;

    /// <summary>
    /// Every bracketed label the flattened cell still shows, in order. A resolved reference flattens to its display
    /// text and loses its brackets, so a surviving bracket has no definition behind it.
    /// </summary>
    public static IEnumerable<string> UndefinedLabels(string cell)
    {
        var at = 0;

        while ((at = cell.IndexOf('[', at)) >= 0)
        {
            var close = cell.IndexOf(']', at + 1);
            if (close < 0) yield break;

            if (cell[(at + 1)..close].Trim() is { Length: > 0 } label) yield return label;
            at = close + 1;
        }
    }

    /// <summary>
    /// One pair per label, in the order given. A null reference means the cell cites the framework as a whole, or does
    /// not show the label after the previous one.
    /// </summary>
    public static IEnumerable<(string Framework, string? Reference)> References(
        string cell, IReadOnlyList<string> labels)
    {
        var from = 0;

        foreach (var label in labels)
        {
            var at = cell.IndexOf(label, from, StringComparison.Ordinal);
            if (at < 0)
            {
                yield return (label, null);
                continue;
            }

            var i = at + label.Length;
            from = i;

            if (i >= cell.Length || cell[i] != '.')
            {
                yield return (label, null);
                continue;
            }

            var start = ++i;
            while (i < cell.Length && cell[i] != ',' && !char.IsWhiteSpace(cell[i])) i++;
            from = i;

            var reference = cell[start..i];
            yield return (label, reference.Length > 0 ? reference : null);
        }
    }
}
