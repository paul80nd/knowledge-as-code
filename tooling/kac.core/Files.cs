namespace kac.core;

public static class Files
{
    public static string ReadLf(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

    // A path this tool builds always names a folder, so a null is a defect rather than a case.
    public static string FolderOf(string path) =>
        Path.GetDirectoryName(path)
        ?? throw new ArgumentException($"'{path}' names no folder above it.", nameof(path));

    public static void OpenFolderFor(string path) => Directory.CreateDirectory(FolderOf(path));

    // Carries the mode, because `.plugin/hooks/breadcrumb` is executable and a hook without its bit fails
    // silently.
    public static void Copy(string source, string target)
    {
        OpenFolderFor(target);
        File.Copy(source, target, overwrite: true);
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(target, File.GetUnixFileMode(source));
    }
}
