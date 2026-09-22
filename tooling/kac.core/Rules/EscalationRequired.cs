using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

// A diagnosis tree is followed during an incident, one branch at a time, so a branch that sends the
// reader nowhere strands them. Each branch either routes, by linking to `#resolution` or `#escalation`,
// or falls through to the next question by saying `continue`. The last question has nothing to fall
// through to, so `continue` strands the reader there as surely as a branch with no route at all.
//
// The message quotes the branch that failed, which is why this stays in C#. The grammar has no
// collections, and an author told that some branch in the tree is wrong has to find it themselves.
//
// Reported per branch, and once for a tree with no escalation in it. `failure-route-stated` asks the
// same of `Resolution`, for the reader whose fix did not work rather than whose diagnosis ran out.
public sealed class EscalationRequired : IDocumentRule
{
    public RuleId RuleId => new("escalation-required");

    private static readonly CheckId DeadEnd = new("diagnosis-dead-end");
    private static readonly CheckId NoEscalation = new("diagnosis-no-escalation");

    public IReadOnlyList<CheckId> Emits => [DeadEnd, NoEscalation];

    public void Check(RuleContext ctx)
    {
        var lists = Branches(ctx.Doc).ToList();

        // A Diagnosis written as prose has no branches to judge. Whether a tree is owed at all is a
        // question about the section, and this rule reads the branches under it.
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

    // Every list under `## Diagnosis`, each as its own group, because whether `continue` strands the
    // reader depends on a list being the last one. Only a list's own items are read: a nested list is
    // one branch's workings and routes nowhere of its own.
    private static IEnumerable<List<Branch>> Branches(Doc d)
    {
        var inSection = false;
        foreach (var block in d.Ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), "Diagnosis", StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
                continue;
            }

            if (!inSection || block is not ListBlock list) continue;

            var branches = list.OfType<ListItemBlock>().Select(Read).ToList();
            if (branches.Count > 0) yield return branches;
        }
    }

    // The literals concatenated, then split and rejoined on whitespace. A branch opens `**Yes** ->`, so
    // the bold run and the arrow arrive as separate literals and the message would quote a double space.
    private static Branch Read(ListItemBlock li) => new(
        string.Join(" ", string.Concat(li.Descendants<LiteralInline>().Select(x => x.Content.ToString()))
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
        li.Descendants<LinkInline>().Select(l => l.Url ?? "").ToList(),
        li.Line + 1);

    private sealed record Branch(string Text, IReadOnlyList<string> Urls, int Line)
    {
        // The anchor as any renderer writes it, matched without regard to case because a runbook writes
        // the label both ways: `[escalate](#escalation)` and `[Escalate](#escalation)`.
        public bool LinksTo(string anchor) =>
            Urls.Any(u => u.EndsWith(anchor, StringComparison.OrdinalIgnoreCase));

        // The bare word the template teaches, at the end of the branch. A branch continuing into prose
        // of its own is told to write the word, which is the form a reader skims for.
        public bool FallsThrough =>
            Text.TrimEnd('.', ' ').EndsWith("continue", StringComparison.OrdinalIgnoreCase);
    }
}
