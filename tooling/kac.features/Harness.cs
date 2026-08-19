// The harness behind every behaviour spec: assemble a throwaway repo root (the real schema plus a
// fixture corpus) and run the validate pipeline in-process against kac.core. It calls Corpus.Load and
// Validator.CheckAll — the same two calls `kac validate` makes, so a check cannot be visible to the
// command and invisible here — and returns the findings rather than printing them. That is the seam
// the steps bind to: a List<Finding> with no Console involved.

using kac.core;

public sealed record ValidationResult(List<Finding> Findings, int Validated, int Skipped);

public static class Harness
{
    private static readonly string RepoRoot = FindRepoRoot();

    public static ValidationResult Validate(string fixtureName)
    {
        var schemaDir = Path.Combine(RepoRoot, "template", ".schema");
        var corpusDir = Path.Combine(RepoRoot, "tooling", "tests", "fixtures", fixtureName, "corpus");
        var temp = Path.Combine(Path.GetTempPath(), "kac-features-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopyTree(schemaDir, Path.Combine(temp, ".schema"));
            CopyTree(corpusDir, temp);

            var corpus = Corpus.Load(temp);
            return new ValidationResult(Validator.CheckAll(corpus), corpus.Docs.Count,
                corpus.SkippedNoFrontmatter);
        }
        finally
        {
            try { Directory.Delete(temp, recursive: true); }
            catch
            {
                /* best-effort cleanup */
            }
        }
    }

    // The repository, found by the solution it holds rather than by the corpus beside it. The specs need
    // the tree that carries the engine, the fixtures and the schema at once, and only one folder answers.
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "tooling", "kac.slnx")))
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
