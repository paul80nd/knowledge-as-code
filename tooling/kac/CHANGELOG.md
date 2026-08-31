# Changelog

> What changed in each published version of `kac`.

This page covers the tool, published to nuget.org as
[`KnowledgeAsCode.Tool`](https://www.nuget.org/packages/KnowledgeAsCode.Tool). The same repository holds the schema, the
framework's documentation and the pages a corpus starts from. Those travel as a template with a version of its own,
which `manifest.yaml` declares and `kac new` stamps into every corpus it creates. A change there is recorded here where
somebody running `kac` can observe it, and nowhere otherwise.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Versions sit below 1.0.0 while the command
surface may still change shape.

A push to `main` publishes whenever `kac.csproj` names a version nuget.org does not already hold, and that publish tags
the commit and opens a release carrying the section for that version. A change lands its entry under `## Unreleased`
first, and whoever owns the branch decides whether it ships now or waits for the rest of what it belongs to.

## Unreleased

### Added

- **A type's `id.width` takes a `min`/`max` span as well as an exact count.** A mnemonic drawn from a concept rather
  than cut to a length can then admit both `std-PR` and `std-SECRET` under one declaration. `kac validate` reports an
  id outside the span as `id-format` and names both ends. An exact `width: 4` behaves as it always has.
- **`filename.carries-id: false` keeps a type's id out of its filenames.** Its records are filed by topic alone, and
  nothing then reads the head of a filename as an id: `id-matches-filename` stays silent, `slug-length` measures the
  whole stem, and a link to the file is a link rather than a citation. `kac validate` refuses the three spans it
  cannot act on. One beside a filename that still carries the id, because `secret-handling.md` would otherwise bind
  to whichever id its first segment happens to spell. One on a `numbered` type, which pads to a single width so that
  ids sort. One whose `min:` sits above its `max:`, which no id can meet.

### Changed

- **Standards take mnemonic ids.** `std-0001` becomes `std-VCS`. A number records the order things were created, and
  a reader meeting one in a control's `verifies:` learns nothing. Filenames are untouched, because a standard is
  already named for its rule area. A corpus that wants its numbered standards back claims
  `.schema/standards.yaml` with a `skip:` entry in `.corpus.yaml`, which stops `kac update` replacing it.
- **`kac generate` heads a table per folder in a type's index.** A type that declares a field with `from: sub-path`
  groups its index rows on the first folder below the type, so a policy folder holding Delivery, Governance,
  Operations and Security reads as four tables instead of one long list. A record filed deeper joins the table its
  first folder heads. A type whose records all sit directly in its folder gets the single table it has always had.
- **The derived column is dropped from a table that repeats it.** Every row under a heading of Security carries
  `security`, so the Category column says nothing there and is left out. A record filed deeper keeps it, because
  `platform/node` under a heading of Platform is the one place `node` is written down. A corpus using no folders at
  all loses a column that was empty in every row.

### Fixed

- **A schema key spelled `no` or `off` now switches its behaviour off.** `on-type-page: no` in `_checks.yaml` read as
  `on-type-page: true`, because only the exact word `false` was taken, and the check was then written onto a type page
  the schema had excused it from. Both spellings of each answer are read.
- **`kac update` finds a seeded record the corpus filed in a sub-folder.** A record's folder sets its category, so a
  corpus files a seeded policy under `policies/governance/` and still holds it. Compared by path, the corpus read as
  holding none, and `update` offered a second copy at the seeded path. Accepting it left two records carrying one id,
  which `kac validate` then failed on `id-unique`. A seed absent from its path is now looked for by the id it carries,
  anywhere under its type's folder, and a match is left alone under `--policy full` as well as under `cautious`. A
  record's relative links are written for the depth it was seeded at, so there is no copy `full` could write a folder
  down that would resolve.

## 0.17.0 - 2026-08-29

### Added

- **A field can be derived from the folder a record sits in.** A type declares `from: sub-path` on a field, and `kac`
  reads its value from the folders between the type's own folder and the file. `policies/security/accs-access-by-identity.md`
  carries `category: security` without a line of frontmatter saying so, and `standards/platform/node/testing.md` carries
  `platform/node`. A record saved straight into its type folder gets an empty value, so you start using categories by
  making a folder. The value reaches the generated index, its sort, and `kac export`.
- **`derived-key` reports a derived field written by hand.** The key is declared, so `unknown-key` admits it and cannot
  say that the value comes from the path. Delete the line, and file the record in the folder you want it to name.
- **`schema-shape` reports a type whose `folder:` is not the name of the file declaring it.** A document's type is
  read from the folder it sits in, and that lookup uses the schema file's name, so the two disagreeing left every
  record of the type unread while `generate` wrote into the folder nobody was reading. The two names now have to
  agree.

### Changed

- **A standard's `axis` field is gone, and `category` replaces it.** Nothing read `axis` but one index column, which
  repeated the folder already shown in each row's link. The composition model stays: the rule-set binding a piece of
  work is the union of the folders that apply to it. Delete `axis:` from every standard.
- **A policy's `category` is read from its folder rather than from its frontmatter.** It is no longer a required enum
  of `security`, `delivery`, `operations` and `governance`. Move each policy into the folder its category named, delete
  the `category:` line, and the exported value is unchanged. The set of folders is now the corpus's own.

### Fixed

- **`kac validate` judges a `#fragment` against the headings alone.** It read a record's frontmatter block as a
  heading, so `fragment-resolves` accepted a link naming an anchor no renderer offers. A link into a record is now
  held to the headings that record carries.

## 0.16.0 - 2026-08-28

### Changed

- **`kac export` carries a field its type declares as a list.** Every such field was written as `null` on every record,
  which reads exactly as a record that holds nothing. A list now travels as a JSON array, and an entry the type
  declares as an object carries the keys that declaration names. A list a record left empty stays `null`, beside the
  field it never wrote. `docs/design/export.md` states the shape.

- **The glossary exports `tags`.** It is the first field to travel as a list. A consumer holding a vendored glossary
  can filter its records by subject without reading each `Scope`. The key lands on `glossary/<record>.json`, and a term
  line in `terms.jsonl` carries no `tags`.

- **`kac new` no longer sends a corpus a link to a type it declined.** A type's root page and its `_template.md` name
  the other types and link to them, which is what makes a full corpus navigable and what left a corpus adopting a subset
  holding dead links. Each page is now unlinked as it is written: a reference to a declined type keeps its own wording
  and loses its link, so `That is a [service](services.md).` arrives as `That is a service.`

  The same happens on `kac update --add-type`, for the page that arrives, and on `kac update --policy full`, which now
  holds a seed to the template as this corpus would have received it rather than as it was authored. Without that a full
  update wrote the links back.

  This reaches the pages a corpus receives once and then owns. A framework document is shared word for word, and
  `framework-names-types` goes on holding it to naming a type rather than linking to one.

  A link into another type's folder has no such repair, because its text names a record. Two seed pages defined one as
  a reference link, which reached a corpus whole, so `glossary.md` and `frameworks.md` now name the record without
  linking it.

- **`kac update --add-type` says what the arriving page does not get.** The new page links to the types the corpus
  holds. The pages already there name it without linking, because each was written while the type was still declined,
  and changing them is the corpus's own call.

- **`kac validate` no longer refuses a schema for naming a type the corpus declined.** A field's `ref:` and a type's
  `versus:` each name a type, and a corpus adopts as many types as it has use for. `.schema/standards.yaml` alone
  reaches four other types, through `ref:` on four fields and a `versus:` naming one of them again, so a corpus adopting
  standards and nothing else met five `schema-dispatch` errors on a schema it had just been sent. Both declarations are
  now left alone where no schema covers the type. Nothing is rendered, and `kac update --add-type` starts the reference
  without an edit to `.schema/`.

  What a record is held to does not soften with it. `ref-resolves` goes on asking that a cited id exists, and a field
  whose every declared type this corpus turned down now admits nothing rather than everything. It names the types the
  declaration wanted, since a type nothing covers has no label to read:
  `'derived-from' points at 'std-0002', which is a Standard. The field points at 'adrs', which this corpus did not
  adopt.`

  With this and the unlinking above, `kac new` adopting any single type writes a corpus that validates and exits 0.

- **`kac update --drop-type` asks before it deletes.** Giving up a type deletes its page and leaves every page still
  naming it holding a dead link. The run says so, says that `kac validate` reports the ones it can reach, and waits for
  an answer. The question takes no by default. `--yes` answers it in advance, and a run with no terminal and no
  `--yes` refuses rather than guessing.

## 0.15.0 - 2026-08-28

### Added

- **`kac validate` says when an import has fallen behind what its source publishes.** `kac restore` keeps the version a
  `consumes:` entry locked for as long as the range still admits it, which is what makes a restore reproducible and is
  also how a corpus sits on a version nobody meant it to sit on. So `validate` asks each source what it holds now, once
  per run, and reports three new checks against `.corpus.yaml`.

  `import-behind` is a **warning**: a newer version sits inside the declared range, and `kac restore` takes it.
  `import-capped` is **information**: a newer version is published and the range holds it back, which is a decision the
  corpus already made. `import-unreachable` is **information** too, for a source this run could not ask, so a lock reads
  as unchecked rather than as current. None of the three fails the build, because failing on somebody else's release
  would turn every downstream red the day a governance corpus ships.

  A source answering with no versions at all reports as unreachable rather than as current, because a registry answers a
  private feed's anonymous reader exactly as it answers a package nobody has published. A corpus with no
  `consumes:` block reads no source and builds no client, and every other check still reads the working tree alone.

- **A third severity, `info`.** `kac validate` counts it in its summary line and in `--json`, where
  `summary.infos` is new, and `kac checks` tallies it apart from the warnings. Neither a warning nor an info changes the
  exit code. A check declares `severity: info` in `.schema/_checks.yaml`. `docs/design/checks.md` covers it.

- **A corpus says who it is, and `pack` and `bundle` stop inventing it.** Four new keys in `.corpus.yaml`:
  `display-name`, `description`, `license` and `author`. `kac export` carries them in a new `about` block, `kac pack`
  writes them into the package a registry lists, and `kac bundle` writes them into the plugin manifest somebody
  installs.

  **A plugin's identity is now generated rather than copied.** `name`, `version`, `displayName`, `description`,
  `author`, `homepage`, `repository`, `license` and `keywords` are all written from the corpus, and a key the corpus
  declared nothing for is removed rather than left standing. `author` is the exception, filed under the corpus's own
  name where it named nobody, because the format asks for one and `claude plugin validate --strict` fails a manifest
  carrying none. `.plugin/.claude-plugin/plugin.json` keeps only what the corpus declares: `metadata.corpusRoot` and
  `metadata.components`, plus any key this tool has never heard of. A manifest copied from a template no longer
  publishes under the template author's name, licence and repository.

  `keywords` are the types the export carried, so a plugin never advertises a type its corpus declined. `kac new`
  writes the four keys bare, because a value supplied there would be inherited rather than chosen.

- **`kac export` names the two keys that address a part.** Each type's manifest entry gains `recordKey` and
  `partKey`, naming which key of a part line says which record it belongs to and which part of that record it is. A type
  names its own keys, so a consumer holding a corpus with a type it never adopted had no way to read them and had to
  assume a spelling. Both are absent where the type keeps no parts, as `partsFile` is. `docs/design/export.md`
  covers it.

- **`kac validate` resolves a reference across a corpus boundary.** A citation carrying a producer's shortcode, as
  `eng:pol-VURM.TIMEBOX`, resolves against the export `kac restore` unpacked under `.imports/`. It is read in prose and
  in a field declaring a `ref:`, so `implements: eng:pol-VURM.TIMEBOX` names one clause rather than a whole policy, and
  both halves are held to existing. Local records and imported ones go through one lookup, so a corpus is not judged
  more loosely for having imported the record it cites.

  Each side keeps its own spelling. A record the reading corpus holds is cited bare, one it imported carries the
  shortcode, and writing either the other way is refused naming the spelling to write.

  A new `import-restored` check fails a corpus declaring an import that is not on disk, and names `kac restore`. Every
  citation into that shortcode then stays quiet, so a run that has not restored reports one line rather than one per
  reference. `docs/cli/validate.md` documents both.

- **`kac restore` fetches the corpora a corpus declares it consumes.** A new `consumes:` block in `.corpus.yaml` names
  each producing corpus, the shortcode it is cited by, the version range it is wanted at and the source it comes from.
  `restore` resolves each range, fetches the package `kac pack` sealed, and unpacks it under `.imports/<shortcode>/`,
  which the template now gitignores. The version each range resolved to is written back onto its own entry, so
  `.corpus.yaml` stays the one description of what a corpus is.

  A `source:` names a registry's service index or a folder of packages. A folder holds the same sealed package a
  registry serves, so a corpus consuming a sibling in its own repository needs no registry, no token and no release. A
  path is relative to the corpus declaring it, as `upstream.url` is.

  A range says `1.2.0` or `^1.2.0` and nothing else, and a caret never takes a prerelease. A lock the range still admits
  is taken without asking the registry, so two restores of an unchanged descriptor write the same bytes. A run says what
  it fetched, at which version, and which corpora were already current.

  A shortcode two entries both claim is refused naming both, as is a corpus two entries both consume, as is a package
  whose own manifest is cited by a different shortcode from the one declared. `KAC_REGISTRY_TOKEN` in the environment
  carries a bearer token for a private feed. `docs/cli/restore.md` documents the verb, and `docs/corpus-descriptor.md`
  the block.

## 0.14.0 - 2026-08-27

### Added

- **`azure-devops-wiki` and a new `azure-devops` target build links.** `kac export` addressed `github` alone. A corpus
  publishing to an Azure DevOps wiki now gets a `?pagePath=` link per record, and one publishing to Azure Repos without
  a wiki gets a `?path=&version=GC<sha>` link. `kac new --publishing azure-devops` accepts the new target and fills its
  base in from a `dev.azure.com` remote, in either the SSH or the HTTPS spelling. A wiki base has to be typed in,
  because a repository's remote says nothing about which wiki publishes it.

  A wiki link is not pinned to a commit, because no `?pagePath=` URL takes one. An agent still reads the version the
  export was built from. `docs/corpus-descriptor.md` sets out both targets.

  The `azure-devops` link form and the anchor an Azure DevOps wiki resolves for a heading carrying punctuation are both
  unconfirmed against a live organisation. The wiki's page path, its anchor parameter and its rejection of a base
  carrying a page id are confirmed.

- **`kac pack` seals an export into a versioned package.** It reads `.dist/export/` and writes one file to
  `.dist/package/`, named for the corpus and its `content-version`. The file is a `.nupkg`, which is a zip carrying a
  small XML manifest a registry reads to name and version it, and both GitHub Packages and Azure DevOps Artifacts store
  one. Everything under `corpus/` inside it is the export, byte for byte, so nothing reading the result needs a NuGet
  client. Two runs over one export produce identical bytes.

  The command refuses a corpus that has not declared `corpus:`, `content-version:` and `shortcode:` in `.corpus.yaml`,
  naming the one that is missing. It publishes nothing: pushing the file is your pipeline's step, and `docs/cli/pack.md`
  carries the command for it.

- **`kac pack --repository <URL>` names where the corpus's source lives.** Some registries read that URL to decide which
  repository a package belongs to, and GitHub Packages refuses a package naming none when the token pushing it is scoped
  to a repository. The element is left out where the flag is not given, because the export states where a record is
  published and that is a different address.

- **A `policy-lookup` skill travels in the plugin.** `kac bundle` ships it beside `glossary-lookup`, and a corpus
  carrying no policies has it trimmed. It reads `policies/clauses.jsonl`, answers from a clause's `level` rather than
  from the modal in its wording, and says which of the four levels it found. What an external framework obliges stayed
  behind with the register that explains it, so the skill names that gap rather than filling it.

- **A component says whether the breadcrumb names it.** `"announce": true` on a manifest entry puts that skill in the
  breadcrumb's last line, and the default leaves it out. The line exists to create a question a session would not think
  to put, so a skill somebody asks for by name does not earn it. A corpus adding a second skill sets `announce`
  on the one worth introducing.

- **`plugin.from` in `.corpus.yaml` reads the plugin tree from one shared folder.** Several corpora in a repository keep
  one copy of the skills and hooks between them instead of a copy each. `kac bundle` merges that tree with the corpus's
  own `.plugin/`, where a file the corpus holds wins, and `kac update` withholds the shared half rather than writing it
  back. The manifest is never taken from the shared tree: it names the plugin, so it stays at
  `.plugin/.claude-plugin/plugin.json` in each corpus. Omit the key and nothing changes. A corpus adopting the key with
  the old copies still on disk has each one reported as a file the template sends nothing to, because a corpus's own
  file wins the merge and a leftover would go on shipping after every upstream change.

- **A corpus created before this declares no component for the new skill.** `kac update` writes the skill, and leaves
  `.plugin/.claude-plugin/plugin.json` alone because the manifest is the corpus's own. A path no component owns ships
  unconditionally, so add the component yourself to have it trimmed where the type is not adopted:

  ```json
  {
    "path": "skills/policy-lookup",
    "requires": [ "policies@2" ],
    "note": "Reads a clause from corpus/policies/clauses.jsonl and the owning policy beside it."
  }
  ```

## 0.13.0 - 2026-08-26

### Added

- **A list field's entries can be objects.** A field declaring `of: object` names its entry's keys in an `entry:` block,
  written with the vocabulary a field is written with. Each key is held to its own `type:`, `pattern:` and `required:`.
  `entry-shape` reports an entry that is not a mapping, and `entry-key` an entry carrying a key the field does not
  declare or missing one it requires.

- **`alignment-rollup` holds a policy's `aligns-with` to its clause table.** Both directions: a binding framework
  reference in an `Alignment` cell and not in the roll-up, and one in the roll-up that no clause cites. The message
  names the reference and the side it is missing from.

- **The roll-up carries the frameworks that bind.** A rule declares `postures:`, naming the standings that oblige a
  summary as the corpus's framework register heads them. A clause may cite a framework filed under any other standing,
  for provenance, and the roll-up leaves it behind. `framework-posture` reports a clause citing a framework the register
  does not place at all, once per framework rather than once per clause.

- **A corpus rule can read the corpus's files.** `CorpusRuleContext` carries the tree, for the rule whose question is
  answered by a page no record links into the graph. A framework register is that case: it holds no frontmatter, so it
  is no record, and it is the only place a standing is written down.

- **`part-ref` reads a part id written beside a link.** `[pol-EVER].BRANCH` cites `pol-EVER.BRANCH`, so a document
  citing six clauses of one policy carries one link definition rather than six. The part id has to sit against the
  closing bracket, so a full stop closing a sentence after a link is still a full stop. A corpus already writing this
  form may see errors it did not before.

### Changed

- **`.corpus.yaml` takes one `base` where it took `human-base` and `raw-base`.** Write the URL a person opens to browse
  the corpus: the GitHub repository with no `/blob` on the end, the Azure Repos `_git` URL, or the wiki's own URL. A
  raw-content host was a GitHub idea that no other target has, and it never served the human case. Edit the
  `publishing:` block by hand: nothing migrates it, and a descriptor still carrying the old keys exports without links.

- **An export's `publishing` block drops `rawTemplate` and carries `base` and `pathPrefix`.** `humanTemplate` stays. An
  agent reading a record's source joins `pathPrefix` ahead of the record's `path` and asks a client that authenticates
  to the target, rather than fetching a bare URL. Only GitHub ever served raw source anonymously, and only for a public
  repository. `formatVersion` moves from 2 to 3, so `kac bundle` and `kac pack` refuse every export built before this.
  Rebuild with `kac export`.

  A record's `links` loses its `raw` half for the same reason. No type's `shapeVersion` moves: that object is written
  for every type by the exporter rather than declared by any one type's `export:` block.

  The `glossary-lookup` and `policy-lookup` skills both tell an agent to fetch the file rather than substitute into a
  template, and to say so plainly where it holds no client for the target.

- **An index column holding a list renders its entries.** A column naming a list field read the value as a scalar and
  wrote an empty cell, so `aligns-with` on a policy index had been blank since the column was added. A column naming a
  list of objects renders what names each entry, which for `aligns-with` is the framework.

- **A policy's `aligns-with` is grouped by framework.** It was a flat list of strings held to an ISO 27001 pattern,
  which is why no other framework could appear in it. Each entry now carries a `framework:` and the `clauses:` reached
  inside it, and any framework may. A corpus holding policies rewrites the field.

- **A clause line carries no `alignment`.** `policies` moves to `export.version: 2`. A framework reference resolves
  through the corpus's own `frameworks.md`, which no consumer receives, so the mapping reached one without what says
  what it is worth.

## 0.12.0 - 2026-08-26

### Fixed

- **`part-ref` reads a citation written as a link.** A corpus cites a part as a code span, and as a link carrying the
  citation as its text or as its label. Only the code span was resolved. A link naming a clause or a term that does not
  exist passed `kac validate`, because a link resolves against a page and the page carries whichever part the citation
  claimed. Every form now reports under `part-ref`, and a link spelling the separator as a colon is reported as one, so
  a corpus using the link form may see errors it did not before.

- **A type index links a record through the category folder holding it.** `kac generate` wrote the filename alone, so a
  record filed under a category below the type's folder was linked as though it sat beside the index. Standards are
  filed that way by declaration, and every link to one was dead.

## 0.11.0 - 2026-08-25

### Added

- **A policy's clauses travel in an export.** `.schema/policies.yaml` declares an `export:` block, so `kac export`
  writes `policies/clauses.jsonl` and one file per policy beside the glossary's. Each clause line carries `level`,
  holding the modal the clause opens with, so a consumer tells a `MUST` from a `COULD` without parsing the words.
  `Purpose` travels as its opening paragraph, and `Scope` and `Exceptions` travel whole.

### Fixed

- **A part's `anchor` is read from where its type takes its parts.** A heading-sourced type carries the part id, which
  is a heading's slug and its anchor alike. A table-sourced type carried that id too, and no fragment resolves to an
  authored clause id. It now carries the slug of the section holding the table, so a link built from a clause line lands
  on the table.

- **A carried section leaves the link reference definitions behind.** They sit in a block at the foot of a record, which
  puts them inside whichever section is written last, so `kac export` joined them onto the end of that section's prose.
  A consumer read a run of paths that nobody sees on the page. A glossary's export is unchanged, because `Scope`
  is never a glossary's last section.

- **`kac export` leaves a clause table in the order its author wrote it.** Every type's parts were sorted on their text,
  which is right for a glossary and wrong for a table grouped by binding level: an advisory clause could reach a
  consumer ahead of the obligations. A heading-sourced type's parts still sort alphabetically.

## 0.10.0 - 2026-08-25

### Added

- **A section can travel cut down.** `export.sections:` in `.schema/<type>.yaml` takes `summary` and `reference`
  alongside `full`. `summary` carries the section's opening paragraph. `reference` carries the key with no words under
  it, leaving a consumer the record's own `path` and `links` to follow. `kac validate` no longer refuses either.

- **The export manifest states the fidelity each section travelled at.** Every entry under `types` carries a
  `sections` object naming its sections and how much of each one travels, so a consumer can tell a cut section from a
  whole one.

### Changed

- **`kac validate` reports a reduced fidelity against `export.parts:` alone.** A part line carries `full`, because
  `line:` already names key by key what of a part travels. A type declaring `summary` or `reference` there is still
  reported as declaring a fidelity nothing carries.

## 0.9.0 - 2026-08-25

### Added

- **A type declares the keys of its own export line.** `export.parts.line:` in `.schema/<type>.yaml` names the keys one
  part writes and the source filling each, drawn from a closed vocabulary covering a part's text, its body, its modal, a
  frontmatter field and a table column. `kac export` reads that declaration and names no key itself, so a second type
  exporting parts costs no code. Glossary is the type that declares one, and its `terms.jsonl` is byte for byte what it
  was.

- **Each type states its own shape version in the export manifest.** `export.version:` in the schema reaches the
  manifest as `shapeVersion` on that type's entry. `formatVersion` covers the envelope alone, so a key added to one
  type's line cannot refuse a consumer reading another.

- **`kac bundle` refuses a component reading a type at a shape the export does not carry.** A `requires` entry may name
  the shape, as `glossary@1`. A bare `glossary` asks for the type and opens none of its files. Either is trimmed, as
  before, where the export carries no such type at all.

- **`kac validate` reports a `line:` that would export nothing.** A key with no source, a source nothing fills, a
  `front.` naming a field no record carries, a `column.` naming a header the type does not declare, and a `part.lead`
  or `part.aside` against a table row are each an error. So is an `export:` block with no `version:`.

### Changed

- **`export.parts:` in a type's schema is a block, and the fidelity moves inside it.** `export.parts: full` becomes
  `export.parts.fidelity: full` with `line:` beside it. `kac validate` reports a type file still carrying the older
  form, naming the fidelity, the `line:` and the `version:` it lacks.

## 0.8.0 - 2026-08-25

### Added

- **A corpus declares the shorthand another corpus cites it by.** `.corpus.yaml` carries a top-level
  `shortcode:`, which is the `eng` in `eng:pol-VURM.TIMEBOX`. `kac validate` refuses a spelling a citation cannot carry,
  and one a type has already taken as its id prefix. `kac export` states the declared shortcode in its manifest, so a
  consumer holding several exports knows which one answers a scoped citation. `kac new` writes the key with no value: a
  shortcode cannot be changed once another corpus has cited it, so it is filled in when one is about to.

### Fixed

- **`kac validate` reads a record whose frontmatter carries a complex key.** A key written as a sequence or a mapping is
  legal YAML and names no field. It was reported as frontmatter that would not parse, which named the wrong fault. It
  now arrives as an empty key, which `unknown-key` reports against the document that wrote it.

## 0.7.0 - 2026-08-24

### Added

- **`kac update` takes a newer framework into a corpus that already has one.** It fetches the template `.corpus.yaml`
  points at, decides file by file what the corpus receives, writes it, and records what it took. Everything it writes
  stays in the working tree and nothing is committed, so `git diff` is the review step.
  [`update`](https://paul80nd.github.io/knowledge-as-code/cli/update/) covers the layers, the flags and what each
  refuses.
- **`kac update --check` reports what would change and writes nothing**, exiting non-zero where anything would. It
  answers in both directions: a framework file the corpus holds differently, and a file the corpus keeps where the
  framework's rules apply that the template sends nothing to.
- **`kac update --add-type` adopts a type, and `--drop-type` gives one up.** Adopting writes the type's schema, root
  page and template, and adds the name to `types:`. Giving one up refuses where the folder still holds records, naming
  the count.
- **`kac update --policy cautious|full` overrides `update-policy:` for one run.** `cautious` writes a seed only where
  the corpus has none. `full` holds every seed to the template and hands the reconciliation to the diff.
- **`update` stamps `upstream.commit` alongside the template version and the date.** A template read from a folder
  resolves no commit, and the key is then left as it stands.

### Removed

- **`kac mechanism` is gone**, and `kac update` replaces both its halves. It compared two corpora on identical paths and
  read a manifest at `tooling/manifest.yaml` that no corpus held, so no corpus could run it against the framework it
  actually took.
- **`role:` in `.corpus.yaml` is no longer written or read.** It said whether a corpus carried the tests that prove the
  tool, and no corpus does. `new` stops writing it, and an `update` over a descriptor still carrying it names the key
  and stops, as it does for any retired key.

### Changed

- **A continuous integration starter is refreshed and never introduced.** `new` writes the starter for the system
  `--ci` named, and an update leaves a starter the corpus does not hold where it is. Which system builds a repository is
  that repository's own answer.

## 0.6.0 - 2026-08-24

### Added

- **`kac new` turns the folder you are standing in into a corpus.** It takes the framework from a template repository at
  a ref, writes what the manifest says a corpus receives, and writes the two files no template can supply:
  `.corpus.yaml` and `README.md`. It then runs `generate`, `validate` and `git add -A`, and stops short of committing.
  [`new`](https://paul80nd.github.io/knowledge-as-code/cli/new/) covers the flags, the defaults and the order it asks
  in.
- **`--from` defaults to the framework's own repository**, and accepts a local path as well as a URL. The template is
  cloned rather than fetched over HTTP, so a repository needing authentication uses the credential helper you already
  have. A local path is the offline escape hatch.
- **`--yes` takes the default for every answer not given.** A run with no terminal and a missing answer exits with an
  error rather than waiting, because a hung pipeline is worse than a failed one.

### Changed

- **A manifest rule may declare `ci:`**, naming the continuous integration system its files serve. `kac new --ci`
  writes the matching starter and no other, so a corpus built by Azure DevOps no longer receives a GitHub Actions
  workflow that would run uninvited.
- **`minimum-tool` in the template manifest moves to `0.6.0`.** A 0.5.0 tool reads that manifest, ignores every `ci:`
  in it, and takes both starters.

## 0.5.0 - 2026-08-24

### Changed

- **`.corpus.yaml` takes a new shape.** `upstream:` now says `path`, `ref`, `commit`, `template-version` and
  `taken-on`, where it said `mechanism-version`, `synced-from` and `synced-on`. `accepted-divergences:` becomes
  `skip:`, and drops `since` and `revisit`. `update-policy:` arrives, defaulting to `cautious`. Every renamed key is
  reported by name, with what to write instead, so nothing is misread in silence; `upstream.synced-from` was dropped
  rather than renamed, and the message says to delete it.
  [The corpus descriptor](https://paul80nd.github.io/knowledge-as-code/corpus-descriptor/) covers the whole file.
- **`mechanism --sync` stamps `upstream.template-version` and `upstream.taken-on`**, where it stamped three keys. It
  leaves `upstream.commit` alone, because a sync reads a directory rather than a git ref and has no commit to record.
- **`mechanism --check` reports a template version**, where it reported a mechanism version. The number has not moved.

### Added

- **A template manifest reads `to:` on a rule**, naming where that rule's files land in a corpus. It replaces the
  pattern's directory prefix, so a template authored in a subdirectory of the repository serving it reaches a corpus's
  own root.
- **A template manifest reads `layer: removed`**, a tombstone naming a file a corpus should delete when it takes a newer
  framework. Nothing acts on it yet: `kac update` is what will.
- **A template manifest reads `minimum-tool`**, the oldest tool that can read it. The template is fetched rather than
  shipped inside the package, so the two version independently.

## 0.4.0 - 2026-08-24

### Changed

- **`kac` finds a corpus by its `.corpus.yaml`**, where it looked for a `.schema/`. It then walks up again from the
  corpus root for the schema to judge that corpus against, so one schema can serve several corpora in one repository. A
  standalone corpus holds both files at its own root and both walks stop there, which is the ordinary case and is
  unchanged. A corpus with no descriptor is no longer found: write one, and
  [the corpus descriptor](https://paul80nd.github.io/knowledge-as-code/corpus-descriptor/) says what goes in it.
- **`kac` names `.corpus.yaml` when it cannot find a corpus**, and reports separately on a corpus with no schema above
  it. The second exits 1 rather than crashing on the first schema file it tries to open.
- **`kac mechanism --help` reads its two option descriptions as sentences.** `--check` closed on a semicolon, and
  `--against` opened on a bare noun phrase. What either flag does has not moved.

## 0.3.0 - 2026-08-23

### Added

- **`--no-color` on every verb.** `NO_COLOR` in the environment asks for the same thing, and the tool already read it.
  Colour goes either way, and bold stays.

### Changed

- **`generate` writes a relative link naming the file**, where it wrote a root-relative link naming the folder. A block
  in `README.md` links `[ADR](adrs.md)`, and one in `knowledge-as-code/taxonomy.md` links `[ADRs](../adrs.md)`. The link
  resolves wherever the corpus sits, rather than only where a renderer maps a folder to the page inside it. Run
  `kac generate` after upgrading: `--check` reports every block carrying the old form until you do.
- **`validate` and `checks` list in aligned columns**, with the severity coloured. Only the message column wraps, so a
  narrow terminal breaks a sentence and never a check id. `checks` splits its count by severity.
- **`generate` marks a file it created**, and counts what it wrote against the size of the whole plan.
- **`export` and `bundle` dim the directory in each path they write**, and colour a remark by whether it is advice or an
  account of the run. Neither changes a word it prints.
- **A failure is red on stderr.** That covers every verb's hard stop, and the heading over a list of what stopped it.
  What the heading names stays plain beneath it.
- **`--json` and every exit code answer as before.** `--json` goes straight to the stream and never carries colour,
  whatever the terminal.
- **Two messages lose a semicolon the house style does not keep.** The `filename / slug-length` row in every generated
  checks table, and the meta-test reporting an over-long description. Run `kac generate` after upgrading: `--check`
  reports every type page carrying the old wording until you do.

## 0.2.1 - 2026-08-21

### Changed

- **The command line is parsed by `Spectre.Console.Cli` rather than `System.CommandLine`.** Every verb, option and exit
  code answers as it did. `--help` reflows into Spectre's layout, `-v` joins `--version`, and `-?` no longer stands for
  `--help`. The tool carries one library for reading a command line and asking a question, rather than two.

## 0.2.0 - 2026-08-20

### Changed

- **`kac index` is now `kac generate`.** The command writes each type's `_index.md` and rewrites the generated blocks in
  every type page, and only the first of those is an index. `--check` is unchanged, and so is everything either half
  writes. There is no alias: a pipeline or script still naming `index` fails until it names `generate`.

## 0.1.1 - 2026-08-20

### Added

- An icon on the nuget.org package page.
- A link from the package page to the release notes for the version being installed.

The tool answers exactly as 0.1.0 does. Only what nuget.org shows about it changed.

## 0.1.0 - 2026-08-20

The first published version.

### Added

- `kac validate` holds a corpus to the schema it carries: frontmatter, identity, structure, clauses, links, the graph
  and the type setup.
- `kac index` regenerates `_index.md` and the generated blocks in each type page. `--check` reports what is stale rather
  than writing it.
- `kac checks` lists every check the validator implements, read from the schema rather than from a list in the tool.
- `kac export` writes the corpus to `.dist/export/` as data a consumer reads instead of cloning.
- `kac bundle` assembles that export and `.plugin/` into an installable plugin.
- `kac mechanism` compares the shared layers against a reference corpus, or takes them from one.
