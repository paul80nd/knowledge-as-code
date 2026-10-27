using kac.core;

namespace kac.tests;

public class HistoryTests
{
    private static readonly Schema Schema = Schema.Load(Repo.Root);

    [Fact]
    public void Every_registered_history_rule_has_a_distinct_id_and_reports_something()
    {
        Assert.Equal(HistoryRules.All.Count, HistoryRules.ByRuleId.Count);
        Assert.All(HistoryRules.All, r => Assert.NotEmpty(r.Emits));
    }

    [Fact]
    public void A_decision_rewritten_after_acceptance_fails_naming_the_section()
    {
        var finding = Assert.Single(Run(
            [Adr("adrs/0001-git.md", "accepted", "We keep records in git.")],
            [Adr("adrs/0001-git.md", "accepted", "We keep records in a wiki instead.")]));

        Assert.Equal(new CheckId("immutable-after-accepted"), finding.Check);
        Assert.Equal(Sev.Error, finding.Severity);
        Assert.Equal("adrs/0001-git.md", finding.File);
        Assert.StartsWith(
            "this ADR was accepted at origin/main, and has changed by more than a typo since: `## Decision`.",
            finding.Message);
    }

    [Fact]
    public void A_rejected_decision_is_held_as_an_accepted_one_is()
        => Assert.Contains("was rejected at origin/main", Assert.Single(Run(
            [Adr("adrs/0001-git.md", "rejected", "We keep records in git.")],
            [Adr("adrs/0001-git.md", "rejected", "We keep records in a wiki instead.")])).Message);

    [Fact]
    public void A_proposed_decision_changes_freely()
        => Assert.Empty(Run(
            [Adr("adrs/0001-git.md", "proposed", "We keep records in git.")],
            [Adr("adrs/0001-git.md", "accepted", "We keep records in a wiki instead.")]));

    [Fact]
    public void A_record_is_paired_by_its_id_so_a_moved_file_is_compared_where_it_went()
        => Assert.Equal("adrs/0001-records.md", Assert.Single(Run(
            [Adr("adrs/0001-git.md", "accepted", "We keep records in git.")],
            [Adr("adrs/0001-records.md", "accepted", "We keep records in a wiki instead.")])).File);

    [Fact]
    public void A_moved_file_whose_text_is_unchanged_passes()
        => Assert.Empty(Run(
            [Adr("adrs/0001-git.md", "accepted", "We keep records in git.")],
            [Adr("adrs/0001-records.md", "accepted", "We keep records in git.")]));

    [Fact]
    public void A_published_postmortem_rewritten_fails()
    {
        var finding = Assert.Single(Run(
            [Postmortem("published", "A certificate expired.")],
            [Postmortem("published", "A deploy removed the certificate.")]));

        Assert.Equal(new CheckId("immutable-after-published"), finding.Check);
        Assert.StartsWith(
            "this postmortem was published at origin/main, and has changed by more than a typo since: `## Summary`.",
            finding.Message);
    }

    [Fact]
    public void A_record_deleted_while_in_force_is_told_to_retire_it()
    {
        var finding = Assert.Single(Run([Adr("adrs/0001-git.md", "accepted", "We keep records in git.")], []));

        Assert.Equal(new CheckId("retire-not-delete"), finding.Check);
        Assert.Equal(Sev.Warning, finding.Severity);
        Assert.Equal("adrs/0001-git.md", finding.File);
        Assert.Contains("adr-0001", finding.Message);
    }

    [Fact]
    public void A_record_whose_frontmatter_no_longer_parses_is_not_reported_as_deleted()
        => Assert.Empty(Run([Adr("adrs/0001-git.md", "accepted", "We keep records in git.")],
            [Required.Parsed("adrs/0001-git.md", "---\nid: [adr-0001\n---\n\n# Keep records in git\n", Schema)]));

    [Theory]
    [InlineData("proposed")]
    [InlineData("superseded")]
    [InlineData("deprecated")]
    public void A_record_deleted_while_out_of_force_reports_nothing(string status)
        => Assert.Empty(Run([Adr("adrs/0001-git.md", status, "We keep records in git.")], []));

    private static List<Finding> Run(IReadOnlyList<Doc> before, IReadOnlyList<Doc> after)
    {
        var found = new List<Finding>();
        Validator.CheckHistory(Schema, after, new Baseline("origin/main", before), found);
        return found;
    }

    private static Doc Adr(string rel, string status, string decision) => Required.Parsed(rel, $"""
        ---
        id: adr-0001
        type: adr
        status: {status}
        ---

        # Keep records in git

        `ADR: adr-0001` `{status.ToUpperInvariant()}`

        ## Decision

        {decision}
        """, Schema);

    private static Doc Postmortem(string status, string summary) => Required.Parsed("postmortems/0001-expired.md", $"""
        ---
        id: pmt-0001
        type: postmortem
        status: {status}
        ---

        # The certificate expired

        `Postmortem: pmt-0001` `{status.ToUpperInvariant()}`

        ## Summary

        {summary}
        """, Schema);
}
