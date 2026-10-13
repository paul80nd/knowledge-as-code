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
| `example-libraries` | `0.20.3` |
| `example-engineering` | `0.28.2` |
| `example-payments` | `0.30.0` |
| `example-dogfooding` | `0.59.0` |

Built from [`6df1b43ad493f1cc6c54d66bd855c8d6c36af706`](https://github.com/paul80nd/knowledge-as-code/commit/6df1b43ad493f1cc6c54d66bd855c8d6c36af706).
