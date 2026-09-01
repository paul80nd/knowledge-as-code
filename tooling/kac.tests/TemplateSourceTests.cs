// Nothing here reaches the network: a folder is read where it sits, and the clone runs against a `file://`
// URL naming a real repository on this machine.

using kac.core;

namespace kac.tests;

public class TemplateSourceTests : IDisposable
{
    private readonly DirectoryInfo _temp = Directory.CreateTempSubdirectory("kac-template-");

    public void Dispose() => _temp.Delete(recursive: true);

    private string Dir(string name)
    {
        var path = Path.Combine(_temp.FullName, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Write(string root, string rel, string text = "")
    {
        var path = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
        Files.OpenFolderFor(path);
        File.WriteAllText(path, text);
    }

    [Fact]
    public void A_folder_is_read_where_it_sits()
    {
        var from = Dir("template");
        Write(from, "manifest.yaml");

        var fetch = TemplateSource.Read("new", from, null, null, _temp.FullName, prompt: false);

        Assert.Null(fetch.Problem);
        var source = fetch.Fetched();
        Assert.Equal(from, source.Root);
        Assert.Null(source.Commit);
    }

    [Fact]
    public void A_folder_survives_being_disposed()
    {
        var from = Dir("template");
        Write(from, "manifest.yaml");

        TemplateSource.Read("new", from, null, null, _temp.FullName, prompt: false).Fetched().Dispose();

        Assert.True(Directory.Exists(from));
    }

    // The take is the fetch plus everything both verbs then ask of the template. Its two refusals are
    // what a rearrangement of that sequence would lose, and the fetch above cannot reach either.
    [Fact]
    public void A_template_with_no_manifest_is_refused_by_the_verb_that_asked()
    {
        var from = Dir("template");
        Write(from, "README.md");

        using var take = TemplateSource.Take("new", from, from, null, null, _temp.FullName,
            prompt: false, toolVersion: "0.6.0");

        Assert.StartsWith("new: ", take.Problem);
        Assert.Contains("holds no manifest.yaml", take.Problem);
    }

    [Fact]
    public void A_template_asking_for_a_newer_tool_is_refused()
    {
        var from = Dir("template");
        Write(from, "manifest.yaml", "minimum-tool: \"9.9.9\"\n");

        using var take = TemplateSource.Take("update", from, from, null, null, _temp.FullName,
            prompt: false, toolVersion: "0.6.0");

        Assert.StartsWith("update: ", take.Problem);
        Assert.Contains("9.9.9", take.Problem);
    }

    [Fact]
    public void A_path_names_the_folder_holding_the_manifest()
    {
        var from = Dir("repo");
        Write(from, "framework/manifest.yaml");

        var fetch = TemplateSource.Read("new", from, null, "framework", _temp.FullName, prompt: false);

        Assert.Equal(Path.Combine(from, "framework"), fetch.Fetched().Root);
    }

    [Fact]
    public void A_repository_is_listed_by_git_so_what_it_ignores_is_never_read()
    {
        var from = Dir("repo");
        Write(from, ".gitignore", "bin/\n");
        Write(from, "manifest.yaml");
        Write(from, "bin/kac.dll");
        if (!GitCli.Repository(from)) return;

        var files = TemplateSource.Read("new", from, null, null, _temp.FullName, prompt: false).Fetched().Files();

        Assert.Contains("manifest.yaml", files);
        Assert.DoesNotContain("bin/kac.dll", files);
    }

    [Fact]
    public void A_folder_that_is_no_repository_is_walked()
    {
        var from = Dir("plain");
        Write(from, "manifest.yaml");
        Write(from, "template/CLAUDE.md");

        var files = TemplateSource.Read("new", from, null, null, _temp.FullName, prompt: false).Fetched().Files();

        Assert.Equal(["manifest.yaml", "template/CLAUDE.md"], files.OrderBy(f => f, StringComparer.Ordinal));
    }

    [Fact]
    public void A_repository_is_cloned_and_the_ref_resolves_to_a_commit()
    {
        var origin = Dir("origin");
        Write(origin, "manifest.yaml", "version: 4\n");
        if (!GitCli.Repository(origin)) return;

        var read = TemplateSource.Read("new", Url(origin), GitCli.Branch, null, _temp.FullName, prompt: false);

        Assert.Null(read.Problem);
        using var fetch = read.Fetched();
        Assert.NotEqual(origin, fetch.Root);
        Assert.Equal(40, fetch.Commit!.Length);
        Assert.Contains("manifest.yaml", fetch.Files());
    }

    [Fact]
    public void A_clone_is_removed_when_it_is_no_longer_needed()
    {
        var origin = Dir("origin");
        Write(origin, "manifest.yaml");
        if (!GitCli.Repository(origin)) return;

        var fetch = TemplateSource.Read("new", Url(origin), null, null, _temp.FullName, prompt: false);
        Assert.Null(fetch.Problem);

        var source = fetch.Fetched();
        var clone = source.Root;
        source.Dispose();

        Assert.False(Directory.Exists(clone));
    }

    [Fact]
    public void A_clone_that_failed_carries_what_git_said()
    {
        var fetch = TemplateSource.Read("new", Url(Path.Combine(_temp.FullName, "absent")), "main", null,
            _temp.FullName, prompt: false);

        Assert.Null(fetch.Source);
        Assert.Contains("could not clone", fetch.Problem);
        Assert.Contains("at 'main'", fetch.Problem);
        Assert.Contains("git said:", fetch.Problem);
    }

    [Fact]
    public void A_clone_holding_no_such_path_is_refused()
    {
        var origin = Dir("origin");
        Write(origin, "manifest.yaml");
        if (!GitCli.Repository(origin)) return;

        var fetch = TemplateSource.Read("new", Url(origin), null, "framework", _temp.FullName, prompt: false);

        Assert.Null(fetch.Source);
        Assert.Contains("holds no 'framework' folder", fetch.Problem);
    }

    [Fact]
    public void A_failed_clone_leaves_no_folder_behind()
    {
        var into = Dir("into");
        TemplateSource.Read("new", Url(Path.Combine(_temp.FullName, "absent")), null, null, into, prompt: false);

        Assert.Empty(Directory.EnumerateFileSystemEntries(into));
    }

    // A local repository addressed as a URL, which is what makes the clone above a real clone and still
    // reaches nothing outside this machine.
    private static string Url(string path) => new Uri(path).AbsoluteUri;
}
