using YamlDotNet.RepresentationModel;

namespace kac.core;

public static class Validator
{
    // Everything the validator has to say about a loaded corpus, in the order a reader would ask it:
    // each record on its own, then the pages that are not records, then the questions that need every
    // record in hand, then whether the corpus has the shape its schema declares.
    //
    // This is the whole of `validate`: the command around it only chooses how to print the result. Any
    // caller wanting to know what the tool thinks of a corpus calls this, so no caller can end up running
    // a subset and believing it ran the lot.
    public static List<Finding> CheckAll(LoadedCorpus corpus)
    {
        var (schema, tree) = (corpus.Schema, corpus.Tree);
        var findings = new List<Finding>();

        // The schema first, because it decides how every document below is read.
        SchemaChecks.Check(schema, findings);

        foreach (var doc in corpus.Docs)
            CheckDocument(doc, schema, tree, findings);

        // Every file `kac generate` writes a block into, held to still carrying the markers to write between.
        // Driven from the list the generator writes from, so every file that gets a block is a file this
        // visits: a type's page and the framework's own pages are one question and get one answer.
        //
        // A file the corpus does not hold is skipped: its absence is type-setup's to report for a page, and
        // no fault at all for a framework document a corpus has not taken.
        foreach (var file in GeneratedFiles.Blocks(corpus.Adopted))
        {
            if (!file.MarkersRequired) continue;
            if (!tree.Exists(file.Path)) continue;

            CheckGeneratedBlocks(file.Path, tree.Read(file.Path), file.Blocks, findings);
        }

        // A type's page is not a record. It carries no frontmatter, and it describes the documents beneath
        // it, so the structural checks do not apply. What it does carry is links, and it is the page every
        // record links back to and every contributor reads first.
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(t.Page)) continue;
            if (!tree.Exists(t.Page)) continue; // absence is type-setup's to report, not this pass's

            var page = Doc.Parse(t.Page, tree.Read(t.Page), schema, requireFrontmatter: false);
            if (page is null) continue;

            // A page is forked and a forked file is never compared against upstream, so nothing else
            // reports frontmatter left on one by a type that became a folder.
            if (page.FrontStartLine > 0)
                findings.Add(new Finding(t.Page, page.FrontStartLine, Sev.Error, new CheckId("page-frontmatter"),
                    "the page carries frontmatter: it describes the records beneath it and is not one, so it has "
                    + $"no id, tier or status of its own. Move what it holds into '{(string.IsNullOrEmpty(t.Folder) ? key : t.Folder)}/' "
                    + "as a record, and delete the block."));

            LinkChecks.Check(page, schema, tree, new Report(page.Rel, findings));
        }

        // The template each collection type carries. It is the one file in a type that every future
        // document is copied from, so a defect in it is not one document's problem but every document's.
        // The defect is found by the next author rather than by whoever last edited it.
        //
        // It is checked here rather than discovered as a record, because it is not one: it holds no id,
        // claims no place in the index, and must not answer to id-unique or to a reciprocal edge. What
        // it is held to is everything a copy of it inherits.
        foreach (var rel in corpus.Templates)
        {
            var template = Doc.Parse(rel, tree.Read(rel), schema);
            if (template is null)
                findings.Add(new Finding(rel, null, Sev.Error, new CheckId("template-fields"),
                    "the template carries no frontmatter: a document copied from it starts with none."));
            else
                CheckDocument(template, schema, tree, findings, DocKind.Template);
        }

        // The framework's own documentation. It takes the ordinary link pass, which discovery never gave
        // it, and one rule of its own about where the page will be read rather than about what it says.
        CheckFrameworkDocs(schema, tree, corpus.Docs, findings);

        // Uniqueness and reciprocity need every document in hand. The index they build is handed on rather
        // than built again: it is the corpus's one account of which id names which document, and a second
        // one would be free to answer differently.
        var byId = CheckCorpus(schema, corpus.Docs, findings);

        CheckCorpusRules(schema, corpus.Docs, byId, tree, findings);
        CheckMinRecords(corpus.Docs, findings);
        CheckTypeSetup(schema, tree, corpus.Descriptor, findings);
        CheckShortcode(schema, corpus.Descriptor, findings);

        return findings;
    }

    public static void CheckDocument(Doc d, Schema schema, Tree tree, List<Finding> f,
        DocKind kind = DocKind.Record)
    {
        var report = new Report(d.Rel, f);

        if (d.Type is null)
        {
            report.Err(new CheckId("type"), $"folder '{d.Folder}' has no schema.");
            return;
        }

        var t = d.Type;

        if (d.Front is null)
        {
            report.Err(new CheckId("frontmatter-parses"), "frontmatter is not a valid YAML mapping.");
            return;
        }

        var present = new Dictionary<string, YamlNode>();
        foreach (var kv in d.Front.Children)
            present[((YamlScalarNode)kv.Key).Value ?? ""] = kv.Value;

        // The frontmatter's keys. A record is asked whether every key it carries is known. Whether the
        // required ones are filled in is a separate question below. A template is asked both at once and
        // answers for the documents copied from it rather than for itself, so the two live together in
        // one check.
        if (kind == DocKind.Template) CheckTemplateFields(d, t, report);
        else
            foreach (var k in d.FrontKeys)
                if (!t.KnownKeys.Contains(k))
                    report.Err(new CheckId("unknown-key"), $"unknown frontmatter key '{k}'.", d.FrontStartLine);

        // The key order must be a topological extension of the chains the schema declares: every pair it
        // orders must hold, and genuinely unconstrained pairs are free. See
        // TypeSchema.DeriveKeyOrderEdges for why the constraint is a pair set rather than a total order.
        CheckKeyOrder(d, t, report);

        // The required fields, universal and type alike, including the ones `required-when` turns on. Not
        // asked of a template. Every value in one is either bare or a placeholder, and both say "not
        // supplied yet". That is the whole point of the file. Whether the fields are all there to be
        // supplied is CheckTemplateFields' question above.
        if (kind == DocKind.Record)
            foreach (var spec in t.DeclaredFields)
            {
                var req = spec.Required || RequiredWhenHolds(spec.RequiredWhenCondition, present);
                var absent = !present.ContainsKey(spec.Name) || ValueChecks.IsAbsent(present[spec.Name]);
                if (req && absent)
                {
                    var why = spec.Required ? "" : $" (required when {spec.RequiredWhen})";
                    report.Err(new CheckId("required-field"),
                        $"missing required field '{spec.Name}'{why}.", d.FrontStartLine);
                }
            }

        // The per-field value checks. What a value is held to is the field's declaration and nothing about
        // the document around it, so the whole of it lives in `ValueChecks` and is testable from a
        // `FieldSpec` and a string.
        foreach (var (name, node) in present)
        {
            var spec = schema.EffectiveField(t, name);
            if (spec is null) continue; // unknown key already reported

            ValueChecks.Check(name, node, spec, kind, d.FrontStartLine, report);
        }

        if (present.TryGetValue("tier", out var tierNode) && Yaml.Raw(tierNode) is { } tier && tier != t.Tier)
            report.Err(new CheckId("tier-matches-type"),
                $"tier '{tier}' does not match the '{t.TypeName}' type tier '{t.Tier}'.",
                Yaml.LineOf(tierNode, d.FrontStartLine));

        // The id, the filename, and the agreement between them. None of it is asked of a template. A
        // template has no id: `svc-{{slug}}` is the instruction to allocate one. `_template.md` is also a
        // reserved name that no type's filename pattern matches or should. What the identity line below
        // still asks is that the template agrees with itself.
        if (kind == DocKind.Record)
        {
            if (present.TryGetValue("id", out var idNode) && Yaml.Raw(idNode) is { } id)
                IdChecks.Check(id, Yaml.LineOf(idNode, d.FrontStartLine), d.Rel, t, report);

            IdChecks.CheckFilename(d.Rel, t, report);
        }

        CheckH1(d, report);
        CheckIdentity(d, t, present, report);

        foreach (var sec in t.RequiredSections)
            if (!d.Sections.Any(s => string.Equals(s.Title, sec, StringComparison.OrdinalIgnoreCase)))
                report.Err(new CheckId("required-section"), $"missing required section '## {sec}'.");

        // A section that is nothing but its heading. `required-section` is answered by a heading existing,
        // and an author with nothing to say cannot delete a required one, so what they leave is a blank
        // beneath it. That blank reads as a finished document to everyone but the person who needed the
        // section. An optional section reaches the same state from the other side, emptied during a
        // cleanup with the heading left behind, and the remedy differs enough to be worth two wordings.
        //
        // A section the schema never declared is the author's own. A template's stand empty for the copy
        // to fill. The section a type keeps its parts in is left to the checks that read them, which are
        // looking at the same blank and can say what belongs there.
        if (kind == DocKind.Record)
            foreach (var s in d.Sections.Where(s => !Md.HasContent(d.Text.AsSpan()[s.BodyStart..s.BodyEnd])))
            {
                if (t.Parts is { } parts
                    && string.Equals(s.Title, parts.Section, StringComparison.OrdinalIgnoreCase)) continue;

                if (t.RequiredSections.Contains(s.Title, StringComparer.OrdinalIgnoreCase))
                    report.Err(new CheckId("empty-section"),
                        $"required section '## {s.Title}' has nothing under it.", s.Line);
                else if (t.OptionalSections.Contains(s.Title, StringComparer.OrdinalIgnoreCase))
                    report.Err(new CheckId("empty-section"),
                        $"section '## {s.Title}' has nothing under it. Write it or delete the heading.", s.Line);
            }

        // The other half of the template convention: `{{…}}` means "supply this", so a record still
        // carrying one is a copy nobody finished. Easy to do and easy to miss: the file has an id, a title
        // and every section, so every other check passes and the document reads as complete until someone
        // follows a link to `{{a}}.md`. Reported once, naming the first, because an unfinished copy holds
        // a dozen and eleven more findings say nothing the first did not.
        if (kind == DocKind.Record) CheckPlaceholders(d, report);

        // The clause table's shape, its ids and its modals. A template's clause rows are a demonstration
        // of the shape, with `{{ID}}` where the id goes, so they are neither unique nor citable and are
        // not asked to be.
        if (kind == DocKind.Record) PartChecks.Check(d, t, report);

        // The notation a citation is written in, which any document may get wrong and a template may
        // demonstrate wrongly for every record copied from it.
        PartChecks.CheckNotation(d, report);

        LinkChecks.Check(d, schema, tree, report, kind);

        // `related:` mirrors `## Related`. This reconciles two halves of the same document, and both
        // halves are examples in a template, so asking it of a template would hold the file to agreeing
        // with itself about documents that do not exist.
        if (kind == DocKind.Record) CheckMirrorsSection(d, t, schema, report);

        // The type's own rules. Every one of them judges a filled-in document: whether the prose has
        // outgrown the links, whether a step hedges, whether a control names its evidence. A template
        // answers none of those questions, and its guidance prose would answer several of them wrongly.
        // This is also the one open-ended set: a type may declare a rule tomorrow. So a template is exempt
        // from the category rather than from the rules that happen to exist today.
        if (kind == DocKind.Record) CheckRules(d, t, report);
    }

    // The markers a generated block lives between. `Generator.SpliceBlock` looks for the pair and returns
    // the text untouched when either is missing, so a file that loses one silently stops being generated
    // into. `generate --check` then agrees it is fresh, because what the generator would write is exactly
    // what is already there. Nothing else can notice, which is why this is a check on the markers rather
    // than on the content between them.
    //
    // Asked of every file `GeneratedFiles.Blocks` names, and of every block it names there: a block whose
    // markers have both gone is the same fault as one that lost a single marker, and is the quieter of
    // the two. `README.md` is the exception and answers for itself. See `BlockFile.MarkersRequired`.
    public static void CheckGeneratedBlocks(string rel, string text, IEnumerable<string> names,
        List<Finding> f)
    {
        foreach (var name in names)
        {
            var begin = text.IndexOf(Generator.Begin(name), StringComparison.Ordinal);
            var end = text.IndexOf(Generator.End(name), StringComparison.Ordinal);
            if (begin < 0 || end < 0)
                f.Add(new Finding(rel, null, Sev.Error, new CheckId("generated-block"),
                    $"the '{name}' block is missing its "
                    + (begin < 0 && end < 0 ? "markers" : begin < 0 ? "BEGIN marker" : "END marker")
                    + ": `kac generate` writes between them and leaves the page alone without both."));
            else if (end < begin)
                f.Add(new Finding(rel, null, Sev.Error, new CheckId("generated-block"),
                    $"the '{name}' block's END marker comes before its BEGIN marker."));
        }
    }

    // Whether each type the schema declares is stood up, and stood up completely.
    //
    // A schema file says the tool manages this type. It does not say the corpus has built it yet. A corpus
    // adopting the framework a type at a time holds the whole schema and grows into it, so a declared type
    // with nothing behind it is a valid, silent state: nothing links to it, nothing generates for it, and
    // no contributor can reach it.
    //
    // What is not valid is half of one. A folder is the signal that the type has been stood up, and from
    // that point everything the type needs must be there: the page a reader arrives on, and the template a
    // contributor copies. A page without a folder is the same fault from the other side. The generated
    // index is deliberately not checked here, because `generate --check` already reports it missing or
    // stale and one fault should not be reported by two commands.
    //
    // A folder counts as present when it holds tracked files. An empty directory git has never seen is not
    // part of the corpus, so the answer is the same in a fresh clone as on the machine that happened to
    // create it.
    private static void CheckTypeSetup(Schema schema, Tree tree, CorpusDescriptor descriptor, List<Finding> f)
    {
        CheckAdoption(schema, tree, descriptor, f);

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var at = $".schema/{key}.yaml";
            var pageExists = !string.IsNullOrEmpty(t.Page) && tree.Exists(t.Page);

            var folder = string.IsNullOrEmpty(t.Folder) ? key : t.Folder;
            if (!tree.HasFolder(folder))
            {
                if (pageExists)
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                        $"type '{key}' has {t.Page} but no '{folder}/': a type is set up as both or neither."));
                continue;
            }

            var missing = new List<string>();
            if (!pageExists) missing.Add(string.IsNullOrEmpty(t.Page) ? $"{key}.md" : t.Page);

            // The template is asked for with `OnDisk`, as `Corpus.DiscoverTemplates` asks for it. `Tree`
            // says why that is the right question for this one file.
            var template = $"{folder}/{Artefact.Template}";
            if (!tree.OnDisk(template)) missing.Add(template);
            if (missing.Count > 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                    $"type '{key}' has a '{folder}/' folder but is not fully set up. Add {string.Join(", ", missing)}."));
        }
    }

    // What the corpus says it has adopted, held against the schema it took and the folders it built.
    //
    // A corpus that declares no `types:` is not asked any of this: adoption is read off its folders
    // instead, so every question below answers itself. Declaring is what turns "these are the folders that
    // happen to be here" into "these are the types we chose", and only the second can be wrong.
    private static void CheckAdoption(Schema schema, Tree tree, CorpusDescriptor descriptor, List<Finding> f)
    {
        if (descriptor.Types is not { } declared) return;

        const string at = ".corpus.yaml";

        foreach (var name in declared.Where(n => !schema.ByFolder.ContainsKey(n)))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                $"'{name}' is adopted here and no schema covers it: either '.schema/{name}.yaml' has not been synced "
                + "from upstream, or the name is wrong."));

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var stoodUp = Corpus.StoodUp(t, tree);
            var adopted = declared.Contains(key, StringComparer.Ordinal);

            // Declared and not built is the state a sync exists to resolve, so it is reported as work
            // outstanding rather than as a contradiction.
            if (adopted && !stoodUp)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                    $"type '{key}' is adopted here and is not stood up. Add {t.Page} and its folder, or drop it from "
                    + "'types:' if it was not wanted."));

            // Built and not declared is the other way round, and is how a corpus drifts back to inferring:
            // the pages would leave the type out while the corpus plainly holds it.
            if (!adopted && stoodUp)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                    $"type '{key}' is stood up here and is not in 'types:'. Every generated list leaves it out while "
                    + "the corpus holds it. Adopt it, or delete what was built."));
        }
    }

    // The shorthand another corpus cites this one by, held to a spelling a citation can carry.
    // `.schema/_checks.yaml` argues each part of that spelling under `shortcode`.
    //
    // A corpus declaring none is silent. It is cited by nothing, and the value is immutable once written,
    // so there is nothing to be gained by holding a corpus to a value it has no use for yet.
    //
    // Both faults are reported at once, because correcting one leaves the other standing. That is what
    // the prefix comparison ignores case for: `STD` is misspelled and is still the standards prefix, and
    // an author told only about the casing would fix it and meet the second refusal on the next run.
    private static void CheckShortcode(Schema schema, CorpusDescriptor descriptor, List<Finding> f)
    {
        if (descriptor.Shortcode is not { } shortcode) return;

        const string at = ".corpus.yaml";

        if (Misspelled(shortcode) is { } fault)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("shortcode"),
                $"shortcode '{shortcode}' {fault}. A shortcode is "
                + $"{CorpusDescriptor.ShortcodeMin} to {CorpusDescriptor.ShortcodeMax} characters, opens on a "
                + "lower-case letter, and carries lower-case letters and digits after it."));

        // Asked of the schema rather than of a list here, so adopting a type shuts its prefix off and
        // declining one leaves it free. A type declaring no prefix takes no spelling with it, which
        // `Schema.IdPrefixes` skips for the same reason.
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            if (t.IdPrefix.Length > 0 && string.Equals(shortcode, t.IdPrefix, StringComparison.OrdinalIgnoreCase))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("shortcode"),
                    $"shortcode '{shortcode}' is the id prefix of '{key}'. A citation opening '{t.IdPrefix}:' "
                    + "reads as that type rather than as this corpus. Pick a shorthand no type has taken."));
    }

    // How a shortcode is spelled wrong, or null where it is spelled correctly. The wording completes
    // "shortcode 'x' ...", and names the first fault alone: an author fixing it re-runs the check.
    private static string? Misspelled(string shortcode)
    {
        if (shortcode.Length < CorpusDescriptor.ShortcodeMin) return "is too short";
        if (shortcode.Length > CorpusDescriptor.ShortcodeMax) return "is too long";
        if (!char.IsAsciiLetterLower(shortcode[0])) return "does not open on a lower-case letter";

        return shortcode.Any(c => !char.IsAsciiLetterLower(c) && !char.IsAsciiDigit(c))
            ? "carries something other than a lower-case letter or a digit"
            : null;
    }

    // The documents describing the framework itself, wherever a corpus keeps them, as globs the listing
    // answers. See `Tree.Match`. The framework's own glossary is one of them, and the only one that is
    // also a record: it is filed under a type and validated like any other. Being shared byte-for-byte is
    // what brings it here as well.
    private static readonly string[] FrameworkDocs =
        ["knowledge-as-code.md", "knowledge-as-code/*.md", "glossary/knowledge-as-code.md"];

    // The framework's own documentation is shared byte-for-byte by every corpus running it, so it has to
    // read correctly in a corpus that adopted three types and in one that adopted seventeen. A link to a
    // type page cannot: it either resolves or is a dead end, depending on a decision the page cannot see.
    //
    // So a framework document names a type and never links to one. Where a link is genuinely wanted, the
    // list it belongs in is generated from the types the corpus stood up. A generated block is exempt for
    // exactly that reason: it is written against this corpus rather than against the framework.
    //
    // Checked here rather than left to `link-resolves`, which would report it only downstream: every type
    // page exists in the corpus that writes these documents, so the defect is invisible precisely where it
    // can be fixed.
    private static void CheckFrameworkDocs(Schema schema, Tree tree, IEnumerable<Doc> docs, List<Finding> f)
    {
        // The ones already validated as records. They have had the link pass, so giving them a second
        // would report every dead link twice.
        var checkedAsRecords = new HashSet<string>(docs.Select(d => d.Rel), StringComparer.OrdinalIgnoreCase);

        var pages = schema.ByFolder.Values
            .Where(t => !string.IsNullOrEmpty(t.Page))
            .ToDictionary(t => "/" + t.Page[..^".md".Length], t => t.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var rel in FrameworkDocs.SelectMany(tree.Match))
        {
            // Read with the generated blocks emptied. Everything below is a question about what a person
            // wrote, and a generated block answers to `generate --check` instead: it is regenerated from
            // this corpus, so its links are this corpus's and are right by construction.
            var doc = Doc.Parse(rel, Generator.Authored(tree.Read(rel)),
                schema, requireFrontmatter: false);
            if (doc is null) continue;

            // The ordinary link pass, which the documents excluded from discovery have never had: the
            // page pass only visits type pages, so a dead link in one reached the wiki silently and was
            // found by a reader. A framework document that is also a record has had it already.
            if (!checkedAsRecords.Contains(rel)) LinkChecks.Check(doc, schema, tree, new Report(doc.Rel, f));

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
                f.Add(new Finding(rel, link.Line, Sev.Error, new CheckId("framework-names-types"), slash > 0
                    ? $"'{link.Target}' links to a record in '{type}' from a document every corpus shares. Those "
                      + "records are the first thing a corpus deletes, so the link dies even where the type is used."
                    : $"'{link.Target}' links to the '{type}' type from a document every corpus shares. Name the type "
                      + "instead: a corpus that has not adopted it reads a dead link, and one that has is no worse "
                      + "off."));
            }
        }
    }

    // The questions that need every record in hand, and the index of them that answering leaves behind:
    // which document each id names, first writer winning. That index is exactly what a corpus rule asks
    // for next.
    public static Dictionary<string, Doc> CheckCorpus(Schema schema, List<Doc> docs, List<Finding> f)
    {
        var byId = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in docs)
        {
            var id = d.FrontScalar("id");
            if (id is null) continue;
            if (byId.TryGetValue(id, out var other))
                f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, new CheckId("id-unique"),
                    $"id '{id}' is also used by {other.Rel}."));
            else
                byId[id] = d;
        }

        // Part citations: `pol-VURM.TIMEBOX`, `gls-knowledge-as-code.corpus`. The whole point of giving a
        // part an address is that something else can name it, and a citation nothing answers to is the
        // failure that machinery exists to prevent: it resolves to a document that plainly exists, so a
        // reader has no reason to doubt the half after the dot.
        //
        // The half before the dot decides the words. A citation into a type that keeps no parts at all
        // is a different mistake from one naming a part that type does not have, and saying so saves the
        // author looking for a heading the document was never going to carry.
        foreach (var d in docs)
        foreach (var (citation, line) in d.PartRefs)
        {
            var dot = citation.IndexOf('.');
            var (docId, partId) = (citation[..dot], citation[(dot + 1)..]);

            if (!byId.TryGetValue(docId, out var target))
            {
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"),
                    $"'{citation}' cites '{docId}', which does not exist."));
                continue;
            }

            var noun = target.Type?.Parts?.Noun;
            if (noun is null)
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"),
                    $"'{citation}' addresses a part of {target.Rel}, and its type offers none. Cite the "
                    + $"document as '{docId}'."));
            else if (!target.Parts.Any(p => string.Equals(p.Id, partId, StringComparison.Ordinal)))
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"),
                    $"'{citation}' cites a {noun} '{partId}' that {target.Rel} does not carry."));
        }

        // Referenced ids: every field the schema gives a `ref:`. The declaration names the type an id in
        // this field belongs to, which reads to whoever holds these files as a target the tool answers
        // for. So both halves of it are asked: that the id names a document, and that the document is of a
        // type the declaration admits. Asked of every ref field alike, reciprocal or not: a one-directional
        // edge (`depends-on`, the estate's own dependency graph) has no counterpart obliged to keep it
        // in step. That makes it the edge with least behind it rather than the one to leave alone.
        //
        // The wrong type is the quieter of the two faults. A dangling id is visibly broken to anyone who
        // follows it. One of the wrong type lands on a real page, so it reads as intentional. Whatever
        // walks the edge afterwards (`no-dependency-cycles`, for one) takes it at its word. A literal
        // the field admits is not an id and is skipped, as it is everywhere else.
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Refs.Count == 0) continue;
                if (spec.Type != "id" && (spec.Type != "list" || spec.Of != "id")) continue;

                var admitted = Admitted(spec, schema);
                foreach (var targetId in d.FrontList(name))
                {
                    if (spec.IsLiteral(targetId)) continue;
                    if (!byId.TryGetValue(targetId, out var target))
                    {
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, new CheckId("ref-resolves"),
                            $"'{name}' points at '{targetId}', which does not exist."));
                        continue;
                    }

                    if (Admits(admitted, target)) continue;
                    f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, new CheckId("ref-resolves"),
                        $"'{name}' points at '{targetId}', which is {WithArticle(target.Type!)}, "
                        + $"not {OneOf(admitted)}."));
                }
            }
        }

        // The reciprocal fields, `supersedes` and `superseded-by` among them. Whether the target exists,
        // and whether it is a document this field may point at, are `ref-resolves`'s above. What is left
        // here is the one question this field asks: whether the document at the other end points back.
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Reciprocal is null || spec.Refs.Count == 0) continue;
                var admitted = Admitted(spec, schema);
                foreach (var targetId in d.FrontList(name))
                {
                    if (!byId.TryGetValue(targetId, out var target)) continue;

                    // A target of the wrong type carries no counterpart field to answer with, so asking
                    // would report one fault twice: once as the wrong type, and once as a silence that
                    // is nothing but its consequence.
                    if (!Admits(admitted, target)) continue;

                    var back = target.FrontList(spec.Reciprocal);
                    var selfId = d.FrontScalar("id");
                    if (!back.Any(b => string.Equals(b, selfId, StringComparison.OrdinalIgnoreCase)))
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, new CheckId("reciprocal"),
                            $"'{name}: {targetId}' is not reciprocated: {target.Rel} must list '{spec.Reciprocal}: {selfId}'."));
                }
            }
        }

        return byId;
    }

    // The types a ref field may point at, as the schema's own objects. A document's type is resolved the
    // same way, so what a target is held against is one object compared with the same object: a type
    // answers to the name of its schema file and to the folder it declares, and neither name has to be
    // the one the other side wrote.
    private static List<TypeSchema> Admitted(FieldSpec spec, Schema schema) =>
        [.. spec.Refs.Select(schema.ByFolder.GetValueOrDefault).OfType<TypeSchema>()];

    // Whether a field may point at the document its id landed on. Where the question cannot be put the
    // answer is yes, which leaves the target to the checks that can: a `ref:` at a folder no schema
    // covers is `schema-dispatch`'s to report, and a document in a folder no type covers is `type`'s.
    private static bool Admits(List<TypeSchema> admitted, Doc target) =>
        admitted.Count == 0 || target.Type is null || admitted.Contains(target.Type);

    // The types named as a reader would say them aloud ("a Service", "an FAQ or a Standard") in the
    // order the declaration lists them. That is the order whoever wrote it chose.
    private static string OneOf(List<TypeSchema> types)
    {
        var names = types.Select(WithArticle).ToList();
        return names.Count switch
        {
            1 => names[0],
            2 => $"{names[0]} or {names[1]}",
            _ => $"{string.Join(", ", names[..^1])}, or {names[^1]}"
        };
    }

    // The type's `rules:` again, for the rules that read the corpus rather than a document. Driven from
    // the schema rather than from the types the records happen to cover, because a rule belongs to its
    // type whether or not the corpus has stood that type up. A rule reporting on an empty set is the
    // rule's answer to give, not the dispatcher's to withhold.
    //
    // A finding names the document the rule chose, so a corpus rule is handed the document to report
    // against where a document rule is handed the `Report` for the one it is reading. Everything else is
    // the same dispatch: found by the id the schema declares, and silent where nothing implements it.
    private static void CheckCorpusRules(Schema schema, List<Doc> docs, Dictionary<string, Doc> byId,
        Tree tree, List<Finding> f)
    {
        foreach (var (_, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        foreach (var rule in t.Rules)
            if (CorpusRules.ByRuleId.TryGetValue(rule.Id, out var implementation))
                implementation.Check(new CorpusRuleContext(docs, byId, tree, t, rule,
                    (at, c, m, l) => Report(Sev.Error, at, c, m, l),
                    (at, c, m, l) => Report(Sev.Warning, at, c, m, l)));
        return;

        void Report(Sev severity, Doc at, CheckId check, string message, int? line)
            => f.Add(new Finding(at.Rel, line, severity, check, message));
    }

    // Where a type declares `min-records:` on a list field, each value in that field is counted across the
    // records of the type, and one carried by fewer records than the floor is reported against every record
    // carrying it.
    //
    // A warning, and permanently one. The corpus decides what its vocabulary is, and the schema only says
    // that a value in this field is for dividing the type into groups. A value newly introduced is below
    // the floor on the day it is written, on the way to being above it.
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
                        Values: d.FrontList(name).Distinct(StringComparer.OrdinalIgnoreCase).ToList()))
                    .ToList();
                var count = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var value in carried.SelectMany(c => c.Values))
                    count[value] = count.GetValueOrDefault(value) + 1;

                foreach (var (doc, values) in carried)
                foreach (var value in values.Where(v => count[v] < floor).Order(StringComparer.Ordinal))
                {
                    var carriers = Count(count[value], $"{type.TypeName} record", $"{type.TypeName} records");
                    f.Add(new Finding(doc.Rel, doc.FrontStartLine, Sev.Warning, new CheckId("min-records"),
                        $"'{name}: {value}' is carried by {carriers}: the schema asks for at least {floor}, "
                        + "because a value here is meant to group records. One that does not belongs in a field "
                        + "that is free to be unique."));
                }
            }
        }
    }

    // The template's frontmatter against the type's fields, in both directions. Read as one question:
    // would a document copied from this file pass its own frontmatter checks? It is one question because
    // the two answers have the same cause, a schema that moved and a template that did not, and the same
    // fix.
    //
    // The bar is what a copy would fail on, not what the type declares. A template is curated: it offers
    // the fields a document of the type will usually fill in, and leaving out an optional one is an
    // editorial choice rather than drift. The ADR template offers `decided-on` and not `deciders`, and no
    // ADR is the worse for it. A required field is the opposite: absent from the template, every copy
    // fails `required-field` on a line its author never wrote. `required-when` is not asked, for the same
    // reason the value checks are not: the field it depends on is a placeholder or bare, so the condition
    // has nothing to read.
    //
    // A reserved key is admitted but never expected: `title` belongs to the publishing platform, and a
    // template has no reason to teach it.
    private static void CheckTemplateFields(Doc d, TypeSchema t, Report report)
    {
        foreach (var k in d.FrontKeys.Where(k => !t.KnownKeys.Contains(k)))
            report.Err(new CheckId("template-fields"),
                $"'{k}' is not a field of the '{t.TypeName}' type: every document copied from this "
                + "template would fail unknown-key.", d.FrontStartLine);

        var carried = new HashSet<string>(d.FrontKeys, StringComparer.Ordinal);
        foreach (var spec in t.DeclaredFields.Where(spec => spec.Required && !carried.Contains(spec.Name)))
            report.Err(new CheckId("template-fields"),
                $"the template does not carry '{spec.Name}', which is required: every document copied "
                + "from it would fail required-field.", d.FrontStartLine);
    }

    private static void CheckPlaceholders(Doc d, Report report)
    {
        var left = Placeholder.Occurrences(d).ToList();
        if (left.Count == 0) return;

        var (token, line) = left[0];
        var rest = left.Count > 1 ? $" There are {left.Count - 1} more in this document." : "";
        report.Err(new CheckId("placeholder-left"),
            $"'{token}' is a placeholder the template left for you to fill in.{rest}", line);
    }

    private static void CheckKeyOrder(Doc d, TypeSchema t, Report report)
    {
        var pos = new Dictionary<string, int>();
        for (var i = 0; i < d.FrontKeys.Count; i++)
            if (!pos.ContainsKey(d.FrontKeys[i]))
                pos[d.FrontKeys[i]] = i;

        foreach (var (a, b) in t.KeyOrderEdges)
            if (pos.TryGetValue(a, out var pa) && pos.TryGetValue(b, out var pb) && pa > pb)
                report.Err(new CheckId("key-order"),
                    $"'{a}' must appear before '{b}' in the frontmatter.", d.FrontStartLine);
    }

    // The H1 is plain descriptive text: no id, no prefix, no shape the schema constrains. So the only
    // thing left to check is that there is one. The type, the id and the status are the identity line's
    // to carry, and CheckIdentity depends on this having run: with no H1 there is no line beneath it,
    // and reporting both would be one fault counted twice.
    private static void CheckH1(Doc d, Report report)
    {
        if (d.H1 is null) report.Err(new CheckId("h1"), "document has no H1.", 1);
    }

    // The identity line, "`Policy: pol-A11Y` `DRAFT`" directly beneath the H1. It states, on the page, the
    // three things frontmatter already carries and an Azure DevOps reader may not scroll to see: what kind
    // of document this is, which one it is, and whether it is in force. Each half is checked against the
    // frontmatter separately, because "this says Standard" and "this says the wrong id" are different
    // mistakes with different fixes. A reader deserves to be told which they made.
    private static void CheckIdentity(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Report report)
    {
        if (d.H1 is null) return;

        var id = present.TryGetValue("id", out var idNode) ? Yaml.Raw(idNode) : null;
        var status = present.TryGetValue("status", out var statusNode) ? Yaml.Raw(statusNode) : null;
        var expected = Expected(t, id, status);

        if (d.IdentitySpans is null)
        {
            report.Err(new CheckId("identity"), $"no identity line follows the H1. Add {expected}.", d.H1Line);
            return;
        }

        // Two spans, the first of them "Type: id". Anything else is reported once, against the whole
        // line, rather than as a cascade of derived complaints about parts that were never there.
        var colon = d.IdentitySpans[0].IndexOf(':');
        if (d.IdentitySpans.Count != 2 || colon <= 0)
        {
            report.Err(new CheckId("identity"),
                $"identity line is malformed. Write it as {expected}.", d.IdentityLine);
            return;
        }

        var gotType = d.IdentitySpans[0][..colon].Trim();
        var gotId = d.IdentitySpans[0][(colon + 1)..].Trim();
        var gotStatus = d.IdentitySpans[1].Trim();

        if (!string.Equals(gotType, t.DisplayName, StringComparison.Ordinal))
            report.Err(new CheckId("identity-type"),
                $"identity line says '{gotType}', but this is {WithArticle(t)}.", d.IdentityLine);

        // Compared against the frontmatter rather than the filename: the id is what every citation uses,
        // and id-matches-filename already ties the frontmatter back to the file. Where the frontmatter
        // is itself absent or malformed its own check has said so, and there is nothing to compare to.
        if (id is not null && !string.Equals(gotId, id, StringComparison.Ordinal))
            report.Err(new CheckId("identity-id"),
                $"identity line id '{gotId}' does not match the document's id '{id}'.",
                d.IdentityLine);

        // Status is lower-case in frontmatter and upper-case on the line: one value, written for a machine
        // in one place and for a reader in the other. So the comparison is case-insensitive, and the
        // casing itself is what the message names when it is wrong.
        if (status is not null && !string.Equals(gotStatus, status, StringComparison.OrdinalIgnoreCase))
            report.Err(new CheckId("identity-status"),
                $"identity line status '{gotStatus}' does not match the document's status '{status}'.",
                d.IdentityLine);
        else if (status is not null && !string.Equals(gotStatus, status.ToUpperInvariant(), StringComparison.Ordinal))
            report.Err(new CheckId("identity-status"), $"identity line status '{gotStatus}' must be upper-case: "
                                                       + $"`{status.ToUpperInvariant()}`.", d.IdentityLine);
    }

    // The line the document should have carried, for a message that shows rather than describes.
    // Placeholders stand in for anything the frontmatter could not supply.
    private static string Expected(TypeSchema t, string? id, string? status) =>
        $"`{t.DisplayName}: {id ?? $"{t.IdPrefix}-…"}` `{status?.ToUpperInvariant() ?? "STATUS"}`";

    private static void CheckMirrorsSection(Doc d, TypeSchema t, Schema schema, Report report)
    {
        foreach (var spec in t.DeclaredFields)
        {
            if (spec.MirrorsSection is not { } section) continue;
            var refTypes = spec.Refs.Select(schema.ByFolder.GetValueOrDefault).OfType<TypeSchema>().ToList();
            if (refTypes.Count == 0) continue;

            var inFront = new HashSet<string>(d.FrontList(spec.Name), StringComparer.OrdinalIgnoreCase);
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
                report.Err(new CheckId("related-matches-section"),
                    $"'{spec.Name}' lists '{id}' but it is not referenced in the '## {section}' section.",
                    d.FrontStartLine);
            foreach (var id in inSection.Except(inFront))
                report.Err(new CheckId("related-matches-section"),
                    $"the '## {section}' section references '{id}' but '{spec.Name}' does not list it.",
                    d.FrontStartLine);
        }
    }

    // The type's own `rules:`, in the order the schema declares them. Two kinds arrive here. A rule
    // carrying an `expr:` is answered by evaluating it, and needs no C# at all. A rule whose question
    // needs a real algorithm is one of `DocumentRules`, looked up by id. `CLAUDE.md` beside this project
    // draws the line between them, and this loop is the whole of the dispatch either way.
    private static void CheckRules(Doc d, TypeSchema t, Report report)
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
                // An expression rule reports under its own rule id, which is the one place the two ids are
                // deliberately the same string. Written out so that sameness is a decision rather than a
                // type the compiler let through.
                var reported = new CheckId(rule.Id.Value);
                if (rule.Severity == Sev.Error) report.Err(reported, rule.Message!, d.FrontStartLine);
                else report.Warn(reported, rule.Message!, d.FrontStartLine);
                continue;
            }

            // A rule with neither an `expr:` nor an implementation is a statement of intent, and is
            // skipped in silence: the schema records what someone wanted, and nothing answers to it yet.
            if (DocumentRules.ByRuleId.TryGetValue(rule.Id, out var implementation))
                implementation.Check(new RuleContext(d, t, rule, report));
        }
    }

    private static bool RequiredWhenHolds(RequiredWhen? condition, Dictionary<string, YamlNode> present)
        => condition is not null
           && present.TryGetValue(condition.Field, out var node)
           && condition.Holds(Yaml.Raw(node));

    private static string Count(int n, string one, string many) => $"{n} {(n == 1 ? one : many)}";

    // One record of a type, named in a sentence: "an ADR", "a Service", "Data". Read from the two labels
    // the schema declares, so every message that names a type reads as English whatever a corpus calls
    // its own.
    //
    // A type whose singular and plural are the same word is a mass noun and takes no article, as in "this
    // is Data". The schema has already said so in declaring `label:` and `label-plural:` alike. Otherwise
    // the article follows how the label is read aloud rather than how it is spelled: a label in capitals
    // is read letter by letter, so it takes what the name of its first letter wants: "an ADR", "an NFR".
    // The letters read with an opening vowel are the whole of that exception.
    private static string WithArticle(TypeSchema t)
    {
        var name = t.DisplayName;
        if (name.Length == 0) return name;
        if (!string.IsNullOrEmpty(t.LabelPlural) && string.Equals(t.Label, t.LabelPlural, StringComparison.Ordinal))
            return name;

        var vowel = name.All(char.IsAsciiLetterUpper)
            ? "AEFHILMNORSX".Contains(name[0])
            : "AEIOU".Contains(char.ToUpperInvariant(name[0]));
        return vowel ? $"an {name}" : $"a {name}";
    }
}
