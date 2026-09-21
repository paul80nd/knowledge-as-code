// The branches of `RecordPath`, which the guards beside it run over whatever a pull request changed.
// Nothing here reads the repository, so each decision stays provable on a path a test writes.

namespace kac.tests;

public class RecordPathTests
{
    private static readonly HashSet<string> Corpora = new(StringComparer.Ordinal) { "library", "payments" };

    private static readonly HashSet<string> Types = new(StringComparer.Ordinal) { "standards", "policies" };

    [Fact]
    public void A_record_of_a_named_type_is_a_record()
        => Assert.True(RecordPath.IsRecord("standards/naming.md", Types));

    [Fact]
    public void A_record_of_a_type_nobody_named_is_not()
        => Assert.False(RecordPath.IsRecord("data/rooms.md", Types));

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

    // The type root page, the generated index, and the shape a new record starts from. An export ships
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

    [Fact]
    public void A_file_sitting_directly_under_examples_is_left_alone()
        => Assert.Null(CorpusOf("examples/README.md"));

    [Fact]
    public void A_path_under_an_unknown_corpus_is_left_alone()
        => Assert.Null(CorpusOf("examples/archive/standards/naming.md"));

    // The corpus name is stripped before the record test, so a corpus called `standards` is still read
    // as a corpus and never as a type folder.
    [Fact]
    public void A_corpus_named_after_a_type_does_not_pass_for_one()
        => Assert.Null(RecordPath.CorpusOf("examples/standards/README.md",
            new HashSet<string>(StringComparer.Ordinal) { "standards" }, Types));

    private static string? CorpusOf(string rel) => RecordPath.CorpusOf(rel, Corpora, Types);
}
