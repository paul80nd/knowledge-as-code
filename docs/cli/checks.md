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

## What it does

`checks` prints every check the validator can report against your corpus. Use it to see what CI will hold that corpus
to, and to find out whether the check you were about to add already exists.

The list is read from that corpus's own `.schema/`, so there is no second catalogue to keep in step. A corpus that
declares a type of its own sees that type's checks here without the tool changing.
[Checks](../design/checks.md) says where a check comes from.

`checks` opens no record and reports no fault in one. [`validate`](validate.md) is what fires these against documents.

## Examples

### List what CI will hold your corpus to

```bash
kac checks
```

One check to a line, with the severity it reports at and what it proves, and a tally at the foot:

```text
  error    schema-unknown-key             Every key in these files is one the loader reads.
  error    frontmatter-parses             The frontmatter block is present and is a valid YAML mapping.
  error    unknown-key                    Every frontmatter key is a universal field, a type field, or a reserved ADO key.
  warning  deprecated-has-successor       A deprecated tool names what replaces it, or the entry is just a complaint.

83 checks: 61 error(s), 22 warning(s).
```

A **warning** is printed and never fails the build.

### Read the catalogue as data

```bash
kac checks --json
```

One object per check, and nothing else on stdout. The tool's own test suite reads this form, and holds every reachable
check to having a fixture that trips it.

### Find out whether a check already exists

```bash
kac checks | grep -i expiry
```

The catalogue is flat and keyed by id, so a grep over it answers faster than reading `.schema/`.

## Known limits

**A run also compares the catalogue against the rows the generator would write onto a type page**, and a mismatch is
named on stderr and exits `1`. That happens whether or not you asked for `--json`, so a drift there fails this command
even though nothing is wrong with the catalogue itself. Those rows are hand-worded for whoever writes a record, and
several catalogue ids fold into one of them, so the two lists are compared and neither is built from the other.

**A rule with no compiled `expr:` does not appear under its own rule id.** A rule with no severity is an intention, and
the type page renders it under *Declared, not yet enforced*. A rule implemented in C# reports under the check id it
emits instead, which the catalogue does carry.

[Checks](../design/checks.md) is the page for adding a check, and says why a rule is data wherever it can be.
