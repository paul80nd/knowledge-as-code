Otherwise-valid records that produce warnings and no error. The golden counts eight warnings, and the run has to exit
0, because only an error fails a build.

* `adrs/0001-warnings.md` has a link definition nothing references, for `unused-definition`, and an alternative with no
  verdict, for `alternatives-verdict`.
* `adrs/0002-list-order.md` has its `tags` out of order, for `list-order`.
* Two ADRs take a branch of `y-statement` each: `0003` runs past the word ceiling, and `0004` lacks two of the six
  moves.
* Two policies take `list-order` on a list of clause numbers. `natr-natural-order.md` sorts `A.8.7` before `A.8.29`
  and passes, and `ordn-ordinal-not-natural.md` sorts them as text and warns. Numbers inside an entry compare as
  numbers.
* `gadgets.yaml` lays a type over the real schema whose `facets` field declares `min-records: 2`, because no real type
  declares it. Three gadgets share `inbound`, which passes. `alone.md` also lists `singular`, which no other gadget
  does, for `min-records`. `emptied.md` writes `facets` with no value, for `empty-optional-key`.

`Warnings.feature` reads this corpus for what `validate` promises about a warning.
