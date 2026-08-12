# Contributing

**This repository is not open to contributions.**

The pattern is still being worked out. The schema, the checks and the framework's own vocabulary move from week to week,
and a change that looks isolated usually binds something three files away. Reviewing outside work against a target that
is still moving costs more than it returns, and it costs the contributor most.

## Issues are working notes, not a queue

Issues here are where a problem gets thought through before it is fixed. Most are written by the maintainer to the
maintainer: open questions, half-formed asks, and premises that a later change turns out to have overtaken. Labels sort
them for the person who wrote them.

They are not an offer of work. Nothing here is available to claim, and issues are not assigned, so please do not ask to
be assigned one.

## What is welcome

* **Issues.** A bug, a contradiction, a rule that does not survive contact with a real corpus.
* **Questions.** About the taxonomy, the sync model, or why something is shaped the way it is.
* **Corrections of fact.** Most valuable where the documentation and the tool disagree.

The maintainer reads all three. Some will sit, and some will be closed without action — that is a judgement about
scope, not about the person who raised it.

## Pull requests

A pull request opened without a prior conversation is closed unmerged, whatever its quality. We say that plainly so
nobody spends an evening on work that was never going to land.

## Take a copy instead

This framework is **copied, not depended on**. The intended way to use it is to take your own cut: clone it, delete the
types you do not want, and change whatever you like in your copy. There is no runtime dependency on this repository and
nothing to remove if you later go your own way. The [README](../README.md) explains the model, and `.corpus.yaml`
records which version of the shared layer a copy is running and where it has deliberately stepped away.

A copy that diverges is the design working, not a failure to contribute upstream.

## The other contributing guide

[`knowledge-as-code/contributing.md`](../knowledge-as-code/contributing.md) is part of the framework, not part of this
policy. It covers how a record is contributed to a corpus *built with* this framework — which template to copy, how
review works, what a reviewer checks. It says nothing about contributing to this repository.

## What would change this

Two things: the pattern settling enough that the schema stops moving under a reviewer's feet, and a second corpus
running the framework in earnest, which is the only thing that shows which parts are general. Until both hold, the
answer is no, and the churn here is the whole reason.
