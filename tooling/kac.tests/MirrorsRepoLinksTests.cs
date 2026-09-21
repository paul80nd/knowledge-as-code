using kac.core;

// The rule reconciles a frontmatter list against the links written under one heading, so every case here
// is a record whose `repos:` and whose `Where it lives` are set against each other. The golden fixture
// exercises one of these; the rest are the shapes a second fixture corpus would only repeat.

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

    // A link into a repository states the path inside it, and the entry never does. So the name ends at
    // the next separator and everything after it is passed over.
    [Fact]
    public void A_link_reaching_inside_the_repository_still_names_it()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending/tree/main/src)\n"));

    // A corpus that has stated no base has said where nothing is, so there is nothing to resolve against.
    [Fact]
    public void A_corpus_stating_no_base_reports_nothing()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: the lending repository.\n",
            reposBase: null));

    // A base written with a trailing slash resolves to the same URL as one written without.
    [Fact]
    public void A_base_ending_in_a_separator_is_not_a_second_one()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending)\n",
            reposBase: "https://git.example.com/estate/"));

    // Another estate's host is not this one, so a link to it belongs to whatever states that service's
    // repositories and never to this comparison.
    [Fact]
    public void A_link_outside_the_base_is_passed_over()
        => Assert.Empty(Check(
            repos: ["lending"],
            section: "* **Repository**: [`lending`](https://git.example.com/estate/lending), mirrored at "
                     + "[a fork](https://elsewhere.example.com/estate/lending)\n"));

    // A link under another heading says nothing about where this service lives.
    [Fact]
    public void A_link_outside_the_heading_is_passed_over()
    {
        var found = Assert.Single(Check(
            repos: ["lending"],
            section: "* **Repository**: the lending repository.\n",
            after: "## Dependencies\n\n[`infra`](https://git.example.com/estate/infra) builds it.\n"));

        Assert.Contains("'repos: lending' is not linked", found.Message);
    }

    // A record missing the heading is `required-section`'s to report, and comparing against a section
    // that is not there would report every entry the record states.
    [Fact]
    public void A_record_without_the_heading_reports_nothing()
        => Assert.Empty(Run("---\nid: svc-lending\nrepos: [ lending ]\n---\n\n# Lending\n",
            "https://git.example.com/estate"));

    // A finding names the line the author has to edit: the frontmatter for an entry nothing links, and
    // the link itself for a repository the field omits.
    [Fact]
    public void Each_finding_names_the_line_it_is_about()
    {
        var found = Check(
            repos: ["lending"],
            section: "* **Repositories**: [`infra`](https://git.example.com/estate/infra)\n");

        Assert.Equal([1, 14], found.Select(f => f.Line));
    }

    private static List<Finding> Check(
        string[] repos, string section, string? reposBase = "https://git.example.com/estate",
        string after = "")
        => Run($"---\nid: svc-lending\nrepos: [ {string.Join(", ", repos)} ]\n---\n\n"
               + $"# Lending\n\n## What it does\n\nLends.\n\n## Where it lives\n\n{section}\n{after}",
            reposBase);

    // One service record, written as a corpus holds it so the rule reads the frontmatter and the parsed
    // links exactly as it reads one.
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
