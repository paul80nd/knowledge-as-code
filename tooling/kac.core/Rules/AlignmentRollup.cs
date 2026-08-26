using YamlDotNet.RepresentationModel;

namespace kac.core;

// A policy states a framework mapping twice: once in the `Alignment` cell of the clause it qualifies,
// and once in the `aligns-with` roll-up the generated index is built from. The cells are the statement
// and the roll-up is a summary of them, so the two agreeing is what makes the index worth reading.
//
// The summary is narrower than the statement, and that is the point of it. A clause may cite a
// framework we took ideas from, for provenance. The roll-up carries the frameworks that bind us, so an
// index read as a coverage table answers what we are on the hook for. Which standings bind is the
// corpus's judgement, declared as `postures:` on the rule.
//
// Both directions, because either half going stale is the same defect wearing a different face. A
// binding reference in a cell and not in the roll-up leaves the index under-reporting coverage. A
// reference in the roll-up that no clause cites is a claim of coverage no clause can show, and that one
// reads as evidence to whoever is looking for it.
//
// A corpus rule rather than a document rule, because the standing is not in the policy. It is on the
// register the policy's own links point at, which carries no frontmatter and is therefore no record.
public sealed class AlignmentRollup : ICorpusRule
{
    public RuleId RuleId => new("alignment-rollup");

    private static readonly CheckId Reports = new("alignment-rollup");
    private static readonly CheckId Unstated = new("framework-posture");

    public IReadOnlyList<CheckId> Emits => [Reports, Unstated];

    // The frontmatter key holding the roll-up. Named here rather than read from the type, because no
    // declaration says which field summarises a column: the type declares the column and the field
    // separately, and only this rule knows they are two accounts of one thing.
    private const string Field = "aligns-with";

    public void Check(CorpusRuleContext ctx)
    {
        if (ctx.Type.Parts is not { } parts) return;

        var column = parts.Columns.FirstOrDefault(c =>
            string.Equals(c, "Alignment", StringComparison.OrdinalIgnoreCase));
        if (column is null) return;

        var register = new Register(ctx.Tree, ctx.Spec.Postures);

        foreach (var doc in ctx.Records)
        {
            var cited = FromClauses(doc, column, register, ctx);
            var declared = FromFrontmatter(doc, out var fieldLine);

            // A policy citing nothing that binds and claiming nothing is the ordinary case for a
            // governance policy, and neither half of the report below has anything to say about it.
            if (cited.Count == 0 && declared.Count == 0) continue;

            foreach (var (reference, line) in Missing(cited, declared))
                ctx.Err(doc, Reports,
                    $"'{reference}' is cited in the clause table and missing from '{Field}'.", line);

            foreach (var (reference, _) in Missing(declared, cited))
                ctx.Err(doc, Reports,
                    $"'{Field}' claims '{reference}', and no clause cites it.", fieldLine);
        }
    }

    // What one side holds that the other does not, in the order the first side reads. Comparison is
    // ordinal: a framework label and a reference into it are both written to be quoted, so a difference
    // of case is a difference worth reporting rather than one to absorb.
    private static IEnumerable<(string Reference, int? Line)> Missing(
        IReadOnlyDictionary<string, int?> side, IReadOnlyDictionary<string, int?> other) =>
        side.Where(pair => !other.ContainsKey(pair.Key)).Select(pair => (pair.Key, pair.Value));

    // Every binding reference the clause table cites, as `framework` or `framework.reference`, keyed so
    // that a reference cited by six clauses is one fact rather than six. The line kept is the first
    // clause citing it, which is where an author reading down the table meets it.
    //
    // A cell is read from the row's own cells rather than from the rendered text, so a column that
    // moves stays readable and a clause mentioning a framework in its wording is not mistaken for a
    // mapping.
    private static Dictionary<string, int?> FromClauses(Doc doc, string column, Register register,
        CorpusRuleContext ctx)
    {
        var found = new Dictionary<string, int?>(StringComparer.Ordinal);
        var said = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in doc.Parts)
        {
            if (row.Cells?.GetValueOrDefault(column) is not { Length: > 0 } cell) continue;

            var labels = row.CellLinks?.GetValueOrDefault(column) ?? [];

            foreach (var (framework, reference) in Alignment.References(cell, labels))
                switch (register.Standing(doc, framework))
                {
                    case Posture.Binding:
                        found.TryAdd(Alignment.Join(framework, reference), row.Line);
                        break;

                    // A framework the register does not place is neither carried nor waved through. The
                    // author is told once per framework rather than once per clause citing it: the fix
                    // is one entry on the register, and a policy citing it six times has one fault.
                    case Posture.Unstated when said.Add(framework):
                        ctx.Err(doc, Unstated,
                            $"'{framework}' is cited here and nothing says what our standing against it "
                            + $"is. {register.Wanted}", row.Line);
                        break;
                }
        }

        return found;
    }

    // The roll-up, flattened to the same vocabulary the cells are read into. A framework carrying no
    // `clauses:` flattens to the framework alone, which is what a cell citing the standard entire
    // writes.
    //
    // Every entry is read, whatever the register says of it. A roll-up naming a framework that binds
    // nothing is caught by the second direction above, which reports that no clause cites it.
    //
    // A malformed entry is skipped rather than reported. What shape the field takes is the schema's to
    // hold, and `entry-key` has already said so against the entry itself.
    private static Dictionary<string, int?> FromFrontmatter(Doc doc, out int? fieldLine)
    {
        var found = new Dictionary<string, int?>(StringComparer.Ordinal);
        fieldLine = doc.FrontStartLine;

        if (doc.Front is null) return found;

        foreach (var kv in doc.Front.Children)
        {
            if (((YamlScalarNode)kv.Key).Value != Field) continue;
            fieldLine = Yaml.LineOf(kv.Value, doc.FrontStartLine);
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

// What a corpus says about a framework one of its clauses cites.
public enum Posture
{
    // Filed under a standing the rule was told obliges a roll-up.
    Binding,

    // Filed under some other standing. A clause may cite it, and the roll-up leaves it behind.
    Loose,

    // Not placed at all, or on no page this corpus holds.
    Unstated
}

// Where a corpus states what its standing against a framework is, read through the links that reach it.
//
// The register is a page rather than a record, so nothing in the graph names it and the rule has no id
// to look it up by. What it does have is the link the clause cell already carries: the label is defined
// as a target with a fragment, which names the page and the heading on it together. So the rule follows
// the link rather than assuming a filename, and a corpus keeping its register anywhere is read with
// nothing configured.
//
// A standing is the `##` a framework's own heading sits under. That grouping is the register as a
// person reads it, so reading it here keeps one account of the fact rather than asking the page to
// carry a second one beside the first.
internal sealed class Register(Tree tree, IReadOnlyList<string> postures)
{
    private readonly Dictionary<string, Dictionary<string, string>> _byPage = new(StringComparer.Ordinal);

    // What to tell an author whose framework the register does not place, naming the standings that
    // would carry it into the roll-up. One string, because every such finding ends the same way.
    public string Wanted => postures.Count == 0
        ? "The rule names no standings, so nothing it cites can bind."
        : $"File it under {Join(postures)}, or under a standing that binds nothing.";

    public Posture Standing(Doc doc, string label)
    {
        if (Anchor(doc, label) is not ({ } page, { } anchor)) return Posture.Unstated;
        if (!Placed(page).TryGetValue(anchor, out var standing)) return Posture.Unstated;

        return postures.Contains(standing, StringComparer.OrdinalIgnoreCase)
            ? Posture.Binding
            : Posture.Loose;
    }

    // The page and heading a label points at, or nothing where the label reaches no fragment of a page
    // the corpus holds. A framework named with no link behind it cannot be placed, which is `Unstated`.
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

    // Every framework heading on a register page, against the standing it sits under. Read once per
    // page: a corpus has one register and every policy asks the same question of it.
    private Dictionary<string, string> Placed(string page)
    {
        if (_byPage.TryGetValue(page, out var known)) return known;

        var placed = new Dictionary<string, string>(StringComparer.Ordinal);
        var standing = "";

        foreach (var (level, text) in Md.Headings(tree.Read(page)))
            if (level <= 2) standing = level == 2 ? text : "";
            else if (standing.Length > 0 && Md.Slug(text) is { Length: > 0 } slug)
                placed.TryAdd(slug, standing);

        _byPage[page] = placed;
        return placed;
    }

    private static string Join(IReadOnlyList<string> words) => words.Count == 1
        ? $"'{words[0]}'"
        : string.Join(", ", words.Take(words.Count - 1).Select(w => $"'{w}'")) + $" or '{words[^1]}'";
}

// How an `Alignment` cell writes a mapping, in the one place both sides of the rule read it from.
//
// A cell holds reference links into the corpus's framework register, each optionally followed by a
// reference within the framework: `[ISO 27001:2022].A.8.24, [NIST SSDF 1.1].PO.5`. The label is the
// framework and the dot is the same addressing a citation uses to reach inside a document.
public static class Alignment
{
    // The two halves of a mapping, joined the way a message and a comparison both need it. A framework
    // cited whole is the framework, so a cell and a roll-up entry with no `clauses:` compare equal.
    public static string Join(string framework, string? reference) =>
        reference is { Length: > 0 } ? $"{framework}.{reference}" : framework;

    // Every mapping a cell states, in the order it states them, as the framework and what of it the
    // clause reaches. Kept apart rather than joined, because the caller weighs the framework on its own
    // before deciding whether the pair travels.
    //
    // Read from the cell's text and its labels together, because neither carries the mapping alone. The
    // text has lost the brackets, so nothing in it says where `NIST SSDF 1.1` ends and `.PO.5` begins;
    // a label knows where it ends and nothing of what follows it. So each label is found in the text in
    // turn, and the reference is whatever runs from the dot after it to the next comma or space.
    //
    // A label the text does not hold is yielded on its own. That is the reading a cell cannot produce,
    // and taking the label at its word keeps one malformed cell from swallowing the labels after it.
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
