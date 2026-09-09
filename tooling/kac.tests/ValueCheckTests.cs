using System.Text.RegularExpressions;
using kac.core;
using YamlDotNet.RepresentationModel;

// In-process unit tests for what one frontmatter value is held to.
//
// Each drives a `FieldSpec` the test declares itself against a scrap of YAML, because that is the whole
// of what `ValueChecks` reads: no corpus, no document, no schema file. The declarations here are
// deliberately not any real type's. What is being shown is that the checks act on what the schema
// declares rather than on anything they assume about policies or glossaries.
//
// The golden fixtures cover each check id once, which is what the coverage gate asks. The branches
// below are the ones a fixture could only duplicate: the two ways a date fails, the near misses a
// timestamp meets, the two ways an enum fails, and every arm of the template exemption.

namespace kac.tests;

public class ValueCheckTests
{
    private const int FrontStart = 2; // the line a `---` block's first key sits on

    private static FieldSpec Field(string type, string name = "field") => new() { Name = name, Type = type };

    // A field declaring `object` holds one mapping, and its keys are judged as a list's entries are.
    // Every other field reads a mapping as an unquoted placeholder, which is what `bare-key` reports.
    [Fact]
    public void An_object_field_holds_a_mapping()
        => Assert.Empty(Run("field: { at: 2026-06-12T09:00:00Z, by: kac/0.24.0 }\n", Event()));

    [Fact]
    public void A_key_inside_an_object_field_is_held_to_its_own_type()
        => Assert.Equal(["timestamp-format"],
            Ids(Run("field: { at: yesterday, by: kac/0.24.0 }\n", Event())));

    [Fact]
    public void A_key_an_object_field_does_not_declare_is_reported()
        => Assert.Equal(["entry-key"], Ids(Run("field: { at: 2026-06-12T09:00:00Z, by: x, why: y }\n", Event())));

    [Fact]
    public void A_key_an_object_field_requires_and_omits_is_reported()
        => Assert.Equal(["entry-key"], Ids(Run("field: { at: 2026-06-12T09:00:00Z }\n", Event())));

    // A scalar under a field declared an object never reaches the mapping arm, so `Entry` reports the
    // shape and names the keys the field wanted.
    [Fact]
    public void A_scalar_under_an_object_field_is_reported_as_the_wrong_shape()
        => Assert.Equal(["entry-shape"], Ids(Run("field: today\n", Event())));

    // A bare key is absent for an object exactly as it is for everything else, so a template writing
    // the key with nothing under it is not read as a malformed object.
    [Fact]
    public void A_bare_object_field_is_absent()
        => Assert.False(ValueChecks.IsAbsent(Value("field: { at: 2026-06-12T09:00:00Z, by: x }\n"), Event()));

    [Fact]
    public void A_mapping_is_absent_to_every_field_that_is_not_an_object()
        => Assert.True(ValueChecks.IsAbsent(Value("field: { at: 2026-06-12T09:00:00Z, by: x }\n"),
            Field("string")));

    // The `event` shape as `_shapes.yaml` declares it, written out here because these tests read no
    // schema file.
    private static FieldSpec Event() => new()
    {
        Name = "field",
        Type = "object",
        Entry =
        [
            new FieldSpec { Name = "at", Type = "timestamp", Required = true },
            new FieldSpec { Name = "by", Type = "string", Required = true }
        ]
    };


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

    [Fact]
    public void A_date_in_the_wrong_shape_says_so()
    {
        var found = Run("field: \"2027/08/04\"\n", Field("date"));
        var date = Assert.Single(found, f => f.Check.Value == "date-format");
        Assert.Contains("must be a YYYY-MM-DD date", date.Message);
    }

    // The distinct wording is the whole reason the shape and the calendar are asked separately.
    [Fact]
    public void A_date_that_is_not_on_the_calendar_says_something_else()
    {
        var found = Run("field: \"2027-13-40\"\n", Field("date"));
        var date = Assert.Single(found, f => f.Check.Value == "date-format");
        Assert.Contains("not a date on the calendar", date.Message);
    }

    [Fact]
    public void An_unquoted_utc_moment_passes()
    {
        Assert.Empty(Run("field: 2027-08-04T09:30:00Z\n", Field("timestamp")));
    }

    // A day is a well-formed value of a different type, and the commonest thing written where a moment
    // belongs. Nothing about it names an hour, so it fails on shape rather than on the calendar.
    [Fact]
    public void A_day_written_where_a_moment_belongs_says_so()
    {
        var found = Run("field: 2027-08-04\n", Field("timestamp"));
        var moment = Assert.Single(found, f => f.Check.Value == "timestamp-format");
        Assert.Contains("must be a YYYY-MM-DDThh:mm:ssZ moment in UTC", moment.Message);
    }

    // A local time is the other near miss. It is written as a moment and names no zone, so two readers
    // would take it as two different instants.
    [Fact]
    public void A_moment_without_its_zone_is_refused()
    {
        var found = Run("field: 2027-08-04T09:30:00\n", Field("timestamp"));
        Assert.Contains("timestamp-format", Ids(found));
    }

    [Fact]
    public void A_moment_that_is_not_on_the_calendar_says_something_else()
    {
        var found = Run("field: 2027-02-31T09:30:00Z\n", Field("timestamp"));
        var moment = Assert.Single(found, f => f.Check.Value == "timestamp-format");
        Assert.Contains("not a moment on the calendar", moment.Message);
    }

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

    // The message reads as English at one entry or it does not.
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

    // The shape test reads the record and leaves the scope and the part alone. See ValueChecks.cs.
    [Theory]
    [InlineData("pol-KNOW")]
    [InlineData("adr-0007")]
    [InlineData("svc-search")]
    [InlineData("pol-VURM.TIMEBOX")]
    [InlineData("std-ERRORS.a-failure-says-what-happened")]
    [InlineData("eng:std-ERRORS.a-failure-says-what-happened")]
    [InlineData("gls-search.title")]
    public void Every_id_style_is_id_shaped_in_a_list(string id)
        => Assert.Empty(Run($"field: [ {id} ]\n", new FieldSpec { Name = "field", Type = "list", Of = "id" }));

    // The record in front of a part still carries the taxonomy's spelling.
    [Theory]
    [InlineData("Pol-KNOW")]
    [InlineData("pol-Know")]
    [InlineData("noprefix")]
    [InlineData("pol-Know.TIMEBOX")]
    [InlineData("noprefix.TIMEBOX")]
    public void A_miscased_or_prefixless_entry_is_still_not_an_id(string id)
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "id" };
        Assert.Equal("id-format", Assert.Single(Run($"field: [ {id} ]\n", spec)).Check.Value);
    }

    [Fact]
    public void An_object_list_declaring_no_entry_block_requires_nothing()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "object" };
        var found = Assert.Single(Run("field:\n  - framework: ISO\n", spec));
        Assert.Equal("entry-key", found.Check.Value);
        Assert.Contains("carries 'framework'", found.Message);
    }

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

    [Fact]
    public void A_bare_key_is_how_absence_is_written()
    {
        Assert.Empty(Run("field:\n", new FieldSpec { Name = "field", Type = "date", Required = true }));
    }

    [Fact]
    public void A_bare_key_on_an_optional_field_is_reported()
    {
        Assert.Equal(["empty-optional-key"], Ids(Run("field:\n", Field("date"))));
    }

    // Whether the field is optional here depends on another field's value, and this pass reads one
    // value at a time.
    [Fact]
    public void A_required_when_field_is_exempt_from_the_optional_reading()
    {
        var spec = new FieldSpec { Name = "field", Type = "date", RequiredWhen = "status = live" };
        Assert.Empty(Run("field:\n", spec));
    }

    // Two findings would be one omission reported twice, so the spelling is the finding and the
    // optional reading is not reached.
    [Fact]
    public void A_value_that_is_absent_and_misspelt_is_reported_once()
    {
        Assert.Equal(["bare-key"], Ids(Run("field: null\n", Field("date"))));
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

    // An empty sequence is the one absent value that is not a scalar.
    [Fact]
    public void An_empty_sequence_is_absent()
    {
        Assert.Equal("bare-key", Assert.Single(Run("field: []\n", Field("list"))).Check.Value);
    }

    [Fact]
    public void A_literal_short_circuits_a_scalar_field()
    {
        var spec = new FieldSpec { Name = "field", Type = "date", AllowLiteral = ["never"] };
        Assert.Empty(Run("field: never\n", spec));
    }

    [Fact]
    public void A_literal_in_a_list_exempts_the_entry_and_not_the_field()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "id", AllowLiteral = ["all"] };
        var found = Assert.Single(Run("field: [ all, NotAnId ]\n", spec));
        Assert.Equal("id-format", found.Check.Value);
        Assert.Contains("'NotAnId'", found.Message);
    }

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

    // A record holding a placeholder is an unfinished copy, so the field's checks report the malformed date.
    [Fact]
    public void The_same_placeholder_in_a_record_is_not_exempt()
    {
        Assert.Contains("date-format", Ids(Run("field: \"{{date}}\"\n", Field("date"))));
    }

    [Theory]
    [InlineData("field: 12345\n")]
    [InlineData("field: -7\n")]
    [InlineData("field: +7\n")]
    [InlineData("field: 0\n")]
    [InlineData("field: \"12345\"\n")] // quoted, which YAML calls a string and this check does not read
    public void A_whole_number_passes(string yaml)
    {
        Assert.Empty(Run(yaml, Field("int")));
    }

    [Theory]
    [InlineData("field: 12a\n")]
    [InlineData("field: 1.5\n")]
    [InlineData("field: 1,000\n")]
    [InlineData("field: 1_000\n")] // a YAML 1.1 separator, refused rather than decoded
    [InlineData("field: 0x1f\n")] // a base prefix, likewise
    public void A_value_not_written_as_a_number_says_so(string yaml)
    {
        var found = Assert.Single(Run(yaml, Field("int")));
        Assert.Equal("int-format", found.Check.Value);
        Assert.Contains("is not a whole number", found.Message);
    }

    // Written as a number and naming no value a `long` holds, which is the second half `Date` and
    // `Timestamp` report under one id for the same reason: the author has one thing to do about either.
    [Fact]
    public void A_number_past_what_the_tool_holds_says_so()
    {
        var found = Assert.Single(Run("field: 99999999999999999999\n", Field("int")));
        Assert.Equal("int-format", found.Check.Value);
        Assert.Contains("more digits than a number can hold", found.Message);
    }

    // An entry of an `of: int` list reaches the same check, and the message says which half of the field
    // is at fault.
    [Fact]
    public void An_entry_of_an_int_list_is_held_to_the_same_thing()
    {
        var spec = new FieldSpec { Name = "field", Type = "list", Of = "int" };
        var found = Assert.Single(Run("field: [ 41, abc ]\n", spec));

        Assert.Equal("int-format", found.Check.Value);
        Assert.Contains("'field' entry 'abc'", found.Message);
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

    // The same spelling in a record, under the id a reader already looks to for a field nobody filled in.
    [Fact]
    public void The_same_mapping_in_a_record_is_reported_as_a_bare_key()
    {
        var found = Assert.Single(Run("field: {{date}}\n", Field("date")));
        Assert.Equal("bare-key", found.Check.Value);
        Assert.Contains("read as a YAML mapping", found.Message);
    }

    // A mapping reaches neither the declared pattern nor any other value check, so `bare-key` is the
    // whole of the finding.
    [Fact]
    public void A_mapping_in_a_pattern_field_is_not_passed_over()
    {
        var spec = new FieldSpec
        {
            Name = "field", Type = "string",
            Pattern = "^(human|role):[a-z.]+$", PatternRegex = new Regex("^(human|role):[a-z.]+$")
        };
        Assert.Equal(["bare-key"], Ids(Run("field: {{owner}}\n", spec)));
    }

    // An object entry recurses into `Check` under the path it reached the entry by, and the fix has to
    // quote the key the file writes rather than that path.
    [Fact]
    public void The_fix_quotes_the_key_an_entry_carries()
    {
        var findings = new List<Finding>();
        ValueChecks.Check("verified.at", Value("field: {{date}}\n"), Field("date"), DocKind.Record,
            FrontStart, new Report("rec.md", findings));

        Assert.Contains("has to be quoted: at: \"{{…}}\".", Assert.Single(findings).Message);
    }

    // The parser reads a frontmatter block on its own, so its line 1 is the block's first key.
    [Fact]
    public void A_finding_names_the_line_in_the_document_not_in_the_block()
    {
        var findings = new List<Finding>();
        var node = Value("other: x\nfield: \"2027/08/04\"\n");
        ValueChecks.Check("field", node, Field("date"), DocKind.Record, FrontStart,
            new Report("rec.md", findings));

        Assert.Equal(FrontStart + 1, Assert.Single(findings).Line);
    }

    [Theory]
    [InlineData("field:\n", true)]
    [InlineData("field: ~\n", true)]
    [InlineData("field: null\n", true)]
    [InlineData("field: []\n", true)]
    [InlineData("field: something\n", false)]
    [InlineData("field: [ a ]\n", false)]
    [InlineData("field: {{owner}}\n", true)] // an unquoted placeholder, which YAML reads as a mapping
    public void IsAbsent_reads_every_way_of_supplying_nothing(string yaml, bool absent)
    {
        Assert.Equal(absent, ValueChecks.IsAbsent(Value(yaml)));
    }
}
