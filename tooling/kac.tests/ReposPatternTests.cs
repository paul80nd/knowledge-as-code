using kac.core;

namespace kac.tests;

/// <summary>
/// The pattern <c>.schema/services.yaml</c> declares on <c>repos</c>, read from the schema every corpus is judged by.
/// </summary>
[Trait(Kind.Of, Kind.Repository)]
public class ReposPatternTests
{
    [Theory]
    [InlineData("lending")]
    [InlineData("My Repo")]
    [InlineData("Proj X/My Repo")]
    [InlineData("paul80nd/knowledge-as-code")]
    [InlineData("Payments.Api_v2")]
    public void An_entry_named_as_its_host_names_it_matches(string entry)
        => Assert.Matches(Pattern, entry);

    [Theory]
    [InlineData("a/b/c")]
    [InlineData("/lending")]
    [InlineData("lending/")]
    [InlineData("estate\\lending")]
    [InlineData("lending#readme")]
    [InlineData("lending?path=/")]
    [InlineData("lend\ting")]
    [InlineData("lend\u007Fing")]
    [InlineData("lending\n")]
    [InlineData(" lending")]
    [InlineData("lending ")]
    [InlineData("Proj X /lending")]
    [InlineData("Proj X/ lending")]
    [InlineData("")]
    public void An_entry_a_host_cannot_name_does_not_match(string entry)
        => Assert.DoesNotMatch(Pattern, entry);

    private static readonly System.Text.RegularExpressions.Regex Pattern =
        Schema.Load(Repo.Root).ByFolder["services"].Fields["repos"].PatternRegex
        ?? throw new InvalidOperationException("services.yaml declares no pattern on 'repos'.");
}
