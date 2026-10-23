Records that cite each other, for the checks that read more than one record at once.

**The ADRs** take the reference rules. `0001-first.md` carries most of the faults: a `related` id with no matching
link, an id that does not exist, a supersession `0002` does not return, a link and a fragment that do not resolve, a
label with no definition, and a bracketed phrase with no definition. `0003` reuses `0002`'s id, so `id-unique` lands
beside `id-matches-filename`, because a duplicate id in a file with another number cannot match its filename.
`0004` and `0005` point `superseded-by` at an id that does not exist and at a record of the wrong type.

`0007` supersedes `0006`, `0006` names it in `superseded-by`, and neither reports anything. Every other supersession
here is written to fail, so this pair is the only passing run of `reciprocal` and of
`required-when: status == superseded`.

**The gizmos** lay a type over the real schema, in `.schema/gizmos.yaml`, with two mirrored fields.

* `depends-on` mirrors `## Dependencies`. `mirrored.md` agrees with its section, and `adrift.md` does not, so this is
  the one place a mirrored section other than `Related` is exercised.
* `covers` mirrors the citations its `Covers` lines gather, for `mirrors-citations`. `covering.md` agrees and also
  cites an ADR the field could never list, which passes. `drifting.md` disagrees in both directions, puts a line in
  the middle of a section, and writes a line naming nothing. `unitalic.md` puts a space before the closing `_`, so
  its line does not render as italic, and its citation still counts. `named-whole.md` and `wholesale.md` name
  `giz-behaving` whole where the field names one behaviour.

`Graph.feature` reads this corpus for what `validate` promises about references.
