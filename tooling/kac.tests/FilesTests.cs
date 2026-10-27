using kac.core;

namespace kac.tests;

public class FilesTests
{
    /// <summary>
    /// Windows refuses to delete a read-only file, and Unix deletes one anyway. Both refuse to delete what a read-only
    /// folder holds, so the folder here is what fails on every platform.
    /// </summary>
    [Fact]
    public void DeleteTree_removes_a_tree_holding_a_read_only_folder_and_a_read_only_file()
    {
        var root = Directory.CreateTempSubdirectory("kac-files-").FullName;
        var objects = Path.Combine(root, ".git", "objects");
        Directory.CreateDirectory(objects);
        var pack = Path.Combine(objects, "pack");
        File.WriteAllText(pack, "");
        File.SetAttributes(pack, FileAttributes.ReadOnly);
        File.SetAttributes(objects, File.GetAttributes(objects) | FileAttributes.ReadOnly);

        Files.DeleteTree(root);

        Assert.False(Directory.Exists(root));
    }

    /// <summary>Windows needs a privilege to create a symbolic link, so the case runs where one is free.</summary>
    [Fact]
    public void DeleteTree_leaves_the_read_only_target_of_a_link_inside_the_tree_read_only()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "creating a symbolic link needs a privilege on Windows.");
        var outside = Directory.CreateTempSubdirectory("kac-files-outside-").FullName;
        var target = Path.Combine(outside, "key");
        File.WriteAllText(target, "");
        File.SetAttributes(target, FileAttributes.ReadOnly);

        var root = Directory.CreateTempSubdirectory("kac-files-").FullName;
        var locked = Path.Combine(root, "locked");
        Directory.CreateDirectory(locked);
        File.CreateSymbolicLink(Path.Combine(locked, "key"), target);
        File.SetAttributes(locked, File.GetAttributes(locked) | FileAttributes.ReadOnly);

        Files.DeleteTree(root);

        Assert.False(Directory.Exists(root));
        Assert.True(File.GetAttributes(target).HasFlag(FileAttributes.ReadOnly));
        Files.DeleteTree(outside);
    }

    [Fact]
    public void DeleteTree_of_a_folder_that_is_not_there_does_nothing()
        => Files.DeleteTree(Path.Combine(Path.GetTempPath(), "kac-files-" + Guid.NewGuid().ToString("n")));
}
