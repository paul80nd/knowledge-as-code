# Landscape review, issue #1160: agent context tools

Family: Cursor rules, Claude Code skills and plugins (with the open Agent Skills format), MCP knowledge servers
(the MCP spec, the reference memory server, Context7, two AWS servers), and Windsurf/Devin and Cline rule files in
brief. Everything below was read on **2026-10-27** unless a line says otherwise. Anything not read directly is marked
**(inferred)**.

Not repeated here, because `docs/compared.md` covers it: `CLAUDE.md` / `AGENTS.md`, `.claude/rules/` `paths:`,
managed org instructions, Microsoft Learn MCP, Rovo, APM, CodeRabbit.

## What kac is, for this comparison

Read from the repository: `README.md`, `docs/compared.md`, `docs/framework/intent.md`, `docs/framework/principles.md`,
`docs/maintaining/plugin.md`, `docs/maintaining/export.md`, `docs/use/skills.md`, `template/.plugin/`.

* `kac export` writes records as JSON and JSONL under `.dist/export/`; `kac bundle` copies that export byte for byte
  into a Claude Code plugin with lookup skills, two hooks and generated views, plus a one-plugin marketplace
  (`docs/maintaining/plugin.md`).
* Delivery today: a `SessionStart` breadcrumb (corpus, counts, which skill to ask); a `PreToolUse` hook that injects
  the **build index** (every standard rule by title) once per session at the first tool call, capped at 10,000
  characters; lookup skills loaded by description match, which grep `rules.jsonl` / `clauses.jsonl` / `terms.jsonl` and
  open the per-record JSON (`template/.plugin/hooks/hooks.json`, `template/.plugin/skills/standards-lookup/SKILL.md`).
* Retrieval is whole-file JSONL read by the agent's own tools. No index, no embedding, no server.
* Validation: `kac validate` against per-type YAML schemas, cross-reference resolution, reciprocal edges, rule ids and
  levels (`README.md`, `docs/framework/principles.md`).
* Sharing: a corpus `consumes:` another; the producer's export is merged into the consumer's flat files under its
  shortcode, so one plugin carries both (`docs/maintaining/export.md`). Plugin version = corpus `content-version`
  (`docs/maintaining/plugin.md#the-plugins-version`).
* Explicitly not done: nothing loads by path (`docs/framework/intent.md`, "No record states which files a rule applies
  to, so nothing loads by path", issue #993).

---

## 1. Cursor rules (`.cursor/rules`, MDC) and Cursor plugins

Sources: <https://cursor.com/docs/context/rules>, <https://cursor.com/docs/context/skills>,
<https://cursor.com/docs/plugins> (all read 2026-10-27).

**What it gives, and when it loads.** A rule is an `.mdc` file under `.cursor/rules` with frontmatter `description`,
`globs`, `alwaysApply` ([rules][cur-rules]). Four ways to attach:

* *Always Apply* (`alwaysApply: true`): every chat.
* *Apply to Specific Files* (`globs`): "attached when a matching file is in context".
* *Apply Intelligently* (`description` only): "the description of the rule will be presented to the Cursor Agent to
  decide if it should be applied".
* *Apply Manually*: only when `@`-mentioned.

Cursor also reads Agent Skills (`SKILL.md`) from `.cursor/skills/`, `.agents/skills/` **and `.claude/skills/`**,
discovered at startup and loaded by description, with `paths` to scope and `disable-model-invocation`
([skills][cur-skills]).

**Validation.** None of rule content. The docs describe no schema check of frontmatter values; a `.md` without
frontmatter is skipped ([rules][cur-rules]). Plugins in Cursor's public marketplace are "manually reviewed before
it's listed" and "must be open source" ([plugins][cur-plugins]): a human gate, not a content check.

**Sharing and versioning.** Project rules are version-controlled files. *Team Rules* are written in the Cursor
dashboard; "When enabled, the rule is required for all team members and cannot be disabled", and they support globs.
Order: "Team Rules → Project Rules → User Rules" ([rules][cur-rules]). Rules from another repository arrive only inside
a plugin: "Rules aren't imported on their own" ([rules][cur-rules]). A Cursor plugin packages "rules, skills, agents,
commands, MCP servers, and hooks"; team marketplaces exist on Teams/Enterprise; admins set each plugin Default Off,
Default On or **Required**; Cursor "re-indexes a marketplace at most once every 10 minutes", and a running Cloud Agent
keeps the versions it started with ([plugins][cur-plugins]). The rules page "says nothing about versioning" of imported
rules (my reading of [rules][cur-rules]).

**Retrieval and scale.** Whole-file injection. Guidance: "Keep rules under 500 lines" and split large rules; reference
files rather than inline them ([rules][cur-rules]). Scale is bounded by how many descriptions and always-on rules fit
in context (inferred).

**Versus kac.**

* *Same.* Markdown in git, loaded by description match (kac's lookup skills are the Apply Intelligently pattern). Both
  can package as a plugin with skills and hooks.
* *Differs.* Cursor injects the rule text itself; kac injects a pointer (build index) and a skill searches JSONL for
  the rule. Cursor's Team Rules are typed into a dashboard and not reviewed by pull request.
* *Lacks (kac).* **Glob attachment**: a Cursor rule or skill can load when a matching file is touched. kac has no
  `paths` on any record or generated skill (intent.md, #993). **Enforced, non-disableable org rules** via Team Rules
  or a Required plugin. **A Cursor output**: Cursor reads `.claude/skills/` and has its own plugin format with rules,
  skills, hooks and MCP, so a `.cursor-plugin` target is reachable, but kac's skills address `${CLAUDE_PLUGIN_ROOT}`,
  which Cursor's docs never mention (inferred: a kac skill copied to `.claude/skills/` would not find its export
  under Cursor).
* *kac ahead.* Rule ids and levels, schema validation, citation checks, a consumed corpus merged under a shortcode.
  Cursor rules are free text that nothing validates.
* *Simpler is enough.* One repo, a handful of conventions keyed to file types: a few glob-scoped `.mdc` files do the
  job with no tool.

## 2. Claude Code skills, plugins and marketplaces (and the Agent Skills format)

Sources: <https://code.claude.com/docs/en/skills>, <https://code.claude.com/docs/en/plugin-marketplaces>,
<https://code.claude.com/docs/en/plugins/host-marketplace.md>, <https://code.claude.com/docs/en/plugins/org.md>,
<https://code.claude.com/docs/en/plugins/dependencies.md>, <https://code.claude.com/docs/en/plugins/relevance.md>,
<https://code.claude.com/docs/en/plugins/cli-reference>, <https://agentskills.io/specification> (all read 2026-10-27).

This is kac's own delivery target, so the point is what the platform already offers that kac does not use.

**What it gives, and when it loads.** "skill descriptions are loaded into context so Claude knows what's available,
but full skill content only loads when invoked"; the body "stays there across later turns" ([skills][cc-skills]).
Description plus `when_to_use` is "truncated at 1,536 characters in the skill listing" ([skills][cc-skills]); the open
spec caps `description` at 1,024 ([agentskills][as-spec]). Frontmatter includes **`paths`** ("Claude loads the skill
automatically only when working with files matching the patterns"), `disable-model-invocation`, `context: fork`,
`hooks`, `allowed-tools` ([skills][cc-skills]). On compaction, "Claude Code re-attaches the most recent invocation of
each skill after the summary, keeping the first 5,000 tokens of each" within 25,000 tokens combined ([skills][cc-skills]).
Progressive disclosure in the spec: metadata ~100 tokens at startup, body < 5,000 tokens recommended, resources on
demand ([agentskills][as-spec]).

**Validation.** Lenient at runtime: "Claude Code ignores a field it doesn't recognize without reporting an error", and
malformed YAML still loads "with no fields set" ([skills][cc-skills]). `claude plugin validate [--strict]` checks
`marketplace.json`, `plugin.json`, and the skills, agents and commands in a directory, exit `0`/`1`/`2` for CI
([cli-reference][cc-cli]); `skills-ref validate` checks spec frontmatter ([agentskills][as-spec]). All of this checks the
**container**, not what the skill says. **`claude plugin eval`** runs prompt-plus-grader cases "several times in an
isolated session with only the target plugin loaded, and by default also without the plugin so the report shows the
difference" ([cli-reference][cc-cli]).

**Sharing and versioning.**

* Marketplace sources: relative path, `github`, `git-subdir`, `url`, `archive` (with `sha256`), `npm`, `command`
  ([marketplaces][cc-mkt]).
* Version: "Users get a new copy only when the plugin's computed version differs"; "If you set `"version": "1.0.0"` and
  push new commits without changing it, users don't receive them"; omit `version` to track commits
  ([host][cc-host]). Pin with `ref`/`sha` or `#<ref>` on add; release channels are two marketplaces
  ([host][cc-host]).
* **Plugin dependencies** with semver ranges resolved against `<plugin>--v<version>` git tags; conflicting ranges fail
  install; cross-marketplace dependencies need `allowCrossMarketplaceDependenciesOn` ([dependencies][cc-deps]).
* Org control: managed `extraKnownMarketplaces` + `enabledPlugins` install and force-enable on every machine;
  "disabling one at their own scope doesn't stop it from loading"; `strictKnownMarketplaces` allowlists sources;
  `autoUpdate` per marketplace ([org][cc-org]). Repository scope: commit `.claude/settings.json` ([org][cc-org]).
* **Relevance**: a marketplace entry's `relevance.signals` (`cwd`, `cli`, `hosts`, `filesRead`, `manifestDeps`) make
  Claude Code *suggest installing* the plugin when a session matches; matching is local; the user always confirms
  ([relevance][cc-rel]).

**Retrieval and scale.** No retrieval layer of its own: a skill's body and files enter context whole when read; search
is whatever the skill tells the agent to do with its tools. Cost scales with the number of skills listed ("Every skill in
the skill listing adds to your context on every turn", [skills][cc-skills]) and with the files a skill makes the agent
read (inferred).

**Versus kac.**

* *Same.* kac is a client of exactly this: `SKILL.md`, plugin, marketplace, `SessionStart` and `PreToolUse` hooks,
  `context: fork` for `standards-review`.
* *Differs.* kac ships one plugin per corpus and merges a consumed corpus into it at export time, rather than declaring
  the producer's plugin as a dependency. kac versions the plugin by `content-version`, not by commit.
* *Lacks (kac).*
  * **`paths` on a skill.** The platform already loads a skill by file glob; kac generates no per-standard or
    per-`applies-to` skill that would use it (#993 is the open question).
  * **Eval harness.** `claude plugin eval` measures plugin vs no-plugin on the same cases. kac measures with its own
    `trials/` (from CLAUDE.md); whether `trials/` could run on `plugin eval` I did not establish.
  * **Plugin dependencies.** A team plugin could depend on the org's plugin with `^2.0` and get org rule updates without
    the team re-exporting. kac's merge-at-export means an org change reaches a team only when the team restores,
    re-exports and re-bundles (inferred from `docs/maintaining/export.md`).
  * **Relevance signals.** kac's generated `marketplace.json` carries no `relevance` block
    (`docs/maintaining/plugin.md#the-marketplace` lists name, owner, source, description only).
  * **Org distribution.** kac docs print an install command; they do not mention `extraKnownMarketplaces` /
    `enabledPlugins` for a fleet (inferred from `bundle` docs read; I did not read every page).
* *kac ahead.* Everything about the content: the platform validates the manifest and the frontmatter, never whether a
  rule cited in a skill exists, whether a record is stale, or whether a standard implements a policy. kac also trims
  skills by the types exported, which the platform has no notion of.
* *Simpler is enough.* A team sharing a few trusted skills across repos needs only a git marketplace and
  `enabledPlugins`; no schema.

## 3. MCP knowledge servers

### 3a. The protocol

Sources: <https://modelcontextprotocol.io/specification/latest/server> (resolves to **2026-07-28**),
<https://modelcontextprotocol.io/specification/2026-07-28/server/tools>,
<https://modelcontextprotocol.io/specification/2025-11-25/server/resources> (the 2025-11-25 resources page; I did not
diff it against 2026-07-28). All read 2026-10-27.

* Control model: Prompts user-controlled, **Resources application-controlled**, **Tools model-controlled**
  ([server][mcp-server]). So a knowledge server reaches the agent mainly through tools the model decides to call, or
  resources the host decides to attach ("the protocol itself does not mandate any specific user interaction model",
  [resources][mcp-res]).
* Resources have URIs, `resources/list` with pagination, templates, `subscribe` / `listChanged`, and annotations
  `audience`, `priority` (0–1), `lastModified` ([resources][mcp-res]).
* Validation: a tool may declare `outputSchema`; "Servers **MUST** provide structured results that conform to this
  schema. Clients **SHOULD** validate" ([tools][mcp-tools]). That validates the **shape of a response**, not the truth
  or currency of the knowledge. "clients **MUST** consider tool annotations to be untrusted unless they come from
  trusted servers" ([tools][mcp-tools]).
* Versioning of content: nothing in the protocol. `lastModified` is a hint ([resources][mcp-res]).

### 3b. MCP reference memory server (knowledge graph)

Sources: <https://github.com/modelcontextprotocol/servers/tree/main/src/memory>,
<https://raw.githubusercontent.com/modelcontextprotocol/servers/main/src/memory/index.ts> (read 2026-10-27).

* Gives: entities (name, type, observations), relations "stored in active voice", tools `create_entities`,
  `create_relations`, `add_observations`, `delete_*`, `read_graph`, `search_nodes`, `open_nodes`; a
  `memory://knowledge-graph` resource ([memory][mem]).
* Loads: on request (tools). The agent writes as well as reads.
* Validation: zod schemas for entity/relation shape; lines that fail `safeParse` are kept out of the graph and written
  back unchanged ([index.ts][mem-src]). No referential checks beyond the shape (inferred from the summary of the code).
* Storage: one local `memory.jsonl` (`MEMORY_FILE_PATH`) ([memory][mem]). No sharing or versioning beyond whatever
  you do with that file (inferred).
* Retrieval: `search_nodes` is a case-insensitive substring match over name, type and observations
  (`e.name.toLowerCase().includes(query.toLowerCase())`), and `read_graph` returns the whole graph ([index.ts][mem-src]).
  Scales linearly; whole-graph reads grow with the graph (inferred).
* *Versus kac.* **Same**: a graph of typed nodes and edges, stored as JSONL (kac's flat files are JSONL too). **Differs**:
  the agent writes the graph directly, with no review; kac routes agent writes to the tracker and only a reviewed record
  reaches the export (`principles.md`, "In through the tracker"). **Lacks (kac)**: an agent-writable store at session
  speed, and a query tool (open-by-name, relations of a node) rather than grep over JSONL. **kac ahead**: reciprocal
  edges checked, owners, review dates, ids; the memory server has none. **Simpler is enough**: personal recall of facts
  across sessions, where nobody else must trust them.

### 3c. Context7 (Upstash)

Sources: <https://github.com/upstash/context7>, <https://context7.com/docs/adding-libraries> (read 2026-10-27).

* Gives: "up-to-date, version-specific documentation and code examples" via MCP tools `resolve-library-id`
  (`libraryName`, `query`) and `query-docs` (`libraryId`, `query`, optional `language`) or the `ctx7` CLI
  ([repo][c7]). Loads on request.
* Versions: you name the version in the prompt and "Context7 will automatically match the appropriate version"
  ([repo][c7]); owners "manage versions through a web admin panel" ([adding][c7-add]).
* Ingestion: "Anyone can add a public library — you don't need to own it"; indexes `.md`, `.mdx`, `.rst`, `.txt`,
  `.ipynb`; refresh "based on popularity", or on every push via a GitHub Action; private sources need "a Pro or
  Enterprise plan" ([adding][c7-add]). Owners can claim a library to unlock the admin panel ([adding][c7-add]).
* Validation / trust: Context7 "cannot guarantee the accuracy, completeness, or security of all library
  documentation"; there is a Report button ([repo][c7]). The API backend, parser and crawler "are private and not part
  of this repository" ([repo][c7]).
* Retrieval: server-side search ranked by `query` ([repo][c7]); the mechanism is not documented (embedding or not, I
  could not establish). Scales with the hosted index, not with the agent's context (inferred).
* *Versus kac.* **Same**: agent pulls a slice on demand rather than loading everything. **Differs**: Context7 indexes
  third-party docs anyone submits; kac holds what your organisation decided, reviewed by PR. **Lacks (kac)**: library
  version selection per query, ranked search, ingestion from an existing docs repo without rewriting it into records,
  refresh-on-push without a re-bundle and reinstall. **kac ahead**: ownership, schema, ids; Context7 disclaims accuracy.
  **Simpler is enough**: the agent needs a public library's current API.

### 3d. Vendor knowledge-base servers: AWS

Sources: <https://awslabs.github.io/mcp/servers/bedrock-kb-retrieval-mcp-server>,
<https://awslabs.github.io/mcp/servers/aws-knowledge-mcp-server> (read 2026-10-27).

* **Bedrock Knowledge Base Retrieval MCP server** (your own organisation's documents): `ListKnowledgeBases`,
  `QueryKnowledgeBases`; discovers knowledge bases tagged `mcp-multirag-kb=true`; "Works with vector knowledge bases";
  optional reranking; a `user_id` parameter for ACL-protected content; IAM credentials required; image results dropped
  ([bedrock-kb][aws-kb]). No content validation is described.
* **AWS Knowledge MCP Server** (AWS's own docs): `search_documentation`, `read_documentation`, `list_regions`,
  `get_regional_availability`, **`retrieve_skill`** ("Retrieve an AWS agent skill that provides domain-specific
  expertise, workflows, and step-by-step procedures"); no authentication, rate limited ([aws-knowledge][aws-k]).
* *Versus kac.* **Same**: retrieval on request, a citation back to the source. **Differs**: vector search over a
  managed index, with per-user ACLs, against kac's grep over a frozen export copied into every install. **Lacks
  (kac)**: semantic search, per-reader access control (every installer of a kac plugin gets the whole export), and
  live content without a reinstall. `retrieve_skill` shows a vendor serving skills over MCP at request time, which is
  one answer to kac's own open question of how fast-changing types reach an agent (intent.md, "Cadence and
  delivery", #946). **kac ahead**: nothing checks a Bedrock knowledge base's documents against a schema, or links a
  rule to the control that proves it. **Simpler is enough**: a large body of existing prose that people search, with
  access rules.

## 4. Windsurf / Devin and Cline rule files (brief)

* **Windsurf / Devin Desktop** (<https://docs.devin.ai/desktop/cascade/memories>, redirected from
  docs.windsurf.com, read 2026-10-27): `.devin/rules/*.md` or `.windsurf/rules/*.md`; `trigger` of `always_on`,
  `model_decision` (description up front, full file when relevant), `glob`, `manual`; limits of 6,000 characters
  global and 12,000 per workspace file; **system-level rules** deployed by IT under `/etc/devin/rules/` etc. that end
  users cannot modify; auto-generated memories are local and "not committed to your repository" ([devin][devin]).
* **Cline** (<https://docs.cline.bot/features/cline-rules>, read 2026-10-27): `.clinerules/` or `.cline/rules/`;
  conditional rules by `paths` globs; per-rule toggle; reads `.cursorrules` and `AGENTS.md` ([cline][cline]).
* *Versus kac.* Same pattern as Cursor: whole-file, description- or glob-triggered, unvalidated. Windsurf's hard
  character caps are a concrete constraint any kac output for that agent would have to meet (inferred).

---

## Cross-cutting conclusions

1. **Every tool in this family validates the container, none the content.** Cursor checks frontmatter presence;
   Claude Code checks manifests and frontmatter; MCP checks the shape of a tool result against `outputSchema`; the
   memory server checks entity shape. None checks that a cited rule exists, that a record has an owner or review date,
   or that edges agree. This is kac's clearest lead, and it is a property of the corpus, not of the delivery.
2. **Path-scoped loading is table stakes elsewhere and absent in kac.** Cursor `globs`, Windsurf `glob`, Cline
   `paths`, Claude Code skill `paths`, and Claude Code `relevance.signals.filesRead` all key delivery to the files
   being touched. kac pushes a title index of every rule instead and leaves path loading to #993.
3. **Distribution machinery kac could lean on but does not.** Plugin dependencies with semver ranges, managed
   `enabledPlugins`, relevance suggestions, Cursor Required plugins, and `claude plugin eval`. kac re-implements
   inheritance at export time (merge under shortcode) and measurement in `trials/`.
4. **Retrieval.** All file-based tools (Cursor, skills, kac) are whole-file plus agent-driven grep; only the MCP
   servers (Context7, Bedrock KB, AWS Knowledge) offer ranked or vector search and live content. kac's JSONL-and-grep
   is adequate at the example corpora's size; how it behaves at thousands of rules I could not establish (no
   measurement found in the files read).

## Things in kac's own docs that were unclear or that I could not establish

* **Version gating may strand skill fixes.** Plugin version = `content-version` (`docs/maintaining/plugin.md`), and
  Claude Code ships a new copy "only when the plugin's computed version differs" ([host][cc-host]). A change to a
  skill or hook from `template/` that does not move `content-version` would not reach installed users. I found no kac
  page that says so. (Inferred from the two statements together.)
* **The build index and compaction.** The build-index hook fires once per session, guarded by a marker directory
  (`docs/maintaining/plugin.md#the-build-index-hook`); the breadcrumb's matcher includes `compact`, the build index's
  does not (`template/.plugin/hooks/hooks.json`). After compaction the index may be summarised away and never
  re-sent. No kac page addresses this. (Inferred.)
* **"Nothing generated is exclusive to it" vs `${CLAUDE_PLUGIN_ROOT}`.** `docs/framework/intent.md` says skills use the
  open `SKILL.md` format and nothing generated is exclusive to Claude Code; every lookup skill addresses the export as
  `${CLAUDE_PLUGIN_ROOT}/corpus/…`, a Claude Code variable. `docs/compared.md` says the plugin "is built for Claude
  Code". The two pages read as being in tension; the intent page is direction, so perhaps not contradictory, but it is
  not said.
* **`requires` without a shape version.** In `template/.plugin/.claude-plugin/plugin.json` every skill pins
  `<type>@N`, but the `hooks` component says `"requires": ["glossary"]` and `"optional": ["standards"]` with no `@N`.
  `docs/maintaining/plugin.md` describes refusing a shape "that is not a whole number" but not whether a bare name is
  allowed. I could not tell from the docs read.
* **Org-wide rollout.** I found no kac page telling an organisation to use managed `extraKnownMarketplaces` /
  `enabledPlugins`; I did not read every page under `docs/use/`, so this may exist elsewhere.

[cur-rules]: https://cursor.com/docs/context/rules
[cur-skills]: https://cursor.com/docs/context/skills
[cur-plugins]: https://cursor.com/docs/plugins
[cc-skills]: https://code.claude.com/docs/en/skills
[cc-mkt]: https://code.claude.com/docs/en/plugin-marketplaces
[cc-host]: https://code.claude.com/docs/en/plugins/host-marketplace.md
[cc-org]: https://code.claude.com/docs/en/plugins/org.md
[cc-deps]: https://code.claude.com/docs/en/plugins/dependencies.md
[cc-rel]: https://code.claude.com/docs/en/plugins/relevance.md
[cc-cli]: https://code.claude.com/docs/en/plugins/cli-reference
[as-spec]: https://agentskills.io/specification
[mcp-server]: https://modelcontextprotocol.io/specification/latest/server
[mcp-res]: https://modelcontextprotocol.io/specification/2025-11-25/server/resources
[mcp-tools]: https://modelcontextprotocol.io/specification/2026-07-28/server/tools
[mem]: https://github.com/modelcontextprotocol/servers/tree/main/src/memory
[mem-src]: https://raw.githubusercontent.com/modelcontextprotocol/servers/main/src/memory/index.ts
[c7]: https://github.com/upstash/context7
[c7-add]: https://context7.com/docs/adding-libraries
[aws-kb]: https://awslabs.github.io/mcp/servers/bedrock-kb-retrieval-mcp-server
[aws-k]: https://awslabs.github.io/mcp/servers/aws-knowledge-mcp-server
[devin]: https://docs.devin.ai/desktop/cascade/memories
[cline]: https://docs.cline.bot/features/cline-rules
