# Security

## Reporting a vulnerability

Report privately through
[GitHub security advisories](https://github.com/paul80nd/knowledge-as-code/security/advisories/new). The report stays
private until there is a fix, and you get a thread to answer questions in.

Do not open a public issue for a suspected vulnerability. Anything that is merely a bug — including one that looks
alarming — is a normal issue.

This repository has one maintainer and nothing running behind it. The maintainer acknowledges a report on seeing it,
usually within a week. There is no service level and no bounty.

## In scope

* **`tooling/`** — the `kac` tool. It reads a repository and writes generated files back into it. The failures that
  matter are ones where it writes outside the corpus, or where a crafted `.schema/` file or record frontmatter makes it
  act outside what its caller asked for.
* **`.github/workflows/`** — CI, which runs the tool over the branch under test.
* **`.schema/`** — the contract the tool enforces, and the copy of it every corpus carries. A schema that makes the
  tool skip a check it declares is a real finding.

## Out of scope

* **The example records.** Example Libraries is fictional and `example.com` is reserved by RFC 2606. A hostname or
  address found there is neither leaked nor live.
* **A corpus built from a copy of this framework.** A copy is its own repository under its own control. Report to
  whoever runs it.
* **Rendering in the published wiki.** That belongs to the publishing target, not to this repository.
* **Scanner output with no demonstrated path to impact.**

CI executes untrusted code by design: `dotnet run --project ../tooling/kac` runs a pull request's own source. What
contains it is the `pull_request` trigger rather than `pull_request_target`, a read-only `contents` permission, and no
secrets reachable from the job. A report that this is dangerous needs to show an escape from that containment.

## Versions

Fixes land on `main`. There is no supported-version matrix, and only the newest version of the tool is fixed.

The two halves reach you differently. `kac` is published as
[`KnowledgeAsCode.Tool`](https://www.nuget.org/packages/KnowledgeAsCode.Tool), so a fix to the tool arrives as a new
version you install. The schema and the framework's documentation are copied rather than depended on, so a fix to those
reaches a corpus only when someone resyncs its copy. Watch this repository. There is no other notification channel.
