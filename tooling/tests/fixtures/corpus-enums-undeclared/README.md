A corpus holding a service, and stating no range for the `platform` that service carries.

`.schema/services.yaml` declares `values: $corpus.platform`, so the values are the estate's to write under `enums:`
in `.corpus.yaml`. This corpus wrote none, and `corpus-enum-undeclared` reports it.

The finding lands against `.corpus.yaml` and is reported once. Nobody who wrote a record can fix it, and a corpus
with thirty services would otherwise meet thirty copies of one setup mistake, each pointing at the wrong file. The
record here carries a value any reasonable list would hold, so nothing else is reported about it.

The question arrives with the first record rather than with the folder. A corpus that has stood services up and
written none has no estate to derive a list from, which is every corpus on the day it is created.

[corpus-enums](../corpus-enums/README.md) is the other half, where the corpus wrote a list and a record left it.
