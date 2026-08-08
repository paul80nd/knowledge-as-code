namespace kac.core;

// A Y-statement is a block-quote beneath the H1 stating six moves. Three things can be wrong with one,
// and the rule stays in C# for the third: an expression could report that the block-quote is not a
// Y-statement, which is the one thing the author already knows. Naming the absent move is the message
// worth reading, and it needs a list the grammar has no way to hold.
public sealed class YStatementPresent : IDocumentRule
{
    public string RuleId => "y-statement-present";

    public IReadOnlyList<CheckDef> Emits =>
    [
        new("y-statement", Sev.Warning, "A short Y-statement block-quote, stating all six moves, follows the H1.")
    ];

    public void Check(RuleContext ctx)
    {
        var (d, warn) = (ctx.Doc, ctx.Warn);

        if (d.YStatement is null)
        {
            warn("y-statement", "no Y-statement block-quote follows the H1.", d.H1Line);
            return;
        }

        var text = Md.PlainText(d.YStatement);
        var line = d.YStatement.Line + 1;

        var missing = MissingMoves(text);
        if (missing.Count > 0)
            warn("y-statement",
                $"Y-statement is missing {QuotedList(missing)}. The six moves are what make it a "
                + "Y-statement rather than a summary of one.", line);

        // The ceiling is the schema's, because a threshold is a judgement a corpus tunes. The moves
        // below are not: a block-quote missing one is not a Y-statement worded differently.
        var max = ctx.Spec.MaxWords ?? 60;
        var words = text.Split([' ', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        if (words > max)
            warn("y-statement", $"Y-statement is {words} words; keep it under {max}.", line);
    }

    // The six moves, in the order they are written.
    private static readonly string[] Moves =
        ["in the context of", "facing", "we decided", "rather than", "to achieve", "accepting"];

    // Asked of the rendered text, so the bold that marks each move in the corpus is already gone and a
    // Y-statement written without it still reads. Whole words only: `facing` is not found inside
    // `surfacing`, and a move followed by a comma is still found.
    public static List<string> MissingMoves(string text)
    {
        var words = new string(text.Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : ' ').ToArray())
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var haystack = $" {string.Join(' ', words)} ";
        return Moves.Where(m => !haystack.Contains($" {m} ", StringComparison.Ordinal)).ToList();
    }

    // The moves as a sentence reads them: "facing", "rather than" and "accepting".
    private static string QuotedList(IReadOnlyList<string> items) =>
        items.Count == 1
            ? Quoted(items[0])
            : $"{string.Join(", ", items.Take(items.Count - 1).Select(Quoted))} and {Quoted(items[^1])}";

    private static string Quoted(string item) => $"\"{item}\"";
}
