@moment:review
Feature: Review. A reviewer checks a change against the rules, and cites each one it applies
  Reader: a reviewer, human or AI, reading a change somebody else wrote.
  Need: the rules to check the change against, each with an id to cite, and which of them a control already checks.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.
  The trial is the review trial, run over the workspaces the plugin trial's full pass kept and cases of its own.

  @intent:nudge @type:standards @trial:review/engineering/health-endpoint
  Scenario: A review of a change that broke a MUST cites the clause it broke
    Baseline: 2 of 3 replies cited std-DEPLOY and the missing commit, and 1 of 3 of the reviewer's own reports.
    Given the 3 plugin-arm workspaces of health-endpoint, whose endpoints report no commit
    When a reviewer reviews each workspace
    Then in at least 2 of 3 reviews the finding cites std-DEPLOY and says the artefact does not record its commit

  @intent:nudge @type:standards @trial:review/engineering/must-in-added-lines
  Scenario: A review of a MUST broken in the change's own lines cites it, and files it as a violation
    Baseline: 3 of 3 replies cited std-SECRET and 0 of 3 filed it under Violations. 3 of 3 reports did both.
    Given a change whose added lines write an API key into source
    When a reviewer reviews the change
    Then in at least 2 of 3 reviews the review cites std-SECRET, and files the finding under Violations

  @intent:nudge @type:standards @trial:review/engineering/should-departure
  Scenario: A review notes a departure from a SHOULD that gives its reason, and does not fail it
    Baseline: 3 of 3 replies cited the rule and the reason, and 3 of 3 of the reviewer's own reports.
    Given a change that departs from a SHOULD rule, and states its reason in the change
    When a reviewer reviews the change
    Then in at least 2 of 3 reviews the review cites the rule and the reason, and does not report it as a failure

  @intent:inform @type:controls @trial:review/engineering/health-endpoint
  Scenario: A review says which rule no control checks
    Baseline: 1 of 3 replies checked std-DEPLOY and said nothing automated checks it, and 1 of 3 reports.
    Given ctl-0002 verifies std-DEPLOY, and its mechanism is not-enforced
    When a reviewer reviews a health-endpoint workspace
    Then in at least 2 of 3 reviews the review checks std-DEPLOY itself, and says no automated check covers it
