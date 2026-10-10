namespace kac.core;

// What `.dist/` holds, and which command owns which part. Each command replaces its own directory whole, so each
// directory must hold nothing another command writes. `.gitignore` covers `.dist/` in one line.
public static class Dist
{
    public const string Root = ".dist";

    public const string ExportDir = "export";
    public const string PluginDir = "plugin";
    public const string PackageDir = "package";
    public const string MarketplaceDir = ".claude-plugin";

    // The local marketplace `bundle` writes beside the plugin, so the plugin installs from a path while it is proved.
    public const string MarketplaceRel = MarketplaceDir + "/marketplace.json";

    // What `kac export` writes: the corpus as data, rebuilt whole each run.
    public const string Export = Root + "/" + ExportDir;

    // What `kac bundle` writes: the plugin directory, rebuilt whole each run.
    public const string Plugin = Root + "/" + PluginDir;

    // What `kac pack` writes: the package a registry is handed, rebuilt whole each run.
    public const string Package = Root + "/" + PackageDir;
}
