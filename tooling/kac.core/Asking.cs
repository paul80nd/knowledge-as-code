using System.Diagnostics.CodeAnalysis;

namespace kac.core;

/// <summary>
/// What is asked, in what order, and what a flag or default answers instead are decisions, and testable without a
/// terminal. A test hands over scripted answers.
/// </summary>
public interface IAsker
{
    string Text(string question, string fallback);

    string Choose(string question, IReadOnlyList<string> options, string fallback);

    IReadOnlyList<string> ChooseMany(string question, IReadOnlyList<string> options);

    /// <summary>Asks a yes or no question.</summary>
    /// <param name="fallback">The answer a bare Enter gives. Yes for a question standing between somebody and the
    /// thing they asked for, and no for one standing in front of a deletion.</param>
    bool Confirm(string question, bool fallback = true);
}

/// <summary>
/// <c>Unattended</c> exists because taking the default is right where the step is what the caller asked for, and
/// stopping is right where it is a deletion they did not.
/// </summary>
public enum Consent
{
    Given,

    /// <summary>The run stops, having written nothing.</summary>
    Withheld,

    /// <summary>Nobody was there to ask, and this question has no default worth taking.</summary>
    Unattended
}

/// <summary>
/// Every answer has a flag, so nothing is reachable only by typing. <c>docs/use/cli/new.md</c> carries the defaults.
/// </summary>
public sealed record NewRequest
{
    public string? Name { get; init; }

    /// <summary>Null where the flag was not given.</summary>
    public string? Types { get; init; }

    public string? Publishing { get; init; }
    public string? Ci { get; init; }

    public string From { get; init; } = Asking.DefaultFrom;
    public string? Ref { get; init; }
    public string? Path { get; init; }

    public bool Yes { get; init; }
}

public static class Asking
{
    /// <summary>
    /// A corpus taking its framework from elsewhere passes <c>--from</c> once, and <c>.corpus.yaml</c> remembers.
    /// </summary>
    public const string DefaultFrom = "https://github.com/paul80nd/knowledge-as-code";

    public const string AllTypes = "all";

    /// <summary>
    /// Read once per command, so a run cannot be interactive in one decision and unattended in the next.
    /// </summary>
    public static bool Asks(bool yes, bool interactive) => interactive && !yes;

    /// <summary>Goes ahead unattended: the step is the one the caller asked for.</summary>
    public static Consent OrDefault(IAsker? asker, string question, bool fallback = true) =>
        asker is null || asker.Confirm(question, fallback) ? Consent.Given : Consent.Withheld;

    /// <summary>Refused unattended, because the step takes something away. Only <c>--yes</c> gets past it.</summary>
    public static Consent OrRefuse(IAsker? asker, bool yes, string question, bool fallback = true) =>
        Unattended(asker, yes) ? Consent.Unattended
        : asker is null || asker.Confirm(question, fallback) ? Consent.Given
        : Consent.Withheld;

    /// <summary>Refused rather than waited on: a hung pipeline is worse than a failed one.</summary>
    private static bool Unattended(IAsker? asker, bool yes) => asker is null && !yes;

    /// <summary>Exactly one of the two is set.</summary>
    public sealed record Answered(NewAnswers? Answers, string? Problem)
    {
        [MemberNotNullWhen(true, nameof(Problem))]
        [MemberNotNullWhen(false, nameof(Answers))]
        public bool Failed => Problem is not null;
    }

    /// <summary>
    /// <c>asker</c> is null where nobody can be asked. <c>origin</c> is the repository's own remote, or null where
    /// there is none yet.
    /// </summary>
    public static Answered Resolve(NewRequest request, string folderName, IReadOnlyList<string> declared,
        string? origin, IAsker? asker)
    {
        if (Unattended(asker, request.Yes) && Unanswerable(request) is { } unanswerable)
            return Problem(unanswerable);

        var name = request.Name ?? (asker is null
            ? folderName
            : asker.Text("What is this corpus called?", folderName));

        if (string.IsNullOrWhiteSpace(name))
            return Problem("new: a corpus needs a name. pass --name, or answer the question.");

        if (Types(request, declared, asker) is not { } types) return Problem(UnknownTypes(request, declared));

        var publishing = request.Publishing ?? (asker is null
            ? Publishing.None
            : asker.Choose("Where is this corpus published?", Publishing.Targets, Publishing.None));

        if (!Publishing.Targets.Contains(publishing, StringComparer.Ordinal))
            return Problem($"new: --publishing '{publishing}' is not a target. it is one of "
                           + $"{string.Join(", ", Publishing.Targets)}.");

        var ci = request.Ci ?? (asker is null
            ? CiSystem.None
            : asker.Choose("What builds this corpus?", CiSystem.All, CiSystem.None));

        if (!CiSystem.All.Contains(ci, StringComparer.Ordinal))
            return Problem($"new: --ci '{ci}' is not a system this tool offers. it is one of "
                           + $"{string.Join(", ", CiSystem.All)}.");

        var published = Base(publishing, origin, asker);

        return new Answered(
            new NewAnswers
            {
                Name = name.Trim(),
                Types = types,
                PublishingTarget = publishing,
                Base = published,
                Ci = ci
            },
            null);
    }

    /// <summary>Every question missing its flag is named at once, so one fix answers all of them.</summary>
    private static string? Unanswerable(NewRequest request)
    {
        List<string> missing = [];
        if (request.Name is null) missing.Add("--name");
        if (request.Types is null) missing.Add("--types");
        if (request.Publishing is null) missing.Add("--publishing");
        if (request.Ci is null) missing.Add("--ci");

        return missing.Count == 0
            ? null
            : $"new: there is no terminal to ask on, and {string.Join(", ", missing)} was not given. "
              + "pass every answer as a flag, or pass --yes to take the defaults.";
    }

    /// <summary><c>types: []</c> is a valid answer: a corpus that adopted none.</summary>
    private static IReadOnlyList<string>? Types(NewRequest request, IReadOnlyList<string> declared,
        IAsker? asker)
    {
        if (request.Types is not { } named)
            return asker is null
                ? declared
                : [.. asker.ChooseMany("Which types does it adopt?", declared)];

        if (named.Trim().Equals(AllTypes, StringComparison.OrdinalIgnoreCase)) return declared;

        var asked = named.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return asked.All(t => declared.Contains(t, StringComparer.Ordinal))
            ? [.. declared.Where(t => asked.Contains(t, StringComparer.Ordinal))]
            : null;
    }

    private static string UnknownTypes(NewRequest request, IReadOnlyList<string> declared)
    {
        var asked = (request.Types ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var unknown = asked.Where(t => !declared.Contains(t, StringComparer.Ordinal));

        return $"new: this template declares no type called {string.Join(" or ", unknown.Select(t => $"'{t}'"))}. "
               + $"it declares {string.Join(", ", declared)}.";
    }

    private static string? Base(string target, string? origin, IAsker? asker)
    {
        if (target.Equals(Publishing.None, StringComparison.Ordinal)) return null;

        var derived = Publishing.BaseFrom(target, origin);
        if (asker is null) return derived;

        var answer = asker.Text("Where is the published corpus browsed?", derived ?? "");

        return answer.Trim() is { Length: > 0 } v ? v : null;
    }

    private static Answered Problem(string message) => new(null, message);
}
