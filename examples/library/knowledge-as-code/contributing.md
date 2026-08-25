# Contributing

> How to add something to this corpus.

The rules are the same whether a person or an AI session is holding the keyboard. Each question below is answered
somewhere that keeps its answer current.

| To find out                               | Read                                                  |
|-------------------------------------------|-------------------------------------------------------|
| where a record goes                       | [Taxonomy](taxonomy.md), which has the decision table |
| what frontmatter it carries               | [Metadata](metadata.md)                               |
| what this type asks of you                | the type's own page, and the `_template.md` beside it |
| how to write the words                    | the `technical-writing` and `writing-a-record` skills |
| what CI will hold it to                   | `kac checks`, run in this corpus                      |
| how a contribution is reviewed and merged | [Contributing][contributing]                          |

**The rules for the words are skills rather than pages.** `technical-writing` carries the floor. `writing-a-record`
carries what this corpus adds and what a record's tier asks on top: the link forms CI enforces, what a `_template.md`
may say, and the constraints each tier brings.

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
* `discoveries/*` has no path rule and merges on a green build.

[contributing]: https://paul80nd.github.io/knowledge-as-code/framework/contributing/
