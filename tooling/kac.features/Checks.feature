Feature: What `checks` lists, and when it fails
`checks` lists every check the validator can report, read from the corpus's own `.schema/`, and compares that
catalogue against the checks table `generate` writes onto each type page. docs/use/cli/checks.md states each behaviour
below. A scenario reads the catalogue `Commands.Checks` prints from, and its exit code.

Scenario: The catalogue lists each check with the severity it reports at and what it proves
  Given the clean fixture corpus
  When I list the checks
  Then it succeeds
  And the catalogue lists:
    | check              | severity | description                                                    |
    | frontmatter-parses | error    | The frontmatter block is present and parses as a YAML mapping. |
    | expiry             | warning  | An active deviation is still inside its review date.           |
    | draft-expiry       | warning  | A draft deviation is still inside its review date.             |

Scenario: A type the corpus declares itself has its checks in the catalogue
  Given the clean fixture corpus
  And the corpus's schema declares a type "gadgets" whose rule "gadgets-are-titled" checks "present('owner')"
  When I list the checks
  Then it succeeds
  And the catalogue lists "gadgets-are-titled"

Scenario: A rule implemented in C# is listed under the check it emits
  Given the clean fixture corpus
  When I list the checks
  Then the catalogue lists "y-statement"
  And the catalogue does not list "y-statement-present"

Scenario: A rule with no severity is not in the catalogue
  Given the clean fixture corpus
  When I list the checks
  Then the catalogue does not list "immutable-after-accepted"

Scenario: The catalogue opens no record and reports no fault in one
  Given the broken-values fixture corpus
  When I list the checks
  Then it succeeds
  And nothing is written

Scenario Outline: A check declared for the type page with no row in the checks table fails the run
  Given the clean fixture corpus
  And the corpus's schema declares a check "an-unlisted-check" for the type page
  When I list the checks <form>
  Then it fails
  And nothing is written

Examples:
  | form       |
  | as text    |
  | as JSON    |

Scenario: A check kept off the type page that has a row in the checks table fails the run
  Given the clean fixture corpus
  And the corpus's schema keeps the check "frontmatter-parses" off the type page
  When I list the checks as text
  Then it fails

Scenario: A row in the checks table for a check the schema does not declare fails the run
  Given the clean fixture corpus
  And the corpus's schema declares no check "frontmatter-parses"
  When I list the checks as text
  Then it fails
