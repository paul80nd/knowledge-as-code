@moment:operate
Feature: Operate. Whoever is on call meets the runbook, and anybody who has hit this failure before
  Reader: an agent or an engineer on call, working from a symptom.
  Need: the runbook for the symptom, a fix somebody verified, and what earlier incidents found.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.

  @intent:follow @type:runbooks @trial:plugin/dog-fooding/house/golden-red-in-ci @proposed
  Scenario: A symptom a runbook opens on gets the runbook's steps, in its order
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given rbk-goldens-disagree opens on a branch that is green locally and red in CI
    And its first immediate action is to run the failing scenario with GITHUB_ACTIONS=true
    And a task reporting a golden scenario that passes locally and fails in CI
    When the session works the failure
    Then in at least 2 of 3 runs the session runs the scenario with GITHUB_ACTIONS=true before it changes any file
    And no run passes --update before it has read the diff

  @intent:follow @type:fixes @trial:plugin/payments/house/two-pending-amounts @proposed
  Scenario: A symptom that matches a verified fix gets the fix's resolution
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given fix-0001 is verified by a person, and matches two pending amounts for one order
    And a task reporting a customer with two pending amounts after a retried payment
    When the session works the report
    Then in at least 2 of 3 runs the caller derives its idempotency key from the order and the operation
    And the reply cites fix-0001

  @intent:inform @type:fixes @trial:plugin/payments/house/unverified-fix @proposed
  Scenario: A draft fix is a lead, not a resolution
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given a draft fix whose symptom matches the report, and whose verified list is empty
    And a task reporting that symptom
    When the session works the report
    Then in at least 2 of 3 runs the reply says the fix is unverified before it applies the resolution

  @intent:inform @type:postmortems @trial:plugin/payments/house/has-this-happened-before @proposed
  Scenario: A question about a past failure gets the incident's account
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given pmt-0002 is the published account of an incident that left two pending amounts on one order
    And a task asking whether two pending amounts have happened before, and what caused them
    When the session answers
    Then in at least 2 of 3 runs the reply cites pmt-0002 and states its cause

  @intent:inform @type:services @trial:plugin/library/house/notices-down @proposed
  Scenario: A failing service's owner comes from its service record
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given svc-notices names human:mira.okonjo as its owner
    And a task reporting that overdue notices stopped going out, and asking who to page
    When the session answers
    Then in at least 2 of 3 runs the reply names mira.okonjo and cites svc-notices
