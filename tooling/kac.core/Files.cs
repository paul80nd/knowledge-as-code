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
    /// Deletes a folder and everything under it, and does nothing where the folder is missing. Git marks the files under
    /// <c>.git/objects</c> read-only and Windows refuses to delete one, so every read-only flag is cleared first.
    /// </summary>
    public static void DeleteTree(string path)
    {
        if (!Directory.Exists(path)) return;

        var everything = new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 };
        foreach (var entry in Directory.EnumerateFileSystemEntries(path, "*", everything).Prepend(path))
        {
            var attributes = File.GetAttributes(entry);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(entry, attributes & ~FileAttributes.ReadOnly);
        }

        Directory.Delete(path, recursive: true);
    }
}
