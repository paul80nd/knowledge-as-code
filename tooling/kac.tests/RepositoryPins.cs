using System.Text.Json;
using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

internal sealed record Pin(string Version, string File);

/// <summary>Every version a repository pins, keyed by package URL, for tests comparing a tool record with it.</summary>
internal static partial class RepositoryPins
{
    private static readonly string[] Requirements = ["docs/requirements.txt", ".github/requirements.txt"];

    private static readonly string[] Workflows = [".github/workflows", "template/.github/workflows"];

    private const string AzurePipeline = "template/azure-pipelines.yml";

    [GeneratedRegex("""<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"\s*/>""")]
    private static partial Regex NugetPin();

    [GeneratedRegex(@"^([A-Za-z0-9._-]+)==(\S+)", RegexOptions.Multiline)]
    private static partial Regex PypiPin();

    [GeneratedRegex(@"uses:\s*([A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+)\S*@[0-9a-f]{40}\s+#\s*v(\S+)")]
    private static partial Regex ActionPin();

    [GeneratedRegex(@"go install\s+(github\.com/[^/\s]+/[^/\s@]+)\S*@v(\S+)")]
    private static partial Regex GoPin();

    [GeneratedRegex(@"^\s+(?:dotnet-)?version:\s*(\d+\.\d+\.\d+)\s*$", RegexOptions.Multiline)]
    private static partial Regex SdkPin();

    /// <summary>
    /// Every NuGet pin under <c>tooling/</c>, every pip pin, the SDK in <c>global.json</c> and the starter pipelines,
    /// and every action and <c>go install</c> in a workflow, keyed by package URL.
    /// </summary>
    internal static IReadOnlyDictionary<string, IReadOnlyList<Pin>> Read(string root)
    {
        var pins = new Dictionary<string, List<Pin>>(StringComparer.Ordinal);

        foreach (var project in Directory.EnumerateFiles(Path.Combine(root, "tooling"), "*.csproj",
                     SearchOption.AllDirectories))
        {
            foreach (Match m in NugetPin().Matches(Files.ReadLf(project)))
                Add($"pkg:nuget/{m.Groups[1].Value}", m.Groups[2].Value, Rel(project));
        }

        foreach (var requirements in Requirements)
        {
            foreach (Match m in PypiPin().Matches(Files.ReadLf(Path.Combine(root, requirements))))
                Add($"pkg:pypi/{m.Groups[1].Value.ToLowerInvariant()}", m.Groups[2].Value, requirements);
        }

        using (var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "global.json"))))
            Add("pkg:generic/dotnet-sdk", json.RootElement.GetProperty("sdk").GetProperty("version").GetString() ?? "",
                "global.json");

        foreach (Match m in SdkPin().Matches(Files.ReadLf(Path.Combine(root, AzurePipeline))))
            Add("pkg:generic/dotnet-sdk", m.Groups[1].Value, AzurePipeline);

        foreach (var workflow in Workflows.SelectMany(w => Directory.EnumerateFiles(Path.Combine(root, w), "*.yml"))
                     .Order())
        {
            var text = Files.ReadLf(workflow);

            // A package URL's github namespace is lower case, so `NuGet/login` is `pkg:github/nuget/login`.
            foreach (Match m in ActionPin().Matches(text))
                Add($"pkg:github/{m.Groups[1].Value.ToLowerInvariant()}", m.Groups[2].Value, Rel(workflow));
            foreach (Match m in GoPin().Matches(text))
                Add($"pkg:golang/{m.Groups[1].Value}", m.Groups[2].Value, Rel(workflow));
            foreach (Match m in SdkPin().Matches(text))
                Add("pkg:generic/dotnet-sdk", m.Groups[1].Value, Rel(workflow));
        }

        return pins.ToDictionary(p => p.Key, IReadOnlyList<Pin> (p) => p.Value, StringComparer.Ordinal);

        string Rel(string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        void Add(string purl, string version, string file)
        {
            if (!pins.TryGetValue(purl, out var list)) pins[purl] = list = [];
            if (!list.Contains(new Pin(version, file))) list.Add(new Pin(version, file));
        }
    }
}
