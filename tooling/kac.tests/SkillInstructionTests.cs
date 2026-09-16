using System.Text.RegularExpressions;
using kac.core;

// What a skill tells a reader to leave out of a record, checked against the fields every type requires.
//
// A skill that forbids a required field produces a record `kac validate` refuses, and the reader finds that out after
// they have written it. `PluginSkillTests`, `PluginSkillFieldTests` and `PluginSkillStalenessTests` read what a skill
// claims about the export, and none of them reads what a skill tells a reader to write.
//
// Nothing in a skill says which type its reader is writing, so a field one type requires and another leaves optional
// is not judged here: `writing-a-record` tells a reader to leave `sources` out, which is right everywhere but
// `reports`. The check runs one way as well. A skill saying less than the schema requires costs nothing, and a skill
// forbidding a required field costs a pull request the corpus rejects.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class SkillInstructionTests
{
    // The two trees a skill is written in. A session writing a record can be reading either.
    private static readonly string[] Trees =
    [
        Path.Combine(Repo.Root, ".claude", "skills"),
        Path.Combine(Repo.Root, "template", ".plugin", "skills")
    ];

    // A clause that forbids something, up to the punctuation ending it. The verbs are the ones the writing skills use
    // for an instruction, so a spelling nobody listed here is a miss rather than a false failure. `does not` is left
    // out on purpose: it describes what the tool writes, and the sentence around it usually tells the reader to
    // supply the field themselves.
    [GeneratedRegex(@"\b(?:never|do not|don[''’]t) (?:write|set|fill|state|give)\b[^.,;\n]*"
                    + @"|\bleaves? [^.,;\n]{0,40}?\bout\b[^.,;\n]*"
                    + @"|\bomits?\b[^.,;\n]*", RegexOptions.IgnoreCase)]
    private static partial Regex Forbids();

    // A code span naming a frontmatter field, bare or with the key's own colon. A span carrying a value names that
    // value, and forbidding one value of `status` leaves the field on the record.
    [GeneratedRegex(@"`(?<field>[a-z][a-z0-9-]*):?`")]
    private static partial Regex FieldSpan();

    public static TheoryData<string> Each()
    {
        var data = new TheoryData<string>();
        foreach (var file in Trees
                     .SelectMany(tree => Directory.EnumerateFiles(tree, "*.md", SearchOption.AllDirectories))
                     .Select(file => Path.GetRelativePath(Repo.Root, file))
                     .Order(StringComparer.Ordinal))
            data.Add(file);

        return data;
    }

    [Theory]
    [MemberData(nameof(Each))]
    public void It_never_tells_a_reader_to_leave_out_a_field_every_type_requires(string skill)
    {
        var required = Required(Schema.Load(Repo.Root));
        var text = Files.ReadLf(Path.Combine(Repo.Root, skill));

        var forbidden = Forbids().Matches(text)
            .SelectMany(clause => FieldSpan().Matches(clause.Value)
                .Select(span => span.Groups["field"].Value)
                .Where(required.Contains)
                .Select(field => $"line {Line(text, clause.Index)}: '{field}' in \"{clause.Value.Trim()}\""))
            .ToList();

        Assert.True(forbidden.Count == 0,
            $"{skill} tells a reader to leave out a field every type requires.\n{string.Join('\n', forbidden)}");
    }

    // Every field required of every record in the corpus. A universal field is the only one that can reach all of
    // them, and a type may refine one of those to optional, so both halves are read rather than assumed.
    private static IReadOnlySet<string> Required(Schema schema) =>
        schema.Universal.Keys
            .Where(name => schema.ByFolder.Values.All(type =>
                schema.EffectiveField(type, name) is { Required: true, RequiredWhen: null }))
            .ToHashSet(StringComparer.Ordinal);

    private static int Line(string text, int index) => text[..index].Count(c => c == '\n') + 1;
}
