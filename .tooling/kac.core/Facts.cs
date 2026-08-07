using Markdig.Syntax;

// ---------------------------------------------------------------------------
// The facts an `expr:` can see
// ---------------------------------------------------------------------------

namespace kac.core;

// Everything a rule expression is allowed to ask about a document, and the whole of it. Each answer
// reads what the parse pass already produced — the evaluator never re-parses markdown — so adding a
// fact means adding one method here and one entry to RuleExpr's function table, and never touching
// the grammar.
//
// Built per document and discarded after its rules have run, which is what makes the measurements it
// caches safe to cache: the document it was built from cannot change while it exists.
public sealed class Facts(Doc doc)
{
    // Null where the frontmatter does not carry the key at all, or carries it as a bare key. A rule
    // comparing against an absent field is answered by RuleExpr's comparison rules, not here.
    public string? Field(string name) => doc.FrontScalar(name);

    public bool Present(string name) => doc.FrontScalar(name) is { Length: > 0 };

    // Case-insensitive, matching required-section: a heading is prose a person wrote, and '## context'
    // is the section the schema means however it was capitalised.
    public bool Section(string title) =>
        doc.H2.Any(h => string.Equals(h, title, StringComparison.OrdinalIgnoreCase));

    // Empty where the document has no H2 at all, so a rule naming the first section reads false rather
    // than throwing on a document that has none.
    public string FirstSection() => doc.H2.Count > 0 ? doc.H2[0] : "";

    public int Links() => doc.Links.Count;

    // Words of prose: every heading and paragraph the document renders, and nothing else. Frontmatter
    // is excluded because it is not prose, and fenced code with it — neither carries inline content, so
    // both fall out of the walk rather than needing to be skipped. Whitespace-separated runs, which is
    // what a person means when they say a Y-statement is too long.
    public int Words() => words ??= CountWords();

    private int? words;

    private int CountWords()
    {
        var total = 0;
        foreach (var leaf in doc.Ast.Descendants<LeafBlock>())
        {
            if (leaf.Inline is null) continue;
            total += Md.PlainText(leaf.Inline)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        return total;
    }
}
