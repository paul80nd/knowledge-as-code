---
name: i-want-to
description: Route a piece of work in this repository to the playbook carrying its steps. Load it before planning any change here, however the request was phrased: "tidy up", "have a look at", "get rid of", "review", "clean up", "fix" and "add" all land somewhere in it. Covers writing and reviewing a record, changing the schema, changing `kac`, adding a knowledge type, sweeping prose, writing the public documentation, and opening a pull request. Also whenever somebody types /i-want-to.
---

# I want to

Match the work to a playbook below, open that file, and copy its steps into your task list **before** you plan anything
task-specific. The failure this prevents is reading a playbook and then writing a bespoke plan that quietly drops its
steps.

**A step you decide not to do stays in the list**, with `skip:` and one line saying why. Dropping it silently is not
allowed.

**Where no playbook fits, say so and work without one.** Forcing a task into the nearest playbook is worse than carrying
none.

**Count first, from the files, whatever the playbook says.** A request carries a claim about the corpus, and the claim
is often wrong. A folder it names may hold no records at all. A mark said to be everywhere may be a handful, each one
the form a rule deliberately keeps. Establish that the thing exists and how much of it there is before planning what to
do about it, and report the count where it answers the request on its own.

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

* [`CLAUDE.md`](../../../CLAUDE.md) at the root says which of the four guidance pages your work answers to, carries the
  conventions and the commands for all four test layers, and names what has already cost a session here.
* **A corpus's own `CLAUDE.md` carries what is that corpus's alone**: the estate it extends, and the producer it has to
  pack before it can restore.
* **Run one `kac` invocation at a time.** Concurrent runs build the same project and contend over its output.
* **Branch and open a pull request.** A push to `main` is rejected.
