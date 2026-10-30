using kac.core;

namespace kac.tests;

public class CountedTests
{
    [Theory]
    [InlineData(0, "0 files")]
    [InlineData(1, "1 file")]
    [InlineData(2, "2 files")]
    public void A_count_of_one_takes_the_singular_and_every_other_count_the_plural(int n, string expected)
    {
        Assert.Equal(expected, Counted.Of(n, "file"));
    }

    [Fact]
    public void A_noun_not_taking_s_is_given_its_plural()
    {
        Assert.Equal("1 entry", Counted.Of(1, "entry", "entries"));
        Assert.Equal("3 entries", Counted.Of(3, "entry", "entries"));
    }
}
