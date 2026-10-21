@moment:operate
Feature: Operate. Whoever is on call meets the runbook, and anybody who has hit this failure before
  Reader: an agent or an engineer on call, working from a symptom.
  Need: the runbook for the symptom, a fix somebody verified, and what earlier incidents found.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.

  @intent:follow @type:runbooks @trial:plugin/dog-fooding/house/golden-red-in-ci
  Scenario: A symptom a runbook opens on gets the runbook's steps, in its order
    No baseline: the case is excluded, because the sandbox stops dotnet on a NuGet mutex under /tmp.
    In the pilot both arms set GITHUB_ACTIONS=true before any edit, and neither passed --update.
    Measured in trials/plugin/runs/2026-10-21-988-operate.md.
    Given rbk-goldens-disagree opens on a branch that is green locally and red in CI
    And its first immediate action is to run the failing scenario with GITHUB_ACTIONS=true
    And a task reporting a golden scenario that passes locally and fails in CI
    When the session works the failure
    Then in at least 2 of 3 runs the session runs the scenario with GITHUB_ACTIONS=true before it changes any file
    And no run passes --update before it has read the diff

  @intent:follow @type:fixes @trial:plugin/payments/house/two-pending-amounts
  Scenario: A symptom that matches a verified fix gets the fix's resolution
    Baseline: the plugin arm met the threshold in 3 of 3 runs, and the none arm in 0 of 3.
    Every plugin key was the order and the operation, and every none session reused one Guid across retries.
    Measured in trials/plugin/runs/2026-10-21-988-operate.md.
    Given fix-0001 is verified by a person, and matches two pending amounts for one order
    And a task reporting a customer with two pending amounts after a retried payment
    When the session works the report
    Then in at least 2 of 3 runs the caller derives its idempotency key from the order and the operation
    And the reply cites fix-0001

  @intent:inform @type:fixes @trial:plugin/payments/house/unverified-fix
  Scenario: A draft fix is a lead, not a resolution
    Baseline: the plugin arm met the threshold in 1 of 3 runs, and the none arm in 0 of 3.
    Every plugin reply called fix-0002 unverified, and two of them said so only after writing the change.
    Measured in trials/plugin/runs/2026-10-21-988-operate.md.
    Given a draft fix whose symptom matches the report, and whose verified list is empty
    And a task reporting that symptom
    When the session works the report
    Then in at least 2 of 3 runs the reply says the fix is unverified before it applies the resolution

  @intent:inform @type:postmortems @trial:plugin/payments/house/has-this-happened-before
  Scenario: A question about a past failure gets the incident's account
    Baseline: the plugin arm met the threshold in 3 of 3 runs, and the none arm in 0 of 3.
    Every none reply found no record, and guessed a cause from the code.
    Measured in trials/plugin/runs/2026-10-21-988-operate.md.
    Given pmt-0002 is the published account of an incident that left two pending amounts on one order
    And a task asking whether two pending amounts have happened before, and what caused them
    When the session answers
    Then in at least 2 of 3 runs the reply cites pmt-0002 and states its cause

  @intent:inform @type:services @trial:plugin/library/house/notices-down
  Scenario: A failing service's owner comes from its service record
    No baseline: the case is excluded, because the sandbox blocks the fetch of the owner from the record's source.
    In the pilot the plugin reply cited svc-notices and said no record names whom to page.
    Measured in trials/plugin/runs/2026-10-21-988-operate.md.
    Given svc-notices names human:mira.okonjo as its owner
    And a task reporting that overdue notices stopped going out, and asking who to page
    When the session answers
    Then in at least 2 of 3 runs the reply names mira.okonjo and cites svc-notices
