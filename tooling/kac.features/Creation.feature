Feature: What `new` settles before it writes anything

  Everything that can stop a run is read before the first question, so that nobody answers six of them
  and is then told the folder was not empty. Two of those states refuse and two carry on.

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
