Feature: What `pack` seals, and what makes it refuse
`pack` zips what `export` wrote into one `.nupkg` under `.dist/package/`, named for the corpus and its content version.
docs/use/cli/pack.md states each behaviour below. Driven against the pack and export-inherited fixtures, through
`Commands.Pack`, after an export with the commit and the time fixed.

Scenario: A pack writes one package named for the corpus and its content version
  Given the pack fixture corpus
  And it has been exported
  When I pack
  Then it succeeds
  And ".dist/package/example-libraries.2.3.4.nupkg" exists
  And nothing outside .dist/ changed
  And nothing under ".dist/export" changed

Scenario: The package holds the envelope and the export byte for byte
  Given the pack fixture corpus
  And it has been exported
  When I pack
  Then the package holds "[Content_Types].xml"
  And the package holds "_rels/.rels"
  And the package holds "example-libraries.nuspec"
  And the package holds ".dist/export" under "corpus/" byte for byte

Scenario: Two packs of one export are the same file
  Given the pack fixture corpus
  And it has been exported
  When I pack
  Then packing again writes the same package

Scenario: A package includes what its corpus consumes
  Given the export-inherited fixture corpus
  And the corpus to pack states:
    """
    content-version: "1.0.0"
    """
  And it has been exported
  When I pack
  Then it succeeds
  And the package holds "corpus/glossary/eng/gls-shared.json"
  And the package holds "corpus/policies/eng/pol-KEEP.json"

Scenario: A corpus that names nobody is filed under its own id
  Given the pack fixture corpus
  And it has been exported
  When I pack
  Then the package's nuspec states "authors" as "example-libraries"
  And the package's nuspec states "description" as "The example-libraries knowledge corpus, exported as data. Cited as 'lib:'."
  And the package's nuspec states no "license"

Scenario: What a registry lists comes from the corpus
  Given the pack fixture corpus
  And the corpus to pack states:
    """
    description: A worked example corpus.
    license: MIT
    author:
      name: Paul Law
    """
  And it has been exported
  When I pack
  Then the package's nuspec states "authors" as "Paul Law"
  And the package's nuspec states "description" as "A worked example corpus. Cited as 'lib:'."
  And the package's nuspec states "license" as "MIT"

Scenario: A pack for a repository states its URL
  Given the pack fixture corpus
  And it has been exported
  When I pack for the repository "https://github.com/example/corpus"
  Then it succeeds
  And the package's nuspec states the repository "https://github.com/example/corpus"

Scenario: A pack with no export is refused
  Given the pack fixture corpus
  When I pack
  Then it refuses
  And nothing is written

Scenario Outline: A pack whose export lacks a fact it is named or cited by is refused
  Given the pack fixture corpus
  And the descriptor states no "<key>"
  And it has been exported
  When I pack
  Then it refuses
  And nothing is written

Examples:
  | key             |
  | corpus          |
  | content-version |
  | shortcode       |

Scenario: A pack of a corpus whose name a registry cannot take is refused
  Given the pack fixture corpus
  And the descriptor states no "corpus"
  And the corpus to pack states:
    """
    corpus: example libraries
    """
  And it has been exported
  When I pack
  Then it refuses
  And nothing is written

Scenario: A pack of a content version a registry cannot order is refused
  Given the pack fixture corpus
  And the descriptor states no "content-version"
  And the corpus to pack states:
    """
    content-version: "two"
    """
  And it has been exported
  When I pack
  Then it refuses
  And nothing is written
