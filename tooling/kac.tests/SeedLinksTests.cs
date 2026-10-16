using kac.core;

namespace kac.tests;

public class SeedLinksTests
{
    private static readonly DeclinedNames Declined = new(
        new HashSet<string>(["policies", "catalogue"], StringComparer.Ordinal),
        new HashSet<string>(["policies", "services"], StringComparer.Ordinal));

    [Theory]
    [InlineData("That is a [policy](policies.md).", "That is a policy.")]
    [InlineData("That is a [policy](policies.md#scope).", "That is a policy.")]
    [InlineData("That is a [policy](../policies.md).", "That is a policy.")]
    [InlineData("That is a [policy](../policies.md#scope).", "That is a policy.")]
    [InlineData("That is a [service](catalogue.md).", "That is a service.")]
    public void A_link_to_a_declined_type_leaves_its_own_text_standing(string sent, string received)
        => Assert.Equal(received, SeedLinks.Unlinked(sent, Declined));

    [Theory]
    [InlineData("That is a [standard](standards.md).")]
    [InlineData("That is a [standard](../standards.md).")]
    public void A_link_to_an_adopted_type_is_left_alone(string sent)
        => Assert.Equal(sent, SeedLinks.Unlinked(sent, Declined));

    /// <summary>The closed set. <c>SeedLinks.cs</c> says why each of these is left alone.</summary>
    [Theory]
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
                new DeclinedNames(new HashSet<string>(["policies", "standards"], StringComparer.Ordinal),
                    new HashSet<string>(StringComparer.Ordinal))));

    [Fact]
    public void A_corpus_that_declined_nothing_receives_the_page_as_it_was_written()
    {
        const string sent = "That is a [policy](policies.md).";
        Assert.Equal(sent, SeedLinks.Unlinked(sent, DeclinedNames.None));
    }

    [Fact]
    public void The_line_endings_around_an_unlinked_reference_are_untouched()
        => Assert.Equal(
            "That is a policy.\r\nAnd a service.\r\n",
            SeedLinks.Unlinked("That is a [policy](policies.md).\r\nAnd a [service](catalogue.md).\r\n",
                Declined));

    [Theory]
    [InlineData("glossary.md", Manifest.Seed, true)]
    [InlineData("glossary/_template.md", Manifest.Seed, true)]
    [InlineData(".schema/glossary.yaml", Manifest.Seed, false)]
    [InlineData("knowledge-as-code.md", Manifest.Overlay, false)]
    public void The_unlinking_reads_markdown_seeds_and_nothing_else(string to, string layer, bool reaches)
        => Assert.Equal(reaches, SeedLinks.Reaches(new PlannedFile($"template/{to}", to, layer)));

    /// <summary><c>catalogue</c> here is what a link to the declined <c>services</c> type writes.</summary>
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

        Assert.Equal(["catalogue", "policies"], declined.Pages.Order(StringComparer.Ordinal));
        Assert.Equal(["policies", "services"], declined.Folders.Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("Read the [index](policies/_index.md).\nKeep this.\n")]
    [InlineData("Read the [index](../policies/_index.md).\nKeep this.\n")]
    [InlineData("- [{{Service}}](../services/{{a}}.md) describes it.\nKeep this.\n")]
    public void A_line_linking_into_a_declined_folder_is_removed_whole(string sent)
        => Assert.Equal("Keep this.\n", SeedLinks.Unlinked(sent, Declined));

    [Fact]
    public void A_definition_into_a_declined_folder_goes_with_every_line_citing_it()
        => Assert.Equal(
            "* [std-{{a}}] sets the rules.\n\n[std-{{a}}]: ../standards/{{a}}.md\n",
            SeedLinks.Unlinked(
                "* [std-{{a}}] sets the rules.\n* [POL-{{a}}] governs it.\n\n"
                + "[pol-{{a}}]: ../policies/{{a}}.md\n[std-{{a}}]: ../standards/{{a}}.md\n",
                Declined));

    [Fact]
    public void A_removed_line_between_two_blank_lines_takes_one_of_them_with_it()
        => Assert.Equal(
            "Dependencies:\r\n\r\nA note.\r\n",
            SeedLinks.Unlinked("Dependencies:\r\n\r\n* [svc-{{a}}] caps it.\r\n\r\nA note.\r\n"
                               + "[svc-{{a}}]: ../services/{{a}}.md\r\n", Declined));
}
