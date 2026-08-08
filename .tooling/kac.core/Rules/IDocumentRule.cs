// ---------------------------------------------------------------------------
// The rules that need C#
// ---------------------------------------------------------------------------

namespace kac.core;

// A rule whose question the expression grammar cannot ask. Most rules are answered by an `expr:` in
// `.schema/<type>.yaml`, which costs a line of YAML and a fixture; this interface is for the rest —
// the ones whose useful message names *which* part of a document is at fault, where one fixed string
// could only say that something is. That is the test, and `../../CLAUDE.md` is where to argue with it.
//
// One rule, one class, one file, so that each is unit-testable on its own rather than only through a
// whole corpus, and so that the dispatcher stays a dictionary lookup however many arrive.
public interface IDocumentRule
{
    // The id in the type schema's `rules:` block. What the dispatcher looks the rule up by, and what
    // decides whether a type has adopted it.
    string RuleId { get; }

    // What this rule reports under, which is usually not RuleId: `y-statement-present` emits
    // `y-statement`. Naming both here makes that gap deliberate, and lets the catalogue be assembled
    // from the rules rather than restated beside them.
    IReadOnlyList<CheckDef> Emits { get; }

    void Check(RuleContext ctx);
}

// Everything a rule is given, and the whole of it. `Spec` is the rule as the schema declared it, so a
// rule reading a threshold reads it from there rather than from a constant of its own.
public sealed record RuleContext(
    Doc Doc,
    TypeSchema Type,
    RuleSpec Spec,
    Action<string, string, int?> Err,
    Action<string, string, int?> Warn);
