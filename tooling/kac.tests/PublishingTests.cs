using kac.core;

// In-process unit tests for how a publishing target addresses what it publishes.
//
// The link forms cannot be shown by a golden fixture: the harness assembles a corpus that is not a git
// repository, so no run there resolves a ref and no run there writes a link. That is the right shape
// for the fixture — it proves the no-links path — and leaves the rules themselves to be proven here.

namespace kac.tests;

public class PublishingTests
{
    private const string Human = "https://github.com/example/corpus/blob";
    private const string Raw = "https://raw.githubusercontent.com/example/corpus";
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";

    private static CorpusDescriptor Descriptor(
        string? target = Publishing.GitHub, string? human = Human, string? raw = Raw, string? prefix = null) =>
        new() { PublishingTarget = target, HumanBase = human, RawBase = raw, PathPrefix = prefix };

    // -- when a corpus can be addressed at all --

    [Fact]
    public void A_github_corpus_with_both_bases_and_a_ref_is_addressable()
    {
        var publishing = Publishing.For(Descriptor(), Sha);

        Assert.NotNull(publishing);
        Assert.Equal(Publishing.GitHub, publishing.Target);
        Assert.Equal(Sha, publishing.Ref);
    }

    // Four ways of being unable to write a link, and one answer to all of them: the caller's question is
    // whether it can write one, and a null says no without asking it to tell the four apart.
    [Theory]
    [InlineData(Publishing.None, Human, Raw, Sha)]           // publishes nowhere
    [InlineData(Publishing.MkDocs, Human, Raw, Sha)]         // a target nothing addresses yet
    [InlineData(Publishing.GitHub, null, Raw, Sha)]          // a target, and no base to build on
    [InlineData(Publishing.GitHub, Human, Raw, null)]        // no ref, so no stable address
    public void A_corpus_the_tool_cannot_address_resolves_to_nothing(
        string? target, string? human, string? raw, string? gitRef)
    {
        Assert.Null(Publishing.For(Descriptor(target, human, raw), gitRef));
    }

    [Fact]
    public void A_descriptor_stating_no_target_resolves_to_nothing()
    {
        Assert.Null(Publishing.For(Descriptor(target: null), Sha));
    }

    // -- the links themselves --

    [Fact]
    public void A_record_is_read_at_one_address_and_fetched_at_another()
    {
        var links = Publishing.For(Descriptor(), Sha)!.Links("glossary/search.md");

        Assert.Equal($"{Human}/{Sha}/glossary/search.md", links.Human);
        Assert.Equal($"{Raw}/{Sha}/glossary/search.md", links.Raw);
    }

    // The anchor lands a person on the part. The raw link takes none: raw source is text and offers
    // nowhere to land, so a fragment there would look like an address and be none.
    [Fact]
    public void A_part_anchors_the_human_link_and_leaves_the_raw_one_alone()
    {
        var links = Publishing.For(Descriptor(), Sha)!.Links("glossary/search.md", "query");

        Assert.Equal($"{Human}/{Sha}/glossary/search.md#query", links.Human);
        Assert.Equal($"{Raw}/{Sha}/glossary/search.md", links.Raw);
    }

    // Every link names the commit rather than a branch, so a citation says what the agent read.
    [Fact]
    public void Both_links_resolve_against_the_ref_and_not_against_a_branch()
    {
        var links = Publishing.For(Descriptor(), Sha)!.Links("glossary/search.md");

        Assert.Contains(Sha, links.Human);
        Assert.Contains(Sha, links.Raw);
        Assert.DoesNotContain("/main/", links.Human);
    }

    // A base written with a trailing slash is the same base, and a corpus should not have to know that
    // the mechanism is about to append one.
    [Fact]
    public void A_trailing_slash_on_a_base_does_not_double()
    {
        var links = Publishing.For(Descriptor(human: Human + "/", raw: Raw + "/"), Sha)!
            .Links("glossary/search.md");

        Assert.Equal($"{Human}/{Sha}/glossary/search.md", links.Human);
        Assert.Equal($"{Raw}/{Sha}/glossary/search.md", links.Raw);
    }

    // -- a corpus that is not the repository --

    // The prefix lands between the commit and the record, which is the one place it can go: a corpus in
    // a subdirectory is addressed at that commit, under that folder.
    [Fact]
    public void A_corpus_in_a_subdirectory_is_addressed_under_it()
    {
        var links = Publishing.For(Descriptor(prefix: "example"), Sha)!.Links("glossary/search.md", "query");

        Assert.Equal($"{Human}/{Sha}/example/glossary/search.md#query", links.Human);
        Assert.Equal($"{Raw}/{Sha}/example/glossary/search.md", links.Raw);
    }

    // Whether the descriptor wrote the prefix with slashes says nothing about where the corpus sits, so
    // three spellings of one folder build one link.
    [Theory]
    [InlineData("example")]
    [InlineData("/example")]
    [InlineData("example/")]
    public void A_prefix_written_with_slashes_addresses_the_same_folder(string prefix)
    {
        var links = Publishing.For(Descriptor(prefix: prefix), Sha)!.Links("glossary/search.md");

        Assert.Equal($"{Raw}/{Sha}/example/glossary/search.md", links.Raw);
    }

    // A corpus that is the repository states no prefix, and its links carry no empty segment.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    public void A_corpus_that_is_the_repository_is_addressed_at_its_root(string? prefix)
    {
        var links = Publishing.For(Descriptor(prefix: prefix), Sha)!.Links("glossary/search.md");

        Assert.Equal($"{Raw}/{Sha}/glossary/search.md", links.Raw);
    }

    // The anchor rule is the target's, and GitHub's is the discarding form the corpus's own links
    // already use — which is what lets one citation name a term and a link land on it.
    [Fact]
    public void The_anchor_is_the_form_the_corpus_already_links_with()
    {
        var publishing = Publishing.For(Descriptor(), Sha)!;

        Assert.Equal("identity-line", Publishing.Anchor("Identity line"));
        Assert.Equal(Md.Slug("Identity line"), Publishing.Anchor("Identity line"));
    }

    // -- the templates --

    // A template leaves the path and the anchor to its reader, and settles everything else. What is
    // settled includes the commit: a ref a reader has to copy is forty characters that can be mistyped
    // into a plausible 404, and the wrong one answers as confidently as the right one.
    [Fact]
    public void A_template_leaves_the_path_and_the_anchor_and_settles_the_rest()
    {
        var templates = Publishing.For(Descriptor(), Sha)!.Templates();

        Assert.Equal($"{Human}/{Sha}/{{path}}#{{anchor}}", templates.Human);
        Assert.Equal($"{Raw}/{Sha}/{{path}}", templates.Raw);
    }

    // The prefix is settled in the template alongside the commit. A reader holding a record's path
    // supplies that alone, and cannot join the two in the wrong order.
    [Fact]
    public void A_template_settles_the_subdirectory_too()
    {
        var templates = Publishing.For(Descriptor(prefix: "example"), Sha)!.Templates();

        Assert.Equal($"{Human}/{Sha}/example/{{path}}#{{anchor}}", templates.Human);
        Assert.Equal($"{Raw}/{Sha}/example/{{path}}", templates.Raw);
    }

    // The asymmetry lives in the templates rather than in the reader. Raw source has no fragment to
    // honour, so the raw template offers nowhere to put one and nobody has to remember why.
    [Fact]
    public void Only_the_template_a_person_follows_takes_an_anchor()
    {
        var templates = Publishing.For(Descriptor(), Sha)!.Templates();

        Assert.Contains(Publishing.AnchorToken, templates.Human, StringComparison.Ordinal);
        Assert.DoesNotContain(Publishing.AnchorToken, templates.Raw, StringComparison.Ordinal);
    }

    // A link resolved here and a link a consumer substitutes for itself are the same string. They are
    // built from one rule, so a corpus reading the export cannot be handed two addresses for one part.
    [Theory]
    [InlineData("query")]
    [InlineData(null)]
    public void A_substituted_template_is_the_link_this_class_resolves(string? anchor)
    {
        var publishing = Publishing.For(Descriptor(), Sha)!;
        var templates = publishing.Templates();
        var links = publishing.Links("glossary/search.md", anchor);

        var human = templates.Human.Replace(Publishing.PathToken, "glossary/search.md", StringComparison.Ordinal);
        human = anchor is null
            ? human.Replace($"#{Publishing.AnchorToken}", "", StringComparison.Ordinal)
            : human.Replace(Publishing.AnchorToken, anchor, StringComparison.Ordinal);

        Assert.Equal(links.Human, human);
        Assert.Equal(links.Raw,
            templates.Raw.Replace(Publishing.PathToken, "glossary/search.md", StringComparison.Ordinal));
    }
}
