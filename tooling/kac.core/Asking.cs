namespace kac.core;

// Who `new` puts a question to. The console implementation sits beside `Commands`, where the rest of the
// console does; a test hands over scripted answers instead.
//
// The seam is here because what is asked, in what order, and what a flag or a default answers instead are
// decisions, and every decision in this project is testable without a terminal. Only the drawing and the
// keystroke are on the other side.
public interface IAsker
{
    string Text(string question, string fallback);

    string Choose(string question, IReadOnlyList<string> options, string fallback);

    IReadOnlyList<string> ChooseMany(string question, IReadOnlyList<string> options);

    bool Confirm(string question);
}

// What `new` was given on the command line. Every answer has a flag, so nothing is reachable only by
// typing. `docs/cli/new.md` carries the table of defaults.
public sealed record NewRequest
{
    public string? Name { get; init; }

    // The types to adopt, comma-separated, or `all`. Null where the flag was not given.
    public string? Types { get; init; }

    public string? Publishing { get; init; }
    public string? Ci { get; init; }

    // Where the template comes from, and which ref and folder inside it. `From` defaults to the
    // framework's own repository: a tool that cannot bootstrap without a URL you have to look up is a
    // tool people get wrong.
    public string From { get; init; } = Asking.DefaultFrom;
    public string? Ref { get; init; }
    public string? Path { get; init; }

    // Take the default for everything unasked, and put no question to anybody.
    public bool Yes { get; init; }
}

// Turning the flags, the defaults and whatever a person says into the answers a creation needs.
public static class Asking
{
    // The framework's own repository, compiled in. A corpus taking its framework from elsewhere passes
    // `--from` once, at creation, and `.corpus.yaml` remembers.
    public const string DefaultFrom = "https://github.com/paul80nd/knowledge-as-code";

    // The value `--types` takes to mean every type the template declares.
    public const string AllTypes = "all";

    // What resolving the answers came to: the answers, or why there are none. Exactly one of the two is
    // set, and a cancelled run is both null: the person was asked and said no.
    public sealed record Answered(NewAnswers? Answers, string? Problem);

    // Resolve every answer `new` needs, asking only what no flag and no default settles.
    //
    // `asker` is null where there is nobody to ask, which is a run with no terminal or one given `--yes`.
    // A question that reaches that state and has no default is an error rather than a wait: a hung
    // pipeline is worse than a failed one.
    //
    // `origin` is the repository's own remote, which the publishing bases are filled in from. Null where
    // there is no remote yet, and the person types them or the corpus states none.
    public static Answered Resolve(NewRequest request, string folderName, IReadOnlyList<string> declared,
        string? origin, IAsker? asker)
    {
        if (asker is null && !request.Yes && Unanswerable(request) is { } unanswerable)
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

    // Why nothing can settle the answers, or null where something can.
    //
    // Every question missing its flag is named at once, so a caller fixing the invocation fixes all of it
    // rather than meeting one more refusal per run.
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

    // The types the corpus adopts, or null where `--types` named one the template does not declare.
    //
    // Asked as a multi-select with everything ticked, because declining is the exception. A corpus may
    // still tick nothing: `types: []` is a corpus that adopted none, which is a state the descriptor can
    // hold and validation can act on.
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

    // Where the published corpus is browsed, offered already filled in by `Publishing.BaseFrom`. A
    // target needing no base is asked nothing, and a run with nobody to ask takes what the remote
    // implied or states none.
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
