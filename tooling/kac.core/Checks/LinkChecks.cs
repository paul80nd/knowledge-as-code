namespace kac.core;

// The link half of a document's checks. They ask about prose, which is what lets them run unchanged
// against a type page: a page is not a record and carries no frontmatter to ask about.
public static class LinkChecks
{
    // A page gets these and nothing else, which is this method with `kind` left at its default. It is
    // read as a record would be, and only its prose is asked about.
    public static void Check(Doc d, Schema schema, Tree tree, Report report, DocKind kind = DocKind.Record)
    {
        foreach (var link in d.Links)
        {
            var target = link.Target;
            if (string.IsNullOrEmpty(target)) continue;
            if (IsExternal(target)) continue;

            // A template's example targets name a document the author has not written yet, so the
            // placeholder stands where the filename will go. Every other target in one is a real link,
            // to the type page or to the framework's own documentation, and is resolved like any other.
            // That is what catches a template pointing at a document the corpus has deleted.
            if (kind == DocKind.Template && Placeholder.In(target)) continue;

            var hash = target.IndexOf('#');
            var fragment = hash >= 0 ? target[(hash + 1)..] : "";
            var path = hash >= 0 ? target[..hash] : target;

            // A fragment with nothing before it names a heading in this document.
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

            // Only a Markdown file offers headings to land on. A link into anything else carries a
            // fragment the corpus cannot judge, and silence is the honest answer. A link naming no
            // fragment asks nothing of the file, so it is never read.
            if (fragment.Length > 0 && file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                CheckFragment(Md.Anchors(tree.Read(file)), fragment, path, link.Line, report);
        }

        // undefined shortcut and reference labels left as literal '[label]'. Id-shaped is an error:
        // the author meant to reference a document. Anything else is only a warning, since a bracket
        // in prose is legal.
        //
        // A template is exempt, because a bracket in its prose is as likely to demonstrate the form as
        // to reference anything. Guidance citing `[pol-DEVI]` shows an author what a clause pointing at
        // another policy looks like, and defines no label because it links to nothing.
        var defined = new HashSet<string>(d.DefinedLabels, StringComparer.OrdinalIgnoreCase);
        if (kind == DocKind.Record)
            foreach (var (inner, line) in d.BareBracketTokens)
            {
                if (defined.Contains(inner)) continue; // a genuine reference that resolved
                if (IdChecks.TryCanonicalId(inner, schema, out _))
                    report.Err(new CheckId("undefined-label"), $"reference '[{inner}]' has no link definition.", line);
                else
                    report.Warn(new CheckId("bracket-literal"),
                        $"'[{inner}]' looks like a reference but has no definition (or use an inline link).", line);
            }

        // A shortcut label doubles as its own display text, so it is read as an id and must be written
        // as one. Reference and definition are matched case-insensitively, so a mis-cased label still
        // resolves: nothing else would catch it.
        //
        // Two ways a label shows the reader an id nothing carries, and the second is asked only where
        // the first passed. A label recognisable as an id and spelled wrongly is told the spelling.
        // Anything else is held against the id the document it leads to carries. Asking both of a
        // mis-cased label would report one fault twice, under two different ids.
        //
        // A template is exempt from the second, as it is from every other question about a bracket: its
        // definitions are exemplars, written under labels nobody has chosen yet.
        var ids = new Dictionary<string, string?>(StringComparer.Ordinal); // see Misnamed
        var cite = kind != DocKind.Template;

        foreach (var link in d.Links)
        {
            if (!link.IsReference || string.IsNullOrEmpty(link.Label)) continue;
            if (IdChecks.TryCanonicalId(link.Label, schema, out var canonical) && link.Label != canonical)
                report.Err(new CheckId("label-canonical"),
                    $"reference '[{link.Label}]' should be written as the id '{canonical}'.", link.Line);
            else if (cite && Misnamed(link.Label, link.Target, d.Rel, schema, tree, ids) is { } led)
                report.Err(new CheckId("label-canonical"),
                    $"reference '[{link.Label}]' leads to '{led.Page}', whose id is '{led.Id}'.", link.Line);
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

        // unused definitions. A template's definitions are exemplars, the block existing to show where
        // definitions go and how they sort, so one that nothing references is the point of it.
        if (kind == DocKind.Record)
            foreach (var label in d.DefinedLabels.Distinct(StringComparer.OrdinalIgnoreCase))
                if (!d.UsedLabels.Contains(label))
                    report.Warn(new CheckId("unused-definition"),
                        $"link definition '[{label}]' is never referenced.");
    }

    // The record a label leads to and the id it carries, where the label is not that id. Null wherever
    // there is nothing to compare: the target leaves the corpus, or names a page rather than a record.
    //
    // This is the half `link-resolves` cannot see. That check passes as soon as the path is real, so a
    // label naming one document and pointing at another gets through it, and the reader is shown an id
    // no document has. The id is read from the target's frontmatter rather than from its filename,
    // because a type may declare `filename.carries-id: false` and keep its id nowhere else.
    //
    // A part is addressed as `<record>.<part>`, so the half before the dot is what has to be the id.
    //
    // `ids` is what each target answered, so a document reaching one record from several labels reads
    // it once. Reading a record means parsing it whole, which is what makes that worth keeping.
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

    // A link that names a heading is a promise the heading is there, and the promise is the half that
    // rots: the file goes on resolving after the heading it pointed into has been renamed, so the link
    // lands silently at the top of the page and the reader is left to find what was meant.
    //
    // Judged on the anchor every renderer agrees on; see `Md.Slug`. A heading whose punctuation makes
    // renderers disagree therefore fails here, where somebody is looking, and not later in the wiki.
    private static void CheckFragment(HashSet<string> anchors, string fragment, string page, int? line,
        Report report)
    {
        if (anchors.Contains(fragment)) return;
        report.Err(new CheckId("fragment-resolves"), $"'#{fragment}' names no heading in '{page}'.", line);
    }

    // The corpus path a link target names, or null where the corpus holds nothing there. It returns the
    // path that resolved, so a caller can go on to read it. Which of the two forms below answered is the
    // resolver's business; the file it found is the caller's.
    //
    // Asked of the corpus and never of the disk, so a link resolves in a fresh clone exactly where it
    // resolves here. A file the repository ignores is not something a reader can follow.
    //
    // Public because the export follows links too, and the corpus should have one account of what a
    // target names. A second copy here is where the check and the export would begin to disagree.
    public static string? Resolve(Tree tree, string fromRel, string target)
    {
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var q = target.IndexOf('?');
        if (q >= 0) target = target[..q];
        if (target.Length == 0) return null; // pure fragment: no file of its own

        var rel = target.StartsWith('/')
            ? Descend("", target.TrimStart('/'))
            : Descend(Folder(fromRel), target);

        if (rel is null) return null; // climbed out of the corpus, so nothing it could name

        // A directory is deliberately not a target. In Azure DevOps `data.md` is the page and `data/`
        // is its children, and the two are one node. So `/data` is a link to the page, which the `.md`
        // form below already resolves. Accepting the directory as well would resolve a link to a type
        // whose page has gone.
        if (tree.Exists(rel)) return rel;
        return tree.Exists(rel + ".md") ? rel + ".md" : null; // ADO resolves links with .md omitted
    }

    private static string Folder(string rel)
    {
        var slash = rel.Replace('\\', '/').LastIndexOf('/');
        return slash < 0 ? "" : rel.Replace('\\', '/')[..slash];
    }

    // `base/target` with '.' and '..' resolved, as a corpus path. Null where '..' climbs above the root:
    // there is no such file, and letting it wrap round to a path that happens to exist would resolve a
    // link nobody could follow.
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
