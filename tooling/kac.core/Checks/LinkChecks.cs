namespace kac.core;

public static class LinkChecks
{
    /// <summary>
    /// A type page is checked with <c>kind</c> left at its default, so only its prose is asked about.
    /// </summary>
    public static void Check(Doc d, Schema schema, Tree tree, Report report, DocKind kind = DocKind.Record)
    {
        foreach (var link in d.Links)
        {
            var target = link.Target;
            if (string.IsNullOrEmpty(target)) continue;
            if (IsExternal(target)) continue;

            if (kind == DocKind.Template && Placeholder.In(target))
            {
                CheckTemplateFolder(tree, d.Rel, target, link.Line, report);
                continue;
            }

            var hash = target.IndexOf('#');
            var fragment = hash >= 0 ? target[(hash + 1)..] : "";
            var path = hash >= 0 ? target[..hash] : target;

            if (path.Length == 0)
            {
                if (fragment.Length > 0)
                    CheckFragment(Md.Anchors(d.Ast), fragment, d.Rel, link.Line, report);
                continue;
            }

            var file = Resolve(tree, d.Rel, target);
            if (file is null)
            {
                report.Err(new CheckId("link-resolves"), $"link target '{target}' does not resolve.", link.Line);
                continue;
            }

            if (fragment.Length > 0 && file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                CheckFragment(Md.Anchors(tree.Read(file)), fragment, path, link.Line, report);
        }

        // An undefined id-shaped label is an error, and any other bracket a warning. A template is exempt: a bracket in
        // its guidance demonstrates the form.
        var defined = new HashSet<string>(d.DefinedLabels, StringComparer.OrdinalIgnoreCase);
        if (kind == DocKind.Record)
            foreach (var (inner, line) in d.BareBracketTokens)
            {
                if (defined.Contains(inner)) continue;
                if (IdChecks.TryCanonicalId(inner, schema, out _))
                    report.Err(new CheckId("undefined-label"), $"reference '[{inner}]' has no link definition.", line);
                else
                    report.Warn(new CheckId("bracket-literal"),
                        $"'[{inner}]' looks like a reference but has no definition (or use an inline link).", line);
            }

        // A label is asked its spelling first, and the id its target carries only where the spelling passed, so a
        // mis-cased label is reported once.
        var ids = new Dictionary<string, string?>(StringComparer.Ordinal);
        var cite = kind != DocKind.Template;

        foreach (var link in d.Links)
        {
            if (!link.IsReference || link.DisplayedLabel is not { Length: > 0 } shown) continue;
            if (IdChecks.TryCanonicalId(shown, schema, out var canonical) && shown != canonical)
                report.Err(new CheckId("label-canonical"),
                    $"reference '[{shown}]' should be written as the id '{canonical}'.", link.Line);
            else if (cite && Misnamed(shown, link.Target, d.Rel, schema, tree, ids) is { } led)
                report.Err(new CheckId("label-canonical"),
                    $"reference '[{shown}]' leads to '{led.Page}', whose id is '{led.Id}'.", link.Line);
        }

        foreach (var label in d.DefinedLabels.Distinct(StringComparer.Ordinal))
        {
            if (IdChecks.TryCanonicalId(label, schema, out var canonical) && label != canonical)
                report.Err(new CheckId("label-canonical"),
                    $"link definition '[{label}]' should be written as the id '{canonical}'.");
            else if (cite && Misnamed(label, d.DefinedTargets.GetValueOrDefault(label, ""), d.Rel, schema,
                         tree, ids) is { } led)
                report.Err(new CheckId("label-canonical"),
                    $"link definition '[{label}]' leads to '{led.Page}', whose id is '{led.Id}'.");
        }

        // A template's definitions are exemplars, so one nothing references is the point of it.
        if (kind == DocKind.Record)
            foreach (var label in d.DefinedLabels.Distinct(StringComparer.OrdinalIgnoreCase))
                if (!d.UsedLabels.Contains(label))
                    report.Warn(new CheckId("unused-definition"),
                        $"link definition '[{label}]' is never referenced.");
    }

    /// <summary>
    /// The id is read from the target's frontmatter, not its filename, because a type may keep its id nowhere else.
    /// <c>ids</c> keeps each target's answer, since reading a record parses it whole.
    /// </summary>
    private static (string Id, string Page)? Misnamed(
        string? label, string target, string fromRel, Schema schema, Tree tree,
        Dictionary<string, string?> ids)
    {
        if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(target) || IsExternal(target)) return null;

        var file = Resolve(tree, fromRel, target);
        if (file is null || !file.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) return null;

        if (!ids.TryGetValue(file, out var found))
            ids[file] = found = Doc.Parse(file, tree.Read(file), schema)?.FrontScalar("id");
        if (found is null) return null;

        var dot = label.IndexOf('.');
        var named = dot > 0 ? label[..dot] : label;
        return string.Equals(named, found, StringComparison.Ordinal) ? null : (found, file);
    }

    public static bool IsExternal(string t)
        => t.StartsWith("http://") || t.StartsWith("https://") || t.StartsWith("mailto:") || t.StartsWith("tel:");

    /// <summary>Judged on the anchor every renderer agrees on. See <c>Md.Slug</c>.</summary>
    private static void CheckFragment(HashSet<string> anchors, string fragment, string page, int? line,
        Report report)
    {
        if (anchors.Contains(fragment)) return;
        report.Err(new CheckId("fragment-resolves"), $"'#{fragment}' names no heading in '{page}'.", line);
    }

    /// <summary>
    /// Asked of the corpus, never of the disk, so a link resolves in a fresh clone as it does here. The export follows
    /// links through this too, so the two keep one account of what a target names.
    /// </summary>
    public static string? Resolve(Tree tree, string fromRel, string target)
    {
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var q = target.IndexOf('?');
        if (q >= 0) target = target[..q];
        if (target.Length == 0) return null;

        var rel = target.StartsWith('/')
            ? Descend("", target.TrimStart('/'))
            : Descend(Folder(fromRel), target);

        if (rel is null) return null;

        // A directory is not a target: in Azure DevOps `data.md` and `data/` are one node, so `/data` links the page,
        // which the `.md` form resolves. Accepting the directory would resolve a link to a type whose page has gone.
        if (tree.Exists(rel)) return rel;
        // ADO resolves links with .md omitted.
        return tree.Exists(rel + ".md") ? rel + ".md" : null;
    }

    /// <summary>
    /// A placeholder target cannot resolve until it is filled, but the folders ahead of the first placeholder already
    /// can. A target that stays in the template's own folder, or climbs to the root, asks nothing.
    /// </summary>
    private static void CheckTemplateFolder(Tree tree, string fromRel, string target, int line, Report report)
    {
        var slash = target[..target.IndexOf("{{", StringComparison.Ordinal)].LastIndexOf('/');
        if (slash < 0) return;

        var folder = target.StartsWith('/')
            ? Descend("", target[..slash].TrimStart('/'))
            : Descend(Folder(fromRel), target[..slash]);
        if (string.IsNullOrEmpty(folder) || tree.HasFolder(folder)) return;

        report.Warn(new CheckId("template-folder-resolves"),
            $"link target '{target}' points into '{folder}/', which this corpus does not have.", line);
    }

    private static string Folder(string rel)
    {
        var slash = rel.Replace('\\', '/').LastIndexOf('/');
        return slash < 0 ? "" : rel.Replace('\\', '/')[..slash];
    }

    /// <summary>Null where '..' climbs above the root, so the path never wraps round to one that exists.</summary>
    private static string? Descend(string baseDir, string target)
    {
        var parts = new List<string>();
        var combined = baseDir.Length == 0 ? target : baseDir + "/" + target;
        foreach (var segment in combined.Replace('\\', '/').Split('/'))
        {
            if (segment is "" or ".") continue;
            if (segment != "..")
            {
                parts.Add(segment);
                continue;
            }

            if (parts.Count == 0) return null;
            parts.RemoveAt(parts.Count - 1);
        }

        return string.Join('/', parts);
    }
}
