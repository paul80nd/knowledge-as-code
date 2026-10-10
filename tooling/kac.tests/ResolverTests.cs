using kac.core;

namespace kac.tests;

public class ResolverTests
{
    [Fact]
    public void A_scoped_citation_resolves_against_the_import_it_names()
        => Assert.Empty(Cite("`eng:pol-SCRT.STORE`"));

    [Fact]
    public void A_scoped_record_cited_whole_resolves_too()
        => Assert.Empty(Cite("`eng:pol-SCRT`"));

    [Fact]
    public void A_clause_the_imported_record_does_not_carry_is_reported()
        => Assert.Equal("'eng:pol-SCRT.NOPE' cites a clause 'NOPE' that eng:pol-SCRT does not carry.",
            Assert.Single(Cite("`eng:pol-SCRT.NOPE`")).Message);

    [Fact]
    public void A_record_no_import_holds_is_reported_as_absent()
        => Assert.Equal("'eng:pol-NONE.X' cites 'eng:pol-NONE', which does not exist.",
            Assert.Single(Cite("`eng:pol-NONE.X`")).Message);

    [Fact]
    public void A_bare_id_naming_an_imported_record_is_told_the_scope_to_write()
        => Assert.Equal(
            "'pol-SCRT.STORE' names a record this corpus imports rather than holds. Write it as "
            + "'eng:pol-SCRT.STORE', so a reader can see which corpus owns it.",
            Assert.Single(Cite("`pol-SCRT.STORE`")).Message);

    [Fact]
    public void A_scoped_id_naming_a_record_this_corpus_holds_is_told_to_drop_the_scope()
        => Assert.Equal(
            "'eng:pol-LOCAL.HERE' scopes a record this corpus holds itself. Write it as "
            + "'pol-LOCAL.HERE': two spellings of one id defeat every search anybody runs for it.",
            Assert.Single(Cite("`eng:pol-LOCAL.HERE`")).Message);

    [Fact]
    public void A_citation_into_a_corpus_reached_through_another_resolves()
        => Assert.Empty(Cite("`gp:pol-OLD.KEEP`"));

    [Fact]
    public void A_grandparent_s_record_is_not_reachable_under_the_corpus_it_arrived_in()
        => Assert.Equal("'eng:pol-OLD.KEEP' cites 'eng:pol-OLD', which does not exist.",
            Assert.Single(Cite("`eng:pol-OLD.KEEP`")).Message);

    [Fact]
    public void A_shortcode_this_corpus_consumes_nothing_under_is_reported()
        => Assert.Equal(
            "'gov:pol-SCRT.STORE' cites 'gov:', and this corpus consumes nothing under that shortcode. "
            + "Declare it in `consumes:`, or correct the spelling.",
            Assert.Single(Cite("`gov:pol-SCRT.STORE`")).Message);

    [Fact]
    public void A_citation_into_an_unrestored_import_reports_nothing_of_its_own()
        => Assert.Empty(Cite("`eng:pol-SCRT.STORE`", ImportGraph.None with { NotRestored = ["eng"] }));

    [Fact]
    public void An_unknown_shortcode_is_left_to_the_restore_while_one_is_outstanding()
        => Assert.Empty(Cite("`gp:pol-OLD.KEEP`", ImportGraph.None with { NotRestored = ["eng"] }));

    [Fact]
    public void A_ref_field_accepts_a_scoped_part_level_id()
        => Assert.Empty(Implements("eng:pol-SCRT.STORE"));

    [Fact]
    public void A_ref_field_asks_both_halves_of_a_scoped_id()
        => Assert.Equal("'implements' points at a clause 'NOPE' that eng:pol-SCRT does not carry.",
            Assert.Single(Implements("eng:pol-SCRT.NOPE")).Message);

    [Fact]
    public void A_ref_field_holds_a_scoped_id_to_the_type_the_declaration_names()
        => Assert.Equal("'implements' points at 'eng:svc-GATE', which is a Service, not a Policy.",
            Assert.Single(Implements("eng:svc-GATE")).Message);

    [Fact]
    public void A_ref_field_holding_a_bare_imported_id_is_told_the_scope_to_write()
        => Assert.Equal(
            "'implements': 'pol-SCRT' names a record this corpus imports rather than holds. Write it as "
            + "'eng:pol-SCRT', so a reader can see which corpus owns it.",
            Assert.Single(Implements("pol-SCRT")).Message);

    [Fact]
    public void A_ref_field_reports_a_scope_naming_no_import()
        => Assert.Contains("consumes nothing under that shortcode",
            Assert.Single(Implements("gov:pol-SCRT")).Message);

    [Fact]
    public void A_mis_cased_imported_id_resolves_as_a_mis_cased_local_one_does()
        => Assert.Empty(Cite("`eng:pol-scrt.STORE`"));

    [Fact]
    public void A_mis_cased_bare_id_is_still_told_the_scope_to_write()
        => Assert.Contains("Write it as 'eng:pol-scrt.STORE'",
            Assert.Single(Cite("`pol-scrt.STORE`")).Message);

    [Fact]
    public void A_corpus_importing_nothing_reads_a_bare_citation_exactly_as_before()
        => Assert.Empty(Cite("`pol-LOCAL.HERE`", ImportGraph.None));

    private static List<Finding> Cite(string body, ImportGraph? imports = null)
        => Check($"# std-ERRORS\n\n{body}\n", null, imports);

    private static List<Finding> Implements(string target, ImportGraph? imports = null)
        => Check("# std-ERRORS\n", target, imports);

    private static List<Finding> Check(string body, string? implements, ImportGraph? imports)
    {
        var schema = Schema();
        var docs = new List<Doc>
        {
            Required.Parsed("standards/error-responses.md",
                $"---\nid: std-ERRORS\n{(implements is null ? "" : $"implements: {implements}\n")}---\n\n{body}",
                schema),
            Required.Parsed("policies/pol-LOCAL.md",
                "---\nid: pol-LOCAL\n---\n\n# pol-LOCAL\n\n## Clauses\n\n"
                + "| Id | Clause |\n|----|--------|\n| `HERE` | **MUST** stay put |\n",
                schema)
        };

        var found = new List<Finding>();
        Validator.CheckCorpus(schema, docs, found, imports ?? Consuming());
        return found;
    }

    /// <summary>
    /// A governance corpus holding one policy with two clauses, and a service, so a field declaring <c>ref:
    /// policies</c> has something of the wrong type to be aimed at.
    /// </summary>
    private static ImportGraph Consuming() =>
        new([
            new Import("eng", "example-engineering", "0.1.0", null, [
                new ImportedRecord("eng", "pol-SCRT", "policies", "policies/scrt.md", true,
                    ["STORE", "ROTATE"]),
                new ImportedRecord("eng", "svc-GATE", "services", "services/gate.md", false, []),

                // This corpus declares no `gp:`, so the scope comes off the record and off nothing else.
                new ImportedRecord("gp", "pol-OLD", "policies", "policies/old.md", true, ["KEEP"])
            ])
        ], [], []);

    /// <summary>The parts declaration is what gives a message the word "clause" rather than "part".</summary>
    private static Schema Schema()
    {
        var implements = new FieldSpec { Name = "implements", Type = "id", Refs = ["policies"] };

        return new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["standards"] = new()
                {
                    Key = "standards", Folder = "standards", Label = "Standard", LabelPlural = "Standards",
                    IdPrefix = "std", FieldOrder = ["implements"],
                    Fields = new Dictionary<string, FieldSpec> { ["implements"] = implements }
                },
                ["policies"] = new()
                {
                    Key = "policies", Folder = "policies", Label = "Policy", LabelPlural = "Policies",
                    IdPrefix = "pol",
                    Parts = new PartSpec("table", "", [], []) { Noun = "clause", Section = "Clauses" }
                },
                ["services"] = new()
                {
                    Key = "services", Folder = "services", Label = "Service", LabelPlural = "Services",
                    IdPrefix = "svc"
                }
            }
        };
    }
}
