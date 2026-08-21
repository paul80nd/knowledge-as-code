using kac.core;

// A rule that needs C# is a class with one method, so each of its ways to fail can be driven directly
// rather than only through a whole corpus. That matters more here than elsewhere: the coverage gate
// reads check ids, not branches, so a rule reporting three faults under one id is green once a fixture
// trips any one of them. These tests are what hold the other two honest.

namespace kac.tests;

public class DocumentRuleTests
{
    // -- the registry --

    [Fact]
    public void Every_registered_rule_has_a_distinct_id_and_reports_something()
    {
        Assert.Equal(DocumentRules.All.Count, DocumentRules.ByRuleId.Count);
        Assert.All(DocumentRules.All, r => Assert.NotEmpty(r.Emits));
    }

    // What each emitted id *means* is `_checks.yaml`'s to say. Here the ids only have to be ids.
    [Fact]
    public void Every_emitted_id_is_a_usable_check_id()
    {
        Assert.All(DocumentRules.All.SelectMany(r => r.Emits), e => Assert.NotEmpty(e.Value));
    }

    // -- y-statement-present: three ways to fail, one check id --

    [Fact]
    public void A_document_with_no_block_quote_is_told_the_Y_statement_is_absent()
    {
        var found = Run(new YStatementPresent(), Adr("Nothing follows the H1 but prose."));
        Assert.Equal("y-statement", Single(found).Check.Value);
        Assert.Equal("no Y-statement block-quote follows the H1.", Single(found).Message);
    }

    [Fact]
    public void A_block_quote_short_of_its_moves_is_told_which_ones()
    {
        var found = Run(new YStatementPresent(),
            Adr("> **In the context of** a summary, **we decided** to drop two moves, **to achieve**\n"
                + "> brevity, **accepting** that it is no longer a Y-statement."));

        Assert.Equal("y-statement", Single(found).Check.Value);
        Assert.StartsWith("Y-statement is missing \"facing\" and \"rather than\".", Single(found).Message);
    }

    [Fact]
    public void A_single_absent_move_is_named_without_the_list_wording()
    {
        var found = Run(new YStatementPresent(),
            Adr("> **In the context of** a summary, **facing** one missing move, **we decided** to leave\n"
                + "> it out, **to achieve** coverage, **accepting** the warning."));

        Assert.StartsWith("Y-statement is missing \"rather than\".", Single(found).Message);
    }

    [Fact]
    public void A_Y_statement_past_the_ceiling_is_told_the_count_and_the_limit()
    {
        var body = "> " + string.Join(" ", Enumerable.Repeat("word", 40)) + " **in the context of** x,"
                   + " **facing** y, **we decided** z, **rather than** w, **to achieve** v, **accepting** u.";
        var found = Run(new YStatementPresent(), Adr(body),
            new RuleSpec { Id = new RuleId("y-statement-present"), MaxWords = 20 });

        Assert.Equal("y-statement", Single(found).Check.Value);
        Assert.Contains("keep it under 20.", Single(found).Message);
    }

    // The ceiling comes from the schema so that a corpus can tune it without a release; the default is
    // only what applies when the schema declares none.
    [Fact]
    public void The_ceiling_is_the_schemas_and_a_Y_statement_within_it_is_silent()
    {
        const string body =
            "> **In the context of** a fixture, **facing** a ceiling, **we decided** to stay under it,\n"
            + "> **rather than** over, **to achieve** silence, **accepting** nothing.";

        Assert.Empty(Run(new YStatementPresent(), Adr(body)));
        Assert.NotEmpty(Run(new YStatementPresent(), Adr(body),
            new RuleSpec { Id = new RuleId("y-statement-present"), MaxWords = 5 }));
    }

    // The moves are matched on the rendered text, so the bold that marks them in the corpus is optional
    // and a move followed by punctuation is still found. Whole words only, or `facing` would be found
    // inside `surfacing`.
    [Theory]
    [InlineData("in the context of a, facing b, we decided c, rather than d, to achieve e, accepting f.", 0)]
    [InlineData("IN THE CONTEXT OF a, FACING b, WE DECIDED c, RATHER THAN d, TO ACHIEVE e, ACCEPTING f.", 0)]
    [InlineData("in the context of surfacing a problem we decided c rather than d to achieve e accepting f", 1)]
    public void Moves_are_matched_as_whole_words_and_without_case(string text, int missing)
        => Assert.Equal(missing, YStatementPresent.MissingMoves(text).Count);

    // -- alternatives-have-verdicts: one finding per open bullet, quoting it --

    [Fact]
    public void An_alternative_left_open_is_quoted_back_and_a_settled_one_is_not()
    {
        var found = Run(new AlternativesHaveVerdicts(),
            Adr("## Alternatives Considered\n\n"
                + "* **A message queue** — we might explore this in a future revision.\n"
                + "* **A second database** — rejected: one is enough.\n"));

        Assert.Equal("alternatives-verdict", Single(found).Check.Value);
        Assert.Contains("A message queue", Single(found).Message);
    }

    // The bullets are found by heading, so a list under any other heading is not this rule's business.
    [Fact]
    public void Bullets_outside_the_section_are_left_alone()
        => Assert.Empty(Run(new AlternativesHaveVerdicts(),
            Adr("## Context\n\n* **A message queue** — we might explore this in a future revision.\n")));

    [Theory]
    [InlineData("**Kafka** — rejected: too heavy for this.", true)]
    [InlineData("**Kafka** — we use RabbitMQ instead.", true)]
    [InlineData("**Kafka** — we might explore this later.", false)]
    public void A_verdict_is_an_outcome_word_or_a_contrastive_cue(string bullet, bool settled)
        => Assert.Equal(settled, AlternativesHaveVerdicts.HasVerdict(bullet));

    // -- terms-are-alphabetical --

    [Fact]
    public void Entries_in_order_are_left_alone()
        => Assert.Empty(Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Borrower\n\nOne.\n\n### Item\n\nTwo.\n\n### Title\n\nThree.")));

    // The message names the entry that moved and the one it belongs before.
    [Fact]
    public void An_entry_out_of_place_names_itself_and_where_it_belongs()
    {
        var found = Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Borrower\n\nOne.\n\n### Item\n\nTwo.\n\n### Branch\n\nThree."));

        Assert.Equal("terms-alphabetical", Single(found).Check.Value);
        Assert.Equal("'Branch' is out of order: it belongs before 'Item'.", Single(found).Message);
    }

    // Casing is the entry's own — a glossary holds `ADR` beside `Borrower` — so ordering by code point
    // would report every initialism as misplaced where a reader scans them as fine.
    [Fact]
    public void Casing_does_not_decide_the_order()
        => Assert.Empty(Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### ADR\n\nOne.\n\n### Borrower\n\nTwo.\n\n### corpus\n\nThree.")));

    // Each entry is judged against the one before it, so a file with two words in the wrong place
    // reports both rather than stopping at the first.
    [Fact]
    public void Every_entry_out_of_place_is_reported()
        => Assert.Equal(2, Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Item\n\nOne.\n\n### Borrower\n\nTwo.\n\n### Adr\n\nThree.")).Count);

    // -- driving a rule --

    private static List<Finding> Run(IDocumentRule rule, Doc doc, RuleSpec? spec = null)
    {
        var found = new List<Finding>();

        rule.Check(new RuleContext(doc, doc.Type!, spec ?? new RuleSpec { Id = rule.RuleId },
            new Report(doc.Rel, found)));
        return found;
    }

    private static Finding Single(List<Finding> found) => Assert.Single(found);

    // A rule reads a parsed document and nothing else, so the fixture is the markdown itself. The
    // schema is empty because no rule here asks anything of the type beyond having one.
    private static Doc Adr(string body)
    {
        var text = $"---\nid: adr-0001\nstatus: accepted\n---\n\n# A title\n\n`ADR: adr-0001` `ACCEPTED`\n\n{body}\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());
        Assert.NotNull(doc);
        return doc;
    }
}
