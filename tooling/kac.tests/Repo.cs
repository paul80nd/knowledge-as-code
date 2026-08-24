using System.Reflection;

namespace kac.tests;

// The repository, found by the solution at its root. A corpus is what `kac` walks up for; what the tests using this
// want is the tree carrying the engine and the pages written beside it, and one folder answers to that.
//
// `kac.features/Harness.cs` and `kac-tests.cs` hold their own copies because each is a different assembly and neither
// references this one. The root `CLAUDE.md` says why none of the three may be unified with the tool's own.
internal static class Repo
{
    internal static readonly string Root = Find();

    // The `kac` the CLI reference asks for its usage. Its path is stamped into this assembly by kac.tests.csproj,
    // because only the build knows which configuration it put the executable in.
    internal static readonly string KacAssembly =
        typeof(Repo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == "KacAssembly").Value
        ?? throw new InvalidOperationException("kac.tests: no 'KacAssembly' metadata. kac.tests.csproj stamps it.");

    private static string Find()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "kac.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException(
            "no 'kac.slnx' above the test assembly: these tests read the repository they ship in.");
    }
}
