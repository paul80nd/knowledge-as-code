// Runs `Corpus.Load` then `Validator.CheckAll`, which are the two calls `kac validate` makes. Assembling a
// subset of that sequence would leave checks unreachable from a spec while every spec went on passing.

using kac.core;

public sealed record ValidationResult(List<Finding> Findings, int Validated, int Skipped);

public static class Harness
{
    internal static readonly string RepoRoot = FindRepoRoot();

    // The day every scenario is judged against, fixed so a rule written against `today()` gives one answer.
    // `FixtureDateTests` in `kac.tests` keeps the fixtures' dates clear of it.
    internal static readonly DateOnly Today = new(2026, 6, 15);

    public static ValidationResult Validate(string fixtureName)
    {
        var schemaDir = Path.Combine(RepoRoot, ".schema");
        var corpusDir = Path.Combine(RepoRoot, "tooling", "tests", "fixtures", fixtureName, "corpus");
        var temp = Path.Combine(Path.GetTempPath(), "kac-features-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopyTree(schemaDir, Path.Combine(temp, ".schema"));

            // The marker that makes the assembled tree a corpus. `kac-tests.cs` says why it is empty.
            Directory.CreateDirectory(temp);
            File.WriteAllText(Path.Combine(temp, ".corpus.yaml"),
                "# Written by the behaviour specs so that kac reads this tree as a corpus.\n");

            CopyTree(corpusDir, temp);

            var corpus = Corpus.Load(temp);
            return new ValidationResult(Validator.CheckAll(corpus, Today), corpus.Docs.Count,
                corpus.SkippedNoFrontmatter);
        }
        finally
        {
            try { Directory.Delete(temp, recursive: true); }
            // A file still open leaves the tree for the machine to clear.
            // Failing here would report cleanup as a corpus fault.
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
            }
        }
    }

    // Found by the solution file, not a `.corpus.yaml`: the specs need the engine, the fixtures and the schema at once.
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "kac.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new InvalidOperationException($"kac.features: repo root not found from {AppContext.BaseDirectory}");
    }

    private static void CopyTree(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(dst, Path.GetRelativePath(src, dir)));
        foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(dst, Path.GetRelativePath(src, file)), overwrite: true);
    }
}
