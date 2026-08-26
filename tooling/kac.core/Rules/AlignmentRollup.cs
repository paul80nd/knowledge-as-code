using YamlDotNet.RepresentationModel;

namespace kac.core;

// A policy states a framework mapping twice: once in the `Alignment` cell of the clause it qualifies,
// and once in the `aligns-with` roll-up the generated index is built from. The cells are the statement
// and the roll-up is a summary of them, so the two agreeing is what makes the index worth reading.
//
// Both directions, because either half going stale is the same defect wearing a different face. A
// reference in a cell and not in the roll-up leaves the index under-reporting coverage. A reference in
// the roll-up and not in any cell is a claim of coverage no clause can show, and that one reads as
// evidence to whoever is looking for it.
//
// The rule is a class because the message has to name the reference and the side it is missing from.
// One fixed string could say only that the two disagree, which leaves an author with two frameworks and
// thirty references to diff by eye.
public sealed class AlignmentRollup : IDocumentRule
{
    public RuleId RuleId => new("alignment-rollup");

    private static readonly CheckId Reports = new("alignment-rollup");

    public IReadOnlyList<CheckId> Emits => [Reports];

    // The frontmatter key holding the roll-up. Named here rather than read from the type, because no
    // declaration says which field summarises a column: the type declares the column and the field
    // separately, and only this rule knows they are two accounts of one thing.
    private const string Field = "aligns-with";

    public void Check(RuleContext ctx)
    {
        if (ctx.Type.Parts is not { } parts) return;

        var column = parts.Columns.FirstOrDefault(c =>
            string.Equals(c, "Alignment", StringComparison.OrdinalIgnoreCase));
        if (column is null) return;

        var cited = FromClauses(ctx.Doc, column);
        var declared = FromFrontmatter(ctx.Doc, out var fieldLine);

        // A policy citing nothing and declaring nothing is the ordinary case for a governance policy,
        // and neither half of the report below has anything to say about it.
        if (cited.Count == 0 && declared.Count == 0) return;

        foreach (var (reference, line) in Missing(cited, declared))
            ctx.Report.Err(Reports,
                $"'{reference}' is cited in the clause table and missing from '{Field}'.", line);

        foreach (var (reference, _) in Missing(declared, cited))
            ctx.Report.Err(Reports,
                $"'{Field}' claims '{reference}', and no clause cites it.", fieldLine);
    }

    // What one side holds that the other does not, in the order the first side reads. Comparison is
    // ordinal: a framework label and a reference into it are both written to be quoted, so a difference
    // of case is a difference worth reporting rather than one to absorb.
    private static IEnumerable<(string Reference, int? Line)> Missing(
        IReadOnlyDictionary<string, int?> side, IReadOnlyDictionary<string, int?> other) =>
        side.Where(pair => !other.ContainsKey(pair.Key)).Select(pair => (pair.Key, pair.Value));

    // Every reference the clause table cites, as `framework` or `framework.reference`, keyed so that a
    // reference cited by six clauses is one fact rather than six. The line kept is the first clause
    // citing it, which is where an author reading down the table meets it.
    //
    // A cell is read from the row's own cells rather than from the rendered text, so a column that
    // moves stays readable and a clause mentioning a framework in its wording is not mistaken for a
    // mapping.
    private static Dictionary<string, int?> FromClauses(Doc doc, string column)
    {
        var found = new Dictionary<string, int?>(StringComparer.Ordinal);

        foreach (var row in doc.Parts)
        {
            if (row.Cells?.GetValueOrDefault(column) is not { Length: > 0 } cell) continue;

            var labels = row.CellLinks?.GetValueOrDefault(column) ?? [];

            foreach (var reference in Alignment.References(cell, labels))
                found.TryAdd(reference, row.Line);
        }

        return found;
    }

    // The roll-up, flattened to the same vocabulary the cells are read into. A framework carrying no
    // `clauses:` flattens to the framework alone, which is what a cell citing the standard entire
    // writes.
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

// How an `Alignment` cell writes a mapping, in the one place both sides of the rule read it from.
//
// A cell holds reference links into `frameworks.md`, each optionally followed by a reference within the
// framework: `[ISO 27001:2022].A.8.24, [NIST SSDF 1.1].PO.5`. The label is the framework and the dot is
// the same addressing a citation uses to reach inside a document.
public static class Alignment
{
    // The two halves of a mapping, joined the way a message and a comparison both need it.
    public static string Join(string framework, string reference) => $"{framework}.{reference}";

    // Every mapping a cell states, in the order it states them.
    //
    // Read from the cell's text and its labels together, because neither carries the mapping alone. The
    // text has lost the brackets, so nothing in it says where `NIST SSDF 1.1` ends and `.PO.5` begins;
    // a label knows where it ends and nothing of what follows it. So each label is found in the text in
    // turn, and the reference is whatever runs from the dot after it to the next comma or space.
    //
    // A label the text does not hold is yielded on its own. That is the reading a cell cannot produce,
    // and taking the label at its word keeps one malformed cell from swallowing the labels after it.
    public static IEnumerable<string> References(string cell, IReadOnlyList<string> labels)
    {
        var from = 0;

        foreach (var label in labels)
        {
            var at = cell.IndexOf(label, from, StringComparison.Ordinal);
            if (at < 0)
            {
                yield return label;
                continue;
            }

            var i = at + label.Length;
            from = i;

            if (i >= cell.Length || cell[i] != '.')
            {
                yield return label;
                continue;
            }

            var start = ++i;
            while (i < cell.Length && cell[i] != ',' && !char.IsWhiteSpace(cell[i])) i++;
            from = i;

            var reference = cell[start..i];
            yield return reference.Length > 0 ? Join(label, reference) : label;
        }
    }
}
