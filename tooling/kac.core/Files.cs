namespace kac.core;

public static class Files
{
    // Read a file as text with LF line endings, so content comparison and generation are
    // line-ending stable regardless of what the working copy happens to carry.
    public static string ReadLf(string path) => File.ReadAllText(path).Replace("\r\n", "\n");
}
