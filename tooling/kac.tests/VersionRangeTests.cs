using kac.core;

// The two forms a `version:` may take, and what each of them admits. Unit tests alone: a range decides
// nothing about a file, so there is nothing here a fixture could hold.

namespace kac.tests;

public class VersionRangeTests
{
    [Theory]
    [InlineData("1.2.0")]
    [InlineData("^1.2.0")]
    [InlineData("0.0.1")]
    [InlineData("1.2.0-rc.1")]
    public void Both_forms_are_read(string range) => Assert.True(VersionRange.Legible(range));

    // Refused where it was written, rather than resolving to nothing and reading as a corpus that has
    // published no version yet.
    [Theory]
    [InlineData("1.2")]
    [InlineData("v1.2.0")]
    [InlineData(">=1.2.0")]
    [InlineData("~1.2.0")]
    [InlineData("latest")]
    [InlineData("*")]
    [InlineData("1.2.0 || 2.0.0")]
    public void Anything_else_is_not_a_range(string range) => Assert.False(VersionRange.Legible(range));

    [Fact]
    public void An_exact_range_admits_that_version_and_no_other()
    {
        Assert.True(VersionRange.Admits("1.2.0", "1.2.0"));
        Assert.False(VersionRange.Admits("1.2.0", "1.2.1"));
        Assert.False(VersionRange.Admits("1.2.0", "1.1.9"));
    }

    // A major above zero promises that nothing below it changed meaning, so the caret runs to the next
    // major.
    [Theory]
    [InlineData("1.2.0", true)]
    [InlineData("1.2.7", true)]
    [InlineData("1.9.0", true)]
    [InlineData("1.1.9", false)]
    [InlineData("2.0.0", false)]
    public void A_caret_above_one_runs_to_the_next_major(string version, bool admitted)
        => Assert.Equal(admitted, VersionRange.Admits("^1.2.0", version));

    // Below one there is no such promise, so the minor carries it instead. A corpus on 0.x that changed
    // a meaning moved its minor, and a consumer asking for `^0.1.0` must not be given 0.2.0.
    [Theory]
    [InlineData("0.1.0", true)]
    [InlineData("0.1.4", true)]
    [InlineData("0.2.0", false)]
    [InlineData("0.0.9", false)]
    public void A_caret_below_one_runs_to_the_next_minor(string version, bool admitted)
        => Assert.Equal(admitted, VersionRange.Admits("^0.1.0", version));

    // A prerelease is published to be asked for by name. A range meaning "the newest safe version" that
    // took one would put an unfinished vocabulary behind a consumer's citations.
    [Fact]
    public void A_caret_never_takes_a_prerelease()
        => Assert.False(VersionRange.Admits("^1.0.0", "1.1.0-rc.1"));

    [Fact]
    public void Naming_a_prerelease_exactly_is_how_a_corpus_opts_in()
        => Assert.True(VersionRange.Admits("1.1.0-rc.1", "1.1.0-rc.1"));

    // Ordered as versions rather than as strings, which is what `10` above `9` is here to catch.
    [Fact]
    public void The_highest_admitted_version_is_the_one_taken()
        => Assert.Equal("1.10.0", VersionRange.Best("^1.0.0", ["1.2.0", "1.10.0", "1.9.3", "2.0.0"]));

    [Fact]
    public void A_range_nothing_satisfies_resolves_to_nothing()
        => Assert.Null(VersionRange.Best("^2.0.0", ["1.2.0", "3.0.0"]));

    // A registry may list anything, and a version this tool cannot order is a version it cannot choose
    // between. It is passed over rather than allowed to throw from inside a comparison.
    [Fact]
    public void A_version_the_registry_lists_that_is_not_one_is_passed_over()
        => Assert.Equal("1.2.0", VersionRange.Best("^1.0.0", ["1.2.0", "nightly", "1.2.0.1", ""]));
}
