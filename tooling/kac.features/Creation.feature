Feature: What `new` settles before it writes anything

Scenario: A folder already inside a corpus is refused
  Given an empty folder
  And a corpus descriptor above it
  When I create a corpus there
  Then it refuses
  And the folder holds nothing new

Scenario: A repository holding uncommitted work is refused
  Given a git repository
  And a file changed since the last commit
  When I create a corpus there
  Then it refuses
  And the folder holds nothing new

Scenario: A folder with no repository has one initialised
  Given an empty folder
  When I create a corpus there
  Then it succeeds
  And the folder is a git repository

Scenario: A folder with no repository and nobody to ask is refused
  Given an empty folder
  When I create a corpus there with nobody to answer
  Then it refuses
  And the folder holds nothing new
  And the folder is not a git repository

Scenario: A repository holding its own committed files keeps them
  Given a git repository
  And it holds a committed "NOTICE.txt"
  When I create a corpus there
  Then it succeeds
  And "NOTICE.txt" is still there

Scenario: A template that cannot be read is refused
  Given a git repository
  When I create a corpus from a template that is not there
  Then it refuses
  And the folder holds nothing new

Scenario: A template needing a newer kac is refused
  Given a git repository
  When I create a corpus there with a kac older than the template needs
  Then it refuses
  And the folder holds nothing new

Scenario: A new corpus is given a descriptor and a README of its own
  Given a git repository
  When I create a corpus there named "field-notes"
  Then it succeeds
  And ".corpus.yaml" in the new corpus says "corpus: field-notes"
  And "README.md" in the new corpus says "# field-notes"

Scenario: A corpus created with --yes is seeded with records its author owns
  Given a git repository whose author is "Alex.Doe@example.com"
  When I create a corpus there adopting "adrs,glossary,policies"
  Then it succeeds
  And "adrs/0001-knowledge-as-code.md" in the new corpus says "owner: human:alex.doe"
  And "policies/devi-deviations.md" in the new corpus says "owner: human:alex.doe"
  And "glossary/knowledge-as-code.md" in the new corpus says "owner: human:paul.law"

Scenario: An owner given as a flag is written into the seeded records
  Given a git repository whose author is "alex.doe@example.com"
  When I create a corpus there owned by "role:head-of-knowledge"
  Then it succeeds
  And "adrs/0001-knowledge-as-code.md" in the new corpus says "owner: role:head-of-knowledge"

Scenario: An owner the pattern refuses is refused
  Given a git repository
  When I create a corpus there owned by "Alex Doe"
  Then it refuses
  And the folder holds nothing new

Scenario: A publishing target nothing builds links for is refused
  Given a git repository
  When I create a corpus there publishing to "mkdocs"
  Then it refuses
  And the folder holds nothing new

Scenario: A new corpus pins the kac that created it
  Given a git repository
  When I create a corpus there with kac "0.99.0+5fa039b0"
  Then it succeeds
  And ".config/dotnet-tools.json" in the new corpus says ""version": "0.99.0""

Scenario: An update leaves the tool manifest to the corpus
  Given a git repository
  When I create a corpus there with kac "0.99.0"
  And the corpus moves its tool manifest to kac "1.2.3"
  Then a full update finds it in step
  And ".config/dotnet-tools.json" in the new corpus says ""version": "1.2.3""

Scenario: A corpus built by GitHub is sent the GitHub starter alone
  Given a git repository
  When I create a corpus there built by "github"
  Then it succeeds
  And the new corpus holds ".github/workflows/kac.yml"
  And the new corpus has no "azure-pipelines.yml"

Scenario: Each hook script arrives with its execute bit
  Given a git repository
  When I create a corpus there
  Then it succeeds
  And ".plugin/hooks/breadcrumb" in the new corpus can be run
  And ".plugin/hooks/build-index" in the new corpus can be run

Scenario: A corpus that declined types is sent no link it cannot follow
  Given a git repository
  When I create a corpus there adopting "adrs,glossary"
  Then it succeeds
  And no link fails to resolve

# `standards.yaml` reaches four other types through `ref:` and `versus:`. A corpus that declined them is asked nothing
# about them.

Scenario: A corpus adopting a single type validates
  Given a git repository
  When I create a corpus there adopting "standards"
  Then it succeeds
  And no link fails to resolve

Scenario: A reference to a declined type's page loses its definition and keeps its words
  Given a git repository
  And a template whose standards template links the deviations page by reference
  When I create a corpus there from that template adopting "standards,policies"
  Then it succeeds
  And no link fails to resolve
  And "standards/_template.md" in the new corpus says "request a deviation. To depart"

Scenario: A corpus adopting every type keeps the links between its type pages
  Given a git repository
  When I create a corpus there adopting "all"
  Then no link fails to resolve
  And "glossary.md" links to "services.md"

Scenario: A full update changes nothing on the corpus new just created
  Given a git repository
  When I create a corpus there adopting "adrs,glossary"
  Then a full update finds it in step

Scenario: A corpus publishing nowhere states no tracker, and validates
  Given a git repository
  When I create a corpus there
  Then it succeeds
  And ".corpus.yaml" in the new corpus states no "tracker" block
