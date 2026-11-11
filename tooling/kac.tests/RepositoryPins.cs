using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

internal sealed record Pin(string Version, string File);

/// <summary>Every version this repository pins, keyed by package URL, for the tests comparing a tool record with it.</summary>
internal static partial class RepositoryPins
{
    [GeneratedRegex("""<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"\s*/>""")]
    private static partial Regex NugetPin();

    [GeneratedRegex(@"^([A-Za-z0-9._-]+)==(\S+)", RegexOptions.Multiline)]
    private static partial Regex PypiPin();

    /// <summary>Every NuGet pin under <c>tooling/</c> and pip pin for the site, keyed by package URL.</summary>
    internal static IReadOnlyDictionary<string, IReadOnlyList<Pin>> Read()
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
}
