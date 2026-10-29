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

    /// <summary>Older than the floor this repository's manifest states.</summary>
    public const string TooOld = "0.0.0";

    /// <summary>
    /// <paramref name="tool"/> is the version of <c>kac</c> the run claims to be, where not new enough.
    /// </summary>
    public static int Create(string folder, string? from = null, string? types = null, string? name = null,
        string ci = CiSystem.None, bool yes = true, string? tool = null, string? owner = null,
        string? publishing = null) =>
        Commands.New(
            folder,
            new NewRequest
            {
                From = from ?? RepoRoot, Yes = yes, Ci = ci, Types = types, Name = name, Owner = owner,
                Publishing = publishing
            },
            tool ?? NewEnough,
            "2026-01-01");

    public static int WouldChangeUnderFull(string folder) =>
        Update(folder, new UpdateRequest { Check = true, Policy = CorpusDescriptor.Full, Yes = true });

    /// <summary>The template is this repository, whatever <paramref name="request"/> names as its source.</summary>
    public static int Update(string folder, UpdateRequest request, string? tool = null) =>
        Commands.Update(folder, request with { From = RepoRoot }, tool ?? NewEnough, "2026-01-01");
}
