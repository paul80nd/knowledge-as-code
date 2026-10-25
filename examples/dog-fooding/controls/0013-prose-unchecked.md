---
id: ctl-0013
type: control
tier: normative
status: active
verifies: [ std-PROSE.DIFFERS ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-19"
owner: human:paul.law
tags: [ documentation, prose, writing ]
---

# Nothing checks prose against the house style

`Control: ctl-0013` `ACTIVE`

No check fails a Markdown file with an em dash, a line over 120 characters, or an American spelling.

## What it checks

* [std-PROSE.DIFFERS] says an em dash "**MUST NOT** appear".
* It says prose "**MUST** wrap at 120 columns, counted in characters".
* It says spelling "**MUST** be British".

## How it works

Nothing runs. The checks these rules need are one job over the Markdown lines a pull request adds:

* `grep -nP '\x{2014}'` fails on an em dash.
* A line-length check fails a line over 120 characters. It counts characters and not bytes, and skips table rows, link
  definitions, fenced blocks and frontmatter.
* A spell checker set to British English, such as cspell with `language: en-GB`, fails a word its dictionary does not
  know.

## Coverage and gaps

A reviewer reading the diff is the only thing that notices any of these today.

The checks would read Markdown, so an em dash in a workflow message or a YAML comment passes them, and
`.github/dependabot.yml` has one. They read no commit message and no pull request body, which
[std-PROSE.COMMITS] governs. Most of the other rules ask for a judgement: one idea per
sentence, a literal verb, no figure of speech. A pattern finds the words and not the meaning.

One rule has a check elsewhere. `clause-quoted-faithfully`, in the `kac validate` run [ctl-0007] describes, reports a
quotation that no longer matches its clause.

[ctl-0007]: 0007-corpus-validation.md
[std-PROSE.COMMITS]: ../standards/prose.md#commits-commit-messages-and-pull-requests
[std-PROSE.DIFFERS]: ../standards/prose.md#differs-where-this-repository-differs-from-microsoft
