using kac.core;

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

    [Theory]
    [InlineData("adr-NNNN")]
    [InlineData("pol-XXXX")]
    [InlineData("/services/example.md")]
    public void The_dialects_the_mark_replaced_are_not_placeholders(string value)
        => Assert.False(Placeholder.In(value));

    /// <summary>
    /// A brace is a YAML indicator only at the head of a value, so where the mark falls decides whether the field is
    /// text at all. This is the whole reason a template quotes some placeholders and not others, and the reason
    /// <c>template-fields</c> reports the unquoted case with the fix in the message.
    /// </summary>
    [Fact]
    public void A_mark_that_opens_a_value_is_read_as_a_mapping_unless_it_is_quoted()
    {
        Assert.IsType<YamlDotNet.RepresentationModel.YamlMappingNode>(Value("review-by: {{date}}"));
        Assert.Equal("{{date}}", Scalar("review-by: \"{{date}}\""));
        Assert.Equal("svc-{{slug}}", Scalar("id: svc-{{slug}}"));
    }

    /// <summary>
    /// The same indicator inside a flow sequence. There a plain scalar may carry no brace anywhere, so the whole
    /// document fails to parse and the list has to be written as a block sequence.
    /// </summary>
    [Fact]
    public void A_mark_in_a_flow_sequence_is_a_parse_error_and_a_block_sequence_is_not()
    {
        Assert.Throws<YamlDotNet.Core.SemanticErrorException>(() => Value("related: [ adr-{{a}} ]"));

        var seq = Assert.IsType<YamlDotNet.RepresentationModel.YamlSequenceNode>(Value("related:\n  - adr-{{a}}"));
        Assert.Equal("adr-{{a}}", ((YamlDotNet.RepresentationModel.YamlScalarNode)seq.Children[0]).Value);
    }

    [Theory]
    [InlineData("---\nid: svc-{{slug}}\n---\n\n# A title\n", "{{slug}}")]
    [InlineData("---\nid: svc-a\ndepends-on:\n  - svc-{{b}}\n---\n\n# A title\n", "{{b}}")]
    [InlineData("---\nid: svc-a\n---\n\n# A title\n\n`Service: svc-{{slug}}` `LIVE`\n", "{{slug}}")]
    [InlineData("---\nid: svc-a\n---\n\n# {{Service name}}\n", "{{Service name}}")]
    [InlineData("---\nid: svc-a\n---\n\n# A title\n\nSee [the other]({{a}}.md).\n", "{{a}}")]
    public void A_placeholder_is_found_in_frontmatter_the_identity_line_the_prose_and_a_link(
        string text, string expected)
    {
        var found = Placeholder.Occurrences(Required.Parsed("services/a.md", text, new Schema())).ToList();
        Assert.Contains(expected, found.Select(f => f.Token));
    }

    [Theory]
    [InlineData("---\nid: svc-a\nowner: {{owner}}\n---\n\n# A title\n")]
    [InlineData("---\nid: svc-a\ngenerated: {{generated}}\n---\n\n# A title\n")]
    public void A_mark_opening_a_frontmatter_value_is_left_to_bare_key(string text)
        => Assert.Empty(Placeholder.Occurrences(Required.Parsed("services/a.md", text, new Schema())));

    [Theory]
    [InlineData("---\nid: svc-a\n---\n\n# A title\n\n```yaml\nrun: echo ${{ vars.id }}\n```\n")]
    [InlineData("---\nid: svc-a\n---\n\n# A title\n\nWrite `${{ vars.id }}` to read it.\n")]
    [InlineData("---\nid: svc-a\n---\n\n# A title\n\n    indented: ${{ vars.id }}\n")]
    public void Code_is_not_scanned(string text)
        => Assert.Empty(Placeholder.Occurrences(Required.Parsed("services/a.md", text, new Schema())));

    [Fact]
    public void Every_occurrence_is_returned_so_the_check_can_count_what_it_does_not_name()
    {
        const string text = "---\nid: svc-{{slug}}\n---\n\n# {{Name}}\n\nIt calls [{{a}}]({{a}}.md).\n";
        Assert.Equal(4, Placeholder.Occurrences(Required.Parsed("services/a.md", text, new Schema())).Count());
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
