using kac.core;

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

    [Fact]
    public void An_unplaced_framework_is_reported_once_however_many_clauses_cite_it()
        => Assert.Single(Run(
            "aligns-with:",
            "| `ONE` | **MUST** do one thing. | [FinOps 2024].cost |",
            "| `TWO` | **MUST** do another.   | [FinOps 2024].waste |"));

    /// <summary>
    /// A cell citing a framework whose label nothing defines. Markdig leaves the brackets as text, so the row
    /// contributes no link and the citation is invisible to every other question the rule asks.
    /// </summary>
    [Fact]
    public void A_citation_whose_label_has_no_definition_is_reported()
    {
        var found = Assert.Single(Unlinked(
            "aligns-with:",
            "| `HOLD` | **MUST** hold personal data lawfully. | [UK GDPR].Art.5(1)(e) |"));

        Assert.Equal("alignment-unlinked", found.Check.Value);
        Assert.Equal("'[UK GDPR]' is cited here and has no link definition, so the cell states no "
                     + "mapping. Define the label against the register entry.", found.Message);
    }

    [Fact]
    public void An_undefined_label_is_reported_once_however_many_clauses_cite_it()
        => Assert.Single(Unlinked(
            "aligns-with:",
            "| `ONE` | **MUST** do one thing. | [UK GDPR].Art.5(1)(e) |",
            "| `TWO` | **MUST** do another.   | [UK GDPR].Art.32(1)(d) |"));

    [Fact]
    public void A_roll_up_claiming_a_framework_whose_citation_is_unread_is_left_alone()
        => Assert.Equal("alignment-unlinked", Assert.Single(Unlinked(
            "aligns-with:\n  - framework: UK GDPR\n    clauses: ['Art.5(1)(e)']",
            "| `HOLD` | **MUST** hold personal data lawfully. | [UK GDPR].Art.5(1)(e) |")).Check.Value);

    [Fact]
    public void A_resolved_reference_missing_from_the_roll_up_is_reported_beside_an_unread_one()
        => Assert.Equal(
            [
                "alignment-unlinked",
                "'ISO 27001:2022.A.5.17' is cited in the clause table and missing from 'aligns-with'."
            ],
            Unlinked("aligns-with:",
                    "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |",
                    "| `HOLD`  | **MUST** hold data lawfully. | [UK GDPR].Art.5(1)(e) |")
                .Select(f => f.Check.Value == "alignment-unlinked" ? f.Check.Value : f.Message));

    [Fact]
    public void A_framework_heading_under_no_standing_is_unplaced()
        => Assert.Equal("framework-posture", Assert.Single(Run(
            "aligns-with:",
            "| `LOOSE` | **MUST** do the thing. | [Loose Framework].x |")).Check.Value);

    [Fact]
    public void The_table_side_reports_against_the_clause_citing_it()
        => Assert.Equal(14, Assert.Single(Run(
            "aligns-with:",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |")).Line);

    /// <summary>The field is line 3 of the fixture below.</summary>
    [Fact]
    public void The_roll_up_side_reports_against_the_field()
        => Assert.Equal(3, Assert.Single(Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.5.17]",
            "| `ZEROSEC` | COULD reach zero secrets. | |")).Line);

    [Fact]
    public void A_reference_cited_by_several_clauses_is_reported_once()
        => Assert.Single(Run(
            "aligns-with:",
            "| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |",
            "| `LOGS`  | **MUST NOT** log one. | [ISO 27001:2022].A.5.17 |"));

    [Fact]
    public void A_policy_citing_nothing_and_claiming_nothing_is_left_alone()
        => Assert.Empty(Run("aligns-with:", "| `ZEROSEC` | COULD reach zero secrets. | |"));

    [Fact]
    public void Several_mappings_in_one_cell_are_read_apart()
        => Assert.Empty(Run(
            "aligns-with:\n  - framework: ISO 27001:2022\n    clauses: [A.8.31]",
            "| `GATE` | **MUST** gate the pipeline. | [ISO 27001:2022].A.8.31, [Azure WAF].reliability |"));

    /// <summary>
    /// <c>NIST SSDF 1.1.PO.5</c> gives no clue where the framework ends. Read as text, it would take <c>NIST SSDF 1</c>
    /// or <c>NIST SSDF 1.1.PO</c>.
    /// </summary>
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

    [Fact]
    public void A_label_repeated_in_one_cell_reads_each_mention()
        => Assert.Equal([("ISO 27001:2022", "A.5.17"), ("ISO 27001:2022", "A.8.24")],
            Alignment.References("ISO 27001:2022.A.5.17, ISO 27001:2022.A.8.24",
                ["ISO 27001:2022", "ISO 27001:2022"]));

    /// <summary>
    /// The register the rule reads. <c>Loose Framework</c> sits above the first standing deliberately.
    /// </summary>
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

    [Fact]
    public void A_framework_the_register_places_and_no_clause_cites_is_reported()
        => Assert.Equal(
            [
                "'Azure Well-Architected Framework' is filed on 'frameworks.md' and no clause cites it.",
                "'ISO 27001' is filed on 'frameworks.md' and no clause cites it."
            ],
            Uncited("aligns-with:\n  - framework: WCAG 2.2 AA",
                "| `CONFORM` | **MUST** conform. | [WCAG 2.2 AA] |").Select(f => f.Message));

    [Fact]
    public void An_uncited_framework_is_reported_against_the_record_that_reached_the_register()
    {
        var found = Uncited("aligns-with:\n  - framework: WCAG 2.2 AA",
            "| `CONFORM` | **MUST** conform. | [WCAG 2.2 AA] |").First();

        Assert.Equal("policies/scrt-secrets.md", found.File);
        Assert.Null(found.Line);
    }

    [Fact]
    public void A_citation_of_a_framework_that_binds_nothing_counts_as_a_citation()
        => Assert.DoesNotContain(
            Uncited("aligns-with:", "| `SPEND` | SHOULD watch the spend. | [Azure WAF].cost-optimization |"),
            f => f.Message.Contains("Azure Well-Architected Framework", StringComparison.Ordinal));

    [Fact]
    public void A_framework_heading_under_no_standing_is_not_reported_as_uncited()
        => Assert.DoesNotContain(
            Uncited("aligns-with:\n  - framework: WCAG 2.2 AA",
                "| `CONFORM` | **MUST** conform. | [WCAG 2.2 AA] |"),
            f => f.Message.Contains("Loose Framework", StringComparison.Ordinal));

    [Fact]
    public void A_policy_citing_nothing_leaves_the_register_unread()
        => Assert.Empty(Uncited("aligns-with:", "| `ZEROSEC` | COULD reach zero secrets. | |"));

    /// <summary>
    /// Markdig resolves a shortcut reference only where one is defined, so the definitions at the foot are what make a
    /// cell's labels links, and a label names the register page and heading. <c>undefined</c> names the labels left
    /// without one, which is how a case states a citation markdown never made into a link.
    /// </summary>
    private static List<Finding> Findings(string frontmatter, IReadOnlyCollection<string> undefined,
        params string[] rows)
    {
        var (doc, tree) = Policy(WithClauses(), frontmatter, undefined, rows);

        var found = new List<Finding>();
        new AlignmentRollup().Check(new CorpusRuleContext(
            [doc], new Dictionary<string, Doc> { ["pol-SCRT"] = doc }, tree, doc.TypeOf(),
            Rollup("Obliged", "Self-obligated"),
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));

        return found;
    }

    private static (Doc Doc, Tree Tree) Policy(Schema schema, string frontmatter,
        IReadOnlyCollection<string> undefined, string[] rows)
    {
        var table = "| Id | Clause | Alignment |\n|----|--------|-----------|\n"
                    + string.Join("\n", rows) + "\n";

        var definitions = string.Join("\n",
            Labels(rows).Where(l => !undefined.Contains(l, StringComparer.Ordinal))
                .Select(l => $"[{l}]: ../frameworks.md#{Anchor(l)}"));

        var text = $"---\nid: pol-SCRT\n{frontmatter}\n---\n\n# Secrets are managed\n\n"
                   + $"`Policy: pol-SCRT` `DRAFT`\n\n## Clauses\n\n{table}\n{definitions}\n";

        var doc = Doc.Parse("policies/scrt-secrets.md", text, schema);
        Assert.NotNull(doc);

        var files = new HashSet<string>(["frameworks.md", "policies/scrt-secrets.md"], StringComparer.Ordinal);
        return (doc, new Tree(files, rel => rel == "frameworks.md" ? Frameworks : text));
    }

    private static RuleSpec Rollup(params string[] postures) => new()
    {
        Id = new RuleId("alignment-rollup"), Severity = Sev.Error, Config = ["postures"], Postures = postures,
        Line = 9
    };

    /// <summary>
    /// The whole of <c>validate</c>, kept to the checks this rule reports under and to <c>schema-dispatch</c> against
    /// the policy type. The record misses a reference its clause cites, so a rule that runs reports it.
    /// </summary>
    private static List<Finding> Validated(params string[] postures)
    {
        var schema = WithClauses(Rollup(postures));
        var (doc, tree) = Policy(schema, "aligns-with:", [],
            ["| `STORE` | **MUST** hold secrets. | [ISO 27001:2022].A.5.17 |"]);

        var corpus = new LoadedCorpus
        {
            Schema = schema,
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "1.0.0" },
            Tree = tree,
            Adopted = [.. schema.ByFolder.Values],
            Docs = [doc],
            Templates = [],
            SkippedNoFrontmatter = 0
        };

        var emits = new AlignmentRollup().Emits;
        return
        [
            .. Validator.CheckAll(corpus, new DateOnly(2026, 1, 1)).Where(f =>
                emits.Contains(f.Check) || (f.Check == Dispatch && f.File == ".schema/policies.yaml"))
        ];
    }

    private static readonly CheckId Dispatch = new("schema-dispatch");

    [Fact]
    public void A_posture_the_register_spells_lets_the_rule_judge_the_records()
    {
        var found = Validated("Obliged", "Self-obligated");

        Assert.DoesNotContain(found, f => f.Check == Dispatch);
        Assert.Equal("policies/scrt-secrets.md", Assert.Single(found, f => f.Check.Value == "alignment-rollup").File);
    }

    [Fact]
    public void A_misspelt_posture_is_reported_once_against_the_schema_and_nothing_against_the_records()
    {
        var found = Assert.Single(Validated("Oblidged", "Self-obligated"));

        Assert.Equal(Dispatch, found.Check);
        Assert.Equal(".schema/policies.yaml", found.File);
        Assert.Equal(9, found.Line);
        Assert.Contains("'Oblidged'", found.Message);
    }

    /// <summary>
    /// Drops <c>framework-uncited</c>, which fires in every case here and is asked its own questions further up.
    /// </summary>
    private static List<Finding> Run(string frontmatter, params string[] rows) =>
        [.. Findings(frontmatter, [], rows).Where(f => f.Check != Unreferenced)];

    private static List<Finding> Uncited(string frontmatter, params string[] rows) =>
        [.. Findings(frontmatter, [], rows).Where(f => f.Check == Unreferenced)];

    /// <summary>The same run with <c>UK GDPR</c> left undefined. No other case cites that label.</summary>
    private static List<Finding> Unlinked(string frontmatter, params string[] rows) =>
        [.. Findings(frontmatter, ["UK GDPR"], rows).Where(f => f.Check != Unreferenced)];

    private static readonly CheckId Unreferenced = new("framework-uncited");

    /// <summary>
    /// The framework's heading without the version the label carries. <c>FinOps 2024</c> deliberately names a heading
    /// the register does not hold.
    /// </summary>
    private static string Anchor(string label) => label switch
    {
        "ISO 27001:2022" => "iso-27001",
        "WCAG 2.2 AA" => "wcag",
        "Azure WAF" => "azure-well-architected-framework",
        "Loose Framework" => "loose-framework",
        _ => "not-on-the-register"
    };

    private static IEnumerable<string> Labels(IEnumerable<string> rows) =>
        rows.SelectMany(r => System.Text.RegularExpressions.Regex.Matches(r, @"\[([^\]]+)\]")
                .Select(m => m.Groups[1].Value))
            .Distinct(StringComparer.Ordinal);

    private static Schema WithClauses(params RuleSpec[] rules) => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["policies"] = new()
            {
                Key = "policies",
                Folder = "policies",
                Rules = rules,
                IdPrefix = "pol",
                IdStyle = "mnemonic",
                Parts = new PartSpec(PartSpec.Table, "", ["MUST"], ["SHOULD", "COULD"])
                {
                    Section = "Clauses",
                    Columns = ["Id", "Clause", "Alignment"]
                }
            }
        }
    };
}
