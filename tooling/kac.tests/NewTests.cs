// Unit tests for the `new` engine: which template file lands where, which never lands at all, and what
// the two composed files say. Every creation here is decided from a listing and a manifest written in the
// test, so nothing writes a tree, clones a repository or reads the network. The last test is the one
// exception, and says why. `tooling/tests/fixtures/new/` is the layer that runs the command over a real
// template.

using kac.core;

namespace kac.tests;

public class NewTests
{
    // A template laid out as this repository lays one out: a `template/` folder relocated to the corpus
    // root, a schema shared from the repository root, and the repository's own machinery withheld.
    private static Manifest Sample() => new()
    {
        Version = 4,
        MinimumTool = "0.5.0",
        Rules =
        [
            new ManifestRule(["manifest.yaml", "template/README.md"], Manifest.Withheld),
            new ManifestRule([".schema/**"], Manifest.Overlay),
            new ManifestRule(["template/knowledge-as-code/**"], Manifest.Overlay, "knowledge-as-code/"),
            new ManifestRule(["template/retired.md"], Manifest.Removed, ""),
            new ManifestRule(["template/azure-pipelines.yml"], Manifest.Seed, "", CiSystem.AzureDevOps),
            new ManifestRule(["template/.github/**"], Manifest.Seed, ".github/", CiSystem.GitHub),
            new ManifestRule(["template/**"], Manifest.Seed, ""),
            new ManifestRule(["**"], Manifest.Withheld)
        ]
    };

    private static NewAnswers Answers(params string[] types) => new()
    {
        Name = "acme-corpus",
        Types = types.Length > 0 ? types : ["adrs"]
    };

    private static Upstream Taken() => new(
        "https://github.com/paul80nd/knowledge-as-code", null, "main", "5fa039b0", 4, "2026-08-24");

    private static IReadOnlySet<string> Set(params string[] files) => files.ToHashSet(StringComparer.Ordinal);

    private static NewPlan Plan(IEnumerable<string> files, NewAnswers? answers = null,
        Func<string, bool>? declines = null) =>
        New.Plan(files.ToHashSet(StringComparer.Ordinal), Sample(), answers ?? Answers(), Taken(),
            declines ?? (_ => false));

    private static string? Landing(NewPlan plan, string from) =>
        plan.Copied.FirstOrDefault(f => f.From == from)?.To;

    [Fact]
    public void A_withheld_file_reaches_no_corpus()
    {
        var plan = Plan(["manifest.yaml", "template/README.md", "template/CLAUDE.md"]);
        Assert.Equal(["template/CLAUDE.md"], plan.Copied.Select(f => f.From));
    }

    [Fact]
    public void A_removed_file_reaches_no_corpus()
    {
        var plan = Plan(["template/retired.md", "template/CLAUDE.md"]);
        Assert.Equal(["template/CLAUDE.md"], plan.Copied.Select(f => f.From));
    }

    [Theory]
    [InlineData("template/CLAUDE.md", "CLAUDE.md")]
    [InlineData("template/adrs/_template.md", "adrs/_template.md")]
    [InlineData("template/knowledge-as-code/taxonomy.md", "knowledge-as-code/taxonomy.md")]
    [InlineData(".schema/adrs.yaml", ".schema/adrs.yaml")]
    public void A_file_lands_where_the_manifest_sends_it(string from, string to)
        => Assert.Equal(to, Landing(Plan([from]), from));

    [Fact]
    public void The_layer_that_sent_a_file_travels_with_it()
    {
        var plan = Plan([".schema/adrs.yaml", "template/CLAUDE.md"]);
        Assert.Equal(Manifest.Overlay, plan.Copied.Single(f => f.To == ".schema/adrs.yaml").Layer);
        Assert.Equal(Manifest.Seed, plan.Copied.Single(f => f.To == "CLAUDE.md").Layer);
    }

    [Fact]
    public void The_plan_is_ordered_by_destination()
    {
        var plan = Plan(["template/policies.md", "template/adrs.md", ".schema/adrs.yaml"]);
        Assert.Equal([".schema/adrs.yaml", "adrs.md", "policies.md"], plan.Copied.Select(f => f.To));
    }

    [Fact]
    public void A_file_no_rule_places_is_named_and_the_template_is_unsound()
    {
        var manifest = new Manifest { Rules = [new ManifestRule([".schema/**"], Manifest.Overlay)] };
        var plan = New.Plan(Set("template/CLAUDE.md"), manifest, Answers(), Taken(), _ => false);

        Assert.Equal(["template/CLAUDE.md"], plan.Unclassified);
        Assert.True(plan.TemplateIsUnsound);
    }

    [Fact]
    public void A_template_every_rule_places_is_sound() => Assert.False(Plan(["template/CLAUDE.md"]).TemplateIsUnsound);

    [Fact]
    public void A_declined_types_schema_page_and_folder_are_not_written()
    {
        var schema = SchemaOf(("adrs", "adrs.md", "adrs"), ("policies", "policies.md", "policies"));
        var plan = Plan(
        [
            ".schema/adrs.yaml", "template/adrs.md", "template/adrs/_template.md",
            ".schema/policies.yaml", "template/policies.md", "template/policies/_template.md"
        ], Answers("adrs"), New.DeclinesTypes(schema, ["adrs"]));

        Assert.Equal([".schema/adrs.yaml", "adrs.md", "adrs/_template.md"], plan.Copied.Select(f => f.To));
        Assert.Equal([".schema/policies.yaml", "policies.md", "policies/_template.md"], plan.DeclinedTypes);
    }

    [Fact]
    public void A_type_is_declined_by_where_its_files_land()
    {
        var declines = New.DeclinesTypes(SchemaOf(("policies", "policies.md", "policies")), []);
        Assert.False(declines("template/policies.md"));
        Assert.True(declines("policies.md"));
    }

    [Theory]
    [InlineData(CiSystem.GitHub, ".github/workflows/kac.yml")]
    [InlineData(CiSystem.AzureDevOps, "azure-pipelines.yml")]
    public void Only_the_chosen_systems_starter_is_written(string ci, string kept)
    {
        var plan = Plan(["template/azure-pipelines.yml", "template/.github/workflows/kac.yml"],
            Answers() with { Ci = ci });

        Assert.Equal([kept], plan.Copied.Select(f => f.To));
        Assert.Single(plan.DeclinedCi);
    }

    [Fact]
    public void A_corpus_building_nowhere_takes_no_starter()
    {
        var plan = Plan(["template/azure-pipelines.yml", "template/.github/workflows/kac.yml"]);

        Assert.Empty(plan.Copied);
        Assert.Equal([".github/workflows/kac.yml", "azure-pipelines.yml"], plan.DeclinedCi);
    }

    [Fact]
    public void A_file_naming_no_system_reaches_every_corpus()
        => Assert.Equal(["CLAUDE.md"], Plan(["template/CLAUDE.md"]).Copied.Select(f => f.To));

    [Fact]
    public void A_system_the_tool_does_not_offer_stops_the_creation()
    {
        var manifest = new Manifest
        {
            Rules = [new ManifestRule(["template/Jenkinsfile"], Manifest.Seed, "", "jenkins")]
        };
        var plan = New.Plan(Set("template/Jenkinsfile"), manifest, Answers(), Taken(), _ => false);

        Assert.Equal(["jenkins"], plan.UnknownCi);
        Assert.Empty(plan.Copied);
        Assert.True(plan.TemplateIsUnsound);
    }

    [Fact]
    public void The_descriptor_is_composed_and_never_copied()
    {
        var plan = Plan(["template/.corpus.yaml"]);
        Assert.DoesNotContain(".corpus.yaml", plan.Copied.Select(f => f.To));
        Assert.Contains(".corpus.yaml", plan.Composed.Select(f => f.Path));
    }

    [Fact]
    public void A_readme_is_composed_where_the_template_sends_none()
        => Assert.Contains("README.md", Plan(["template/README.md"]).Composed.Select(f => f.Path));

    [Fact]
    public void A_readme_the_template_sends_is_kept_instead()
    {
        var manifest = new Manifest { Rules = [new ManifestRule(["template/**"], Manifest.Seed, "")] };
        var plan = New.Plan(Set("template/README.md"), manifest, Answers(), Taken(), _ => false);

        Assert.Equal(["README.md"], plan.Copied.Select(f => f.To));
        Assert.Equal([".corpus.yaml"], plan.Composed.Select(f => f.Path));
    }

    [Fact]
    public void Paths_names_everything_the_creation_writes()
    {
        var plan = Plan(["template/CLAUDE.md", ".schema/adrs.yaml"]);
        Assert.Equal([".corpus.yaml", ".schema/adrs.yaml", "CLAUDE.md", "README.md"], plan.Paths);
    }

    [Fact]
    public void The_descriptor_names_the_corpus_and_the_types_it_adopted()
    {
        var yaml = New.Descriptor(Answers("adrs", "glossary"), Taken());

        Assert.Contains("corpus: acme-corpus\n", yaml);
        Assert.Contains("types:\n  - adrs\n  - glossary\n", yaml);
        Assert.Contains($"descriptor-version: {CorpusDescriptor.Format}\n", yaml);
        Assert.Contains($"update-policy: {CorpusDescriptor.Cautious}\n", yaml);
    }

    [Fact]
    public void The_descriptor_records_where_the_framework_came_from()
    {
        var yaml = New.Descriptor(Answers(), Taken());

        Assert.Contains("  url: https://github.com/paul80nd/knowledge-as-code\n", yaml);
        Assert.Contains("  ref: main\n", yaml);
        Assert.Contains("  commit: 5fa039b0\n", yaml);
        Assert.Contains("  template-version: 4\n", yaml);
        Assert.Contains("  taken-on: \"2026-08-24\"\n", yaml);
    }

    [Fact]
    public void A_key_the_take_could_not_answer_is_written_bare()
    {
        var yaml = New.Descriptor(Answers(), new Upstream("./template", null, null, null, 4, "2026-08-24"));

        Assert.Contains("  ref:\n", yaml);
        Assert.Contains("  commit:\n", yaml);
        Assert.Contains("  path:\n", yaml);
    }

    [Fact]
    public void A_corpus_publishing_nowhere_states_no_bases()
    {
        var yaml = New.Descriptor(Answers(), Taken());

        Assert.Contains($"publishing-target: {Publishing.None}\n", yaml);
        Assert.DoesNotContain("publishing:\n", yaml);
    }

    [Fact]
    public void A_corpus_that_gave_bases_states_them()
    {
        var yaml = New.Descriptor(
            Answers() with
            {
                PublishingTarget = Publishing.GitHub,
                HumanBase = "https://github.com/acme/corpus/blob",
                RawBase = "https://raw.githubusercontent.com/acme/corpus"
            },
            Taken());

        Assert.Contains("publishing:\n  human-base: https://github.com/acme/corpus/blob\n", yaml);
        Assert.Contains("  raw-base: https://raw.githubusercontent.com/acme/corpus\n", yaml);
    }

    [Theory]
    [InlineData("no", "corpus: \"no\"\n")]
    [InlineData("1.5", "corpus: \"1.5\"\n")]
    [InlineData("*star", "corpus: \"*star\"\n")]
    [InlineData("acme: corpus", "corpus: \"acme: corpus\"\n")]
    public void A_name_yaml_would_misread_is_quoted(string name, string expected)
        => Assert.Contains(expected, New.Descriptor(Answers() with { Name = name }, Taken()));

    // Every line of the descriptor a corpus receives, so a comment cannot arrive wrapped by an editor.
    [Fact]
    public void No_descriptor_line_runs_past_120_characters()
        => Assert.DoesNotContain(New.Descriptor(Answers(), Taken()).Split('\n'), l => l.Length > 120);

    [Fact]
    public void The_readme_arrives_carrying_the_markers_for_every_block_declared_on_it()
    {
        var readme = New.Readme(Answers());

        foreach (var block in GeneratedFiles.Blocks([]).Single(f => f.Path == "README.md").Blocks)
        {
            Assert.Contains(Generator.Begin(block), readme);
            Assert.Contains(Generator.End(block), readme);
        }
    }

    [Fact]
    public void The_readme_opens_on_the_corpus_name()
        => Assert.StartsWith("# acme-corpus\n", New.Readme(Answers()));

    [Fact]
    public void No_readme_line_runs_past_120_characters()
        => Assert.DoesNotContain(New.Readme(Answers()).Split('\n'), l => l.Length > 120);

    [Theory]
    [InlineData("0.6.0", "0.6.0")]        // the version it asks for
    [InlineData("0.6.0", "0.7.1")]        // newer
    [InlineData("0.6.0", "0.6.0+abc123")] // the build stamp is not part of the version
    [InlineData("0.6.0", "1.0.0-rc.1")]   // nor is a pre-release tag
    [InlineData(null, "0.1.0")]           // a template asking for nothing
    [InlineData("", "0.1.0")]
    public void A_tool_new_enough_for_the_template_is_not_stopped(string? minimum, string tool)
        => Assert.Null(New.TooOldFor(minimum, tool, "new"));

    [Fact]
    public void A_tool_older_than_the_template_is_told_which_version_to_get()
    {
        var problem = New.TooOldFor("0.6.0", "0.5.0", "new");

        Assert.Contains("needs kac 0.6.0 or newer", problem);
        Assert.Contains("this is 0.5.0", problem);
    }

    [Fact]
    public void A_minimum_that_is_not_a_version_is_reported_as_one()
        => Assert.Contains("which is not a version", New.TooOldFor("latest", "0.6.0", "new"));

    [Fact]
    public void A_tool_version_nothing_can_parse_stops_nothing()
        => Assert.Null(New.TooOldFor("0.6.0", "dev", "new"));

    // The manifest this repository ships, which is the one thing here read from disk. A rule naming a
    // system the tool cannot offer withholds its files from every corpus, and the fault is easiest to
    // read beside the vocabulary rather than in a golden diff.
    [Fact]
    public void Every_system_this_repositorys_manifest_names_is_one_the_tool_offers()
    {
        var manifest = Manifest.LoadFrom(Path.Combine(Repo.Root, "manifest.yaml"));
        var named = manifest.Rules.Select(r => r.Ci).OfType<string>().Distinct(StringComparer.Ordinal);

        Assert.All(named, ci => Assert.Contains(ci, CiSystem.All));
    }

    // A schema holding nothing but the folder, page and key of each type, which is all `DeclinesTypes`
    // reads. Building one here keeps the test off the real `.schema/`, whose types change.
    private static Schema SchemaOf(params (string Key, string Page, string Folder)[] types) => new()
    {
        ByFolder = types.ToDictionary(
            t => t.Key,
            t => new TypeSchema { Key = t.Key, Page = t.Page, Folder = t.Folder },
            StringComparer.Ordinal)
    };
}
