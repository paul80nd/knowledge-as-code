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

# A type page cross-references the other types, and a corpus adopting a subset holds no page for those
# links to reach. The links are taken out as each page is written.

Scenario: A corpus that declined types is sent no link it cannot follow
  Given a git repository
  When I create a corpus there adopting "adrs,glossary"
  Then it succeeds
  And no link fails to resolve

# A type's schema reaches its neighbours too: `standards.yaml` alone reaches four other types, through
# `ref:` on four fields and a `versus:` naming one of them again. Those name types rather than folders, so
# a corpus that declined them is asked nothing about them, and adopting one later needs no edit there.

Scenario: A corpus adopting a single type validates
  Given a git repository
  When I create a corpus there adopting "standards"
  Then it succeeds
  And no link fails to resolve

Scenario: A corpus adopting every type keeps the links between its type pages
  Given a git repository
  When I create a corpus there adopting "all"
  Then no link fails to resolve
  And "glossary.md" links to "services.md"

Scenario: A full update changes nothing on the corpus new just created
  Given a git repository
  When I create a corpus there adopting "adrs,glossary"
  Then a full update finds it in step
