using kac.core;

public sealed record ValidationResult(List<Finding> Findings, int Validated, int Skipped);

public static class Harness
{
    internal static readonly string RepoRoot = FindRepoRoot();

    /// <summary>
    /// The day every scenario is judged against, fixed so a rule written against <c>today()</c> gives one answer.
    /// <c>FixtureDateTests</c> in <c>kac.tests</c> keeps the fixtures' dates clear of it.
    /// </summary>
    internal static readonly DateOnly Today = new(2026, 6, 15);

    /// <summary>
    /// Copies the schema and a fixture's <c>corpus/</c> into a new temporary folder, and returns its path. A fixture's
    /// <c>plugin/</c> lands at <c>.plugin/</c>, where <c>bundle</c> reads it. The caller deletes the folder.
    /// </summary>
    public static string Assemble(string fixtureName)
    {
        var fixture = Path.Combine(RepoRoot, "tooling", "tests", "fixtures", fixtureName);
        if (!Directory.Exists(fixture))
            throw new InvalidOperationException($"kac.features: no fixture named '{fixtureName}'");

        var temp = Path.Combine(Path.GetTempPath(), "kac-features-" + Guid.NewGuid().ToString("N"));
        CopyTree(Path.Combine(RepoRoot, ".schema"), Path.Combine(temp, ".schema"));

        // The marker that makes the assembled tree a corpus. `kac-tests.cs` says why it is empty.
        File.WriteAllText(Path.Combine(temp, ".corpus.yaml"),
            "# Written by the behaviour specs so that kac reads this tree as a corpus.\n");

        CopyTree(Path.Combine(fixture, "corpus"), temp);

        var plugin = Path.Combine(fixture, "plugin");
        if (Directory.Exists(plugin)) CopyTree(plugin, Path.Combine(temp, ".plugin"));

        return temp;
    }

    /// <summary>
    /// Runs <c>Corpus.Load</c> then <c>Validator.CheckAll</c>, the two calls <c>kac validate</c> makes. A subset of that
    /// sequence would leave checks unreachable from a spec while every spec went on passing.
    /// </summary>
    public static ValidationResult Validate(string corpusRoot)
    {
        var corpus = Corpus.Load(corpusRoot);
        return new ValidationResult(Validator.CheckAll(corpus, Today), corpus.Docs.Count, corpus.SkippedNoFrontmatter);
    }

    public static void Delete(string folder)
    {
        try { Directory.Delete(folder, recursive: true); }
        // A file still open leaves the tree for the machine to clear.
        // Failing here would report cleanup as a corpus fault.
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
        {
        }
    }

    /// <summary>
    /// Found by the solution file, not a <c>.corpus.yaml</c>: the specs need the engine, the fixtures and the schema at
    /// once.
    /// </summary>
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
