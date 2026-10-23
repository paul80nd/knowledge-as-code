What `kac new` writes into an empty folder, taking this repository as its template. The fixture has no corpus, because
the command it runs creates one.

The runner calls `kac new --yes --name acme-corpus --ci github` and checks three things.

* **`expected-tree.txt`** is every path in the new corpus. `.github/workflows/kac.yml` is in it and
  `azure-pipelines.yml` is not, which is `--ci github` narrowing what lands.
* **`expected-descriptor.yaml`** is the `.corpus.yaml` it composed, with every type adopted. The repository's path and
  today's date are written as `<repository>` and `<today>`, because neither can be committed.
* **The exit code is 0.** `new` runs `validate` before it returns, so this says the corpus it created validates.

File bodies are not compared. They are the template's, and the template has tests of its own. A change to
`manifest.yaml` or to a file the template sends moves this fixture, so regenerate it with `KAC_UPDATE_GOLDENS=1` and
read the diff.

`Creation.feature` covers what `new` refuses, and the other behaviours its page promises.
