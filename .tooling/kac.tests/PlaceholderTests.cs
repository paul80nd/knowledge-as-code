using kac.core;

// The placeholder mark, and the two positions where YAML reads it as something other than text. Tested
// here because both are properties of the mark rather than of any one template: a corpus writing its
// own templates meets them the first time it writes a date field, and the answer must not depend on
// which type it happened to be writing.

namespace kac.tests;

public class PlaceholderTests
{
    [Theory]
    [InlineData("svc-{{slug}}")]
    [InlineData("{{a}}.md")]
    [InlineData("/adrs/{{a}}.md#{{anchor}}")]
    public void The_mark_is_recognised_wherever_it_falls_in_a_value(string value)
        => Assert.True(Placeholder.In(value));

    [Theory]
    [InlineData("svc-search")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("{ not a mark }")]
    public void Anything_without_the_mark_is_a_value(string? value)
        => Assert.False(Placeholder.In(value));

    // The stand-in words the templates used before the mark was settled. They are ordinary text now, so
    // a template still writing one gets the finding it has coming rather than a silent exemption — and
    // `example` stays available as a slug a real document may want.
    [Theory]
    [InlineData("adr-NNNN")]
    [InlineData("pol-XXXX")]
    [InlineData("/services/example.md")]
    public void The_dialects_the_mark_replaced_are_not_placeholders(string value)
        => Assert.False(Placeholder.In(value));

    // A brace is a YAML indicator only at the head of a value, so where the mark falls decides whether
    // the field is text at all. This is the whole reason a template quotes some placeholders and not
    // others, and the reason `template-fields` reports the unquoted case with the fix in the message.
    [Fact]
    public void A_mark_that_opens_a_value_is_read_as_a_mapping_unless_it_is_quoted()
    {
        Assert.IsType<YamlDotNet.RepresentationModel.YamlMappingNode>(Value("review-by: {{date}}"));
        Assert.Equal("{{date}}", Scalar("review-by: \"{{date}}\""));
        Assert.Equal("svc-{{slug}}", Scalar("id: svc-{{slug}}"));
    }

    // The same indicator, inside a flow sequence rather than at the head of a value: there a plain
    // scalar may not carry a brace anywhere, so the whole document fails to parse and the list has to
    // be written as a block sequence.
    [Fact]
    public void A_mark_in_a_flow_sequence_is_a_parse_error_and_a_block_sequence_is_not()
    {
        Assert.Throws<YamlDotNet.Core.SemanticErrorException>(() => Value("related: [ adr-{{a}} ]"));

        var seq = Assert.IsType<YamlDotNet.RepresentationModel.YamlSequenceNode>(Value("related:\n  - adr-{{a}}"));
        Assert.Equal("adr-{{a}}", ((YamlDotNet.RepresentationModel.YamlScalarNode)seq.Children[0]).Value);
    }

    private static YamlDotNet.RepresentationModel.YamlNode Value(string yaml)
    {
        var stream = new YamlDotNet.RepresentationModel.YamlStream();
        stream.Load(new StringReader(yaml));
        var map = (YamlDotNet.RepresentationModel.YamlMappingNode)stream.Documents[0].RootNode;
        return map.Children.First().Value;
    }

    private static string? Scalar(string yaml)
        => (Value(yaml) as YamlDotNet.RepresentationModel.YamlScalarNode)?.Value;
}
