using kac.core;

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
            ("knowledge-as-code/metadata.md", "schema-universal,checks-universal,types-metadata", true),
            ("knowledge-as-code/taxonomy.md", "types-placement,types-detail,types-versus,types-graph", true),
            ("knowledge-as-code/lineage.md", "types-lineage,types-collisions", true),
            ("README.md", "types-index", false)
        ], files.Select(f => (f.Path, string.Join(',', f.Blocks), f.MarkersRequired)));
    }

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

    [Fact]
    public void A_type_without_a_page_names_no_blocks()
    {
        var files = GeneratedFiles.Blocks([Type("glossary", "")]);

        Assert.DoesNotContain(files, f => f.Blocks.Any(b => b.StartsWith("schema-glossary")));
    }

    [Fact]
    public void Only_the_framework_pages_come_back_with_an_update()
    {
        var framework = GeneratedFiles.Blocks(TwoTypes).Where(f => f.FromFramework).Select(f => f.Path);

        Assert.Equal(
            ["knowledge-as-code/metadata.md", "knowledge-as-code/taxonomy.md", "knowledge-as-code/lineage.md"],
            framework);
    }

    [Fact]
    public void Only_the_corpus_readme_may_decline_its_block()
    {
        var optional = GeneratedFiles.Blocks(TwoTypes).Where(f => !f.MarkersRequired).Select(f => f.Path);

        Assert.Equal(["README.md"], optional);
    }
}

public class GeneratedFilePlanTests
{
    private static TypeSchema Type(string key) => new()
    {
        Key = key,
        TypeName = key,
        Folder = key,
        Page = $"{key}.md"
    };

    private const string Markers = """
                                   # ADRs

                                   <!-- BEGIN GENERATED: schema-adrs -->
                                   stale
                                   <!-- END GENERATED: schema-adrs -->

                                   <!-- BEGIN GENERATED: checks-adrs -->
                                   stale
                                   <!-- END GENERATED: checks-adrs -->
                                   """;

    private static Tree Corpus(params (string Path, string Text)[] files) => new(
        new HashSet<string>(files.Select(f => f.Path), StringComparer.Ordinal),
        rel => files.First(f => f.Path == rel).Text);

    private static List<GeneratedFiles.GeneratedFile> Plan(IReadOnlyList<TypeSchema> adopted, Tree tree)
        => GeneratedFiles.Plan(new Schema(), adopted, [], tree);

    [Fact]
    public void An_adopted_type_whose_folder_the_corpus_holds_gets_an_index()
    {
        var plan = Plan([Type("adrs")], Corpus(("adrs/0001-a.md", "# A")));

        Assert.Contains(plan, f => f.Path == "adrs/_index.md");
    }

    [Fact]
    public void An_adopted_type_whose_folder_is_absent_gets_nothing()
    {
        var plan = Plan([Type("adrs")], Corpus(("adrs.md", "# ADRs")));

        Assert.DoesNotContain(plan, f => f.Path == "adrs/_index.md");
    }

    [Fact]
    public void A_type_the_corpus_declined_is_not_in_the_plan()
    {
        var plan = Plan([], Corpus(("adrs/0001-a.md", "# A"), ("adrs.md", Markers)));

        Assert.DoesNotContain(plan, f => f.Path.StartsWith("adrs"));
    }

    [Fact]
    public void An_index_the_corpus_does_not_hold_yet_is_written_rather_than_skipped()
    {
        var file = Assert.Single(Plan([Type("adrs")], Corpus(("adrs/0001-a.md", "# A"))),
            f => f.Path == "adrs/_index.md");

        Assert.Null(file.Current);
        Assert.True(file.Stale);
    }

    [Fact]
    public void A_block_carrying_file_the_corpus_does_not_hold_is_skipped()
    {
        var plan = Plan([Type("adrs")], Corpus(("adrs/0001-a.md", "# A")));

        Assert.DoesNotContain(plan, f => f.Path == "adrs.md");
    }

    [Fact]
    public void A_page_holding_what_it_should_is_not_stale()
    {
        var first = Assert.Single(Plan([Type("adrs")], Corpus(("adrs/0001-a.md", "# A"), ("adrs.md", Markers))),
            f => f.Path == "adrs.md");

        var second = Assert.Single(
            Plan([Type("adrs")], Corpus(("adrs/0001-a.md", "# A"), ("adrs.md", first.Wanted))),
            f => f.Path == "adrs.md");

        Assert.True(first.Stale);
        Assert.False(second.Stale);
    }

    [Fact]
    public void A_file_that_carries_no_marker_is_left_as_it_is()
    {
        var file = Assert.Single(Plan([], Corpus(("README.md", "# A corpus\n\nno block here.\n"))),
            f => f.Path == "README.md");

        Assert.Equal(file.Current, file.Wanted);
        Assert.False(file.Stale);
    }
}

public class GeneratedBlockCheckTests
{
    private static List<Finding> Check(string text, params string[] blocks)
    {
        var findings = new List<Finding>();
        Validator.CheckGeneratedBlocks("page.md", text, blocks, false, findings);
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

    [Fact]
    public void A_block_that_lost_both_markers_is_still_reported()
    {
        var finding = Assert.Single(Check("# A page\n\nnothing generated here.\n", "types-index"));

        Assert.Contains("missing its markers", finding.Message);
    }

    [Fact]
    public void A_framework_page_that_lost_a_marker_names_the_command_restoring_it()
    {
        var findings = new List<Finding>();
        Validator.CheckGeneratedBlocks("knowledge-as-code/metadata.md", "# Metadata\n", ["checks-universal"], true,
            findings);

        Assert.Contains("`kac update`", Assert.Single(findings).Message);
    }

    [Fact]
    public void A_page_the_corpus_owns_names_no_update() =>
        Assert.DoesNotContain("`kac update`", Assert.Single(Check("# A page\n", "types-index")).Message);

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
        var findings = Check(Whole, "types-index", "types-versus", "types-graph");

        Assert.Equal(["types-versus", "types-graph"],
            findings.Select(f => f.Message.Split('\'')[1]));
    }
}
