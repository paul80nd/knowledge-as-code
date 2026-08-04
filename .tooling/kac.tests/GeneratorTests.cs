// Unit tests for the pure Generator helpers. The full INDEX/<type>.md generation is covered by the
// golden 'index' scenario; these pin the table/catalogue consistency and the splice in-process.
public class GeneratorTests
{
    [Fact]
    public void ChecksTableProblems_is_empty_for_the_shipped_catalogue()
    {
        // The reader-facing table must stay a faithful, complete view of the catalogue. `kac checks`
        // enforces this out-of-process for CI; this is the same invariant, unit-testable in-process.
        Assert.Empty(Generator.ChecksTableProblems());
    }

    [Fact]
    public void SpliceBlock_replaces_only_between_the_named_markers()
    {
        var text = "before\n<!-- BEGIN GENERATED: x -->\nOLD\n<!-- END GENERATED: x -->\nafter";

        var result = Generator.SpliceBlock(text, "x", "NEW");

        Assert.Contains("<!-- BEGIN GENERATED: x -->\n\nNEW\n\n<!-- END GENERATED: x -->", result);
        Assert.DoesNotContain("OLD", result);
        Assert.StartsWith("before\n", result);
        Assert.EndsWith("\nafter", result);
    }

    [Fact]
    public void SpliceBlock_leaves_text_untouched_when_the_marker_is_absent()
    {
        const string text = "no markers here";
        Assert.Equal(text, Generator.SpliceBlock(text, "missing", "NEW"));
    }
}
