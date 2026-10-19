@moment:assure
Feature: Assure. The corpus owner finds what is uncovered, unchecked and due
  Reader: the corpus owner, or an auditor, asking about the corpus as a whole.
  Need: which policy clauses have no standard, which rules no control proves, and what is past its review.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.

  @intent:inform @type:reports @trial:plugin/dog-fooding/house/uncovered-clauses @proposed
  Scenario: A question about coverage gets the published report, with its date and its limits
    Measured by a new plugin trial case, graded on the reply. No run yet, so there is no baseline.
    Given rpt-clause-coverage answers which clauses the corpus covers, and states when it was generated
    And a task asking which policy clauses nothing in the corpus covers
    When the session answers
    Then in at least 2 of 3 runs the reply cites rpt-clause-coverage and gives the date it was generated
    And it states the report's limits

  @intent:inform @type:controls @trial:plugin/engineering/house/is-deploy-enforced @proposed
  Scenario: A question about enforcement gets the control, and its mechanism
    Measured by a new plugin trial case, graded on the reply. No run yet, so there is no baseline.
    Given ctl-0002 verifies std-DEPLOY, and its mechanism is not-enforced
    And a task asking whether anything checks that an artefact records its commit
    When the session answers
    Then in at least 2 of 3 runs the reply cites ctl-0002 and says nothing enforces the rule

  @intent:respect @type:deviations @trial:plugin/dog-fooding/house/deviations-due @proposed
  Scenario: A question about open departures gets each one with its review date
    Measured by a new plugin trial case, graded on the reply. No run yet, so there is no baseline.
    Given the active deviations in examples/dog-fooding, each with a review-by date
    And a task asking which accepted departures come up for review first
    When the session answers
    Then in at least 2 of 3 runs the reply cites every deviation due on the earliest date, and gives that date
