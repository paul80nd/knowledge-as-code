using kac.core;

namespace kac.tests;

public class SubstanceTests
{
    private const string Before = """
        ---
        id: adr-0001
        type: adr
        status: accepted
        decided-on: "2026-01-05"
        ---

        # Keep records in git

        `ADR: adr-0001` `ACCEPTED`

        ## Decision

        We keep every record in git, reviewed through a pull request. See [the guide](guide.md) for the steps.

        ## Consequences

        Every change has an author and a reviewer.
        """;

    [Fact]
    public void A_record_with_the_same_text_has_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before)));

    [Fact]
    public void A_reflow_is_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before.Replace(
            "reviewed through a pull request. See", "reviewed through a pull\nrequest.   See"))));

    [Fact]
    public void A_new_link_target_is_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before.Replace("(guide.md)", "(docs/guide.md#steps)"))));

    [Fact]
    public void A_new_reference_link_label_and_its_definition_are_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before
            .Replace("[the guide](guide.md)", "[the guide][guide]")
            .Replace("a reviewer.", "a reviewer.\n\n[guide]: docs/guide.md"))));

    [Fact]
    public void A_typo_fixed_in_each_of_several_words_is_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before.Replace("record in", "recrod in").Replace("author", "autor")),
            Adr(Before)));

    [Fact]
    public void A_word_changed_by_more_than_two_edits_is_a_change_to_its_section()
    {
        var change = Assert.Single(Substance.Changes(Adr(Before), Adr(Before.Replace("git,", "a wiki,"))));
        Assert.Equal("`## Decision`", change.Part);
        Assert.Equal(12, change.Line);
    }

    [Fact]
    public void A_word_added_is_a_change_even_when_every_word_is_close()
        => Assert.Equal("`## Consequences`", Assert.Single(Substance.Changes(Adr(Before),
            Adr(Before.Replace("and a reviewer", "and a a reviewer")))).Part);

    [Fact]
    public void Swapped_words_are_a_change()
        => Assert.Single(Substance.Changes(Adr(Before),
            Adr(Before.Replace("an author and a reviewer", "a reviewer and an author"))));

    [Fact]
    public void A_new_title_is_a_change_above_the_first_section()
        => Assert.Equal("the title and opening", Assert.Single(Substance.Changes(Adr(Before),
            Adr(Before.Replace("# Keep records in git", "# Keep records in a wiki")))).Part);

    [Fact]
    public void A_status_move_and_the_identity_line_that_follows_it_are_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before
            .Replace("status: accepted", "status: superseded\nsuperseded-by: adr-0002")
            .Replace("`ACCEPTED`", "`SUPERSEDED`"))));

    [Fact]
    public void A_review_date_moved_or_added_is_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before
            .Replace("decided-on: \"2026-01-05\"", "decided-on: \"2026-01-05\"\nreview-by: \"2027-01-05\""))));

    [Fact]
    public void A_warning_accepted_in_frontmatter_is_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before), Adr(Before.Replace("decided-on: \"2026-01-05\"",
            "decided-on: \"2026-01-05\"\naccepts:\n  - check: y-statement\n    reason: \"Written before the rule.\""))));

    [Fact]
    public void Any_other_frontmatter_move_is_a_change_naming_the_field()
    {
        var change = Assert.Single(Substance.Changes(Adr(Before), Adr(Before.Replace("2026-01-05", "2026-02-05"))));
        Assert.Equal("`decided-on`", change.Part);
        Assert.Equal(1, change.Line);
    }

    [Fact]
    public void A_frontmatter_field_added_is_a_change()
        => Assert.Equal("`tags`", Assert.Single(Substance.Changes(Adr(Before),
            Adr(Before.Replace("status: accepted", "status: accepted\ntags: [git]")))).Part);

    [Fact]
    public void A_section_added_is_a_change_naming_it()
        => Assert.Equal("`## Notes` added", Assert.Single(Substance.Changes(Adr(Before),
            Adr(Before + "\n## Notes\n\nOne more thing.\n"))).Part);

    [Fact]
    public void A_section_renamed_is_one_removed_and_one_added()
        => Assert.Equal(["`## Consequences` removed", "`## Outcomes` added"],
            Substance.Changes(Adr(Before), Adr(Before.Replace("## Consequences", "## Outcomes")))
                .Select(c => c.Part));

    [Fact]
    public void A_table_realigned_around_a_longer_link_target_is_no_change()
        => Assert.Empty(Substance.Changes(
            Adr(Before + "\n\n| When | What |\n|------|------|\n| 9:00 | [down](a.md) |\n"),
            Adr(Before + "\n\n| When | What                      |\n|------|---------------------------|\n"
                + "| 9:00 | [down](runbooks/outage.md) |\n")));

    [Fact]
    public void A_corrected_autolink_is_no_change()
        => Assert.Empty(Substance.Changes(Adr(Before.Replace("a reviewer.", "a reviewer. <https://old.example/spec>")),
            Adr(Before.Replace("a reviewer.", "a reviewer. <https://new.example/spec/v2>"))));

    [Fact]
    public void Sections_sharing_a_title_pair_in_order_and_one_deleted_is_a_change()
    {
        var two = Before + "\n\n## Notes\n\nThe first note.\n\n## Notes\n\nThe second note.\n";
        Assert.Empty(Substance.Changes(Adr(two), Adr(two)));
        Assert.Equal("`## Notes` removed", Assert.Single(Substance.Changes(Adr(two),
            Adr(Before + "\n\n## Notes\n\nThe first note.\n"))).Part);
    }

    private static Doc Adr(string text) => Required.Parsed("adrs/0001-keep-records-in-git.md", text, new Schema());
}
