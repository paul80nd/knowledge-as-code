#:package System.CommandLine@2.0.*
#:project ./kac.core/kac.core.csproj
#:property TargetFramework=net10.0
#:property Nullable=enable

// kac — the knowledge-as-code validator and generator.
//
// One tool, several subcommands, sharing a schema-loading and markdown-parsing core:
//
//   validate   check the corpus against .schema/*.yaml
//   index      regenerate _index.md and the generated blocks in <type>.md
//   mechanism  enforce the portability manifest: check the shared layers against a
//              reference corpus, or sync them from one
//
// The tool is deliberately free of type-specific rules: everything it enforces is
// read from the YAML schema, so adding a type is adding a YAML file, not editing C#.
// See .tooling/README.md for what fails versus warns and how each check maps to the schema.

using System.CommandLine;
using kac.core;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac: could not locate the repo root (no .schema above the cwd).");
    return 2;
}

// validate — check the corpus against the schema.
var pathsArg = new Argument<string[]>("paths")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "Subtrees or files to validate (default: the whole repo)."
};
var jsonOpt = new Option<bool>("--json") { Description = "Emit the summary and findings as JSON." };
var validate = new Command("validate", "Check the corpus against .schema/*.yaml.")
{
    pathsArg,
    jsonOpt
};
validate.SetAction(pr => Commands.Validate(repoRoot, [.. pr.GetValue(pathsArg) ?? []], pr.GetValue(jsonOpt)));

// index — regenerate _index.md and the generated blocks in <type>.md.
var checkOpt = new Option<bool>("--check") { Description = "Fail if a generated file is stale instead of writing it." };
var index = new Command("index", "Regenerate _index.md and the generated blocks in <type>.md.")
{
    checkOpt
};
index.SetAction(pr => Commands.Index(repoRoot, pr.GetValue(checkOpt)));

// checks — list every check the validator implements. Its purpose is machinery, not humans:
// the test suite reads `checks --json` to assert every rule is exercised by a fixture, so a
// new rule cannot ship without a golden covering it.
var checksJsonOpt = new Option<bool>("--json") { Description = "Emit the check catalogue as JSON." };
var checks = new Command("checks", "List every check the validator implements.")
{
    checksJsonOpt
};
checks.SetAction(pr => Commands.Checks(repoRoot, pr.GetValue(checksJsonOpt)));

// mechanism — enforce the portability manifest. `--check` compares this corpus's shared layers
// against a reference copy and reports drift, following the same discipline as `index --check`:
// recompute, compare, name what is stale, exit non-zero, never write. `--sync` is the write half:
// it takes those layers from the reference, records what it took, and regenerates.
var mechCheckOpt = new Option<bool>("--check") { Description = "Compare the shared layers against a reference and report drift; never writes." };
var mechSyncOpt = new Option<bool>("--sync") { Description = "Take the shared layers from the reference, then record what it took in .mechanism.lock." };
var againstOpt = new Option<string?>("--against") { Description = "Reference corpus (a path). Defaults to upstream.url in .mechanism.lock." };
var mechanism = new Command("mechanism", "Enforce the portability manifest: compare the shared layers against a reference, or take them from one.")
{
    mechCheckOpt,
    mechSyncOpt,
    againstOpt
};
mechanism.SetAction(pr =>
    Commands.Mechanism(repoRoot, pr.GetValue(mechCheckOpt), pr.GetValue(mechSyncOpt), pr.GetValue(againstOpt)));

var root = new RootCommand("kac — the knowledge-as-code validator and generator.") { validate, index, checks, mechanism };

// Bad arguments exit 1 (System.CommandLine's default) — the printed error makes it
// obvious it was a usage problem rather than corpus errors. Exit 2 is reserved for the
// pre-flight failure above (no repo root), where the tool never got as far as parsing.
return root.Parse(args).Invoke();

static string? FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".schema")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

// ---------------------------------------------------------------------------
// This file is only the CLI surface: it wires System.CommandLine to Commands and does the repo-root
// pre-flight. Every subcommand's logic — and all the mechanics — live in the kac.core project,
// referenced via the #:project directive at the top, one class per file and named for what it holds.
// Four of them carry the substance: Schema.cs loads .schema/*.yaml, Document.cs parses a record,
// Validator.cs holds the checks, Generator.cs builds the generated blocks. The rest are helpers.
// ---------------------------------------------------------------------------
