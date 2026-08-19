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
//   export     write the corpus to .dist/export/ as data a consumer reads instead of cloning
//   bundle     assemble that export and the .plugin/ tree into an installable plugin
//   checks     list every check the validator implements
//   mechanism  enforce the portability manifest: check the shared layers against a
//              reference corpus, or sync them from one
//
// This file is only the CLI surface: it wires System.CommandLine to Commands and does the repo-root
// pre-flight. Every subcommand's logic lives in the kac.core project, referenced by the #:project
// directive above — one class per file, named for what it holds. Four carry the substance: Schema.cs
// loads .schema/*.yaml, Document.cs parses a record, Validator.cs holds the checks, Generator.cs
// builds the generated blocks.
//
// The tool is deliberately free of type-specific rules: everything it enforces is
// read from the YAML schema, so adding a type is adding a YAML file, not editing C#.
// See tooling/features/checks.md for what fails versus warns and how each check maps to the schema.

using System.CommandLine;
using kac.core;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac: could not locate the repo root (no .schema above the cwd).");
    return 2;
}

// `validate` takes no paths: several of its checks ask about the shape of the corpus rather than
// about a document, and a subset cannot answer them.
var jsonOpt = new Option<bool>("--json") { Description = "Emit the summary and findings as JSON." };
var validate = new Command("validate", "Check the corpus against .schema/*.yaml.")
{
    jsonOpt
};
validate.SetAction(pr => Commands.Validate(repoRoot, pr.GetValue(jsonOpt)));

var checkOpt = new Option<bool>("--check") { Description = "Fail if a generated file is stale instead of writing it." };
var index = new Command("index", "Regenerate _index.md and the generated blocks in <type>.md.")
{
    checkOpt
};
index.SetAction(pr => Commands.Index(repoRoot, pr.GetValue(checkOpt)));

// `export` writes the corpus to `.dist/export/` as data a consumer reads instead of cloning. `--type`
// narrows what is written and never what is read: the whole corpus is loaded either way, so ids resolve
// against every record rather than against the ones a narrowed run happened to reach.
var typeOpt = new Option<string?>("--type") { Description = "Export one type rather than every type that contributes." };
var export = new Command("export", "Write the corpus to .dist/export/ as a versioned export.")
{
    typeOpt
};
export.SetAction(pr => Commands.Export(repoRoot, pr.GetValue(typeOpt)));

// `bundle` assembles what `export` wrote, plus the `.plugin/` tree, into a plugin directory under
// `.dist/plugin/`. Two commands rather than one because they fail differently and are proved
// differently: an export is wrong about the corpus, and a bundle is wrong about what it shipped.
var bundle = new Command("bundle", "Assemble the export and .plugin/ into a plugin under .dist/plugin/.");
bundle.SetAction(_ => Commands.Bundle(repoRoot));

// `checks` is machinery before it is documentation: the test suite reads `checks --json` to assert
// every rule is exercised by a fixture, so a new rule cannot ship without a golden covering it.
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
var mechSyncOpt = new Option<bool>("--sync") { Description = "Take the shared layers from the reference, then record what it took in .corpus.yaml." };
var againstOpt = new Option<string?>("--against") { Description = "Reference corpus (a path). Defaults to upstream.url in .corpus.yaml." };
var mechanism = new Command("mechanism", "Enforce the portability manifest: compare the shared layers against a reference, or take them from one.")
{
    mechCheckOpt,
    mechSyncOpt,
    againstOpt
};
mechanism.SetAction(pr =>
    Commands.Mechanism(repoRoot, pr.GetValue(mechCheckOpt), pr.GetValue(mechSyncOpt), pr.GetValue(againstOpt)));

var root = new RootCommand("kac — the knowledge-as-code validator and generator.")
    { validate, index, export, bundle, checks, mechanism };

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
