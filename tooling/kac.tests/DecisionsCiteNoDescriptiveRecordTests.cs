using kac.core;

namespace kac.tests;

public class DecisionsCiteNoDescriptiveRecordTests
{
    [Fact]
    public void A_decision_linking_a_descriptive_record_is_reported()
    {
        var found = Assert.Single(
            Run("## Decision\n\nRoute every call through [the gateway](../services/gateway.md).\n"));

        Assert.Equal("decision-cites-descriptive", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal("adrs/0001-a-decision.md", found.File);
        Assert.Equal(14, found.Line);
        Assert.Equal(
            "'Decision' rests on 'services/gateway.md', a service record. A service changes as the estate does, and "
            + "an ADR is not rewritten once accepted. State the constraint itself here, and cite the service from "
            + "Context or References.",
            found.Message);
    }

    [Fact]
    public void A_consequence_linking_a_descriptive_record_is_reported()
        => Assert.StartsWith("'Consequences' rests on", Assert.Single(
            Run("## Consequences\n\nCalls slow by [the gateway](../services/gateway.md)'s hop.\n")).Message);

    [Theory]
    [InlineData("Context")]
    [InlineData("Alternatives Considered")]
    [InlineData("References")]
    public void A_link_outside_the_asserting_sections_is_left_alone(string section)
        => Assert.Empty(
            Run($"## Decision\n\nRoute every call.\n\n## {section}\n\nSee [the gateway](../services/gateway.md).\n"));

    [Fact]
    public void A_reference_style_link_is_judged_where_it_is_used()
        => Assert.Single(Run("## Decision\n\nRoute every call through [svc-gateway].\n\n## References\n\n"
                             + "Nothing more.\n\n[svc-gateway]: ../services/gateway.md\n"));

    [Fact]
    public void A_link_with_an_anchor_is_judged_by_its_record()
        => Assert.Single(Run("## Decision\n\nRoute every call through [the gateway](../services/gateway.md#owner).\n"));

    [Fact]
    public void A_link_to_a_normative_record_is_left_alone()
        => Assert.Empty(Run("## Decision\n\nFollow [the API standard](../standards/api.md).\n"));

    [Fact]
    public void A_link_to_a_type_page_is_left_alone()
        => Assert.Empty(Run("## Decision\n\nKeep [the services](../services.md) behind a gateway.\n"));

    [Fact]
    public void Each_link_in_a_section_is_reported()
        => Assert.Equal(2, Run("## Decision\n\nRoute through [the gateway](../services/gateway.md).\n\n"
                               + "## Consequences\n\nAdd a hop at [the gateway](../services/gateway.md).\n").Count);

    /// <summary>
    /// The ADR is written as a corpus keeps one, beside a service and a standard it may link.
    /// </summary>
    private static List<Finding> Run(string body)
    {
        var adrs = new TypeSchema { Key = "adrs", TypeName = "adr", Folder = "adrs", Tier = "decided" };
        var services = new TypeSchema
            { Key = "services", TypeName = "service", Folder = "services", Tier = "descriptive" };
        var standards = new TypeSchema
            { Key = "standards", TypeName = "standard", Folder = "standards", Tier = "normative" };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
            {
                ["adrs"] = adrs, ["services"] = services, ["standards"] = standards
            }
        };

        var docs = new (string Rel, string Text)[]
        {
            ("adrs/0001-a-decision.md",
                $"---\nid: adr-0001\nstatus: accepted\n---\n\n# A decision\n\n## Context\n\nWhy.\n\n{body}"),
            ("services/gateway.md", "---\nid: svc-gateway\n---\n\n# Gateway\n"),
            ("standards/api.md", "---\nid: std-API\n---\n\n# API\n")
        }.Select(d =>
        {
            var doc = Doc.Parse(d.Rel, d.Text, schema);
            Assert.NotNull(doc);
            return doc;
        }).ToList();

        var tree = new Tree(docs.Select(d => d.Rel).Append("services.md").ToHashSet(StringComparer.Ordinal), _ => "");

        var found = new List<Finding>();
        new DecisionsCiteNoDescriptiveRecord().Check(new CorpusRuleContext(
            docs, docs.ToDictionary(d => d.FrontScalar("id") ?? d.Rel), tree, adrs,
            new RuleSpec { Id = new RuleId("decisions-cite-no-descriptive-record") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));
        return found;
    }
}
