using kac.core;

// In-process unit tests for `mirrors-citations`, which reconciles a field against the footnote lines
// carrying its label. The golden fixture pins the four faults over a corpus; these pin the arms a
// fixture cannot reach, because each needs a document the corpora would report something else about.

namespace kac.tests;

public class MirrorsCitationsTests
{
    // Both sides ignore case, which is what the schema promises. `Except` builds a set of its own and
    // reaches for the default comparer unless it is handed one, so the promise is easy to lose.
    [Fact]
    public void A_clause_cited_in_another_case_reconciles()
        => Assert.Empty(Reconcile("_**Covers:** [pol-SCRT].rotate_", "pol-SCRT.ROTATE"));

    // The field points at policies alone, so a line naming a standard is prose rather than coverage.
    // Reporting it would be a finding the author could not clear: putting the id in the field fails
    // `ref-resolves` instead.
    [Fact]
    public void A_citation_of_a_type_the_field_does_not_reference_is_passed_over()
        => Assert.Empty(Reconcile("_**Covers:** [pol-SCRT].ROTATE, and [std-OTHER].a-rule_",
            "pol-SCRT.ROTATE"));

    // A line naming only what the field cannot carry gathers nothing, so it is empty in the sense the
    // reconciliation reads, whatever it says on the page.
    [Fact]
    public void A_line_naming_only_types_the_field_does_not_reference_is_reported_as_empty()
        => Assert.Equal("this 'Covers' line names nothing it could gather. Name what the section "
                        + "answers, or take the line off.",
            Assert.Single(Reconcile("_**Covers:** [std-OTHER].a-rule_")).Message);

    // A section carrying no line stays silent, which is the point of the key: a rule discharging no
    // clause is ordinary. Only a line somebody wrote is asked to name something.
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

    // The citations still count, so a line put in the wrong place is one finding rather than one per id
    // it named.
    [Fact]
    public void A_line_that_closes_no_section_is_reported_where_it_sits()
    {
        var found = Assert.Single(Reconcile(
            "_**Covers:** [pol-SCRT].ROTATE_\n\nMore prose under the same heading.", "pol-SCRT.ROTATE"));

        Assert.Equal("this 'Covers' line stands in the middle of a section. Write it as the last thing "
                     + "under the heading it belongs to.", found.Message);
        Assert.Equal(11, found.Line);
    }

    // The two faults about a line are independent, so a line that is both misplaced and empty is told
    // both things. Each names a different repair.
    [Fact]
    public void A_line_that_is_both_misplaced_and_empty_is_reported_twice()
        => Assert.Equal(2,
            Reconcile("_**Covers:**_\n\nMore prose under the same heading.").Count);

    // Markdown will not read an underscore with a space against it, so the line is not italic and the
    // underscores reach the page. The citations still parse, so the field reports nothing about them.
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

    // Either mark, since the form writes underscores outside and asterisks inside. An author who
    // reached for the other pair made the same mistake and is told the same thing.
    [Theory]
    [InlineData("*__Covers:__ [pol-SCRT].ROTATE *")]
    [InlineData("__**Covers:** [pol-SCRT].ROTATE __")]
    public void A_stray_run_of_either_mark_is_reported_where_it_sits(string body)
        => Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.",
            Assert.Single(Reconcile(body, "pol-SCRT.ROTATE")).Message);

    // A paragraph runs on past a soft line break, and the italic form stops at its closing mark. So the
    // broken form stops at the break: the prose after one is not the footnote's to gather, and reading
    // it would send the author to the field over a citation the line never claimed.
    [Fact]
    public void A_citation_on_the_line_after_a_broken_one_is_not_gathered()
        => Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.",
            Assert.Single(Reconcile(
                    "_**Covers:** [pol-SCRT].ROTATE _\nAlso see [pol-SCRT].STORE, in passing.",
                    "pol-SCRT.ROTATE"))
                .Message);

    // Reported and stopped. The line above closes no section and the one here gathers nothing, and
    // neither is asked until the line is the form.
    [Fact]
    public void A_line_that_is_not_italic_is_told_that_and_nothing_else()
        => Assert.Equal("this 'Covers' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.",
            Assert.Single(Reconcile("_**Covers:** _\n\nMore prose under the same heading.")).Message);

    // A bold label opening a paragraph is the labelled prose form, which a glossary's `**Not:**` is
    // written in. Only a stray underscore says the author reached for a footnote.
    [Fact]
    public void A_line_that_is_bold_alone_is_not_a_footnote()
        => Assert.Equal("'implements' lists 'pol-SCRT.ROTATE' and no 'Covers' line names it. Close the "
                        + "section that answers it with one, or take the id out of the field.",
            Assert.Single(Reconcile("**Covers:** [pol-SCRT].ROTATE", "pol-SCRT.ROTATE")).Message);

    // Putting the id in the field is what the ordinary message implies, and `ref-resolves` refuses it
    // there. So the line is told what the field would have told the author on the next run, in the
    // target type's own word for a part.
    [Fact]
    public void A_line_naming_a_record_whole_under_a_part_required_field_is_told_to_name_the_part()
    {
        var found = Assert.Single(Reconcile("_**Covers:** [pol-SCRT]_"));

        Assert.Equal("this 'Covers' line names 'pol-SCRT' whole, and 'implements' names a clause. Write "
                     + "'pol-SCRT.<clause>', one entry per clause. A bare id reads as every clause "
                     + "covered.", found.Message);
        Assert.Equal(10, found.Line);
    }

    // Once for the line however many ids it named. One repair reported many times is what the other
    // three faults are shaped to avoid, and this one was written outside that shape.
    [Fact]
    public void A_line_naming_several_records_whole_is_reported_once()
        => Assert.Equal("this 'Covers' line names 'pol-SCRT', 'pol-OTHR' whole, and 'implements' names "
                        + "a clause. Write each as '<id>.<clause>', one entry per clause. A bare id "
                        + "reads as every clause covered.",
            Assert.Single(Reconcile("_**Covers:** [pol-SCRT], [pol-OTHR]_")).Message);

    // The same id twice on one line is one fault. The union this replaced went through a set, so the
    // repeat collapsed there and has to collapse here.
    [Fact]
    public void A_record_named_whole_twice_on_one_line_is_reported_once()
        => Assert.Equal("this 'Covers' line names 'pol-SCRT' whole, and 'implements' names a clause. "
                        + "Write 'pol-SCRT.<clause>', one entry per clause. A bare id reads as every "
                        + "clause covered.",
            Assert.Single(Reconcile("_**Covers:** [pol-SCRT], [pol-SCRT]_")).Message);

    // A field admitting a bare id has nothing to complain about, so the ordinary message is the right
    // one and it stands against the frontmatter.
    [Fact]
    public void A_line_naming_a_record_whole_under_a_field_admitting_one_keeps_the_ordinary_message()
        => Assert.Equal("a 'Covers' line names 'pol-SCRT' and 'implements' does not list it.",
            Assert.Single(Reconcile(Schema(partRequired: false), "_**Covers:** [pol-SCRT]_")).Message);

    // Two fields spelling one label are handed the same lines, so what is wrong with a line is decided
    // across both. A line answering one of them is not empty for the other, and a misplaced line is one
    // finding rather than two.
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
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found);
        Assert.DoesNotContain(found, x => x.Check.Value == "mirrors-citations");
    }

    // The same two fields, and a line neither of them could gather. One finding, because the fault is
    // the line's rather than either field's.
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
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found);
        Assert.Equal("this 'Covers' line names nothing it could gather. Name what the section answers, "
                     + "or take the line off.",
            Assert.Single(found, x => x.Check.Value == "mirrors-citations").Message);
    }

    // Two fields reconciling against one label, each pointing at a type of its own.
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

    // A standard whose `implements:` reconciles against a `Covers` line, and the policy its clause ids
    // point at. Two types, because the reconciliation is scoped to the types the field references and a
    // one-type corpus could never show that. The policy keeps clauses, as the real one does, so a
    // message about a citation of one has the word to use.
    private static Schema Schema(bool partRequired = true) => new()
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

    // Only the findings under test come back. The pass has plenty else to say about a document this
    // bare, and none of it is what these assert.
    //
    // The definitions are what make each label a link. A shortcut reference with none behind it is a
    // bracket in prose, and the citation would never be collected at all.
    private static List<Finding> Reconcile(string body, params string[] implements)
        => Reconcile(Schema(), body, implements);

    private static List<Finding> Reconcile(Schema schema, string body, params string[] implements)
    {
        var field = string.Concat(implements.Select(id => $"  - {id}\n"));
        var text = $"---\nid: std-SECRET\nimplements:\n{field}---\n\n# A standard\n\n"
                   + $"## A rule\n\n{body}\n\n[pol-SCRT]: scrt.md\n[pol-OTHR]: othr.md\n"
                   + "[std-OTHER]: other.md\n";
        var doc = Doc.Parse("standards/secret-handling.md", text, schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found);
        return [.. found.Where(x => x.Check.Value == "mirrors-citations")];
    }
}
