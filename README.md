# knowledge-as-code — the plugin marketplace

Generated. Every file here is built by [`publish-plugin.yml`][workflow] on each push to
`main` and replaced whole on the next one. Nothing here is edited by hand, and this branch
is never merged into `main`.

[workflow]: https://github.com/paul80nd/knowledge-as-code/blob/main/.github/workflows/publish-plugin.yml

The source is on [`main`](https://github.com/paul80nd/knowledge-as-code).

## What these are

Each plugin here carries one of the **worked corpora** that ship with knowledge-as-code. Install
one and ask it questions. The estates they describe are invented and govern nobody, except
`example-dogfooding`, which describes the repository they all come from.

## Installing

```
/plugin marketplace add paul80nd/knowledge-as-code@marketplace
/plugin install <plugin>@knowledge-as-code
```

## This build

| Plugin | Corpus content version |
|---|---|
| `example-libraries` | `0.2.2` |
| `example-engineering` | `0.16.0` |
| `example-payments` | `0.10.0` |
| `example-dogfooding` | `0.22.3` |

Built from [`c17b029c9ea65a73e6fceff42cb83c28add3b381`](https://github.com/paul80nd/knowledge-as-code/commit/c17b029c9ea65a73e6fceff42cb83c28add3b381).
