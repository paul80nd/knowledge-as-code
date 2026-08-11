// Unit tests for Manifest.Resolve — the first-rule-wins glob layering. Manifest.Load,
// MechanismLock.Load and the MechanismCheck engine are covered by the golden 'mechanism' scenario.

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
    [InlineData("knowledge-as-code/manifest.yaml", "synced")]     // first rule wins
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

    // The two answers are different states, not the same one written two ways. A lock that says nothing
    // leaves adoption to the filesystem, so nothing it holds can be surplus to what it declared.
    [Fact]
    public void A_lock_declaring_no_types_adopts_everything()
    {
        var lockFile = new MechanismLock();

        Assert.Null(lockFile.Types);
        Assert.True(lockFile.Adopted("adrs"));
        Assert.False(MechanismCheck.Declined(".schema/adrs.yaml", lockFile));
    }

    [Fact]
    public void A_lock_declaring_types_declines_the_schema_files_of_the_rest()
    {
        var lockFile = new MechanismLock { Types = ["adrs"] };

        Assert.True(lockFile.Adopted("adrs"));
        Assert.False(lockFile.Adopted("runbooks"));
        Assert.False(MechanismCheck.Declined(".schema/adrs.yaml", lockFile));
        Assert.True(MechanismCheck.Declined(".schema/runbooks.yaml", lockFile));
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
        => Assert.False(MechanismCheck.Declined(path, new MechanismLock { Types = ["adrs"] }));

    [Fact]
    public void Types_are_read_from_the_lock()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(dir, ".mechanism.lock"),
            "role: consumer\ntypes:\n  - adrs\n  - policies\n");

        Assert.Equal(["adrs", "policies"], MechanismLock.Load(dir).Types);
    }
}

