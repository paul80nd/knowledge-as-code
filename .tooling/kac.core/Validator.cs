using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

// ---------------------------------------------------------------------------
// The checks
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Validator
{
    // Everything the validator has to say about a loaded corpus, in the order a reader would ask it:
    // each record on its own, then the pages that are not records, then the questions that need every
    // record in hand, then whether the corpus has the shape its schema declares.
    //
    // This is the whole of `validate` — the command around it only chooses how to print the result.
    // Any caller wanting to know what the tool thinks of a corpus calls this, so no caller can end up
    // running a subset and believing it ran the lot.
    public static List<Finding> CheckAll(LoadedCorpus corpus)
    {
        var (schema, repoRoot) = (corpus.Schema, corpus.RepoRoot);
        var findings = new List<Finding>();

        foreach (var doc in corpus.Docs)
            CheckDocument(doc, schema, repoRoot, findings);

        // A collection type's page is not a record — it carries no frontmatter and describes the
        // documents beneath it rather than being one — so the structural checks do not apply. What it
        // does carry is links, and the generated blocks, and it is the page every record links back
        // to and every contributor reads first. A single-document type's page is absent from this
        // pass because it is a record, already checked above.
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(t.Page)) continue;
            if (corpus.Paths.Count > 0
                && !corpus.Paths.Any(p => t.Page == p.Replace('\\', '/').TrimEnd('/'))) continue;
            var full = Path.Combine(repoRoot, t.Page);
            if (!File.Exists(full)) continue; // absence is type-setup's to report, not this pass's

            var text = File.ReadAllText(full);

            // Every type page carries the two generated blocks, whatever its shape.
            CheckGeneratedBlocks(t.Page, text, [$"schema-{key}", $"checks-{key}"], findings);

            // The link pass is only for a collection's page. A single-document type's page has
            // already had it, as a record, along with everything else.
            if (t.IsSingleDocument) continue;
            var page = Doc.Parse(t.Page, text, schema, requireFrontmatter: false);
            if (page is not null) CheckPageLinks(page, schema, repoRoot, findings);
        }

        // Corpus-wide checks (uniqueness, reciprocity) need every doc in hand.
        CheckCorpus(corpus.Docs, findings);

        // Whether each declared type is stood up. Skipped when the run is narrowed to given paths:
        // asking about one document is not asking about the shape of the corpus, and answering
        // anyway would bury the reply.
        if (corpus.Paths.Count == 0)
            CheckTypeSetup(schema, repoRoot, corpus.Files, findings);

        return findings;
    }

    public static void CheckDocument(Doc d, Schema schema, string repoRoot, List<Finding> f)
    {
        if (d.Type is null)
        {
            Err("type", $"folder '{d.Folder}' has no schema.");
            return;
        }

        var t = d.Type;

        // -- frontmatter parses --
        if (d.Front is null)
        {
            Err("frontmatter-parses", "frontmatter is not a valid YAML mapping.");
            return;
        }

        var present = new Dictionary<string, YamlNode>();
        foreach (var kv in d.Front.Children)
            present[((YamlScalarNode)kv.Key).Value ?? ""] = kv.Value;

        // -- unknown keys --
        foreach (var k in d.FrontKeys)
            if (!t.KnownKeys.Contains(k))
                Err("unknown-key", $"unknown frontmatter key '{k}'.", d.FrontStartLine);

        // -- key order --
        // The actual order must be a topological extension of the chains the schema declares: every
        // pair it orders must hold, and genuinely unconstrained pairs are free. See
        // TypeSchema.DeriveKeyOrderEdges for why the constraint is a pair set rather than a total order.
        CheckKeyOrder(d, t, Err);

        // -- required fields (universal + type), incl. required-when --
        foreach (var spec in t.DeclaredFields)
        {
            var req = spec.Required || RequiredWhenHolds(spec.RequiredWhenCondition, present);
            var absent = !present.ContainsKey(spec.Name) || IsAbsentValue(present[spec.Name]);
            if (req && absent)
            {
                var why = spec.Required ? "" : $" (required when {spec.RequiredWhen})";
                Err("required-field", $"missing required field '{spec.Name}'{why}.", d.FrontStartLine);
            }
        }

        // -- per-field value checks --
        foreach (var (name, node) in present)
        {
            var spec = schema.EffectiveField(t, name);
            if (spec is null) continue; // unknown key already reported

            // absent values must be bare keys, never null / ~ / "" / —
            if (IsAbsentValue(node))
            {
                if (!IsBareKey(node))
                    Err("bare-key",
                        $"'{name}' is absent but not a bare key — use '{name}:' with no value (not null, ~, \"\", or —).",
                        Line(node, d));
                continue;
            }

            switch (spec.Type)
            {
                case "date": CheckDate(name, node, d, Err); break;
                case "enum": CheckEnum(name, node, spec, d, Err); break;
                case "list": CheckList(name, node, spec, d, Err, Warn); break;
            }

            // A declared `pattern:` applies to a scalar field's value; for a list it applies to each
            // entry, so CheckList handles that half where it already walks the sequence.
            if (spec.Type != "list" && node is YamlScalarNode)
                CheckPattern(name, "value", node, spec, d, Err);
        }

        // -- tier matches type --
        if (present.TryGetValue("tier", out var tierNode) && Scalar(tierNode) is { } tier && tier != t.Tier)
            Err("tier-matches-type", $"tier '{tier}' does not match the '{t.TypeName}' type tier '{t.Tier}'.",
                Line(tierNode, d));

        // -- id: prefix, width, matches filename number --
        CheckId(d, t, present, Err);

        // -- filename pattern + slug length --
        CheckFilename(d, t, Err);

        // -- H1 present --
        CheckH1(d, Err);

        // -- identity line beneath the H1 agrees with the frontmatter --
        CheckIdentity(d, t, present, Err);

        // -- required sections --
        foreach (var sec in t.RequiredSections)
            if (!d.H2.Any(h => string.Equals(h, sec, StringComparison.OrdinalIgnoreCase)))
                Err("required-section", $"missing required section '## {sec}'.");

        // -- clause table shape, ids and modals --
        CheckClauses(d, t, Err, Warn);

        // -- links resolve --
        CheckLinks(d, schema, repoRoot, Err, Warn);

        // -- related mirrors ## Related --
        CheckMirrorsSection(d, t, schema, Err);

        // -- the type's own rules --
        CheckRules(d, t, Err, Warn);
        return;

        void Warn(string check, string msg, int? line = null) =>
            f.Add(new Finding(d.Rel, line, Sev.Warning, check, msg));

        void Err(string check, string msg, int? line = null) => f.Add(new Finding(d.Rel, line, Sev.Error, check, msg));
    }

    // The markers a generated block lives between. `Generator.SpliceBlock` looks for the pair and
    // returns the text untouched when either is missing, so a page that loses one silently stops
    // being generated into — and `index --check` agrees it is fresh, because what the generator would
    // write is exactly what is already there. Nothing else can notice, which is why this is a check
    // on the markers rather than on the content between them.
    public static void CheckGeneratedBlocks(string rel, string text, IEnumerable<string> names,
        List<Finding> f)
    {
        foreach (var name in names)
        {
            var begin = text.IndexOf($"<!-- BEGIN GENERATED: {name} -->", StringComparison.Ordinal);
            var end = text.IndexOf($"<!-- END GENERATED: {name} -->", StringComparison.Ordinal);
            if (begin < 0 || end < 0)
                f.Add(new Finding(rel, null, Sev.Error, "generated-block",
                    $"the '{name}' block is missing its "
                    + (begin < 0 && end < 0 ? "markers" : begin < 0 ? "BEGIN marker" : "END marker")
                    + $" — `kac index` writes between them and leaves the page alone without both."));
            else if (end < begin)
                f.Add(new Finding(rel, null, Sev.Error, "generated-block",
                    $"the '{name}' block's END marker comes before its BEGIN marker."));
        }
    }

    // Whether each type the schema declares is stood up, and stood up completely.
    //
    // A schema file says the tool manages this type; it does not say the corpus has built it yet. A
    // corpus adopting the framework a type at a time holds the whole schema and grows into it, so a
    // declared type with nothing behind it is a valid, silent state — nothing links to it, nothing
    // generates for it, and no contributor can reach it.
    //
    // What is not valid is half of one. A folder is the signal that the type has been stood up, and
    // from that point everything the type needs must be there: the page a reader arrives on, and the
    // template a contributor copies. A page without a folder is the same fault from the other side.
    // The generated INDEX is deliberately not checked here — `index --check` already reports it
    // missing or stale, and one fault should not be reported by two commands.
    //
    // A folder counts as present when it holds tracked files. An empty directory git has never seen
    // is not part of the corpus, so the answer is the same in a fresh clone as on the machine that
    // happened to create it.
    public static void CheckTypeSetup(Schema schema, string repoRoot, IEnumerable<string> corpusFiles,
        List<Finding> f)
    {
        var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rel in corpusFiles)
        {
            var slash = rel.Replace('\\', '/').IndexOf('/');
            if (slash > 0) folders.Add(rel.Replace('\\', '/')[..slash]);
        }

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var at = $".schema/{key}.yaml";
            var pageExists = !string.IsNullOrEmpty(t.Page) && File.Exists(Path.Combine(repoRoot, t.Page));

            if (t.IsSingleDocument)
            {
                if (folders.Contains(key))
                    f.Add(new Finding(at, null, Sev.Error, "type-setup",
                        $"type '{key}' is single-document, so '{key}/' must not exist — its page is the document."));
                continue;
            }

            var folder = string.IsNullOrEmpty(t.Folder) ? key : t.Folder;
            if (!folders.Contains(folder))
            {
                if (pageExists)
                    f.Add(new Finding(at, null, Sev.Error, "type-setup",
                        $"type '{key}' has {t.Page} but no '{folder}/' — a type is set up as both or neither."));
                continue;
            }

            var missing = new List<string>();
            if (!pageExists) missing.Add(string.IsNullOrEmpty(t.Page) ? $"{key}.md" : t.Page);
            if (!File.Exists(Path.Combine(repoRoot, folder, "template.md"))) missing.Add($"{folder}/template.md");
            if (missing.Count > 0)
                f.Add(new Finding(at, null, Sev.Error, "type-setup",
                    $"type '{key}' has a '{folder}/' folder but is not fully set up — add {string.Join(", ", missing)}."));
        }
    }

    public static void CheckCorpus(List<Doc> docs, List<Finding> f)
    {
        // id uniqueness across the whole wiki.
        var byId = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in docs)
        {
            var id = d.FrontScalar("id");
            if (id is null) continue;
            if (byId.TryGetValue(id, out var other))
                f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "id-unique",
                    $"id '{id}' is also used by {other.Rel}."));
            else
                byId[id] = d;
        }

        // Clause citations — `pol-VURM.TIMEBOX`. The whole point of giving a clause an id is that
        // something else can name it, and a citation nothing answers to is the failure that machinery
        // exists to prevent: it resolves to a document that plainly exists, so a reader has no reason to
        // doubt the half after the dot.
        foreach (var d in docs)
        foreach (var (citation, line) in d.ClauseRefs)
        {
            var dot = citation.IndexOf('.');
            var (docId, clauseId) = (citation[..dot], citation[(dot + 1)..]);

            if (!byId.TryGetValue(docId, out var target))
                f.Add(new Finding(d.Rel, line, Sev.Error, "clause-ref",
                    $"'{citation}' cites '{docId}', which does not exist."));
            else if (!target.Clauses.Any(c => string.Equals(c.IdSpan, clauseId, StringComparison.Ordinal)))
                f.Add(new Finding(d.Rel, line, Sev.Error, "clause-ref",
                    $"'{citation}' cites a clause '{clauseId}' that {target.Rel} does not carry."));
        }

        // reciprocal fields (e.g. supersedes / superseded-by).
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Reciprocal is null || spec.Ref is null) continue;
                foreach (var targetId in FrontIdList(d, name))
                {
                    if (!byId.TryGetValue(targetId, out var target))
                    {
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "reciprocal",
                            $"'{name}' points at '{targetId}', which does not exist."));
                        continue;
                    }

                    var back = FrontIdList(target, spec.Reciprocal);
                    var selfId = d.FrontScalar("id");
                    if (!back.Any(b => string.Equals(b, selfId, StringComparison.OrdinalIgnoreCase)))
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "reciprocal",
                            $"'{name}: {targetId}' is not reciprocated — {target.Rel} must list '{spec.Reciprocal}: {selfId}'."));
                }
            }
        }
    }

    // -- helpers for individual checks --

    private static void CheckDate(string name, YamlNode node, Doc d, Action<string, string, int?> err)
    {
        var sc = node as YamlScalarNode;
        var v = sc?.Value ?? "";
        var quoted = sc?.Style is ScalarStyle.DoubleQuoted or ScalarStyle.SingleQuoted;
        if (!quoted)
            err("date-quoted", $"'{name}' date must be quoted, e.g. \"{v}\".", Line(node, d));
        if (!IsIsoDate(v))
            err("date-format", $"'{name}' must be a YYYY-MM-DD date, got '{v}'.", Line(node, d));
    }

    private static void CheckEnum(string name, YamlNode node, FieldSpec spec, Doc d, Action<string, string, int?> err)
    {
        var v = Scalar(node);
        if (v is null)
        {
            err("enum", $"'{name}' must be a scalar.", Line(node, d));
            return;
        }

        if (spec.Values is not null && !spec.Values.Contains(v))
            err("enum", $"'{name}' value '{v}' is not one of: {string.Join(", ", spec.Values)}.", Line(node, d));
        if (v != v.ToLowerInvariant())
            err("enum-lowercase", $"'{name}' enum value '{v}' must be lowercase.", Line(node, d));
    }

    private static void CheckList(string name, YamlNode node, FieldSpec spec, Doc d, Action<string, string, int?> err,
        Action<string, string, int?> warn)
    {
        if (node is not YamlSequenceNode seq)
        {
            err("list", $"'{name}' must be a YAML sequence.", Line(node, d));
            return;
        }

        foreach (var item in seq.Children)
        {
            var v = Scalar(item);
            if (spec.Of == "id" && v is not null && !LooksLikeId(v))
                err("id-format", $"'{name}' entry '{v}' is not a valid id.", Line(item, d));
            CheckPattern(name, "entry", item, spec, d, err);
        }

        // Every list field in the taxonomy is a set — no field's sequence carries meaning — so
        // alphabetical is simply the order that scan-reads and the one order two authors will agree
        // on. Only the first pair out of order is reported; the rest are noise once the author
        // re-sorts the field.
        for (var i = 1; i < seq.Children.Count; i++)
        {
            if (Scalar(seq.Children[i - 1]) is not { } prev || Scalar(seq.Children[i]) is not { } cur) continue;
            if (Natural.Compare(prev, cur) <= 0) continue;
            warn("list-order", $"'{name}' is not in alphabetical order — '{cur}' should come before '{prev}'.",
                Line(seq.Children[i], d));
            break;
        }
    }

    // A field's declared `pattern:` — the schema's own regex, applied to whatever scalar carries the
    // value. `noun` distinguishes a scalar field's "value" from a list's "entry" in the message.
    private static void CheckPattern(string name, string noun, YamlNode node, FieldSpec spec, Doc d,
        Action<string, string, int?> err)
    {
        if (spec.PatternRegex is null) return;
        var v = Scalar(node);
        if (v is null) return;
        if (!spec.PatternRegex.IsMatch(v))
            err("field-pattern", $"'{name}' {noun} '{v}' does not match {spec.Pattern}.", Line(node, d));
    }

    private static void CheckKeyOrder(Doc d, TypeSchema t, Action<string, string, int?> err)
    {
        var pos = new Dictionary<string, int>();
        for (var i = 0; i < d.FrontKeys.Count; i++)
            if (!pos.ContainsKey(d.FrontKeys[i]))
                pos[d.FrontKeys[i]] = i;

        foreach (var (a, b) in t.KeyOrderEdges)
            if (pos.TryGetValue(a, out var pa) && pos.TryGetValue(b, out var pb) && pa > pb)
                err("key-order", $"'{a}' must appear before '{b}' in the frontmatter.", d.FrontStartLine);
    }

    private static void CheckId(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Action<string, string, int?> err)
    {
        if (!present.TryGetValue("id", out var idNode)) return;
        var id = Scalar(idNode);
        if (id is null) return;

        // A `literal` id is the whole id, declared by the schema — the single-document types, where
        // there is one document and so one name for it. There is no prefix to carry and no filename
        // discriminator to agree with, so this is the whole check.
        if (t.IdStyle == "literal")
        {
            if (!string.Equals(id, t.IdValue, StringComparison.Ordinal))
                err("id-format", $"id '{id}' must be '{t.IdValue}', the value the type declares.", Line(idNode, d));
            return;
        }

        var expectPrefix = t.IdPrefix + "-";
        if (!id.StartsWith(expectPrefix, StringComparison.Ordinal))
        {
            err("id-prefix", $"id '{id}' must start with '{expectPrefix}'.", Line(idNode, d));
            return;
        }

        var numPart = id[expectPrefix.Length..];
        if (t.IdStyle == "numbered")
        {
            var fileNum = FilenameNumber(d.Rel);
            if (numPart.Length != t.IdWidth || !numPart.All(char.IsDigit))
                err("id-format", $"id '{id}' must be '{expectPrefix}' followed by {t.IdWidth} digits.",
                    Line(idNode, d));
            else if (fileNum is not null && numPart != fileNum)
                err("id-matches-filename", $"id '{id}' number does not match filename number '{fileNum}'.",
                    Line(idNode, d));
        }
        else if (t.IdStyle == "mnemonic")
        {
            // The id carries the mnemonic upper-case (pol-VURM); the filename carries it lower-case
            // (vurm-…md), so the two are compared case-insensitively.
            var fileMnemonic = FilenameMnemonic(d.Rel, t.IdWidth);
            if (numPart.Length != t.IdWidth || !numPart.All(char.IsLetterOrDigit)
                                            || !char.IsLetter(numPart[0]) || numPart != numPart.ToUpperInvariant())
                err("id-format",
                    $"id '{id}' must be '{expectPrefix}' followed by {t.IdWidth} upper-case alphanumeric "
                    + "characters beginning with a letter.", Line(idNode, d));
            else if (fileMnemonic is not null
                     && !numPart.Equals(fileMnemonic, StringComparison.OrdinalIgnoreCase))
                err("id-matches-filename",
                    $"id '{id}' mnemonic does not match filename mnemonic '{fileMnemonic}'.", Line(idNode, d));
        }
    }

    private static void CheckFilename(Doc d, TypeSchema t, Action<string, string, int?> err)
    {
        var name = Path.GetFileName(d.Rel);
        if (t.FilenameRegex is not null && !t.FilenameRegex.IsMatch(name))
            err("filename-pattern", $"filename '{name}' does not match {t.FilenamePattern}.", null);
        var slug = name;
        if (slug.EndsWith(".md")) slug = slug[..^3];
        var dash = slug.IndexOf('-');
        if (dash >= 0)
        {
            var head = slug[..dash];
            var isIdPrefix = t.IdStyle switch
            {
                "numbered" => head.All(char.IsDigit),
                "mnemonic" => head.Length == t.IdWidth && head.All(char.IsLetterOrDigit),
                _ => false
            };
            if (isIdPrefix) slug = slug[(dash + 1)..];
        }
        if (slug.Length > t.SlugMax)
            err("slug-length", $"slug '{slug}' is {slug.Length} characters; the limit is {t.SlugMax}.", null);
    }

    // The H1 is plain descriptive text — no id, no prefix, no shape the schema constrains — so the only
    // thing left to check is that there is one. What used to be read out of the H1 is now the identity
    // line's job, and CheckIdentity depends on this having run: with no H1 there is no line beneath it,
    // and reporting both would be one fault counted twice.
    private static void CheckH1(Doc d, Action<string, string, int?> err)
    {
        if (d.H1 is null) err("h1", "document has no H1.", 1);
    }

    // The identity line — "`Policy: pol-A11Y` `DRAFT`" directly beneath the H1. It states, on the page,
    // the three things frontmatter already carries and an Azure DevOps reader may not scroll to see:
    // what kind of document this is, which one it is, and whether it is in force. Each half is checked
    // against the frontmatter separately, because "this says Standard" and "this says the wrong id" are
    // different mistakes with different fixes and a reader deserves to be told which they made.
    //
    // Every collection type carries one. A single-document type has no records to identify — its page
    // is the document — so it is skipped, on the shape the schema declares rather than on a folder
    // happening to be absent.
    private static void CheckIdentity(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Action<string, string, int?> err)
    {
        if (d.H1 is null || t.IsSingleDocument) return;

        var id = present.TryGetValue("id", out var idNode) ? Scalar(idNode) : null;
        var status = present.TryGetValue("status", out var statusNode) ? Scalar(statusNode) : null;
        var expected = Expected(t, id, status);

        if (d.IdentitySpans is null)
        {
            err("identity", $"no identity line follows the H1 — add {expected}.", d.H1Line);
            return;
        }

        // Two spans, the first of them "Type: id". Anything else is reported once, against the whole
        // line, rather than as a cascade of derived complaints about parts that were never there.
        var colon = d.IdentitySpans[0].IndexOf(':');
        if (d.IdentitySpans.Count != 2 || colon <= 0)
        {
            err("identity", $"identity line is malformed — write it as {expected}.", d.IdentityLine);
            return;
        }

        var gotType = d.IdentitySpans[0][..colon].Trim();
        var gotId = d.IdentitySpans[0][(colon + 1)..].Trim();
        var gotStatus = d.IdentitySpans[1].Trim();

        if (!string.Equals(gotType, t.DisplayName, StringComparison.Ordinal))
            err("identity-type", $"identity line says '{gotType}', but this is a {t.DisplayName}.", d.IdentityLine);

        // Compared against the frontmatter rather than the filename: the id is what every citation uses,
        // and id-matches-filename already ties the frontmatter back to the file. Where the frontmatter
        // is itself absent or malformed its own check has said so, and there is nothing to compare to.
        if (id is not null && !string.Equals(gotId, id, StringComparison.Ordinal))
            err("identity-id", $"identity line id '{gotId}' does not match the document's id '{id}'.",
                d.IdentityLine);

        // Status is lower-case in frontmatter and upper-case on the line — one value, written for a
        // machine in one place and for a reader in the other, so the comparison is case-insensitive
        // and the casing itself is what the message names when it is wrong.
        if (status is not null && !string.Equals(gotStatus, status, StringComparison.OrdinalIgnoreCase))
            err("identity-status",
                $"identity line status '{gotStatus}' does not match the document's status '{status}'.",
                d.IdentityLine);
        else if (status is not null && !string.Equals(gotStatus, status.ToUpperInvariant(), StringComparison.Ordinal))
            err("identity-status", $"identity line status '{gotStatus}' must be upper-case — "
                                   + $"`{status.ToUpperInvariant()}`.", d.IdentityLine);
    }

    // The clause table — the section where a policy actually binds. Every other section describes,
    // qualifies or explains; these rows are the obligations themselves, and each carries an id so a
    // standard, a control or a deviation can cite the one it answers rather than the whole document.
    //
    // Runs only for a type whose schema declares a `clauses:` block, and only once the section itself is
    // present: a missing section is `required-section`'s to report, and saying it twice would make one
    // fault look like two.
    private static void CheckClauses(Doc d, TypeSchema t, Action<string, string, int?> err,
        Action<string, string, int?> warn)
    {
        if (t.Clauses is not { } spec) return;
        if (!d.H2.Any(h => string.Equals(h, spec.Section, StringComparison.OrdinalIgnoreCase))) return;

        var headers = string.Join(" | ", spec.Columns);

        if (d.ClauseHeaders is null)
        {
            err("clause-table", $"the '## {spec.Section}' section holds no table — write one row per "
                                + $"obligation, headed '{headers}'.", d.ClauseSectionLine);
            return;
        }

        if (!d.ClauseHeaders.SequenceEqual(spec.Columns, StringComparer.Ordinal))
        {
            err("clause-table", $"the clause table is headed '{string.Join(" | ", d.ClauseHeaders)}' — "
                                + $"it must be headed '{headers}'.", d.ClauseTableLine);
            return;
        }

        if (d.Clauses.Count == 0)
        {
            err("clause-table", "the clause table has no rows — a policy that binds nothing binds nobody.",
                d.ClauseTableLine);
            return;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var highest = 0;
        var disordered = false;

        foreach (var row in d.Clauses)
        {
            CheckClauseId(row, spec, seen, err);

            // The modal is the binding level, so a row without one is a sentence rather than an
            // obligation and nothing below it can be judged either.
            var modal = spec.ModalsLongestFirst.FirstOrDefault(m => row.Text.StartsWith(m, StringComparison.Ordinal));
            if (modal is null)
            {
                err("clause-modal", $"clause '{Md.Snippet(row.Text)}' does not open with a modal — write one of "
                                    + $"{string.Join(", ", spec.Levels)}.", row.Line);
                continue;
            }

            // Bold carries the binding level visually, and the reader skimming a long table reads the
            // weight before the words. A binding modal that is not bold reads as advice; an advisory one
            // that is bold reads as an obligation. Both are the wrong document.
            var binds = spec.Binding.Contains(modal, StringComparer.Ordinal);
            if (binds && !string.Equals(row.BoldLead, modal, StringComparison.Ordinal))
                err("clause-modal", $"'{modal}' binds — write it bold, `**{modal}**`.", row.Line);
            else if (!binds && row.BoldLead is not null)
                err("clause-modal", $"'{modal}' does not bind — write it plain, not bold.", row.Line);

            // A second modal in the same row is two obligations sharing one id, so a citation of it can
            // only ever name half of what it means.
            var rest = row.Text[modal.Length..];
            if (spec.ModalsLongestFirst.FirstOrDefault(m => rest.Contains(m, StringComparison.Ordinal)) is { } second)
                warn("clause-compound", $"clause '{row.IdSpan ?? row.IdText}' carries a second '{second}' — "
                                        + "one obligation per clause, or the citation is ambiguous.", row.Line);

            // Reported once, against the first row that breaks the grouping: a table sorted wholly the
            // wrong way would otherwise report every row after the first.
            var rank = spec.Rank(modal);
            if (rank < highest && !disordered)
            {
                warn("clause-order", $"clause '{row.IdSpan ?? row.IdText}' is a '{modal}' but follows a "
                                     + $"'{spec.Levels[highest]}' — group the table "
                                     + $"{string.Join(", ", spec.Levels)}.", row.Line);
                disordered = true;
            }

            highest = Math.Max(highest, rank);
        }
    }

    private static void CheckClauseId(ClauseRow row, ClauseSpec spec, HashSet<string> seen,
        Action<string, string, int?> err)
    {
        // Written as a code span for the same reason the identity line's id is: it is a handle rather
        // than a word, and Md.PlainText cannot tell the two apart once the span is flattened.
        if (row.IdSpan is null)
        {
            err("clause-id-format", $"clause id '{Md.Snippet(row.IdText)}' is not a code span — write it as "
                                    + $"`{Md.Snippet(row.IdText)}`.", row.Line);
            return;
        }

        if (spec.IdPatternRegex is { } idPattern && !idPattern.IsMatch(row.IdSpan))
            err("clause-id-format", $"clause id '{row.IdSpan}' does not match {spec.IdPattern}.", row.Line);

        // Ordinal, because `pol-SCRT.LOGS` and `pol-SCRT.logs` differing only in case is not two clauses
        // a reader could tell apart either.
        if (!seen.Add(row.IdSpan))
            err("clause-id-unique", $"clause id '{row.IdSpan}' is used twice — a citation of it names "
                                    + "two obligations.", row.Line);
    }

    // The line the document should have carried, for a message that shows rather than describes.
    // Placeholders stand in for anything the frontmatter could not supply.
    private static string Expected(TypeSchema t, string? id, string? status) =>
        $"`{t.DisplayName}: {id ?? $"{t.IdPrefix}-…"}` `{status?.ToUpperInvariant() ?? "STATUS"}`";

    // The link half of a document's checks, on their own, for a page that is not a record. Every one
    // of them asks about prose rather than about frontmatter, so they are the checks that carry over
    // to a type page unchanged.
    public static void CheckPageLinks(Doc d, Schema schema, string repoRoot, List<Finding> f) =>
        CheckLinks(d, schema, repoRoot,
            (check, msg, line) => f.Add(new Finding(d.Rel, line, Sev.Error, check, msg)),
            (check, msg, line) => f.Add(new Finding(d.Rel, line, Sev.Warning, check, msg)));

    private static void CheckLinks(Doc d, Schema schema, string repoRoot, Action<string, string, int?> err,
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
            if (TryCanonicalId(inner, schema, out _))
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
            if (TryCanonicalId(link.Label, schema, out var canonical) && link.Label != canonical)
                err("label-canonical",
                    $"reference '[{link.Label}]' should be written as the id '{canonical}'.", link.Line);
        }

        foreach (var label in d.DefinedLabels.Distinct(StringComparer.Ordinal))
            if (TryCanonicalId(label, schema, out var canonical) && label != canonical)
                err("label-canonical",
                    $"link definition '[{label}]' should be written as the id '{canonical}'.", null);

        // unused definitions
        foreach (var label in d.DefinedLabels.Distinct(StringComparer.OrdinalIgnoreCase))
            if (!d.UsedLabels.Contains(label))
                warn("unused-definition", $"link definition '[{label}]' is never referenced.", null);
    }

    // A label is id-shaped when its prefix names a type and the remainder fits that type's id style.
    // The canonical form is the id exactly as the document carries it: the prefix always lower-case,
    // a mnemonic always upper-case, a slug always lower-case.
    private static bool TryCanonicalId(string label, Schema schema, out string canonical)
    {
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
                if (rest.Length != t.IdWidth || !rest.All(char.IsDigit)) return false;
                canonical = $"{t.IdPrefix}-{rest}";
                return true;
            case "mnemonic":
                if (rest.Length != t.IdWidth || !rest.All(char.IsLetterOrDigit) || !char.IsLetter(rest[0]))
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

    private static void CheckMirrorsSection(Doc d, TypeSchema t, Schema schema, Action<string, string, int?> err)
    {
        foreach (var name in t.FieldOrder)
        {
            var spec = t.Fields[name];
            if (spec.MirrorsSection is null || spec.Ref is null) continue;
            if (!schema.ByFolder.TryGetValue(spec.Ref, out var refType)) continue;

            var inFront = new HashSet<string>(FrontIdList(d, name), StringComparer.OrdinalIgnoreCase);
            var inSection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var link in d.RelatedSectionLinks)
            {
                var id = IdFromLink(link, refType);
                if (id is not null) inSection.Add(id);
            }

            foreach (var id in inFront.Except(inSection))
                err("related-matches-section",
                    $"'{name}' lists '{id}' but it is not referenced in the '## {spec.MirrorsSection}' section.",
                    d.FrontStartLine);
            foreach (var id in inSection.Except(inFront))
                err("related-matches-section",
                    $"the '## {spec.MirrorsSection}' section references '{id}' but '{name}' does not list it.",
                    d.FrontStartLine);
        }
    }

    // The type's own `rules:`, in the order the schema declares them. Two kinds arrive here: a rule
    // carrying an `expr:` is answered by evaluating it, and needs no C# at all; a rule whose question
    // needs a real algorithm is one of `DocumentRules`, looked up by id. SPEC.md draws the line between
    // them, and this loop is the whole of the dispatch either way.
    private static void CheckRules(Doc d, TypeSchema t, Action<string, string, int?> err,
        Action<string, string, int?> warn)
    {
        // Built once for the document and only where a rule actually asks something of it, so a type
        // with no expression rules measures nothing.
        Facts? facts = null;

        foreach (var rule in t.Rules)
        {
            if (rule.Compiled is { } compiled)
            {
                facts ??= new Facts(d);
                if (RuleExpr.Eval(compiled, facts)) continue;
                var report = rule.Severity == Sev.Error ? err : warn;
                report(rule.Id, rule.Message!, d.FrontStartLine);
                continue;
            }

            // A rule with neither an `expr:` nor an implementation is a statement of intent, and is
            // skipped in silence: the schema records what someone wanted, and nothing answers to it yet.
            if (DocumentRules.ByRuleId.TryGetValue(rule.Id, out var implementation))
                implementation.Check(new RuleContext(d, t, rule, err, warn));
        }
    }

    // -- small utilities --

    private static bool RequiredWhenHolds(RequiredWhen? condition, Dictionary<string, YamlNode> present)
        => condition is not null
           && present.TryGetValue(condition.Field, out var node)
           && condition.Holds(Scalar(node));

    private static bool IsAbsentValue(YamlNode node) =>
        node switch
        {
            YamlScalarNode sc => string.IsNullOrEmpty(sc.Value) || sc.Value is "~" or "null" or "Null" or "NULL",
            YamlSequenceNode seq => seq.Children.Count == 0,
            _ => false
        };

    private static bool IsBareKey(YamlNode node)
        => node is YamlScalarNode { Style: ScalarStyle.Plain } sc && string.IsNullOrEmpty(sc.Value);

    private static bool IsIsoDate(string v)
        => v.Length == 10 && v[4] == '-' && v[7] == '-'
           && v[..4].All(char.IsDigit) && v[5..7].All(char.IsDigit) && v[8..].All(char.IsDigit);

    private static bool LooksLikeId(string v) => v.Contains('-') && v == v.ToLowerInvariant();

    private static string? Scalar(YamlNode node) => (node as YamlScalarNode)?.Value;

    private static int? Line(YamlNode node, Doc d)
        => node.Start.Line > 0 ? (int)node.Start.Line + d.FrontStartLine - 1 : d.FrontStartLine;

    private static List<string> FrontIdList(Doc d, string key)
    {
        var result = new List<string>();
        if (d.Front is null) return result;
        foreach (var kv in d.Front.Children)
            if (((YamlScalarNode)kv.Key).Value == key)
            {
                if (kv.Value is YamlSequenceNode seq)
                    result.AddRange(seq.Children.Select(Scalar).OfType<string>());
                else if (Scalar(kv.Value) is { Length: > 0 } s) result.Add(s);
            }

        return result;
    }

    private static string? FilenameNumber(string rel)
    {
        var name = Path.GetFileName(rel);
        var i = 0;
        while (i < name.Length && char.IsDigit(name[i])) i++;
        return i > 0 ? name[..i] : null;
    }

    private static string? FilenameMnemonic(string rel, int width)
    {
        var name = Path.GetFileName(rel);
        var dash = name.IndexOf('-');
        if (dash != width) return null;
        var head = name[..dash];
        return head.All(char.IsLetterOrDigit) ? head : null;
    }

    private static string? IdFromLink(LinkRef link, TypeSchema refType)
    {
        // Resolve the link's target filename to the ref type's id, e.g. 0007-…md -> adr-0007,
        // or vurm-…md -> pol-VURM where the type is mnemonic.
        var target = link.Target;
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var file = target.Split('/').LastOrDefault() ?? "";
        if (refType.IdStyle == "mnemonic")
        {
            var mnemonic = FilenameMnemonic(file, refType.IdWidth);
            return mnemonic is null ? null : $"{refType.IdPrefix}-{mnemonic.ToUpperInvariant()}";
        }

        var i = 0;
        while (i < file.Length && char.IsDigit(file[i])) i++;
        return i == refType.IdWidth ? $"{refType.IdPrefix}-{file[..i]}" : null;
    }

    private static bool IsExternal(string t)
        => t.StartsWith("http://") || t.StartsWith("https://") || t.StartsWith("mailto:") || t.StartsWith("tel:");

    private static bool ResolveTarget(string repoRoot, string fromRel, string target)
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
