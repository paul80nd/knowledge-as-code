// Built from strings rather than a `.schema/` on disk, so each case is a declaration nobody would commit.

using kac.core;

namespace kac.tests;

public class SchemaLoadTests
{
    // The four shared blocks, each declaring nothing. A case adds only the keys it is about.
    private static Dictionary<string, string> Blocks() => new(StringComparer.Ordinal)
    {
        ["_enums.yaml"] = "enums: {}",
        ["_tiers.yaml"] = "tiers: {}",
        ["_checks.yaml"] = "checks: {}",
        ["_universal.yaml"] = "fields: {}"
    };

    [Fact]
    public void A_schema_handed_over_as_strings_loads()
    {
        var files = Blocks();
        files["_tiers.yaml"] = """
                               tiers:
                                 governing:
                                   label: Governing
                               """;
        files["adrs.yaml"] = """
                             label: Decision
                             id:
                               prefix: adr
                             """;

        var schema = Schema.Load(files);

        Assert.Equal("Governing", Assert.Single(schema.Tiers).Label);
        Assert.Equal("Decision", schema.ByFolder["adrs"].Label);
        Assert.Equal(["adr"], schema.IdPrefixes);
    }

    [Fact]
    public void A_map_holding_nothing_loads_a_schema_declaring_nothing()
    {
        var schema = Schema.Load(new Dictionary<string, string>(StringComparer.Ordinal));

        Assert.Empty(schema.ByFolder);
        Assert.Empty(schema.Checks);
        Assert.Empty(schema.Tiers);
    }

    [Fact]
    public void A_shared_block_declares_no_type()
    {
        var files = Blocks();
        files["_extra.yaml"] = "label: Not a type";

        Assert.Empty(Schema.Load(files).ByFolder);
    }

    // `New` iterates `ByFolder` without sorting first, so the order the loader inserts in is the order
    // it reports.
    [Fact]
    public void Types_are_read_in_ordinal_name_order_whatever_order_the_map_is_in()
    {
        var files = Blocks();
        files["tools.yaml"] = "label: Tool";
        files["adrs.yaml"] = "label: Decision";
        files["policies.yaml"] = "label: Policy";

        Assert.Equal(["adrs", "policies", "tools"], Schema.Load(files).ByFolder.Keys);
    }

    [Fact]
    public void A_field_drawing_on_a_declared_enum_takes_its_values()
    {
        var files = Blocks();
        files["_enums.yaml"] = """
                               enums:
                                 status:
                                   values: [draft, accepted]
                               """;
        files["_universal.yaml"] = """
                                   fields:
                                     status:
                                       values: $enums.status
                                   """;

        var status = Schema.Load(files).Universal["status"];

        Assert.Equal(["draft", "accepted"], status.Values);
        Assert.Null(status.Problem);
    }

    // The declaration says the values are written down somewhere. A field loaded with no range would go
    // unchecked while the schema went on claiming it has one.
    [Fact]
    public void A_field_drawing_on_an_enum_nothing_declares_records_the_problem()
    {
        var files = Blocks();
        files["_universal.yaml"] = """
                                   fields:
                                     status:
                                       values: $enums.status
                                   """;

        var status = Schema.Load(files).Universal["status"];

        Assert.Null(status.Values);
        Assert.Contains("$enums.status", status.Problem!);
        Assert.Contains("declares no 'status'", status.Problem!);
    }

    // A `.schema/` short of one of its shared blocks fails through the findings naming what is missing,
    // rather than on the file the loader could not open.
    [Fact]
    public void A_file_the_map_does_not_hold_reads_as_an_empty_document()
    {
        var files = Blocks();
        files.Remove("_enums.yaml");
        files["_universal.yaml"] = """
                                   fields:
                                     status:
                                       values: $enums.status
                                   """;

        Assert.Contains("declares no 'status'", Schema.Load(files).Universal["status"].Problem!);
    }

    private static RuleSpec OnlyRule(string typeYaml)
    {
        var files = Blocks();
        files["adrs.yaml"] = typeYaml;
        return Assert.Single(Schema.Load(files).ByFolder["adrs"].Rules);
    }

    [Fact]
    public void A_rule_carrying_all_three_compiles()
    {
        var rule = OnlyRule("""
                            rules:
                              - id: has-a-title
                                expr: "present('title')"
                                severity: error
                                message: give the record a title.
                            """);

        Assert.Null(rule.Problem);
        Assert.NotNull(rule.Compiled);
        Assert.Equal(Sev.Error, rule.Severity);
    }

    [Fact]
    public void A_rule_with_an_expr_and_no_severity_loads_inert_and_says_why()
    {
        var rule = OnlyRule("""
                            rules:
                              - id: has-a-title
                                expr: "present('title')"
                                message: give the record a title.
                            """);

        Assert.Null(rule.Compiled);
        Assert.Contains("no severity", rule.Problem!);
    }

    [Fact]
    public void A_rule_with_an_expr_and_no_message_loads_inert_and_says_why()
    {
        var rule = OnlyRule("""
                            rules:
                              - id: has-a-title
                                expr: "present('title')"
                                severity: error
                            """);

        Assert.Null(rule.Compiled);
        Assert.Contains("no message", rule.Problem!);
    }

    [Fact]
    public void A_rule_whose_expr_does_not_compile_loads_inert_and_quotes_the_reason()
    {
        var rule = OnlyRule("""
                            rules:
                              - id: has-a-title
                                expr: "present('title'"
                                severity: error
                                message: give the record a title.
                            """);

        Assert.Null(rule.Compiled);
        Assert.Contains("has-a-title", rule.Problem!);
    }

    // A severity the tool does not recognise reads as absent, which leaves the rule declared but not
    // enforced. A rule declaring no expr at all is a statement of intent and carries no problem.
    [Fact]
    public void A_rule_declaring_only_an_id_is_a_statement_of_intent()
    {
        var rule = OnlyRule("""
                            rules:
                              - id: immutable-after-accepted
                            """);

        Assert.Null(rule.Severity);
        Assert.Null(rule.Problem);
        Assert.Null(rule.Compiled);
    }

    // A key nothing reads is a declaration in a file documented as the contract the tool enforces.
    [Fact]
    public void A_key_the_loader_never_asks_for_is_named_with_its_file_and_its_level()
    {
        var files = Blocks();
        files["adrs.yaml"] = """
                             label: Decision
                             id:
                               prefix: adr
                               widht: 4
                             """;

        var unread = Assert.Single(Schema.Load(files).UnreadKeys);

        Assert.Equal(".schema/adrs.yaml", unread.File);
        Assert.Equal("the 'id' block", unread.Where);
        Assert.Equal("widht", unread.Key);
    }

    // `notes:` is the one key admitted at every level and required at none.
    [Fact]
    public void The_commentary_key_is_never_reported_as_unread()
    {
        var files = Blocks();
        files["adrs.yaml"] = """
                             label: Decision
                             notes: why this type exists.
                             id:
                               prefix: adr
                               notes: why the prefix is this one.
                             """;

        Assert.Empty(Schema.Load(files).UnreadKeys);
    }

    // `Yaml.Str` answers null for a sequence, so a key the schema may write either way has to arrive as
    // a list. Read as a scalar, every declaration using the list form would look like an absent key.
    [Theory]
    [InlineData("ref: adrs", new[] { "adrs" })]
    [InlineData("ref: [adrs, policies]", new[] { "adrs", "policies" })]
    public void A_key_the_schema_may_write_either_way_arrives_as_a_list(string declared, string[] expected)
    {
        var files = Blocks();
        files["_universal.yaml"] = $"""
                                    fields:
                                      supersedes:
                                        {declared}
                                    """;

        Assert.Equal(expected, Schema.Load(files).Universal["supersedes"].Refs);
    }

    // A description is written over several lines in the schema and rendered into a table cell as one.
    [Fact]
    public void A_description_written_over_several_lines_collapses_to_one()
    {
        var files = Blocks();
        files["_universal.yaml"] = """
                                   fields:
                                     status:
                                       description: >
                                         where the record
                                         has got to.
                                   """;

        Assert.Equal("where the record has got to.", Schema.Load(files).Universal["status"].Description);
    }
}
