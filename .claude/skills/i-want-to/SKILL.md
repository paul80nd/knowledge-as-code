---
name: i-want-to
description: Route a piece of work in this repository to the playbook carrying its steps. Covers writing and reviewing a record, changing the schema, changing `kac`, adding a knowledge type, sweeping prose, writing the public documentation, and opening a pull request. Load it before planning any of those, and whenever somebody types /i-want-to.
---

# I want to

Match the work to a playbook below, open that file, and copy its steps into your task list **before** you plan anything
task-specific. The failure this prevents is reading a playbook and then writing a bespoke plan that quietly drops its
steps.

**A step you decide not to do stays in the list**, with `skip:` and one line saying why. Dropping it silently is not
allowed.

**Where no playbook fits, say so and work without one.** Forcing a task into the nearest playbook is worse than carrying
none.

## The playbooks

| I want to                                           | Playbook                                                      |
|-----------------------------------------------------|---------------------------------------------------------------|
| write a record, or rewrite one                      | [adding-a-record](playbooks/adding-a-record.md)               |
| review records against the rules                    | [reviewing-records](playbooks/reviewing-records.md)           |
| change the schema, or write a rule                  | [changing-the-schema](playbooks/changing-the-schema.md)       |
| change `kac`, or add a check                        | [changing-the-tool](playbooks/changing-the-tool.md)           |
| add a knowledge type                                | [adding-a-type](playbooks/adding-a-type.md)                   |
| apply the writing rules across a folder             | [sweeping-prose](playbooks/sweeping-prose.md)                 |
| write the README, the package page or the docs site | [writing-public-docs](playbooks/writing-public-docs.md)       |
| open a pull request                                 | [opening-a-pull-request](playbooks/opening-a-pull-request.md) |

**Every other playbook ends by running `opening-a-pull-request`.** It is where the version, the changelog and the pages
your change made wrong are dealt with.

## Which writing skill

`technical-writing` is the floor and every surface loads it first. One voice goes on top, chosen by who reads the words
rather than by which folder holds them.

| You are writing                                                       | Load next             |
|-----------------------------------------------------------------------|-----------------------|
| a record, a type page, or a `description:` and `notes:` in `.schema/` | `writing-a-record`    |
| a comment, a feature document, a test name, or the changelog          | `writing-in-the-tool` |
| the root `README.md`, `PACKAGE.md`, or the documentation site         | `writing-the-docs`    |
| a commit message or a pull request body                               | nothing more          |

## What holds whatever you are doing

These are not repeated in the playbooks. Read them where they live.

* [`CLAUDE.md`](../../../CLAUDE.md) at the root says which of the four guidance pages your work answers to, and what has
  already cost a session here.
* [`example/CLAUDE.md`](../../../example/CLAUDE.md) carries the corpus conventions and the commands for all four test
  layers.
* **Run one `kac` invocation at a time.** Concurrent runs build the same project and contend over its output.
* **Branch and open a pull request.** A push to `main` is rejected.
