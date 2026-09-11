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

`checks` prints every check the validator can report against your corpus. Read it to see what CI will hold that corpus
to, and to find out whether the check you were about to add already exists.

`kac` reads the list from that corpus's own `.schema/`, so there is no second catalogue to keep in step. A corpus that
declares a type of its own sees that type's checks here without any change to the tool.
[Checks](../design/checks.md) says where a check comes from.

`checks` opens no record and reports no fault in one. [`validate`](validate.md) runs these against documents.

## Examples

### The catalogue

```bash
kac checks
```

One check to a line, with the severity it reports at and what it proves, and a tally at the foot:

```text
  error    schema-unknown-key             Every key in these files is one the loader reads.
  error    frontmatter-parses             The frontmatter block is present and is a valid YAML mapping.
  error    unknown-key                    Every frontmatter key is a universal field, a type field, or a reserved ADO key.
  warning  deprecated-has-successor       A deprecated tool names what replaces it, or the entry is just a complaint.

103 checks: 74 error(s), 27 warning(s), 2 info.
```

Only an error fails the build. A warning is something to act on. An info reports something true that is nobody's fault,
such as a version your own range holds back.

### The catalogue as data

```bash
kac checks --json
```

One object per check, and nothing else on stdout. The tool's own test suite reads this form, and checks that every
reachable check has a fixture that trips it.

### A search for an existing check

```bash
kac checks | grep -i expiry
```

The catalogue is flat and keyed by id, so a grep over it answers faster than reading `.schema/`:

```text
  warning  expiry                         An active deviation is still inside the review date it carries.
```

## Known limits

**A run also compares the catalogue against the rows the generator would write onto a type page.** `kac` reports a
mismatch on stderr and exits `1`, whether or not you asked for `--json`. A drift there fails this command even though
nothing is wrong with the catalogue itself. Those rows are worded by hand for whoever writes a record, and several
catalogue ids fold into one row, so `kac` compares the two lists and builds neither from the other.

**A rule with no compiled `expr:` does not appear under its own rule id.** A rule with no severity is an intention, and
the type page renders it under **Declared, not yet enforced**. A rule implemented in C# reports under the check id it
emits instead, and the catalogue does list that one.

[Checks](../design/checks.md) is the page for adding a check, and says why a rule is data wherever it can be.
