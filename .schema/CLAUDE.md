# Changing the schema

[`README.md`](README.md) is the reference for the keys. This is what will bite you.

* **A key you write may do nothing.** The loader accepts declarations the validator never dispatches — unimplemented
  rule ids, a `ref:` at a type no schema covers, `values:` on a list field. Before relying on a key, find it in
  `.tooling/kac.core/Schema.cs`, then find the code that reads what it parsed into. Finding it parsed is not enough.
  The open question in `README.md` tracks this.

* **Run `./kac index` after any change.** Every type page carries generated `schema-<type>` and `checks-<type>`
  blocks derived from these files, so a schema edit alone leaves the corpus stale and fails `index --check` in CI.

* **The test fixtures validate against these files**, not against copies. A change here can move golden expectations
  in `.tooling/tests/fixtures/`, so run `dotnet run .tooling/kac-tests.cs` as well as `./kac validate`.

* **Field order is load-bearing.** `key-order` requires a document's frontmatter to be a topological extension of the
  universal order followed by the type's. Reordering fields here can invalidate documents that were correct, and the
  failure surfaces in the corpus rather than here.

* **Templates do not follow.** Nothing generates `<type>/template.md` and nothing validates it, so a field added here
  has to be added there by hand or every document copied from it will be wrong.
