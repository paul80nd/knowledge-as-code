using System.Globalization;
using YamlDotNet.RepresentationModel;

namespace kac.core;

public sealed class YStatementPresent : IDocumentRule
{
    public RuleId RuleId => new("y-statement-present");

    private static readonly CheckId Reports = new("y-statement");

    public IReadOnlyList<CheckId> Emits => [Reports];

    private const string Limit = "max-words";

    public IReadOnlyList<string> Reads => [Limit];

    private const int DefaultLimit = 60;

    public IEnumerable<string> Misconfigured(RuleSpec spec, Schema schema)
    {
        if (!spec.Config.TryGetValue(Limit, out var declared) || MaxWords(spec) is not null) yield break;

        var written = declared is YamlScalarNode { Value: { } value } ? $"is '{value}'" : "is not a single value";
        yield return $"'{Limit}' {written}, and a word limit is a whole number above zero. Write one, such as "
                     + $"{DefaultLimit}.";
    }

    private static int? MaxWords(RuleSpec spec) =>
        int.TryParse(Yaml.Str(spec.Config.GetValueOrDefault(Limit)), NumberStyles.None, CultureInfo.InvariantCulture,
            out var words) && words > 0
            ? words
            : null;

    public void Check(RuleContext ctx)
    {
        var (d, report) = (ctx.Doc, ctx.Report);

        if (d.YStatement is null)
        {
            report.Warn(Reports, "no Y-statement block-quote follows the H1.", d.H1Line);
            return;
        }

        var text = Md.PlainText(d.YStatement);
        var line = d.YStatement.Line + 1;

        var missing = MissingMoves(text);
        if (missing.Count > 0)
            report.Warn(Reports,
                $"Y-statement is missing {QuotedList(missing)}. The six moves are what make it a "
                + "Y-statement rather than a summary of one.", line);

        var max = MaxWords(ctx.Spec) ?? DefaultLimit;
        var words = text.Split([' ', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        if (words > max)
            report.Warn(Reports, $"Y-statement is {words} words; keep it under {max}.", line);
    }

    private static readonly string[] Moves =
        ["in the context of", "facing", "we decided", "rather than", "to achieve", "accepting"];

    /// <summary>
    /// Asked of the rendered text, in whole words, so <c>facing</c> is not found inside <c>surfacing</c>.
    /// </summary>
    public static List<string> MissingMoves(string text)
    {
        var words = new string(text.Select(c => char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : ' ').ToArray())
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var haystack = $" {string.Join(' ', words)} ";
        return [.. Moves.Where(m => !haystack.Contains($" {m} ", StringComparison.Ordinal))];
    }

    private static string QuotedList(IReadOnlyList<string> items) =>
        items.Count == 1
            ? Quoted(items[0])
            : $"{string.Join(", ", items.Take(items.Count - 1).Select(Quoted))} and {Quoted(items[^1])}";

    private static string Quoted(string item) => $"\"{item}\"";
}
