Feature: What `restore` fetches, and what makes it refuse
`restore` fetches each corpus `.corpus.yaml` lists under `consumes:`, unpacks it under `.imports/<shortcode>/`, and
writes the version it resolved back as `resolved:`. docs/use/cli/restore.md states each behaviour below. Driven
against the export fixture as the consumer and the pack fixture as the producer, packed into a folder source,
through `Commands.Restore`. The token scenarios serve that folder as a private registry held in memory.

Scenario: A corpus that consumes nothing restores nothing
  Given the export fixture corpus
  When I restore
  Then it succeeds
  And nothing is written

Scenario: A first restore unpacks the corpus and locks its version
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "^2.3.0"
  When I restore
  Then it succeeds
  And "lib" was restored at "2.3.4"
  And ".imports/lib/glossary/terms.jsonl" exists
  And the restore locks the producer at "2.3.4"

Scenario: A range takes the newest version it admits
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the producer is packed for restore at "2.4.0"
  And the producer is packed for restore at "3.0.0"
  And the corpus restores the producer as "lib" at "^2.3.0"
  When I restore
  Then it succeeds
  And "lib" was restored at "2.4.0"

Scenario: A range below 1.0.0 admits the same minor version alone
  Given the export fixture corpus
  And the producer is packed for restore at "0.16.0"
  And the producer is packed for restore at "0.16.2"
  And the producer is packed for restore at "0.17.0"
  And the corpus restores the producer as "lib" at "^0.16.0"
  When I restore
  Then it succeeds
  And "lib" was restored at "0.16.2"

Scenario: A repeat run writes nothing
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And it has been restored
  When I restore
  Then it succeeds
  And nothing is written

Scenario: A lock the range admits is taken without asking the source
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And it has been restored
  And "feed" is deleted
  When I restore
  Then it succeeds
  And nothing is written

Scenario: A lock the range admits is kept when a newer version exists
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the producer is packed for restore at "2.4.0"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And the restore lock reads "2.3.4"
  When I restore
  Then it succeeds
  And "lib" was restored at "2.3.4"

Scenario: A move of the range re-resolves the lock
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the producer is packed for restore at "2.4.0"
  And the corpus restores the producer as "lib" at "2.3.4"
  And it has been restored
  And the restore range is changed to "^2.4.0"
  When I restore
  Then it succeeds
  And "lib" was restored at "2.4.0"
  And the restore locks the producer at "2.4.0"

Scenario: A prerelease is restored where the range names it exactly
  Given the export fixture corpus
  And the producer is packed for restore at "2.4.0-rc.1"
  And the corpus restores the producer as "lib" at "2.4.0-rc.1"
  When I restore
  Then it succeeds
  And "lib" was restored at "2.4.0-rc.1"

Scenario: A restore sends the registry token to an https source
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores "example-libraries" as "lib" at "^2.3.0" from "https://feed.example/v3/index.json"
  When I restore from a private feed with the registry token "s3cret"
  Then it succeeds
  And "lib" was restored at "2.3.4"

Scenario: A restore sends no registry token over plain HTTP
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores "example-libraries" as "lib" at "^2.3.0" from "http://feed.example/v3/index.json"
  When I restore from a private feed with the registry token "s3cret"
  Then it refuses
  And nothing is written

Scenario: A restore refuses a shortcode the package does not carry
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "gov" at "^2.3.0"
  When I restore
  Then it refuses
  And nothing is written

Scenario: A restore refuses a range no version in the source satisfies
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "^9.0.0"
  When I restore
  Then it refuses
  And nothing is written

Scenario Outline: A restore refuses a range of any other form
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "<range>"
  When I restore
  Then it refuses
  And nothing is written

Examples:
  | range           |
  | >=2.3.0         |
  | 2.3.4 \|\| 2.4.0 |
  | 2.*             |
  | ^2.4.0-rc.1     |

Scenario: A restore refuses two entries claiming one shortcode
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And "example-other" is packed for restore at "1.0.0"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And the corpus restores "example-other" as "lib" at "^1.0.0" from "feed"
  When I restore
  Then it refuses
  And nothing is written

Scenario: A restore refuses two entries consuming one corpus
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And the corpus restores "example-libraries" as "lib2" at "^2.3.0" from "feed"
  When I restore
  Then it refuses
  And nothing is written

Scenario: A restore refuses a folder that no longer holds the locked version
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.5"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And the restore lock reads "2.3.4"
  When I restore
  Then it refuses
  And nothing is written

Scenario: A restore refuses everything where one entry fails
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the corpus restores the producer as "lib" at "^2.3.0"
  And the corpus restores "example-other" as "oth" at "^1.0.0" from "no-such-folder"
  When I restore
  Then it refuses
  And nothing is written
  And ".imports/lib" does not exist

Scenario: A restore refuses a package holding an entry past 16MB
  Given the export fixture corpus
  And the producer is packed for restore at "2.3.4"
  And the packed "2.3.4" holds an entry past the restore cap
  And the corpus restores the producer as "lib" at "^2.3.0"
  When I restore
  Then it refuses
  And nothing is written
