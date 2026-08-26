namespace kac.core;

// A rule whose question no single document can answer. A cycle in a dependency graph, a term nothing
// defines, a store no service claims: each is a property of the set, and none of them is visible from
// inside a member of it.
//
// Separate from `IDocumentRule` rather than a wider context on it, so that what a rule may read is
// legible from the interface it implements. The dispatcher runs each from its own pass, and
// `../../CLAUDE.md` holds the test for which one a new rule wants.
public interface ICorpusRule
{
    // The id in the type schema's `rules:` block, and what the rule reports under: the same pair
    // `IDocumentRule` declares, for the same reasons, so the catalogue is assembled from the rules
    // rather than restated beside them.
    RuleId RuleId { get; }

    IReadOnlyList<CheckId> Emits { get; }

    void Check(CorpusRuleContext ctx);
}

// Everything a corpus rule is given. `Docs` is every record the corpus holds and `ById` is that same
// set indexed as the corpus checks already index it, so a rule resolving an id resolves it exactly as
// `ref-resolves` did rather than building a second index free to disagree.
//
// Reporting takes the document to report against, which is the difference from `RuleContext`: a rule
// spanning documents chooses which of them a finding belongs to, and it is rarely the whole set.
//
// `Tree` is the corpus's files, for the rule whose question is answered by a page no record links into
// the graph. A framework register is that case: it carries no frontmatter and no id, so it is absent
// from `Docs` while being the only place the corpus says what a framework standing is.
public sealed record CorpusRuleContext(
    IReadOnlyList<Doc> Docs,
    IReadOnlyDictionary<string, Doc> ById,
    Tree Tree,
    TypeSchema Type,
    RuleSpec Spec,
    Action<Doc, CheckId, string, int?> Err,
    Action<Doc, CheckId, string, int?> Warn)
{
    // The records of the type whose schema declares the rule. Most of what a corpus rule asks is asked
    // of these. `Docs` is there for the rules whose question reaches outside the type, as a glossary's
    // does: the terms it defines are used everywhere but the glossary.
    public IEnumerable<Doc> Records => Docs.Where(d => d.Type == Type);
}
