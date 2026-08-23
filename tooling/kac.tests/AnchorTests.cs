using kac.core;

// The anchor a heading offers a link. `fragment-resolves` is only as good as this function, and the
// goldens reach it through one corpus and one heading. The interesting cases are the headings nobody
// would write on purpose and every renderer treats differently.

namespace kac.tests;

public class AnchorTests
{
    [Theory]
    [InlineData("ISO 27001", "iso-27001")]
    [InlineData("DORA metrics", "dora-metrics")]
    [InlineData("EN 301 549", "en-301-549")]
    [InlineData("Azure Well-Architected Framework", "azure-well-architected-framework")]
    // These are the forms that make Azure DevOps and GitHub disagree.
    [InlineData("ISO/IEC 27001:2022", "isoiec-270012022")]
    [InlineData("WCAG 2.2 AA", "wcag-22-aa")]
    [InlineData("Scope: central vs repo-local", "scope-central-vs-repo-local")]
    // A heading carrying the `{#id}` attribute syntax folds the attribute into the anchor instead of
    // becoming it. Nothing this corpus publishes through implements that syntax.
    [InlineData("ISO/IEC 27001:2022 {#iso27001-2022}", "isoiec-270012022-iso27001-2022")]
    // Each side of an em dash is a word, and the spaces around it survive as their own hyphens.
    [InlineData("Say less — once", "say-less--once")]
    public void A_heading_becomes_the_anchor_every_renderer_agrees_on(string heading, string slug)
        => Assert.Equal(slug, Md.Slug(heading));

    [Fact]
    public void Every_heading_offers_an_anchor_whatever_its_level()
    {
        var anchors = Md.Anchors("# Frameworks\n\n## Obliged\n\n### ISO 27001\n\ntext\n\n### UK GDPR\n");

        Assert.Equal(["frameworks", "iso-27001", "obliged", "uk-gdpr"], anchors.Order());
    }

    // A heading is read as it renders, so the emphasis and code marks around a word drop out of the
    // anchor. The underscore inside the word stays: it is a character a name may carry, and the fold
    // only breaks on punctuation between names.
    [Fact]
    public void Emphasis_and_code_spans_are_read_through()
        => Assert.Equal(["the-_index-file"], Md.Anchors("## The `_index` **file**\n").Order());
}
