// The branches of `ExportedRecord.CorpusOf`, which the guard beside it runs over whatever a pull request
// changed. Nothing here reads the repository, so each decision stays provable on a path a test writes.

namespace kac.tests;

public class ExportedRecordTests
{
    private static readonly HashSet<string> Corpora = new(StringComparer.Ordinal) { "library", "payments" };

    private static readonly HashSet<string> Exported =
        new(StringComparer.Ordinal) { "standards", "policies" };

    [Fact]
    public void A_record_in_an_exported_type_names_its_corpus()
        => Assert.Equal("library", CorpusOf("examples/library/standards/naming.md"));

    // A corpus files its records in subfolders, and the type reads them all.
    [Fact]
    public void A_record_nested_under_the_type_folder_names_its_corpus()
        => Assert.Equal("payments", CorpusOf("examples/payments/policies/data/retention.md"));

    [Fact]
    public void A_record_of_a_type_that_exports_nothing_is_left_alone()
        => Assert.Null(CorpusOf("examples/library/data/rooms.md"));

    // The type root page, the generated index and the shape a new record starts from. An export ships
    // none of them.
    [Theory]
    [InlineData("examples/library/standards.md")]
    [InlineData("examples/library/standards/_index.md")]
    [InlineData("examples/library/standards/_template.md")]
    public void A_page_the_export_leaves_behind_is_left_alone(string path)
        => Assert.Null(CorpusOf(path));

    [Fact]
    public void A_file_that_is_not_markdown_is_left_alone()
        => Assert.Null(CorpusOf("examples/library/standards/diagram.svg"));

    // `template/` states no stamp to move, and a fixture corpus states one as part of the fixture.
    [Theory]
    [InlineData("template/standards/naming.md")]
    [InlineData("tooling/tests/fixtures/rules/corpus/standards/naming.md")]
    public void A_tree_outside_examples_is_left_alone(string path)
        => Assert.Null(CorpusOf(path));

    // A folder under `examples/` holding no descriptor is not a corpus, whatever it holds.
    [Fact]
    public void A_folder_that_is_not_a_corpus_is_left_alone()
        => Assert.Null(CorpusOf("examples/README.md"));

    [Fact]
    public void A_path_under_an_unknown_corpus_is_left_alone()
        => Assert.Null(CorpusOf("examples/archive/standards/naming.md"));

    private static string? CorpusOf(string rel) => ExportedRecord.CorpusOf(rel, Corpora, Exported);
}
