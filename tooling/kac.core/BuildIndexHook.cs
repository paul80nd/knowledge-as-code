using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// The output the build-index hook prints the first time a session reads a file: the build index as context for the
/// session. <c>docs/maintaining/plugin.md</c> says why it is rendered at bundle time.
/// </summary>
public static class BuildIndexHook
{
    /// <summary>
    /// Beside the script that prints it. <c>Bundler</c> writes it only where the hook directory and the build index
    /// both survived the trim.
    /// </summary>
    public const string RenderedFile = "hooks/build-index.json";

    /// <summary>
    /// The most characters Claude Code passes to a session as one hook's context. Over it, the session sees a file path
    /// and the first 2,000 characters.
    /// </summary>
    public const int Cap = 10_000;

    /// <summary><c>Pointed</c> is true where the index was over <see cref="Cap"/>.</summary>
    public sealed record Rendered(string Output, bool Pointed, int IndexLength);

    /// <summary>One line of JSON, so the script prints it without a parser.</summary>
    public static Rendered Render(View.Source source)
    {
        var index = View.Render(View.BuildIndex, source);
        var pointed = index.Length > Cap;

        var output = new JsonObject
        {
            ["hookSpecificOutput"] = new JsonObject
            {
                ["hookEventName"] = "PreToolUse",
                ["additionalContext"] = pointed ? View.BuildIndexPointer(source) : index
            }
        };

        return new Rendered(output.ToJsonString(OneLine) + "\n", pointed, index.Length);
    }

    private static readonly JsonSerializerOptions OneLine = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
