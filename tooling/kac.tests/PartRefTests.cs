using kac.core;

// In-process unit tests for `part-ref`, which is answered across the whole corpus rather than within a
// document: whether a `record-id.part` citation reaches the part it names. Three ways it fails and the
// words differ for each, so what is asserted here is the wording as much as the finding.
//
// The corpus is handed to the check directly, as `RefCheckTests` does. One of the three arms is a
// citation into a type that keeps no parts at all. That arm needs three types in one corpus, two of
// them keeping parts a different way, and a fixture would have to stand every one of them up to say it.

namespace kac.tests;

public class PartRefTests
{
    [Fact]
    public void A_citation_of_a_clause_the_policy_carries_passes()
        => Assert.Empty(Cite("pol-VURM.TIMEBOX"));

    [Fact]
    public void A_citation_of_a_term_the_glossary_carries_passes()
        => Assert.Empty(Cite("gls-house-words.identity-line"));

    // The message names no part, because there is no document to have been looking in.
    [Fact]
    public void A_citation_of_a_document_that_does_not_exist_names_the_document()
    {
        var found = Assert.Single(Cite("pol-ZZZZ.ANY"));

        Assert.Equal("part-ref", found.Check.Value);
        Assert.Equal("'pol-ZZZZ.ANY' cites 'pol-ZZZZ', which does not exist.", found.Message);
    }

    // A general wording would send the author looking through a document for a heading it was never going to carry.
    [Fact]
    public void A_citation_into_a_type_that_keeps_no_parts_says_so()
    {
        var found = Assert.Single(Cite("adr-0002.ANY"));

        Assert.Equal("part-ref", found.Check.Value);
        Assert.Equal("'adr-0002.ANY' addresses a part of adrs/adr-0002.md, and its type offers none. "
                     + "Cite the document as 'adr-0002'.", found.Message);
    }

    // The type's own noun is used, so a glossary is told about a term and a policy about a clause.
    [Theory]
    [InlineData("pol-VURM.MISSING", "cites a clause 'MISSING' that policies/vurm-remediation.md does not carry.")]
    [InlineData("gls-house-words.missing", "cites a term 'missing' that glossary/house-words.md does not carry.")]
    public void A_citation_of_a_part_the_document_does_not_carry_uses_the_types_own_noun(
        string citation, string tail)
    {
        var found = Assert.Single(Cite(citation));

        Assert.Equal("part-ref", found.Check.Value);
        Assert.Equal($"'{citation}' {tail}", found.Message);
    }

    // Ordinal, at both ends: a miscased citation is reported rather than quietly resolved to the entry the
    // author probably meant.
    [Theory]
    [InlineData("pol-VURM.timebox")]
    [InlineData("gls-house-words.Identity-Line")]
    public void A_citation_in_the_wrong_case_reaches_nothing(string citation)
        => Assert.Equal("part-ref", Assert.Single(Cite(citation)).Check.Value);

    // Three types: a policy keeping its parts in a table, a glossary keeping its as headings, and an ADR
    // keeping none. One record of each, and the citation under test is written into a fourth document
    // whose own type keeps nothing. That is where a citation is usually written.
    private static List<Finding> Cite(string citation)
    {
        var policies = new TypeSchema
        {
            Key = "policies", Folder = "policies", IdPrefix = "pol",
            Parts = new PartSpec(PartSpec.Table, "", ["MUST"], []) { Section = "Clauses", Noun = "clause" }
        };
        var glossary = new TypeSchema
        {
            Key = "glossary", Folder = "glossary", IdPrefix = "gls",
            Parts = new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms", Noun = "term", Level = 3 }
        };
        var adrs = new TypeSchema { Key = "adrs", Folder = "adrs", IdPrefix = "adr" };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
                { ["policies"] = policies, ["glossary"] = glossary, ["adrs"] = adrs }
        };

        var docs = new[]
        {
            Parse(schema, "policies/vurm-remediation.md", "pol-VURM",
                "## Clauses\n\n| Id | Clause |\n|----|--------|\n| `TIMEBOX` | **MUST** be timeboxed |\n"),
            Parse(schema, "glossary/house-words.md", "gls-house-words",
                "## Terms\n\n### Identity line\n\nThe line beneath the H1.\n"),
            Parse(schema, "adrs/adr-0002.md", "adr-0002", "## Context\n\nSomething was decided.\n"),
            Parse(schema, "adrs/adr-0001.md", "adr-0001", $"Answering `{citation}` in full.\n")
        }.OfType<Doc>().ToList();

        Assert.Equal(4, docs.Count); // a document that did not parse would quietly shrink the corpus

        var found = new List<Finding>();
        Validator.CheckCorpus(schema, docs, found);
        return found;
    }

    private static Doc? Parse(Schema schema, string rel, string id, string body) =>
        Doc.Parse(rel, $"---\nid: {id}\n---\n\n# {id}\n\n{body}", schema);
}
