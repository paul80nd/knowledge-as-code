@moment:learn
Feature: Learn. A person new to the team reads the corpus as a wiki and can act on it
  Reader: a person new to the team, or a reader standing in for one, with the corpus and nobody to ask.
  Need: the domain and the house style, found by reading the records.
  Each scenario is a hypothesis. It holds where its trial case passes at the threshold its Then states.
  The trial is the comprehension trial, given a corpus in place of a section of the documentation site.
  Readers A, B and C each answer every question, as that trial runs them.

  @intent:inform @type:glossary @trial:comprehension/library
  Scenario: A new reader tells two of the estate's words apart
    Baseline: 3 of 3 readers answer an item and cite it, in trials/comprehension/runs/2026-10-21-library.md.
    Given gls-example-libraries defines an item as one copy of a title, and says only an item can be lent
    When a reader given examples/library is asked whether a borrower borrows a title or an item
    Then at least 2 of 3 readers answer an item, and cite gls-example-libraries

  @intent:inform @type:explanations @trial:comprehension/library
  Scenario: A new reader learns how a part of the estate works from its explanation
    Baseline: 3 of 3 readers answer as it states and cite it, in trials/comprehension/runs/2026-10-21-library.md.
    Given exp-covers-path explains how a jacket image reaches the page a borrower sees
    When a reader given examples/library is asked what a borrower sees when a leg of that path fails
    Then at least 2 of 3 readers answer as exp-covers-path states, and cite it

  @intent:inform @type:offerings @trial:comprehension/library
  Scenario: A new reader learns what the estate offers, and what is not live yet
    Baseline: 2 of 3 readers say it is still being built, in trials/comprehension/runs/2026-10-21-library.md.
    Two of the three readers still answered "Yes" when asked whether a borrower can reserve today.
    Given ofr-reservations is building, and offers a hold on an item held at another branch, for a fee
    When a reader given examples/library is asked whether a borrower can reserve an item from another branch today
    Then at least 2 of 3 readers say the offering is still being built, and cite ofr-reservations
