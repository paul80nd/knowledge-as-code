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
    public void A_record_whose_id_changed_at_its_own_path_is_not_reported_as_deleted()
        => Assert.Empty(Run([Fix("fixes/goldens-disagree.md", "fix-0003")],
            [Fix("fixes/goldens-disagree.md", "fix-goldens-disagree")]));

    [Fact]
    public void A_record_git_reports_as_moved_is_not_reported_as_deleted()
        => Assert.Empty(Run([Nfr("nfrs/0001-latency.md", "nfr-0001")], [Nfr("nfrs/latency.md", "nfr-latency")],
            Moved("nfrs/0001-latency.md", "nfrs/latency.md")));

    [Fact]
    public void A_record_deleted_while_in_force_is_told_to_retire_it_beside_a_new_record_elsewhere()
        => Assert.Equal(new CheckId("retire-not-delete"), Assert.Single(Run(
            [Nfr("nfrs/0001-latency.md", "nfr-0001")], [Nfr("nfrs/refunds.md", "nfr-refunds")])).Check);

    [Fact]
    public void A_record_moved_onto_an_id_the_base_already_holds_is_told_to_retire_it()
        => Assert.Equal("nfrs/0001-latency.md", Assert.Single(Run(
            [Nfr("nfrs/0001-latency.md", "nfr-0001"), Nfr("nfrs/0002-refunds.md", "nfr-0002")],
            [Nfr("nfrs/latency.md", "nfr-0002")], Moved("nfrs/0001-latency.md", "nfrs/latency.md"))).File);

    /// <summary><c>nfrs/0002-refunds.md</c> is unchanged, so the base read leaves it out.</summary>
    [Fact]
    public void A_record_moved_onto_an_id_an_unchanged_record_holds_is_told_to_retire_it()
        => Assert.Equal("nfrs/0001-latency.md", Assert.Single(Run(
            [Nfr("nfrs/0001-latency.md", "nfr-0001")],
            [Nfr("nfrs/latency.md", "nfr-0002"), Nfr("nfrs/0002-refunds.md", "nfr-0002")],
            Moved("nfrs/0001-latency.md", "nfrs/latency.md"))).File);

    [Fact]
    public void A_published_postmortem_citing_a_renamed_record_by_its_new_id_passes()
        => Assert.Empty(Run(
            [Nfr("nfrs/0001-latency.md", "nfr-0001"), Cites("nfr-0001", "[nfr-0001]")],
            [Nfr("nfrs/latency.md", "nfr-latency"), Cites("nfr-latency", "[nfr-latency]")],
            Moved("nfrs/0001-latency.md", "nfrs/latency.md")));

    [Fact]
    public void A_published_postmortem_citing_another_record_in_place_of_a_renamed_one_fails()
    {
        var finding = Assert.Single(Run(
            [Nfr("nfrs/0001-latency.md", "nfr-0001"), Cites("nfr-0001", "[nfr-0001]")],
            [Nfr("nfrs/latency.md", "nfr-latency"), Cites("nfr-refund-settlement", "[nfr-refund-settlement]")],
            Moved("nfrs/0001-latency.md", "nfrs/latency.md")));

        Assert.Equal(new CheckId("immutable-after-published"), finding.Check);
        Assert.Contains("since: `prompted`, `## Summary`.", finding.Message);
    }

    [Fact]
    public void A_renamed_id_written_outside_a_link_is_a_change_in_substance()
        => Assert.Contains("since: `## Summary`.", Assert.Single(Run(
            [Nfr("nfrs/0001-latency.md", "nfr-0001"), Cites("nfr-0001", "`nfr-0001`")],
            [Nfr("nfrs/latency.md", "nfr-latency"), Cites("nfr-latency", "`nfr-latency`")],
            Moved("nfrs/0001-latency.md", "nfrs/latency.md"))).Message);

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

        Assert.Equal(new CheckId("clause-id-kept"), finding.Check);
        Assert.Equal(Sev.Error, finding.Severity);
        Assert.Equal("policies/vurm-records.md", finding.File);
        Assert.Equal(
            "this policy was `active` at origin/main, and its clause `LOCAL` is gone. A citation of pol-VURM.LOCAL "
            + "may outlive it, so restore the clause, or retire pol-VURM and write the change in a new policy.",
            finding.Message);
        Assert.Equal(11, finding.Line);
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

        Assert.Equal(new CheckId("rule-id-kept"), finding.Check);
        Assert.Equal("standards/testing.md", finding.File);
        Assert.Equal(
            "this standard was `active` at origin/main, and its rule `FAST` is gone. A citation of std-TEST.FAST "
            + "may outlive it, so restore the rule, or write the change in a new standard that names std-TEST in "
            + "`supersedes`.",
            finding.Message);
    }

    [Fact]
    public void A_rule_whose_id_no_longer_parses_is_left_to_the_format_check()
        => Assert.Empty(Run(
            [Standard("active", "Testing", ("CLEAN", "A clean record reports nothing"), ("FAST", "Tests run fast"))],
            [Standard("active", "Testing", ("CLEAN", "A clean record reports nothing"), ("Fast", "Tests run fast"))]));

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

    private static List<Finding> Run(IReadOnlyList<Doc> before, IReadOnlyList<Doc> after,
        IReadOnlyDictionary<string, string>? moved = null)
    {
        var found = new List<Finding>();
        Validator.CheckHistory(Schema, after, new Baseline("origin/main", before, moved ?? Moved()), found);
        return found;
    }

    /// <summary>What git reports as renamed, from the path at the base to the path now.</summary>
    private static Dictionary<string, string> Moved(params string[] pairs)
    {
        var moved = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i + 1 < pairs.Length; i += 2) moved[pairs[i]] = pairs[i + 1];
        return moved;
    }

    private static Doc Fix(string rel, string id) => Required.Parsed(rel, $"""
        ---
        id: {id}
        type: fix
        status: active
        ---

        # Goldens disagree

        `Fix: {id}` `ACTIVE`
        """, Schema);

    private static Doc Nfr(string rel, string id) => Required.Parsed(rel, $"""
        ---
        id: {id}
        type: nfr
        status: agreed
        ---

        # Authorisation latency

        `NFR: {id}` `AGREED`
        """, Schema);

    /// <summary>A published postmortem naming <paramref name="prompted"/>, and citing it in its Summary.</summary>
    private static Doc Cites(string prompted, string citation) => Required.Parsed("postmortems/0002-slow.md", $"""
        ---
        id: pmt-0002
        type: postmortem
        status: published
        prompted:
          - {prompted}
        ---

        # Authorisations ran slow

        `Postmortem: pmt-0002` `PUBLISHED`

        ## Summary

        Authorisations ran past the target {citation} sets.

        [{prompted}]: ../nfrs/{prompted}.md
        """, Schema);

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
