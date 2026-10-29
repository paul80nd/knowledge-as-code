using kac.core;

namespace kac.tests;

public class MirrorsRepoLinksTests
{
    [Fact]
    public void An_entry_linked_under_the_heading_is_left_alone()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending)\n"));

    [Fact]
    public void An_entry_nothing_under_the_heading_links_is_reported()
    {
        var found = Assert.Single(Check(
            repos: ["lending"],
            section: "* **Repository**: the lending repository.\n"));

        Assert.Equal("mirrors-repo-links", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal(
            "'repos: lending' is not linked under 'Where it lives'. Link it as "
            + "https://git.example.com/estate/lending, or drop it from `repos`.",
            found.Message);
    }

    [Fact]
    public void A_repository_linked_under_the_heading_that_the_field_omits_is_reported()
    {
        var found = Assert.Single(Check(
            repos: ["lending"],
            section: "* **Repositories**: [`lending`](https://git.example.com/estate/lending) and "
                     + "[`infra`](https://git.example.com/estate/infra)\n"));

        Assert.Equal(
            "'Where it lives' links https://git.example.com/estate/infra, which `repos` does not state. "
            + "Add 'infra' to `repos`, or link a repository this service is changed in.",
            found.Message);
    }

    [Fact]
    public void A_link_reaching_inside_the_repository_still_names_it()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending/tree/main/src)\n"));

    [Fact]
    public void A_corpus_stating_no_base_reports_nothing()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: the lending repository.\n",
            reposBase: null));

    [Fact]
    public void A_base_ending_in_a_separator_is_not_a_second_one()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending)\n",
            reposBase: "https://git.example.com/estate/"));

    [Fact]
    public void A_link_outside_the_base_is_passed_over()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending), mirrored at "
                     + "[a fork](https://elsewhere.example.com/estate/lending)\n"));

    [Fact]
    public void A_link_outside_the_heading_is_passed_over()
    {
        var found = Assert.Single(Check(
            repos: ["lending"],
            section: "* **Repository**: the lending repository.\n",
            after: "## Dependencies\n\n[`infra`](https://git.example.com/estate/infra) builds it.\n"));

        Assert.Contains("'repos: lending' is not linked", found.Message);
    }

    [Fact]
    public void A_record_without_the_heading_reports_nothing()
        => Assert.Empty(Run("---\nid: svc-lending\nrepos: [ lending ]\n---\n\n# Lending\n",
            "https://git.example.com/estate"));

    [Fact]
    public void Each_finding_names_the_line_it_is_about()
    {
        var found = Check(
            repos: ["lending"],
            section: "* **Repositories**: [`infra`](https://git.example.com/estate/infra)\n");

        Assert.Equal([1, 14], found.Select(f => f.Line));
    }

    [Fact]
    public void A_link_is_compared_with_its_segments_percent_decoded()
        => Assert.Empty(Check(
            repos: ["My Repo"],
            section: "* **Repository**: [`My Repo`](https://git.example.com/estate/my%20repo)\n"));

    [Fact]
    public void An_entry_naming_its_owner_resolves_as_written_on_any_other_host()
        => Assert.Empty(Check(
            repos: ["paul80nd/knowledge-as-code"],
            section: "* **Repository**: [`kac`](https://github.com/paul80nd/knowledge-as-code/tree/main)\n",
            reposBase: "https://github.com"));

    [Fact]
    public void A_link_an_owner_entry_omits_is_named_in_the_owner_form()
    {
        var found = Assert.Single(Check(
            repos: ["paul80nd/knowledge-as-code"],
            section: "* **Repositories**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code) and "
                     + "[`skills`](https://github.com/mattpocock/skills/tree/main)\n",
            reposBase: "https://github.com"));

        Assert.Equal(
            "'Where it lives' links https://github.com/mattpocock/skills, which `repos` does not state. "
            + "Add 'mattpocock/skills' to `repos`, or link a repository this service is changed in.",
            found.Message);
    }

    [Fact]
    public void An_organisation_base_resolves_a_project_and_a_repository_in_each_project()
        => Assert.Empty(Check(
            repos: ["Proj X/My Repo", "Platform/infra"],
            section: "* **Repositories**: [`My Repo`](https://dev.azure.com/acme/proj%20x/_git/my%20repo) and "
                     + "[`infra`](https://dev.azure.com/Acme/Platform/_git/infra?path=/src)\n",
            reposBase: "https://dev.azure.com/Acme"));

    [Fact]
    public void An_organisation_base_suggests_the_link_to_the_project_s_repository_percent_encoded()
    {
        var found = Assert.Single(Check(
            repos: ["Proj X/My Repo"],
            section: "* **Repository**: the payments repository.\n",
            reposBase: "https://dev.azure.com/Acme"));

        Assert.Equal(
            "'repos: Proj X/My Repo' is not linked under 'Where it lives'. Link it as "
            + "https://dev.azure.com/Acme/Proj%20X/_git/My%20Repo, or drop it from `repos`.",
            found.Message);
    }

    [Fact]
    public void An_organisation_base_names_a_link_no_entry_states_as_its_project_and_repository()
    {
        var found = Assert.Single(Check(
            repos: ["Platform/infra"],
            section: "* **Repositories**: [`infra`](https://dev.azure.com/Acme/Platform/_git/infra) and "
                     + "[`My Repo`](https://dev.azure.com/Acme/Proj%20X/_git/My%20Repo/pullrequests)\n",
            reposBase: "https://dev.azure.com/Acme"));

        Assert.Equal(
            "'Where it lives' links https://dev.azure.com/Acme/Proj%20X/_git/My%20Repo, which `repos` does not "
            + "state. Add 'Proj X/My Repo' to `repos`, or link a repository this service is changed in.",
            found.Message);
    }

    [Fact]
    public void A_project_base_resolves_a_repository_under_its_git_path()
        => Assert.Empty(Check(
            repos: ["My Repo"],
            section: "* **Repository**: [`My Repo`](https://dev.azure.com/acme/proj%20x/_git/my%20repo)\n",
            reposBase: "https://dev.azure.com/acme/Proj%20X"));

    [Fact]
    public void A_project_base_suggests_the_link_under_its_git_path()
    {
        var found = Assert.Single(Check(
            repos: ["My Repo"],
            section: "* **Repository**: the payments repository.\n",
            reposBase: "https://dev.azure.com/acme/Proj%20X"));

        Assert.Contains("Link it as https://dev.azure.com/acme/Proj%20X/_git/My%20Repo,", found.Message);
    }

    [Fact]
    public void A_project_base_passes_over_a_link_into_another_project()
        => Assert.Single(Check(
            repos: ["My Repo"],
            section: "* **Repository**: [`My Repo`](https://dev.azure.com/acme/Platform/_git/My%20Repo)\n",
            reposBase: "https://dev.azure.com/acme/Proj%20X"));

    [Fact]
    public void A_git_base_resolves_a_repository_beneath_it()
        => Assert.Empty(Check(
            repos: ["My Repo"],
            section: "* **Repository**: [`My Repo`](https://dev.azure.com/acme/Proj%20X/_git/My%20Repo)\n",
            reposBase: "https://dev.azure.com/acme/Proj%20X/_git"));

    [Theory]
    [InlineData("https://acme.visualstudio.com", "https://dev.azure.com/acme/Proj%20X/_git/My%20Repo")]
    [InlineData("https://dev.azure.com/acme", "https://acme.visualstudio.com/Proj%20X/_git/My%20Repo")]
    public void An_organisation_answers_on_either_azure_devops_host(string reposBase, string link)
        => Assert.Empty(Check(
            repos: ["Proj X/My Repo"],
            section: $"* **Repository**: [`My Repo`]({link})\n",
            reposBase: reposBase));

    [Fact]
    public void A_visualstudio_project_base_resolves_a_repository_under_its_git_path()
        => Assert.Empty(Check(
            repos: ["My Repo"],
            section: "* **Repository**: [`My Repo`](https://dev.azure.com/acme/Proj%20X/_git/My%20Repo)\n",
            reposBase: "https://acme.visualstudio.com/Proj%20X"));

    [Fact]
    public void A_link_into_another_organisation_is_passed_over()
        => Assert.Single(Check(
            repos: ["Proj X/My Repo"],
            section: "* **Repository**: [`My Repo`](https://dev.azure.com/other/Proj%20X/_git/My%20Repo)\n",
            reposBase: "https://dev.azure.com/acme"));

    [Fact]
    public void A_bare_entry_against_an_organisation_base_is_reported_with_the_form_it_needs()
    {
        var found = Assert.Single(Check(
            repos: ["infra"],
            section: "* **Repository**: [`infra`](https://dev.azure.com/acme/Platform/_git/infra)\n",
            reposBase: "https://dev.azure.com/acme"));

        Assert.Equal(
            "'repos: infra' does not fit `repos-base`, which is an Azure DevOps organisation. "
            + "Write it as '<project>/<repo>'.",
            found.Message);
        Assert.Equal(1, found.Line);
    }

    [Theory]
    [InlineData("https://dev.azure.com/acme/Platform")]
    [InlineData("https://dev.azure.com/acme/Platform/_git")]
    public void An_entry_naming_a_project_against_a_project_base_is_reported_with_the_form_it_needs(
        string reposBase)
    {
        var found = Assert.Single(Check(
            repos: ["Platform/infra"],
            section: "* **Repository**: [`infra`](https://dev.azure.com/acme/Platform/_git/infra)\n",
            reposBase: reposBase));

        Assert.Equal(
            "'repos: Platform/infra' does not fit `repos-base`, which names an Azure DevOps project. "
            + "Write it as '<repo>'.",
            found.Message);
    }

    [Fact]
    public void An_entry_on_another_host_has_no_form_to_fit()
        => Assert.Empty(Check(
            repos: ["infra", "platform/infra"],
            section: "* **Repositories**: [`infra`](https://git.example.com/estate/infra) and "
                     + "[`platform/infra`](https://git.example.com/estate/platform/infra)\n"));

    [Fact]
    public void A_suggested_link_is_percent_encoded_a_segment_at_a_time()
    {
        var found = Assert.Single(Check(
            repos: ["Team A/My Repo"],
            section: "* **Repository**: the payments repository.\n"));

        Assert.Contains("Link it as https://git.example.com/estate/Team%20A/My%20Repo,", found.Message);
    }

    private static List<Finding> Check(
        string[] repos, string section, string? reposBase = "https://git.example.com/estate",
        string after = "")
        => Run($"---\nid: svc-lending\nrepos: [ {string.Join(", ", repos)} ]\n---\n\n"
               + $"# Lending\n\n## What it does\n\nLends.\n\n## Where it lives\n\n{section}\n{after}",
            reposBase);

    /// <summary>
    /// One service record, written as a corpus holds it, so the rule reads it exactly as it reads one.
    /// </summary>
    private static List<Finding> Run(string text, string? reposBase)
    {
        var services = new TypeSchema
        {
            Key = "services",
            Folder = "services",
            FieldOrder = [Repos.Name],
            Fields = new Dictionary<string, FieldSpec> { [Repos.Name] = Repos }
        };

        var schema = new Schema
            { ByFolder = new Dictionary<string, TypeSchema> { ["services"] = services } };

        var doc = Doc.Parse("services/lending.md", text, schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        new MirrorsRepoLinks().Check(new CorpusRuleContext(
            [doc], new Dictionary<string, Doc> { ["svc-lending"] = doc }, Empty, services,
            new RuleSpec { Id = new RuleId("mirrors-repo-links") },
            new Dictionary<string, string>(StringComparer.Ordinal), reposBase,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));
        return found;
    }

    private static readonly FieldSpec Repos = new() { Name = "repos", Type = "list", Of = "string" };

    private static readonly Tree Empty = new(new HashSet<string>(StringComparer.Ordinal), _ => "");
}
