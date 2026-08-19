A corpus that declares its types, and disagrees with itself three ways.

`.corpus.yaml` adopts `runbooks` and `widgets`. The corpus holds neither, and holds `adrs`, which it has not
adopted.

* **`runbooks` is adopted and not stood up** — the state a sync exists to resolve, so it reads as work outstanding.
* **`widgets` is adopted and no schema covers it** — the descriptor names a type the corpus was never given.
* **`adrs` is stood up and not adopted** — every generated list of types would leave it out while the corpus plainly
  holds it, which is how a corpus drifts back to being described by its folders.

The findings land against `.corpus.yaml` rather than against `.schema/`: what is wrong is the declaration, and the
file holding it is the one a corpus owner edits.

Every other type the schema declares is absent and undeclared, which is silent — that is a corpus growing into the
schema one type at a time, and it is the state `type-setup` has always had to stay quiet about.
