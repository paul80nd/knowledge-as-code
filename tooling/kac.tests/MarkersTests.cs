// Unit tests for the marker protocol: what a splice writes between a pair of markers, and what `update`
// reads on a page carrying them.

using kac.core;

namespace kac.tests;

public class MarkersTests
{
    [Fact]
    public void SpliceBlock_replaces_only_between_the_named_markers()
    {
        const string text = "before\n<!-- BEGIN GENERATED: x -->\nOLD\n<!-- END GENERATED: x -->\nafter";

        var result = Markers.SpliceBlock(text, "x", "NEW");

        Assert.Contains("<!-- BEGIN GENERATED: x -->\n\nNEW\n\n<!-- END GENERATED: x -->", result);
        Assert.DoesNotContain("OLD", result);
        Assert.StartsWith("before\n", result);
        Assert.EndsWith("\nafter", result);
    }

    // A corpus that adopted few types has blocks with nothing to say: no pair of its types is easily
    // confused, none of its words collides. Two blank lines between the markers reads as deleted content.
    [Fact]
    public void SpliceBlock_closes_an_empty_block_on_the_next_line()
    {
        const string text = "<!-- BEGIN GENERATED: x -->\n\nOLD\n\n<!-- END GENERATED: x -->";

        Assert.Equal("<!-- BEGIN GENERATED: x -->\n<!-- END GENERATED: x -->",
            Markers.SpliceBlock(text, "x", ""));
    }

    [Fact]
    public void SpliceBlock_leaves_text_untouched_when_the_marker_is_absent()
    {
        const string text = "no markers here";
        Assert.Equal(text, Markers.SpliceBlock(text, "missing", "NEW"));
    }

    // Authored is what `update --check` compares, so it decides whether an overlay page may carry a
    // corpus-specific table.
    [Fact]
    public void Authored_drops_what_a_generated_block_holds_and_keeps_the_markers()
    {
        const string local = "prose\n<!-- BEGIN GENERATED: a -->\n\n| one |\n\n<!-- END GENERATED: a -->\nafter";
        const string reference =
            "prose\n<!-- BEGIN GENERATED: a -->\n\n| another |\n\n<!-- END GENERATED: a -->\nafter";

        Assert.Equal(Markers.Authored(reference), Markers.Authored(local));
        Assert.Contains("<!-- BEGIN GENERATED: a -->", Markers.Authored(local));
        Assert.Contains("<!-- END GENERATED: a -->", Markers.Authored(local));
        Assert.DoesNotContain("one", Markers.Authored(local));
    }

    [Fact]
    public void Authored_still_sees_a_difference_in_the_prose_around_a_block()
    {
        const string local = "prose\n<!-- BEGIN GENERATED: a -->\nX\n<!-- END GENERATED: a -->\n";
        const string reference = "other prose\n<!-- BEGIN GENERATED: a -->\nX\n<!-- END GENERATED: a -->\n";

        Assert.NotEqual(Markers.Authored(reference), Markers.Authored(local));
    }

    [Fact]
    public void Authored_empties_every_block_on_a_page_that_carries_several()
    {
        const string text = "a\n<!-- BEGIN GENERATED: one -->\nX\n<!-- END GENERATED: one -->\n"
                            + "b\n<!-- BEGIN GENERATED: two -->\nY\n<!-- END GENERATED: two -->\nc";

        var authored = Markers.Authored(text);

        Assert.DoesNotContain("X", authored);
        Assert.DoesNotContain("Y", authored);
        Assert.Contains("\nb\n", authored);
    }

    [Fact]
    public void Authored_compares_the_whole_page_where_a_block_is_never_closed()
    {
        // The generator cannot follow the structure, so nothing is treated as generated and the drift
        // stays visible rather than being masked by a marker someone deleted half of.
        const string text = "a\n<!-- BEGIN GENERATED: one -->\nX\n";

        Assert.Equal(text, Markers.Authored(text));
    }

    [Fact]
    public void Authored_leaves_a_page_with_no_blocks_exactly_as_it_is()
    {
        const string text = "just prose\n";
        Assert.Equal(text, Markers.Authored(text));
    }
}
