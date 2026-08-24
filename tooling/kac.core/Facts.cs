using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Markdig.Syntax;

namespace kac.core;

// Everything a rule expression is allowed to ask about a document, and the whole of it. Each answer
// reads what the parse pass already produced, and the evaluator never re-parses markdown. So adding
// a fact means adding one method here and one entry to RuleExpr's function table, and never touching
// the grammar.
//
// Built per document and discarded after its rules have run, which is what makes the measurements it
// caches safe to cache: the document it was built from cannot change while it exists.
public sealed class Facts(Doc doc)
{
    // Null where the frontmatter does not carry the key at all, or carries it as a bare key. A rule
    // comparing against an absent field is answered by RuleExpr's comparison rules, not here.
    public string? Field(string name) => doc.FrontScalar(name);

    // Whether the field carries anything, asked of either shape a field may be written in. A rule guards
    // on the field, not on how the schema declared it: `present('derived-from')` is the same question
    // whether the type declares a scalar id or a list of them, and a reading that saw only the scalar
    // would answer no to every list field and make a rule that guards on one impossible to satisfy.
    //
    // A bare key and an empty sequence are both absent, which is the same state `bare-key` reports on.
    public bool Present(string name) => doc.FrontList(name).Count > 0;

    // Case-insensitive, matching required-section: a heading is prose a person wrote, and '## context'
    // is the section the schema means however it was capitalised.
    public bool Section(string title) =>
        doc.Sections.Any(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));

    // How many times that heading appears. `section()` answers whether a document has one. This answers
    // whether it has more than one, which is a different fault: a page carrying two of a heading that
    // names the thing it is about is two documents that have been filed as one.
    public int SectionCount(string title) =>
        doc.Sections.Count(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));

    // Empty where the document has no H2 at all, so a rule naming the first section reads false rather
    // than throwing on a document that has none.
    public string FirstSection() => doc.Sections.Count > 0 ? doc.Sections[0].Title : "";

    public int Links() => doc.Links.Count;

    // Whether the body matches a pattern the schema supplies. Read as written, so code fences, link
    // targets and the markdown syntax itself are all in scope; `docs/checks.md` says which
    // rules need that. It is also what lets `\*\*MUST\*\*` find a bold modal that the rendered text
    // would have flattened away.
    //
    // Frontmatter is excluded: it is checked field by field, against patterns its fields declare.
    public bool Matches(string pattern) => Pattern(pattern).IsMatch(Body);

    // A pattern asked of one frontmatter scalar, which `matches()` deliberately cannot reach: the body
    // is prose and the frontmatter is fields, and a field is judged against what its own declaration
    // says. False for an absent field, so a rule about a value guards nothing. Whether the field ought
    // to be there is `required-field`'s question, asked in better words.
    public bool FieldMatches(string name, string pattern) =>
        doc.FrontScalar(name) is { Length: > 0 } value && Pattern(pattern).IsMatch(value);

    // The same question asked of one section's body. False where the document has no such section, so a
    // rule naming a section reads as satisfied rather than throwing; whether the section ought to be
    // there is `required-section`'s.
    public bool SectionMatches(string title, string pattern) =>
        SectionText(title) is { } text && Pattern(pattern).IsMatch(text);

    // Words of prose: every heading and paragraph the document renders, and nothing else. Frontmatter
    // is excluded because it is not prose, and fenced code with it. Neither carries inline content, so
    // both fall out of the walk rather than needing to be skipped. Whitespace-separated runs, which is
    // what a person means when they say a Y-statement is too long.
    public int Words() => words ??= CountWords();

    private int? words;

    private string Body => body ??= doc.Text[doc.BodyStart..];

    private string? body;

    // The source of one section: the first of them, where a document repeats a heading, which is the
    // one a rule naming it means. Found on the heading text, because that is what the schema names a
    // section by everywhere else, and case-insensitively for the same reason `section()` is.
    private string? SectionText(string title) =>
        doc.Sections.FirstOrDefault(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase))
            is { } section
            ? doc.Text[section.BodyStart..section.BodyEnd]
            : null;

    // Schema patterns are few and fixed, and every document of a type asks the same ones, so they are
    // parsed once for the life of the process rather than once per document.
    private static readonly ConcurrentDictionary<string, Regex> Patterns = new();

    private static Regex Pattern(string pattern) =>
        Patterns.GetOrAdd(pattern, p => new Regex(p, RegexOptions.CultureInvariant));

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
