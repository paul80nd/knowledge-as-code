// The expression layer for `rules:`. Two things are pinned here that nothing else can pin: what the
// grammar accepts, and what a comparison means when the field it names is absent. Both are contracts
// with whoever writes a schema, and neither is visible in a golden — a corpus only ever shows the
// answer for the documents it happens to hold.

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

    // -- the facts --

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

    [Fact]
    public void Section_matches_a_heading_whatever_its_case()
        => Assert.True(Eval("section('context')", body: "## Context\n\nProse."));

    [Fact]
    public void First_section_is_empty_where_the_document_has_no_h2()
    {
        Assert.True(Eval("first_section() == 'Symptoms'", body: "## Symptoms\n\nx\n\n## Escalation\n\ny"));
        Assert.True(Eval("first_section() == ''", body: "No headings at all."));
    }

    // The H1 is prose the document renders, so it counts; the frontmatter above it does not.
    [Fact]
    public void Words_counts_prose_and_not_frontmatter()
        => Assert.True(Eval("words() == 6", "id: adr-0001\nowner: alex.doe", "One two three four."));

    [Fact]
    public void Links_counts_the_links_the_body_carries()
        => Assert.True(Eval("links() == 2", body: "See [a](/a.md) and [b](/b.md)."));

    // -- absence --
    //
    // A comparison where either side is absent is false, and `!=` is its negation, so it is true. The
    // point of the rule is that it is one rule: a schema author guards with `present(…) implies …`
    // rather than working out which way silence falls for each operator.

    [Theory]
    [InlineData("field('nope') == 'x'", false)]
    [InlineData("field('nope') != 'x'", true)]
    [InlineData("field('nope') < 'x'", false)]
    [InlineData("field('nope') >= 'x'", false)]
    [InlineData("field('nope') == field('also-nope')", false)]
    [InlineData("field('nope') != field('also-nope')", true)]
    public void A_comparison_against_an_absent_field_is_false_and_its_negation_true(string expr, bool expected)
        => Assert.Equal(expected, Eval(expr));

    // The idiom that follows from it: a rule about a field that may be missing says so, and stays quiet
    // rather than reporting a fault that required-field has already reported.
    [Fact]
    public void A_guarded_rule_is_satisfied_when_the_field_it_guards_is_absent()
    {
        const string rule = "present('detected-on') and present('occurred-on') "
                            + "implies field('detected-on') >= field('occurred-on')";

        Assert.True(Eval(rule));                                                        // neither present
        Assert.True(Eval(rule, "id: adr-0001\ndetected-on: \"2026-06-12\"\noccurred-on: \"2026-06-11\""));
        Assert.False(Eval(rule, "id: adr-0001\ndetected-on: \"2026-06-10\"\noccurred-on: \"2026-06-11\""));
    }

    // ISO dates order correctly as text, which is why the grammar carries no date type.
    [Fact]
    public void Iso_dates_order_as_text()
        => Assert.True(Eval("field('a') < field('b')", "id: adr-0001\na: \"2026-01-09\"\nb: \"2026-01-10\""));

    // -- the grammar --

    // There are no `true`/`false` literals: every condition starts from something the document says.
    // `not present('nope')` is how a rule writes a constant, and nothing needs one.
    [Theory]
    [InlineData("present('id') or present('nope')", true)]
    [InlineData("present('id') and present('nope')", false)]
    [InlineData("present('nope') implies present('nope')", true)]  // false implies anything
    [InlineData("present('id') implies present('nope')", false)]
    [InlineData("not present('nope')", true)]
    [InlineData("words() <= links() * 40", true)]
    [InlineData("(1 + 2) * 3 == 9", true)]
    [InlineData("1 + 2 * 3 == 7", true)]                           // precedence, not left to right
    [InlineData("4 / 0 == 0", true)]                               // division by zero yields zero
    public void The_grammar_evaluates_as_declared(string expr, bool expected)
        => Assert.Equal(expected, Eval(expr, body: "One [link](/a.md)."));

    // -- what will not compile --
    //
    // Every one of these is a schema defect that would otherwise become a check that silently never
    // fires, so each is a load-time failure rather than a quiet false.

    [Theory]
    [InlineData("field('status')", "yes/no")]                    // not a question
    [InlineData("words()", "yes/no")]
    [InlineData("wordcount() > 3", "not a fact")]                // unknown function
    [InlineData("field()", "1 argument")]                        // wrong arity
    [InlineData("field('a', 'b')", "1 argument")]
    [InlineData("words() == 'three'", "cannot compare")]         // number against text
    [InlineData("field('a') * 2 > 3", "wants a number")]         // arithmetic on text
    [InlineData("present('a') < present('b')", "cannot compare")] // ordering yes/no answers
    [InlineData("words() >", "expected a value")]                // truncated
    [InlineData("words() 3", "unexpected")]                      // two expressions
    [InlineData("field('unterminated", "unterminated string")]
    [InlineData("words() # 3", "unexpected character")]
    [InlineData("present", "must be called with parentheses")]
    public void A_defective_expression_fails_to_compile(string expr, string expectedInMessage)
    {
        var ex = Assert.Throws<RuleExprException>(() => RuleExpr.Compile(expr));
        Assert.Contains(expectedInMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // A chained comparison is a sentence rather than a condition, and the grammar declines it rather
    // than picking an associativity the author did not choose.
    [Fact]
    public void A_chained_comparison_does_not_parse()
        => Assert.Throws<RuleExprException>(() => RuleExpr.Compile("1 < words() < 40"));
}
