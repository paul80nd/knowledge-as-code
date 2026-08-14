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
            (check, msg, line) => f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId(check), msg)),
            (check, msg, line) => f.Add(new Finding(d.Rel, line, Sev.Warning, new CheckId(check), msg)));

    public static void Check(Doc d, Schema schema, string repoRoot, Action<string, string, int?> err,
        Action<string, string, int?> warn, DocKind kind = DocKind.Record)
    {
        foreach (var link in d.Links)
        {
            var target = link.Target;
            if (string.IsNullOrEmpty(target)) continue;
            if (IsExternal(target)) continue;

            // A template's example targets name a document the author has not written yet, so the
            // placeholder stands where the filename will go. Every other target in one is a real link
            // — to the type page, to the framework's own documentation — and is resolved like any
            // other, which is what catches a template pointing at a document the corpus has deleted.
            if (kind == DocKind.Template && Placeholder.In(target)) continue;

            var hash = target.IndexOf('#');
            var fragment = hash >= 0 ? target[(hash + 1)..] : "";
            var path = hash >= 0 ? target[..hash] : target;

            // A fragment with nothing before it names a heading in this document.
            if (path.Length == 0)
            {
                CheckFragment(d.Text, fragment, d.Rel, link.Line, err);
                continue;
            }

            var file = Resolve(repoRoot, d.Rel, target);
            if (file is null)
            {
                err("link-resolves", $"link target '{target}' does not resolve.", link.Line);
                continue;
            }

            // Only a Markdown file offers headings to land on. A link into anything else carries a
            // fragment the corpus cannot judge, and silence is the honest answer.
            if (file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                CheckFragment(Files.ReadLf(file), fragment, path, link.Line, err);
        }

        // undefined shortcut/reference labels left as literal '[label]'. Id-shaped is an error — the
        // author meant to reference a document; anything else is only a warning, since a bracket in
        // prose is legal.
        //
        // Not asked of a template, where a bracket in prose is as likely to demonstrate the form as to
        // reference anything. Guidance citing `[pol-DEVI]` shows an author what a clause pointing at
        // another policy looks like, and defines no label because it links to nothing.
        var defined = new HashSet<string>(d.DefinedLabels, StringComparer.OrdinalIgnoreCase);
        if (kind == DocKind.Record)
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

        // unused definitions. A template's definitions are exemplars — the block exists to show where
        // definitions go and how they sort — so one that nothing references is not the oversight it is
        // in a record.
        if (kind == DocKind.Record)
            foreach (var label in d.DefinedLabels.Distinct(StringComparer.OrdinalIgnoreCase))
                if (!d.UsedLabels.Contains(label))
                    warn("unused-definition", $"link definition '[{label}]' is never referenced.", null);
    }

    public static bool IsExternal(string t)
        => t.StartsWith("http://") || t.StartsWith("https://") || t.StartsWith("mailto:") || t.StartsWith("tel:");

    // A link that names a heading is a promise the heading is there, and the promise is the half that
    // rots: the file goes on resolving after the heading it pointed into has been renamed, so the link
    // lands silently at the top of the page and the reader is left to find what was meant.
    //
    // Judged on the anchor every renderer agrees on — see `Md.Slug`. A heading whose punctuation makes
    // renderers disagree therefore fails here rather than only in the wiki, which is the point.
    private static void CheckFragment(string markdown, string fragment, string page, int? line,
        Action<string, string, int?> err)
    {
        if (fragment.Length == 0) return;
        if (Md.Anchors(markdown).Contains(fragment)) return;
        err("fragment-resolves", $"'#{fragment}' names no heading in '{page}'.", line);
    }

    public static bool ResolveTarget(string repoRoot, string fromRel, string target)
        => Resolve(repoRoot, fromRel, target) is not null || StripsToNothing(target);

    private static bool StripsToNothing(string target)
    {
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var q = target.IndexOf('?');
        if (q >= 0) target = target[..q];
        return target.Length == 0;
    }

    // The file a link target names, or null where nothing is there. Returns the path that exists so a
    // caller can go on to read it — which of the two forms below resolved is not the caller's business,
    // but the file it found is.
    private static string? Resolve(string repoRoot, string fromRel, string target)
    {
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var q = target.IndexOf('?');
        if (q >= 0) target = target[..q];
        if (target.Length == 0) return null; // pure fragment — no file of its own

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
        if (File.Exists(basePath)) return basePath;
        return File.Exists(basePath + ".md") ? basePath + ".md" : null; // ADO resolves links with .md omitted
    }
}
