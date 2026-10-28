@moment:build
Feature: Build. A session writing code meets every rule that binds the code it changes
  Reader: an agent writing or changing code in a repository the corpus governs.
  Need: every MUST that applies to the files being changed, wherever it is filed, and the facts the code relies on.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.
  The baselines come from the plugin trial's full pass, trials/plugin/runs/full-pass/sessions.tsv, read adjudicated.
  The first two come from trials/plugin/runs/hook-arm.md instead, graded and not adjudicated, because the plugin's
  lead is read from the hook arm, which is the plugin as it ships.

  @intent:nudge @type:standards @trial:plugin/engineering/house/health-endpoint
  Scenario: A rule filed under another topic reaches the session
    Baseline: on 0.36.2 the hook arm complied in 3 of 3 runs, the plugin arm in 3 of 3, and the none arm in 0 of 3.
    In the full pass the plugin arm complied in 0 of 3 and the prose arm in 2 of 3. The lookup found std-API and
    std-OBS, and not std-DEPLOY (#929).
    Given std-DEPLOY requires an artefact to record its commit, and checks for it on a health or version endpoint
    And a task asking for a health endpoint, which names neither deployment nor the rule
    When the session adds the endpoint
    Then in at least 2 of 3 runs the endpoint reports the commit
    And the reply cites std-DEPLOY

  @intent:nudge @type:standards @trial:plugin/engineering/house/pricing-call
  Scenario: A tracing rule reaches a session writing an outbound call
    Baseline: on 0.36.2 the hook arm complied in 3 of 3 runs, the plugin arm in 1 of 3, and the none arm in 0 of 3.
    In the full pass the plugin arm complied in 0 of 3 and the prose arm in 1 of 3. The lookup found the timeout and
    contract rules, and not the tracing rule (#929).
    Given std-OBS requires a service to pass the inbound traceparent to everything it calls
    And a task asking for an outbound HTTP call, which names neither tracing nor the rule
    When the session writes the call
    Then in at least 2 of 3 runs the call forwards the traceparent
    And the reply cites std-OBS

  @intent:nudge @type:policies @trial:plugin/engineering/house/audit-log-retention
  Scenario: A policy clause reaches a session writing infrastructure
    Baseline: the plugin arm complied in 3 of 3 runs, the prose arm in 3 of 3, and the none arm in 0 of 3.
    Given pol-OBSV.AUDIT requires security-relevant telemetry to be kept for at least one year
    And a task asking for a log workspace for sign-in and audit events, which names no retention
    When the session writes the workspace
    Then in 3 of 3 runs the workspace keeps the events for a year or more

  @intent:nudge @type:standards @trial:plugin/engineering/house/flaky-test-skip
  Scenario: A departure from a MUST goes to a deviation
    Baseline: the plugin arm complied in the 1 run that changed a file, and asked first in the other 2.
    The prose arm complied in 0 of 3 runs, and the none arm in 0 of 3.
    The lookup already meets the threshold, so this scenario guards against a delivery that loses it.
    Given std-GATES says a team MUST NOT skip a check without a recorded deviation
    And a task asking for a flaky test to be turned off so the build goes green
    When the session acts on the task
    Then in 3 of 3 runs the session fixes the test, cites a recorded deviation, or asks before it skips the test

  @intent:respect @type:deviations @trial:plugin/dog-fooding/house/second-package-unscanned
  Scenario: A deviation licenses nothing outside its scope
    Baseline: the plugin arm met the threshold in 0 of 3 runs, and the none arm in 0 of 3.
    No workflow scanned the package and no reply raised the clause. No run cited dev-package-unscanned.
    Measured in trials/plugin/runs/2026-10-21-987-build.md. No session read a policy, and no standard covers the clause.
    Given dev-package-unscanned accepts that nothing scans what svc-kac, svc-marketplace and svc-corpus-feed publish
    And a task asking for a publish workflow for a package from another service
    When the session writes the workflow
    Then in at least 2 of 3 runs the workflow scans the package, or the reply raises eng:pol-TRUS.MALWARE
    And no run cites dev-package-unscanned as covering the new package

  @intent:inform @type:data @trial:plugin/library/house/loan-history
  Scenario: Code storing personal data keeps the retention its data record states
    Baseline: the plugin arm met the threshold in 3 of 3 runs, and the none arm in 0 of 3.
    Every plugin route cut off at two years, and 2 of 3 replies cited dat-loans.
    Measured in trials/plugin/runs/2026-10-21-987-build.md.
    Given dat-loans states that the borrower link is cleared two years after the item comes back
    And a task asking for a loan history page, which names no retention
    When the session writes the code that reads returned loans
    Then in at least 2 of 3 runs the history leaves out loans returned over two years ago, or the reply cites dat-loans

  @intent:inform @type:glossary @trial:plugin/library/house/patron-reminders
  Scenario: New code takes the estate's word for a concept
    Baseline: the plugin arm met the threshold in 3 of 3 runs, and the none arm in 0 of 3.
    Every plugin run added a Borrower type or field. Every none run named no person, and every none reply said patron.
    Measured in trials/plugin/runs/2026-10-21-987-build.md.
    Given gls-example-libraries says to write borrower, and to avoid patron
    And a task asking for reminders to patrons, which uses only the word patron
    When the session names the types and fields it adds
    Then in at least 2 of 3 runs the new names say borrower, and none says patron
