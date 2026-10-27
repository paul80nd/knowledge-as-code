@moment:assure
Feature: Assure. The corpus owner finds what is uncovered, unchecked and due
  Reader: the corpus owner, or an auditor, asking about the corpus as a whole.
  Need: which policy clauses have no standard, which rules no control proves, and what is past its review.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.
  The baselines come from trials/plugin/runs/2026-10-20-990-assure.md, the plugin arm alone, read adjudicated.

  @intent:inform @type:reports @trial:plugin/dog-fooding/house/uncovered-clauses
  Scenario: A question about coverage gets the published report, with its date and its limits
    Baseline: the plugin arm met the threshold in 1 of 3 runs, so the scenario does not hold.
    Every run gave the report's date and a limit, and 2 of 3 named the report by title and not by id.
    The report's body does not travel, so no run read its own Limits section.
    Given rpt-clause-coverage answers which clauses the corpus covers, and states when it was generated
    And a task asking which policy clauses nothing in the corpus covers
    When the session answers
    Then in at least 2 of 3 runs the reply cites rpt-clause-coverage and gives the date it was generated
    And it states the report's limits

  @intent:inform @type:controls @trial:plugin/engineering/house/is-deploy-enforced
  Scenario: A question about enforcement gets the control, and its mechanism
    Baseline: the plugin arm met the threshold in 3 of 3 runs, so the scenario holds.
    Given ctl-0002 verifies std-DEPLOY, and its mechanism is not-enforced
    And a task asking whether anything checks that an artefact records its commit
    When the session answers
    Then in at least 2 of 3 runs the reply cites ctl-0002 and says nothing enforces the rule

  @intent:respect @type:deviations @trial:plugin/dog-fooding/house/deviations-due
  Scenario: A question about open departures gets each one with its review date
    Baseline: the plugin arm met the threshold in 2 of 3 runs, so the scenario holds.
    Every run gave the five deviations due on 2027-03-07, and the third named them by title and not by id.
    Given the active deviations in examples/dog-fooding, each with a review-by date
    And a task asking which accepted departures come up for review first
    When the session answers
    Then in at least 2 of 3 runs the reply cites every deviation due on the earliest date, and gives that date

  @proposed @check:restore @type:policies @trial:imports/new-producer-version
  Scenario: A new version of a consumed corpus says what it broke
    No baseline: no trial measures it yet.
    Given a team corpus whose standards cite clauses of a corpus it consumes
    And a new version of that corpus, which drops or renames a clause one of those standards cites
    When the corpus owner restores the new version and validates
    Then kac validate reports each citation the new version broke, and no other finding
    And it reports them before the team's next change merges
