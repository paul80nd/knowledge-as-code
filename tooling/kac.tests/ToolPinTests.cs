using kac.core;
using YamlDotNet.RepresentationModel;

// std-CONFIG asks a tool record's `versions` to contain the version this repository pins. `ctl-tool-pin-tests` says
// what this proves and what it leaves to a reviewer.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class ToolPinTests
{
    private const string Corpus = "examples/dog-fooding";

    private static readonly Schema Schema = Schema.Load(Repo.Root);

    [Fact]
    public void Every_tool_record_in_use_contains_the_version_this_repository_pins()
    {
        var pins = RepositoryPins.Read(Repo.Root);
        var tools = Path.Combine(Repo.Root, Corpus, "tools");
        var read = 0;
        var faults = new List<string>();

        foreach (var path in Directory.EnumerateFiles(tools, "*.md", SearchOption.AllDirectories).Order())
        {
            if (Path.GetFileName(path).StartsWith('_')) continue;

            var rel = Path.GetRelativePath(Path.Combine(Repo.Root, Corpus), path).Replace('\\', '/');
            if (Faults(rel, Files.ReadLf(path), pins) is not { } found) continue;

            read++;
            faults.AddRange(found);
        }

        // Zero is this guard gone quiet, and green is what it would report from then on.
        Assert.True(read > 0,
            $"no tool record in {Corpus} is in use and has 'packages', so this guard is checking nothing. Either the "
            + "field was renamed, or the records moved.");

        Assert.True(faults.Count == 0,
            "these tool records do not contain what this repository pins:\n  " + string.Join("\n  ", faults)
            + "\nMove `versions` to contain the pin. std-CONFIG says what a tool record states.");
    }

    /// <summary>
    /// Null where the record approves no package, or is deprecated or rejected, so it is not one this guard reads.
    /// </summary>
    internal static IReadOnlyList<string>? Faults(string rel, string text,
        IReadOnlyDictionary<string, IReadOnlyList<Pin>> pins)
    {
        if (Doc.Parse(rel, text, Schema) is not { Front: { } front }) return null;
        if (Yaml.Str(Yaml.Get(front, "status")) is "deprecated" or "rejected") return null;
        if (Yaml.Get(front, "packages") is not YamlSequenceNode packages) return null;

        var id = Yaml.Str(Yaml.Get(front, "id")) ?? rel;
        var faults = new List<string>();

        foreach (var package in packages)
        {
            var purl = Yaml.Str(Yaml.Get(package, "purl")) ?? "";
            var range = Yaml.Str(Yaml.Get(package, "versions"));
            var found = pins.TryGetValue(purl, out var p) ? p : [];

            if (range is null)
            {
                faults.AddRange(found.Select(pin =>
                    $"{id} gives no `versions` for {purl}, and {pin.File} pins {pin.Version}."));
                continue;
            }

            if (found.Count == 0)
            {
                faults.Add($"{id} gives {purl} {range}, and nothing in this repository pins it.");
                continue;
            }

            faults.AddRange(found.Where(pin => !Contains(range, pin.Version))
                .Select(pin => $"{id} gives {purl} {range}, and {pin.File} pins {pin.Version}."));
        }

        return faults;
    }

    /// <summary>A range ending <c>.x</c> contains every version under its prefix. Any other is one version.</summary>
    internal static bool Contains(string range, string version) =>
        range.EndsWith(".x", StringComparison.Ordinal)
            ? version.StartsWith(range[..^1], StringComparison.Ordinal)
            : range == version;

    private const string InStep = """
                                  ---
                                  id: tol-widget
                                  packages:
                                    - { purl: pkg:nuget/Widget, versions: 2.x }
                                  ---

                                  # Widget
                                  """;

    private static readonly Dictionary<string, IReadOnlyList<Pin>> WidgetAt241 = new(StringComparer.Ordinal)
    {
        ["pkg:nuget/Widget"] = [new Pin("2.4.1", "tooling/kac/kac.csproj")]
    };

    [Fact]
    public void A_record_whose_range_contains_its_pin_reports_nothing() =>
        Assert.Empty(Faults("tools/widget.md", InStep, WidgetAt241) ?? ["not read"]);

    [Fact]
    public void An_exact_version_equal_to_its_pin_reports_nothing() =>
        Assert.Empty(Faults("tools/widget.md", InStep.Replace("2.x", "2.4.1"), WidgetAt241) ?? ["not read"]);

    [Theory]
    [InlineData("deprecated")]
    [InlineData("rejected")]
    public void A_record_out_of_use_is_not_read(string status) =>
        Assert.Null(Faults("tools/widget.md", InStep.Replace("id: tol-widget", $"id: tol-widget\nstatus: {status}"),
            new Dictionary<string, IReadOnlyList<Pin>>()));

    [Theory]
    [InlineData("2.4.0")]
    [InlineData("3.x")]
    [InlineData("2.3.x")]
    public void A_range_not_containing_the_pin_is_reported(string range)
    {
        var faults = Faults("tools/widget.md", InStep.Replace("2.x", range), WidgetAt241);

        Assert.Contains(faults ?? [], f => f.Contains($"gives pkg:nuget/Widget {range}, and tooling/kac/kac.csproj "
                                                      + "pins 2.4.1", StringComparison.Ordinal));
    }

    [Fact]
    public void A_pin_under_a_longer_major_is_outside_the_range()
    {
        var at20 = new Dictionary<string, IReadOnlyList<Pin>>(StringComparer.Ordinal)
        {
            ["pkg:nuget/Widget"] = [new Pin("20.0.0", "tooling/kac/kac.csproj")]
        };

        Assert.Contains(Faults("tools/widget.md", InStep, at20) ?? [],
            f => f.Contains("pins 20.0.0", StringComparison.Ordinal));
    }

    [Fact]
    public void A_package_pinned_twice_is_reported_for_the_pin_outside_its_range()
    {
        var twice = new Dictionary<string, IReadOnlyList<Pin>>(StringComparer.Ordinal)
        {
            ["pkg:nuget/Widget"] = [new Pin("2.4.1", "global.json"), new Pin("2.4.0", "template/azure-pipelines.yml")]
        };

        var faults = Faults("tools/widget.md", InStep.Replace("2.x", "2.4.1"), twice) ?? [];

        Assert.Equal(["tol-widget gives pkg:nuget/Widget 2.4.1, and template/azure-pipelines.yml pins 2.4.0."], faults);
    }

    [Fact]
    public void A_pinned_package_with_no_range_is_reported()
    {
        var faults = Faults("tools/widget.md", InStep.Replace(", versions: 2.x", ""), WidgetAt241);

        Assert.Contains(faults ?? [], f => f.Contains("gives no `versions`", StringComparison.Ordinal));
    }

    [Fact]
    public void A_package_with_no_range_and_no_pin_reports_nothing() =>
        Assert.Empty(Faults("tools/widget.md", InStep.Replace(", versions: 2.x", ""),
            new Dictionary<string, IReadOnlyList<Pin>>()) ?? ["not read"]);

    [Fact]
    public void A_range_nothing_pins_is_reported()
    {
        var faults = Faults("tools/widget.md", InStep, new Dictionary<string, IReadOnlyList<Pin>>());

        Assert.Contains(faults ?? [], f => f.Contains("nothing in this repository pins", StringComparison.Ordinal));
    }
}
