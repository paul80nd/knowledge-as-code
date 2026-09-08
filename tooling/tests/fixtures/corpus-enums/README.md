A corpus whose `platform` range is its own, and a record carrying a value that range does not hold.

`.schema/services.yaml` declares `values: $corpus.platform`, which states no values and sends the check to the
corpus. `.corpus.yaml` here holds `dotnet-api` and `dotnet-web`, and the record carries `azure-function`.

The finding is an ordinary `enum`, and that is the point. From the record's side the fault is a value outside the
declared range, whoever declared it, so the id an author already knows is the id they meet. What moves is the list
the message quotes back: these two values are the descriptor's rather than the schema's.

[corpus-enums-undeclared](../corpus-enums-undeclared/README.md) is the other half, where the corpus wrote no list
at all.
