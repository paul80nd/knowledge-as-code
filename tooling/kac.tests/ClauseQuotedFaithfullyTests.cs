using kac.core;

namespace kac.tests;

public class ClauseQuotedFaithfullyTests
{
    [Fact]
    public void A_quotation_the_clause_still_uses_is_left_alone()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* [std-ERRORS.WHAT] says a failure \"**MUST** be answered with the "
            + "status code that describes it\"."));

    [Fact]
    public void A_quotation_the_clause_no_longer_uses_is_reported()
    {
        var found = Assert.Single(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* [std-ERRORS.WHAT] says a failure \"**MUST** be answered with a `404`\"."));

        Assert.Equal("clause-quoted-faithfully", found.Check.Value);
        Assert.Equal(Sev.Error, found.Severity);
        Assert.Equal(
            "'std-ERRORS.WHAT' does not say \"**MUST** be answered with a `404`\". "
            + "Re-read the clause in standards/error-responses.md and quote it word for word, or cite the "
            + "clause that says this.",
            found.Message);
    }

    [Fact]
    public void A_quotation_wrapped_onto_the_next_line_is_read_whole()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* [std-ERRORS.WHAT] says a failure \"**MUST** be answered with the\n"
            + "  status code that describes it\"."));

    [Fact]
    public void A_clause_wrapped_onto_the_next_line_is_read_whole()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code\n  that describes it.",
            "* [std-ERRORS.WHAT] says a failure \"**MUST** be answered with the "
            + "status code that describes it\"."));

    [Fact]
    public void A_quotation_of_a_second_bullet_under_the_heading_is_read()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.\n"
            + "- A body **MUST** say what the caller can do next.",
            "* [std-ERRORS.WHAT] says a body \"**MUST** say what the caller can do next\"."));

    [Fact]
    public void A_quotation_of_the_heading_itself_is_read()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* [std-ERRORS.WHAT] is titled \"A failure says what happened\"."));

    [Fact]
    public void A_quotation_taken_from_the_next_clause_is_reported()
        => Assert.Single(Run(
            "- A failure **MUST** be answered with the status code that describes it.\n\n"
            + "### BODY: A body is machine-readable\n\n- A body **MUST** be JSON.",
            "* [std-ERRORS.WHAT] says a body \"**MUST** be JSON\"."));

    [Fact]
    public void A_quoted_span_on_a_line_carrying_no_citation_is_left_alone()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "The proof is the `tool` job's log, under the step \"Run kac.core unit tests\".\n\n"
            + "* [std-ERRORS.WHAT] says a failure \"**MUST** be answered with the "
            + "status code that describes it\"."));

    [Fact]
    public void A_quotation_beside_a_citation_written_as_a_code_span_is_read()
        => Assert.Single(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* `std-ERRORS.WHAT` says a failure \"**MUST** be answered with a `404`\"."));

    [Fact]
    public void A_quotation_of_an_imported_clause_is_skipped()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* `eng:std-ERRORS.WHAT` says a failure \"**MUST** be answered with a `404`\"."));

    [Fact]
    public void A_citation_of_a_record_this_corpus_does_not_have_is_skipped()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* `std-MISSING.WHAT` says a failure \"**MUST** be answered with a `404`\"."));

    [Fact]
    public void A_citation_of_a_part_the_record_does_not_have_is_skipped()
        => Assert.Empty(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* `std-ERRORS.BODY` says a failure \"**MUST** be answered with a `404`\"."));

    [Fact]
    public void A_quotation_that_differs_only_in_case_is_reported()
        => Assert.Single(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* [std-ERRORS.WHAT] says a failure \"**must** be answered with the "
            + "status code that describes it\"."));

    [Fact]
    public void Each_quotation_on_one_line_is_read_separately()
        => Assert.Single(Run(
            "- A failure **MUST** be answered with the status code that describes it.",
            "* [std-ERRORS.WHAT] says \"A failure\" and \"is answered with a `404`\"."));

    [Fact]
    public void A_clause_written_as_a_table_row_is_read_as_it_was_written()
        => Assert.Empty(Clauses(
            "* [pol-SCRT.STORE] says a secret \"**MUST NOT** be written into a repository\"."));

    [Fact]
    public void A_quotation_a_table_row_does_not_carry_is_reported()
        => Assert.Single(Clauses(
            "* [pol-SCRT.STORE] says a secret \"**MUST NOT** be written into a build log\"."));

    /// <summary>
    /// The standard's parts are headings, so the clause is the bullet under the one the citation names.
    /// </summary>
    private static List<Finding> Run(string clause, string control) => Findings(
        "standards/error-responses.md",
        "---\nid: std-ERRORS\ntype: standard\ntier: normative\nstatus: active\nowner: human:alex.doe\n---\n\n"
        + "# Error responses\n\n`Standard: std-ERRORS` `ACTIVE`\n\n## Rules\n\n"
        + $"### WHAT: A failure says what happened\n\n{clause}\n",
        "std-ERRORS", control);

    /// <summary>The policy's parts are table rows, so the clause is the row the citation names.</summary>
    private static List<Finding> Clauses(string control) => Findings(
        "policies/secrets.md",
        "---\nid: pol-SCRT\ntype: policy\ntier: normative\nstatus: active\nowner: human:alex.doe\n---\n\n"
        + "# Secrets\n\n`Policy: pol-SCRT` `ACTIVE`\n\n## Clauses\n\n"
        + "| Id | Clause |\n|----|--------|\n"
        + "| `STORE` | **MUST NOT** be written into a repository. |\n",
        "pol-SCRT", control);

    private static List<Finding> Findings(string rel, string text, string id, string control)
    {
        var cited = Doc.Parse(rel, text, Types);
        Assert.NotNull(cited);

        // A shortcut reference renders as plain text until something defines the label, so the two
        // labels the tests reach for are defined here whether a given test writes one or not.
        var body = "---\nid: ctl-0001\ntype: control\ntier: normative\nstatus: active\n"
                   + "owner: human:alex.doe\n---\n\n# A merge waits for the validate job\n\n"
                   + $"`Control: ctl-0001` `ACTIVE`\n\n## What it checks\n\n{control}\n\n"
                   + "[std-ERRORS.WHAT]: "
                   + "../standards/error-responses.md#what-a-failure-says-what-happened\n"
                   + "[pol-SCRT.STORE]: ../policies/secrets.md#store\n";

        var doc = Doc.Parse("controls/0001-merge-gate.md", body, Types);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        new ClauseQuotedFaithfully().Check(new CorpusRuleContext(
            [doc, cited],
            new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase) { [id] = cited },
            new Tree(new HashSet<string>(StringComparer.Ordinal), _ => ""),
            doc.TypeOf(), new RuleSpec { Id = new RuleId("clause-quoted-faithfully") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));

        return found;
    }

    /// <summary>A control keeps no parts of its own, so the citing type declares none.</summary>
    private static readonly Schema Types = new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["controls"] = new() { Key = "controls", IdPrefix = "ctl" },
            ["standards"] = new()
            {
                Key = "standards",
                IdPrefix = "std",
                Parts = new PartSpec(PartSpec.Headings, "^[A-Z][A-Z0-9]{1,6}$", [], []) { Section = "Rules", Level = 3 }
            },
            ["policies"] = new()
            {
                Key = "policies",
                IdPrefix = "pol",
                Parts = new PartSpec(PartSpec.Table, "", ["MUST", "MUST NOT"], []) { Section = "Clauses" }
            }
        }
    };
}
