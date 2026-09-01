using kac.core;

// `alignment-rollup` reports two faults under one id and `framework-posture` under another, and the
// coverage gate reads ids. So a fixture tripping any one of them turns that id green. These are what
// hold the rest honest.
//
// The cell reader gets its own tests at the foot. What it has to get right is a boundary the flattened
// cell no longer marks, and every framework label in the corpus with a dot in it is a case for it.

namespace kac.tests;

public class AlignmentRollupTests
{
    [Fact]
    public void A_roll_up_matching_the_binding_references_is_left_alone()
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

    // The standing decides whether a citation is an obligation or a note about where an idea came from.
    // Only the first belongs in a summary read as coverage.
    [Fact]
    public void A_framework_that_binds_nothing_is_cited_freely_and_rolled_up_never()
        => Assert.Empty(Run(
            "aligns-with:",
            "| `SPEND` | SHOULD watch the spend. | [Azure WAF].cost-optimization |"));

    [Fact]
    public void A_roll_up_claiming_a_framework_that_binds_nothing_is_reported()
        => Assert.Equal("'aligns-with' claims 'Azure WAF.cost-optimization', and no clause cites it.",
            Assert.Single(Run(
                "aligns-with:\n  - framework: Azure WAF\n    clauses: [cost-optimization]",
                "| `SPEND` | SHOULD watch the spend. | [Azure WAF].cost-optimization |")).Message);

    // Self-obligated binds because a policy of ours says so, so it rolls up like an obliged one.
    [Fact]
    public void A_self_obligated_framework_binds()
        => Assert.Empty(Run(
            "aligns-with:\n  - framework: WCAG 2.2 AA",
            "| `CONFORM` | **MUST** conform. | [WCAG 2.2 AA] |"));

    [Fact]
    public void A_framework_the_register_does_not_place_is_reported()
    {
        var found = Assert.Single(Run(
            "aligns-with:",
            "| `NEW` | **MUST** do the thing. | [FinOps 2024].cost |"));

        Assert.Equal("framework-posture", found.Check.Value);
        Assert.Equal("'FinOps 2024' is cited here and nothing says what our standing against it is. "
                     + "File it under 'Obliged' or 'Self-obligated', or under a standing that binds nothing.",
            found.Message);
    }

    // The fix is one entry on the register, so a policy citing the same unplaced framework twice has one
    // fault rather than two.
    [Fact]
    public void An_unplaced_framework_is_reported_once_however_many_clauses_cite_it()
        => Assert.Single(Run(
            "aligns-with:",
            "| `ONE` | **MUST** do one thing. | [FinOps 2024].cost |",
            "| `TWO` | **MUST** do another.   | [FinOps 2024].waste |"));

    // A heading above the first standing sits under none, which is a register that offers the anchor and
    // places nothing.
    [Fact]
    public void A_framework_heading_under_no_standing_is_unplaced()
        => Assert.Equal("framework-posture", Assert.Single(Run(
            "aligns-with:",
            "| `LOOSE` | **MUST** do the thing. | [Loose Framework].x |")).Check.Value);

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

    // A cell holds several mappings, and the reader has to end each label's reference at the comma. The
    // second one binds nothing, so only the first reaches the roll-up.
    [Fact]
    public void Several_mappings_in_one_cell_are_read_apart()
        => Assert.Empty(Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.8.31]",
            "| `GATE` | **MUST** gate the pipeline. | [ISO 27001:2022].A.8.31, [Azure WAF].reliability |"));

    // The case the flattened cell cannot answer alone. `NIST SSDF 1.1.PO.5` gives no clue where the
    // framework ends, and reading it as text would take `NIST SSDF 1` or `NIST SSDF 1.1.PO`.
    [Theory]
    [InlineData("NIST SSDF 1.1", "PO.5")]
    [InlineData("NIST AI RMF 1.0", "MAP")]
    [InlineData("WCAG 2.2 AA", "§9")]
    [InlineData("ISO 27001:2022", "A.8.24")]
    [InlineData("UK GDPR", "Art.5(1)(e)")]
    public void A_label_holding_a_dot_still_ends_where_the_label_ends(string framework, string reference)
        => Assert.Equal([(framework, reference)],
            Alignment.References($"{framework}.{reference}", [framework]));

    [Fact]
    public void A_label_with_nothing_after_it_reads_as_the_framework_alone()
        => Assert.Equal([("WCAG 2.2 AA", (string?)null)],
            Alignment.References("WCAG 2.2 AA", ["WCAG 2.2 AA"]));

    [Fact]
    public void A_label_the_flattened_cell_does_not_hold_is_taken_at_its_word()
        => Assert.Equal([("ISO 27001:2022", (string?)null)], Alignment.References("", ["ISO 27001:2022"]));

    // Each label is found from where the last one ended, so a framework citing itself twice in one cell
    // does not resolve both labels onto the first mention.
    [Fact]
    public void A_label_repeated_in_one_cell_reads_each_mention()
        => Assert.Equal([("ISO 27001:2022", "A.5.17"), ("ISO 27001:2022", "A.8.24")],
            Alignment.References("ISO 27001:2022.A.5.17, ISO 27001:2022.A.8.24",
                ["ISO 27001:2022", "ISO 27001:2022"]));

    // The register the rule reads, and the whole of what a standing is: the `##` a framework's own
    // heading sits under. `Loose Framework` sits above the first one deliberately.
    private const string Frameworks = """
                                      # Frameworks

                                      ### Loose Framework

                                      Above every standing, so it sits under none.

                                      ## Obliged

                                      ### ISO 27001

                                      Registered against it.

                                      ## Self-obligated

                                      ### WCAG

                                      A policy of ours holds us to it.

                                      ## Inspiration

                                      ### Azure Well-Architected Framework

                                      Ideas taken, nothing bound.
                                      """;

    // The definitions at the foot are not decoration. Markdig resolves a shortcut reference only where
    // one is defined, so a cell written without them parses as literal brackets and the rule reads no
    // links at all. They are also how the rule reaches the register, because a label names the page and
    // the heading on it.
    private static List<Finding> Run(string frontmatter, params string[] rows)
    {
        var table = "| Id | Clause | Alignment |\n|----|--------|-----------|\n"
                    + string.Join("\n", rows) + "\n";

        var definitions = string.Join("\n",
            Labels(rows).Select(l => $"[{l}]: ../frameworks.md#{Anchor(l)}"));

        var text = $"---\nid: pol-SCRT\n{frontmatter}\n---\n\n# Secrets are managed\n\n"
                   + $"`Policy: pol-SCRT` `DRAFT`\n\n## Clauses\n\n{table}\n{definitions}\n";

        var doc = Doc.Parse("policies/scrt-secrets.md", text, WithClauses());
        Assert.NotNull(doc);

        var files = new HashSet<string>(["frameworks.md", "policies/scrt-secrets.md"], StringComparer.Ordinal);
        var tree = new Tree(files, rel => rel == "frameworks.md" ? Frameworks : text);

        var found = new List<Finding>();
        new AlignmentRollup().Check(new CorpusRuleContext(
            [doc], new Dictionary<string, Doc> { ["pol-SCRT"] = doc }, tree, doc.TypeOf(),
            new RuleSpec { Id = new RuleId("alignment-rollup"), Postures = ["Obliged", "Self-obligated"] },
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));

        return found;
    }

    // The anchor a label points at, which is the framework's heading without the version the label
    // carries. `FinOps 2024` deliberately names a heading the register does not hold.
    private static string Anchor(string label) => label switch
    {
        "ISO 27001:2022" => "iso-27001",
        "WCAG 2.2 AA" => "wcag",
        "Azure WAF" => "azure-well-architected-framework",
        "Loose Framework" => "loose-framework",
        _ => "not-on-the-register"
    };

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
                Parts = new PartSpec(PartSpec.Table, "", ["MUST"], ["SHOULD", "COULD"])
                {
                    Section = "Clauses",
                    Columns = ["Id", "Clause", "Alignment"]
                }
            }
        }
    };
}
