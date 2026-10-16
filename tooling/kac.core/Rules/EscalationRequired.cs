using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public sealed class EscalationRequired : IDocumentRule
{
    public RuleId RuleId => new("escalation-required");

    private static readonly CheckId DeadEnd = new("diagnosis-dead-end");
    private static readonly CheckId NoEscalation = new("diagnosis-no-escalation");

    public IReadOnlyList<CheckId> Emits => [DeadEnd, NoEscalation];

    public void Check(RuleContext ctx)
    {
        var lists = Branches(ctx.Doc).ToList();

        if (lists.Count == 0) return;

        var anyEscalation = false;

        for (var i = 0; i < lists.Count; i++)
        {
            var last = i == lists.Count - 1;

            foreach (var branch in lists[i])
            {
                if (branch.LinksTo("#escalation")) anyEscalation = true;
                if (branch.LinksTo("#resolution") || branch.LinksTo("#escalation")) continue;

                if (!branch.FallsThrough)
                    ctx.Report.Err(DeadEnd,
                        $"this diagnosis branch routes nowhere: \"{Md.Snippet(branch.Text)}\". Send it to " +
                        "`[Resolution](#resolution)` or to `[escalate](#escalation)`, or write `continue`.",
                        branch.Line);
                else if (last)
                    ctx.Report.Err(DeadEnd,
                        $"this diagnosis branch says `continue` under the last question: " +
                        $"\"{Md.Snippet(branch.Text)}\". Nothing follows it, so link it to `#resolution` " +
                        "or to `#escalation`.",
                        branch.Line);
            }
        }

        if (!anyEscalation)
            ctx.Report.Err(NoEscalation,
                "no branch of this diagnosis reaches `#escalation`, so a reader the tree does not answer " +
                "has nowhere to go. Close one branch with `[escalate](#escalation)`.",
                ctx.Doc.FrontStartLine);
    }

    /// <summary>
    /// Each list is its own group, because whether <c>continue</c> strands the reader depends on the list being the
    /// last one.
    /// </summary>
    private static IEnumerable<List<Branch>> Branches(Doc d) =>
        Md.ListsUnder(d.Ast, "Diagnosis")
            .Select(list => list.OfType<ListItemBlock>().Select(Read).ToList())
            .Where(branches => branches.Count > 0);

    /// <summary>
    /// The literals concatenated, then split and rejoined on whitespace. A branch opens <c>**Yes** -&gt;</c>, so the
    /// bold run and the arrow arrive as separate literals and the message would quote a double space.
    /// </summary>
    private static Branch Read(ListItemBlock li) => new(
        string.Join(" ", string.Concat(li.Descendants<LiteralInline>().Select(x => x.Content.ToString()))
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
        li.Descendants<LinkInline>().Select(l => l.Url ?? "").ToList(),
        li.Line + 1);

    private sealed record Branch(string Text, IReadOnlyList<string> Urls, int Line)
    {
        /// <summary>Case-insensitive, because a runbook writes the label both ways.</summary>
        public bool LinksTo(string anchor) =>
            Urls.Any(u => u.EndsWith(anchor, StringComparison.OrdinalIgnoreCase));

        public bool FallsThrough =>
            Text.TrimEnd('.', ' ').EndsWith("continue", StringComparison.OrdinalIgnoreCase);
    }
}
