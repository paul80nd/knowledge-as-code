using System.Text.RegularExpressions;
using kac.core;
using YamlDotNet.RepresentationModel;

// In-process unit tests for what one frontmatter value is held to.
//
// Each drives a `FieldSpec` the test declares itself against a scrap of YAML, because that is the whole
// of what `ValueChecks` reads: no corpus, no document, no schema file. The declarations here are
// deliberately not any real type's — what is being shown is that the checks act on what the schema
// declares rather than on anything they assume about policies or glossaries.
//
// The golden fixtures cover each check id once, which is what the coverage gate asks. The branches
// below are the ones a fixture could only duplicate: the two ways a date fails, the two ways an enum
// does, and every arm of the template exemption.

namespace kac.tests;

public class ValueCheckTests
{
    private const int FrontStart = 2; // the line a `---` block's first key sits on

    private static FieldSpec Field(string type, string name = "field") => new() { Name = name, Type = type };

    // The value under `field:`, read from a real parse so quoting style and node positions are the
    // parser's rather than the test's.
    private static YamlNode Value(string yaml)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        return ((YamlMappingNode)stream.Documents[0].RootNode).Children[new YamlScalarNode("field")];
    }

    private static List<Finding> Run(string yaml, FieldSpec spec, DocKind kind = DocKind.Record)
    {
        var findings = new List<Finding>();
        ValueChecks.Check("field", Value(yaml), spec, kind, FrontStart, new Report("rec.md", findings));
        return findings;
    }

    private static string[] Ids(List<Finding> f) => [.. f.Select(x => x.Check.Value)];

    // -- dates: the shape and the calendar are two faults under one id --

    [Fact]
    public void A_quoted_iso_date_passes()
    {
        Assert.Empty(Run("field: \"2027-08-04\"\n", Field("date")));
    }

    // Both fire: an unquoted date is also being read as something other than a string.
    [Fact]
    public void An_unquoted_date_is_reported_as_unquoted()
    {
        Assert.Contains("date-quoted", Ids(Run("field: 2027-08-04\n", Field("date"))));
    }

    // Not written as a date at all — the characters are wrong before any day is named.
    [Fact]
    public void A_date_in_the_wrong_shape_says_so()
    {
        var found = Run("field: \"2027/08/04\"\n", Field("date"));
        var date = Assert.Single(found, f => f.Check.Value == "date-format");
        Assert.Contains("must be a YYYY-MM-DD date", date.Message);
    }

    // Written as a date, and naming a day that has never existed. The distinct wording is the whole
    // reason the shape and the calendar are asked separately.
    [Fact]
    public void A_date_that_is_not_on_the_calendar_says_something_else()
    {
        var found = Run("field: \"2027-13-40\"\n", Field("date"));
        var date = Assert.Single(found, f => f.Check.Value == "date-format");
        Assert.Contains("not a date on the calendar", date.Message);
    }

    // -- enums: membership and casing are separate, and one value can fail both --

    [Fact]
    public void An_out_of_range_enum_lists_what_was_allowed()
    {
        var spec = new FieldSpec { Name = "field", Type = "enum", Values = ["draft", "active"] };
        var found = Assert.Single(Run("field: retired\n", spec));
        Assert.Equal("enum", found.Check.Value);
        Assert.Contains("is not one of: draft, active", found.Message);
    }

    [Fact]
    public void A_capitalised_enum_trips_both_membership_and_casing()
    {
        var spec = new FieldSpec { Name = "field", Type = "enum", Values = ["draft", "active"] };
        Assert.Equal(["enum", "enum-lowercase"], Ids(Run("field: Draft\n", spec)));
    }

    [Fact]
    public void A_sequence_where_an_enum_is_declared_is_not_a_scalar()
    {
        var spec = new FieldSpec { Name = "field", Type = "enum", Values = ["draft"] };
        var found = Assert.Single(Run("field:\n  - draft\n", spec));
        Assert.Equal("enum", found.Check.Value);
        Assert.Contains("must be a scalar", found.Message);
    }

    // -- lists --

    [Fact]
    public void A_scalar_where_a_list_is_declared_is_reported_once()
    {
        var found = Assert.Single(Run("field: notasequence\n", Field("list")));
        Assert.Equal("list", found.Check.Value);
    }

    // Reported before the entries are read, so an author fixing a short list is not also told an entry
    // is malformed and made to re-run to find the floor.
    [Fact]
    public void A_list_under_its_floor_counts_what_is_there()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", MinItems = 3 };
        var found = Assert.Single(Run("field: [ a, b ]\n", spec));
        Assert.Equal("min-items", found.Check.Value);
        Assert.Contains("has 2 entries", found.Message);
    }

    // The singular is worth a test of its own: the message reads as English at one entry or it does not.
    [Fact]
    public void One_entry_is_an_entry_not_entries()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", MinItems = 2 };
        Assert.Contains("has 1 entry:", Assert.Single(Run("field: [ a ]\n", spec)).Message);
    }

    [Fact]
    public void Only_the_first_pair_out_of_order_is_reported()
    {
        var found = Assert.Single(Run("field: [ zebra, alpha, beta ]\n", Field("list")));
        Assert.Equal("list-order", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Contains("'alpha' should come before 'zebra'", found.Message);
    }

    // Alphabetical as a reader means it, which is `Natural`'s answer rather than a byte-wise one.
    [Fact]
    public void Digit_runs_in_a_list_compare_as_numbers()
    {
        Assert.Empty(Run("field: [ a-8, a-29 ]\n", Field("list")));
    }

    [Fact]
    public void A_list_entry_that_is_not_an_id_is_named()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "id" };
        var found = Assert.Single(Run("field: [ NotAnId ]\n", spec));
        Assert.Equal("id-format", found.Check.Value);
        Assert.Contains("entry 'NotAnId'", found.Message);
    }

    // Every id style a type may declare reaches a list field, and a mnemonic carries its discriminator
    // upper-case. `implements: [ pol-KNOW ]` is the edge the taxonomy declares between a standard and a
    // policy, so a shape test that read the whole entry as lower-case made that edge unwritable.
    [Theory]
    [InlineData("pol-KNOW")]
    [InlineData("adr-0007")]
    [InlineData("svc-search")]
    [InlineData("pol-VURM.TIMEBOX")]
    public void Every_id_style_is_id_shaped_in_a_list(string id)
        => Assert.Empty(Run($"field: [ {id} ]\n", new FieldSpec { Name = "field", Type = "list", Of = "id" }));

    // The prefix names a type and is lower-case wherever it appears, and a discriminator is cased one way
    // or the other rather than both. Loosening the case test to reach the mnemonic stops here.
    [Theory]
    [InlineData("Pol-KNOW")]
    [InlineData("pol-Know")]
    [InlineData("noprefix")]
    public void A_miscased_or_prefixless_entry_is_still_not_an_id(string id)
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "id" };
        Assert.Equal("id-format", Assert.Single(Run($"field: [ {id} ]\n", spec)).Check.Value);
    }

    // -- patterns: the same declaration reads differently for a scalar and for a list --

    [Fact]
    public void A_pattern_on_a_scalar_field_calls_it_a_value()
    {
        var spec = new FieldSpec
        {
            Name = "field", Type = "string", Pattern = "^[a-z]+$", PatternRegex = new Regex("^[a-z]+$")
        };
        var found = Assert.Single(Run("field: Nope1\n", spec));
        Assert.Equal("field-pattern", found.Check.Value);
        Assert.Contains("value 'Nope1'", found.Message);
    }

    [Fact]
    public void A_pattern_on_a_list_field_applies_to_each_entry()
    {
        var spec = new FieldSpec
        {
            Name = "field", Type = "list", Pattern = "^[a-z]+$", PatternRegex = new Regex("^[a-z]+$")
        };
        var found = Assert.Single(Run("field: [ Nope1 ]\n", spec));
        Assert.Equal("field-pattern", found.Check.Value);
        Assert.Contains("entry 'Nope1'", found.Message);
    }

    // -- absence, and the one shape of it that is correct --

    [Fact]
    public void A_bare_key_is_how_absence_is_written()
    {
        Assert.Empty(Run("field:\n", Field("date")));
    }

    [Theory]
    [InlineData("field: ~\n")]
    [InlineData("field: null\n")]
    [InlineData("field: \"\"\n")]
    public void Every_other_way_of_writing_nothing_is_reported(string yaml)
    {
        var found = Assert.Single(Run(yaml, Field("date")));
        Assert.Equal("bare-key", found.Check.Value);
    }

    // An empty sequence is absent too, and is the one absent value that is not a scalar.
    [Fact]
    public void An_empty_sequence_is_absent()
    {
        Assert.Equal("bare-key", Assert.Single(Run("field: []\n", Field("list"))).Check.Value);
    }

    // -- literals: a word the field admits beside its declared type --

    [Fact]
    public void A_literal_short_circuits_a_scalar_field()
    {
        var spec = new FieldSpec { Name = "field", Type = "date", AllowLiteral = ["never"] };
        Assert.Empty(Run("field: never\n", spec));
    }

    // A list is not short-circuited: the literal exempts its own entry and the rest are still read.
    [Fact]
    public void A_literal_in_a_list_exempts_the_entry_and_not_the_field()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "id", AllowLiteral = ["all"] };
        var found = Assert.Single(Run("field: [ all, NotAnId ]\n", spec));
        Assert.Equal("id-format", found.Check.Value);
        Assert.Contains("'NotAnId'", found.Message);
    }

    // -- a template answers for the documents copied from it, not for itself --

    [Fact]
    public void A_placeholder_in_a_template_is_read_as_absent()
    {
        Assert.Empty(Run("field: \"{{date}}\"\n", Field("date"), DocKind.Template));
    }

    [Fact]
    public void A_placeholder_inside_a_list_exempts_the_field()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "id" };
        Assert.Empty(Run("field: [ \"svc-{{a}}\", NotAnId ]\n", spec, DocKind.Template));
    }

    // The same value in a record is an unfinished copy, so the field's own checks read the mark and
    // report it as the malformed date it is. The exemption is the template's alone.
    [Fact]
    public void The_same_placeholder_in_a_record_is_not_exempt()
    {
        Assert.Contains("date-format", Ids(Run("field: \"{{date}}\"\n", Field("date"))));
    }

    // An unquoted placeholder opens a YAML flow mapping, so the value never arrives as text. Reported
    // with the fix, rather than left to the date check to quote an empty string back at the author.
    [Fact]
    public void An_unquoted_placeholder_is_reported_as_a_mapping()
    {
        var found = Assert.Single(Run("field: {{date}}\n", Field("date"), DocKind.Template));
        Assert.Equal("template-fields", found.Check.Value);
        Assert.Contains("read as a YAML mapping", found.Message);
    }

    // -- where a finding lands --

    // The parser reads a frontmatter block on its own, so its line 1 is the block's first key. A finding
    // has to name the line in the document, which is what `frontStart` turns it into.
    [Fact]
    public void A_finding_names_the_line_in_the_document_not_in_the_block()
    {
        var findings = new List<Finding>();
        var node = Value("other: x\nfield: \"2027/08/04\"\n");
        ValueChecks.Check("field", node, Field("date"), DocKind.Record, FrontStart,
            new Report("rec.md", findings));

        Assert.Equal(FrontStart + 1, Assert.Single(findings).Line);
    }

    // -- the reading of "absent" that the required-field pass shares --

    [Theory]
    [InlineData("field:\n", true)]
    [InlineData("field: ~\n", true)]
    [InlineData("field: null\n", true)]
    [InlineData("field: []\n", true)]
    [InlineData("field: something\n", false)]
    [InlineData("field: [ a ]\n", false)]
    public void IsAbsent_reads_every_way_of_supplying_nothing(string yaml, bool absent)
    {
        Assert.Equal(absent, ValueChecks.IsAbsent(Value(yaml)));
    }
}
