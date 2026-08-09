// ---------------------------------------------------------------------------
// Links, labels and link definitions
// ---------------------------------------------------------------------------

namespace kac.core;

// The link half of a document's checks. They ask about prose rather than about frontmatter, which is
// what lets them run unchanged against a type page — a page that is not a record and has no
// frontmatter to ask about.
public static class LinkChecks
{
    // A page that is not a record gets these and nothing else.
    public static void CheckPage(Doc d, Schema schema, string repoRoot, List<Finding> f) =>
        Check(d, schema, repoRoot,
            (check, msg, line) => f.Add(new Finding(d.Rel, line, Sev.Error, check, msg)),
            (check, msg, line) => f.Add(new Finding(d.Rel, line, Sev.Warning, check, msg)));

    public static void Check(Doc d, Schema schema, string repoRoot, Action<string, string, int?> err,
        Action<string, string, int?> warn)
    {
        foreach (var link in d.Links)
        {
            var target = link.Target;
            if (string.IsNullOrEmpty(target)) continue;
            if (IsExternal(target) || target.StartsWith('#')) continue;
            if (!ResolveTarget(repoRoot, d.Rel, target))
                err("link-resolves", $"link target '{target}' does not resolve.", link.Line);
        }

        // undefined shortcut/reference labels left as literal '[x]'. Id-shaped is an error — the author
        // meant to reference a document; anything else is only a warning, since '[x]' in prose is legal.
        var defined = new HashSet<string>(d.DefinedLabels, StringComparer.OrdinalIgnoreCase);
        foreach (var (inner, line) in d.BareBracketTokens)
        {
            if (defined.Contains(inner)) continue; // a genuine reference that resolved
            if (IdChecks.TryCanonicalId(inner, schema, out _))
                err("undefined-label", $"reference '[{inner}]' has no link definition.", line);
            else
                warn("bracket-literal",
                    $"'[{inner}]' looks like a reference but has no definition (or use an inline link).", line);
        }

        // A shortcut label doubles as its own display text, so it is read as an id and must be written
        // as one. Reference and definition are matched case-insensitively, so a mis-cased label still
        // resolves — nothing else would catch it.
        foreach (var link in d.Links)
        {
            if (!link.IsReference || string.IsNullOrEmpty(link.Label)) continue;
            if (IdChecks.TryCanonicalId(link.Label, schema, out var canonical) && link.Label != canonical)
                err("label-canonical",
                    $"reference '[{link.Label}]' should be written as the id '{canonical}'.", link.Line);
        }

        foreach (var label in d.DefinedLabels.Distinct(StringComparer.Ordinal))
            if (IdChecks.TryCanonicalId(label, schema, out var canonical) && label != canonical)
                err("label-canonical",
                    $"link definition '[{label}]' should be written as the id '{canonical}'.", null);

        // unused definitions
        foreach (var label in d.DefinedLabels.Distinct(StringComparer.OrdinalIgnoreCase))
            if (!d.UsedLabels.Contains(label))
                warn("unused-definition", $"link definition '[{label}]' is never referenced.", null);
    }

    public static bool IsExternal(string t)
        => t.StartsWith("http://") || t.StartsWith("https://") || t.StartsWith("mailto:") || t.StartsWith("tel:");

    public static bool ResolveTarget(string repoRoot, string fromRel, string target)
    {
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var q = target.IndexOf('?');
        if (q >= 0) target = target[..q];
        if (target.Length == 0) return true; // pure fragment

        string basePath;
        if (target.StartsWith('/'))
            basePath = Path.Combine(repoRoot, target.TrimStart('/'));
        else
        {
            var fromDir = Path.GetDirectoryName(Path.Combine(repoRoot, fromRel)) ?? repoRoot;
            basePath = Path.GetFullPath(Path.Combine(fromDir, target));
        }

        basePath = basePath.Replace('\\', '/');

        // A directory is deliberately not a target. In Azure DevOps `data.md` is the page and `data/`
        // is its children — one node — so `/data` is a link to the page, which the `.md` form below
        // already resolves. Accepting the directory as well would resolve a link to a type whose page
        // has gone, and would do it inconsistently: git cannot track an empty directory, so the same
        // link passes on the machine that created the folder and fails in CI.
        return File.Exists(basePath)
               || File.Exists(basePath + ".md"); // ADO resolves links with .md omitted
    }
}
