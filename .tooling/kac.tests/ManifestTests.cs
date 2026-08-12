// Unit tests for Manifest.Resolve — the first-rule-wins glob layering. Manifest.Load and
// CorpusDescriptor.Load read files and are covered by the golden 'mechanism' scenario; the engines that
// read what they return are in MechanismTests.

using kac.core;

namespace kac.tests;

public class ManifestTests
{
    private static Manifest Sample() => new()
    {
        Rules =
        [
            new ManifestRule(["knowledge-as-code/**"], "synced"),
            new ManifestRule(["**/*.md"], "forked"),
            new ManifestRule(["**"], "local") // catch-all
        ]
    };

    [Theory]
    [InlineData("knowledge-as-code/taxonomy.md", "synced")]       // first rule wins
    [InlineData("adrs/0001-x.md", "forked")]                      // falls through to the .md rule
    [InlineData("scripts/build.txt", "local")]                    // only the catch-all matches
    public void Resolve_returns_the_first_matching_rules_layer(string path, string expected)
        => Assert.Equal(expected, Sample().Resolve(path));

    [Fact]
    public void Resolve_is_null_when_no_rule_matches()
    {
        var m = new Manifest { Rules = [new ManifestRule(["knowledge-as-code/**"], "synced")] };
        Assert.Null(m.Resolve("adrs/0001-x.md"));
    }

    // -- the types a corpus has adopted --

    // The two answers are different states, not the same one written two ways. A descriptor that says
    // nothing leaves adoption to the filesystem, so nothing it holds can be surplus to what it declared.
    [Fact]
    public void A_descriptor_declaring_no_types_adopts_everything()
    {
        var descriptor = new CorpusDescriptor();

        Assert.Null(descriptor.Types);
        Assert.True(descriptor.Adopted("adrs"));
        Assert.False(MechanismCheck.Declined(".schema/adrs.yaml", "synced", descriptor));
    }

    [Fact]
    public void A_descriptor_declaring_types_declines_the_schema_files_of_the_rest()
    {
        var descriptor = new CorpusDescriptor { Types = ["adrs"] };

        Assert.True(descriptor.Adopted("adrs"));
        Assert.False(descriptor.Adopted("runbooks"));
        Assert.False(MechanismCheck.Declined(".schema/adrs.yaml", "synced", descriptor));
        Assert.True(MechanismCheck.Declined(".schema/runbooks.yaml", "synced", descriptor));
    }

    // Everything else under `.schema/` belongs to no type and is shared whatever a corpus adopted, so a
    // corpus is never let off holding it.
    [Theory]
    [InlineData(".schema/_universal.yaml")]
    [InlineData(".schema/_tiers.yaml")]
    [InlineData(".schema/README.md")]
    [InlineData(".tooling/kac.cs")]
    [InlineData("runbooks.md")]
    public void Nothing_but_a_type_file_is_ever_declined(string path)
        => Assert.False(MechanismCheck.Declined(path, "synced", new CorpusDescriptor { Types = ["adrs"] }));

    [Fact]
    public void Types_are_read_from_the_descriptor()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"),
            "role: consumer\ntypes:\n  - adrs\n  - policies\n");

        Assert.Equal(["adrs", "policies"], CorpusDescriptor.Load(dir).Types);
    }

    // -- the layer a corpus's role declines --

    // The same decision as declining a type, taken about the tests rather than about the schema. A
    // consumer runs a tool proven upstream, so a fixture it does not hold is neither missing nor drifted.
    [Theory]
    [InlineData("consumer", true)]
    [InlineData("source", false)]
    [InlineData("", false)] // a descriptor that has said nothing is held to everything, as with types
    public void A_consumer_declines_the_verification_layer(string role, bool declined)
    {
        var descriptor = new CorpusDescriptor { Role = role };

        Assert.Equal(!declined, descriptor.Verifies);
        Assert.Equal(declined, MechanismCheck.Declined(".tooling/kac.tests/GlobTests.cs", "verification", descriptor));
        Assert.False(MechanismCheck.Declined(".tooling/kac.cs", "synced", descriptor));
    }

    // -- what a sync records --

    [Fact]
    public void Stamping_rewrites_the_upstream_block_and_leaves_the_commentary()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        var path = Path.Combine(dir, ".corpus.yaml");
        File.WriteAllText(path,
            "role: consumer\n\nupstream:\n  url:               ../src\n"
            + "  mechanism-version: 1\n  synced-from:       # n/a\n  synced-on:         \"2026-01-01\"\n\n"
            + "# Why a divergence is worth accepting.\naccepted-divergences: []\n");

        CorpusDescriptor.Stamp(dir, 3, "../src", "2026-08-11");

        var after = File.ReadAllText(path);
        Assert.Contains("  mechanism-version: 3\n", after);
        Assert.Contains("  synced-from:       ../src\n", after);
        Assert.Contains("  synced-on:         \"2026-08-11\"\n", after);
        Assert.Contains("  url:               ../src\n", after); // untouched: the sync does not own it
        Assert.Contains("# Why a divergence is worth accepting.", after);
    }

    // A descriptor that has never been synced has no block to rewrite, so the first sync opens one.
    [Fact]
    public void Stamping_a_lock_with_no_upstream_block_writes_one()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".corpus.yaml"), "role: consumer\n");

        CorpusDescriptor.Stamp(dir, 3, "../src", "2026-08-11");

        var reloaded = CorpusDescriptor.Load(dir);
        Assert.Equal("consumer", reloaded.Role);
        Assert.Contains("upstream:", File.ReadAllText(Path.Combine(dir, ".corpus.yaml")));
    }
}

