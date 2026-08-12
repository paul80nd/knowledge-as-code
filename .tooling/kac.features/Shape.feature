Feature: Corpus shape checks
  Beyond the records themselves, kac asks whether the corpus has the shape its schema declares: every type
  stood up as both a page and a folder, and every page carrying a generated block still holding the markers
  to write between. Driven in-process against the type-setup and type-pages fixtures — the same corpora
  their JSON goldens pin.

  Scenario: A type is stood up as both a page and a folder, or as neither
    Given the type-setup fixture corpus
    When I validate the corpus
    Then validation reports 0 documents and 0 skipped
    And the findings are exactly:
      | file                 | line | check      | message                                                                                    |
      | .schema/data.yaml    |      | type-setup | type 'data' has data.md but no 'data/' — a type is set up as both or neither.               |
      | .schema/tools.yaml   |      | type-setup | type 'tools' has a 'tools/' folder but is not fully set up — add tools.md, tools/_template.md. |

  Scenario: A collection's page is checked for its generated markers and its links
    Given the type-pages fixture corpus
    When I validate the corpus
    Then validation reports 0 documents and 0 skipped
    And the findings are exactly:
      | file                          | line | check                 | message                                                                                                                                                                                               |
      | adrs.md                       |      | generated-block       | the 'schema-adrs' block is missing its BEGIN marker — `kac index` writes between them and leaves the page alone without both.                                                                         |
      | adrs.md                       | 6    | link-resolves         | link target '/nope.md' does not resolve.                                                                                                                                                              |
      | knowledge-as-code/taxonomy.md | 4    | framework-names-types | '/adrs' links to the 'adrs' type from a document every corpus shares. Name the type instead: a corpus that has not adopted it reads a dead link, and one that has is no worse off.                    |
      | knowledge-as-code/taxonomy.md | 5    | link-resolves         | link target '/adrs/0001-knowledge-as-code.md' does not resolve.                                                                                                                                       |
      | knowledge-as-code/taxonomy.md | 5    | framework-names-types | '/adrs/0001-knowledge-as-code.md' links to a record in 'adrs' from a document every corpus shares. Those records are the first thing a corpus deletes, so the link dies even where the type is used.  |
      | knowledge-as-code/taxonomy.md | 7    | link-resolves         | link target '/nowhere.md' does not resolve.                                                                                                                                                           |
      | knowledge-as-code/taxonomy.md | 8    | fragment-resolves     | '#no-such-heading' names no heading in '/adrs.md'.                                                                                                                                                    |
      | knowledge-as-code/taxonomy.md | 8    | framework-names-types | '/adrs.md#no-such-heading' links to the 'adrs' type from a document every corpus shares. Name the type instead: a corpus that has not adopted it reads a dead link, and one that has is no worse off. |
      | knowledge-as-code/taxonomy.md | 10   | framework-names-types | '/adrs.md' links to the 'adrs' type from a document every corpus shares. Name the type instead: a corpus that has not adopted it reads a dead link, and one that has is no worse off.                 |

  # The count is the assertion here. A template is checked and is not a document: it holds no id, takes no
  # place in an index, and answers to nothing corpus-wide. Were it discovered as a record instead, this
  # would read 1, and the findings below would be joined by a dozen more saying the file is not filled in.
  Scenario: A type's template is checked without being counted as a record
    Given the broken-template fixture corpus
    When I validate the corpus
    Then validation reports 0 documents and 0 skipped
    And the findings are exactly:
      | file                | line | check           | message                                                                                                                    |
      | adrs/_template.md   | 1    | template-fields | 'priority' is not a field of the 'adr' type — every document copied from this template would fail unknown-key.             |
      | adrs/_template.md   | 1    | template-fields | the template does not carry 'owner', which is required — every document copied from it would fail required-field.          |
      | adrs/_template.md   | 4    | template-fields | 'decided-on' is read as a YAML mapping rather than a value — a placeholder that opens one has to be quoted: decided-on: "{{…}}". |
      | adrs/_template.md   | 21   | link-resolves   | link target '0404-superseded-and-deleted.md' does not resolve.                                                             |
