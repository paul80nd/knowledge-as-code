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

    [Fact]
    public void A_clause_removed_from_an_active_policy_fails_naming_the_clause()
    {
        var finding = Assert.Single(Run(
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"), ("LOCAL", "link nothing"))],
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"))]));

        Assert.Equal(new CheckId("part-id-kept"), finding.Check);
        Assert.Equal(Sev.Error, finding.Severity);
        Assert.Equal("policies/vurm-records.md", finding.File);
        Assert.Equal(
            "this policy was `active` at origin/main, and its clause `LOCAL` is gone. A citation of pol-VURM.LOCAL "
            + "may outlive it, so restore the clause, and write the change in a new policy that supersedes pol-VURM.",
            finding.Message);
    }

    [Fact]
    public void A_clause_renamed_in_an_active_policy_fails_naming_the_old_id()
        => Assert.Contains("its clause `LOCAL` is gone", Assert.Single(Run(
            [Policy("active", "Keep records in git", ("LOCAL", "link nothing"))],
            [Policy("active", "Keep records in git", ("NOLINK", "link nothing"))])).Message);

    [Fact]
    public void Each_clause_removed_is_reported_on_its_own()
        => Assert.Equal(2, Run(
            [Policy("active", "Keep records in git", ("CLEAN", "keep this free"), ("LOCAL", "link nothing"),
                ("SHORT", "stay short"))],
            [Policy("active", "Keep records in git", ("CLEAN", "keep this free"))]).Count);

    [Fact]
    public void A_clause_added_to_an_active_policy_passes()
        => Assert.Empty(Run(
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"))],
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"), ("LOCAL", "link nothing"))]));

    [Fact]
    public void A_clause_reworded_and_a_policy_retitled_pass()
        => Assert.Empty(Run(
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"))],
            [Policy("active", "Keep every record in git", ("CLEAN", "keep this record clear of every finding"))]));

    [Fact]
    public void A_draft_policy_made_active_with_a_clause_removed_passes()
        => Assert.Empty(Run(
            [Policy("draft", "Keep records in git", ("CLEAN", "stay clean"), ("LOCAL", "link nothing"))],
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"))]));

    [Fact]
    public void A_retired_policy_loses_a_clause_freely()
        => Assert.Empty(Run(
            [Policy("retired", "Keep records in git", ("CLEAN", "stay clean"), ("LOCAL", "link nothing"))],
            [Policy("retired", "Keep records in git", ("CLEAN", "stay clean"))]));

    [Fact]
    public void A_policy_retired_in_the_same_change_that_drops_a_clause_fails()
        => Assert.Single(Run(
            [Policy("active", "Keep records in git", ("CLEAN", "stay clean"), ("LOCAL", "link nothing"))],
            [Policy("retired", "Keep records in git", ("CLEAN", "stay clean"))]));

    [Fact]
    public void A_rule_removed_from_an_active_standard_fails_naming_the_rule()
    {
        var finding = Assert.Single(Run(
            [Standard("active", "Testing", ("CLEAN", "A clean record reports nothing"), ("FAST", "Tests run fast"))],
            [Standard("active", "Testing", ("CLEAN", "A clean record reports nothing"))]));

        Assert.Equal(new CheckId("part-id-kept"), finding.Check);
        Assert.Equal("standards/testing.md", finding.File);
        Assert.Equal(
            "this standard was `active` at origin/main, and its rule `FAST` is gone. A citation of std-TEST.FAST "
            + "may outlive it, so restore the rule, and write the change in a new standard that supersedes std-TEST.",
            finding.Message);
    }

    [Fact]
    public void A_rule_renamed_in_an_active_standard_fails_naming_the_old_id()
        => Assert.Contains("its rule `FAST` is gone", Assert.Single(Run(
            [Standard("active", "Testing", ("FAST", "Tests run fast"))],
            [Standard("active", "Testing", ("QUICK", "Tests run fast"))])).Message);

    [Fact]
    public void A_rule_added_reworded_or_retitled_in_an_active_standard_passes()
        => Assert.Empty(Run(
            [Standard("active", "Testing", ("CLEAN", "A clean record reports nothing"))],
            [Standard("active", "Testing everywhere", ("CLEAN", "A clean record reports no finding at all"),
                ("FAST", "Tests run fast"))]));

    [Fact]
    public void A_draft_standard_made_active_with_a_rule_removed_passes()
        => Assert.Empty(Run(
            [Standard("draft", "Testing", ("CLEAN", "A clean record reports nothing"), ("FAST", "Tests run fast"))],
            [Standard("active", "Testing", ("CLEAN", "A clean record reports nothing"))]));

    [Theory]
    [InlineData("deprecated")]
    [InlineData("superseded")]
    public void A_standard_out_of_force_loses_a_rule_freely(string status)
        => Assert.Empty(Run(
            [Standard(status, "Testing", ("CLEAN", "A clean record reports nothing"), ("FAST", "Tests run fast"))],
            [Standard(status, "Testing", ("CLEAN", "A clean record reports nothing"))]));

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

    private static Doc Policy(string status, string title, params (string Id, string Clause)[] clauses)
        => Required.Parsed("policies/vurm-records.md", $"""
            ---
            id: pol-VURM
            type: policy
            status: {status}
            ---

            # {title}

            `Policy: pol-VURM` `{status.ToUpperInvariant()}`

            ## Clauses

            | Id | Clause | Alignment |
            |----|--------|-----------|
            {string.Join("\n", clauses.Select(c => $"| `{c.Id}` | **MUST** {c.Clause} | |"))}
            """, Schema);

    private static Doc Standard(string status, string title, params (string Id, string Heading)[] rules)
        => Required.Parsed("standards/testing.md", $"""
            ---
            id: std-TEST
            type: standard
            status: {status}
            ---

            # {title}

            `Standard: std-TEST` `{status.ToUpperInvariant()}`

            ## Rules

            {string.Join("\n\n", rules.Select(r => $"### {r.Id}: {r.Heading}\n\n- A test **MUST** pass."))}
            """, Schema);
}
