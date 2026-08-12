using System.Globalization;
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

        // The schema first, because it decides how every document below is read. Reported even when the
        // run is narrowed to given paths — unlike the corpus-shape checks, this is not a question about
        // the corpus but about the terms it was judged on, and those hold however few documents were
        // asked about.
        SchemaChecks.Check(schema, findings);

        foreach (var doc in corpus.Docs)
            CheckDocument(doc, schema, repoRoot, findings);

        // Every file `kac index` writes a block into, held to still carrying the markers to write between.
        // Driven from the list the generator writes from, so every file it writes is a file this visits: a
        // type's page and the framework's own pages are one question and get one answer.
        //
        // A file that is not on disk is skipped: its absence is type-setup's to report for a page, and no
        // fault at all for a framework document a corpus has not taken.
        foreach (var file in GeneratedFiles.Blocks(Corpus.Adopted(schema, repoRoot, corpus.Descriptor)))
        {
            if (!file.MarkersRequired) continue;
            if (corpus.Paths.Count > 0
                && !corpus.Paths.Any(p => file.Path == p.Replace('\\', '/').TrimEnd('/'))) continue;

            var full = Path.Combine(repoRoot, file.Path);
            if (!File.Exists(full)) continue;

            CheckGeneratedBlocks(file.Path, File.ReadAllText(full), file.Blocks, findings);
        }

        // A type's page is not a record — it carries no frontmatter and describes the documents
        // beneath it rather than being one — so the structural checks do not apply. What it does carry
        // is links, and it is the page every record links back to and every contributor reads first.
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(t.Page)) continue;
            if (corpus.Paths.Count > 0
                && !corpus.Paths.Any(p => t.Page == p.Replace('\\', '/').TrimEnd('/'))) continue;
            var full = Path.Combine(repoRoot, t.Page);
            if (!File.Exists(full)) continue; // absence is type-setup's to report, not this pass's

            var text = File.ReadAllText(full);
            var page = Doc.Parse(t.Page, text, schema, requireFrontmatter: false);
            if (page is null) continue;

            // A page is not a record, so frontmatter on one is a leftover. It comes from a type that
            // used to be a single document: the folder arrives in a sync and the old page survives
            // beside it, still holding the content. Nothing else says so, because a page is forked and
            // a forked file is never compared against upstream.
            if (page.FrontStartLine > 0)
                findings.Add(new Finding(t.Page, page.FrontStartLine, Sev.Error, "page-frontmatter",
                    "the page carries frontmatter — it describes the records beneath it and is not one, so it has "
                    + $"no id, tier or status of its own. Move what it holds into '{(string.IsNullOrEmpty(t.Folder) ? key : t.Folder)}/' "
                    + "as a record, and delete the block."));

            LinkChecks.CheckPage(page, schema, repoRoot, findings);
        }

        // The template each collection type carries. It is the one file in a type that every future
        // document is copied from, so a defect in it is not one document's problem but every
        // document's — and it is found by the next author rather than by whoever last edited it.
        //
        // It is checked here rather than discovered as a record, because it is not one: it holds no id,
        // claims no place in the index, and must not answer to id-unique or to a reciprocal edge. What
        // it is held to is everything a copy of it inherits.
        foreach (var rel in corpus.Templates)
        {
            var template = Doc.Parse(rel, File.ReadAllText(Path.Combine(repoRoot, rel)), schema);
            if (template is null)
                findings.Add(new Finding(rel, null, Sev.Error, "template-fields",
                    "the template carries no frontmatter — a document copied from it starts with none."));
            else
                CheckDocument(template, schema, repoRoot, findings, DocKind.Template);
        }

        // The framework's own documentation, held to the one rule that is about where it will be read
        // rather than about what it says. Skipped when the run is narrowed, like the other checks that
        // ask about the shape of the corpus rather than about a document.
        if (corpus.Paths.Count == 0)
            CheckFrameworkDocs(schema, repoRoot, corpus.Docs, findings);

        // Corpus-wide checks (uniqueness, reciprocity) need every doc in hand.
        CheckCorpus(corpus.Docs, findings);

        // How often a value recurs is a question about the whole of a type, so a narrowed run cannot
        // answer it: every value in one document is carried by one document. Skipped there, like the
        // other checks that ask about the corpus rather than about a record.
        if (corpus.Paths.Count == 0)
            CheckMinRecords(corpus.Docs, findings);

        // Whether each declared type is stood up. Skipped when the run is narrowed to given paths:
        // asking about one document is not asking about the shape of the corpus, and answering
        // anyway would bury the reply.
        if (corpus.Paths.Count == 0)
            CheckTypeSetup(schema, repoRoot, corpus.Files, corpus.Descriptor, findings);

        return findings;
    }

    public static void CheckDocument(Doc d, Schema schema, string repoRoot, List<Finding> f,
        DocKind kind = DocKind.Record)
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

        // -- the frontmatter's keys --
        // A record is asked whether every key it carries is known; whether the required ones are filled
        // in is a separate question below. A template is asked both at once and answers for the
        // documents copied from it rather than for itself, so the two live together in one check.
        if (kind == DocKind.Template) CheckTemplateFields(d, t, Err);
        else
            foreach (var k in d.FrontKeys)
                if (!t.KnownKeys.Contains(k))
                    Err("unknown-key", $"unknown frontmatter key '{k}'.", d.FrontStartLine);

        // -- key order --
        // The actual order must be a topological extension of the chains the schema declares: every
        // pair it orders must hold, and genuinely unconstrained pairs are free. See
        // TypeSchema.DeriveKeyOrderEdges for why the constraint is a pair set rather than a total order.
        CheckKeyOrder(d, t, Err);

        // -- required fields (universal + type), incl. required-when --
        // Not asked of a template. Every value in one is either bare or a placeholder, both of which
        // say "not supplied yet" — which is the whole point of the file. That the fields are all there
        // to be supplied is CheckTemplateFields' question above.
        if (kind == DocKind.Record)
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

            // A placeholder opening a value is a flow mapping to YAML, not text — `{` is an indicator
            // in that one position, so `review-by: {{date}}` parses as a mapping and arrives here with
            // nothing readable in it. Reported with the fix rather than left to the value checks, which
            // would say the date is malformed and quote an empty string back at whoever wrote it.
            if (kind == DocKind.Template && node is YamlMappingNode)
            {
                Err("template-fields",
                    $"'{name}' is read as a YAML mapping rather than a value — a placeholder that opens "
                    + "one has to be quoted: " + name + ": \"{{…}}\".", Line(node, d));
                continue;
            }

            // A placeholder is not a value: `{{slug}}` is the instruction to supply one. Read as absent
            // in a template — the field's own checks would otherwise report the mark as a malformed
            // date, an unknown enum value or an id of the wrong shape, which is three ways of saying
            // the file has not been filled in.
            if (kind == DocKind.Template && HasPlaceholder(node)) continue;

            // absent values must be bare keys, never null / ~ / "" / —
            if (IsAbsentValue(node))
            {
                if (!IsBareKey(node))
                    Err("bare-key",
                        $"'{name}' is absent but not a bare key — use '{name}:' with no value (not null, ~, \"\", or —).",
                        Line(node, d));
                continue;
            }

            // A word the field admits beside its declared type — `last-rehearsed: "never"`. It is taken
            // as written and nothing further is asked of it, which is the whole of what `allow-literal`
            // means. A list is not short-circuited here: there the literal is one entry among ids, and
            // CheckList exempts that entry rather than the field.
            if (spec.Type != "list" && spec.IsLiteral(Scalar(node))) continue;

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

        // -- id: prefix, shape, agreement with the filename --
        // -- filename pattern + slug length --
        // Neither is asked of a template. It has no id — `svc-{{slug}}` is the instruction to allocate
        // one — and `_template.md` is a reserved name that no type's filename pattern matches or should.
        // What the identity line below still asks is that the template agrees with itself.
        if (kind == DocKind.Record)
        {
            if (present.TryGetValue("id", out var idNode) && Scalar(idNode) is { } id)
                IdChecks.Check(id, Line(idNode, d), d.Rel, t, Err);

            IdChecks.CheckFilename(d.Rel, t, Err);
        }

        // -- H1 present --
        CheckH1(d, Err);

        // -- identity line beneath the H1 agrees with the frontmatter --
        CheckIdentity(d, t, present, Err);

        // -- required sections --
        foreach (var sec in t.RequiredSections)
            if (!d.Sections.Any(s => string.Equals(s.Title, sec, StringComparison.OrdinalIgnoreCase)))
                Err("required-section", $"missing required section '## {sec}'.");

        // -- a section that is nothing but its heading --
        // `required-section` is answered by a heading existing, and an author with nothing to say cannot
        // delete a required one, so what they leave is a blank beneath it — which reads as a finished
        // document to everyone but the person who needed the section. An optional section reaches the
        // same state from the other side, emptied during a cleanup with the heading left behind, and the
        // remedy differs enough to be worth two wordings.
        //
        // A section the schema never declared is the author's own. A template's stand empty for the copy
        // to fill. The clause section is left to `clause-table`, which is looking at the same blank and
        // can say what belongs there.
        if (kind == DocKind.Record)
            foreach (var s in d.Sections.Where(s => !Md.HasContent(d.Text.AsSpan()[s.BodyStart..s.BodyEnd])))
            {
                if (t.Clauses is { } clauses
                    && string.Equals(s.Title, clauses.Section, StringComparison.OrdinalIgnoreCase)) continue;

                if (t.RequiredSections.Contains(s.Title, StringComparer.OrdinalIgnoreCase))
                    Err("empty-section", $"required section '## {s.Title}' has nothing under it.", s.Line);
                else if (t.OptionalSections.Contains(s.Title, StringComparer.OrdinalIgnoreCase))
                    Err("empty-section",
                        $"section '## {s.Title}' has nothing under it — write it or delete the heading.", s.Line);
            }

        // -- placeholders left from the template --
        // The other half of the convention: `{{…}}` means "supply this", so a record still carrying one
        // is a copy nobody finished. Easy to do and easy to miss — the file has an id, a title and every
        // section, so every other check passes and the document reads as complete until someone follows
        // a link to `{{a}}.md`. Reported once, naming the first: an unfinished copy holds a dozen, and
        // eleven more findings say nothing the first did not.
        if (kind == DocKind.Record) CheckPlaceholders(d, Err);

        // -- clause table shape, ids and modals --
        // A template's clause rows are a demonstration of the shape, with `{{ID}}` where the id goes, so
        // they are neither unique nor citable and are not asked to be.
        if (kind == DocKind.Record) ClauseChecks.Check(d, t, Err, Warn);

        // -- links resolve --
        LinkChecks.Check(d, schema, repoRoot, Err, Warn, kind);

        // -- related mirrors ## Related --
        // A reconciliation between two halves of the same document, both of which are examples in a
        // template — it would hold the file to agreeing with itself about documents that do not exist.
        if (kind == DocKind.Record) CheckMirrorsSection(d, t, schema, Err);

        // -- the type's own rules --
        // Every one of them judges a filled-in document: whether the prose has outgrown the links,
        // whether a step hedges, whether a control names its evidence. A template answers none of those
        // questions, and its guidance prose would answer several of them wrongly. This is also the one
        // open-ended set — a type may declare a rule tomorrow — so a template is exempt from the
        // category rather than from the rules that happen to exist today.
        if (kind == DocKind.Record) CheckRules(d, t, Err, Warn);
        return;

        void Warn(string check, string msg, int? line = null) =>
            f.Add(new Finding(d.Rel, line, Sev.Warning, check, msg));

        void Err(string check, string msg, int? line = null) => f.Add(new Finding(d.Rel, line, Sev.Error, check, msg));
    }

    // The markers a generated block lives between. `Generator.SpliceBlock` looks for the pair and
    // returns the text untouched when either is missing, so a file that loses one silently stops
    // being generated into — and `index --check` agrees it is fresh, because what the generator would
    // write is exactly what is already there. Nothing else can notice, which is why this is a check
    // on the markers rather than on the content between them.
    //
    // Asked of every file `GeneratedFiles.Blocks` names, and of every block it names there: a block whose
    // markers have both gone is the same fault as one that lost a single marker, and is the quieter of
    // the two. `README.md` is the exception, and answers for itself — see `BlockFile.MarkersRequired`.
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
    // The generated index is deliberately not checked here — `index --check` already reports it
    // missing or stale, and one fault should not be reported by two commands.
    //
    // A folder counts as present when it holds tracked files. An empty directory git has never seen
    // is not part of the corpus, so the answer is the same in a fresh clone as on the machine that
    // happened to create it.
    public static void CheckTypeSetup(Schema schema, string repoRoot, IEnumerable<string> corpusFiles,
        CorpusDescriptor descriptor, List<Finding> f)
    {
        CheckAdoption(schema, repoRoot, descriptor, f);

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
            if (!File.Exists(Path.Combine(repoRoot, folder, Artefact.Template)))
                missing.Add($"{folder}/{Artefact.Template}");
            if (missing.Count > 0)
                f.Add(new Finding(at, null, Sev.Error, "type-setup",
                    $"type '{key}' has a '{folder}/' folder but is not fully set up — add {string.Join(", ", missing)}."));
        }
    }

    // What the corpus says it has adopted, held against the schema it took and the folders it built.
    //
    // A corpus that declares no `types:` is not asked any of this: adoption is read off its folders
    // instead, so every question below answers itself. Declaring is what turns "these are the folders that
    // happen to be here" into "these are the types we chose", and only the second can be wrong.
    private static void CheckAdoption(Schema schema, string repoRoot, CorpusDescriptor descriptor, List<Finding> f)
    {
        if (descriptor.Types is not { } declared) return;

        const string at = ".corpus.yaml";

        foreach (var name in declared.Where(n => !schema.ByFolder.ContainsKey(n)))
            f.Add(new Finding(at, null, Sev.Error, "type-setup",
                $"'{name}' is adopted here and no schema covers it — either '.schema/{name}.yaml' has not been synced "
                + "from upstream, or the name is wrong."));

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var stoodUp = Corpus.StoodUp(t, repoRoot);
            var adopted = declared.Contains(key, StringComparer.Ordinal);

            // Declared and not built is the state a sync exists to resolve, so it is reported as work
            // outstanding rather than as a contradiction.
            if (adopted && !stoodUp)
                f.Add(new Finding(at, null, Sev.Error, "type-setup",
                    $"type '{key}' is adopted here and is not stood up — add {t.Page} and its folder, or drop it from "
                    + "'types:' if it was not wanted."));

            // Built and not declared is the other way round, and is how a corpus drifts back to inferring:
            // the pages would leave the type out while the corpus plainly holds it.
            if (!adopted && stoodUp)
                f.Add(new Finding(at, null, Sev.Error, "type-setup",
                    $"type '{key}' is stood up here and is not in 'types:' — every generated list leaves it out while "
                    + "the corpus holds it. Adopt it, or delete what was built."));
        }
    }

    // The documents describing the framework itself, wherever a corpus keeps them. The framework's own
    // glossary is one of them, and the only one that is also a record: it is filed under a type and
    // validated like any other. Being shared byte-for-byte is what brings it here as well.
    private static readonly string[] FrameworkDocs =
        ["knowledge-as-code.md", "knowledge-as-code/", "glossary/knowledge-as-code.md"];

    // The framework's own documentation is shared byte-for-byte by every corpus running it, so it has to
    // read correctly in a corpus that adopted three types and in one that adopted seventeen. A link to a
    // type page cannot: it either resolves or is a dead end, depending on a decision the page cannot see.
    //
    // So a framework document names a type and never links to one. Where a link is genuinely wanted, the
    // list it belongs in is generated from the types the corpus stood up, and a generated block is exempt
    // for exactly that reason — it is written against this corpus rather than against the framework.
    //
    // Checked here rather than left to `link-resolves`, which would report it only downstream: every type
    // page exists in the corpus that writes these documents, so the defect is invisible precisely where it
    // can be fixed.
    private static void CheckFrameworkDocs(Schema schema, string repoRoot, IEnumerable<Doc> docs,
        List<Finding> f)
    {
        // The ones already validated as records. They have had the link pass, so giving them a second
        // would report every dead link twice.
        var checkedAsRecords = new HashSet<string>(docs.Select(d => d.Rel), StringComparer.OrdinalIgnoreCase);

        var pages = schema.ByFolder.Values
            .Where(t => !string.IsNullOrEmpty(t.Page))
            .ToDictionary(t => "/" + t.Page[..^".md".Length], t => t.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var rel in FrameworkFiles(repoRoot))
        {
            // Read with the generated blocks emptied. Everything below is a question about what a person
            // wrote, and a generated block answers to `index --check` instead — it is regenerated from this
            // corpus, so its links are this corpus's and are right by construction.
            var doc = Doc.Parse(rel, Generator.Authored(Files.ReadLf(Path.Combine(repoRoot, rel))),
                schema, requireFrontmatter: false);
            if (doc is null) continue;

            // The ordinary link pass, which the documents excluded from discovery have never had: the
            // page pass only visits type pages, so a dead link in one reached the wiki silently and was
            // found by a reader. A framework document that is also a record has had it already.
            if (!checkedAsRecords.Contains(rel)) LinkChecks.CheckPage(doc, schema, repoRoot, f);

            foreach (var link in doc.Links)
            {
                var target = link.Target.Split('#')[0].TrimEnd('/');
                if (target.Length == 0 || LinkChecks.IsExternal(target)) continue;

                // Azure DevOps resolves a page with or without its extension, so both forms are the same
                // link and both are caught. Dropping the extension here lets one lookup answer for `/adrs`
                // and `/adrs.md` alike.
                if (target.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) target = target[..^3];

                var slash = target.LastIndexOf('/');
                var page = slash > 0 ? target[..slash] : target;
                if (!pages.TryGetValue(page, out var type)) continue;

                // A path *into* a type's folder names a record rather than the type, which is worse: every
                // corpus is told to delete the records it inherits, so that link is dead even where the
                // type was adopted.
                f.Add(new Finding(rel, link.Line, Sev.Error, "framework-names-types", slash > 0
                    ? $"'{link.Target}' links to a record in '{type}' from a document every corpus shares. Those "
                      + "records are the first thing a corpus deletes, so the link dies even where the type is used."
                    : $"'{link.Target}' links to the '{type}' type from a document every corpus shares. Name the type "
                      + "instead: a corpus that has not adopted it reads a dead link, and one that has is no worse "
                      + "off."));
            }
        }
    }

    private static IEnumerable<string> FrameworkFiles(string repoRoot)
    {
        foreach (var entry in FrameworkDocs)
        {
            var full = Path.Combine(repoRoot, entry);

            if (!entry.EndsWith('/'))
            {
                if (File.Exists(full)) yield return entry;
                continue;
            }

            if (!Directory.Exists(full)) continue;
            foreach (var path in Directory.EnumerateFiles(full, "*.md").Order(StringComparer.Ordinal))
                yield return entry + Path.GetFileName(path);
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

        // Referenced ids — every field the schema gives a `ref:`. The declaration names the type an id
        // in this field belongs to, which reads to whoever holds these files as a target the tool
        // answers for. Asked of every ref field alike, reciprocal or not: a one-directional edge —
        // `depends-on`, the estate's own dependency graph — has no counterpart obliged to keep it in
        // step, which makes it the edge with least behind it rather than the one to leave alone.
        //
        // Only the target's existence is asked. A literal the field admits is not an id and is skipped,
        // as it is everywhere else.
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Refs.Count == 0) continue;
                if (spec.Type != "id" && (spec.Type != "list" || spec.Of != "id")) continue;
                foreach (var targetId in FrontList(d, name))
                {
                    if (spec.IsLiteral(targetId) || byId.ContainsKey(targetId)) continue;
                    f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "ref-resolves",
                        $"'{name}' points at '{targetId}', which does not exist."));
                }
            }
        }

        // reciprocal fields (e.g. supersedes / superseded-by). Whether the target exists is `ref-resolves`
        // above; what is left here is the one question this field asks, which is whether the document at
        // the other end points back.
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Reciprocal is null || spec.Refs.Count == 0) continue;
                foreach (var targetId in FrontList(d, name))
                {
                    if (!byId.TryGetValue(targetId, out var target)) continue;

                    var back = FrontList(target, spec.Reciprocal);
                    var selfId = d.FrontScalar("id");
                    if (!back.Any(b => string.Equals(b, selfId, StringComparison.OrdinalIgnoreCase)))
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "reciprocal",
                            $"'{name}: {targetId}' is not reciprocated — {target.Rel} must list '{spec.Reciprocal}: {selfId}'."));
                }
            }
        }
    }

    // Where a type declares `min-records:` on a list field, each value in that field is counted across the
    // records of the type, and one carried by fewer records than the floor is reported against every record
    // carrying it.
    //
    // A warning, and permanently one. The corpus decides what its vocabulary is — the schema only says
    // that a value in this field is for dividing the type into groups — and a value newly introduced is
    // below the floor on the day it is written, on the way to being above it.
    private static void CheckMinRecords(List<Doc> docs, List<Finding> f)
    {
        foreach (var group in docs.Where(d => d.Type is not null).GroupBy(d => d.Type!))
        {
            var type = group.Key;
            foreach (var name in type.FieldOrder)
            {
                if (type.Fields[name].MinRecords is not { } floor) continue;

                // Counted case-insensitively, matching how a reader searches, so `Public` and `public`
                // are one value falling short rather than two. Counted once per record as well: a record
                // listing a value twice is still one record carrying it, which is what the floor asks
                // about.
                var carried = group
                    .Select(d => (Doc: d,
                        Values: FrontList(d, name).Distinct(StringComparer.OrdinalIgnoreCase).ToList()))
                    .ToList();
                var count = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var value in carried.SelectMany(c => c.Values))
                    count[value] = count.GetValueOrDefault(value) + 1;

                foreach (var (doc, values) in carried)
                foreach (var value in values.Where(v => count[v] < floor).Order(StringComparer.Ordinal))
                {
                    var carriers = Count(count[value], $"{type.TypeName} record", $"{type.TypeName} records");
                    f.Add(new Finding(doc.Rel, doc.FrontStartLine, Sev.Warning, "min-records",
                        $"'{name}: {value}' is carried by {carriers} — the schema asks for at least {floor}, "
                        + "because a value here is meant to group records. One that does not belongs in a field "
                        + "that is free to be unique."));
                }
            }
        }
    }

    // -- helpers for individual checks --

    // The template's frontmatter against the type's fields, in both directions. Read as one question —
    // would a document copied from this file pass its own frontmatter checks? — because the two answers
    // have the same cause, a schema that moved and a template that did not, and the same fix.
    //
    // The bar is what a copy would fail on, not what the type declares. A template is curated: it
    // offers the fields a document of the type will usually fill in, and leaving out an optional one is
    // an editorial choice rather than drift — the ADR template offers `decided-on` and not `deciders`,
    // and no ADR is the worse for it. A required field is the opposite: absent from the template, every
    // copy fails `required-field` on a line its author never wrote. `required-when` is not asked, for
    // the same reason the value checks are not — the field it depends on is a placeholder or bare, so
    // the condition has nothing to read.
    //
    // A reserved key is admitted but never expected: `title` belongs to the publishing platform, and a
    // template has no reason to teach it.
    private static void CheckTemplateFields(Doc d, TypeSchema t, Action<string, string, int?> err)
    {
        foreach (var k in d.FrontKeys.Where(k => !t.KnownKeys.Contains(k)))
            err("template-fields",
                $"'{k}' is not a field of the '{t.TypeName}' type — every document copied from this "
                + "template would fail unknown-key.", d.FrontStartLine);

        var carried = new HashSet<string>(d.FrontKeys, StringComparer.Ordinal);
        foreach (var spec in t.DeclaredFields.Where(spec => spec.Required && !carried.Contains(spec.Name)))
            err("template-fields",
                $"the template does not carry '{spec.Name}', which is required — every document copied "
                + "from it would fail required-field.", d.FrontStartLine);
    }

    private static void CheckPlaceholders(Doc d, Action<string, string, int?> err)
    {
        var left = Placeholder.Occurrences(d).ToList();
        if (left.Count == 0) return;

        var (token, line) = left[0];
        var rest = left.Count > 1 ? $" There are {left.Count - 1} more in this document." : "";
        err("placeholder-left",
            $"'{token}' is a placeholder the template left for you to fill in.{rest}", line);
    }

    // Whether a value carries the placeholder mark anywhere — the scalar itself, or any entry of a
    // sequence. Asked of the whole field rather than of each entry, because a list in a template is a
    // demonstration of the field's shape and `[ svc-{{a}}, svc-real ]` is not a state worth modelling.
    private static bool HasPlaceholder(YamlNode node) =>
        node switch
        {
            YamlScalarNode sc => Placeholder.In(sc.Value),
            YamlSequenceNode seq => seq.Children.Any(HasPlaceholder),
            _ => false
        };

    // Shape then calendar, under one id: both answers leave the author with the same thing to do, and the
    // message is what tells them which they wrote — `2026/06/12` is not written as a date at all, where
    // `2026-13-40` is written as one and names a day that has never existed.
    private static void CheckDate(string name, YamlNode node, Doc d, Action<string, string, int?> err)
    {
        var sc = node as YamlScalarNode;
        var v = sc?.Value ?? "";
        var quoted = sc?.Style is ScalarStyle.DoubleQuoted or ScalarStyle.SingleQuoted;
        if (!quoted)
            err("date-quoted", $"'{name}' date must be quoted, e.g. \"{v}\".", Line(node, d));

        if (!IsIsoShape(v))
            err("date-format", $"'{name}' must be a YYYY-MM-DD date, got '{v}'.", Line(node, d));
        else if (!DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            err("date-format", $"'{name}' is not a date on the calendar, got '{v}'.", Line(node, d));
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

        // A floor the schema sets on a field whose value is its breadth. Reported before the entries are
        // read, because an author told both that the list is short and that one of its entries is
        // malformed will fix the second and re-run to find the first.
        if (spec.MinItems is { } min && seq.Children.Count < min)
            err("min-items",
                $"'{name}' has {Count(seq.Children.Count, "entry", "entries")} — the schema asks for at least {min}.",
                Line(node, d));

        foreach (var item in seq.Children)
        {
            var v = Scalar(item);
            if (spec.IsLiteral(v)) continue; // a word the field admits beside its ids — `applies-to: [all]`
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

    // The H1 is plain descriptive text — no id, no prefix, no shape the schema constrains — so the only
    // thing left to check is that there is one. The type, the id and the status are the identity line's
    // to carry, and CheckIdentity depends on this having run: with no H1 there is no line beneath it,
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
    private static void CheckIdentity(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Action<string, string, int?> err)
    {
        if (d.H1 is null) return;

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

    // The line the document should have carried, for a message that shows rather than describes.
    // Placeholders stand in for anything the frontmatter could not supply.
    private static string Expected(TypeSchema t, string? id, string? status) =>
        $"`{t.DisplayName}: {id ?? $"{t.IdPrefix}-…"}` `{status?.ToUpperInvariant() ?? "STATUS"}`";

    private static void CheckMirrorsSection(Doc d, TypeSchema t, Schema schema, Action<string, string, int?> err)
    {
        foreach (var spec in t.DeclaredFields)
        {
            if (spec.MirrorsSection is not { } section) continue;
            var refTypes = spec.Refs.Select(schema.ByFolder.GetValueOrDefault).OfType<TypeSchema>().ToList();
            if (refTypes.Count == 0) continue;

            var inFront = new HashSet<string>(FrontList(d, spec.Name), StringComparer.OrdinalIgnoreCase);
            var inSection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var link in d.MirroredSectionLinks.GetValueOrDefault(section, []))
            {
                // A field may point at several types, and a link cites whichever of them owns the folder
                // it lands in. At most one can, so the first answer is the answer.
                var id = refTypes.Select(rt => IdChecks.IdFromLink(link, d.Rel, rt))
                    .FirstOrDefault(x => x is not null);
                if (id is not null) inSection.Add(id);
            }

            foreach (var id in inFront.Except(inSection))
                err("related-matches-section",
                    $"'{spec.Name}' lists '{id}' but it is not referenced in the '## {section}' section.",
                    d.FrontStartLine);
            foreach (var id in inSection.Except(inFront))
                err("related-matches-section",
                    $"the '## {section}' section references '{id}' but '{spec.Name}' does not list it.",
                    d.FrontStartLine);
        }
    }

    // The type's own `rules:`, in the order the schema declares them. Two kinds arrive here: a rule
    // carrying an `expr:` is answered by evaluating it, and needs no C# at all; a rule whose question
    // needs a real algorithm is one of `DocumentRules`, looked up by id. `CLAUDE.md` beside this project
    // draws the line between them, and this loop is the whole of the dispatch either way.
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

    // Written as a date, which is a question about the characters. Whether those characters name a day is
    // `DateOnly`'s to answer, and asking it here as well would be two tests of the same string with one
    // able to disagree with the other.
    private static bool IsIsoShape(string v)
        => v.Length == 10 && v[4] == '-' && v[7] == '-'
           && v[..4].All(char.IsDigit) && v[5..7].All(char.IsDigit) && v[8..].All(char.IsDigit);

    private static bool LooksLikeId(string v) => v.Contains('-') && v == v.ToLowerInvariant();

    private static string Count(int n, string one, string many) => $"{n} {(n == 1 ? one : many)}";

    private static string? Scalar(YamlNode node) => (node as YamlScalarNode)?.Value;

    private static int? Line(YamlNode node, Doc d)
        => node.Start.Line > 0 ? (int)node.Start.Line + d.FrontStartLine - 1 : d.FrontStartLine;

    private static List<string> FrontList(Doc d, string key)
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
}
