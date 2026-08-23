# `checks`: the catalogue printed

<!-- BEGIN GENERATED: usage-checks -->

```text
kac checks [--json] [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--json`     | Emit the check catalogue as JSON.                           |
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-checks -->

## Intent

`checks` prints every check the validator can emit against the corpus it is run in. It reads them from that corpus's
own `.schema/`. So the corpus answers "what will CI hold this corpus to", and no document has to be remembered and
maintained alongside it.

`--json` gives you the same catalogue as data. The test suite reads that form, and holds every check to having a
fixture that trips it.

The command also exits non-zero where the reader-facing table on a type page has drifted from the catalogue. That exit
is what keeps the table honest.

## What it is not

**It is not [`validate`](validate.md).** `checks` opens no record and reports no fault in one. A check missing from a
validate run has either not been declared or not been tripped. `checks` is how you tell those two apart.

## The specification

[Checks](../checks.md) carries the rest: where a check comes from, what the schema pass refuses before any record is
read, and why a rule is data wherever it can be.
