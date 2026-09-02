namespace kac.tests;

// What a test in this project is about, so that the two kinds here can be run apart.
//
// Most of them ask whether `kac` behaves: given a schema, a document or an export, does the code answer
// correctly. They are what a change to `kac.core` breaks.
//
// A few ask whether **this repository** still holds together: whether a page states the usage the parser
// accepts, whether a comment cites a file that exists, whether the source keeps a convention no compiler
// enforces, whether the changelog carries a section for the version `kac.csproj` names. Those fail when content here drifts and never when the tool's logic is
// wrong, and no corpus consuming `kac` carries any of them, because none of it is shipped.
//
// A trait rather than a project of their own. They are a small tail of this suite rather than half of it,
// `Repo` and `CliReference` are read from both sides, and the whole suite runs in under a second, so a
// second project would buy a name and cost a build. `tooling/README.md` carries the commands.
public static class Kind
{
    // The trait's name. Written once here because a filter matches it as a string, and a second spelling
    // would be a class quietly running in neither pack.
    public const string Of = "Kind";

    public const string Repository = "Repository";
}
