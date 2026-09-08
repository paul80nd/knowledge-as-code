using kac.core;

// In-process unit tests for the decision the link pass turns on: whether a target names something the
// corpus holds. It is fiddly, it is quiet when wrong, and the goldens can only reach it through a whole
// corpus. Whether a bracketed label is an id is asked here too, but answered in IdChecks and tested
// beside it.
//
// Driven through `LinkChecks.Check`, which is what the validator calls, against a `Tree` built from a
// listing. The resolver reads the corpus and never the disk, so the corpus a test needs is a set of
// paths and the text behind them.

namespace kac.tests;

public class LinkCheckTests
{
    [Fact]
    public void A_target_resolves_absolute_from_the_root_or_relative_to_the_document()
    {
        Assert.Empty(Unresolved("adrs/0001-a.md", "[b](/adrs/0002-b.md)"));
        Assert.Empty(Unresolved("adrs/0001-a.md", "[b](0002-b.md)"));
        Assert.Empty(Unresolved("adrs/0001-a.md", "[up](../adrs.md)"));
        Assert.Single(Unresolved("adrs/0001-a.md", "[gone](/adrs/0099-gone.md)"));
    }

    // Azure DevOps resolves a link with the extension left off, so the corpus is written that way and
    // the check has to follow.
    [Fact]
    public void The_md_extension_may_be_omitted()
        => Assert.Empty(Unresolved("adrs/0001-a.md", "[b](/adrs/0002-b)"));

    [Fact]
    public void A_directory_is_not_a_target_but_the_page_beside_it_is()
    {
        Assert.Empty(Unresolved("index.md", "[adrs](/adrs)"));      // resolves as adrs.md
        Assert.Single(Unresolved("index.md", "[pics](/pictures)")); // a folder with no page
    }

    // An ignored file is on the disk of whoever created it and in no clone, so a link to one is dead
    // everywhere the corpus is read.
    [Fact]
    public void A_target_the_corpus_does_not_hold_does_not_resolve()
        => Assert.Single(Unresolved("index.md", "[draft](/_plan/notes.md)"));

    // A fragment or a query addresses within a target. It never names a different one.
    [Theory]
    [InlineData("[b](/adrs/0002-b.md#context)")]
    [InlineData("[b](/adrs/0002-b.md?raw=1)")]
    public void A_fragment_or_query_is_stripped_before_the_target_is_looked_up(string markdown)
        => Assert.Empty(Unresolved("adrs/0001-a.md", markdown));

    // A bare fragment never reaches the resolver.
    [Fact]
    public void A_bare_fragment_names_a_heading_in_this_document()
    {
        Assert.Empty(Findings("adrs/0001-a.md", "# Context\n\n[here](#context)\n"));
        Assert.Single(Findings("adrs/0001-a.md", "# Context\n\n[there](#decision)\n"));
    }

    [Theory]
    [InlineData("https://example.com/a", true)]
    [InlineData("http://example.com/a", true)]
    [InlineData("mailto:someone@example.com", true)]
    [InlineData("/adrs/0002-b.md", false)]
    public void An_external_target_is_left_alone(string target, bool external)
        => Assert.Equal(external, LinkChecks.IsExternal(target));

    // The other half of the pass: what a bracketed label shows the reader.
    [Fact]
    public void A_label_shaped_like_an_id_is_held_against_the_record_it_leads_to()
        => Assert.Equal(
        [
            "reference '[std-BOGUS]' leads to 'standards/workflows.md', whose id is 'std-CI'.",
            "link definition '[std-BOGUS]' leads to 'standards/workflows.md', whose id is 'std-CI'."
        ], Labels("[std-BOGUS] says so.\n\n[std-BOGUS]: /standards/workflows.md\n"));

    // `WORKFLOWS` is longer than the width the type declares, so nothing reads it as an id at all.
    [Fact]
    public void A_label_too_long_to_be_an_id_is_held_against_the_record_too()
        => Assert.Equal(
        [
            "reference '[std-WORKFLOWS]' leads to 'standards/workflows.md', whose id is 'std-CI'.",
            "link definition '[std-WORKFLOWS]' leads to 'standards/workflows.md', whose id is 'std-CI'."
        ], Labels("[std-WORKFLOWS] says so.\n\n[std-WORKFLOWS]: /standards/workflows.md\n"));

    [Fact]
    public void A_mis_cased_label_is_told_the_canonical_form_alone()
        => Assert.Equal(
        [
            "reference '[std-ci]' should be written as the id 'std-CI'.",
            "link definition '[std-ci]' should be written as the id 'std-CI'."
        ], Labels("[std-ci] says so.\n\n[std-ci]: /standards/workflows.md\n"));

    // A renderer resolves a reference to the first definition of a repeated label, so the second is
    // not what the reader follows and is not what the label is held against.
    [Fact]
    public void A_label_defined_twice_is_held_against_the_first_definition()
        => Assert.Empty(Labels("[std-BOGUS] says so.\n\n[std-BOGUS]: /standards.md\n"
                               + "[std-BOGUS]: /standards/workflows.md\n"));

    // A template's definitions demonstrate the form, so the label is one nobody has chosen yet.
    [Fact]
    public void A_template_is_not_held_to_the_record_its_exemplar_points_at()
        => Assert.Empty(Labels("[an example] says so.\n\n[an example]: /standards/workflows.md\n",
            DocKind.Template));

    [Theory]
    [InlineData("[std-CI] says so.\n\n[std-CI]: /standards/workflows.md\n")]
    [InlineData("[std-CI.the-rule] says so.\n\n[std-CI.the-rule]: /standards/workflows.md#the-rule\n")]
    [InlineData("[the standards] says so.\n\n[the standards]: /standards.md\n")] // a page has no id
    [InlineData("[a picture]: /pictures/cat.png\n\nSee [a picture].\n")]             // nor has a png
    public void A_label_that_names_what_it_leads_to_is_silent(string markdown)
        => Assert.Empty(Labels(markdown));

    // Everything `label-canonical` said about one document, in the order it was reported.
    private static List<string> Labels(string markdown, DocKind kind = DocKind.Record) =>
    [
        .. Cited(markdown, kind).Where(f => f.Check.Value == "label-canonical").Select(f => f.Message)
    ];

    private static List<Finding> Cited(string markdown, DocKind kind)
    {
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["standards"] = new()
                {
                    Folder = "standards", IdPrefix = "std", IdStyle = "mnemonic", IdWidth = new(2, 7),
                    FilenameCarriesId = false
                }
            }
        };

        var doc = Required.Parsed("standards/prose.md", "# Prose\n\n" + markdown, schema,
            requireFrontmatter: false);
        var findings = new List<Finding>();
        LinkChecks.Check(doc, schema, Corpus(), new Report(doc.Rel, findings), kind);
        return findings;
    }

    private static List<string> Unresolved(string fromRel, string markdown) =>
    [
        .. Findings(fromRel, markdown).Where(f => f.Check.Value == "link-resolves").Select(f => f.Message)
    ];

    private static List<Finding> Findings(string fromRel, string markdown)
    {
        var schema = new Schema();
        var doc = Required.Parsed(fromRel, markdown, schema, requireFrontmatter: false);
        var findings = new List<Finding>();
        LinkChecks.Check(doc, schema, Corpus(), new Report(doc.Rel, findings));
        return findings;
    }

    // What the corpus holds, as a listing. `_plan/notes.md` is deliberately absent: it is the file on
    // disk that git ignores, and the test above turns on those being two questions.
    private static Tree Corpus() => new(
        new HashSet<string>(StringComparer.Ordinal)
        {
            "adrs.md",
            "index.md",
            "adrs/0001-a.md",
            "adrs/0002-b.md",
            "standards.md",
            "standards/workflows.md",
            "pictures/cat.png"
        },
        rel => rel switch
        {
            "adrs.md" => "# ADRs\n",
            "index.md" => "# Index\n",
            "adrs/0001-a.md" => "# A\n",
            "adrs/0002-b.md" => "# B\n\n## Context\n",
            "standards.md" => "# Standards\n",
            // The id lives in frontmatter and nowhere in the filename, which is the case the filename
            // cannot answer.
            "standards/workflows.md" => "---\nid: std-CI\n---\n\n# Workflows\n\n## The rule\n",
            _ => ""
        });
}
