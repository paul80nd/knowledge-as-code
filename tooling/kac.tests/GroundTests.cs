// Unit tests for what `new` reads off a folder before it asks anything, driven through real temp trees.
// What each state comes to is the command's to decide, so these hold the facts and never the refusals.

using kac.core;

namespace kac.tests;

public class GroundTests : IDisposable
{
    private readonly DirectoryInfo _temp = Directory.CreateTempSubdirectory("kac-ground-");

    public void Dispose() => _temp.Delete(recursive: true);

    private string Dir(string name)
    {
        var path = Path.Combine(_temp.FullName, name);
        Directory.CreateDirectory(path);
        return path;
    }

    // -- a corpus already here --

    [Fact]
    public void A_folder_with_no_descriptor_above_it_is_no_corpus()
        => Assert.Null(New.Survey(Dir("empty")).Corpus);

    [Fact]
    public void A_descriptor_in_this_folder_is_found()
    {
        var dir = Dir("corpus");
        File.WriteAllText(Path.Combine(dir, Corpus.Descriptor), "corpus: here\n");

        Assert.Equal(dir, New.Survey(dir).Corpus);
    }

    // The framework reaching a subfolder of a corpus is the same fault as it reaching the corpus itself,
    // and it is the quieter of the two.
    [Fact]
    public void A_descriptor_above_this_folder_is_found_too()
    {
        var above = Dir("corpus");
        File.WriteAllText(Path.Combine(above, Corpus.Descriptor), "corpus: here\n");
        var below = Path.Combine(above, "notes");
        Directory.CreateDirectory(below);

        Assert.Equal(above, New.Survey(below).Corpus);
    }

    // -- git --

    [Fact]
    public void A_folder_that_is_no_repository_says_so_and_reports_no_state()
    {
        var ground = New.Survey(Dir("plain"));

        Assert.False(ground.Repository);
        Assert.Null(ground.Dirty);
    }

    [Fact]
    public void A_repository_with_nothing_uncommitted_is_clean()
    {
        var dir = Dir("clean");
        File.WriteAllText(Path.Combine(dir, "notes.md"), "hello\n");
        if (!GitCli.Repository(dir)) return;

        var ground = New.Survey(dir);

        Assert.True(ground.Repository);
        Assert.False(ground.Dirty);
    }

    // A dirty tree stops the run, so that what `new` writes is legible as a diff.
    [Fact]
    public void A_repository_holding_uncommitted_work_is_dirty()
    {
        var dir = Dir("dirty");
        File.WriteAllText(Path.Combine(dir, "notes.md"), "hello\n");
        if (!GitCli.Repository(dir)) return;
        File.WriteAllText(Path.Combine(dir, "notes.md"), "changed\n");

        Assert.True(New.Survey(dir).Dirty);
    }

    // -- what the folder holds --

    [Fact]
    public void An_empty_folder_holds_nothing()
        => Assert.Empty(New.Survey(Dir("empty")).Holds);

    [Fact]
    public void What_a_folder_holds_is_named_in_order()
    {
        var dir = Dir("busy");
        File.WriteAllText(Path.Combine(dir, "notes.md"), "");
        File.WriteAllText(Path.Combine(dir, "LICENCE"), "");
        Directory.CreateDirectory(Path.Combine(dir, "src"));

        Assert.Equal(["LICENCE", "notes.md", "src"], New.Survey(dir).Holds);
    }

    // A repository with no other content is an empty folder as far as the warning goes: `.git` is what
    // the run is about to write into, and not something a creation is mixed in with.
    [Fact]
    public void A_repository_holding_nothing_else_still_holds_nothing()
    {
        var dir = Dir("fresh");
        if (!GitCli.Run(dir, "init", "-q", ".")) return;

        Assert.Empty(New.Survey(dir).Holds);
    }
}
