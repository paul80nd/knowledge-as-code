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

`validate` checks every record in your corpus against the schema its type declares. CI runs it on every pull request,
and you run it before you push.

It decides which files count as records, applies the checks the schema declares, and reports each fault against the file
that caused it. [Discovery](../design/discovery.md) is the pass that decides what it reads.
[Checks](../design/checks.md) says where each check comes from.

Run it from inside your corpus. `kac` finds the corpus by walking up for a `.corpus.yaml`.

### A corpus that consumes another

A corpus declaring `consumes:` cites records another corpus published. `validate` resolves those citations against the
exports [`restore`](restore.md) unpacked under `.imports/`. `eng:pol-VURM.TIMEBOX` reaches a clause of an imported
policy, in prose and in a field declaring a `ref:` alike. A clause that corpus does not have fails here exactly as a
local one would.

**Run `restore` first.** A declared import that has not arrived is an error, and the message names the command to run.

Each side keeps its own spelling. Cite a record your corpus owns bare, and cite an imported one with its producer's
shortcode. `validate` refuses either written the other way, and says which spelling to use.
[Imports](../design/imports.md) says why resolution works this way, and what a check may ask of an imported record.

### An import that has fallen behind

`validate` asks each source your `consumes:` block names what it publishes now. A newer version inside your range is a
**warning**, and `kac restore` takes it. A newer version your range holds back is an **info**, and so is a source this
run could not ask. That last one usually wants the token [`restore`](restore.md#a-private-feed) describes.

None of the three changes the exit code, and a corpus with no `consumes:` block reads no source at all.
[Imports](../design/imports.md#an-import-that-has-fallen-behind) says why being behind is never an error here.

## Examples

### A clean run

```bash
kac validate
```

`validate` prints the counts and exits `0`:

```text
validated 13 document(s) and 8 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
```

### A run that finds faults

`validate` groups the faults under the file that caused them, names the check that fired and the line, and exits `1`:

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

### The findings as JSON

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
    "warnings": 0,
    "infos": 0
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

### A pipeline step

```bash
dotnet tool restore
dotnet tool run kac validate
```

Only an error changes the exit code. [Exit codes](index.md#exit-codes) lists the three.
[Running it in CI](../ci.md) has the whole workflow.

## Known limits

**Every check reads the corpus, and none reads the estate the corpus describes.** A service deleted last month still
validates cleanly. A green run says the corpus is consistent, not that it is right.

**Discovery falls back to a directory walk where git cannot answer.** `kac` walks a tree that is not a repository for
`*.md` instead, and that walk obeys no exclude file. A Markdown file the corpus had ignored is then discovered and
validated. [Discovery](../design/discovery.md#the-fallback-walk) says what else changes.

**It reaches the network where your corpus consumes another.** Every other check reads your working tree. This one asks
each source in `consumes:` what it publishes. A source that does not answer within twenty seconds reports
`import-unreachable`, and the run continues. A corpus with no `consumes:` block opens no connection at all.

**A quotation of an imported clause is not checked.** `clause-quoted-faithfully` compares a quoted span against the
clause the same line cites. An export sends a record's ids and its fields, not its wording, so the words of an imported
clause are not here to compare against and the check passes over the citation.

**`immutable-after-accepted` is declared and does not run.** Whether the content of an accepted document changed is a
question about a diff, and this command reads a working tree.

[`generate`](generate.md) writes the blocks this command checks a file still has.
