A corpus whose stated range carries a value no record can satisfy.

`.corpus.yaml` holds `platform: [Dotnet-Web]`. An enum value is lower case, because it is a grep target first and
prose second, so `enum-lowercase` refuses the capitalised spelling in a record and `enum` refuses every other one.
The range is unsatisfiable from both sides.

`corpus-enum-undeclared` reports it against `.corpus.yaml`, which is the file to edit. This is the second fault the
id carries, and it is the same fault as a list nobody wrote: no record can satisfy either.

The record's own `enum` finding stands beside it rather than being suppressed. The record genuinely does not match
the range as written, and hiding that would leave an author correcting the descriptor with no sign of what it broke.

[corpus-enums-undeclared](../corpus-enums-undeclared/README.md) carries the other fault under this id.
