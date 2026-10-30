The committed output of `kac generate` over a corpus with one type, which is the golden for every generated block.

The corpus stands up `adrs` alone and holds two ADRs, one accepted and one proposed. The run asserts
`generate --check` finds every generated file fresh. A change to a generator fails here until an update rewrites the
files under `corpus/`, and the diff is the change a corpus would see.

* `adrs/_index.md` pins the index tables. The accepted ADR leads, with its `decided-on` cell filled. The proposed one
  follows under `## Not in force`, with that cell empty.
* `adrs.md` pins the `schema-adrs` and `checks-adrs` blocks spliced into the type page.
* `README.md` pins `types-index`, which lists the one type the corpus holds and none of the others the schema declares.
* The pages under `knowledge-as-code/` pin the framework's blocks: `checks-universal` and `types-metadata` in
  `metadata.md`, the placement, detail, versus and graph blocks in `taxonomy.md`, and the lineage and collisions blocks
  in `lineage.md`.

The `README.md` under `corpus/` is the corpus's own page. This file sits outside `corpus/`, so `kac` never reads it.

`generate-stale` copies this corpus and drops a row. `Generate.feature` reads it for what `generate` promises.
