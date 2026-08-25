// What the grammar accepts, and what a comparison means when the field it names is absent. Both are
// contracts with whoever writes a schema, and a corpus only ever shows the answer for what it holds.

using kac.core;

namespace kac.tests;

public class RuleExprTests
{
    // A document with the frontmatter, sections and links a case needs, parsed the way kac parses one.
    private static Facts FactsFor(string frontmatter, string body)
    {
        var doc = Doc.Parse("adrs/0001-a-title.md", $"---\n{frontmatter}\n---\n\n# A title\n\n{body}",
            new Schema());
        return new Facts(doc!);
    }

    private static bool Eval(string expr, string frontmatter = "id: adr-0001", string body = "Some prose.") =>
        RuleExpr.Eval(RuleExpr.Compile(expr), FactsFor(frontmatter, body));

    [Fact]
    public void Field_reads_a_frontmatter_scalar()
        => Assert.True(Eval("field('status') == 'deprecated'", "id: adr-0001\nstatus: deprecated"));

    [Fact]
    public void Present_is_false_for_a_bare_key_as_well_as_a_missing_one()
    {
        Assert.False(Eval("present('owner')", "id: adr-0001\nowner:"));
        Assert.False(Eval("present('owner')", "id: adr-0001"));
        Assert.True(Eval("present('owner')", "id: adr-0001\nowner: alex.doe"));
    }

    // `provenance-required` is the rule the list reading exists for: both fields it names are lists, and
    // a reading that saw only scalars would make every standard fail it however it was written.
    [Fact]
    public void Present_reads_a_list_as_well_as_a_scalar()
    {
        Assert.True(Eval("present('derived-from')", "id: std-0001\nderived-from: [ adr-0001 ]"));
        Assert.False(Eval("present('derived-from')", "id: std-0001\nderived-from: []"));
        Assert.False(Eval("present('derived-from')", "id: std-0001\nderived-from:"));
    }

    [Fact]
    public void Section_matches_a_heading_whatever_its_case()
        => Assert.True(Eval("section('context')", body: "## Context\n\nProse."));

    [Fact]
    public void First_section_is_empty_where_the_document_has_no_h2()
    {
        Assert.True(Eval("first_section() == 'Symptoms'", body: "## Symptoms\n\nx\n\n## Escalation\n\ny"));
        Assert.True(Eval("first_section() == ''", body: "No headings at all."));
    }

    // `section()` asks whether a document has one. `section_count()` asks whether it has more than one,
    // which is a different fault. A heading is prose someone wrote, so `## symptom` is the section the
    // schema means however it was capitalised.
    [Fact]
    public void Section_count_counts_the_heading_and_ignores_its_casing()
    {
        Assert.True(Eval("section_count('Symptom') == 2", body: "## Symptom\n\na\n\n## symptom\n\nb"));
        Assert.True(Eval("section_count('Symptom') == 1", body: "## Symptom\n\na\n\n## Cause\n\nb"));
        Assert.True(Eval("section_count('Symptom') == 0", body: "## Cause\n\nb"));
    }

    // The body pattern facts cannot see frontmatter, because a field is judged against what its own
    // declaration says. `field_matches` is false for an absent field, because whether the field ought to
    // be there is `required-field`'s question.
    [Fact]
    public void Field_matches_reads_the_frontmatter_that_matches_cannot()
    {
        const string front = "id: nfr-0001\nmeasured-by: Monitored where practical.";
        Assert.True(Eval("field_matches('measured-by', '(?i)where practical')", front, "Body text."));
        Assert.False(Eval("field_matches('measured-by', 'p95')", front, "p95 appears in the body only."));
        Assert.False(Eval("field_matches('nope', '.')", front, "Body text."));
    }

    // The H1 is prose the document renders, so it counts; the frontmatter above it does not.
    [Fact]
    public void Words_counts_prose_and_not_frontmatter()
        => Assert.True(Eval("words() == 6", "id: adr-0001\nowner: alex.doe", "One two three four."));

    [Fact]
    public void Links_counts_the_links_the_body_carries()
        => Assert.True(Eval("links() == 2", body: "See [a](/a.md) and [b](/b.md)."));

    // One rule covers every operator: a schema author guards with `present(…) implies …` rather than
    // working out which way silence falls for each.

    [Theory]
    [InlineData("field('nope') == 'x'", false)]
    [InlineData("field('nope') != 'x'", true)]
    [InlineData("field('nope') < 'x'", false)]
    [InlineData("field('nope') >= 'x'", false)]
    [InlineData("field('nope') == field('also-nope')", false)]
    [InlineData("field('nope') != field('also-nope')", true)]
    public void A_comparison_against_an_absent_field_is_false_and_its_negation_true(string expr, bool expected)
        => Assert.Equal(expected, Eval(expr));

    // A guarded rule stays quiet rather than reporting a fault that required-field has already reported.
    [Fact]
    public void A_guarded_rule_is_satisfied_when_the_field_it_guards_is_absent()
    {
        const string rule = "present('detected-on') and present('occurred-on') "
                            + "implies field('detected-on') >= field('occurred-on')";

        Assert.True(Eval(rule)); // neither present
        Assert.True(Eval(rule, "id: adr-0001\ndetected-on: \"2026-06-12\"\noccurred-on: \"2026-06-11\""));
        Assert.False(Eval(rule, "id: adr-0001\ndetected-on: \"2026-06-10\"\noccurred-on: \"2026-06-11\""));
    }

    // ISO dates order correctly as text, which is why the grammar carries no date type.
    [Fact]
    public void Iso_dates_order_as_text()
        => Assert.True(Eval("field('a') < field('b')", "id: adr-0001\na: \"2026-01-09\"\nb: \"2026-01-10\""));

    // There are no `true`/`false` literals: every condition starts from something the document says.
    // `not present('nope')` is how a rule writes a constant, and nothing needs one.
    [Theory]
    [InlineData("present('id') or present('nope')", true)]
    [InlineData("present('id') and present('nope')", false)]
    [InlineData("present('nope') implies present('nope')", true)] // false implies anything
    [InlineData("present('id') implies present('nope')", false)]
    [InlineData("not present('nope')", true)]
    [InlineData("words() <= links() * 40", true)]
    [InlineData("(1 + 2) * 3 == 9", true)]
    [InlineData("1 + 2 * 3 == 7", true)] // precedence, not left to right
    [InlineData("4 / 0 == 0", true)]
    public void The_grammar_evaluates_as_declared(string expr, bool expected)
        => Assert.Equal(expected, Eval(expr, body: "One [link](/a.md)."));

    // Each of these would otherwise become a check that silently never fires.

    [Theory]
    [InlineData("field('status')", "yes/no")] // not a question
    [InlineData("words()", "yes/no")]
    [InlineData("wordcount() > 3", "not a fact")] // unknown function
    [InlineData("field()", "1 argument")]         // wrong arity
    [InlineData("field('a', 'b')", "1 argument")]
    [InlineData("words() == 'three'", "cannot compare")]          // number against text
    [InlineData("field('a') * 2 > 3", "wants a number")]          // arithmetic on text
    [InlineData("present('a') < present('b')", "cannot compare")] // ordering yes/no answers
    [InlineData("words() >", "expected a value")]                 // truncated
    [InlineData("words() 3", "unexpected")]                       // two expressions
    [InlineData("field('unterminated", "unterminated string")]
    [InlineData("words() # 3", "unexpected character")]
    [InlineData("present", "must be called with parentheses")]
    public void A_defective_expression_fails_to_compile(string expr, string expectedInMessage)
    {
        var ex = Assert.Throws<RuleExprException>(() => RuleExpr.Compile(expr));
        Assert.Contains(expectedInMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // Parsing a chain would need the grammar to pick an associativity the author did not choose.
    [Fact]
    public void A_chained_comparison_does_not_parse()
        => Assert.Throws<RuleExprException>(() => RuleExpr.Compile("1 < words() < 40"));

    [Fact]
    public void Matches_sees_inside_a_fenced_code_block()
        => Assert.True(Eval("matches('AKIA[0-9A-Z]{6}')",
            body: "Configure it:\n\n```\nAWS_KEY=AKIA123456\n```\n"));

    // The flattened text a word count walks carries no fenced code at all, so folding one fact onto the
    // other fails here rather than in a corpus with a credential in it.
    [Fact]
    public void Words_does_not_see_what_matches_does()
        => Assert.True(Eval("words() == 4 and matches('secret')",
            body: "Configure it:\n\n```\nTOKEN=secret\n```\n"));

    // The emphasis markers are gone by the time the text is flattened, so a rule looking for a bold modal
    // has nowhere else to find one.
    [Fact]
    public void Matches_sees_markdown_syntax()
    {
        Assert.True(Eval(@"matches('\*\*MUST\*\*')", body: "You **MUST** do this."));
        Assert.False(Eval(@"matches('\*\*MUST\*\*')", body: "You MUST do this."));
    }

    // Frontmatter is checked field by field against patterns its own fields declare, and a rule sweeping
    // the prose should not trip over the metadata above it.
    [Fact]
    public void Matches_does_not_see_the_frontmatter()
        => Assert.False(Eval("matches('alex.doe')", "id: adr-0001\nowner: alex.doe", "Prose with no name in it."));

    [Fact]
    public void Section_matches_is_bounded_by_the_next_heading()
    {
        const string body = "## Steps\n\nDo the thing.\n\n## Verification\n\nTypically it worked.\n";
        Assert.False(Eval("section_matches('Steps', 'Typically')", body: body));
        Assert.True(Eval("section_matches('Verification', 'Typically')", body: body));
    }

    [Fact]
    public void Section_matches_is_false_where_the_section_is_absent()
        => Assert.False(Eval("section_matches('Nowhere', 'anything')", body: "## Steps\n\nDo the thing."));

    [Fact]
    public void Section_matches_finds_the_heading_whatever_its_case()
        => Assert.True(Eval("section_matches('steps', 'thing')", body: "## Steps\n\nDo the thing."));

    [Theory]
    [InlineData("matches()", "1 argument")]
    [InlineData("section_matches('Steps')", "2 argument")]
    [InlineData("matches(3)", "wants text")]
    public void The_text_probe_is_type_checked_like_any_other_fact(string expr, string expected)
    {
        var ex = Assert.Throws<RuleExprException>(() => RuleExpr.Compile(expr));
        Assert.Contains(expected, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // A doubled quote is one quote, as in YAML and SQL. Regular expressions are the strings that most need
    // one, and this is the escape that avoids putting a second backslash layer over an expression already
    // full of them. The pattern below is the character class ["'].
    [Fact]
    public void A_doubled_quote_is_one_quote()
    {
        const string quoteClass = "matches('[\"'']')";

        Assert.True(Eval(quoteClass, body: "It's here."));
        Assert.True(Eval(quoteClass, body: "A \"quote\"."));
        Assert.False(Eval(quoteClass, body: "Nothing quoted."));
    }

    [Fact]
    public void A_string_left_open_by_an_escaped_quote_still_fails_to_compile()
        => Assert.Throws<RuleExprException>(() => RuleExpr.Compile("matches('a''"));
}
