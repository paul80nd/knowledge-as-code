using kac.core;

// In-process unit tests for the two decisions the link pass turns on: whether a target resolves on
// disk, and whether a bracketed label is an id. Both are fiddly, both are quiet when wrong — a label
// that stops being read as an id simply stops being checked — and the goldens can only reach them
// through a whole corpus.

namespace kac.tests;

public class LinkCheckTests
{
    private static Schema SchemaWith(params TypeSchema[] types) =>
        new() { ByFolder = types.ToDictionary(t => t.Folder) };

    private static TypeSchema Numbered() => new()
        { Folder = "adrs", IdPrefix = "adr", IdStyle = "numbered", IdWidth = 4 };

    private static TypeSchema Mnemonic() => new()
        { Folder = "policies", IdPrefix = "pol", IdStyle = "mnemonic", IdWidth = 4 };

    private static TypeSchema Slug() => new()
        { Folder = "tools", IdPrefix = "tol", IdStyle = "slug" };

    // -- is this label an id? --

    // The canonical form is the id as a document carries it: the prefix always lower-case, a mnemonic
    // always upper-case, a slug always lower-case. `label-canonical` is the difference between the two.
    [Theory]
    [InlineData("adr-0001", "adr-0001")]
    [InlineData("ADR-0001", "adr-0001")]   // the prefix is matched without case and written back lower
    [InlineData("pol-scrt", "pol-SCRT")]   // a mnemonic is written upper
    [InlineData("POL-Scrt", "pol-SCRT")]
    [InlineData("tol-Ripgrep", "tol-ripgrep")]
    public void An_id_shaped_label_is_recognised_and_given_its_canonical_form(string label, string expected)
    {
        Assert.True(LinkChecks.TryCanonicalId(label, SchemaWith(Numbered(), Mnemonic(), Slug()), out var canonical));
        Assert.Equal(expected, canonical);
    }

    // Anything else is prose in brackets, and warns as `bracket-literal` rather than erroring.
    [Theory]
    [InlineData("adr-001")]        // too few digits for the declared width
    [InlineData("adr-00011")]      // too many
    [InlineData("adr-000a")]       // not digits
    [InlineData("pol-1SCR")]       // a mnemonic opens with a letter
    [InlineData("xyz-0001")]       // no type carries that prefix
    [InlineData("an unlinked placeholder")]
    [InlineData("-0001")]          // nothing before the dash
    [InlineData("adr-")]           // nothing after it
    public void Anything_else_is_prose_in_brackets(string label)
        => Assert.False(LinkChecks.TryCanonicalId(label, SchemaWith(Numbered(), Mnemonic(), Slug()), out _));

    // -- does this target resolve? --

    [Fact]
    public void A_target_resolves_absolute_from_the_root_or_relative_to_the_document()
    {
        var root = TempCorpus();

        Assert.True(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "/adrs/0002-b.md"));
        Assert.True(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "0002-b.md"));
        Assert.True(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "../adrs.md"));
        Assert.False(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "/adrs/0099-gone.md"));
    }

    // Azure DevOps resolves a link with the extension left off, so the corpus is written that way and
    // the check has to follow.
    [Fact]
    public void The_md_extension_may_be_omitted()
        => Assert.True(LinkChecks.ResolveTarget(TempCorpus(), "adrs/0001-a.md", "/adrs/0002-b"));

    // A directory is deliberately not a target: `/adrs` is a link to the page `adrs.md`, and accepting
    // the folder as well would resolve a link to a type whose page has gone. Git cannot track an empty
    // directory either, so the same link would pass locally and fail in CI.
    [Fact]
    public void A_directory_is_not_a_target_but_the_page_beside_it_is()
    {
        var root = TempCorpus();
        Assert.True(LinkChecks.ResolveTarget(root, "index.md", "/adrs"));      // resolves as adrs.md
        Assert.False(LinkChecks.ResolveTarget(root, "index.md", "/pictures")); // a folder with no page
    }

    // A fragment or a query is addressing within a target, not a different one.
    [Theory]
    [InlineData("/adrs/0002-b.md#context")]
    [InlineData("/adrs/0002-b.md?raw=1")]
    [InlineData("#a-heading-in-this-document")]
    public void A_fragment_or_query_is_stripped_before_the_target_is_looked_up(string target)
        => Assert.True(LinkChecks.ResolveTarget(TempCorpus(), "adrs/0001-a.md", target));

    [Theory]
    [InlineData("https://example.com/a", true)]
    [InlineData("http://example.com/a", true)]
    [InlineData("mailto:someone@example.com", true)]
    [InlineData("/adrs/0002-b.md", false)]
    public void An_external_target_is_left_alone(string target, bool external)
        => Assert.Equal(external, LinkChecks.IsExternal(target));

    // A corpus on disk, because the resolver asks the filesystem and mocking that would only pin the
    // mock. Built once per class and left for the runner to clear.
    private static string TempCorpus()
    {
        var root = Path.Combine(Path.GetTempPath(), "kac-link-tests");
        Directory.CreateDirectory(Path.Combine(root, "adrs"));
        Directory.CreateDirectory(Path.Combine(root, "pictures"));
        File.WriteAllText(Path.Combine(root, "adrs.md"), "# ADRs\n");
        File.WriteAllText(Path.Combine(root, "index.md"), "# Index\n");
        File.WriteAllText(Path.Combine(root, "adrs", "0001-a.md"), "# A\n");
        File.WriteAllText(Path.Combine(root, "adrs", "0002-b.md"), "# B\n");
        return root;
    }
}
