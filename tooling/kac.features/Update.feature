Feature: What `update` takes into a corpus, and what makes it refuse
`update` fetches the corpus's framework again, writes what the corpus receives, and commits nothing.
docs/use/cli/update.md states each behaviour below. Each scenario creates a corpus from this repository read as a
template, commits it, and runs `Commands.Update` over it, so `git status` is what the run wrote.

Scenario: A check over a corpus in step passes and writes nothing
  Given a corpus to update, adopting "adrs,glossary"
  When I check the corpus for an update
  Then it succeeds
  And the update left the corpus as it was

Scenario: A check over a corpus behind its framework fails and writes nothing
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to ".schema/adrs.yaml"
  When I check the corpus for an update
  Then it fails
  And the update left the corpus as it was

Scenario: An update takes the framework's copy of a file the corpus holds differently
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to ".schema/adrs.yaml"
  When I update the corpus
  Then it succeeds
  And the update changed ".schema/adrs.yaml"
  And ".schema/adrs.yaml" in the updated corpus no longer says "Edited by the corpus."

Scenario: An update commits nothing and stages nothing
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to ".schema/adrs.yaml"
  When I update the corpus
  Then it succeeds
  And the update changed ".schema/adrs.yaml"
  And the update committed and staged nothing

Scenario: An update leaves a seed file the corpus changed
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to "CLAUDE.md"
  When I update the corpus
  Then it succeeds
  And the update left "CLAUDE.md" alone

Scenario: An update under the full policy writes the seed files again
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to "CLAUDE.md"
  When I update the corpus under the "full" policy
  Then it succeeds
  And "CLAUDE.md" in the updated corpus no longer says "Edited by the corpus."

Scenario: An update leaves a seeded record the corpus filed deeper
  Given a corpus to update, adopting "adrs,glossary,policies"
  And the corpus to update commits a move of "policies/devi-deviations.md" to "policies/governance/devi-deviations.md"
  When I update the corpus
  Then it succeeds
  And the updated corpus has no "policies/devi-deviations.md"

Scenario: An update deletes what the manifest retires, and nothing else
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a file ".schema/CLAUDE.md"
  And the corpus to update commits a file "knowledge-as-code/retired.md"
  When I update the corpus
  Then it succeeds
  And the updated corpus has no ".schema/CLAUDE.md"
  And the updated corpus holds "knowledge-as-code/retired.md"

Scenario: An update never writes a path the corpus skips
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to ".claude/skills/technical-writing/SKILL.md"
  And the corpus to update commits a skip of ".claude/skills/technical-writing/SKILL.md"
  When I update the corpus
  Then it succeeds
  And the update left ".claude/skills/technical-writing/SKILL.md" alone

Scenario: An update writes none of a plugin tree the corpus reads from elsewhere
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update reads its plugin tree from "../shared-plugin"
  And the corpus to update commits the deletion of its copy of the shared plugin tree
  When I update the corpus
  Then it succeeds
  And the update left ".plugin/" alone

Scenario: A check over a corpus keeping a copy of a plugin tree it reads from elsewhere fails
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update reads its plugin tree from "../shared-plugin"
  When I check the corpus for an update
  Then it fails
  And the update left the corpus as it was

Scenario: A check over a corpus keeping a framework file the template does not send fails
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a file "knowledge-as-code/retired.md"
  When I check the corpus for an update
  Then it fails
  And the update left the corpus as it was

Scenario: An update stamps the descriptor format this tool writes
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits an older descriptor-version
  When I update the corpus
  Then it succeeds
  And the updated descriptor states the format this tool writes

Scenario: An update from a template needing a newer kac is refused
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits a change to ".schema/adrs.yaml"
  When I update the corpus with a kac older than the template needs
  Then it refuses
  And the update left the corpus as it was

Scenario: An update over a tree with uncommitted changes is refused
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update holds an uncommitted change to ".schema/adrs.yaml"
  When I update the corpus
  Then it refuses
  And ".schema/adrs.yaml" in the updated corpus still says "Not committed."
  And the update left ".corpus.yaml" alone

Scenario: A check runs over a tree with uncommitted changes
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update holds an uncommitted change to "CLAUDE.md"
  When I check the corpus for an update
  Then it succeeds

Scenario: A type the corpus has not adopted is not adopted by an update
  Given a corpus to update, adopting "adrs,glossary"
  When I update the corpus
  Then it succeeds
  And the updated corpus does not adopt "policies"
  And the updated corpus has no "policies.md"

Scenario: An update adding a type writes its page and template, and adopts it
  Given a corpus to update, adopting "adrs,glossary"
  When I update the corpus adding "policies"
  Then it succeeds
  And the updated corpus holds "policies.md"
  And the updated corpus holds "policies/_template.md"
  And the updated corpus holds ".schema/policies.yaml"
  And the updated corpus adopts "policies"

Scenario: An update dropping a type whose folder holds no record deletes its page
  Given a corpus to update, adopting "adrs,glossary,tools"
  When I update the corpus dropping "tools" with --yes
  Then it succeeds
  And the updated corpus has no "tools.md"
  And the updated corpus does not adopt "tools"

Scenario: An update dropping a type whose folder holds a record is refused
  Given a corpus to update, adopting "adrs,glossary,tools"
  And the corpus to update commits a record "tools/git.md"
  When I update the corpus dropping "tools" with --yes
  Then it refuses
  And the update left the corpus as it was

Scenario: An update dropping a type with nobody to answer is refused
  Given a corpus to update, adopting "adrs,glossary,tools"
  When I update the corpus dropping "tools" with nobody to answer
  Then it refuses
  And the update left the corpus as it was

Scenario: An update over a descriptor using a renamed key is refused
  Given a corpus to update, adopting "adrs,glossary"
  And the corpus to update commits "accepted-divergences: []" in its descriptor
  When I update the corpus
  Then it refuses
  And the update left the corpus as it was
