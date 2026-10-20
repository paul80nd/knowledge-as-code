# Installing a corpus plugin

A corpus plugin is a corpus (one repository of knowledge records kept in git) packaged for Claude Code. A reader
installs it from the marketplace your corpus publishes, and takes each new version from there. Turn on auto-update for
that marketplace, and a version you publish loads in the reader's next session. Claude Code leaves auto-update off for
every marketplace but Anthropic's official ones.

```text
/plugin marketplace add paul80nd/knowledge-as-code@marketplace   # register the marketplace
/plugin install example-dogfooding@knowledge-as-code             # install one plugin from it
```

Both commands run inside a Claude Code session. [`bundle`](cli/bundle.md) writes a plugin and a marketplace offering
it, and does not publish either. Push that marketplace to a repository your readers can clone. The examples on this
page use the marketplace this repository publishes on its `marketplace` branch.

## The names an install uses

| Name        | Where it comes from                                                      | In the examples                          |
|-------------|--------------------------------------------------------------------------|------------------------------------------|
| Source      | The repository containing the marketplace, then `@` and the branch       | `paul80nd/knowledge-as-code@marketplace` |
| Marketplace | `name` in the marketplace's `.claude-plugin/marketplace.json`            | `knowledge-as-code`                      |
| Plugin      | `corpus` in `.corpus.yaml`, which `bundle` writes as the plugin's `name` | `example-dogfooding`                     |

A marketplace `bundle` writes has the plugin's name. Where a pipeline publishes several plugins in one marketplace,
that marketplace has a name of its own.

## Auto-update for everyone in a repository

Commit the marketplace to the repository's `.claude/settings.json`, with `autoUpdate` set to `true`:

```json
{
  "enabledPlugins": {
    "example-dogfooding@knowledge-as-code": true
  },
  "extraKnownMarketplaces": {
    "knowledge-as-code": {
      "source": { "source": "github", "repo": "paul80nd/knowledge-as-code", "ref": "marketplace" },
      "autoUpdate": true
    }
  }
}
```

* The key under `extraKnownMarketplaces` is the marketplace name.
* `enabledPlugins` takes `<plugin>@<marketplace>`.
* Claude Code registers the marketplace once a reader accepts the workspace trust dialog for the folder.
* A marketplace `bundle` writes lists its plugin by a relative path, so the plugin loads with no install step.

`claude plugin marketplace add <source> --scope project` writes the `extraKnownMarketplaces` entry for you. Check that
the entry it wrote has `"autoUpdate": true`, and add it where it does not.

## Auto-update for a whole organisation

An administrator puts the same `extraKnownMarketplaces` entry, with `"autoUpdate": true`, in
[managed settings](https://code.claude.com/docs/en/plugins/org#set-update-policy). Auto-update is then on for everyone
who receives those settings, and Claude Code refuses a reader's own toggle. A managed `enabledPlugins` entry installs
the plugin and turns it on for each of them.

## Auto-update for one reader

In a session, run `/plugin`, open **Marketplaces**, select the marketplace, and select **Enable auto-update**. An
`autoUpdate` value in a settings file takes precedence over this toggle.

## When a new version loads

Claude Code updates a plugin only when its `version` changes. `bundle` writes the plugin's `version` from
`content-version` in `.corpus.yaml`, as [the plugin manifest](cli/bundle.md#the-plugin-manifest) shows. Move
`content-version` to publish a new version.

With auto-update on, Claude Code waits a random delay of up to ten minutes after the first message of a session. It
then refreshes each marketplace with auto-update on, and updates the plugins installed from it on disk. The running
session keeps the version it loaded, and shows `Plugin updated: <name> · Run /reload-plugins to apply`. Run
`/reload-plugins` to load the new version now. Otherwise it loads at the next launch.

Without auto-update, a reader takes a new version only by asking for it:

```text
/plugin marketplace update knowledge-as-code              # in a session
claude plugin update example-dogfooding@knowledge-as-code # in a shell
```

## What stops auto-update

* **An environment variable.** `DISABLE_UPDATES=1`, `DISABLE_AUTOUPDATER=1` or
  `CLAUDE_CODE_DISABLE_NONESSENTIAL_TRAFFIC=1` turns the whole pass off, unless `FORCE_AUTOUPDATE_PLUGINS=1` is also
  set.
* **A cloud session.** It never shows the workspace trust dialog, so it ignores the marketplaces a repository's
  `.claude/settings.json` lists.
* **A private marketplace with no stored credential.** The background check never prompts. Where git cannot
  authenticate without asking, the update fails and the reader keeps the version they have. Run `gh auth login` and
  then `gh auth setup-git` to store a credential for GitHub.

Claude Code's own pages on
[keeping users up to date](https://code.claude.com/docs/en/plugins/host-marketplace#keep-users-up-to-date) and
[when auto-update runs](https://code.claude.com/docs/en/plugins/loading#when-auto-update-runs) give the full rules.

## Where to go next

[Skills](skills.md) says which skills an installed plugin contains, and which record types each one reads.
