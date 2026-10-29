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
| `example-libraries` | `0.31.0` |
| `example-engineering` | `0.45.1` |
| `example-payments` | `0.41.1` |
| `example-dogfooding` | `0.91.4` |

Built from [`8ea1b5a5f9f175b28a13b15fe1ccd03249bf34af`](https://github.com/paul80nd/knowledge-as-code/commit/8ea1b5a5f9f175b28a13b15fe1ccd03249bf34af).
