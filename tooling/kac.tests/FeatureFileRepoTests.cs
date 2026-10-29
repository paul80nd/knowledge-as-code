using kac.core;

namespace kac.tests;

public class FeatureFileRepoTests
{
    [Fact]
    public void A_path_opening_in_a_repository_a_listed_service_names_is_left_alone()
        => Assert.Empty(Check(
            paths: ["lending/features/renewal.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", ["lending"])]));

    [Fact]
    public void A_path_opening_anywhere_else_is_reported()
    {
        var found = Assert.Single(Check(
            paths: ["qa-pack/features/renewal.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", ["lending"])]));

        Assert.Equal("feature-file-repo", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal(
            "'feature-files: qa-pack/features/renewal.feature' starts in 'qa-pack', which is no repository "
            + "of a service this record implements. Those services are in: lending.",
            found.Message);
    }

    [Fact]
    public void Any_of_the_listed_services_repositories_will_do()
        => Assert.Empty(Check(
            paths: ["platform/tests/browse.feature", "search/features/type-ahead.feature"],
            services: ["svc-catalogue", "svc-search"],
            repos: [("svc-catalogue", ["platform"]), ("svc-search", ["search"])]));

    [Fact]
    public void Any_repository_one_service_lists_will_do()
        => Assert.Empty(Check(
            paths: ["covers-import/features/jackets.feature"],
            services: ["svc-covers-cdn"],
            repos: [("svc-covers-cdn", ["covers-import", "infrastructure"])]));

    [Fact]
    public void The_repositories_the_message_offers_are_sorted()
        => Assert.Contains("Those services are in: alpha, zulu.",
            Assert.Single(Check(
                paths: ["nowhere/x.feature"],
                services: ["svc-z", "svc-a"],
                repos: [("svc-z", ["zulu"]), ("svc-a", ["alpha"])])).Message);

    [Fact]
    public void A_bare_filename_names_no_repository()
        => Assert.Contains("starts in 'renewal.feature'",
            Assert.Single(Check(
                paths: ["renewal.feature"],
                services: ["svc-lending"],
                repos: [("svc-lending", ["lending"])])).Message);

    [Fact]
    public void Every_path_at_fault_is_reported()
        => Assert.Equal(2, Check(
            paths: ["one/a.feature", "two/b.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", ["lending"])]).Count);

    [Fact]
    public void A_repository_is_matched_without_regard_to_casing()
        => Assert.Empty(Check(
            paths: ["Lending/features/renewal.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", ["lending"])]));

    [Fact]
    public void A_path_opening_in_a_repository_named_with_its_project_is_left_alone()
        => Assert.Empty(Check(
            paths: ["proj x/My Repo/features/renewal.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", ["Proj X/My Repo"])]));

    [Fact]
    public void A_path_opening_in_another_repository_of_that_project_is_reported()
    {
        var found = Assert.Single(Check(
            paths: ["Proj X/Other/features/renewal.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", ["Proj X/My Repo"])]));

        Assert.Equal(
            "'feature-files: Proj X/Other/features/renewal.feature' starts in 'Proj X/Other', which is no repository "
            + "of a service this record implements. Those services are in: Proj X/My Repo.",
            found.Message);
    }

    [Fact]
    public void A_bare_repository_beside_one_named_with_its_project_still_matches_on_one_segment()
        => Assert.Empty(Check(
            paths: ["lending/features/renewal.feature", "Proj X/My Repo/features/renewal.feature"],
            services: ["svc-lending", "svc-catalogue"],
            repos: [("svc-lending", ["lending"]), ("svc-catalogue", ["Proj X/My Repo"])]));

    [Fact]
    public void A_record_whose_services_state_no_repository_is_passed_over()
        => Assert.Empty(Check(
            paths: ["qa-pack/x.feature"],
            services: ["svc-lending"],
            repos: [("svc-lending", null)]));

    [Fact]
    public void An_id_naming_no_service_contributes_no_repository()
        => Assert.Empty(Check(
            paths: ["qa-pack/x.feature"],
            services: ["svc-missing"],
            repos: []));

    [Fact]
    public void A_record_listing_no_feature_files_reports_nothing()
        => Assert.Empty(Check(
            paths: [],
            services: ["svc-lending"],
            repos: [("svc-lending", ["lending"])]));

    [Fact]
    public void A_type_with_no_field_referencing_services_is_left_alone()
    {
        var offerings = OfferingType(implementedBy: null);
        Assert.Empty(Run(offerings,
            paths: ["qa-pack/x.feature"], services: [], repos: [("svc-lending", ["lending"])]));
    }

    private static readonly FieldSpec ImplementedBy = new()
        { Name = "implemented-by", Type = "list", Of = "id", Refs = ["services"] };

    private static readonly FieldSpec FeatureFiles = new()
        { Name = "feature-files", Type = "list", Of = "string" };

    private static readonly FieldSpec Repos = new() { Name = "repos", Type = "list", Of = "string" };

    private static TypeSchema OfferingType(FieldSpec? implementedBy)
    {
        var fields = implementedBy is null ? new[] { FeatureFiles } : [implementedBy, FeatureFiles];
        return new TypeSchema
        {
            Key = "offerings",
            Folder = "offerings",
            FieldOrder = [.. fields.Select(f => f.Name)],
            Fields = fields.ToDictionary(f => f.Name)
        };
    }

    private static List<Finding> Check(
        string[] paths, string[] services, (string Id, string[]? Repos)[] repos)
        => Run(OfferingType(ImplementedBy), paths, services, repos);

    /// <summary>
    /// One offering and the services it names, written as frontmatter so the rule reads them exactly as it reads a
    /// corpus.
    /// </summary>
    private static List<Finding> Run(
        TypeSchema offerings, string[] paths, string[] services, (string Id, string[]? Repos)[] repos)
    {
        var serviceType = new TypeSchema
        {
            Key = "services",
            Folder = "services",
            FieldOrder = [Repos.Name],
            Fields = new Dictionary<string, FieldSpec> { [Repos.Name] = Repos }
        };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
                { ["offerings"] = offerings, ["services"] = serviceType }
        };

        var implementedBy = offerings.FieldOrder.Contains(ImplementedBy.Name)
            ? $"{ImplementedBy.Name}: [{string.Join(", ", services)}]\n"
            : "";
        var featureFiles = paths.Length == 0
            ? ""
            : $"{FeatureFiles.Name}: [{string.Join(", ", paths)}]\n";

        var docs = new List<Doc>();
        if (Doc.Parse("offerings/borrowing.md",
                $"---\nid: ofr-borrowing\n{implementedBy}{featureFiles}---\n\n# Borrowing\n", schema)
            is { } offering)
            docs.Add(offering);

        foreach (var (id, repo) in repos)
            if (Doc.Parse($"services/{id}.md",
                    $"---\nid: {id}\n{(repo is null ? "" : $"repos: [{string.Join(", ", repo)}]\n")}---\n\n# {id}\n",
                    schema)
                is { } service)
                docs.Add(service);

        Assert.Equal(1 + repos.Length, docs.Count);

        var byId = docs.ToDictionary(d => d.Scalar("id"), d => d, StringComparer.OrdinalIgnoreCase);
        var found = new List<Finding>();

        new FeatureFileRepo().Check(new CorpusRuleContext(docs, byId, Empty, offerings,
            new RuleSpec { Id = new RuleId("feature-file-repo") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => Report(Sev.Error, at, c, m, l),
            (at, c, m, l) => Report(Sev.Warning, at, c, m, l)));
        return found;

        void Report(Sev severity, Doc at, CheckId check, string message, int? line)
            => found.Add(new Finding(at.Rel, line, severity, check, message));
    }

    /// <summary>The rule reads the records and never the tree.</summary>
    private static readonly Tree Empty = new(new HashSet<string>(StringComparer.Ordinal), _ => "");
}
