// Unit tests for Manifest.Resolve — the first-rule-wins glob layering. Manifest.Load,
// MechanismLock.Load and the MechanismCheck engine are covered by the golden 'mechanism' scenario.
public class ManifestTests
{
    private static Manifest Sample() => new()
    {
        Rules =
        [
            new ManifestRule(["knowledge-as-code/**"], "synced"),
            new ManifestRule(["**/*.md"], "forked"),
            new ManifestRule(["**"], "local"), // catch-all
        ],
    };

    [Theory]
    [InlineData("knowledge-as-code/schema/adrs.yaml", "synced")] // first rule wins
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
}
