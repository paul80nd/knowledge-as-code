Two ways a declared type can be wrongly stood up, and nothing else.

* `tools/` exists with no `tools.md` and no `tools/_template.md` — stood up and incomplete.
* `data.md` exists with no `data/` — the same fault from the other side.

Every other type the schema declares is absent entirely, which is the valid state this check must stay silent on: a
corpus holding the whole schema and growing into it one type at a time.
