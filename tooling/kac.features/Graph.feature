Feature: Cross-document graph checks
kac reconciles ids across the whole corpus: uniqueness, referenced ids, reciprocal fields, a field
mirroring a section, and a field mirroring the citations a labelled line gathers. Driven in-process
against the graph fixture, which lays a gizmo type over the real schema. A second type gives an ADR a
document of another type to point at, and it gives a field mirroring a section other than 'Related'
something to mirror. The gizmo type keeps parts of its own, so a citation into one is what the
labelled lines carry.

Background:
  Given the graph fixture corpus

Scenario: A duplicated id is reported against the later document
  When I validate the corpus
  Then the findings for "adrs/0003-third.md" are exactly:
    | line | check               | message                                                     |
    |    1 | id-matches-filename | id 'adr-0002' number does not match filename number '0003'. |
    |    1 | id-unique           | id 'adr-0002' is also used by adrs/0002-second.md.          |

Scenario: A reference to an id nothing carries is reported once, whether or not the field reciprocates
  When I validate the corpus
  Then the findings for "adrs/0004-dangling.md" are exactly:
    | line | check        | message                                                     |
    |    1 | ref-resolves | 'superseded-by' points at 'adr-0099', which does not exist. |

Scenario: A reference that resolves to a document of the wrong type is reported, and reported once
  When I validate the corpus
  Then the findings for "adrs/0005-mistyped.md" are exactly:
    | line | check        | message                                                                 |
    |    1 | ref-resolves | 'superseded-by' points at 'giz-mirrored', which is a Gizmo, not an ADR. |

Scenario: A field reconciles against the section it names, whichever section that is
  When I validate the corpus
  Then the findings for "gizmos/mirrored.md" are exactly:
    | line | check | message |
  And the findings for "gizmos/adrift.md" are exactly:
    | line | check                   | message                                                                                    |
    |    1 | related-matches-section | the '## Dependencies' section references 'giz-mirrored' but 'depends-on' does not list it. |

Scenario: A field reconciles against the lines carrying its label, whichever label that is
  When I validate the corpus
  Then the findings for "gizmos/covering.md" are exactly:
    | line | check | message |
  And the findings for "gizmos/drifting.md" are exactly:
    | line | check             | message                                                                                                                             |
    |    1 | mirrors-citations | 'covers' lists 'giz-behaving.it-rattles' and no 'Covers' line names it. Close the section that answers it with one, or take the id out of the field. |
    |    1 | mirrors-citations | a 'Covers' line names 'giz-behaving.it-purrs' and 'covers' does not list it.                                                         |
    |   31 | mirrors-citations | this 'Covers' line stands in the middle of a section. Write it as the last thing under the heading it belongs to.                    |
    |   39 | mirrors-citations | this 'Covers' line names nothing it could gather. Name what the section answers, or take the line off.                          |

Scenario: The whole graph produces exactly these findings and nothing else
  When I validate the corpus
  Then validation reports 10 documents and 0 skipped
  And the findings are exactly:
    | file                  | severity | line | check                   | message                                                                                              |
    | adrs/0001-first.md    | error    |    1 | related-matches-section | 'related' lists 'adr-0002' but it is not referenced in the '## Related' section.                     |
    | adrs/0001-first.md    | error    |    1 | related-matches-section | 'related' lists 'adr-0099' but it is not referenced in the '## Related' section.                     |
    | adrs/0001-first.md    | error    |    1 | ref-resolves            | 'related' points at 'adr-0099', which does not exist.                                                |
    | adrs/0001-first.md    | error    |    1 | reciprocal              | 'supersedes: adr-0002' is not reciprocated: adrs/0002-second.md must list 'superseded-by: adr-0001'. |
    | adrs/0001-first.md    | error    |   30 | link-resolves           | link target 'nonexistent-target.md' does not resolve.                                                |
    | adrs/0001-first.md    | error    |   30 | undefined-label         | reference '[ADR-0099]' has no link definition.                                                       |
    | adrs/0001-first.md    | error    |   32 | fragment-resolves       | '#renamed-away' names no heading in '0002-second.md'.                                                |
    | adrs/0001-first.md    | warning  |   33 | bracket-literal         | '[an unlinked placeholder]' looks like a reference but has no definition (or use an inline link).    |
    | adrs/0003-third.md    | error    |    1 | id-matches-filename     | id 'adr-0002' number does not match filename number '0003'.                                          |
    | adrs/0003-third.md    | error    |    1 | id-unique               | id 'adr-0002' is also used by adrs/0002-second.md.                                                   |
    | adrs/0004-dangling.md | error    |    1 | ref-resolves            | 'superseded-by' points at 'adr-0099', which does not exist.                                          |
    | adrs/0005-mistyped.md | error    |    1 | ref-resolves            | 'superseded-by' points at 'giz-mirrored', which is a Gizmo, not an ADR.                              |
    | gizmos/adrift.md      | error    |    1 | related-matches-section | the '## Dependencies' section references 'giz-mirrored' but 'depends-on' does not list it.           |
    | gizmos/drifting.md    | error    |    1 | mirrors-citations       | 'covers' lists 'giz-behaving.it-rattles' and no 'Covers' line names it. Close the section that answers it with one, or take the id out of the field. |
    | gizmos/drifting.md    | error    |    1 | mirrors-citations       | a 'Covers' line names 'giz-behaving.it-purrs' and 'covers' does not list it.                         |
    | gizmos/drifting.md    | error    |   31 | mirrors-citations       | this 'Covers' line stands in the middle of a section. Write it as the last thing under the heading it belongs to. |
    | gizmos/drifting.md    | error    |   39 | mirrors-citations       | this 'Covers' line names nothing it could gather. Name what the section answers, or take the line off.       |
