// Where a generated block goes in a page, over pages built for the purpose. `CliReferenceTests` asks the
// same method about the real documentation, and can only report the answer the pages happen to give.

using kac.core;
using Xunit.Sdk;

namespace kac.tests;

public class CliBlockTests
{
    private const string Name = "usage-validate";

    [Fact]
    public void A_block_between_two_markers_takes_the_body_it_is_given()
    {
        var page = $"# `validate` check a corpus\n\n{Generator.Begin(Name)}\n\nold\n\n{Generator.End(Name)}\n\nAfter.\n";

        var written = CliReference.Replaced(page, Name, "new");

        Assert.Equal(
            $"# `validate` check a corpus\n\n{Generator.Begin(Name)}\n\nnew\n\n{Generator.End(Name)}\n\nAfter.\n",
            written);
    }

    [Fact]
    public void A_page_holding_no_markers_gets_the_block_under_its_heading()
    {
        var page = "# `validate` check a corpus\n\nAfter.\n";

        var written = CliReference.Replaced(page, Name, "new");

        Assert.Equal(
            $"# `validate` check a corpus\n\n{Generator.Begin(Name)}\n\nnew\n\n{Generator.End(Name)}\n\nAfter.\n",
            written);
    }

    // `Generator.SpliceBlock` hands back a page it could not splice untouched, and the caller reads an
    // untouched page as one in step. So a block that lost its end marker would report itself fresh for as
    // long as nobody opened it.
    [Fact]
    public void A_block_that_opens_and_never_closes_is_reported()
    {
        var page = $"# `validate` check a corpus\n\n{Generator.Begin(Name)}\n\nfrozen\n\nAfter.\n";

        var thrown = Assert.Throws<XunitException>(() => CliReference.Replaced(page, Name, "new"));

        Assert.Contains(Name, thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_page_with_no_heading_to_put_a_block_under_says_so()
    {
        var thrown = Assert.Throws<XunitException>(() => CliReference.Replaced("Nothing but prose.\n", Name, "new"));

        Assert.Contains(Name, thrown.Message, StringComparison.Ordinal);
    }
}
