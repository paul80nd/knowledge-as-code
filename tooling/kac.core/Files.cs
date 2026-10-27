namespace kac.core;

public static class Files
{
    public static string ReadLf(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

    /// <summary>A path this tool builds always names a folder, so a null is a defect rather than a case.</summary>
    public static string FolderOf(string path) =>
        Path.GetDirectoryName(path)
        ?? throw new ArgumentException($"'{path}' names no folder above it.", nameof(path));

    public static void OpenFolderFor(string path) => Directory.CreateDirectory(FolderOf(path));

    /// <summary>
    /// Carries the mode, because <c>.plugin/hooks/breadcrumb</c> is executable and a hook without its bit fails
    /// silently.
    /// </summary>
    public static void Copy(string source, string target)
    {
        OpenFolderFor(target);
        File.Copy(source, target, overwrite: true);
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(target, File.GetUnixFileMode(source));
    }

    /// <summary>
    /// Deletes a folder and everything under it, and does nothing where the folder is missing. Git marks the files
    /// under <c>.git/objects</c> read-only and Windows refuses to delete one, so a refused delete clears every
    /// read-only flag and tries again. A link keeps its flag, so nothing outside the tree changes.
    /// </summary>
    public static void DeleteTree(string path)
    {
        if (!Directory.Exists(path)) return;

        try
        {
            Directory.Delete(path, recursive: true);
            return;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }

        var root = new DirectoryInfo(path);
        var everything = new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 };
        foreach (var entry in root.EnumerateFileSystemInfos("*", everything).Prepend(root))
            if (entry.LinkTarget is null && entry.Attributes.HasFlag(FileAttributes.ReadOnly))
                entry.Attributes &= ~FileAttributes.ReadOnly;

        Directory.Delete(path, recursive: true);
    }
}
