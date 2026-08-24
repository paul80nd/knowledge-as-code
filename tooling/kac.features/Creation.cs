// The harness behind the creation specs: put a folder into a state, run `new` against it in-process, and
// answer with the exit code. It calls `Commands.New`, which is the whole of what `kac new` runs, so a
// precondition cannot stop the command and go unnoticed here.
//
// The template is this repository, read as a folder rather than cloned. Nothing here reaches the network.

using kac.core;

public static class Creation
{
    private static readonly string RepoRoot = Harness.RepoRoot;

    // A version the template can always be read by, taken from the template's own floor. What a tool too
    // old for a manifest does is settled in the unit tests, and pinning a number here would break these
    // specs every time that floor moved.
    private static string NewEnough =>
        Manifest.LoadFrom(Path.Combine(RepoRoot, Manifest.FileName)).MinimumTool ?? "0.0.0";

    public static int Create(string folder, string? from = null) =>
        Commands.New(
            folder,
            new NewRequest { From = from ?? RepoRoot, Yes = true, Ci = CiSystem.None },
            NewEnough,
            "2026-01-01");
}
