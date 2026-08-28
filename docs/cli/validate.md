# `validate` check the corpus against its schema

<!-- BEGIN GENERATED: usage-validate -->

```text
kac validate [--json] [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--json`     | Emit the summary and findings as JSON.                      |
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-validate -->

## What it does

`validate` holds every record in your corpus to the schema its type declares. CI runs it on every pull request, and
you run it before you push.

It decides which files count as records, applies the checks the schema declares, and reports each fault against the
file that caused it. [Discovery](../design/discovery.md) is the pass deciding what it reads.
[Checks](../design/checks.md) says where each check comes from.

Run it from inside your corpus. `kac` finds the corpus by walking up for a `.corpus.yaml`.

### A corpus that consumes another validates against both

A corpus declaring `consumes:` cites records another corpus published, and `validate` resolves those citations against
the exports [`restore`](restore.md) unpacked under `.imports/`. `eng:pol-VURM.TIMEBOX` reaches a clause of an imported
policy, in prose and in a field declaring a `ref:` alike, and a clause that corpus does not carry fails here exactly as
a local one would.

**Run `restore` first.** A declared import that has not arrived is an error naming the command, rather than a citation
quietly passed over. A local run that checks less than the pipeline does is how a broken reference reaches your default
branch.

Each side keeps its own spelling. A record your corpus holds is cited bare and one it imported carries its producer's
shortcode, so writing either the other way is refused: two spellings of one obligation defeat every search anybody runs
for it. [Imports](../design/imports.md) says why resolution works this way, and what a check may ask of an imported
record.

## Examples

### Validate the corpus you are standing in

```bash
kac validate
```

A clean run names the counts and exits `0`:

```text
validated 13 document(s) and 8 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
```

A run that finds faults groups them under the file that caused them, names the check that fired and the line, and
exits `1`:

```text
adrs/0001-knowledge-as-code.md
  error  [required-field]  missing required field 'owner'.  (adrs/0001-knowledge-as-code.md:1)
  error  [id-format]       id 'adr-1' must be 'adr-' followed by 4 digits.  (adrs/0001-knowledge-as-code.md:1)
  error  [link-resolves]   link target '0099-nothing.md' does not resolve.  (adrs/0001-knowledge-as-code.md:8)
  error  [identity-id]     identity line id 'adr-0001' does not match the document's id 'adr-1'.  (adrs/0001-knowledge-as-code.md:12)

validated 13 document(s) and 8 template(s), skipped 0 without frontmatter. 4 error(s), 0 warning(s)
```

The name in brackets is a check id, and [`checks`](checks.md) prints what every one of them proves.
[Troubleshooting](../troubleshooting.md) covers the findings you meet first.

### Emit the findings as JSON

```bash
kac validate --json
```

Use this to feed a script or a reviewer bot. The summary comes first, then one object per finding:

```json
{
  "summary": {
    "validated": 13,
    "templates": 8,
    "skipped": 0,
    "errors": 4,
    "warnings": 0
  },
  "findings": [
    {
      "file": "adrs/0001-knowledge-as-code.md",
      "line": 1,
      "severity": "error",
      "check": "required-field",
      "message": "missing required field 'owner'."
    }
  ]
}
```

### Run it in a pipeline

```bash
dotnet tool restore
dotnet tool run kac validate
```

A warning is printed and never changes the exit code. [Exit codes](index.md#exit-codes) carries the three.
[Running it in CI](../ci.md) carries the whole workflow.

## Known limits

**Every check reads the corpus, and none reads the estate the corpus describes.** A service deleted last month still
validates cleanly. A green run says the corpus is consistent rather than that it is right.

**Discovery falls back to a directory walk where git cannot answer.** A tree that is not a repository is walked for
`*.md` instead, which honours no exclude file, so a Markdown file the corpus had ignored is discovered and validated.
[Discovery](../design/discovery.md#the-fallback-walk-honours-nothing) says what that changes.

**`immutable-after-accepted` needs git history and does not run.** Whether the content of an accepted document changed
is a question about a diff, and this command reads a working tree.

[`generate`](generate.md) writes the blocks this command holds a file to still carrying.
