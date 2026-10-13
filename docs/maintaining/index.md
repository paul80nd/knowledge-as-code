# Maintaining `kac`

This section is for the maintainer: whoever builds, releases or forks `kac`, and needs to know what it is specified to
do. A corpus is one repository of knowledge records kept in git. A check is one fault `kac validate` can report against
a corpus.

## The pages here

* **[The documentation charter](charter.md)** says which reader each section of this site serves, which kinds of page
  each section may contain, and where the argument behind a change is written.
* **[Checks in C#](checks.md)** says when a rule needs C#, how to add a fact to the expression grammar, and why a rule
  is data wherever it can be.
* **[Adding a report](reports.md)** is the order to follow when you add a report to `kac report`.
* **[The plugin bundle](plugin.md)** says how `kac bundle` lays out what it writes, what the breadcrumb hook says, and
  how the plugin manifest is edited.

## Where to go next

[`tooling/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/README.md) covers building `kac`
and its test layers, beside the code.
