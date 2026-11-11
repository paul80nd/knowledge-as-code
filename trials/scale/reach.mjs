#!/usr/bin/env node
// Splits the scale trial's question half by reach, and states what each answer cost.
//
//   reach.mjs <plugin aggregate-result.json> [<prose aggregate-result.json>]
//
// The plugin stage's `with` arm is the plugin arm and its `without` arm the none arm. The prose stage's one arm is the
// prose arm. A case's reach is the third tag of its prompt.md under cases/house/ here. Input tokens come from the
// `usage` on each session's trace, so they count what the session read, never an estimate.
import { existsSync, readFileSync, writeSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

const files = process.argv.slice(2);
if (files.length < 1 || files.length > 2) {
  stop(2, "usage: reach.mjs <plugin aggregate-result.json> [<prose aggregate-result.json>]");
}

const cases = join(dirname(fileURLToPath(import.meta.url)), "cases", "house");

// The tags of a case, from the frontmatter of its prompt.md.
function tags(name) {
  const text = readFileSync(join(cases, name, "prompt.md"), "utf8");
  return (/^tags:\s*\[(.*)\]\s*$/m.exec(text)?.[1] ?? "").split(",").map((s) => s.trim());
}

// What a session read: every input token, cached or not, summed over its turns.
function inputTokens(trace) {
  if (!trace || !existsSync(trace)) return null;
  const lines = readFileSync(trace, "utf8").trim().split("\n");
  for (let i = lines.length - 1; i >= 0; i--) {
    let line;
    try {
      line = JSON.parse(lines[i]);
    } catch {
      continue;
    }
    if (line.type !== "result" || !line.usage) continue;
    const u = line.usage;
    return (u.input_tokens ?? 0) + (u.cache_creation_input_tokens ?? 0) + (u.cache_read_input_tokens ?? 0);
  }
  return null;
}

// A set of graders passes where one of its alternatives does, and alternatives share the part of the name before `--`.
function passes(graders, prefix) {
  const sets = new Map();
  for (const g of graders.filter((g) => g.name.startsWith(prefix))) {
    const set = g.name.split("--")[0];
    sets.set(set, (sets.get(set) ?? false) || g.passed);
  }
  return sets.size > 0 && [...sets.values()].every(Boolean);
}

const sessions = [];
files.forEach((file, i) => {
  const result = JSON.parse(readFileSync(file, "utf8"));
  for (const c of result.cases) {
    const reach = tags(c.name)[2] ?? "untagged";
    for (const [arm, runs] of Object.entries(c.arms)) {
      const name = i === 1 ? "prose" : arm === "with" ? "plugin" : "none";
      for (const run of runs) {
        sessions.push({
          arm: name, reach, case: c.name, cost: run.costUsd ?? 0, tokens: inputTokens(run.tracePath),
          answered: passes(run.graders, "task-"), cited: passes(run.graders, "cite-"),
        });
      }
    }
  }
});

const median = (xs) => {
  const s = xs.filter((x) => x !== null).sort((a, b) => a - b);
  if (s.length === 0) return null;
  return s.length % 2 ? s[(s.length - 1) / 2] : (s[s.length / 2 - 1] + s[s.length / 2]) / 2;
};
const pct = (n, d) => (d === 0 ? "-" : `${n}/${d} (${Math.round((100 * n) / d)}%)`);

console.log("| Arm | Reach | Sessions | Answered | Cited | Median input tokens | Cost | Cost per answer |");
console.log("|-----|-------|---------:|---------:|------:|--------------------:|-----:|----------------:|");
for (const arm of ["plugin", "prose", "none"]) {
  for (const reach of ["one-record", "across-records", "all"]) {
    const s = sessions.filter((x) => x.arm === arm && (reach === "all" || x.reach === reach));
    if (s.length === 0) continue;
    const answered = s.filter((x) => x.answered).length;
    const cost = s.reduce((sum, x) => sum + x.cost, 0);
    const tokens = median(s.map((x) => x.tokens));
    console.log(`| ${arm} | ${reach} | ${s.length} | ${pct(answered, s.length)} | `
      + `${pct(s.filter((x) => x.cited).length, s.length)} | ${tokens === null ? "-" : Math.round(tokens)} | `
      + `$${cost.toFixed(2)} | ${answered === 0 ? "-" : `$${(cost / answered).toFixed(2)}`} |`);
  }
}
