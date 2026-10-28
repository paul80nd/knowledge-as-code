#!/usr/bin/env node
// Merges one stage's result files from several batches into one result file that score.mjs reads.
//
//   merge.mjs <out.json> <aggregate-result.json>...
//
// Each case's runs are the runs of every batch that ran it. The cost is the sum, and the result is partial if any
// batch was. Every batch must have run the same stage, model, Claude Code version and plugin, and must grade each case
// it shares with the others by the same graders.
import { readFileSync, writeFileSync, writeSync } from "node:fs";

// Prints a message to stderr and exits. The write is synchronous, so the exit cannot cut the message off.
function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

if (process.argv.length < 5) stop(2, "usage: merge.mjs <out.json> <aggregate-result.json> <aggregate-result.json>...");
const [out, ...batchPaths] = process.argv.slice(2);

const batches = batchPaths.map((f) => {
  try {
    return JSON.parse(readFileSync(f, "utf8"));
  } catch (error) {
    return stop(1, `merge.mjs: cannot read ${f}: ${error.message}`);
  }
});

// A value as JSON, escaping DEL as jq does.
const json = (v, indent) => JSON.stringify(v, null, indent).replaceAll("\u007f", "\\u007f");

// jq's order of values: null, false, true, numbers, strings by code point, arrays, then objects by their sorted keys.
const rank = (v) => (v === null ? 0 : v === false ? 1 : v === true ? 2 : typeof v === "number" ? 3
  : typeof v === "string" ? 4 : Array.isArray(v) ? 5 : 6);
function compare(a, b) {
  if (rank(a) !== rank(b)) return rank(a) - rank(b);
  if (typeof a === "number") return a - b;
  if (typeof a === "string") return Buffer.compare(Buffer.from(a), Buffer.from(b));
  if (Array.isArray(a)) {
    for (let i = 0; i < Math.min(a.length, b.length); i++) {
      const c = compare(a[i], b[i]);
      if (c !== 0) return c;
    }
    return a.length - b.length;
  }
  if (rank(a) === 6) {
    const keys = [Object.keys(a).sort(compare), Object.keys(b).sort(compare)];
    const c = compare(...keys);
    if (c !== 0) return c;
    for (const k of keys[0]) {
      const d = compare(a[k], b[k]);
      if (d !== 0) return d;
    }
  }
  return 0;
}
const unique = (values) => [...values].sort(compare).filter((v, i, all) => i === 0 || compare(all[i - 1], v) !== 0);
const groupBy = (values, key) => {
  const groups = [];
  for (const v of [...values].sort((x, y) => compare(key(x), key(y)))) {
    if (groups.length > 0 && compare(key(groups.at(-1)[0]), key(v)) === 0) groups.at(-1).push(v);
    else groups.push([v]);
  }
  return groups;
};
const or = (v) => (v === undefined ? null : v);

const fields = [
  [".suite.ablation", (r) => or(r.suite?.ablation)],
  [".suite.modelOverride", (r) => or(r.suite?.modelOverride)],
  [".claudeVersion", (r) => or(r.claudeVersion)],
  [".suite.plugins | map({name, version})", (r) => r.suite.plugins.map((p) => ({ name: or(p.name),
    version: or(p.version) }))],
];
for (const [field, read] of fields) {
  const values = unique(batches.map(read));
  if (values.length !== 1) stop(1, `merge.mjs: the batches differ in ${field}: ${json(values)}`);
}

const allCases = batches.flatMap((b) => b.cases);
const graders = groupBy(allCases.map((c) => ({ name: or(c.name), graders: c.graders.map((g) => ({ name: or(g.name),
  type: or(g.type), config: or(g.config) })) })), (c) => c.name)
  .filter((group) => unique(group.map((c) => c.graders)).length > 1).map((group) => group[0].name);
if (graders.length > 0) {
  writeSync(2, `merge.mjs: these cases are graded differently in different batches: ${json(graders, 2)}\n`);
  stop(1, "merge.mjs: regrade the older batch with regrade.mjs first");
}

// The first batch, with the cost summed, partial if any batch was, and each case given the runs of every batch.
const merged = batches[0];
merged.costUsd = batches.reduce((sum, b) => (b.costUsd === null || b.costUsd === undefined ? sum
  : sum === null ? b.costUsd : sum + b.costUsd), null);
merged.partial = batches.some((b) => b.partial !== null && b.partial !== undefined && b.partial !== false);
merged.cases = groupBy(allCases, (c) => or(c.name)).map(([kase]) => ({
  ...kase,
  arms: Object.fromEntries(Object.keys(kase.arms).map((arm) => [arm, allCases
    .filter((c) => compare(or(c.name), or(kase.name)) === 0).flatMap((c) => c.arms?.[arm] ?? [])])),
}));
writeFileSync(out, json(merged, 2) + "\n");

writeSync(1, `merged ${batches.length} batch(es) into ${out}\n`);
