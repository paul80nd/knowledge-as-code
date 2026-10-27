using System.Text.RegularExpressions;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class KindTests
{
    private static readonly string[] ReadTheRepositoryToExerciseTheTool =
    [
        "Repo.cs", "CliReference.cs", "SkillReference.cs", "Diff.cs", "ManifestTests.cs", "NewTests.cs",
        "UpdateTests.cs", "CommandsTests.cs", "HistoryTests.cs", "BaselineTests.cs",
    ];

    [GeneratedRegex(@"\bRepo\.Root\b")]
    private static partial Regex ReadsTheRepository();

    [Fact]
    public void A_test_reading_this_repository_says_which_kind_it_is()
    {
        var attribute = $"[Trait({nameof(Kind)}.{nameof(Kind.Of)}, {nameof(Kind)}.{nameof(Kind.Repository)})]";
        var unmarked = new List<string>();

        foreach (var file in Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling", "kac.tests"), "*.cs"))
        {
            var name = Path.GetFileName(file);
            if (ReadTheRepositoryToExerciseTheTool.Contains(name, StringComparer.Ordinal)) continue;

            var source = File.ReadAllText(file);
            if (ReadsTheRepository().IsMatch(source) && !source.Contains(attribute, StringComparison.Ordinal))
                unmarked.Add(name);
        }

        Assert.True(unmarked.Count == 0,
            $"these read the repository and carry no kind: {string.Join(", ", unmarked)}. Add "
            + $"{attribute} where the test asks whether this repository holds together, or name the file "
            + $"in {nameof(ReadTheRepositoryToExerciseTheTool)} where it reads the template to exercise "
            + "the tool.");
    }
}
