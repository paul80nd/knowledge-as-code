// In-process unit tests for the manifest glob matcher: the kind of focused, table-driven coverage
// the golden and subprocess suites cannot express. Exercises the kac.core Glob type directly.

using kac.core;

namespace kac.tests;

public class GlobTests
{
    [Theory]
    [InlineData("adrs/0001-x.md", "adrs/*.md", true)]              // * matches within one segment
    [InlineData("adrs/sub/0001.md", "adrs/*.md", false)]           // * does not cross a '/'
    [InlineData("a/b/c.md", "**/*.md", true)]                      // leading **/ matches at any depth
    [InlineData("c.md", "**/*.md", true)]                          // ...including the root
    [InlineData("tooling/kac.core/Schema.cs", "tooling/**", true)] // ** spans segments
    [InlineData("axmd", "a.md", false)]                            // the '.' is a literal, not any-char
    public void IsMatch_matches_expected(string path, string pattern, bool expected)
        => Assert.Equal(expected, Glob.IsMatch(path, pattern));

    // A corrupted cache surfaces in whichever spec scenario happens to be running, never in a test that
    // names the cache, so the concurrent ask is put here deliberately.
    [Fact]
    public void The_cache_survives_being_asked_from_several_threads()
    {
        var patterns = Enumerable.Range(0, 200).Select(i => $"folder{i}/**/*.md").ToList();

        Parallel.ForEach(patterns, pattern => Assert.True(Glob.IsMatch(pattern.Replace("**/*", "a/b"), pattern)));

        Assert.All(patterns, pattern => Assert.True(Glob.IsMatch(pattern.Replace("**/*", "a/b"), pattern)));
    }
}
