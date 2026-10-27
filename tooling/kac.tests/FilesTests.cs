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

    [Fact]
    public void DeleteTree_of_a_folder_that_is_not_there_does_nothing()
        => Files.DeleteTree(Path.Combine(Path.GetTempPath(), "kac-files-" + Guid.NewGuid().ToString("n")));
}
