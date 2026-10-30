# `validate` check the corpus against its schema

<!-- BEGIN GENERATED: usage-validate -->

```text
kac validate [--json] [--no-color] [--since <REF>]
```

| Option          | What it does                                                                   |
|-----------------|--------------------------------------------------------------------------------|
| `--json`        | Emit the summary and findings as JSON.                                         |
| `--no-color`    | Turn colour off. NO_COLOR in the environment does the same.                    |
| `--since <REF>` | Also compare each record with its state at this revision, such as origin/main. |

<!-- END GENERATED: usage-validate -->

## What it does

`validate` checks every record in your corpus against the schema its type declares. CI runs it on every pull request,
and you run it before you push.

It decides which files count as records, applies the checks the schema declares, and reports each fault against the file
that caused it. [Discovery](../discovery.md) is the pass that decides what it reads.
[Checks](../../reference/checks.md) says where each check comes from.

Run it from inside your corpus. `kac` finds the corpus by walking up for a `.corpus.yaml`.

### A corpus that consumes another

A corpus declaring `consumes:` cites records another corpus published. `validate` resolves those citations against the
exports [`restore`](restore.md) unpacked under `.imports/`. `eng:pol-VURM.TIMEBOX` reaches a clause of an imported
policy, in prose and in a field declaring a `ref:` alike. A clause that corpus does not have fails here exactly as a
local one would.

**Run `restore` first.** A declared import that has not arrived is an error, and the message names the command to run.

Each side keeps its own spelling. Cite a record your corpus owns bare, and cite an imported one with its producer's
shortcode. `validate` refuses either written the other way, and says which spelling to use. The producer is whoever
wrote the record, so a corpus reached through another is cited by its own shortcode and not by the one in your
`consumes:` block.
[Imports](../imports.md) says why resolution works this way, and what a check may ask of an imported record.

### An import that has fallen behind

`validate` asks each source your `consumes:` block names what it publishes now. A newer version inside your range is a
**warning**, and `kac restore` takes it. A newer version your range holds back is an **info**, and so is a source this
run could not ask. That last one usually wants the token [`restore`](restore.md#a-private-feed) describes.

None of the three changes the exit code, and a corpus with no `consumes:` block reads no source at all.
[Imports](../imports.md#an-import-that-has-fallen-behind) says why being behind is never an error here.

### The `accepts` field

A record keeps a warning it has weighed by naming the check in its own `accepts:` field, with the reason:

```yaml
accepts:
  - check: critical-service-is-alerted
    reason: "`kac` is a command-line tool and emits nothing at run time. Operational notes records the gap."
```

`validate` then reports every warning that check raises on that record as an **info**, reading `accepted:` and the
reason. An info finding takes no annotation and no pull request comment, so CI stops raising it. A local run and
`--json` still show it.

An entry takes a warning only. An entry naming a check that reports errors, a check that reports only info, or an id
that [`checks`](checks.md) does not list is the error `acceptance-check`. An entry that matches no warning on its record
is the warning `acceptance-stale`, which asks you to remove the entry. Accept a warning where the record says why it
stands, and never to hide unfinished work.

### Comparing with a base revision

```bash
kac validate --since origin/main
```

`--since` also compares each record with its state at the revision you name. `validate` reads that revision and your
working tree, and no history between them. It pairs the two copies of a record by `id`, so a file you moved is still
compared. A record whose `id` changed is a rename. It pairs with the record of its type now at its path, or at the path
git reports it moved to, where that record's `id` is new. Git reports a move once the file is staged or committed.

| Check                       | Level   | What it reports                                                        |
|-----------------------------|---------|------------------------------------------------------------------------|
| `immutable-after-accepted`  | error   | a change in substance to an ADR that was `accepted` or `rejected`      |
| `immutable-after-published` | error   | a change in substance to a postmortem that was `published`             |
| `clause-id-kept`            | error   | a clause id gone from a policy that was `active`                       |
| `rule-id-kept`              | error   | a rule id gone from a standard that was `active`                       |
| `retire-not-delete`         | warning | a record deleted while its status was the value its type puts in force |

`clause-id-kept` and `rule-id-kept` read the id alone. A part reworded, a part added or a new title passes. A record
that was not `active` at the revision changes freely, and that includes a draft made `active` in the same change.

A change in substance is anything except these:

* **Layout.** A reflow passes, and so does a table realigned around a longer cell.
* **A link target.** The words a reader sees must stay the same. An autolink, as `<https://example.com>`, may change
  whole.
* **A typo.** Each changed word is within two edits of the word it replaced, and the section keeps its word count.
* **`status`, `superseded-by`, `review-by` and `accepts`.** Any other frontmatter field that moves is a change in
  substance.
* **A renamed `id`.** A frontmatter value, or the whole text of a link, may swap a renamed record's old `id` for its
  new one. The same swap anywhere else in the prose, or a swap for any other `id`, is a change in substance.

`validate` compares the body one `##` section at a time. A section added, removed or renamed is a change in substance.

A record deleted as a draft, or after it was retired or superseded, reports nothing. A renamed record was not deleted,
and reports nothing either. Something outside the corpus may still cite an in-force record's id, so retire it through
its `status`.

Without `--since`, `validate` prints one line naming the checks it skipped. A ref git cannot resolve exits `1` and names
the ref. That includes a shallow clone lacking it, and a tree outside git.

### Annotations under CI

Under a CI platform, `validate` also writes each error and each warning as an annotation. The report and the exit code
are the same as in a local run.

| Platform        | Detected by           | Where an annotation shows                                                    |
|-----------------|-----------------------|------------------------------------------------------------------------------|
| GitHub Actions  | `GITHUB_ACTIONS=true` | On the check, and on the line it names under *Files changed* when it changed |
| Azure Pipelines | `TF_BUILD=True`       | In the run's summary, beside the step that ran `validate`                    |

A warning annotation leaves an Azure Pipelines step *Succeeded*. Each path is relative to the root of the git
repository, so a corpus in a subfolder still lands on the right file.

### A pull request comment

On a pull request with an error or a warning, `validate` also posts one comment listing them. The comment is a Markdown
table with a row per finding, giving the severity, the file and line, the check and the message. Its heading names the
corpus and gives the counts. The name is `corpus:` from `.corpus.yaml`. Where that is not set, it is the corpus's folder
in the repository, or "this corpus" for a corpus at the repository's root.

An icon stands beside each severity word and the heading: ❌ for an error, ⚠️ for a warning and ✅ for a clean run. The
word is always there, so the icon is never the only cue.

A hidden line naming the corpus marks the comment, so each later run updates the same one. Once the findings have gone,
the comment says the corpus validated with no errors or warnings. A clean pull request with no earlier comment gets
none, and `validate` never deletes one. Each corpus in a repository keeps a comment of its own.

On Azure Pipelines the comment opens a thread. The thread is active while the corpus has findings. A clean run closes
it, and a run that finds them again makes it active.

`validate` posts only where the pipeline passes a token to the step and grants it the permission:

| Platform        | Token the step needs                        | Permission it needs                                 |
|-----------------|---------------------------------------------|-----------------------------------------------------|
| GitHub Actions  | `GITHUB_TOKEN: ${{ github.token }}`         | `pull-requests: write` on the job                   |
| Azure Pipelines | `SYSTEM_ACCESSTOKEN: $(System.AccessToken)` | *Contribute to pull requests* for the build service |

Install `kac` at a pinned version in a job with this permission, so no release you did not choose runs with the token.
The starter pipelines [`new`](new.md) writes restore `kac` from `.config/dotnet-tools.json`, and pass the token.

Without the token, a run with findings prints a note and posts nothing, and a clean run prints nothing. A pull request
from a fork gets a token that cannot write to it, so the post fails. Either way the note names the pull request, and the
exit code is unchanged.

An info finding appears in neither an annotation nor the comment, and `--json` writes neither.

## Examples

### A clean run

```bash
kac validate
```

`validate` prints the counts, names the checks it skipped without `--since`, and exits `0`:

```text
validated 13 document(s) and 8 template(s), skipped 0 without frontmatter. 0 error(s), 0 warning(s)
validate: skipped immutable-after-accepted and retire-not-delete, which compare each record with an earlier revision. Name one with `--since <ref>` to run them.
```

### A run that finds faults

`validate` groups the faults under the file that caused them, names the check that fired and the line, and exits `1`:

```text
adrs/0001-knowledge-as-code.md
  error  [required-field]  missing required field 'owner'.  (adrs/0001-knowledge-as-code.md:1)
  error  [id-format]       id 'adr-1' must be 'adr-' followed by 4 digits.  (adrs/0001-knowledge-as-code.md:2)
  error  [link-resolves]   link target '0099-nothing.md' does not resolve.  (adrs/0001-knowledge-as-code.md:8)
  error  [identity-id]     identity line id 'adr-0001' does not match the document's id 'adr-1'.  (adrs/0001-knowledge-as-code.md:12)

validated 13 document(s) and 8 template(s), skipped 0 without frontmatter. 4 error(s), 0 warning(s)
validate: skipped immutable-after-accepted and retire-not-delete, which compare each record with an earlier revision. Name one with `--since <ref>` to run them.
```

The name in brackets is a check id, and [`checks`](checks.md) prints what every one of them proves.
[Troubleshooting](../troubleshooting.md) covers the findings you meet first.

### A run under GitHub Actions

`validate` prints the same report, then one line per error or warning before the counts. GitHub reads each of these
lines as an annotation:

```text
::error file=adrs/0001-knowledge-as-code.md,line=1,title=required-field::missing required field 'owner'.
```

Under Azure Pipelines the same finding is written as:

```text
##vso[task.logissue type=error;sourcepath=adrs/0001-knowledge-as-code.md;linenumber=1;code=required-field]missing required field 'owner'.
```

Where that is the run's only finding, the pull request comment on either platform reads:

```markdown
<!-- kac validate: acme-corpus -->
### ❌ `kac validate` for acme-corpus: 1 error, 0 warnings

| Severity | File | Check | Message |
|----------|------|-------|---------|
| ❌ error | `adrs/0001-knowledge-as-code.md:1` | `required-field` | missing required field 'owner'. |
```

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
validated. [Discovery](../discovery.md#the-fallback-walk) says what else changes.

**It reaches the network where your corpus consumes another.** Every other check reads your working tree. This one asks
each source in `consumes:` what it publishes. A source that does not answer within twenty seconds reports
`import-unreachable`, and the run continues. A corpus with no `consumes:` block opens no connection for its checks. A
pull request comment is the one other connection, and a local run never makes it.

**A quotation of an imported clause is not checked.** `clause-quoted-faithfully` compares a quoted span against the
clause the same line cites. An export sends a record's ids and its fields, not its wording, so the words of an imported
clause are not here to compare against and the check passes over the citation.

**GitHub shows ten annotations of each kind per step.** `validate` writes one for every error and warning, in file
order, and GitHub shows the first ten errors and the first ten warnings. The job log keeps the whole report.

**`--since` reads two revisions and walks no history.** It compares the working tree with the tip of the ref, not with
where your branch left it. A record changed on `main` since your branch began reads as a change your branch made. A
GitHub pull request checks out the merge commit, so CI compares only what the pull request changes.

**Nothing catches a reused id.** A record retired, then deleted later, leaves its id free, and `validate` passes a new
record that takes it.

[`generate`](generate.md) writes the blocks this command checks a file still has.
