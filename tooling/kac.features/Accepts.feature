Feature: Accepting a warning
A record keeps a warning by naming its check in `accepts:`, with a reason. Driven in-process against the accepts
fixture.

Background:
  Given the accepts fixture corpus

Scenario: An accepted warning is reported as info, with its reason
  When I validate the corpus
  Then the findings for "adrs/0001-accepted.md" are exactly:
    | severity | line | check             | message                                                          |
    | info     |      | unused-definition | accepted: The definition is kept for the page that will cite it. |

Scenario: An entry matching no warning on its record is stale
  When I validate the corpus
  Then the findings for "adrs/0002-stale.md" are exactly:
    | severity | line | check            | message                                                                                                       |
    | warning  | 9    | acceptance-stale | no 'unused-definition' warning is left on this record for this entry to accept. Remove the entry from 'accepts'. |

Scenario: A stale entry accepts the warning once its record raises it again
  Given "adrs/0002-stale.md" ends with "[unused-ref]: /adrs"
  When I validate the corpus
  Then the findings for "adrs/0002-stale.md" are exactly:
    | severity | line | check             | message                                            |
    | info     |      | unused-definition | accepted: Nothing here is unused any more.         |

Scenario: An entry naming anything but a check that warns is refused
  When I validate the corpus
  Then the findings for "adrs/0003-refused.md" are exactly:
    | severity | line | check            | message                                                                                              |
    | error    | 9    | acceptance-check | 'acceptance-stale' cannot be accepted. Remove the entry it reports, and then this one.               |
    | error    | 10   | acceptance-check | 'h1' reports errors, and 'accepts' takes a warning only. Fix what it reports, and remove the entry.  |
    | error    | 11   | acceptance-check | 'import-capped' reports info, and 'accepts' takes a warning only. Remove the entry.                  |
    | error    | 12   | acceptance-check | 'no-such-check' is not a check `kac validate` reports. `kac checks` lists every check id.            |

Scenario: An entry with no reason accepts nothing
  When I validate the corpus
  Then the findings for "adrs/0004-empty-reason.md" are exactly:
    | severity | line | check             | message                                                            |
    | warning  |      | unused-definition | link definition '[unused-ref]' is never referenced.                |
    | error    | 9    | entry-key         | 'accepts' entry 'unused-definition' leaves 'reason' with no value. |
