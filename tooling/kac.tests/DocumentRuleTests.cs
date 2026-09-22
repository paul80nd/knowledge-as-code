using kac.core;

// A rule that needs C# is a class with one method, so each of its ways to fail can be driven directly
// rather than only through a whole corpus. That matters more here than elsewhere: the coverage gate
// reads check ids, not branches, so a rule reporting three faults under one id is green once a fixture
// trips any one of them. These tests are what hold the other two honest.

namespace kac.tests;

public class DocumentRuleTests
{
    [Fact]
    public void Every_registered_rule_has_a_distinct_id_and_reports_something()
    {
        Assert.Equal(DocumentRules.All.Count, DocumentRules.ByRuleId.Count);
        Assert.All(DocumentRules.All, r => Assert.NotEmpty(r.Emits));
    }

    // What each emitted id *means* is `_checks.yaml`'s to say.
    [Fact]
    public void Every_emitted_id_is_a_usable_check_id()
    {
        Assert.All(DocumentRules.All.SelectMany(r => r.Emits), e => Assert.NotEmpty(e.Value));
    }

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

    // A corpus can tune the ceiling without a release, and the default applies only where the schema
    // declares none.
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

    // The bullets are found by heading.
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

    [Fact]
    public void Entries_in_order_are_left_alone()
        => Assert.Empty(Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Borrower\n\nOne.\n\n### Item\n\nTwo.\n\n### Title\n\nThree.")));

    [Fact]
    public void An_entry_out_of_place_names_itself_and_where_it_belongs()
    {
        var found = Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Borrower\n\nOne.\n\n### Item\n\nTwo.\n\n### Branch\n\nThree."));

        Assert.Equal("terms-alphabetical", Single(found).Check.Value);
        Assert.Equal("'Branch' is out of order: it belongs before 'Item'.", Single(found).Message);
    }

    // Casing is the entry's own, and a glossary holds `ADR` beside `Borrower`. So ordering by code
    // point would report every initialism as misplaced where a reader scans them as fine.
    [Fact]
    public void Casing_does_not_decide_the_order()
        => Assert.Empty(Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### ADR\n\nOne.\n\n### Borrower\n\nTwo.\n\n### corpus\n\nThree.")));

    // Each entry is judged against the one before it.
    [Fact]
    public void Every_entry_out_of_place_is_reported()
        => Assert.Equal(2, Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Item\n\nOne.\n\n### Borrower\n\nTwo.\n\n### Adr\n\nThree.")).Count);

    [Fact]
    public void A_changelog_running_newest_first_is_left_alone()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-14: third.\n- 2026-09-02: second.\n- 2026-08-20: initial version.")));

    [Fact]
    public void An_entry_appended_to_the_foot_names_itself_and_the_entry_above_it()
    {
        var found = Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-02: second.\n- 2026-08-20: initial version.\n- 2026-09-14: third."));

        Assert.Equal("changelog-order", Single(found).Check.Value);
        Assert.Equal("changelog entry '2026-09-14' is out of order: it belongs above '2026-09-02'.",
            Single(found).Message);
    }

    // The entry moves to the top of the run it is newer than, not to the line above it.
    [Fact]
    public void An_entry_is_sent_above_the_first_entry_it_is_newer_than()
    {
        var found = Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-02: second.\n- 2026-08-30: also.\n- 2026-08-20: first.\n"
                + "- 2026-08-31: appended."));

        Assert.Equal("changelog entry '2026-08-31' is out of order: it belongs above '2026-08-30'.",
            Single(found).Message);
    }

    // Two entries on one day are one change written twice, and neither came before the other.
    [Fact]
    public void Two_entries_sharing_a_date_are_left_alone()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-02: one.\n- 2026-09-02: another.\n- 2026-08-20: initial version.")));

    // Each entry is judged against the one before it.
    [Fact]
    public void Every_entry_out_of_place_in_a_changelog_is_reported()
        => Assert.Equal(2, Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-08-20: one.\n- 2026-09-02: two.\n- 2026-09-14: three.")).Count);

    // A template's date is a placeholder, so it opens no entry and is passed over. Reporting one would
    // fail every corpus on the file it was sent to start from.
    [Fact]
    public void A_bullet_opening_on_something_other_than_a_date_is_passed_over()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- {{YYYY-MM-DD}}: initial version.\n- 2026-08-20: earlier.")));

    // The entries are found by heading.
    [Fact]
    public void Bullets_outside_the_changelog_are_left_alone()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Summary\n\n- 2026-08-20: one.\n- 2026-09-02: two.")));

    [Theory]
    [InlineData("2026-09-14: added a rule.", "2026-09-14")]
    [InlineData("  2026-09-14 : added a rule.", "2026-09-14")]
    [InlineData("{{YYYY-MM-DD}}: initial version.", null)]
    [InlineData("2026-9-14: added a rule.", null)]
    [InlineData("2026-09-14 added a rule.", null)]
    public void An_entry_opens_on_an_ISO_date_before_a_colon(string bullet, string? date)
        => Assert.Equal(date, ChangelogNewestFirst.OpeningDate(bullet));

    private static List<Finding> Run(IDocumentRule rule, Doc doc, RuleSpec? spec = null)
    {
        var found = new List<Finding>();

        // `RuleContext` asks for a type and no rule below reads one, so it is handed a bare stand-in
        // rather than the document's own. Parsing against an empty schema leaves that null.
        rule.Check(new RuleContext(doc, new TypeSchema(), spec ?? new RuleSpec { Id = rule.RuleId },
            new Report(doc.Rel, found)));
        return found;
    }

    private static Finding Single(List<Finding> found) => Assert.Single(found);

    // A rule reads a parsed document and nothing else, so the fixture is the markdown itself. The
    // schema is empty because no rule here asks anything of the type, which is why `Run` above can
    // hand the context a stand-in.
    private static Doc Adr(string body)
    {
        var text = $"---\nid: adr-0001\nstatus: accepted\n---\n\n# A title\n\n`ADR: adr-0001` `ACCEPTED`\n\n{body}\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());
        Assert.NotNull(doc);
        return doc;
    }
}
