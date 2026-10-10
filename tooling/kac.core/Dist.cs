namespace kac.core;

/// <summary>
/// What <c>.dist/</c> holds, and which command owns which part. Each command replaces its own directory whole, so each
/// directory must hold nothing another command writes. <c>.gitignore</c> covers <c>.dist/</c> in one line.
/// </summary>
public static class Dist
{
    public const string Root = ".dist";

    public const string ExportDir = "export";
    public const string PluginDir = "plugin";
    public const string PackageDir = "package";
    public const string MarketplaceDir = ".claude-plugin";

    /// <summary>
    /// The local marketplace <c>bundle</c> writes beside the plugin, so the plugin installs from a path while it is
    /// proved.
    /// </summary>
    public const string MarketplaceRel = MarketplaceDir + "/marketplace.json";

    /// <summary>What <c>kac export</c> writes: the corpus as data, rebuilt whole each run.</summary>
    public const string Export = Root + "/" + ExportDir;

    /// <summary>What <c>kac bundle</c> writes: the plugin directory, rebuilt whole each run.</summary>
    public const string Plugin = Root + "/" + PluginDir;

    /// <summary>What <c>kac pack</c> writes: the package a registry is handed, rebuilt whole each run.</summary>
    public const string Package = Root + "/" + PackageDir;
}
