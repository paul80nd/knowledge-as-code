using kac.core;

namespace kac.tests;

public class SeedLinksTests
{
    private static readonly DeclinedNames Declined = new(
        new Dictionary<string, string>(StringComparer.Ordinal) { ["policies"] = "policies", ["catalogue"] = "services" },
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
                new DeclinedNames(
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["policies"] = "policies", ["standards"] = "standards"
                    },
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

        Assert.Equal(["catalogue", "policies"], declined.Pages.Keys.Order(StringComparer.Ordinal));
        Assert.Equal("services", declined.Pages["catalogue"]);
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

    [Theory]
    [InlineData("Request a [waiver][pol].", "Request a waiver.")]
    [InlineData("Request a [pol][].", "Request a pol.")]
    [InlineData("Request a [pol].", "Request a pol.")]
    [InlineData("Request a [Pol].", "Request a Pol.")]
    [InlineData("Request a [pol]: soon.", "Request a pol: soon.")]
    [InlineData("Request a [waiver][pol] or a [pol].", "Request a waiver or a pol.")]
    public void A_definition_naming_a_declined_page_goes_and_each_use_keeps_its_text(string sent, string received)
        => Assert.Equal(
            $"{received}\n\n[std]: ../standards.md\n",
            SeedLinks.Unlinked($"{sent}\n\n[pol]: ../policies.md#scope\n[std]: ../standards.md\n", Declined));

    [Theory]
    [InlineData("Request a [waiver][std].\n\n[std]: ../standards.md\n")]
    [InlineData("Request a [std].\n\n[std]: standards.md\n")]
    public void A_definition_naming_an_adopted_page_is_left_alone(string sent)
        => Assert.Equal(sent, SeedLinks.Unlinked(sent, Declined));

    [Fact]
    public void An_inline_link_sharing_a_removed_label_is_left_alone()
        => Assert.Equal(
            "A [pol](https://example.test/) and a pol.\n",
            SeedLinks.Unlinked("A [pol](https://example.test/) and a [pol].\n[pol]: ../policies.md\n", Declined));

    /// <summary><c>catalogue</c> is the page of the declined <c>services</c> type.</summary>
    [Fact]
    public void A_line_citing_a_removed_definition_is_reported_by_the_type_it_named()
    {
        var (_, lines) = SeedLinks.Unlink("# Title\n\nSee the [svc][].\n\n[svc]: catalogue.md\n", Declined);

        var line = Assert.Single(lines);
        Assert.Equal(3, line.Number);
        Assert.Equal(["services"], line.Types);
    }

    [Fact]
    public void A_removed_line_between_two_blank_lines_takes_one_of_them_with_it()
        => Assert.Equal(
            "Dependencies:\r\n\r\nA note.\r\n",
            SeedLinks.Unlinked("Dependencies:\r\n\r\n* [svc-{{a}}] caps it.\r\n\r\nA note.\r\n"
                               + "[svc-{{a}}]: ../services/{{a}}.md\r\n", Declined));

    /// <summary><c>catalogue</c> is the page of the declined <c>services</c> type.</summary>
    [Fact]
    public void A_line_that_lost_a_link_is_reported_by_its_number_and_the_type_it_named()
    {
        var (_, lines) = SeedLinks.Unlink("# Title\n\nThat is a [service](catalogue.md).\n", Declined);

        var line = Assert.Single(lines);
        Assert.Equal(3, line.Number);
        Assert.Equal(["services"], line.Types);
    }

    [Fact]
    public void A_line_is_numbered_on_the_page_as_written_after_a_line_dropped_whole()
    {
        var (text, lines) = SeedLinks.Unlink(
            "Read the [index](policies/_index.md).\nAn [ADR](adrs.md) or a [policy](policies.md).\n", Declined);

        Assert.Equal("An [ADR](adrs.md) or a policy.\n", text);
        Assert.Equal(1, Assert.Single(lines).Number);
    }

    [Fact]
    public void A_line_linking_two_declined_types_names_both_in_the_order_it_linked_them()
    {
        var (_, lines) = SeedLinks.Unlink(
            "A [service](catalogue.md), a [standard](standards.md), a [policy](policies.md) and a "
            + "[service](catalogue.md).\n", Declined);

        Assert.Equal(["services", "policies"], Assert.Single(lines).Types);
    }

    [Fact]
    public void A_page_linking_no_declined_type_reports_no_line()
        => Assert.Empty(SeedLinks.Unlink("That is a [standard](standards.md).\n", Declined).Lines);
}
