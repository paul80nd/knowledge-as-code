Three ways a declared type can be wrongly stood up, and nothing else.

* `tools/` exists with no `tools.md` and no `tools/_template.md` — stood up and incomplete.
* `data.md` exists with no `data/` — the same fault from the other side.
* `trinkets/` exists at all, and `trinkets` is a single-document type whose page *is* the document.

Every other type the schema declares is absent entirely, which is the valid state this check must stay silent on: a
corpus holding the whole schema and growing into it one type at a time.

`trinkets` is the fixture's own type, laid over the real schema the way `schema-declarations` lays `widgets.yaml` over
it. No type the framework declares is single-document, so the shape needs a type of its own to be tested against.
