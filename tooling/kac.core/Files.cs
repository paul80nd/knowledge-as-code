namespace kac.core;

public static class Files
{
    // Read a file as text with LF line endings, so content comparison and generation are
    // line-ending stable regardless of what the working copy happens to carry.
    public static string ReadLf(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

    // Copy one file into a corpus, opening the folders above it and carrying the mode across.
    //
    // `.plugin/hooks/breadcrumb` is executable, and a hook arriving without its bit fails silently rather
    // than reporting anything. The mode is read from the source rather than named here, so a template
    // making a second file executable needs no change. Windows has no mode to read, and git there records
    // none either.
    public static void Copy(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, overwrite: true);
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(target, File.GetUnixFileMode(source));
    }
}
