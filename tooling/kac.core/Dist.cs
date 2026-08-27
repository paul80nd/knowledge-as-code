namespace kac.core;

// The one statement of what `.dist/` holds and which command owns which part of it.
//
// Both commands that write here replace their own directory whole rather than overwriting in place, so
// each has to own a directory nothing else writes to: an export that deleted `.dist/` would take a
// plugin built from it with it, and a bundle that wrote beside the export's files would leave the next
// export deciding which of them were orphans. Two named subtrees is what keeps delete-then-write a
// local rule instead of a negotiation between the two commands.
//
// `.gitignore` covers `.dist/` in one line, so a directory added here is untracked from the day it
// exists.
public static class Dist
{
    // The root, and the marketplace root with it. A marketplace is a directory holding
    // `.claude-plugin/marketplace.json`, and the plugins it lists sit beneath that directory.
    // `claude plugin validate` refuses a source path containing `..`, so a marketplace cannot sit
    // beside the plugin and point sideways at it. `.dist/` is therefore the marketplace, and the plugin
    // below is what it lists.
    public const string Root = ".dist";

    // The names inside the root. A command writing several files under one of them assembles its paths
    // from these, so the directory is named once whether it is being written to or being deleted.
    public const string ExportDir = "export";
    public const string PluginDir = "plugin";
    public const string PackageDir = "package";
    public const string MarketplaceDir = ".claude-plugin";

    // The local marketplace `bundle` writes beside the plugin, so the plugin can be installed from a
    // path while it is being proved. Relative to the root, because that is where a bundle addresses it.
    public const string MarketplaceRel = MarketplaceDir + "/marketplace.json";

    // What `kac export` writes: the corpus as data, rebuilt whole each run.
    public const string Export = Root + "/" + ExportDir;

    // What `kac bundle` writes: the plugin directory, rebuilt whole each run.
    public const string Plugin = Root + "/" + PluginDir;

    // What `kac pack` writes: the package a registry is handed, rebuilt whole each run. A third named
    // subtree for the reason the first two are named: the directory is deleted before it is written, so
    // it must hold nothing another command owns.
    public const string Package = Root + "/" + PackageDir;
}
