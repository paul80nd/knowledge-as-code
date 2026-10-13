Feature: What `validate` makes of an import
`validate` resolves a citation against this corpus's records and the imports restored under `.imports/` together, and
asks each import's source what it now publishes. docs/use/imports.md states each behaviour below. Driven in-process
against the export-inherited fixture for a restored import, the imports fixture for a missing one, and the
import-versions fixture for an import that has fallen behind.

Scenario: A citation resolves into a restored import, and is checked against what it carries
  Given the export-inherited fixture corpus
  And "glossary/local.md" ends with "Shelved from `eng:gls-shared.record` under `eng:pol-KEEP.HOLD`, never `eng:gls-shared.ledger`."
  When I validate the corpus
  Then the findings for "glossary/local.md" are exactly:
    | line | check    | message                                                                     |
    | 29   | part-ref | 'eng:gls-shared.ledger' cites a term 'ledger' that eng:gls-shared does not carry. |

Scenario Outline: A citation spelt the wrong way round fails with the spelling to write
  Given the export-inherited fixture corpus
  And "glossary/local.md" ends with "Cited as `<citation>`."
  When I validate the corpus
  Then the findings for "glossary/local.md" are exactly:
    | line | check    | message   |
    | 29   | part-ref | <message> |

Examples:
  | citation            | message                                                                                                                                         |
  | gls-shared.record   | 'gls-shared.record' names a record this corpus imports rather than holds. Write it as 'eng:gls-shared.record', so a reader can see which corpus owns it. |
  | eng:gls-local.shelf | 'eng:gls-local.shelf' scopes a record this corpus holds itself. Write it as 'gls-local.shelf': two spellings of one id defeat every search anybody runs for it. |

Scenario: A record arriving through a chain resolves under the shortcode of the corpus that wrote it
  Given the export-inherited fixture corpus
  And "glossary/local.md" ends with "Kept in `gp:gls-old.store`, never `eng:gls-old.store`."
  When I validate the corpus
  Then the findings for "glossary/local.md" are exactly:
    | line | check    | message                                                     |
    | 29   | part-ref | 'eng:gls-old.store' cites 'eng:gls-old', which does not exist. |

Scenario: A declared import with nothing restored fails in one finding naming kac restore
  Given the imports fixture corpus
  When I validate the corpus
  Then the findings are exactly:
    | file         | line | check           | message                                                                               |
    | .corpus.yaml |      | import-restored | `consumes:` declares 'eng:' and .imports/eng/ holds no export. Run `kac restore`. |

Scenario: A citation into an import with nothing restored reports nothing of its own
  Given the export-inherited fixture corpus
  And ".imports/eng" is deleted
  And "glossary/local.md" ends with "Shelved from `eng:gls-shared.record`, never `eng:gls-shared.ledger`."
  When I validate the corpus
  Then the findings for ".corpus.yaml" are exactly:
    | line | check           | message                                                                               |
    |      | import-restored | `consumes:` declares 'eng:' and .imports/eng/ holds no export. Run `kac restore`. |
  And no findings are reported for "glossary/local.md"

Scenario: An import behind its source is a warning, and one capped or unreachable is information
  Given the import-versions fixture corpus
  When I validate the corpus against its sources
  Then the findings for ".corpus.yaml" are exactly:
    | severity | line | check              | message                                                                                                                                                                                                         |
    | error    |      | import-restored    | `consumes:` declares 'eng:' and .imports/eng/ holds no export. Run `kac restore`.                                                                                                                               |
    | error    |      | import-restored    | `consumes:` declares 'sec:' and .imports/sec/ holds no export. Run `kac restore`.                                                                                                                               |
    | error    |      | import-restored    | `consumes:` declares 'ops:' and .imports/ops/ holds no export. Run `kac restore`.                                                                                                                               |
    | warning  |      | import-behind      | 'eng:' is locked at 0.1.1 and 0.1.2 is published. '^0.1.0' admits it, so `kac restore` takes it.                                                                                                                |
    | info     |      | import-capped      | 'sec:' is locked at 0.1.1 and 0.2.0 is published. '^0.1.0' holds it back.                                                                                                                                       |
    | info     |      | import-unreachable | could not check what 'ops:' publishes, so 0.1.1 is unchecked rather than current. there is no folder at nowhere. `source:` names either a registry's service index or a folder holding the packages a producer built. |

Scenario: An imported record is held to none of this corpus's rules
  Given the export-inherited fixture corpus
  And ".imports/eng/glossary/gls-shared.json" states "fields.status" as "nonsense"
  And ".imports/eng/glossary/gls-shared.json" states "fields.review-by" as "2001-01-01"
  And ".imports/eng/glossary/gls-shared.json" states "fields.title" as null
  And ".imports/eng/glossary/gls-shared.json" states "sections" as {}
  And "glossary/local.md" ends with "Shelved from `eng:gls-shared.record`."
  When I validate the corpus
  Then no findings are reported for "glossary/local.md"
  And no finding is reported under ".imports"
