# Maintaining `kac`

This section is for the maintainer: whoever builds, releases or forks `kac`, and needs to know what it is specified to
do. A corpus is one repository of knowledge records kept in git. A check is one fault `kac validate` can report against
a corpus.

## Start here

Read these in order to go from a clone to a published fork.

* **[Architecture](architecture.md)** says how the entrypoint, `kac.core` and the schema fit together, and the order a
  command runs in.
* **[Build, test and release](build-test-release.md)** says how to build `kac`, run its unit, feature and golden
  test layers, and publish a version.
* **[Forking](forking.md)** lists every identifier and secret a fork replaces before it can publish the tool, the
  corpus packages, the plugin or the site.
* **[Contributing](contributing.md)** says how a change is proposed, what a pull request owes, and which checks gate it.

## Changing a part of `kac`

* **[The documentation charter](charter.md)** says which reader each section of this site serves, which kinds of page
  each section may contain, and where the argument behind a change is written.
* **[Checks in C#](checks.md)** says when a rule needs C#, how to add a fact to the expression grammar, and why a rule
  is data wherever it can be.
* **[Adding a report](reports.md)** is the order to follow when you add a report to `kac report`.
* **[The plugin bundle](plugin.md)** says how `kac bundle` lays out what it writes, what the breadcrumb hook says, and
  how the plugin manifest is edited.

## Where to go next

[Architecture](architecture.md) is the first page to read before you change `kac`.
