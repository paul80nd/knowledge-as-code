// Runs `new` through `Commands.New`, so a precondition that stops the command fails a spec. The template is this
// repository read as a folder, so nothing reaches the network.

using kac.core;

public static class Creation
{
    private static readonly string RepoRoot = Harness.RepoRoot;

    /// <summary>
    /// Read from the template's own floor, because a pinned number would break these specs whenever that floor moved.
    /// </summary>
    private static string NewEnough =>
        Manifest.LoadFrom(Path.Combine(RepoRoot, Manifest.FileName)).MinimumTool ?? "0.0.0";

    public static int Create(string folder, string? from = null, string? types = null) =>
        Commands.New(
            folder,
            new NewRequest { From = from ?? RepoRoot, Yes = true, Ci = CiSystem.None, Types = types },
            NewEnough,
            "2026-01-01");

    public static int WouldChangeUnderFull(string folder) =>
        Commands.Update(
            folder,
            new UpdateRequest
            {
                From = RepoRoot, Check = true, Policy = CorpusDescriptor.Full, Yes = true
            },
            NewEnough,
            "2026-01-01");
}
