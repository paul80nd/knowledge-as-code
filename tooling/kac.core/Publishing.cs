// ---------------------------------------------------------------------------
// Publishing targets — how a published corpus is addressed
// ---------------------------------------------------------------------------

namespace kac.core;

// The two addresses one record has once it is published. A person follows `Human` and reads the record
// rendered; an agent fetches `Raw` and reads the source it was written in.
//
// Both are carried because neither serves the other's reader. A rendered page costs an agent a parse of
// someone else's HTML, and raw markdown is not what a person opening a citation expects to meet.
public sealed record PublishedLinks(string Human, string Raw);

// The two addresses stated once, as strings a reader substitutes into: `{path}` for a record's path and
// `{anchor}` for a part inside it. `Publishing.Templates` builds them.
public sealed record LinkTemplates(string Human, string Raw);

// How a publishing target addresses what it publishes: the rule joining a base to a record's path, and
// the anchor rule for a part inside it.
//
// The rules live here rather than in `.corpus.yaml` because they are a property of the target and not of
// the corpus. Two corpora published to GitHub build a link the same way and differ only in where their
// repository sits, so the descriptor supplies the bases and nothing else.
public sealed class Publishing
{
    public const string AzureDevOpsWiki = "azure-devops-wiki";
    public const string GitHub = "github";
    public const string MkDocs = "mkdocs";
    public const string None = "none";

    // The two placeholders a template leaves for its reader to fill.
    public const string PathToken = "{path}";
    public const string AnchorToken = "{anchor}";

    // The targets that build a link today. `azure-devops-wiki` and `mkdocs` are named by the descriptor
    // and addressed by nothing, so a corpus on either exports without links rather than with links built
    // on a convention no one has settled.
    private static readonly IReadOnlyList<string> Addressable = [GitHub];

    private readonly string humanBase;
    private readonly string rawBase;

    // The corpus root's place inside the published repository, carrying its own leading slash, or empty
    // where the corpus is the repository. Normalised on the way in so the rest of the class joins it the
    // same way whether the descriptor wrote it with slashes or without.
    private readonly string prefix;

    public required string Target { get; init; }

    // The commit every link resolves against. A link naming a branch answers a later question than the
    // one the export was built to answer: what the corpus said when it was read.
    public required string Ref { get; init; }

    private Publishing(string humanBase, string rawBase, string? pathPrefix)
    {
        this.humanBase = humanBase.TrimEnd('/');
        this.rawBase = rawBase.TrimEnd('/');
        prefix = pathPrefix?.Trim('/') is { Length: > 0 } p ? "/" + p : "";
    }

    // How this corpus addresses its published form, or null where it has no addressable one — it
    // publishes nowhere, it names a target nothing builds links for, or it names one and supplies no
    // bases. Null in every case, because a caller's question is whether it can write a link at all, and
    // three ways of being unable to are one answer.
    //
    // `gitRef` is the commit the export was built from. Null where git could not say, which is a corpus
    // whose records have no stable address, so it takes the same answer as the rest.
    public static Publishing? For(CorpusDescriptor descriptor, string? gitRef)
    {
        if (descriptor.PublishingTarget is not { } target) return null;
        if (!Addressable.Contains(target, StringComparer.Ordinal)) return null;
        if (descriptor.HumanBase is not { Length: > 0 } human) return null;
        if (descriptor.RawBase is not { Length: > 0 } raw) return null;
        if (gitRef is not { Length: > 0 } commit) return null;

        return new Publishing(human, raw, descriptor.PathPrefix) { Target = target, Ref = commit };
    }

    // The anchor a link uses to land on a part. GitHub derives it from the heading by discarding the
    // punctuation it meets, which is the form `Md.Slug` writes and the form the corpus's own links
    // already use.
    public static string Anchor(string heading) => Md.Slug(heading);

    // The link rules as two strings a reader substitutes into, so an export states each address once and
    // a line carries only what varies: the record's path, and the anchor of the part inside it.
    //
    // **The ref is baked in rather than left as a placeholder.** Forty hex characters passing through an
    // agent's hands is forty chances at a one-digit slip, and the result of one is a plausible 404 that
    // neither the agent nor its reader is likely to check. With the commit already in the string, the
    // worst a substitution can produce is a wrong path, which is visible and correctable.
    //
    // **Only the human template takes an anchor.** Raw source is text and offers nowhere to land, so the
    // asymmetry is written into the templates rather than left as a rule each reader has to remember.
    //
    // **The path prefix is baked in for the same reason the ref is.** It is a property of where the
    // corpus sits and never varies between two records, so a reader substituting into the template has
    // one thing to supply and cannot put it on the wrong side of the commit.
    public LinkTemplates Templates() =>
        new($"{humanBase}/{Ref}{prefix}/{PathToken}#{AnchorToken}", $"{rawBase}/{Ref}{prefix}/{PathToken}");

    // Where a record is read and where it is fetched, resolved. `anchor` names a part inside it, and a
    // link with no anchor drops the fragment along with it.
    //
    // Built by substituting into the templates above, so a link this method resolves and a link a
    // consumer assembles for the same record are the same string. Two constructions of one address would
    // be two places for the rule to live.
    public PublishedLinks Links(string relPath, string? anchor = null)
    {
        var templates = Templates();

        var human = anchor is { Length: > 0 }
            ? templates.Human.Replace(AnchorToken, anchor, StringComparison.Ordinal)
            : templates.Human.Replace($"#{AnchorToken}", "", StringComparison.Ordinal);

        return new PublishedLinks(
            human.Replace(PathToken, relPath, StringComparison.Ordinal),
            templates.Raw.Replace(PathToken, relPath, StringComparison.Ordinal));
    }
}
