// In-process unit tests for the manifest glob matcher — the kind of focused, table-driven coverage
// the golden/subprocess suite can't express. Exercises the kac.core Glob type directly.
public class GlobTests
{
    [Theory]
    [InlineData("adrs/0001-x.md", "adrs/*.md", true)]      // * matches within one segment
    [InlineData("adrs/sub/0001.md", "adrs/*.md", false)]   // * does not cross a '/'
    [InlineData("a/b/c.md", "**/*.md", true)]              // leading **/ matches at any depth
    [InlineData("c.md", "**/*.md", true)]                  // ...including the root
    [InlineData("knowledge-as-code/schema/x.yaml", "knowledge-as-code/**", true)] // ** spans segments
    [InlineData("axmd", "a.md", false)]                    // the '.' is a literal, not any-char
    public void IsMatch_matches_expected(string path, string pattern, bool expected)
        => Assert.Equal(expected, Glob.IsMatch(path, pattern));
}
