Feature: What `bundle` assembles, and what makes it refuse
`bundle` assembles the export and the plugin tree into a Claude Code plugin under `.dist/plugin/`, and the marketplace
that offers it under `.dist/`. docs/use/cli/bundle.md states each behaviour below. Driven against the bundle and
bundle-empty fixtures, whose `plugin/` is the corpus's `.plugin/`, through `Commands.Bundle`.

Scenario: A bundle writes the plugin, its account of itself and the marketplace
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then it succeeds
  And ".dist/plugin/.claude-plugin/plugin.json" exists
  And ".dist/plugin/bundle.json" exists
  And ".dist/.claude-plugin/marketplace.json" exists
  And ".dist/plugin/skills/glossary-lookup/SKILL.md" exists
  And the bundle includes "skills/glossary-lookup"
  And nothing outside .dist/ changed

Scenario: A component reading a type the export lacks is trimmed with its files
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then the bundle trims "skills/decision-lookup"
  And ".dist/plugin/skills/decision-lookup" does not exist

Scenario: A file no component claims ships
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then ".dist/plugin/README.md" exists

Scenario: The plugin contains the export unchanged
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then ".dist/plugin/corpus" is a copy of ".dist/export"
  And nothing under ".dist/export" changed

Scenario: A bundle rebuilds .dist/plugin/ whole
  Given the bundle fixture corpus
  And it has been exported
  And ".dist/plugin/skills/orphan/SKILL.md" is left from an earlier run
  When I bundle
  Then it succeeds
  And ".dist/plugin/skills/orphan" does not exist

Scenario: The plugin manifest takes who the plugin is from the descriptor
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then ".dist/plugin/.claude-plugin/plugin.json" states "name" as "example-libraries"
  And ".dist/plugin/.claude-plugin/plugin.json" states "version" as "2.3.4"
  And ".dist/plugin/.claude-plugin/plugin.json" states "author.name" as "Paul Law"

Scenario: A key the descriptor gives nothing for is removed, bar the author and the description
  Given the bundle-empty fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" states "license" as "MIT"
  When I bundle
  Then it succeeds
  And ".dist/plugin/.claude-plugin/plugin.json" states no "license"
  And ".dist/plugin/.claude-plugin/plugin.json" states "author.name" as "example-empty"
  And ".dist/plugin/.claude-plugin/plugin.json" states "description" as "The example-empty knowledge corpus, and the skills that read it."

Scenario: A corpus stating no content version keeps the version its plugin manifest states
  Given the bundle-empty fixture corpus
  And it has been exported
  When I bundle
  Then it succeeds
  And ".dist/plugin/.claude-plugin/plugin.json" states "version" as "0.0.1"

Scenario: A bundle that trims every component still builds
  Given the bundle-empty fixture corpus
  And it has been exported
  When I bundle
  Then it succeeds
  And the bundle trims "skills/glossary-lookup"
  And the bundle trims "hooks"
  And ".dist/plugin/.claude-plugin/plugin.json" exists

Scenario: A bundle with no export is refused
  Given the bundle fixture corpus
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle with no plugin tree is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin" is deleted
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle reading the plugin tree from a folder that is not there is refused
  Given the bundle fixture corpus
  And it has been exported
  And the descriptor reads the plugin tree from "../shared/.plugin"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose plugin tree has no manifest is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" is deleted
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose plugin manifest is not JSON is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" is replaced with "not json"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose plugin manifest states no corpusRoot is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" states no "metadata.corpusRoot"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose corpusRoot the plugin tree already uses is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" states "metadata.corpusRoot" as "skills"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle over an export with no manifest is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".dist/export/manifest.json" is deleted
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle over an export naming no corpus is refused
  Given the bundle fixture corpus
  And the descriptor states no "corpus"
  And it has been exported
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle over an export at another format is refused
  Given the bundle fixture corpus
  And it has been exported
  And ".dist/export/manifest.json" states "formatVersion" as 99
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose component reads a shape the export does not have is refused
  Given the bundle fixture corpus
  And it has been exported
  And the component "skills/glossary-lookup" requires "glossary@9"
  When I bundle
  Then it refuses
  And nothing is written
