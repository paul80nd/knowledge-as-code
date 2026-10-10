using kac.core;

namespace kac.tests;

public class ShortcodeTests
{
    [Fact]
    public void A_corpus_declaring_no_shortcode_is_not_asked_anything()
        => Assert.Empty(Shortcode(null));

    [Theory]
    [InlineData("eng")]
    [InlineData("ex")]
    [InlineData("pay16")]
    [InlineData("acme2026")]
    public void A_lower_case_shortcode_of_letters_and_digits_is_silent(string shortcode)
        => Assert.Empty(Shortcode(shortcode));

    [Fact]
    public void A_shortcode_of_one_character_is_too_short()
        => Assert.Contains("is too short", Assert.Single(Shortcode("e")).Message);

    [Fact]
    public void A_shortcode_of_nine_characters_is_too_long()
        => Assert.Contains("is too long", Assert.Single(Shortcode("acme20264")).Message);

    [Theory]
    [InlineData("Eng")]
    [InlineData("9ng")]
    [InlineData("-ng")]
    public void A_shortcode_opening_on_anything_but_a_lower_case_letter_is_refused(string shortcode)
        => Assert.Contains("does not open on a lower-case letter", Assert.Single(Shortcode(shortcode)).Message);

    /// <summary>
    /// <c>my-corp</c> is the case to hold. A hyphen inside a shortcode would make a scoped reference look like an id on
    /// the left of the colon, which is how <c>Document.cs</c> collects a citation.
    /// </summary>
    [Theory]
    [InlineData("my-corp")]
    [InlineData("eng.uk")]
    [InlineData("engUK")]
    public void A_shortcode_carrying_anything_but_a_lower_case_letter_or_a_digit_is_refused(string shortcode)
        => Assert.Contains("carries something other than", Assert.Single(Shortcode(shortcode)).Message);

    [Fact]
    public void A_shortcode_a_type_uses_as_its_id_prefix_names_the_type()
        => Assert.Contains("is the id prefix of 'standards'", Assert.Single(Shortcode("std")).Message);

    [Fact]
    public void A_shortcode_no_type_uses_is_left_alone()
        => Assert.Empty(Shortcode("adr2"));

    [Fact]
    public void A_miscased_type_prefix_is_reported_as_both()
    {
        var findings = Shortcode("STD");

        Assert.Equal(2, findings.Count);
        Assert.Contains(findings, f => f.Message.Contains("does not open on a lower-case letter"));
        Assert.Contains(findings, f => f.Message.Contains("is the id prefix of 'standards'"));
    }

    private static List<Finding> Shortcode(string? shortcode)
    {
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
            {
                ["adrs"] = new() { Key = "adrs", TypeName = "adrs", Folder = "adrs", IdPrefix = "adr" },
                ["standards"] = new()
                {
                    Key = "standards", TypeName = "standards", Folder = "standards", IdPrefix = "std"
                }
            }
        };

        var tree = new Tree(new HashSet<string>(StringComparer.Ordinal), _ => "", _ => false);
        var descriptor = new CorpusDescriptor { Shortcode = shortcode };

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, descriptor), Required.Today)
                .Where(f => f.Check.Value == "shortcode")
        ];
    }
}
