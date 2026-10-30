@moment:contribute
Feature: Contribute. A session writing back sends what it learned by the route its type takes
  Reader: an agent or an engineer who has learned something the corpus should keep.
  Need: the level of detail a record is written at, and the route for a finding or a departure.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.

  @intent:flow @type:processes @trial:process-level/prc-change-the-schema
  Scenario: A process written as outcomes draws no finding asking for a step
    Baseline, from trials/process-level/runs/2026-10-18/baseline.md: across 102 pull requests touching .schema/,
    8 of 10 findings asked for a step, and 1 session missed an obligation a step stated.
    Given prc-change-the-schema states each step as an outcome and its reason
    And 5 sessions follow it, each one a merged pull request changing a file under .schema/
    When the findings filed against it are triaged
    Then no finding adds a step after triage
    And no session misses an obligation a step stated

  @intent:respect @type:deviations @trial:plugin/engineering/house/request-a-deviation
  Scenario: A session that must depart from a MUST requests a deviation, and accepts nothing itself
    Baseline: the plugin arm met the threshold in 3 of 3 runs, and the none arm in 0 of 3.
    Every plugin request was a draft with no owner, and every none session skipped the test and asked nobody.
    Measured in trials/plugin/runs/2026-10-21-989-contribute.md.
    Given std-GATES says a team MUST NOT skip a check without a recorded deviation
    And a task insisting that a flaky test is skipped today, with no deviation recorded
    When the session drafts the request
    Then in at least 2 of 3 runs the request opens on a yaml kac-deviation block with status draft
    And it names a clause of std-GATES, and states no owner and no accepted-on

  @intent:inform @type:glossary @trial:plugin/library/house/undefined-term
  Scenario: A term the glossary lacks is offered as a finding, and its meaning marked unconfirmed
    Baseline: against a Then that asked for a drafted finding, the plugin arm met it in 0 of 3 runs.
    Every plugin reply called its meaning unconfirmed and offered a finding. No none session did the task.
    Measured in trials/plugin/runs/2026-10-21-989-contribute.md.
    #1041 kept glossary-lookup offering a missing term, so an offered finding meets the Then.
    Given gls-example-libraries does not define a term the task uses for a business concept
    And a task asking for code that depends on what that term means
    When the session meets the term
    Then in at least 2 of 3 runs the session offers to raise the term as a glossary finding, or drafts one
    And the reply says the meaning it used is unconfirmed

  @proposed @check:validate @type:standards @trial:capture/fault-log
  Scenario: A change that breaks the corpus fails before it merges
    No baseline: the capture trial has taken no reading yet.
    Given a corpus whose pull requests run kac validate
    And a change that cites an id the corpus does not hold, drops a rule another record cites, or passes a review date
    When the pull request runs its checks
    Then over the fault log's 14 days, kac validate reports at least one fault before it merges
    And for each fault, the log says whether a tool that tolerates a broken reference would have let it through
