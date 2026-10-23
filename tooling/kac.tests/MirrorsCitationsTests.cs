using kac.core;

namespace kac.tests;

public class MirrorsCitationsTests
{
    [Fact]
    public void A_clause_cited_in_another_case_reconciles()
        => Assert.Empty(Reconcile("_**Covers:** [pol-SCRT].rotate_", "pol-SCRT.ROTATE"));

    [Fact]
    public void A_citation_of_a_type_the_field_does_not_reference_is_passed_over()
        => Assert.Empty(Reconcile("_**Covers:** [pol-SCRT].ROTATE, and [std-OTHER].a-rule_",
            "pol-SCRT.ROTATE"));

    [Fact]
    public void A_line_naming_only_types_the_field_does_not_reference_is_reported_as_empty()
        => Assert.Equal("this 'Covers' line names nothing it could gather. Name what the section "
                        + "answers, or take the line off.",
            Assert.Single(Reconcile("_**Covers:** [std-OTHER].a-rule_")).Message);

    [Fact]
    public void A_section_closing_on_no_line_at_all_is_not_reported()
        => Assert.Empty(Reconcile("Prose alone, under a heading that covers nothing."));

    [Fact]
    public void A_clause_no_line_names_is_reported_against_the_field()
        => Assert.Equal("'implements' lists 'pol-SCRT.STORE' and no 'Covers' line names it. Close the "
                        + "section that answers it with one, or take the id out of the field.",
            Assert.Single(Reconcile("_**Covers:** [pol-SCRT].ROTATE_", "pol-SCRT.ROTATE", "pol-SCRT.STORE"))
                .Message);

    [Fact]
    public void A_clause_the_field_does_not_list_is_reported_against_the_field()
        => Assert.Equal("a 'Covers' line names 'pol-SCRT.STORE' and 'implements' does not list it.",
            Assert.Single(Reconcile("_**Covers:** [pol-SCRT].ROTATE, [pol-SCRT].STORE_", "pol-SCRT.ROTATE"))
                .Message);

    [Fact]
    public void A_line_that_closes_no_section_is_reported_where_it_sits()
    {
        var found = Assert.Single(Reconcile(
            "_**Covers:** [pol-SCRT].ROTATE_\n\nMore prose under the same heading.", "pol-SCRT.ROTATE"));

        Assert.Equal("this 'Covers' line stands in the middle of a section. Write it as the last thing "
                     + "under the heading it belongs to.", found.Message);
        Assert.Equal(11, found.Line);
    }

    [Fact]
    public void A_line_followed_by_a_heading_below_its_part_is_reported_where_it_sits()
    {
        var found = Assert.Single(InARule("#### Rotating by hand\n\nMore prose under the same rule."));

        Assert.Equal("this 'Covers' line stands in the middle of a section. Write it as the last thing "
                     + "under the heading it belongs to.", found.Message);
        Assert.Equal(15, found.Line);
    }

    [Theory]
    [InlineData("### Store the secret\n\nYou MUST store it encrypted.")]
    [InlineData("## Exceptions\n\nNone.")]
    [InlineData("")]
    public void A_line_followed_by_the_next_part_the_next_section_or_nothing_closes_its_part(string after)
        => Assert.Empty(InARule(after));

    [Fact]
    public void A_line_that_is_both_misplaced_and_empty_is_reported_twice()
        => Assert.Equal(2,
            Reconcile("_**Covers:**_\n\nMore prose under the same heading.").Count);

    /// <summary>
    /// Markdown will not read an underscore with a space against it, so the line is not italic and the underscores
    /// reach the page. The citations still parse, so the field reports nothing about them.
    /// </summary>
    [Theory]
    [InlineData("_**Covers:** [pol-SCRT].ROTATE _")]
    [InlineData("_ **Covers:** [pol-SCRT].ROTATE_")]
    [InlineData("_**Covers:** [pol-SCRT].ROTATE")]
    public void A_line_a_stray_underscore_left_out_of_italic_is_reported_where_it_sits(string body)
    {
        var found = Assert.Single(Reconcile(body, "pol-SCRT.ROTATE"));

        Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                     + "emphasis mark needs a word against it at each end.", found.Message);
        Assert.Equal(11, found.Line);
    }

    [Theory]
    [InlineData("*__Covers:__ [pol-SCRT].ROTATE *")]
    [InlineData("__**Covers:** [pol-SCRT].ROTATE __")]
    public void A_stray_run_of_either_mark_is_reported_where_it_sits(string body)
        => Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.",
            Assert.Single(Reconcile(body, "pol-SCRT.ROTATE")).Message);

    [Fact]
    public void A_citation_on_the_line_after_a_broken_one_is_not_gathered()
        => Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.",
            Assert.Single(Reconcile(
                    "_**Covers:** [pol-SCRT].ROTATE _\nAlso see [pol-SCRT].STORE, in passing.",
                    "pol-SCRT.ROTATE"))
                .Message);

    [Fact]
    public void A_line_that_is_not_italic_is_told_that_and_nothing_else()
        => Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.",
            Assert.Single(Reconcile("_**Covers:** _\n\nMore prose under the same heading.")).Message);

    [Fact]
    public void A_line_that_is_bold_alone_is_not_a_footnote()
        => Assert.Equal("'implements' lists 'pol-SCRT.ROTATE' and no 'Covers' line names it. Close the "
                        + "section that answers it with one, or take the id out of the field.",
            Assert.Single(Reconcile("**Covers:** [pol-SCRT].ROTATE", "pol-SCRT.ROTATE")).Message);

    [Fact]
    public void A_line_naming_a_record_whole_under_a_part_required_field_is_told_to_name_the_part()
    {
        var found = Assert.Single(Reconcile("_**Covers:** [pol-SCRT]_"));

        Assert.Equal("this 'Covers' line names 'pol-SCRT' whole, and 'implements' names a clause. Write "
                     + "'pol-SCRT.<clause>', one entry per clause. A bare id reads as every clause "
                     + "covered.", found.Message);
        Assert.Equal(10, found.Line);
    }

    [Fact]
    public void A_line_naming_several_records_whole_is_reported_once()
        => Assert.Equal("this 'Covers' line names 'pol-SCRT', 'pol-OTHR' whole, and 'implements' names "
                        + "a clause. Write each as '<id>.<clause>', one entry per clause. A bare id "
                        + "reads as every clause covered.",
            Assert.Single(Reconcile("_**Covers:** [pol-SCRT], [pol-OTHR]_")).Message);

    [Fact]
    public void A_record_named_whole_twice_on_one_line_is_reported_once()
        => Assert.Equal("this 'Covers' line names 'pol-SCRT' whole, and 'implements' names a clause. "
                        + "Write 'pol-SCRT.<clause>', one entry per clause. A bare id reads as every "
                        + "clause covered.",
            Assert.Single(Reconcile("_**Covers:** [pol-SCRT], [pol-SCRT]_")).Message);

    [Fact]
    public void A_line_naming_a_record_whole_under_a_field_admitting_one_keeps_the_ordinary_message()
        => Assert.Equal("a 'Covers' line names 'pol-SCRT' and 'implements' does not list it.",
            Assert.Single(Reconcile(Schema(partRequired: false), "_**Covers:** [pol-SCRT]_")).Message);

    [Fact]
    public void A_line_answering_one_of_two_fields_sharing_a_label_is_not_reported_for_the_other()
    {
        var schema = SharedLabelSchema();
        var doc = Doc.Parse("standards/secret-handling.md",
            "---\nid: std-SECRET\nimplements:\n  - pol-SCRT.ROTATE\ndecided-by:\n---\n\n"
            + "# A standard\n\n## A rule\n\n_**Covers:** [pol-SCRT].ROTATE_\n\n[pol-SCRT]: scrt.md\n",
            schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found, Required.Today);
        Assert.DoesNotContain(found, x => x.Check.Value == "mirrors-citations");
    }

    [Fact]
    public void A_line_neither_of_two_fields_sharing_a_label_could_gather_is_reported_once()
    {
        var schema = SharedLabelSchema();
        var doc = Doc.Parse("standards/secret-handling.md",
            "---\nid: std-SECRET\nimplements:\ndecided-by:\n---\n\n"
            + "# A standard\n\n## A rule\n\n_**Covers:** [std-OTHER].a-rule_\n\n[std-OTHER]: other.md\n",
            schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found, Required.Today);
        Assert.Equal("this 'Covers' line names nothing it could gather. Name what the section answers, "
                     + "or take the line off.",
            Assert.Single(found, x => x.Check.Value == "mirrors-citations").Message);
    }

    private static Schema SharedLabelSchema() => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["standards"] = new()
            {
                Key = "standards", Folder = "standards", IdPrefix = "std",
                DeclaredFields =
                [
                    new FieldSpec
                    {
                        Name = "implements", Type = "list", Of = "id", Refs = ["policies"],
                        MirrorsCitations = "Covers"
                    },
                    new FieldSpec
                    {
                        Name = "decided-by", Type = "list", Of = "id", Refs = ["adrs"],
                        MirrorsCitations = "Covers"
                    }
                ]
            },
            ["policies"] = new() { Key = "policies", Folder = "policies", IdPrefix = "pol" },
            ["adrs"] = new() { Key = "adrs", Folder = "adrs", IdPrefix = "adr" }
        }
    };

    /// <summary>
    /// A standard whose <c>implements:</c> reconciles against a <c>Covers</c> line, and the policy its clause ids point
    /// at. Two types, because the reconciliation is scoped to the types the field references.
    /// </summary>
    private static Schema Schema(bool partRequired = true, PartSpec? parts = null) => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["standards"] = new()
            {
                Key = "standards", Folder = "standards", IdPrefix = "std", Parts = parts,
                DeclaredFields =
                [
                    new FieldSpec
                    {
                        Name = "implements", Type = "list", Of = "id", Refs = ["policies"],
                        MirrorsCitations = "Covers", PartRequired = partRequired
                    }
                ]
            },
            ["policies"] = new()
            {
                Key = "policies", Folder = "policies", IdPrefix = "pol",
                Parts = new PartSpec("table", "", [], []) { Noun = "clause", Section = "Clauses" }
            }
        }
    };

    /// <summary>
    /// The definitions are what make each label a link. A shortcut reference with none behind it is a bracket in prose,
    /// and the citation would never be collected at all.
    /// </summary>
    private static List<Finding> Reconcile(string body, params string[] implements)
        => Reconcile(Schema(), body, implements);

    private static List<Finding> Reconcile(Schema schema, string body, params string[] implements)
    {
        var field = string.Concat(implements.Select(id => $"  - {id}\n"));
        var text = $"---\nid: std-SECRET\nimplements:\n{field}---\n\n# A standard\n\n"
                   + $"## A rule\n\n{body}\n\n[pol-SCRT]: scrt.md\n[pol-OTHR]: othr.md\n"
                   + "[std-OTHER]: other.md\n";
        return Findings(schema, text);
    }

    /// <summary>
    /// A <c>Covers</c> line closing the body of an H3 rule under <c>## Rules</c>, the way a standard keeps its parts,
    /// with <paramref name="after"/> written below it.
    /// </summary>
    private static List<Finding> InARule(string after)
    {
        var schema = Schema(parts: new PartSpec(PartSpec.Headings, "", ["MUST"], [])
        {
            Noun = "rule", Section = "Rules"
        });
        var text = "---\nid: std-SECRET\nimplements:\n  - pol-SCRT.ROTATE\n---\n\n# A standard\n\n## Rules\n\n"
                   + "### Rotate the secret\n\nYou MUST rotate it yearly.\n\n_**Covers:** [pol-SCRT].ROTATE_\n\n"
                   + $"{after}\n\n[pol-SCRT]: scrt.md\n";
        return Findings(schema, text);
    }

    private static List<Finding> Findings(Schema schema, string text)
    {
        var doc = Doc.Parse("standards/secret-handling.md", text, schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found, Required.Today);
        return [.. found.Where(x => x.Check.Value == "mirrors-citations")];
    }
}
