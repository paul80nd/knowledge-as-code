# Contributing

> How to add something to this corpus.

The rules are the same whether a person or an AI session is holding the keyboard. Each question below is answered
somewhere that keeps its answer current.

| To find out                               | Read                                                  |
|-------------------------------------------|-------------------------------------------------------|
| where a record goes                       | [Taxonomy](taxonomy.md), which has the decision table |
| what frontmatter it carries               | [Metadata](metadata.md)                               |
| what this type asks of you                | the type's own page, and the `_template.md` beside it |
| how to write the words                    | the writing skills, which the paragraph below lists   |
| what CI will hold it to                   | `kac checks`, run in this corpus                      |
| how a contribution is reviewed and merged | [Contributing][contributing]                          |
| what to do with a finding somebody filed  | the `harvest-findings` skill, described below         |
| where an unchecked observation goes       | the tracker `.corpus.yaml` names, not a record here   |

**The rules for the words are skills rather than pages.** `technical-writing` states the floor, and every surface
answers to it. `writing-a-record` states what this corpus adds and what a record's tier asks on top: the link forms CI
enforces, what a `_template.md` may say, and the constraints each tier brings. Where this corpus adopted `reports`,
`writing-a-report` sits on both and states the four verdicts a person writes into the cells `kac report` leaves
blank.

**`harvest-findings` is about the backlog rather than the words.** An agent holding this corpus as a plugin (a
read-only copy it installs) files what it noticed as an issue. That skill reads those issues, sorts each one into a
route, and drafts the record a routed one asks for as a pull request. Run it here, where the records are.

**The schema outranks all of it.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report, are executable
where a page is not.

## Contributing a record of a type

Type-specific steps live with the type. Each type's page says what that type holds, what it is not, and what it asks of
you when you add one. Where you are not sure which type you need, [Taxonomy](taxonomy.md) has the decision table.

## Branches and review

Trunk-based. Short-lived branches, a pull request into `main`, and the wiki publishes from `main`.

**The policy below is a starting point rather than a rule of the framework.** The tier model is what a change to it
should follow.

* Minimum one reviewer.
* Build validation required. Schema, links and generated-content freshness must pass.
* Path-scoped automatically-included reviewers raise the bar on Decided and Normative content (`adrs/*`, `standards/*`,
  `policies/*`) without raising it everywhere. This is how the tier model is enforced in practice. Azure DevOps sets a
  minimum reviewer count per branch and scopes required reviewers per path, which is the pairing this needs.
* Every other path has no rule and merges on a green build.

[contributing]: https://paul80nd.github.io/knowledge-as-code/framework/contributing/
