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

    // The targets that build a link today. `azure-devops-wiki` and `mkdocs` are named by the descriptor
    // and addressed by nothing, so a corpus on either exports without links rather than with links built
    // on a convention no one has settled.
    public static readonly IReadOnlyList<string> Addressable = [GitHub];

    private readonly string humanBase;
    private readonly string rawBase;

    public required string Target { get; init; }

    // The commit every link resolves against. A link naming a branch answers a later question than the
    // one the export was built to answer: what the corpus said when it was read.
    public required string Ref { get; init; }

    private Publishing(string humanBase, string rawBase)
    {
        this.humanBase = humanBase.TrimEnd('/');
        this.rawBase = rawBase.TrimEnd('/');
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

        return new Publishing(human, raw) { Target = target, Ref = commit };
    }

    // The anchor a link uses to land on a part. GitHub derives it from the heading by discarding the
    // punctuation it meets, which is the form `Md.Slug` writes and the form the corpus's own links
    // already use.
    public string Anchor(string heading) => Md.Slug(heading);

    // Where a record is read and where it is fetched. `anchor` names a part inside it, and is left off
    // the raw link: raw source is text and offers nowhere to land, so a fragment there would look like
    // an address and be none. A grep of the flat terms file already carries the term's own words, so
    // what the raw link is for is fetching the record whole.
    public PublishedLinks Links(string relPath, string? anchor = null)
    {
        var fragment = anchor is { Length: > 0 } a ? $"#{a}" : "";
        return new PublishedLinks($"{humanBase}/{Ref}/{relPath}{fragment}", $"{rawBase}/{Ref}/{relPath}");
    }
}
