// Test harness for the BDD pilot: assemble a throwaway repo root (the real schema plus a fixture
// corpus) and run the validate pipeline in-process against kac.core — the same sequence
// Commands.Validate performs, but returning structured findings instead of printing. This is the
// clean seam BDD steps bind to: Validator returns List<Finding> with no Console involved.

using kac.core;

public sealed record ValidationResult(List<Finding> Findings, int Validated, int Skipped);

public static class Harness
{
    private static readonly string RepoRoot = FindRepoRoot();

    public static ValidationResult Validate(string fixtureName)
    {
        var schemaDir = Path.Combine(RepoRoot, ".schema");
        var corpusDir = Path.Combine(RepoRoot, ".tooling", "tests", "fixtures", fixtureName, "corpus");
        var temp = Path.Combine(Path.GetTempPath(), "kac-features-" + Guid.NewGuid().ToString("N"));
        try
        {
            CopyTree(schemaDir, Path.Combine(temp, ".schema"));
            CopyTree(corpusDir, temp);

            var schema = Schema.Load(temp);
            var findings = new List<Finding>();
            var docs = new List<Doc>();
            var skipped = 0;

            foreach (var rel in Corpus.Discover(temp, schema, []))
            {
                var doc = Doc.Parse(rel, File.ReadAllText(Path.Combine(temp, rel)), schema);
                if (doc is null) { skipped++; continue; }
                docs.Add(doc);
            }

            foreach (var doc in docs)
                Validator.CheckDocument(doc, schema, temp, findings);
            Validator.CheckCorpus(docs, findings);

            return new ValidationResult(findings, docs.Count, skipped);
        }
        finally
        {
            try { Directory.Delete(temp, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".schema")))
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
