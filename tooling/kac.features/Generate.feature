Feature: What `generate` writes, and what it leaves alone
`generate` rewrites what kac derives from frontmatter and the schema, and `--check` reports what a run would rewrite.
docs/use/cli/generate.md states each behaviour below. Driven against the generate, generate-stale and
generate-adoption fixtures, through `Commands.Generate`.

Scenario: A run rewrites a stale index whole, and no other file
  Given the generate fixture corpus
  And "adrs/_index.md" ends with "A line nothing generated."
  When I generate
  Then it succeeds
  And only "adrs/_index.md" changed
  And "adrs/_index.md" does not contain "A line nothing generated."
  And the generated files are fresh

Scenario: A run rewrites a block between its markers and keeps the words around it
  Given the generate fixture corpus
  And the generated blocks in "adrs.md" are emptied
  And "adrs.md" ends with "Words the corpus wrote."
  When I generate
  Then it succeeds
  And only "adrs.md" changed
  And "adrs.md" contains "Words the corpus wrote."
  And the generated files are fresh

Scenario: A run over a fresh corpus writes nothing
  Given the generate fixture corpus
  When I generate
  Then it succeeds
  And nothing is written

Scenario: A check over a fresh corpus passes
  Given the generate fixture corpus
  When I check the generated files
  Then it succeeds

Scenario: A check over a stale corpus fails and writes nothing
  Given the generate-stale fixture corpus
  When I check the generated files
  Then it fails
  And nothing is written

Scenario: A type the descriptor does not adopt gets no index
  Given the generate-adoption fixture corpus
  When I generate
  Then it succeeds
  And nothing is written
  And "policies/_index.md" does not exist

Scenario: A type adopted with no folder is not created
  Given the generate-adoption fixture corpus
  And the descriptor adopts "glossary" as well
  When I generate
  Then it succeeds
  And nothing is written
  And "glossary" does not exist
