using System.Text.RegularExpressions;
using kac.core;
using YamlDotNet.RepresentationModel;

// std-A11Y asks for a new assessment wherever a pin moves its major or minor, and below 0.1.0 wherever it moves at
// all. `ctl-assessed-pin-tests` says what this proves and what it leaves to a reviewer.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class AssessedPinTests
{
    private const string Corpus = "examples/dog-fooding";

    private const string Assessment = "Accessibility";

    private static readonly Schema Schema = Schema.Load(Repo.Root);

    internal sealed record Pin(string Version, string File);

    [GeneratedRegex("""<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"\s*/>""")]
    private static partial Regex NugetPin();

    [GeneratedRegex(@"^([A-Za-z0-9._-]+)==(\S+)", RegexOptions.Multiline)]
    private static partial Regex PypiPin();

    [Fact]
    public void Every_assessed_component_is_pinned_inside_its_range_and_its_assessment_names_the_pin()
    {
        var pins = Pins();
        var tools = Path.Combine(Repo.Root, Corpus, "tools");
        var assessed = 0;
        var faults = new List<string>();

        foreach (var path in Directory.EnumerateFiles(tools, "*.md", SearchOption.AllDirectories).Order())
        {
            if (Path.GetFileName(path).StartsWith('_')) continue;

            var rel = Path.GetRelativePath(Path.Combine(Repo.Root, Corpus), path).Replace('\\', '/');
            if (Faults(rel, Files.ReadLf(path), pins) is not { } found) continue;

            assessed++;
            faults.AddRange(found);
        }

        // Zero is this guard gone quiet, and green is what it would report from then on.
        Assert.True(assessed > 0,
            $"no tool record in {Corpus} has an '{Assessment}' section and 'packages', so this guard is checking "
            + "nothing. Either the section was renamed, or the records moved.");

        Assert.True(faults.Count == 0,
            "these assessments do not match what this repository pins:\n  " + string.Join("\n  ", faults)
            + "\nRepeat the assessment at the pinned version. std-A11Y says what an assessment states.");
    }

    /// <summary>
    /// Null where the record has no assessment, approves no package, or is deprecated or rejected, so it is not one
    /// this guard reads.
    /// </summary>
    internal static IReadOnlyList<string>? Faults(string rel, string text,
        IReadOnlyDictionary<string, IReadOnlyList<Pin>> pins)
    {
        if (Doc.Parse(rel, text, Schema) is not { Front: { } front } doc) return null;
        if (Yaml.Str(Yaml.Get(front, "status")) is "deprecated" or "rejected") return null;
        if (doc.Sections.FirstOrDefault(s => s.Title == Assessment) is not { } section) return null;
        if (Yaml.Get(front, "packages") is not YamlSequenceNode packages) return null;

        var id = Yaml.Str(Yaml.Get(front, "id")) ?? rel;
        var body = text[section.BodyStart..section.BodyEnd];
        var faults = new List<string>();

        foreach (var package in packages)
        {
            var purl = Yaml.Str(Yaml.Get(package, "purl")) ?? "";
            var range = Yaml.Str(Yaml.Get(package, "versions")) ?? "";

            if (!pins.TryGetValue(purl, out var found) || found.Count == 0)
            {
                faults.Add($"{id} approves {purl}, and nothing in this repository pins it.");
                continue;
            }

            foreach (var pin in found)
            {
                // Below 0.1.0 the whole version is the line.
                var exact = pin.Version.StartsWith("0.0.", StringComparison.Ordinal);
                var line = exact ? pin.Version : MajorMinor(pin.Version);

                if (exact)
                {
                    if (range != pin.Version)
                        faults.Add($"{id} approves {purl} {range}, and {pin.File} pins {pin.Version}. Below 0.1.0 "
                                   + "the range is the pinned version.");
                }
                else if (!range.EndsWith(".x", StringComparison.Ordinal))
                    faults.Add($"{id} approves {purl} with no range ending '.x', and {pin.File} pins {pin.Version}.");
                else if (!pin.Version.StartsWith(range[..^1], StringComparison.Ordinal))
                    faults.Add($"{id} approves {purl} {range}, and {pin.File} pins {pin.Version}.");

                // A version, so `0.57.x`, `10.57.2` and `0.0.720` fail to name `0.57.2` or `0.0.72`.
                var named = exact ? $@"(?<![\d.]){Regex.Escape(line)}(?!\d)" : $@"(?<![\d.]){Regex.Escape(line)}\.\d";
                if (!Regex.IsMatch(body, named))
                    faults.Add($"{id} pins {purl} at {pin.Version}, and its {Assessment} section names no "
                               + $"{line} version.");
            }
        }

        return faults;
    }

    private static string MajorMinor(string version) => string.Join('.', version.Split('.').Take(2));

    /// <summary>Every NuGet pin under <c>tooling/</c> and pip pin for the site, keyed by package URL.</summary>
    private static Dictionary<string, IReadOnlyList<Pin>> Pins()
    {
        var pins = new Dictionary<string, List<Pin>>(StringComparer.Ordinal);

        foreach (var project in Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling"), "*.csproj",
                     SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(Repo.Root, project).Replace('\\', '/');
            foreach (Match m in NugetPin().Matches(Files.ReadLf(project)))
                Add($"pkg:nuget/{m.Groups[1].Value}", m.Groups[2].Value, rel);
        }

        const string requirements = "docs/requirements.txt";
        foreach (Match m in PypiPin().Matches(Files.ReadLf(Path.Combine(Repo.Root, requirements))))
            Add($"pkg:pypi/{m.Groups[1].Value.ToLowerInvariant()}", m.Groups[2].Value, requirements);

        return pins.ToDictionary(p => p.Key, IReadOnlyList<Pin> (p) => p.Value, StringComparer.Ordinal);

        void Add(string purl, string version, string file)
        {
            if (!pins.TryGetValue(purl, out var list)) pins[purl] = list = [];
            list.Add(new Pin(version, file));
        }
    }

    private const string InStep = """
                                  ---
                                  id: tol-widget
                                  packages:
                                    - { purl: pkg:nuget/Widget, versions: 0.57.x }
                                  ---

                                  # Widget

                                  ## Accessibility

                                  Assessed at Widget 0.57.2, by reading every call that colours.
                                  """;

    private static readonly Dictionary<string, IReadOnlyList<Pin>> WidgetAt0572 = new(StringComparer.Ordinal)
    {
        ["pkg:nuget/Widget"] = [new Pin("0.57.2", "tooling/kac/kac.csproj")]
    };

    [Fact]
    public void A_record_in_step_with_its_pin_reports_nothing() =>
        Assert.Empty(Faults("tools/widget.md", InStep, WidgetAt0572) ?? ["not read"]);

    [Fact]
    public void A_record_with_no_assessment_is_not_read() =>
        Assert.Null(Faults("tools/widget.md", InStep.Replace("## Accessibility", "## Related"), WidgetAt0572));

    [Theory]
    [InlineData("deprecated")]
    [InlineData("rejected")]
    public void A_record_out_of_use_is_not_read(string status) =>
        Assert.Null(Faults("tools/widget.md", InStep.Replace("id: tol-widget", $"id: tol-widget\nstatus: {status}"),
            new Dictionary<string, IReadOnlyList<Pin>>()));

    [Fact]
    public void A_pin_outside_the_range_is_reported()
    {
        var faults = Faults("tools/widget.md", InStep.Replace("0.57.x", "0.55.x"), WidgetAt0572);

        Assert.Contains(faults ?? [], f => f.Contains("approves pkg:nuget/Widget 0.55.x", StringComparison.Ordinal));
    }

    [Fact]
    public void A_range_naming_one_version_is_reported()
    {
        var faults = Faults("tools/widget.md", InStep.Replace("0.57.x", "0.57.2"), WidgetAt0572);

        Assert.Contains(faults ?? [], f => f.Contains("no range ending '.x'", StringComparison.Ordinal));
    }

    [Fact]
    public void An_assessment_naming_another_version_is_reported()
    {
        var faults = Faults("tools/widget.md", InStep.Replace("Widget 0.57.2", "Widget 0.55.0"), WidgetAt0572);

        Assert.Contains(faults ?? [], f => f.Contains("names no 0.57 version", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Widget 0.57.x")]
    [InlineData("Widget 10.57.2")]
    public void An_assessment_must_name_the_version_itself(string named)
    {
        var faults = Faults("tools/widget.md", InStep.Replace("Widget 0.57.2", named), WidgetAt0572);

        Assert.Contains(faults ?? [], f => f.Contains("names no 0.57 version", StringComparison.Ordinal));
    }

    private static readonly Dictionary<string, IReadOnlyList<Pin>> WidgetAt0072 = new(StringComparer.Ordinal)
    {
        ["pkg:nuget/Widget"] = [new Pin("0.0.72", "tooling/kac/kac.csproj")]
    };

    private static readonly string ExactInStep =
        InStep.Replace("versions: 0.57.x", "versions: 0.0.72").Replace("Widget 0.57.2", "Widget 0.0.72");

    [Fact]
    public void A_record_approving_a_pin_below_0_1_0_exactly_reports_nothing() =>
        Assert.Empty(Faults("tools/widget.md", ExactInStep, WidgetAt0072) ?? ["not read"]);

    [Fact]
    public void A_range_over_a_pin_below_0_1_0_is_reported()
    {
        var faults = Faults("tools/widget.md", ExactInStep.Replace("versions: 0.0.72", "versions: 0.0.x"),
            WidgetAt0072);

        Assert.Contains(faults ?? [], f => f.Contains("approves pkg:nuget/Widget 0.0.x", StringComparison.Ordinal));
    }

    [Fact]
    public void An_exact_range_naming_another_version_below_0_1_0_is_reported()
    {
        var faults = Faults("tools/widget.md", ExactInStep.Replace("versions: 0.0.72", "versions: 0.0.71"),
            WidgetAt0072);

        Assert.Contains(faults ?? [], f => f.Contains("approves pkg:nuget/Widget 0.0.71", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Widget 0.0.71")]
    [InlineData("Widget 0.0.720")]
    [InlineData("Widget 10.0.72")]
    public void An_assessment_below_0_1_0_must_name_the_pinned_version(string named)
    {
        var faults = Faults("tools/widget.md", ExactInStep.Replace("Widget 0.0.72", named), WidgetAt0072);

        Assert.Contains(faults ?? [], f => f.Contains("names no 0.0.72 version", StringComparison.Ordinal));
    }

    [Fact]
    public void A_package_nothing_pins_is_reported()
    {
        var faults = Faults("tools/widget.md", InStep, new Dictionary<string, IReadOnlyList<Pin>>());

        Assert.Contains(faults ?? [], f => f.Contains("nothing in this repository pins", StringComparison.Ordinal));
    }
}
