# `checks` list every check the validator can report

<!-- BEGIN GENERATED: usage-checks -->

```text
kac checks [--json] [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--json`     | Emit the check catalogue as JSON.                           |
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-checks -->

## What it is for

`checks` prints every check the validator can emit against the corpus it is run in, read from that corpus's own
`.schema/`. There is no second list to keep in step.

`--json` gives you the same catalogue as data. The test suite reads that form, and holds every check to having a
fixture that trips it.

## What it is not

**It is not [`validate`](validate.md).** `checks` opens no record and reports no fault in one.

## How it works

A run loads that `.schema/` and builds the catalogue from it: every check the schema declares, with the severity it
reports at and what it proves. The list prints one check to a line, and a tally at the foot splits them by severity.
A reader comes to it to learn how much of it fails a build.

`--json` writes that same catalogue as data, one object per check, and prints nothing else.

Either way the run then compares the catalogue against the reader-facing checks table generated onto each type
page. A check the catalogue declares and no row covers, or a row naming a check that no longer exists, is named on
stderr and exits `1`. That comparison happens whether or not `--json` was asked for.

[Checks](../checks.md) is the page for adding a check, or for deciding whether the one you want already exists. It
says where a check comes from, what the schema pass refuses, and why a rule is data wherever it can be.
