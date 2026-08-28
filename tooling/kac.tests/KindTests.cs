using System.Text.RegularExpressions;

// The trait that sorts this project into two packs, held to being applied.
//
// Nothing about a missing `[Trait]` fails: the class runs, and runs in the wrong pack. So a repository
// guard written without one goes on breaking the tight loop that exists to leave those guards out, which
// is the whole of what the trait buys.
//
// What gives it away is reading the repository. A test that opens `Repo.Root` is either asking whether
// this repository still holds together, or standing the real template up to exercise the tool. Both are
// legitimate and only the first takes the trait, so the second is named below rather than inferred.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class KindTests
{
    // Files that read the repository and are not repository guards.
    //
    // `Repo` and `CliReference` are helpers rather than test classes. The other three stand the real
    // template up and ask what `kac` makes of it, which is the tool's behaviour over the only corpus
    // that ships with it. A fixture could not answer for the template, because the template is the thing
    // under test.
    private static readonly string[] ReadTheRepositoryToExerciseTheTool =
        ["Repo.cs", "CliReference.cs", "ManifestTests.cs", "NewTests.cs", "UpdateTests.cs"];

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
