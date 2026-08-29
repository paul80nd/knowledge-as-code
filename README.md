# knowledge-as-code — the plugin marketplace

Generated. Every file here is built by [`publish-plugin.yml`][workflow] on each push to
`main` and replaced whole on the next one. Nothing here is edited by hand, and this branch
is never merged into `main`.

[workflow]: https://github.com/paul80nd/knowledge-as-code/blob/main/.github/workflows/publish-plugin.yml

The source is on [`main`](https://github.com/paul80nd/knowledge-as-code).

## What these are

Each plugin here carries one of the **worked example corpora** that ship with knowledge-as-code.
The estates they describe are invented, they govern nobody, and they are here so that somebody
deciding whether to adopt the framework can install one and ask it questions. Each plugin's own
description says so.

## Installing

```
/plugin marketplace add paul80nd/knowledge-as-code@marketplace
/plugin install <plugin>@knowledge-as-code
```

## This build

| Plugin | Corpus content version |
|---|---|
| `example-libraries` | `0.1.1` |
| `example-engineering` | `0.2.0` |

Built from [`d071c2dbedb3ecb1b91a81f56d47d68c94deb822`](https://github.com/paul80nd/knowledge-as-code/commit/d071c2dbedb3ecb1b91a81f56d47d68c94deb822).
