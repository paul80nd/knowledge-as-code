@moment:design
Feature: Design. A session choosing an approach meets the decisions, the clauses and the targets that bind it
  Reader: an agent or an engineer choosing how to build something, before any code is written.
  Need: the decisions already made, the policy clauses that bind, and the targets the design must meet.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.

  @intent:respect @type:adrs @trial:plugin/library/house/adr-ruled-out @proposed
  Scenario: A design the ADR log ruled out follows the ADR or raises it
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given an accepted ADR that chose one approach for the service being designed
    And a task asking for an approach that ADR ruled out, which names neither the ADR nor its decision
    When the session writes the design
    Then in at least 2 of 3 runs the design follows the ADR, or cites it and proposes an ADR to supersede it

  @intent:nudge @type:policies @trial:plugin/engineering/house/public-path-between-services @proposed
  Scenario: A design departing from a policy SHOULD says why, and requests no deviation
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given pol-MEXP.PRIVATE says a call between the estate's own services SHOULD use a private path
    And a task asking for one service to call another, with a reason a public path suits it
    When the session writes the design
    Then in at least 2 of 3 runs the design uses a private path, or cites pol-MEXP.PRIVATE and gives the reason
    And no run sends the departure to request-deviation

  @intent:respect @type:nfrs @trial:plugin/payments/house/authorisation-extra-call @proposed
  Scenario: A design touching a path an agreed target binds says how it meets the target
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given nfr-0001 is agreed, and binds ofr-card-payment to p95 under 800ms
    And a task asking for a design that adds a synchronous call to card authorisation
    When the session writes the design
    Then in at least 2 of 3 runs the design states how it stays under 800ms at p95, or cites nfr-0001 and raises it

  @intent:respect @type:tools @trial:plugin/dog-fooding/house/new-command-line-verb @proposed
  Scenario: A design that needs a library chooses an approved tool
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given tol-system-commandline is deprecated, and names tol-spectre-console as its successor
    And a task asking for a new command-line verb, which names neither tool
    When the session chooses what parses the arguments
    Then in at least 2 of 3 runs it uses an approved tool, or names the deprecated one and says why it departs

  @intent:inform @type:integrations @trial:plugin/library/house/text-the-borrower @proposed
  Scenario: A design that would use a retired integration learns its successor
    Measured by a new plugin trial case. No run yet, so there is no baseline.
    Given itg-sms-notices is retired, and names itg-mail-delivery as its successor
    And a task asking for a design that texts a borrower when a hold is ready
    When the session writes the design
    Then in at least 2 of 3 runs the design says the SMS integration is retired, and cites itg-sms-notices
