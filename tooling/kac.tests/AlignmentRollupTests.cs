using kac.core;

// `alignment-rollup` reports two faults under one id, and the coverage gate reads ids. So a fixture
// tripping either one turns the whole check green, and these are what hold the other branch honest.
//
// The cell reader gets its own tests below. What it has to get right is a boundary the flattened cell
// no longer marks, and every framework label in the corpus with a dot in it is a case for it.

namespace kac.tests;

public class AlignmentRollupTests
{
    [Fact]
    public void A_roll_up_matching_the_table_is_left_alone()
        => Assert.Empty(Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.5.17, A.8.24]",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |",
            "| `KEYS`  | **MUST** protect keys. | [ISO 27001:2022].A.8.24 |"));

    [Fact]
    public void A_reference_the_table_cites_and_the_roll_up_omits_is_named()
    {
        var found = Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.5.17]",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |",
            "| `KEYS`  | **MUST** protect keys. | [ISO 27001:2022].A.8.24 |");

        Assert.Equal("alignment-rollup", Assert.Single(found).Check.Value);
        Assert.Equal("'ISO 27001:2022.A.8.24' is cited in the clause table and missing from 'aligns-with'.",
            Assert.Single(found).Message);
    }

    [Fact]
    public void A_reference_the_roll_up_claims_and_no_clause_cites_is_named()
    {
        var found = Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.5.17, A.8.24]",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |");

        Assert.Equal("'aligns-with' claims 'ISO 27001:2022.A.8.24', and no clause cites it.",
            Assert.Single(found).Message);
    }

    // The finding points at the clause, because that is where the author reads the reference they have
    // to roll up. The other direction has no clause to point at and falls back to the field, which is
    // line 3 of the fixture below.
    [Fact]
    public void The_table_side_reports_against_the_clause_citing_it()
        => Assert.Equal(14, Assert.Single(Run(
            "aligns-with:",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |")).Line);

    [Fact]
    public void The_roll_up_side_reports_against_the_field()
        => Assert.Equal(3, Assert.Single(Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.5.17]",
            "| `ZEROSEC` | COULD reach zero secrets. | |")).Line);

    // A framework cited whole compares as the framework alone, on both sides.
    [Fact]
    public void A_framework_cited_with_no_reference_answers_an_entry_with_no_clauses()
        => Assert.Empty(Run(
            "aligns-with:\n  - framework: WCAG 2.2 AA",
            "| `CONFORM` | **MUST** conform. | [WCAG 2.2 AA] |"));

    [Fact]
    public void A_framework_cited_whole_is_not_answered_by_a_reference_into_it()
        => Assert.Equal(2, Run(
            "aligns-with:\n  - framework: WCAG 2.2 AA\n    clauses: [§9]",
            "| `CONFORM` | **MUST** conform. | [WCAG 2.2 AA] |").Count);

    // Six clauses citing one control is one fact to roll up, and one finding where it is missing.
    [Fact]
    public void A_reference_cited_by_several_clauses_is_reported_once()
        => Assert.Single(Run(
            "aligns-with:",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |",
            "| `LOGS`  | **MUST NOT** log one. | [ISO 27001:2022].A.5.17 |"));

    [Fact]
    public void A_policy_citing_nothing_and_claiming_nothing_is_left_alone()
        => Assert.Empty(Run("aligns-with:", "| `ZEROSEC` | COULD reach zero secrets. | |"));

    // A cell holds several mappings, and the reader has to end each label's reference at the comma.
    [Fact]
    public void Several_mappings_in_one_cell_are_read_apart()
        => Assert.Empty(Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.8.31]\n"
            + "  - framework: NIST SSDF 1.1\n    clauses: [PO.5]",
            "| `GATE` | **MUST** gate the pipeline. | [ISO 27001:2022].A.8.31, [NIST SSDF 1.1].PO.5 |"));

    // The case the flattened cell cannot answer alone. `NIST SSDF 1.1.PO.5` gives no clue where the
    // framework ends, and reading it as text would take `NIST SSDF 1` or `NIST SSDF 1.1.PO`.
    [Theory]
    [InlineData("NIST SSDF 1.1", "PO.5")]
    [InlineData("NIST AI RMF 1.0", "MAP")]
    [InlineData("WCAG 2.2 AA", "§9")]
    [InlineData("ISO 27001:2022", "A.8.24")]
    [InlineData("UK GDPR", "Art.5(1)(e)")]
    public void A_label_holding_a_dot_still_ends_where_the_label_ends(string framework, string reference)
        => Assert.Equal([$"{framework}.{reference}"],
            Alignment.References($"{framework}.{reference}", [framework]));

    [Fact]
    public void A_label_with_nothing_after_it_reads_as_the_framework_alone()
        => Assert.Equal(["WCAG 2.2 AA"], Alignment.References("WCAG 2.2 AA", ["WCAG 2.2 AA"]));

    [Fact]
    public void A_label_the_flattened_cell_does_not_hold_is_taken_at_its_word()
        => Assert.Equal(["ISO 27001:2022"], Alignment.References("", ["ISO 27001:2022"]));

    // Each label is found from where the last one ended, so a framework citing itself twice in one cell
    // does not resolve both labels onto the first mention.
    [Fact]
    public void A_label_repeated_in_one_cell_reads_each_mention()
        => Assert.Equal(["ISO 27001:2022.A.5.17", "ISO 27001:2022.A.8.24"],
            Alignment.References("ISO 27001:2022.A.5.17, ISO 27001:2022.A.8.24",
                ["ISO 27001:2022", "ISO 27001:2022"]));

    // The definitions at the foot are not decoration. Markdig resolves a shortcut reference only where
    // one is defined, so a cell written without them parses as literal brackets and the rule reads no
    // links at all. Every policy in the corpus carries them, and a fixture that did not would be
    // testing a document nobody writes.
    private static List<Finding> Run(string frontmatter, params string[] rows)
    {
        var table = "| Id | Clause | Alignment |\n|----|--------|-----------|\n"
                    + string.Join("\n", rows) + "\n";

        var definitions = string.Join("\n", Labels(rows).Select(l => $"[{l}]: ../frameworks.md#f"));
        var text = $"---\nid: pol-SCRT\n{frontmatter}\n---\n\n# Secrets are managed\n\n"
                   + $"`Policy: pol-SCRT` `DRAFT`\n\n## Clauses\n\n{table}\n{definitions}\n";

        var doc = Doc.Parse("policies/scrt-secrets.md", text, WithClauses());
        Assert.NotNull(doc);

        var found = new List<Finding>();
        var rule = new AlignmentRollup();
        rule.Check(new RuleContext(doc, doc.Type!, new RuleSpec { Id = rule.RuleId },
            new Report(doc.Rel, found)));
        return found;
    }

    // Every label the rows cite, once each, in the order they appear.
    private static IEnumerable<string> Labels(IEnumerable<string> rows) =>
        rows.SelectMany(r => System.Text.RegularExpressions.Regex.Matches(r, @"\[([^\]]+)\]")
                .Select(m => m.Groups[1].Value))
            .Distinct(StringComparer.Ordinal);

    private static Schema WithClauses() => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["policies"] = new()
            {
                IdPrefix = "pol",
                Parts = new PartSpec(PartSpec.Table, "", ["MUST"], ["COULD"])
                {
                    Section = "Clauses",
                    Columns = ["Id", "Clause", "Alignment"]
                }
            }
        }
    };
}
