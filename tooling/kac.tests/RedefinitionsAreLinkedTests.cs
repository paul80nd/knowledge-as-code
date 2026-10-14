using kac.core;

namespace kac.tests;

public class RedefinitionsAreLinkedTests
{
    [Fact]
    public void A_term_defined_in_one_glossary_is_left_alone()
        => Assert.Empty(Run(("general", "### Record\n\nA document filed under a type."),
            ("search", "### Title\n\nThe indexed field.")));

    [Fact]
    public void A_redefinition_neither_entry_links_is_reported_once()
    {
        var found = Assert.Single(Run(("general", "### Title\n\nA work catalogued."),
            ("search", "### Title\n\nThe indexed field.")));

        Assert.Equal("redefinition-unlinked", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal("glossary/general.md", found.File);
        Assert.Equal(
            "'Title' is defined here and in 'glossary/search.md', and neither entry links to the other. "
            + "Link one entry to the other's anchor, as `search.md#title`, in its `**Not:**` line.",
            found.Message);
    }

    [Fact]
    public void The_finding_lands_on_the_glossary_that_narrows_the_other()
    {
        var found = Assert.Single(Narrowing(new() { ["search"] = "gls-general" },
            ("general", "### Title\n\nA work catalogued."), ("search", "### Title\n\nThe indexed field.")));

        Assert.Equal("glossary/search.md", found.File);
        Assert.Contains("as `general.md#title`", found.Message);
    }

    [Fact]
    public void The_finding_lands_on_the_glossary_further_down_its_narrows_chain()
        => Assert.Equal("glossary/search.md", Assert.Single(Narrowing(new() { ["search"] = "gls-libraries" },
            ("general", "### Title\n\nA work catalogued."), ("libraries", "### Loan\n\nA borrowing."),
            ("search", "### Title\n\nThe indexed field."))).File);

    [Fact]
    public void The_suggested_link_is_relative_to_the_glossary_told()
        => Assert.Contains("as `../general.md#title`", Assert.Single(Narrowing(
            new() { ["sub/search"] = "gls-general" },
            ("general", "### Title\n\nA work catalogued."),
            ("sub/search", "### Title\n\nThe indexed field."))).Message);

    [Fact]
    public void The_narrower_entry_linking_the_general_one_is_enough()
        => Assert.Empty(Run(("general", "### Title\n\nA work catalogued."),
            ("search", "### Title\n\nThe indexed field.\n\n**Not:** a work. See [gls-general.title].\n\n"
                       + "[gls-general.title]: general.md#title")));

    [Fact]
    public void The_general_entry_linking_the_narrower_one_is_enough()
        => Assert.Empty(Run(("general", "### Title\n\nA work catalogued. See [the field](search.md#title)."),
            ("search", "### Title\n\nThe indexed field.")));

    [Fact]
    public void A_link_to_the_file_alone_does_not_count()
        => Assert.Single(Run(("general", "### Title\n\nA work catalogued."),
            ("search", "### Title\n\nThe indexed field. See [gls-general].\n\n[gls-general]: general.md")));

    [Fact]
    public void A_link_written_under_another_term_does_not_count()
        => Assert.Single(Run(("general", "### Title\n\nA work catalogued."),
            ("search", "### Query\n\nWhat a reader types. See [a title](general.md#title).\n\n"
                       + "### Title\n\nThe indexed field.")));

    [Fact]
    public void Each_term_two_glossaries_share_is_judged_on_its_own()
    {
        var found = Run(("general", "### Record\n\nA document.\n\n### Title\n\nA work catalogued."),
            ("search", "### Record\n\nAn indexed row. See [gls-general.record].\n\n### Title\n\nThe indexed field."
                       + "\n\n[gls-general.record]: general.md#record"));

        Assert.Contains("'Title'", Assert.Single(found).Message);
    }

    [Fact]
    public void A_term_three_glossaries_share_is_judged_pair_by_pair()
        => Assert.Equal(2, Run(("a", "### Title\n\nOne."),
            ("b", "### Title\n\nTwo. See [gls-a.title].\n\n[gls-a.title]: a.md#title"),
            ("c", "### Title\n\nThree.")).Count);

    private static List<Finding> Run(params (string Slug, string Terms)[] glossaries) => Narrowing([], glossaries);

    /// <summary>
    /// Each glossary is written as a corpus keeps one, so the rule reads its parts as the parser leaves them.
    /// </summary>
    private static List<Finding> Narrowing(
        Dictionary<string, string> narrows, params (string Slug, string Terms)[] glossaries)
    {
        var glossary = new TypeSchema
        {
            Key = "glossary",
            Folder = "glossary",
            Parts = new PartSpec(PartSpec.Headings, "", [], [])
                { Section = "Terms", Noun = "term", Asides = ["Also", "Avoid", "Not"] }
        };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal) { ["glossary"] = glossary }
        };

        var docs = glossaries.Select(g =>
        {
            var narrowed = narrows.TryGetValue(g.Slug, out var broader) ? $"narrows: {broader}\n" : "";
            var id = $"gls-{g.Slug.Replace('/', '-')}";
            var text = $"---\nid: {id}\nstatus: draft\n{narrowed}---\n\n# A title\n\n`Glossary: {id}` `DRAFT`\n\n"
                       + $"## Scope\n\nOne context.\n\n## Terms\n\n{g.Terms}\n";
            var doc = Doc.Parse($"glossary/{g.Slug}.md", text, schema);
            Assert.NotNull(doc);
            return doc;
        }).ToList();

        var tree = new Tree(docs.Select(d => d.Rel).ToHashSet(StringComparer.Ordinal), _ => "");

        var found = new List<Finding>();
        new RedefinitionsAreLinked().Check(new CorpusRuleContext(
            docs, docs.ToDictionary(d => d.FrontScalar("id") ?? d.Rel), tree, glossary,
            new RuleSpec { Id = new RuleId("redefinitions-are-linked") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));
        return found;
    }
}
