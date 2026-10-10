using System.Text;
using kac.core;

namespace kac.tests;

public class FreshnessTests
{
    private const string Shelf = "../engineering/.dist/package";

    [Fact]
    public void A_lock_on_the_newest_version_the_range_admits_reports_nothing()
        => Assert.Equal(Standing.Current, Read(resolved: "0.1.1", holds: ["0.1.0", "0.1.1"]).How);

    [Fact]
    public void A_newer_version_inside_the_range_is_behind()
    {
        var standing = Read(resolved: "0.1.1", holds: ["0.1.1", "0.1.2"]);

        Assert.Equal(Standing.Behind, standing.How);
        Assert.Equal("0.1.2", standing.Available);
    }

    [Fact]
    public void The_version_named_is_the_newest_the_range_admits()
        => Assert.Equal("0.1.4", Read(resolved: "0.1.1", holds: ["0.1.2", "0.1.4", "0.2.0"]).Available);

    [Fact]
    public void A_newer_version_outside_the_range_is_capped()
    {
        var standing = Read(resolved: "0.1.1", holds: ["0.1.1", "0.2.0"]);

        Assert.Equal(Standing.Capped, standing.How);
        Assert.Equal("0.2.0", standing.Available);
    }

    [Fact]
    public void A_prerelease_above_the_lock_is_neither()
        => Assert.Equal(Standing.Current, Read(resolved: "0.1.1", holds: ["0.1.1", "0.2.0-rc.1"]).How);

    [Fact]
    public void An_exact_range_holding_an_older_version_is_capped()
        => Assert.Equal(Standing.Capped, Read(range: "0.1.1", resolved: "0.1.1", holds: ["0.1.2"]).How);

    // A feed answers 404 for an unpublished package and for a private one refusing an anonymous read, so the empty
    // listing must not read as current. The words are `Registry.Absent`'s, which differ for a folder and a registry.
    [Fact]
    public void A_source_listing_no_versions_at_all_is_unreachable()
    {
        var standing = Read(resolved: "0.1.1", holds: []);

        Assert.Equal(Standing.Unreachable, standing.How);
        Assert.Equal(Registry.Absent(Shelf), standing.Problem);
    }

    [Fact]
    public void A_lock_its_own_range_no_longer_admits_is_passed_over()
        => Assert.Empty(Freshness.Read(
            [Declared(range: "0.1.1", resolved: "0.2.0")], Feed(["0.1.1", "0.2.0", "0.3.0"])));

    [Fact]
    public void A_source_that_could_not_be_asked_is_unreachable()
    {
        var standing = Read(resolved: "0.1.1", holds: null);

        Assert.Equal(Standing.Unreachable, standing.How);
        Assert.Null(standing.Available);
        Assert.NotNull(standing.Problem);
        Assert.Contains(Shelf, standing.Problem, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "eng", "^0.1.0", "0.1.1", Shelf)]
    [InlineData("example-engineering", null, "^0.1.0", "0.1.1", Shelf)]
    [InlineData("example-engineering", "eng", null, "0.1.1", Shelf)]
    [InlineData("example-engineering", "eng", "^0.1.0", "0.1.1", null)]
    public void An_entry_a_restore_would_refuse_is_passed_over(
        string? corpus, string? shortcode, string? range, string? resolved, string? source)
        => Assert.Empty(Freshness.Read(
            [new Consumed(corpus, shortcode, range, resolved, source)], Feed(["0.1.2"])));

    [Fact]
    public void An_entry_with_no_resolved_version_is_passed_over()
        => Assert.Empty(Freshness.Read([Declared(resolved: null)], Feed(["0.1.2"])));

    [Fact]
    public void A_corpus_consuming_nothing_asks_nothing()
        => Assert.Empty(Freshness.Read([], Feed(["0.1.2"])));

    [Fact]
    public void Each_import_stands_on_its_own()
    {
        var standings = Freshness.Read(
            [Declared(), Declared(corpus: "example-security", shortcode: "sec", resolved: "0.1.2")],
            Feed(["0.1.1", "0.1.2"]));

        Assert.Equal(Standing.Behind, standings[0].How);
        Assert.Equal(Standing.Current, standings[1].How);
    }

    private static ImportStanding Read(
        string range = "^0.1.0", string? resolved = "0.1.1", string[]? holds = null) =>
        Assert.Single(Freshness.Read([Declared(range, resolved)], Feed(holds)));

    private static Consumed Declared(
        string range = "^0.1.0",
        string? resolved = "0.1.1",
        string corpus = "example-engineering",
        string shortcode = "eng") =>
        new(corpus, shortcode, range, resolved, Shelf);

    // A shelf holding one file per version, named as `kac pack` names them, or no folder at all where
    // `holds` is null.
    private static Registry Feed(string[]? holds) =>
        new(
            _ => new Fetched(null, 0, "no registry stands behind these tests."),
            new FolderFeed(
                _ => holds?.SelectMany(v =>
                        new[] { $"example-engineering.{v}.nupkg", $"example-security.{v}.nupkg" })
                    .ToList(),
                _ => Encoding.UTF8.GetBytes("")));
}
