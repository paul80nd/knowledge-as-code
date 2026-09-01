namespace kac.core;

public static class Files
{
    // Read a file as text with LF line endings, so content comparison and generation are
    // line-ending stable regardless of what the working copy happens to carry.
    public static string ReadLf(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

    // The folder holding a file. `Path.GetDirectoryName` answers null for a path naming no folder
    // above it, which a path this tool builds never is, so the null is a defect rather than a case.
    public static string FolderOf(string path) =>
        Path.GetDirectoryName(path)
        ?? throw new ArgumentException($"'{path}' names no folder above it.", nameof(path));

    // Open the folders above a file, so a write to it lands.
    public static void OpenFolderFor(string path) => Directory.CreateDirectory(FolderOf(path));

    // Copy one file into a corpus, opening the folders above it and carrying the mode across.
    //
    // `.plugin/hooks/breadcrumb` is executable, and a hook arriving without its bit fails silently rather
    // than reporting anything. The mode is read from the source rather than named here, so a template
    // making a second file executable needs no change. Windows has no mode to read, and git there records
    // none either.
    public static void Copy(string source, string target)
    {
        OpenFolderFor(target);
        File.Copy(source, target, overwrite: true);
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(target, File.GetUnixFileMode(source));
    }
}
