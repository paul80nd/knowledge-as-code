namespace kac.tests;

// The repository, found by the solution at its root. A corpus is what `kac` walks up for; what the tests using this
// want is the tree carrying the engine and the pages written beside it, and one folder answers to that.
//
// The two walk-ups are deliberately separate. `kac.features/Harness.cs` and `kac-tests.cs` hold their own copies for
// the same reason: each is a different assembly, and neither references this one.
internal static class Repo
{
    internal static readonly string Root = Find();

    private static string Find()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "kac.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException(
            "no 'kac.slnx' above the test assembly — these tests read the repository they ship in.");
    }
}
