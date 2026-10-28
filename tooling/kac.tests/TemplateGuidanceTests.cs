using kac.core;

namespace kac.tests;

/// <summary>
/// A record written as its <c>_template.md</c> asks has to pass <c>validate</c>, so the template's frontmatter guidance
/// states each limit a first record otherwise meets only as a finding.
/// </summary>
[Trait(Kind.Of, Kind.Repository)]
public class TemplateGuidanceTests
{
    private static readonly Schema Declared = Schema.Load(Repo.Root);

    public static TheoryData<string> Types()
    {
        var data = new TheoryData<string>();
        foreach (var folder in Declared.ByFolder.Keys.Order(StringComparer.Ordinal)) data.Add(folder);
        return data;
    }

    [Theory]
    [MemberData(nameof(Types))]
    public void A_template_states_its_type_s_slug_limit(string folder)
    {
        var max = Declared.ByFolder[folder].SlugMax;

        Assert.True(Paragraphs(folder).Any(p => p.Contains("slug", StringComparison.Ordinal)
                                                && p.Contains($"{max} characters", StringComparison.Ordinal)),
            $"template/{folder}/_template.md does not say the slug is {max} characters at most. "
            + "state it in the Frontmatter guidance, where the filename is chosen.");
    }

    [Theory]
    [MemberData(nameof(Types))]
    public void A_template_whose_type_has_a_list_field_states_the_alphabetical_order(string folder)
    {
        Assert.SkipUnless(Declared.ByFolder[folder].DeclaredFields.Any(f => f.Type == "list"),
            $"type '{folder}' declares no list field.");

        Assert.True(Paragraphs(folder).Any(p => p.Contains("alphabetical order", StringComparison.Ordinal)),
            $"template/{folder}/_template.md does not say a list field's entries go in alphabetical order. "
            + "state it in the Frontmatter guidance.");
    }

    /// <summary>
    /// The paragraphs of the frontmatter guidance, from the <c>**Frontmatter**</c> heading to the end of the guidance
    /// block. A line break inside a paragraph is read as a space, so a phrase wrapped across lines still matches.
    /// </summary>
    private static IEnumerable<string> Paragraphs(string folder)
    {
        var text = Files.ReadLf(Path.Combine(Repo.Root, "template", folder, "_template.md"));
        var start = text.IndexOf("**Frontmatter**", StringComparison.Ordinal);
        Assert.True(start >= 0, $"template/{folder}/_template.md has no **Frontmatter** guidance.");

        var end = text.IndexOf("DELETE TO HERE", start, StringComparison.Ordinal);
        var guidance = end < 0 ? text[start..] : text[start..end];

        return guidance.Split("\n\n").Select(p => string.Join(' ', p.Split('\n').Select(l => l.Trim())));
    }
}
