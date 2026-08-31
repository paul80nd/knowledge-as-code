using kac.core;

// In-process unit tests for `mirrors-citations`, which reconciles a field against the footnote lines
// carrying its label. The golden fixture pins the three faults over a corpus; these pin the arms a
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

    // A standard whose `implements:` reconciles against a `Covers` line, and the policy its clause ids
    // point at. Two types, because the reconciliation is scoped to the types the field references and a
    // one-type corpus could never show that.
    private static Schema Schema() => new()
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
                    }
                ]
            },
            ["policies"] = new() { Key = "policies", Folder = "policies", IdPrefix = "pol" }
        }
    };

    // Only the findings under test come back. The pass has plenty else to say about a document this
    // bare, and none of it is what these assert.
    //
    // The definitions are what make each label a link. A shortcut reference with none behind it is a
    // bracket in prose, and the citation would never be collected at all.
    private static List<Finding> Reconcile(string body, params string[] implements)
    {
        var field = string.Concat(implements.Select(id => $"  - {id}\n"));
        var text = $"---\nid: std-SECRET\nimplements:\n{field}---\n\n# A standard\n\n"
                   + $"## A rule\n\n{body}\n\n[pol-SCRT]: scrt.md\n[std-OTHER]: other.md\n";
        var doc = Doc.Parse("standards/secret-handling.md", text, Schema());
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, Schema(), new Tree(new HashSet<string>(), _ => ""), found);
        return [.. found.Where(x => x.Check.Value == "mirrors-citations")];
    }
}
