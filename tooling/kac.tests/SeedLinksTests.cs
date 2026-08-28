using kac.core;

namespace kac.tests;

public class SeedLinksTests
{
    private static readonly IReadOnlySet<string> Declined =
        new HashSet<string>(["policies", "services"], StringComparer.Ordinal);

    [Theory]
    [InlineData("That is a [policy](policies.md).", "That is a policy.")]
    [InlineData("That is a [policy](policies.md#scope).", "That is a policy.")]
    [InlineData("That is a [policy](../policies.md).", "That is a policy.")]
    [InlineData("That is a [policy](../policies.md#scope).", "That is a policy.")]
    public void A_link_to_a_declined_type_leaves_its_own_text_standing(string sent, string received)
        => Assert.Equal(received, SeedLinks.Unlinked(sent, Declined));

    [Theory]
    [InlineData("That is a [standard](standards.md).")]
    [InlineData("That is a [standard](../standards.md).")]
    public void A_link_to_an_adopted_type_is_left_alone(string sent)
        => Assert.Equal(sent, SeedLinks.Unlinked(sent, Declined));

    // The closed set. `SeedLinks.cs` says why each of these is left alone.
    [Theory]
    [InlineData("Read the [index](policies/_index.md).")]
    [InlineData("Read the [index](../policies/_index.md).")]
    [InlineData("Read [the policies](https://example.test/policies.md).")]
    [InlineData("Read [the policies](/policies.md).")]
    public void Anything_outside_the_closed_set_is_left_alone(string sent)
        => Assert.Equal(sent, SeedLinks.Unlinked(sent, Declined));

    [Fact]
    public void Every_link_on_one_line_is_answered_for_separately()
        => Assert.Equal(
            "A policy binds, a standard says how, and an [ADR](adrs.md) records the choice.",
            SeedLinks.Unlinked(
                "A [policy](policies.md) binds, a [standard](standards.md) says how, and an "
                + "[ADR](adrs.md) records the choice.",
                new HashSet<string>(["policies", "standards"], StringComparer.Ordinal)));

    [Fact]
    public void A_corpus_that_declined_nothing_receives_the_page_as_it_was_written()
    {
        const string sent = "That is a [policy](policies.md).";
        Assert.Equal(sent, SeedLinks.Unlinked(sent, new HashSet<string>(StringComparer.Ordinal)));
    }

    // Line endings are the template's own. A page checked out with CRLF is written back with CRLF, so a
    // corpus created on Windows does not fail `generate --check` on the day it is created.
    [Fact]
    public void The_line_endings_around_an_unlinked_reference_are_untouched()
        => Assert.Equal(
            "That is a policy.\r\nAnd a service.\r\n",
            SeedLinks.Unlinked("That is a [policy](policies.md).\r\nAnd a [service](services.md).\r\n",
                Declined));

    [Theory]
    [InlineData("glossary.md", Manifest.Seed, true)]
    [InlineData("glossary/_template.md", Manifest.Seed, true)]
    [InlineData(".schema/glossary.yaml", Manifest.Seed, false)]
    [InlineData("knowledge-as-code.md", Manifest.Overlay, false)]
    public void The_unlinking_reads_markdown_seeds_and_nothing_else(string to, string layer, bool reaches)
        => Assert.Equal(reaches, SeedLinks.Reaches(new PlannedFile($"template/{to}", to, layer)));

    // A link carries the page, and a type is free to name one that does not match its folder. So the set
    // is keyed on the page: `catalogue` here is what a link to the declined `services` type writes.
    [Fact]
    public void A_type_is_declined_under_the_name_its_page_carries()
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["_enums.yaml"] = "enums: {}",
            ["_tiers.yaml"] = "tiers: {}",
            ["_checks.yaml"] = "checks: {}",
            ["_universal.yaml"] = "fields: {}",
            ["adrs.yaml"] = "label: Decision\npage: adrs.md",
            ["policies.yaml"] = "label: Policy\npage: policies.md",
            ["services.yaml"] = "label: Service\npage: catalogue.md"
        };

        var declined = SeedLinks.Declined(Schema.Load(files), ["adrs"]);

        Assert.Equal(["catalogue", "policies"], declined.Order(StringComparer.Ordinal));
    }
}
