@moment:review
Feature: Review. A reviewer checks a change against the rules, and cites each one it applies
  Reader: a reviewer, human or AI, reading a change somebody else wrote.
  Need: the rules to check the change against, each with an id to cite, and which of them a control already checks.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.
  The trial is the review trial #944 asks for, run over the workspaces the plugin trial's full pass kept.

  @intent:nudge @type:standards @trial:review/engineering/health-endpoint @proposed
  Scenario: A review of a change that broke a MUST cites the clause it broke
    Measured by the review trial. No run yet, so there is no baseline.
    Given the 3 plugin-arm workspaces of health-endpoint, whose endpoints report no commit
    When a reviewer reviews each workspace
    Then in at least 2 of 3 reviews the finding cites std-DEPLOY and says the artefact does not record its commit

  @intent:nudge @type:standards @trial:review/engineering/should-departure @proposed
  Scenario: A review notes a departure from a SHOULD that gives its reason, and does not fail it
    Measured by the review trial. No run yet, so there is no baseline.
    Given a change that departs from a SHOULD rule, and states its reason in the change
    When a reviewer reviews the change
    Then in at least 2 of 3 reviews the review cites the rule and the reason, and does not report it as a failure

  @intent:inform @type:controls @trial:review/engineering/health-endpoint @proposed
  Scenario: A review says which rule no control checks
    Measured by the review trial. No run yet, so there is no baseline.
    Given ctl-0002 verifies std-DEPLOY, and its mechanism is not-enforced
    When a reviewer reviews a health-endpoint workspace
    Then in at least 2 of 3 reviews the review checks std-DEPLOY itself, and says no automated check covers it
