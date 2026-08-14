using kac.core;

// In-process unit tests for the list of generated files, and for the check that holds a corpus to it.
//
// The list is the point of the module: `index` writes from it and `validate` reads from it, so a case
// here asserts the whole list rather than one entry. A golden fixture proves the pass reaches the CLI;
// it cannot show that the list is complete, because a fixture only holds the files it happens to carry.

namespace kac.tests;

public class GeneratedFilesTests
{
    private static TypeSchema Type(string key, string page) => new()
    {
        Key = key,
        TypeName = key,
        Folder = key,
        Page = page
    };

    private static readonly TypeSchema[] TwoTypes = [Type("adrs", "adrs.md"), Type("policies", "policies.md")];

    [Fact]
    public void Every_generated_file_and_its_blocks_are_named_once()
    {
        var files = GeneratedFiles.Blocks(TwoTypes);

        Assert.Equal(
        [
            ("adrs.md", "schema-adrs,checks-adrs", true),
            ("policies.md", "schema-policies,checks-policies", true),
            ("knowledge-as-code/metadata.md", "schema-universal,types-metadata", true),
            ("knowledge-as-code/taxonomy.md",
                "types-placement,types-detail,types-versus,types-graph,types-edges", true),
            ("knowledge-as-code/lineage.md", "types-lineage,types-collisions", true),
            ("README.md", "types-index", false)
        ], files.Select(f => (f.Path, string.Join(',', f.Blocks), f.MarkersRequired)));
    }

    // The framework's own pages are the corpus's however few types it took, so their blocks are named
    // whether or not anything is generated into them. A corpus that adopted nothing still holds the files.
    [Fact]
    public void The_framework_pages_are_named_for_a_corpus_that_adopted_no_types()
    {
        var files = GeneratedFiles.Blocks([]);

        Assert.Equal(
            [
                "knowledge-as-code/metadata.md", "knowledge-as-code/taxonomy.md",
                "knowledge-as-code/lineage.md", "README.md"
            ],
            files.Select(f => f.Path));
    }

    // A type with no page of its own carries no blocks: the two it would have are written into the page,
    // and there is nowhere to put them.
    [Fact]
    public void A_type_without_a_page_names_no_blocks()
    {
        var files = GeneratedFiles.Blocks([Type("glossary", "")]);

        Assert.DoesNotContain(files, f => f.Blocks.Any(b => b.StartsWith("schema-glossary")));
    }

    // `README.md` belongs to the corpus, which decides whether it wants the block at all. Everything else
    // arrives from the framework with its markers, so a marker that has gone is a fault.
    [Fact]
    public void Only_the_corpus_readme_may_decline_its_block()
    {
        var optional = GeneratedFiles.Blocks(TwoTypes).Where(f => !f.MarkersRequired).Select(f => f.Path);

        Assert.Equal(["README.md"], optional);
    }
}

// The marker check itself. The golden fixture trips one of these; the rest are here, because the coverage
// gate reads ids rather than branches and would call the check covered on the strength of the first.
public class GeneratedBlockCheckTests
{
    private static List<Finding> Check(string text, params string[] blocks)
    {
        var findings = new List<Finding>();
        Validator.CheckGeneratedBlocks("page.md", text, blocks, findings);
        return findings;
    }

    private const string Whole = """
                                 # A page

                                 <!-- BEGIN GENERATED: types-index -->
                                 a table
                                 <!-- END GENERATED: types-index -->
                                 """;

    [Fact]
    public void A_block_with_both_markers_is_silent() => Assert.Empty(Check(Whole, "types-index"));

    [Theory]
    [InlineData("<!-- BEGIN GENERATED: types-index -->", "BEGIN marker")]
    [InlineData("<!-- END GENERATED: types-index -->", "END marker")]
    public void A_block_that_lost_one_marker_names_the_one_it_lost(string deleted, string expected)
    {
        var finding = Assert.Single(Check(Whole.Replace(deleted, ""), "types-index"));

        Assert.Equal("generated-block", finding.Check.Value);
        Assert.Contains(expected, finding.Message);
    }

    // The quieter fault, and the reason the check reads a declared list rather than the file: a block
    // deleted outright leaves nothing behind to notice it has gone.
    [Fact]
    public void A_block_that_lost_both_markers_is_still_reported()
    {
        var finding = Assert.Single(Check("# A page\n\nnothing generated here.\n", "types-index"));

        Assert.Contains("missing its markers", finding.Message);
    }

    [Fact]
    public void Markers_in_the_wrong_order_are_reported()
    {
        var finding = Assert.Single(Check(
            "<!-- END GENERATED: types-index -->\n<!-- BEGIN GENERATED: types-index -->", "types-index"));

        Assert.Contains("END marker comes before its BEGIN marker", finding.Message);
    }

    [Fact]
    public void Each_missing_block_is_reported_on_its_own()
    {
        var findings = Check(Whole, "types-index", "types-graph", "types-edges");

        Assert.Equal(["types-graph", "types-edges"],
            findings.Select(f => f.Message.Split('\'')[1]));
    }
}
