using kac.core;

// In-process unit tests for a record's addressable parts. Every branch here is driven by a type's
// `parts:` declaration rather than by anything specific to policies or glossaries, so these build small
// declarations of their own — which is also the only way to show that the source, the modals, the levels
// and the id pattern are read from the schema rather than assumed.

namespace kac.tests;

public class PartCheckTests
{
    private static PartSpec Table() =>
        new(PartSpec.Table, "^[A-Z]{3,8}$", ["MUST", "MUST NOT"], ["SHOULD"])
            { Section = "Clauses", Noun = "clause", Columns = ["Id", "Clause"] };

    private static PartSpec Headings() =>
        new(PartSpec.Headings, "", [], []) { Section = "Terms", Noun = "term", Level = 3 };

    private const string Header = "## Clauses\n\n| Id | Clause |\n|----|--------|\n";

    // -- the table itself, and why a broken one stops the pass --

    [Fact]
    public void A_section_with_no_table_says_what_to_write()
    {
        var found = Run("## Clauses\n\nProse where a table should be.\n");
        Assert.Equal("clause-table", Assert.Single(found).Check.Value);
        Assert.Contains("headed 'Id | Clause'", Assert.Single(found).Message);
    }

    [Fact]
    public void A_mis_headed_table_names_both_headings_and_stops()
    {
        var found = Run("## Clauses\n\n| Ref | Rule |\n|-----|------|\n| `LOGS` | not a modal in sight |\n");
        Assert.Equal("clause-table", Assert.Single(found).Check.Value);
    }

    // Reported and stopped rather than left to the row checks, which would find nothing and say nothing.
    [Fact]
    public void An_empty_table_binds_nobody()
    {
        var found = Run(Header);
        Assert.Equal("clause-table", Assert.Single(found).Check.Value);
        Assert.Contains("binds nothing binds nobody", Assert.Single(found).Message);
    }

    // A missing section is `required-section`'s to report; saying it twice would make one fault two.
    [Fact]
    public void A_document_without_the_section_is_left_to_required_section()
        => Assert.Empty(Run("## Context\n\nNothing to do with clauses.\n"));

    [Fact]
    public void A_type_declaring_no_parts_is_not_asked_about_them()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST** be retained. |\n", null, "policies"));

    // -- the modal, which is the binding level --

    [Fact]
    public void A_row_that_opens_with_no_modal_is_not_an_obligation()
    {
        var found = Run(Header + "| `LOGS` | Audit logs are retained for a year. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("MUST, MUST NOT, SHOULD", Assert.Single(found).Message);
    }

    // Bold carries the level visually, so a binding modal written plain reads as advice.
    [Fact]
    public void A_binding_modal_written_plain_is_reported()
    {
        var found = Run(Header + "| `LOGS` | MUST be retained for a year. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("write it bold", Assert.Single(found).Message);
    }

    [Fact]
    public void An_advisory_modal_written_bold_is_reported()
    {
        var found = Run(Header + "| `LOGS` | **SHOULD** be retained for a year. |\n");
        Assert.Contains("write it plain", Assert.Single(found).Message);
    }

    // Longest first, or "MUST NOT" would be read as the "MUST" that prefixes it and then reported as a
    // compound clause carrying a second modal.
    [Fact]
    public void A_longer_modal_is_recognised_before_the_one_that_prefixes_it()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST NOT** leave the tenancy. |\n"));

    [Fact]
    public void A_second_modal_in_one_row_is_two_obligations_sharing_an_id()
    {
        var found = Run(Header + "| `LOGS` | **MUST** be retained and SHOULD be indexed. |\n");
        Assert.Equal("clause-compound", Assert.Single(found).Check.Value);
        Assert.Contains("carries a second 'SHOULD'", Assert.Single(found).Message);
    }

    // -- the id, which is what makes a clause citable --

    [Fact]
    public void An_id_that_is_not_a_code_span_is_a_word_rather_than_a_handle()
    {
        var found = Run(Header + "| LOGS | **MUST** be retained. |\n");
        Assert.Equal("clause-id-format", Assert.Single(found).Check.Value);
        Assert.Contains("write it as `LOGS`", Assert.Single(found).Message);
    }

    // The pattern is the type's, so this is what shows it is read rather than assumed.
    [Fact]
    public void An_id_outside_the_types_pattern_names_the_pattern()
    {
        var found = Run(Header + "| `lo` | **MUST** be retained. |\n");
        Assert.Contains("does not match ^[A-Z]{3,8}$", Assert.Single(found).Message);
    }

    // Ordinal, because `LOGS` and `logs` differing only in case is not two clauses a reader could tell
    // apart either — though here the pattern catches the lower-cased one first.
    [Fact]
    public void An_id_used_twice_makes_a_citation_of_it_ambiguous()
    {
        var found = Run(Header
                        + "| `LOGS` | **MUST** be retained. |\n"
                        + "| `LOGS` | **MUST** be indexed. |\n");
        var one = Assert.Single(found);
        Assert.Equal("part-id-unique", one.Check.Value);
        Assert.Contains("two clauses here address as 'LOGS'", one.Message);
    }

    // -- the ordering, reported once --

    [Fact]
    public void A_table_out_of_order_is_reported_against_the_first_row_that_breaks_it()
    {
        var found = Run(Header
                        + "| `AAA` | **MUST** be first. |\n"
                        + "| `BBB` | SHOULD be last. |\n"
                        + "| `CCC` | **MUST** be first too. |\n"
                        + "| `DDD` | **MUST** and again. |\n");

        var order = found.Where(f => f.Check.Value == "clause-order").ToList();
        Assert.Single(order);
        Assert.Contains("'CCC' is a 'MUST' but follows a 'SHOULD'", order[0].Message);
    }

    [Fact]
    public void A_table_grouped_binding_before_advisory_is_silent()
        => Assert.Empty(Run(Header
                            + "| `AAA` | **MUST** be retained. |\n"
                            + "| `BBB` | **MUST NOT** leave the tenancy. |\n"
                            + "| `CCC` | SHOULD be indexed. |\n"));

    // -- the notation a citation is written in --

    [Fact]
    public void A_colon_separated_citation_names_the_form_to_write()
    {
        var found = Notation("Answering `pol-VURM:TIMEBOX` in full.\n");
        var one = Assert.Single(found);
        Assert.Equal("part-ref", one.Check.Value);
        Assert.Contains("'pol-VURM.TIMEBOX'", one.Message);
    }

    [Fact]
    public void A_dot_separated_citation_passes_the_notation_check()
    {
        Assert.Empty(Notation("Answering `pol-VURM.TIMEBOX` in full.\n"));
    }

    // A shortcode carries no type prefix and no hyphen, so a scoped reference is not a citation with the
    // wrong separator and must not be reported as one.
    [Theory]
    [InlineData("eng:pol-VURM")]
    [InlineData("eng:pol-VURM.TIMEBOX")]
    [InlineData("Policy: pol-VURM")]
    [InlineData("ISO27001:2022 A.8.25")]
    public void A_colon_the_corpus_does_use_is_left_alone(string span)
    {
        Assert.Empty(Notation($"Written as `{span}` here.\n"));
    }

    // The check reads no table, so it runs on a type that offers no parts — which is every type a
    // citation is actually written in.
    [Fact]
    public void The_notation_is_checked_where_no_parts_are_declared()
    {
        Assert.Single(Notation("Answering `pol-VURM:TIMEBOX` in full.\n", null));
    }

    // -- parts written as headings --

    private const string Terms = "## Terms\n\n";

    // The heading is the address, so the checks a table needs have nothing to ask here. A glossary's
    // entries are its parts and none of the table's shape applies to them.
    [Fact]
    public void Headings_under_the_section_are_parts_and_nothing_else_is_asked()
        => Assert.Empty(Run(Terms + "### Corpus\n\nA body of records.\n\n### Drift\n\nCopies parting.\n",
            Headings(), "glossary"));

    // The id a heading offers is the anchor a link to it would use, which is what makes a citation and a
    // link name the same thing.
    [Fact]
    public void A_headings_address_is_the_anchor_it_slugs_to()
    {
        var doc = Parse(Terms + "### Identity line\n\nThe line beneath the H1.\n",
            Headings(), "glossary").Item1;

        var part = Assert.Single(doc.Parts);
        Assert.Equal("identity-line", part.Id);
        Assert.Equal("Identity line", part.Text);
    }

    // Two entries slugging to one address is the glossary's version of a repeated clause id, and the
    // message uses the word the type's own readers do.
    [Fact]
    public void Two_headings_slugging_alike_collide()
    {
        var found = Run(Terms + "### Identity line\n\nOne.\n\n### Identity-line\n\nAnother.\n",
            Headings(), "glossary");

        var one = Assert.Single(found);
        Assert.Equal("part-id-unique", one.Check.Value);
        Assert.Contains("two terms here address as 'identity-line'", one.Message);
    }

    // A heading below the declared level is prose inside an entry rather than an entry of its own.
    [Fact]
    public void A_heading_at_another_level_is_not_a_part()
    {
        var doc = Parse(Terms + "### Corpus\n\nA body of records.\n\n#### An aside\n\nMore.\n",
            Headings(), "glossary").Item1;

        Assert.Equal(["corpus"], doc.Parts.Select(p => p.Id));
    }

    // The section bounds the parts: an entry filed outside it offers no address, because a citation
    // resolves against what the type said it would find and nothing else.
    [Fact]
    public void A_heading_outside_the_section_is_not_a_part()
    {
        var doc = Parse(Terms + "### Corpus\n\nA body.\n\n## Notes\n\n### Stray\n\nFiled wrongly.\n",
            Headings(), "glossary").Item1;

        Assert.Equal(["corpus"], doc.Parts.Select(p => p.Id));
    }

    // Nothing else reports this, so without the check a glossary holding no terms validates in silence.
    [Fact]
    public void A_section_holding_no_headings_says_what_belongs_there()
    {
        var found = Run(Terms, Headings(), "glossary");

        var one = Assert.Single(found);
        Assert.Equal("part-none", one.Check.Value);
        Assert.Contains("the '## Terms' section holds no terms — write each one as an H3 heading.", one.Message);
    }

    // A heading at the wrong level is prose rather than a part, so a section holding only those holds no
    // parts and is told so.
    [Fact]
    public void A_section_holding_only_headings_at_another_level_holds_no_parts()
    {
        var found = Run(Terms + "#### Corpus\n\nA body of records.\n", Headings(), "glossary");
        Assert.Equal("part-none", Assert.Single(found).Check.Value);
    }

    // Reported and stopped, so the checks that read parts do not report on a record already known to
    // carry none.
    [Fact]
    public void An_empty_section_is_not_also_reported_as_an_empty_entry()
        => Assert.DoesNotContain(Run(Terms, Headings(), "glossary"), f => f.Check.Value == "part-empty");

    // `clause-table` already tells a table source that its section holds nothing, and one fault arriving
    // under two ids reads as two faults.
    [Fact]
    public void An_empty_table_is_not_also_reported_as_a_section_holding_nothing()
        => Assert.DoesNotContain(Run(Header), f => f.Check.Value == "part-none");

    // Two sound entries stand either side of the empty one, so the message has to say which failed.
    [Fact]
    public void A_heading_with_nothing_under_it_names_the_entry()
    {
        var found = Run(Terms + "### Corpus\n\nA body of records.\n\n### Hollow\n\n### Drift\n\nCopies parting.\n",
            Headings(), "glossary");

        var one = Assert.Single(found);
        Assert.Equal("part-empty", one.Check.Value);
        Assert.Contains("term 'Hollow' has nothing under it", one.Message);
    }

    // The last entry is the one an off-by-one would let through.
    [Fact]
    public void The_last_heading_is_asked_the_same_question()
    {
        var found = Run(Terms + "### Corpus\n\nA body of records.\n\n### Hollow\n", Headings(), "glossary");
        Assert.Equal("part-empty", Assert.Single(found).Check.Value);
    }

    // Read on the source as written, so what a renderer would offer as a block still counts as nothing
    // written.
    [Theory]
    [InlineData("---")]
    [InlineData("—")]
    [InlineData("*")]
    public void A_mark_standing_in_for_the_words_is_still_an_empty_entry(string body)
    {
        var found = Run(Terms + $"### Hollow\n\n{body}\n", Headings(), "glossary");
        Assert.Equal("part-empty", Assert.Single(found).Check.Value);
    }

    // The other half of the guard above: the heading question is never put to a row.
    [Fact]
    public void A_table_row_is_not_asked_whether_a_heading_holds_anything()
        => Assert.DoesNotContain(Run(Header + "| `LOGS` | **MUST** be retained. |\n"),
            f => f.Check.Value == "part-empty");

    // -- driving the checks --

    private static List<Finding> Run(string body) => Run(body, Table(), "policies");

    private static List<Finding> Run(string body, PartSpec? spec, string folder)
    {
        var (doc, type) = Parse(body, spec, folder);

        var found = new List<Finding>();
        PartChecks.Check(doc, type, new Report(doc.Rel, found));
        return found;
    }

    private static List<Finding> Notation(string body) => Notation(body, Table());

    private static List<Finding> Notation(string body, PartSpec? spec)
    {
        var (doc, _) = Parse(body, spec, "policies");

        var found = new List<Finding>();
        PartChecks.CheckNotation(doc, new Report(doc.Rel, found));
        return found;
    }

    // The schema carries the type's prefix as well as its parts, because the parser tells a citation
    // from a filename of the same shape by asking whether the half before the dot opens with one.
    private static (Doc, TypeSchema) Parse(string body, PartSpec? spec, string folder)
    {
        var type = new TypeSchema { Folder = folder, IdPrefix = folder == "glossary" ? "gls" : "pol", Parts = spec };
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { [folder] = type } };
        var doc = Doc.Parse($"{folder}/scrt-security.md", $"---\nid: pol-SCRT\n---\n\n# A record\n\n{body}", schema);
        Assert.NotNull(doc);
        return (doc, type);
    }
}
