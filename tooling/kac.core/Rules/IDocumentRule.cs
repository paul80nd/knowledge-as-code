namespace kac.core;

// A rule whose message names which part of a document is at fault. `../../CLAUDE.md` states the test.
public interface IDocumentRule
{
    // The id in the type schema's `rules:` block, which the dispatcher looks the rule up by.
    RuleId RuleId { get; }

    // Usually not `RuleId`: `y-statement-present` emits `y-statement`. What each id means is `_checks.yaml`'s to say.
    IReadOnlyList<CheckId> Emits { get; }

    void Check(RuleContext ctx);
}

// Read a threshold from `Spec`, the rule as the schema declared it, never from a constant of the rule's own.
public sealed record RuleContext(Doc Doc, TypeSchema Type, RuleSpec Spec, Report Report);
