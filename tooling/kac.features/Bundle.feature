Feature: What `bundle` assembles, and what makes it refuse
`bundle` assembles the export and the plugin tree into a Claude Code plugin under `.dist/plugin/`, and the marketplace
that offers it under `.dist/`. docs/use/cli/bundle.md states each behaviour below. Driven against the bundle and
bundle-empty fixtures, whose `plugin/` is the corpus's `.plugin/`, through `Commands.Bundle`. The export-inherited
fixture takes the bundle fixture's plugin tree, so a bundle reads an export carrying what a corpus consumes.

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

Scenario: A bundle reads the export and never the corpus
  Given the bundle fixture corpus
  And it has been exported
  And "glossary" is deleted
  And ".schema" is deleted
  When I bundle
  Then it succeeds
  And the bundle includes "skills/glossary-lookup"
  And ".dist/plugin/corpus/glossary/terms.jsonl" exists

Scenario: A bundle reads a shared plugin tree and keeps this corpus's own manifest
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree is shared from "shared/.plugin"
  And "shared/.plugin/.claude-plugin/plugin.json" states "metadata.components" as []
  When I bundle
  Then it succeeds
  And the bundle includes "skills/glossary-lookup"
  And the bundle trims "skills/decision-lookup" because "the export carries no adrs"
  And ".dist/plugin/skills/glossary-lookup/SKILL.md" exists
  And ".dist/plugin/hooks/breadcrumb" exists

Scenario: A file this corpus keeps wins over the shared tree's copy
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree is shared from "shared/.plugin"
  And the corpus keeps its own "skills/glossary-lookup/SKILL.md"
  When I bundle
  Then ".dist/plugin/skills/glossary-lookup/SKILL.md" contains "This corpus's own copy."

Scenario: A bundle rebuilds .dist/plugin/ whole
  Given the bundle fixture corpus
  And it has been exported
  And ".dist/plugin/orphan.json" is left from an earlier run
  When I bundle
  Then it succeeds
  And ".dist/plugin/orphan.json" does not exist

Scenario: A component reading a type the export lacks is trimmed with its files
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then the bundle trims "skills/decision-lookup" because "the export carries no adrs"
  And ".dist/plugin/skills/decision-lookup" does not exist

Scenario: A trim takes whole path segments
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree has a component "skills/decision-lookup-notes" reading "glossary"
  When I bundle
  Then ".dist/plugin/skills/decision-lookup-notes/SKILL.md" exists

Scenario: A component reading a type only a consumed corpus exports ships
  Given the export-inherited fixture corpus
  And it has been exported
  And the corpus keeps the bundle fixture's plugin tree
  And the plugin tree has a component "skills/policy-lookup" reading "policies"
  When I bundle
  Then it succeeds
  And the bundle includes "skills/policy-lookup"
  And ".dist/plugin/skills/policy-lookup/SKILL.md" exists

Scenario: A component reading no type ships beside one that reads a type
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree has a component "skills/corpus-retrieval" reading no type
  When I bundle
  Then the bundle includes "skills/corpus-retrieval"
  And ".dist/plugin/skills/corpus-retrieval/SKILL.md" exists

Scenario: A component reading no type leaves with the last component reading one
  Given the bundle-empty fixture corpus
  And it has been exported
  And the plugin tree has a component "skills/corpus-retrieval" reading no type
  When I bundle
  Then the bundle trims "skills/corpus-retrieval" because "no component it supports survived"
  And ".dist/plugin/skills/corpus-retrieval" does not exist

Scenario: A standalone component always ships
  Given the bundle-empty fixture corpus
  And it has been exported
  And the plugin tree has a standalone component "skills/raise-finding"
  When I bundle
  Then the bundle includes "skills/raise-finding"
  And ".dist/plugin/skills/raise-finding/SKILL.md" exists

Scenario: A file outside the skill and hook directories ships with no component
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

Scenario: The plugin manifest takes who the plugin is from the descriptor
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" states "name" as "the-template"
  And ".plugin/.claude-plugin/plugin.json" states "displayName" as "The Template"
  When I bundle
  Then ".dist/plugin/.claude-plugin/plugin.json" states "name" as "example-libraries"
  And ".dist/plugin/.claude-plugin/plugin.json" states "displayName" as "Example Libraries"
  And ".dist/plugin/.claude-plugin/plugin.json" states "version" as "2.3.4"
  And ".dist/plugin/.claude-plugin/plugin.json" states "author.name" as "Paul Law"
  And ".dist/plugin/.claude-plugin/plugin.json" states "keywords" as ["knowledge-as-code", "glossary"]

Scenario: A key the descriptor gives nothing for is removed, bar the author and the description
  Given the bundle-empty fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" states "license" as "MIT"
  And ".plugin/.claude-plugin/plugin.json" states "homepage" as "https://example.com/template"
  When I bundle
  Then it succeeds
  And ".dist/plugin/.claude-plugin/plugin.json" states no "license"
  And ".dist/plugin/.claude-plugin/plugin.json" states no "homepage"
  And ".dist/plugin/.claude-plugin/plugin.json" states "author.name" as "example-empty"
  And ".dist/plugin/.claude-plugin/plugin.json" states "description" as "The example-empty knowledge corpus, and the skills that read it."

Scenario: The plugin manifest takes its homepage and repository from the publishing base
  Given the bundle fixture corpus
  And the descriptor publishes to github at "https://github.com/example/libraries"
  And it has been exported
  When I bundle
  Then it succeeds
  And ".dist/plugin/.claude-plugin/plugin.json" states "homepage" as "https://github.com/example/libraries"
  And ".dist/plugin/.claude-plugin/plugin.json" states "repository" as "https://github.com/example/libraries"

Scenario: The plugin manifest lists the components that survived, and keeps every key the tool does not know
  Given the bundle fixture corpus
  And it has been exported
  And ".plugin/.claude-plugin/plugin.json" states "x-maintainer" as {"team": "libraries"}
  When I bundle
  Then the plugin manifest lists the components "skills/glossary-lookup, hooks"
  And ".dist/plugin/.claude-plugin/plugin.json" states "x-maintainer" as {"team": "libraries"}
  And ".dist/plugin/.claude-plugin/plugin.json" states "metadata.corpusRoot" as "corpus"

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
  And the bundle trims "skills/glossary-lookup" because "the export carries no glossary"
  And the bundle trims "hooks" because "the export carries no glossary"
  And ".dist/plugin/.claude-plugin/plugin.json" exists

Scenario: bundle.json states the plugin and the export it was built around
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then ".dist/plugin/bundle.json" states "bundleVersion" as 1
  And ".dist/plugin/bundle.json" states "plugin" as "example-libraries"
  And ".dist/plugin/bundle.json" states "version" as "2.3.4"
  And ".dist/plugin/bundle.json" states "corpusRoot" as "corpus"
  And ".dist/plugin/bundle.json" states "export.corpus" as "example-libraries"
  And ".dist/plugin/bundle.json" states "export.contentVersion" as "2.3.4"
  And ".dist/plugin/bundle.json" states "export.types" as ["glossary"]
  And ".dist/plugin/bundle.json" states no "generatedAt"
  And ".dist/plugin/bundle.json" states no "commit"

Scenario: The marketplace offers the plugin from the directory beneath it
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then ".dist/.claude-plugin/marketplace.json" states "name" as "example-libraries"
  And ".dist/.claude-plugin/marketplace.json" states "owner.name" as "Paul Law"
  And ".dist/.claude-plugin/marketplace.json" states "plugins" as [{"name": "example-libraries", "description": "A worked example corpus, holding an invented estate.", "source": "./plugin"}]

Scenario: The breadcrumb names at most six records of a type
  Given the bundle fixture corpus
  And the glossary holds 8 records
  And it has been exported
  When I bundle
  Then the breadcrumb has the line "glossary. 15 entries across 8 records: Estate, Extra 4, Extra 5, Extra 6, Extra 7 and 3 more."

Scenario: The breadcrumb gives a type a line for each corpus that wrote some of it
  Given the export-inherited fixture corpus
  And it has been exported
  And the corpus keeps the bundle fixture's plugin tree
  When I bundle
  Then the breadcrumb has the line "glossary. 1 entry across 1 record: Local."
  And the breadcrumb has the line "glossary (from eng). 1 entry across 1 record: Shared."
  And the breadcrumb has the line "glossary (from gp). 1 entry across 1 record: Old."
  And the breadcrumb has the line "policies (from eng). 1 entry across 1 record: Keeping what we are asked to keep."

Scenario: The breadcrumb asks for each announced skill that survived
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree has a component "skills/corpus-retrieval" reading no type
  When I bundle
  Then the breadcrumb has the line "Ask the glossary-lookup skill before you answer from what you already know."
  And the breadcrumb does not mention "corpus-retrieval"
  And the breadcrumb does not mention "decision-lookup"

Scenario: The breadcrumb leaves with a trimmed hook
  Given the bundle-empty fixture corpus
  And it has been exported
  When I bundle
  Then the bundle trims "hooks" because "the export carries no glossary"
  And ".dist/plugin/hooks/breadcrumb.txt" does not exist

Scenario: A bundle writes each view the plugin manifest declares
  Given the bundle-views fixture corpus
  And it has been exported
  When I bundle
  Then it succeeds
  And the bundle includes "views/build-index.md"
  And the bundle includes "views/review.md"
  And ".dist/plugin/views/build-index.md" exists
  And ".dist/plugin/views/review.md" exists

Scenario: A view whose types the export lacks is trimmed, and written nowhere
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree declares the view "views/review.md" as "review" reading "standards@2"
  When I bundle
  Then it succeeds
  And the bundle trims "views/review.md" because "the export carries no standards"
  And ".dist/plugin/views/review.md" does not exist

Scenario: A view reads an optional type only where the export carries it
  Given the bundle-views fixture corpus
  And "controls" is deleted
  And "controls.md" is deleted
  And it has been exported
  When I bundle
  Then it succeeds
  And the bundle includes "views/review.md"
  And ".dist/plugin/views/review.md" contains "This corpus exports no controls"

Scenario: The hook keeps its permission bit
  Given the bundle fixture corpus
  And it has been exported
  When I bundle
  Then ".dist/plugin/hooks/breadcrumb" is executable

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

Scenario: A bundle whose component reads a shape that is not a whole number is refused
  Given the bundle fixture corpus
  And it has been exported
  And the component "skills/glossary-lookup" requires "glossary@one"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose view reads a shape the export does not have is refused
  Given the bundle-views fixture corpus
  And it has been exported
  And the component "views/review.md" requires "standards@9"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle declaring a view this tool does not write is refused
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree declares the view "views/summary.md" as "summary" reading "glossary"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose plugin tree already holds a view's file is refused
  Given the bundle-views fixture corpus
  And it has been exported
  And the plugin tree has a file "views/review.md"
  When I bundle
  Then it refuses
  And nothing is written

Scenario: A bundle whose plugin tree holds a skill no component declares is refused
  Given the bundle fixture corpus
  And it has been exported
  And the plugin tree has a file "skills/stray/SKILL.md"
  When I bundle
  Then it refuses
  And nothing is written
