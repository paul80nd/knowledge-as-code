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
      | .schema/glossary.yaml |     | type-setup | type 'glossary' is single-document, so 'glossary/' must not exist — its page is the document. |
      | .schema/tools.yaml   |      | type-setup | type 'tools' has a 'tools/' folder but is not fully set up — add tools.md, tools/_template.md. |

  Scenario: A collection's page is checked for its generated markers and its links
    Given the type-pages fixture corpus
    When I validate the corpus
    Then validation reports 1 documents and 0 skipped
    And the findings are exactly:
      | file         | line | check           | message                                                                                                                 |
      | adrs.md      |      | generated-block | the 'schema-adrs' block is missing its BEGIN marker — `kac index` writes between them and leaves the page alone without both. |
      | adrs.md      | 6    | link-resolves   | link target '/nope.md' does not resolve.                                                                                |
      | glossary.md  | 1    | id-format       | id 'not-glossary' must be 'glossary', the value the type declares.                                                      |
