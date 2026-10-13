# Imports

A corpus that lists another under `consumes:` cites that corpus's records without keeping a copy of them. A corpus is
one repository of knowledge records kept in git, and an import is the export of another corpus, restored under
`.imports/`. This page lists what `kac` is built to do with an import, and the scenario proving each behaviour.
[Imports](../use/imports.md) owns the full statement of each one, and [Pack and restore](packages.md) owns how a
package is fetched.

## What it does

* `restore` unpacks each `consumes:` entry under `.imports/<shortcode>/`.
  [The round trip](../use/imports.md#the-round-trip)
* `validate` resolves every citation against this corpus's records and the restored imports together.
  [The round trip](../use/imports.md#the-round-trip)
* A bare id names a record this corpus holds, and an imported one carries its producer's shortcode.
  [Spelling a citation](../use/imports.md#spelling-a-citation)
* A record arriving through a chain keeps the shortcode of the corpus that wrote it.
  [Spelling a citation](../use/imports.md#spelling-a-citation)
* A check reads an imported record's id, type, path and parts, and nothing else.
  [What a check may ask](../use/imports.md#what-a-check-may-ask-across-the-boundary)
* An export includes what its corpus consumes, at any depth, and so does a package built from it.
  [What an export inherits](../use/imports.md#what-an-export-inherits)
* `export.exclude` withholds this corpus's own records alone, so inherited records travel whole.
  [What an export inherits](../use/imports.md#what-an-export-inherits)
* A report covers the clauses a corpus imports beside its own. [`report`](../use/cli/report.md)

## What it refuses

* `validate` fails a declared import whose folder holds no export, in one finding naming `kac restore`.
  [A missing restore](../use/imports.md#a-missing-restore)
* `validate` fails a citation spelt the wrong way round, and the message gives the spelling to write.
  [Spelling a citation](../use/imports.md#spelling-a-citation)
* `export` and `report` refuse a consumed corpus with nothing restored. [Export](export.md)

## What it promises a consumer

* A version behind is never an error. A newer version inside the range is `import-behind`, a warning. One outside it is
  `import-capped`, and a source that could not be asked is `import-unreachable`, each an info.
  [An import that has fallen behind](../use/imports.md#an-import-that-has-fallen-behind)
* An imported record is never held to this corpus's rules.
  [What a check may ask](../use/imports.md#what-a-check-may-ask-across-the-boundary)

## Specified by

[`Imports.feature`][Imports.feature]:

* A citation resolves into a restored import, and is checked against what it carries
* A citation spelt the wrong way round fails with the spelling to write
* A record arriving through a chain resolves under the shortcode of the corpus that wrote it
* A declared import with nothing restored fails in one finding naming kac restore
* A citation into an import with nothing restored reports nothing of its own
* An import behind its source is a warning, and one capped or unreachable is information
* An imported record is held to none of this corpus's rules

[`Restore.feature`][Restore.feature]:

* A first restore unpacks the corpus and locks its version

[`Export.feature`][Export.feature]:

* What the corpus consumes travels under the shortcode of the corpus that wrote it
* export.exclude withholds this corpus's own records alone
* An export refuses a consumed corpus that has not been restored

[`Pack.feature`][Pack.feature]:

* A package includes what its corpus consumes

[`Report.feature`][Report.feature]:

* A corpus that imports policies reports on them, and names this corpus first among its sources
* A report refuses a consumed corpus that has not been restored

## Where to go next

[Imports](../use/imports.md) follows a citation from the producer's export to the consumer's build.

[Imports.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Imports.feature
[Restore.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Restore.feature
[Export.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Export.feature
[Pack.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Pack.feature
[Report.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Report.feature
