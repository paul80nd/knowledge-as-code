using System.Reflection;

namespace kac.tests;

// `kac.features/Harness.cs` and `kac-tests.cs` hold their own copies, because neither references this assembly.
// `tooling/CLAUDE.md` says why none of the three may be unified with the tool's own walk-ups.
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
