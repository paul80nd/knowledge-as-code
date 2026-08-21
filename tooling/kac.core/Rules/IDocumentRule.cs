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
    RuleId RuleId { get; }

    // The check ids this rule reports under, which are usually not RuleId: `y-statement-present` emits
    // `y-statement`. Naming both here makes that gap deliberate, and gives it two types so the pair
    // cannot swap. What each id means is `_checks.yaml`'s to say.
    IReadOnlyList<CheckId> Emits { get; }

    void Check(RuleContext ctx);
}

// Everything a rule is given, and the whole of it. `Spec` is the rule as the schema declared it, so a
// rule reading a threshold reads it from there rather than from a constant of its own.
//
// `Report` is the one the document is being read through, so a rule writes what it found where every
// other check on that document writes it, and under an id of the same type.
public sealed record RuleContext(Doc Doc, TypeSchema Type, RuleSpec Spec, Report Report);
