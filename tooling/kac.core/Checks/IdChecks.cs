namespace kac.core;

// The shape of a discriminator is stated once here and read three ways. `../../CLAUDE.md` says why a second copy is not
// written.
public static class IdChecks
{
    // The styles dispatched below, not a list of spellings. `SchemaChecks` reads it to reject a style nothing acts on,
    // so a name added here needs its branch.
    public static readonly IReadOnlySet<string> IdStyles =
        new HashSet<string>(["numbered", "mnemonic", "slug"], StringComparer.Ordinal);

    public static void Check(string id, int? line, string rel, TypeSchema t, Report report)
    {
        var expectPrefix = t.IdPrefix + "-";
        if (!id.StartsWith(expectPrefix, StringComparison.Ordinal))
        {
            report.Err(new CheckId("id-prefix"), $"id '{id}' must start with '{expectPrefix}'.", line);
            return;
        }

        // The shape first, then the agreement, so a malformed id is reported once.
        var rest = id[expectPrefix.Length..];
        var carried = FilenameDiscriminator(rel, t);

        switch (t.IdStyle)
        {
            case "numbered":
                if (!t.IdWidth.Admits(rest.Length) || !rest.All(char.IsDigit))
                    report.Err(new CheckId("id-format"),
                        $"id '{id}' must be '{expectPrefix}' followed by {t.IdWidth} digits.", line);
                else if (carried is not null && rest != carried)
                    report.Err(new CheckId("id-matches-filename"),
                        $"id '{id}' carries number '{rest}', and the filename carries '{carried}'.", line);
                break;

            // Compared case-insensitively: the id carries the mnemonic upper-case and the filename lower-case.
            case "mnemonic":
                if (!t.IdWidth.Admits(rest.Length) || !rest.All(char.IsLetterOrDigit)
                                                   || !char.IsLetter(rest[0]) || rest != rest.ToUpperInvariant())
                    report.Err(new CheckId("id-format"),
                        $"id '{id}' must be '{expectPrefix}' followed by {t.IdWidth} upper-case alphanumeric "
                        + "characters beginning with a letter.", line);
                else if (carried is not null && !rest.Equals(carried, StringComparison.OrdinalIgnoreCase))
                    report.Err(new CheckId("id-matches-filename"),
                        $"id '{id}' carries mnemonic '{rest}', and the filename carries '{carried}'.", line);
                break;

            // A slug is the whole filename stem, compared entire and case-sensitively.
            case "slug":
                if (!IsSlug(rest))
                    report.Err(new CheckId("id-format"),
                        $"id '{id}' must be '{expectPrefix}' followed by lower-case letters, digits and hyphens.",
                        line);
                else if (carried is not null && rest != carried)
                    report.Err(new CheckId("id-matches-filename"),
                        $"id '{id}' carries slug '{rest}', and the filename carries '{carried}'.", line);
                break;
        }
    }

    public static void CheckFilename(string rel, TypeSchema t, Report report)
    {
        var name = Path.GetFileName(rel);
        if (t.FilenameRegex is not null && !t.FilenameRegex.IsMatch(name))
            report.Err(new CheckId("filename-pattern"), $"filename '{name}' does not match {t.FilenamePattern}.");

        // The limit is on the slug the author chose. A discriminator at the head of the name is skipped, and a slug id
        // is measured whole.
        var slug = Path.GetFileNameWithoutExtension(name);
        if (FilenameDiscriminator(rel, t) is { } head && slug.StartsWith(head + "-", StringComparison.Ordinal))
            slug = slug[(head.Length + 1)..];

        if (slug.Length > t.SlugMax)
            report.Err(new CheckId("slug-length"),
                $"slug '{slug}' is {slug.Length} characters; the limit is {t.SlugMax}.");
    }

    // Null where the type keeps its id out of the filename, or the name does not open with a discriminator in the
    // style's shape.
    private static string? FilenameDiscriminator(string rel, TypeSchema t)
    {
        if (!t.FilenameCarriesId) return null;

        var name = Path.GetFileName(rel);
        switch (t.IdStyle)
        {
            case "numbered":
            {
                var i = 0;
                while (i < name.Length && char.IsDigit(name[i])) i++;
                return i > 0 ? name[..i] : null;
            }

            case "mnemonic":
            {
                var dash = name.IndexOf('-');
                if (!t.IdWidth.Admits(dash)) return null;
                var head = name[..dash];
                return head.All(char.IsLetterOrDigit) ? head : null;
            }

            case "slug":
            {
                var stem = Path.GetFileNameWithoutExtension(name);
                return IsSlug(stem) ? stem : null;
            }

            default:
                return null;
        }
    }

    // Deliberately laxer about case than the checks above, so a mis-cased label in prose is recognised and can be
    // reported. A part is addressed as `<record>.<part>`, and the part half is judged against the type's own `parts:`
    // declaration.
    public static bool TryCanonicalId(string label, Schema schema, out string canonical)
    {
        var dot = label.IndexOf('.');
        if (dot > 0 && dot < label.Length - 1)
        {
            canonical = "";
            var part = label[(dot + 1)..];
            if (!TryCanonicalId(label[..dot], schema, out var record)) return false;
            if (!NamesAPart(record, part, schema)) return false;

            canonical = $"{record}.{part}";
            return true;
        }

        canonical = "";
        var dash = label.IndexOf('-');
        if (dash <= 0 || dash == label.Length - 1) return false;
        var prefix = label[..dash];
        var rest = label[(dash + 1)..];

        var t = schema.ByFolder.Values.FirstOrDefault(x =>
            x.IdPrefix.Length > 0 && string.Equals(x.IdPrefix, prefix, StringComparison.OrdinalIgnoreCase));
        if (t is null) return false;

        switch (t.IdStyle)
        {
            case "numbered":
                if (!t.IdWidth.Admits(rest.Length) || !rest.All(char.IsDigit)) return false;
                canonical = $"{t.IdPrefix}-{rest}";
                return true;
            case "mnemonic":
                if (!t.IdWidth.Admits(rest.Length) || !rest.All(char.IsLetterOrDigit) || !char.IsLetter(rest[0]))
                    return false;
                canonical = $"{t.IdPrefix}-{rest.ToUpperInvariant()}";
                return true;
            case "slug":
                if (!rest.All(c => char.IsLetterOrDigit(c) || c == '-')) return false;
                canonical = $"{t.IdPrefix}-{rest.ToLowerInvariant()}";
                return true;
            default:
                return false;
        }
    }

    // A type declaring no parts has none to name, so `pol-DEVI.md` is not read as a citation.
    private static bool NamesAPart(string record, string part, Schema schema)
    {
        var prefix = record[..record.IndexOf('-')];
        var t = schema.ByFolder.Values.FirstOrDefault(x => string.Equals(x.IdPrefix, prefix, StringComparison.Ordinal));

        if (t?.Parts is not { } spec) return false;
        if (spec.IdPatternRegex is { } pattern) return pattern.IsMatch(part);

        return part.Length > 0 && part == Md.Slug(part);
    }

    // Null where the target is not a record of that type, which is the common answer. The folder decides the type, not
    // the filename, because a slug is shaped like any other word.
    public static string? IdFromLink(LinkRef link, string fromRel, TypeSchema refType)
    {
        var target = link.Target;
        foreach (var cut in new[] { '#', '?' })
        {
            var at = target.IndexOf(cut);
            if (at >= 0) target = target[..at];
        }

        if (target.Length == 0) return null;
        if (Folder(target, fromRel) != refType.Folder) return null;
        if (FilenameDiscriminator(target, refType) is not { } discriminator) return null;

        return refType.IdStyle switch
        {
            "numbered" => refType.IdWidth.Admits(discriminator.Length)
                ? $"{refType.IdPrefix}-{discriminator}"
                : null,
            "mnemonic" => $"{refType.IdPrefix}-{discriminator.ToUpperInvariant()}",
            "slug" => $"{refType.IdPrefix}-{discriminator}",
            _ => null
        };
    }

    // String work, not a filesystem walk: whether the file is there is `link-resolves`'s question.
    private static string Folder(string target, string fromRel)
    {
        target = target.Replace('\\', '/');
        var segments = new List<string>();
        if (!target.StartsWith('/'))
            segments.AddRange(fromRel.Replace('\\', '/').Split('/')[..^1]);

        foreach (var part in target.TrimStart('/').Split('/')[..^1])
        {
            if (part is "" or ".") continue;
            if (part == "..")
            {
                if (segments.Count > 0) segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(part);
        }

        return string.Join('/', segments);
    }

    // ASCII, because a slug is also a URL path segment and a filename.
    private static bool IsSlug(string s) =>
        s.Length > 0 && s.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-');
}
