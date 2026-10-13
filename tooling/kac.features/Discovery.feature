Feature: Which files discovery reads
Discovery decides which schema judges a corpus, which files it lists, and which of those are records.
docs/use/discovery.md states each behaviour below. A scenario adds files to the clean fixture corpus, validates it,
and reads the count of records and skipped files, and the findings.

Scenario: A schema above the corpus judges it
  Given the clean fixture corpus
  And the corpus sits in "corpus" below its schema
  When I validate the corpus in "corpus"
  Then validation reports 7 documents and 0 skipped
  And no findings are reported

Scenario: A file git ignores is not read
  Given the clean fixture corpus
  And the corpus is a git repository
  And ".gitignore" is replaced with "policies/notes.md"
  And a file "policies/notes.md" with no frontmatter
  When I validate the corpus
  Then validation reports 7 documents and 0 skipped

Scenario: A file the corpus ignores is read where the corpus is not a repository
  Given the clean fixture corpus
  And ".gitignore" is replaced with "policies/notes.md"
  And a file "policies/notes.md" with no frontmatter
  When I validate the corpus
  Then validation reports 7 documents and 1 skipped

Scenario: A link to a file that is not Markdown resolves in a repository
  Given the clean fixture corpus
  And the corpus is a git repository
  And a file "adrs/diagram.png"
  And "adrs/0001-clean-baseline.md" ends with "[The diagram](diagram.png)"
  When I validate the corpus
  Then no findings are reported

Scenario: A link to a file that is not Markdown fails where the corpus is not a repository
  Given the clean fixture corpus
  And a file "adrs/diagram.png"
  And "adrs/0001-clean-baseline.md" ends with "[The diagram](diagram.png)"
  When I validate the corpus
  Then the findings are exactly:
    | file                        | line | check         | message                                     |
    | adrs/0001-clean-baseline.md | 39   | link-resolves | link target 'diagram.png' does not resolve. |

Scenario: A folder discovery skips at the corpus root is read inside a type folder
  Given the clean fixture corpus
  And a file ".git/notes.md" with no frontmatter
  And a file ".idea/notes.md" with no frontmatter
  And a file ".claude/notes.md" with no frontmatter
  And a file "policies/.claude/notes.md" with no frontmatter
  When I validate the corpus
  Then validation reports 7 documents and 1 skipped

Scenario: A link into a folder the fallback walk skips fails
  Given the clean fixture corpus
  And a file ".claude/notes.md" with no frontmatter
  And "adrs/0001-clean-baseline.md" ends with "[The notes](/.claude/notes.md)"
  When I validate the corpus
  Then the findings are exactly:
    | file                        | line | check         | message                                           |
    | adrs/0001-clean-baseline.md | 39   | link-resolves | link target '/.claude/notes.md' does not resolve. |

Scenario: A Markdown file outside every type folder is not a record
  Given the clean fixture corpus
  And a file "README.md" with no frontmatter
  And a file "CLAUDE.md" with no frontmatter
  And a file "notes/loose.md" with no frontmatter
  When I validate the corpus
  Then validation reports 7 documents and 0 skipped
  And no findings are reported

Scenario: A file in a type folder with no frontmatter is skipped, and the run passes
  Given the clean fixture corpus
  And a file "policies/notes.md" with no frontmatter
  When I validate the corpus
  Then validation reports 7 documents and 1 skipped
  And no findings are reported
