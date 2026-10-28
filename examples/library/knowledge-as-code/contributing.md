# Contributing

> How to add something to this corpus.

The rules are the same whether a person or an AI session is holding the keyboard. Each question below is answered
somewhere that keeps its answer current.

| To find out                               | Read                                                  |
|-------------------------------------------|-------------------------------------------------------|
| where a record goes                       | [Taxonomy](taxonomy.md), which has the decision table |
| what frontmatter it carries               | [Metadata](metadata.md)                               |
| what this type asks of you                | the type's own page, and the `_template.md` beside it |
| how to write the words                    | `technical-writing`, then `writing-a-record`          |
| what CI will hold it to                   | `kac checks`, run in this corpus                      |
| how a contribution is reviewed and merged | [Contributing][contributing]                          |
| what to do with a finding somebody filed  | the `harvest-findings` skill                          |
| where an unchecked observation goes       | the tracker `.corpus.yaml` names, not a record here   |

Where this corpus adopted `reports`, `writing-a-report` states the four verdicts a person writes into the cells
`kac report` leaves blank.

**The schema outranks all of it.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report, are executable
where a page is not.

## Contributing a record of a type

Type-specific steps live with the type. Each type's page says what that type holds, what it is not, and what it asks of
you when you add one.

Two rules apply to a record of every type, and `kac validate` checks both. Keep the slug to 30 characters at most: the
filename without `.md` and without any number or mnemonic at its head. Write each list field's entries in alphabetical
order. A report's `sources` is the exception, and keeps the order `kac report` writes.

[contributing]: https://paul80nd.github.io/knowledge-as-code/framework/contributing/
