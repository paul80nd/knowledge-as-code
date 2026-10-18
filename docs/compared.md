# Compared with alternatives

A `CLAUDE.md`, an organisation's managed instructions, a documentation server, a wiki, a package manager for agent
context and an AI reviewer all give an AI coding session context. This page says what each one already does well, what
Knowledge as Code (KaC) adds, and when the simpler option is enough. Each claim about another product cites that
product's own documentation, read between 13 and 18 October 2026.

A **corpus** is one repository of knowledge records kept in git. A **record** is one Markdown document in it, with a
declared **type** such as a standard or a runbook. The **plugin** is the corpus packaged for Claude Code.

This page describes the design. It claims no measured advantage over any of the options below.

## What KaC adds

Each option below gives an agent text to read. KaC adds four things to that text:

* **A rule has an id and a level.** Each rule in a standard has an id an agent can cite, such as
  `std-SECRET.a-secret-comes-from-the-store`. It states its level in bold, such as **MUST**.
* **A record is checked.** `kac validate` checks every record against the schema its type declares. A broken
  reference fails CI.
* **One standard applies in every repository that takes it.** A team's corpus consumes an organisation's corpus. It
  cites those records under the organisation's [shortcode](framework/metadata.md#the-shortcode).
* **A policy links to the standard and the control.** A standard's `implements:` cites the policy clauses it
  implements. A control is a record saying what checks a rule, and its `verifies:` cites the standards it checks.

## A `CLAUDE.md` or an `AGENTS.md`

A Markdown file in the repository gives the agent its instructions as it starts.

* **What it does well.** Claude Code loads every `CLAUDE.md` from the working directory upwards at the start of each
  session. It loads one in a subdirectory once the agent works there ([Claude Code: memory][cc-memory]). `AGENTS.md`
  is plain Markdown with no required fields. The nearest one in the directory tree takes precedence
  ([agents.md][agents-md]). Neither needs a tool. A rule in `.claude/rules/` can name `paths:` globs, and then loads
  only when Claude works with a matching file ([Claude Code: memory][cc-memory]).
* **What KaC adds.** Claude Code treats these files as "context, not enforced configuration"
  ([Claude Code: memory][cc-memory]). Nothing checks what they say. A record is checked against its schema, has an
  owner and a status, and gives each rule an id.
* **When it is enough.** One repository, one team, and rules that fit on one page.

## Instructions an organisation manages centrally

An administrator sets the instructions once, and they apply to every member of the organisation.

* **What it does well.** Claude Code reads a managed-policy `CLAUDE.md` from a fixed path on each machine. No user
  setting can exclude it ([Claude Code: memory][cc-memory]). In GitHub Copilot, an organisation owner types
  instructions into a text box in the organisation's settings. They apply to Copilot Chat, code review and the cloud
  agent on GitHub.com ([GitHub: organisation instructions][gh-org]). Personal and repository instructions rank above
  them, and Copilot receives all three ([GitHub: response customisation][gh-precedence]).
* **What KaC adds.** The organisation's corpus is reviewed by pull request and versioned. A team takes a new version
  when it restores, and `kac validate` fails a citation the new version broke.
* **When it is enough.** A few preferences that apply everywhere, such as the language a reply is written in.

## A documentation server reached over MCP

The agent queries a server for documentation through the Model Context Protocol (MCP).

* **What it does well.** The Microsoft Learn MCP Server lets an agent search Microsoft's documentation, fetch a whole
  article and search code samples. It needs no authentication. Its index refreshes after content updates, and in full
  once a day ([Microsoft Learn MCP Server][ms-learn-mcp]). The agent asks for what it needs, when it needs it.
* **What KaC adds.** The Microsoft Learn server returns what Microsoft published. A corpus states what your
  organisation decided, in records you write and review.
* **When it is enough.** The agent needs a product's public documentation, and none of your own rules.

## A wiki the agent is pointed at

The agent searches the pages people already keep, through a connector.

* **What it does well.** The Atlassian Rovo MCP Server searches Jira and Confluence in natural language. It fetches a
  page's content with the permissions the user already has ([Atlassian: Rovo search and fetch][rovo]). The knowledge
  stays where people already write it.
* **What KaC adds.** CI checks each record against its type's schema, and fails a citation that no longer resolves.
  A corpus can be published as a wiki too, from the same files the plugin is built from.
* **When it is enough.** The readers are people, and nobody needs to cite a rule or prove it is checked.

## A package manager for agent context

A manifest in the repository names the skills, instructions and MCP servers a project needs, and a tool installs them.

* **What it does well.** Microsoft's Agent Package Manager (APM) calls itself "a dependency manager for AI agents". A
  lockfile pins exact versions and content hashes, so a fresh clone resolves the same content. It installs into GitHub
  Copilot, Claude Code, Cursor, Codex, Gemini and other agents ([APM][apm]).
* **What KaC adds.** APM installs the content it is given, at the version it is given. `kac` checks what that content
  says: each record against its type's schema, and each citation against the record it names.
* **When it is enough.** You are sharing skills and instructions you already trust, and nobody needs to cite or check a
  rule in them.

## An AI reviewer given your guidelines

A review bot reads your instruction files and checks each pull request against them.

* **What it does well.** CodeRabbit reads `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md` and other agents'
  rule files as code guidelines. Each file applies to its own directory and the directories below it
  ([CodeRabbit: code guidelines][coderabbit]).
* **What KaC adds.** A guideline file is free text, so a finding cannot cite the rule it applied. Each rule in a KaC
  standard has an id, and a control records which rules a tool already checks.
* **When it is enough.** Your rules fit in a few files, and nobody has to trace a review finding back to a rule.

## What KaC does not do

* **It does not enforce a rule on code.** The plugin gives a session context, as a `CLAUDE.md` does. A control
  records what checks a rule. CI on the corpus checks the records. [Design intent](framework/intent.md) states where
  review after the work is going.
* **The plugin is built for Claude Code.** Another agent can read the Markdown in a checkout. It does without the
  plugin's skills, which look a record up when the work calls for it.
* **It costs a tool and a schema.** A corpus needs `kac`, the .NET SDK and a CI job to check it.

KaC works alongside the other options. The `kac` repository's own
[`CLAUDE.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/CLAUDE.md) sends a session to the skills its
plugin ships.

## Where to go next

[One corpus or several](use/one-corpus-or-several.md) helps you decide how many corpora to keep, once you have decided to
keep one.

[agents-md]: https://agents.md/
[apm]: https://microsoft.github.io/apm/
[cc-memory]: https://code.claude.com/docs/en/memory
[coderabbit]: https://docs.coderabbit.ai/knowledge-base/code-guidelines
[gh-org]: https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/add-custom-instructions/add-organization-instructions
[gh-precedence]: https://docs.github.com/en/copilot/concepts/prompting/response-customization
[ms-learn-mcp]: https://learn.microsoft.com/en-us/training/support/mcp
[rovo]: https://support.atlassian.com/atlassian-rovo-mcp-server/docs/using-rovo-search-and-fetch-in-the-atlassian-remote-mcp-server
