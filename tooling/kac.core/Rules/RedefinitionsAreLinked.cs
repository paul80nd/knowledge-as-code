using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public sealed class RedefinitionsAreLinked : ICorpusRule
{
    public RuleId RuleId => new("redefinitions-are-linked");

    private static readonly CheckId Unlinked = new("redefinition-unlinked");

    public IReadOnlyList<CheckId> Emits => [Unlinked];

    /// <summary>Renaming the field in <c>.schema/glossary.yaml</c> means renaming it here.</summary>
    private const string NarrowsField = "narrows";

    public void Check(CorpusRuleContext ctx)
    {
        // Ordered by path, so a finding lands on the same glossary on every run.
        var entries = ctx.Records
            .OrderBy(d => d.Rel, StringComparer.Ordinal)
            .SelectMany(d => d.Parts.Where(p => p.Id is not null).Select(p => (Doc: d, Part: p)))
            .GroupBy(e => e.Part.Id, StringComparer.Ordinal);

        foreach (var term in entries)
        {
            var defined = term.ToList();
            for (var i = 0; i < defined.Count; i++)
            for (var j = i + 1; j < defined.Count; j++)
            {
                var (a, b) = (defined[i], defined[j]);
                if (a.Doc == b.Doc) continue;
                if (Links(ctx.Tree, a.Doc, a.Part, b.Doc) || Links(ctx.Tree, b.Doc, b.Part, a.Doc)) continue;

                // A glossary every corpus shares cannot link a record only one corpus has, so the narrower one is told.
                if (Depth(ctx, b.Doc) > Depth(ctx, a.Doc)) (a, b) = (b, a);

                var target = $"{Relative(a.Doc.Rel, b.Doc.Rel)}#{b.Part.Id}";
                ctx.Warn(a.Doc, Unlinked,
                    $"'{Md.Snippet(a.Part.Text)}' is defined here and in '{b.Doc.Rel}', and neither entry links to the "
                    + $"other. Link one entry to the other's anchor, as `{target}`, in its `**Not:**` line.",
                    a.Part.Line);
            }
        }
    }

    /// <summary>
    /// How many glossaries sit above this one along <c>narrows</c>. A cycle or an id naming no record ends the walk.
    /// </summary>
    private static int Depth(CorpusRuleContext ctx, Doc doc)
    {
        var seen = new HashSet<Doc> { doc };
        var at = doc;
        while (at.FrontScalar(NarrowsField) is { Length: > 0 } id
               && ctx.ById.GetValueOrDefault(id) is { } up
               && seen.Add(up))
            at = up;

        return seen.Count - 1;
    }

    private static string Relative(string fromRel, string toRel) =>
        Path.GetRelativePath(Path.GetDirectoryName(fromRel) ?? "", toRel).Replace('\\', '/');

    /// <summary>
    /// Whether the entry's body links the other glossary at this term's anchor. A link to the file alone does not
    /// count.
    /// </summary>
    private static bool Links(Tree tree, Doc from, PartRow part, Doc to) =>
        from.Ast.Descendants<LinkInline>()
            .Where(l => !l.IsImage && part.Contains(l.Span))
            .Select(l => l.Url ?? "")
            .Any(url => url.IndexOf('#') is var hash and >= 0
                        && string.Equals(url[(hash + 1)..], part.Id, StringComparison.Ordinal)
                        && LinkChecks.Resolve(tree, from.Rel, url) == to.Rel);
}
