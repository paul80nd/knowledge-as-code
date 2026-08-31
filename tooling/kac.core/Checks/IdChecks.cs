namespace kac.core;

// An id is a prefix and a discriminator: `adr-0007`, `pol-VURM`, `svc-search`. The prefix names the
// type and the discriminator names the document. Where the filename carries that same discriminator the
// agreement between them is what makes an id and a path two names for one document rather than two
// documents, and what lets a link to a file be read as a citation of an id. `_universal.yaml` states the
// asymmetry it defends: a filename may be corrected, an id may not.
//
// `numbered` and `mnemonic` put the discriminator at the head of a longer filename (0007-…, vurm-…), so
// the rest of the name is a slug the author chose. `slug-max` measures that rest. A `slug` id is the
// whole filename stem, with no head to skip.
//
// A type declaring `filename.carries-id: false` files its records by topic and puts the id in
// frontmatter alone. Everything reading the head of a filename then reads nothing, which is what stops
// `secret-handling.md` binding to a `std-SECRET` its author never wrote.
//
// The shape of a discriminator is stated here once and read three ways: forwards, holding an id to its
// filename; backwards, reading a link's filename as the id it cites; and sideways, deciding whether a
// bracketed label in prose is an id at all. A second copy of it in any of the three would be a place for
// the styles to disagree, and the disagreement would be silent. A label that stops being read as an
// id stops being checked.
public static class IdChecks
{
    // The id styles these checks apply. Held here rather than in the schema loader because this is where
    // the branches are, and SchemaChecks reads it to reject a style nothing acts on. It is a set of what
    // is dispatched rather than a list of what is spelled correctly: adding a name here without a branch
    // beneath is the mistake it exists to prevent.
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

        // The shape first, then the agreement, so a malformed id is reported once rather than twice.
        // Where the filename carries no discriminator the agreement is skipped, for either of two
        // reasons: the type keeps its id out of the filename, or that name has failed filename-pattern
        // and one broken name is one finding.
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
                        $"id '{id}' number does not match filename number '{carried}'.", line);
                break;

            // The id carries the mnemonic upper-case (pol-VURM); the filename carries it lower-case
            // (vurm-…md), so the two are compared case-insensitively.
            case "mnemonic":
                if (!t.IdWidth.Admits(rest.Length) || !rest.All(char.IsLetterOrDigit)
                                                   || !char.IsLetter(rest[0]) || rest != rest.ToUpperInvariant())
                    report.Err(new CheckId("id-format"),
                        $"id '{id}' must be '{expectPrefix}' followed by {t.IdWidth} upper-case alphanumeric "
                        + "characters beginning with a letter.", line);
                else if (carried is not null && !rest.Equals(carried, StringComparison.OrdinalIgnoreCase))
                    report.Err(new CheckId("id-matches-filename"),
                        $"id '{id}' mnemonic does not match filename mnemonic '{carried}'.", line);
                break;

            // A slug is not a fixed-width discriminator cut from the front of the filename: it is the
            // whole of it. So the two are compared entire, and case-sensitively: a slug is lower-case in
            // both places, and this is where that is said.
            case "slug":
                if (!IsSlug(rest))
                    report.Err(new CheckId("id-format"),
                        $"id '{id}' must be '{expectPrefix}' followed by lower-case letters, digits and hyphens.",
                        line);
                else if (carried is not null && rest != carried)
                    report.Err(new CheckId("id-matches-filename"),
                        $"id '{id}' slug does not match filename slug '{carried}'.", line);
                break;
        }
    }

    public static void CheckFilename(string rel, TypeSchema t, Report report)
    {
        var name = Path.GetFileName(rel);
        if (t.FilenameRegex is not null && !t.FilenameRegex.IsMatch(name))
            report.Err(new CheckId("filename-pattern"), $"filename '{name}' does not match {t.FilenamePattern}.");

        // The limit is on the slug the author chose, not on the discriminator they did not. Where the
        // style puts one at the head of a longer name it is skipped; a slug id *is* the whole stem, so
        // it is not a prefix of itself and the whole of it is measured.
        var slug = Path.GetFileNameWithoutExtension(name);
        if (FilenameDiscriminator(rel, t) is { } head && slug.StartsWith(head + "-", StringComparison.Ordinal))
            slug = slug[(head.Length + 1)..];

        if (slug.Length > t.SlugMax)
            report.Err(new CheckId("slug-length"),
                $"slug '{slug}' is {slug.Length} characters; the limit is {t.SlugMax}.");
    }

    // What a filename carries of the id, and nothing of the slug beside it. Null where the type keeps its
    // id out of the filename, and null where the name does not open with a discriminator in the shape the
    // style declares.
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

    // A label is id-shaped when its prefix names a type and the remainder fits that type's id style.
    // The canonical form is the id exactly as the document carries it: the prefix always lower-case,
    // a mnemonic always upper-case, a slug always lower-case.
    //
    // Deliberately laxer about case than the id checks above, and for the opposite reason: this reads a
    // label written in prose to say what it should have been, so it has to recognise the
    // mis-cased form to be able to report it.
    //
    // A part is addressed the same way, as `<record>.<part>`: `pol-VURM.TIMEBOX`, `gls-search.title`.
    // That is the form a citation already uses, and now the form a link to a part uses too. The record
    // half canonicalises as above; the part half is judged against the type's own `parts:` declaration
    // and carried through as written, because only that declaration knows what a part id looks like.
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

    // Whether `part` is shaped like a part of the type that owns `record`.
    //
    // Asked of the type's `parts:` block, because that is the only thing that knows: a policy declares
    // `id-pattern:` and its clauses are upper-case mnemonics, and a glossary sources its parts from
    // headings, whose id is the anchor a heading slugs to. A type declaring no parts has none to name,
    // so `pol-DEVI.md` and `adr-0007.something` fall out here rather than being read as citations.
    private static bool NamesAPart(string record, string part, Schema schema)
    {
        var prefix = record[..record.IndexOf('-')];
        var t = schema.ByFolder.Values.FirstOrDefault(x => string.Equals(x.IdPrefix, prefix, StringComparison.Ordinal));

        if (t?.Parts is not { } spec) return false;
        if (spec.IdPatternRegex is { } pattern) return pattern.IsMatch(part);

        // No declared pattern: the parts come from headings, so a part id is what `Md.Slug` writes.
        return part.Length > 0 && part == Md.Slug(part);
    }

    // The id a link cites, read from the file it points at: `0007-…md` under a numbered type is
    // `adr-0007`. Null where the target is not a record of that type, which is the common answer: this
    // is asked of every link in a `## Related` section, and most of them are prose citations.
    //
    // The folder decides what type a target belongs to, not the filename. A numbered or mnemonic name is
    // distinctive enough that asking the folder rarely changes the answer, but a slug is shaped like any
    // other word. `ripgrep.md` says nothing about which folder it came from. `services.md`, a type page
    // rather than a record, would read as a record of the type it heads. So a relative target is
    // resolved against the folder the citing document sits in, and has to land in the type's own.
    public static string? IdFromLink(LinkRef link, string fromRel, TypeSchema refType)
    {
        var target = link.Target;
        foreach (var cut in new[] { '#', '?' })
        {
            var at = target.IndexOf(cut);
            if (at >= 0) target = target[..at];
        }

        if (target.Length == 0) return null; // a pure fragment addresses this document, not another
        if (Folder(target, fromRel) != refType.Folder) return null;
        if (FilenameDiscriminator(target, refType) is not { } discriminator) return null;

        return refType.IdStyle switch
        {
            // A discriminator of the wrong width is a filename that opens with digits without being a
            // record. The id checks report that where it is written, and it cites nothing here.
            "numbered" => refType.IdWidth.Admits(discriminator.Length)
                ? $"{refType.IdPrefix}-{discriminator}"
                : null,
            "mnemonic" => $"{refType.IdPrefix}-{discriminator.ToUpperInvariant()}",
            "slug" => $"{refType.IdPrefix}-{discriminator}",
            _ => null
        };
    }

    // The folder a target lands in, repo-relative: an absolute target from the root, a relative one from
    // the folder the citing document sits in. String work rather than a filesystem walk, because the
    // question is which type the link names and not whether the file is there. `link-resolves` asks
    // that, and asking it twice would report a missing file as a citation of nothing.
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

    // The slug alphabet, and the one place it is stated: lower-case letters, digits and the hyphen that
    // joins them. ASCII rather than Unicode because a slug is also a URL path segment and a filename on
    // three operating systems.
    private static bool IsSlug(string s) =>
        s.Length > 0 && s.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-');
}
