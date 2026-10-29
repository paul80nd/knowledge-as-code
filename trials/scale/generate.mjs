#!/usr/bin/env node
// Writes a corpus of <apps> applications from the records under examples/, for the scale trial.
//
//   generate.mjs <apps> <dir>
//
// The corpus is <dir>/corpus-<apps>. Put <dir> in this clone, untracked and not ignored. kac lists a corpus's files
// through git and leaves out every file git ignores, and outside git its export has no source links. Where <dir> is
// outside the clone, a copy of .schema/ is written to <dir>/.schema, where kac's walk-up finds it.
//
// Each application gets a copy of every record of an application type from examples/payments, examples/library and
// examples/dog-fooding, with its ids, file names, repositories and owner rewritten. The fixed records are written
// once. The corpus consumes examples/engineering, as payments does, so policies arrive through `kac restore`.
// A citation of a record the corpus leaves out becomes a code span, so every link still resolves.
import { cpSync, existsSync, mkdirSync, readdirSync, readFileSync, statSync, writeFileSync, writeSync } from "node:fs";
import { basename, dirname, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

if (process.argv.length !== 4) stop(2, "usage: generate.mjs <apps> <dir>");
const apps = Number(process.argv[2]);
if (!Number.isInteger(apps) || apps < 1 || apps > 8) stop(2, "generate.mjs: <apps> is a whole number from 1 to 8");
const out = resolve(process.argv[3], `corpus-${apps}`);
if (existsSync(out) && readdirSync(out).length > 0) stop(2, `generate.mjs: ${out} is not empty`);

const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
if (!out.startsWith(root + sep)) {
  cpSync(join(root, ".schema"), resolve(process.argv[3], ".schema"), { recursive: true });
}
const examples = join(root, "examples");
const base = "https://git.example.com/example-estate";

// Each application's name, which prefixes its slugs, and the person who owns its records.
const APPS = [
  { name: "alder", owner: "human:ada.lind" },
  { name: "birch", owner: "human:bo.kerr" },
  { name: "cedar", owner: "human:cy.mora" },
  { name: "damson", owner: "human:dee.hart" },
  { name: "elder", owner: "human:eli.ford" },
  { name: "fir", owner: "human:fen.roe" },
  { name: "gorse", owner: "human:gil.yeo" },
  { name: "hazel", owner: "human:hal.marsh" },
].slice(0, apps);

// The corpora records come from, with the address their repository names resolve against.
const SOURCES = {
  payments: "https://git.example.com/example-payments",
  library: "https://git.example.com/example-libraries",
  "dog-fooding": "https://github.com/paul80nd",
};

// The types every application has a copy of, with the corpora holding records of each.
const PER_APP = {
  services: ["payments", "library", "dog-fooding"],
  integrations: ["library"],
  data: ["library"],
  nfrs: ["payments"],
  offerings: ["payments"],
  runbooks: ["dog-fooding"],
  fixes: ["payments", "dog-fooding"],
  postmortems: ["payments"],
  deviations: ["dog-fooding"],
};

// The types written once, with the corpora holding records of each.
const FIXED = {
  standards: ["payments", "dog-fooding"],
  adrs: ["library"],
  glossary: ["library"],
  explanations: ["library"],
};

// The corpus each type page and template comes from.
const PAGE_FROM = {
  services: "library", integrations: "library", data: "library", nfrs: "payments", offerings: "payments",
  runbooks: "dog-fooding", fixes: "payments", postmortems: "payments", deviations: "dog-fooding",
  standards: "payments", adrs: "library", glossary: "library", explanations: "library",
};

const NUMBERED = new Set(["nfrs", "postmortems", "fixes"]);
const PREFIX = {
  services: "svc", integrations: "itg", data: "dat", nfrs: "nfr", offerings: "ofr", runbooks: "rbk", fixes: "fix",
  postmortems: "pmt", deviations: "dev",
};
const SLUG_MAX = 30;

// Every .md file below a folder but the template and the generated index, in a stable order.
function records(dir) {
  const found = [];
  for (const e of readdirSync(dir, { withFileTypes: true }).sort((a, b) => (a.name < b.name ? -1 : 1))) {
    const p = join(dir, e.name);
    if (e.isDirectory()) found.push(...records(p));
    else if (e.name.endsWith(".md") && e.name !== "_template.md" && e.name !== "_index.md") found.push(p);
  }
  return found;
}

const idOf = (text) => /^id:\s*(\S+)\s*$/m.exec(text)?.[1];
const reposOf = (text) => (/^repos:\s*\[([^\]]*)\]/m.exec(text)?.[1] ?? "").split(",").map((s) => s.trim())
  .filter((s) => s !== "");

// Shortens a slug at a hyphen until it fits the type's limit.
function fit(slug) {
  while (slug.length > SLUG_MAX) slug = slug.slice(0, slug.lastIndexOf("-"));
  return slug;
}

// Every record the corpus holds: its source, the file it is written to, and its old and new id.
const plan = [];
// For each source and application (0 for a fixed record), the old id to the new one, and the old path to the new.
const idMap = new Map();
const pathMap = new Map();
const key = (src, app) => `${src}#${app}`;
const mapFor = (m, src, app) => {
  if (!m.has(key(src, app))) m.set(key(src, app), new Map());
  return m.get(key(src, app));
};
// The repositories each source's services name.
const repos = Object.fromEntries(Object.keys(SOURCES).map((s) => [s, new Set()]));

for (const [type, srcs] of Object.entries(FIXED)) {
  for (const src of srcs) {
    const dir = join(examples, src, type);
    for (const file of records(dir)) {
      const id = idOf(readFileSync(file, "utf8"));
      const dest = join(out, type, relative(dir, file));
      plan.push({ src, app: 0, type, file, dest, id });
      for (let a = 0; a <= apps; a++) {
        mapFor(idMap, src, a).set(id, id);
        mapFor(pathMap, src, a).set(file, dest);
      }
    }
  }
}

APPS.forEach((app, i) => {
  for (const [type, srcs] of Object.entries(PER_APP)) {
    let n = 0;
    const count = srcs.reduce((sum, s) => sum + records(join(examples, s, type)).length, 0);
    for (const src of srcs) {
      const dir = join(examples, src, type);
      for (const file of records(dir)) {
        const text = readFileSync(file, "utf8");
        const id = idOf(text);
        if (type === "services") for (const r of reposOf(text)) repos[src].add(r);
        n++;
        const name = basename(file, ".md");
        let newId;
        let newName;
        if (NUMBERED.has(type)) {
          const num = String(i * count + n).padStart(4, "0");
          newId = `${PREFIX[type]}-${num}`;
          const numbered = /^\d{4}-/.test(name);
          const slug = fit(`${app.name}-${numbered ? name.slice(5) : name}`);
          newName = numbered ? `${num}-${slug}` : slug;
        } else {
          newName = fit(`${app.name}-${name}`);
          newId = `${PREFIX[type]}-${newName}`;
        }
        const dest = join(out, type, `${newName}.md`);
        plan.push({ src, app: i + 1, type, file, dest, id, newId });
        mapFor(idMap, src, i + 1).set(id, newId);
        mapFor(pathMap, src, i + 1).set(file, dest);
      }
    }
  }
});

// The ids of the records each source holds and this corpus leaves out.
const left = Object.fromEntries(Object.keys(SOURCES).map((src) => {
  const held = new Set(plan.filter((r) => r.src === src).map((r) => r.id));
  const ids = new Set();
  for (const e of readdirSync(join(examples, src), { withFileTypes: true })) {
    if (!e.isDirectory() || e.name.startsWith(".") || e.name === "knowledge-as-code") continue;
    for (const f of records(join(examples, src, e.name))) {
      const id = idOf(readFileSync(f, "utf8"));
      if (id !== undefined && !held.has(id)) ids.add(id);
    }
  }
  return [src, ids];
}));

// A fixed record cites the first application's copy of a record.
for (const src of Object.keys(SOURCES)) {
  for (const [k, v] of mapFor(idMap, src, 1)) if (!mapFor(idMap, src, 0).has(k)) mapFor(idMap, src, 0).set(k, v);
  for (const [k, v] of mapFor(pathMap, src, 1)) if (!mapFor(pathMap, src, 0).has(k)) mapFor(pathMap, src, 0).set(k, v);
}

// The pages outside the type folders, which every source maps to the same file here.
const TYPES = [...Object.keys(PER_APP), ...Object.keys(FIXED)].sort();
for (const src of Object.keys(SOURCES)) {
  for (let a = 0; a <= apps; a++) {
    const m = mapFor(pathMap, src, a);
    for (const t of TYPES) {
      m.set(join(examples, src, `${t}.md`), join(out, `${t}.md`));
      m.set(join(examples, src, t, "_index.md"), join(out, t, "_index.md"));
    }
    for (const f of ["contributing", "lineage", "metadata", "taxonomy"]) {
      m.set(join(examples, src, "knowledge-as-code", `${f}.md`), join(out, "knowledge-as-code", `${f}.md`));
    }
    m.set(join(examples, src, "knowledge-as-code.md"), join(out, "knowledge-as-code.md"));
    m.set(join(examples, src, "frameworks.md"), join(out, "frameworks.md"));
  }
}

const escape = (s) => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
// A path with forward slashes, as a Markdown link and .corpus.yaml write it on every platform.
const posix = (p) => p.split(sep).join("/");

// Rewrites every id the map knows, where it stands alone. An id after `:` belongs to another corpus.
function rewriteIds(text, map) {
  const ids = [...map.keys()].filter((k) => map.get(k) !== k).sort((a, b) => b.length - a.length);
  if (ids.length === 0) return text;
  const re = new RegExp(`(?<![A-Za-z0-9:_/-])(${ids.map(escape).join("|")})(?![A-Za-z0-9_-])`, "g");
  return text.replace(re, (m) => map.get(m));
}

// A link target here, or null where the record it leads to is left out.
function retarget(target, src, from, dest, paths) {
  if (/^([a-z]+:|#|\/)/i.test(target)) return target;
  const hash = target.indexOf("#");
  const path = hash < 0 ? target : target.slice(0, hash);
  const fragment = hash < 0 ? "" : target.slice(hash);
  const abs = resolve(dirname(from), decodeURI(path));
  const corpus = join(examples, src);
  let to;
  if (paths.has(abs)) to = paths.get(abs);
  else if (abs.startsWith(corpus + sep) || abs === corpus) return null;
  else to = abs;
  let rel = posix(relative(dirname(dest), to));
  if (rel === "") rel = basename(to);
  return rel + fragment;
}

// Rewrites the links of a Markdown body outside its code fences, and unlinks those whose record is left out.
function relink(body, src, from, dest, paths) {
  const parts = body.split(/(^```[\s\S]*?^```[^\n]*$)/m);
  const dropped = new Set();
  for (let i = 0; i < parts.length; i += 2) {
    parts[i] = parts[i].replace(/^\[([^\]\n]+)\]:[ \t]+(\S+)(.*)$/gm, (m, label, target, rest) => {
      const t = retarget(target, src, from, dest, paths);
      if (t === null) {
        dropped.add(label.toLowerCase());
        return "\u0000";
      }
      return `[${label}]: ${t}${rest}`;
    }).replace(/\u0000\n?/g, "");
    parts[i] = parts[i].replace(/\[((?:[^\[\]]|\[[^\]]*\])*)\]\(([^)\s]+)\)/g, (m, text, target) => {
      const t = retarget(target, src, from, dest, paths);
      return t === null ? text : `[${text}](${t})`;
    });
  }
  if (dropped.size === 0) return parts.join("");
  const plain = (label) => (/^([a-z]+:)?[a-z]{3}-[\w.-]+$/.test(label) ? `\`${label}\`` : label);
  for (let i = 0; i < parts.length; i += 2) {
    parts[i] = parts[i]
      .replace(/\[([^\[\]\n]+)\]\[([^\[\]\n]+)\]/g, (m, text, label) => (dropped.has(label.toLowerCase()) ? text : m))
      .replace(/\[([^\[\]\n]+)\]\[\]/g, (m, label) => (dropped.has(label.toLowerCase()) ? plain(label) : m))
      .replace(/\[([^\[\]\n]+)\](?![(\[:])/g, (m, label) => (dropped.has(label.toLowerCase()) ? plain(label) : m));
  }
  return parts.join("");
}

// Takes each id of a record left out from the frontmatter's lists, and a key whose list is left empty.
function strip(front, gone) {
  const lines = front.split("\n");
  const kept = [];
  for (let i = 0; i < lines.length; i++) {
    const inline = /^([a-z-]+):\s*\[(.*)\]\s*$/.exec(lines[i]);
    if (inline) {
      const items = inline[2].split(",").map((s) => s.trim()).filter((s) => s !== "");
      const rest = items.filter((s) => !gone.has(s));
      if (rest.length === items.length) kept.push(lines[i]);
      else if (rest.length > 0) kept.push(`${inline[1]}: [ ${rest.join(", ")} ]`);
      continue;
    }
    if (/^[a-z-]+:\s*$/.test(lines[i]) && /^\s+- /.test(lines[i + 1] ?? "")) {
      const items = [];
      let j = i + 1;
      while (/^\s+- /.test(lines[j] ?? "")) items.push(lines[j++]);
      const rest = items.filter((l) => !gone.has(l.replace(/^\s+- /, "").trim()));
      if (rest.length > 0) kept.push(lines[i], ...rest);
      i = j - 1;
      continue;
    }
    kept.push(lines[i]);
  }
  return kept.join("\n");
}

// The whole rewrite of one file: ids, links, and for an application's copy its repositories, owner and title.
function rewrite(text, src, app, from, dest) {
  let t = rewriteIds(text, mapFor(idMap, src, app));
  const fm = /^---\n[\s\S]*?\n---\n/.exec(t);
  const head = fm ? fm[0] : "";
  let body = relink(t.slice(head.length), src, from, dest, mapFor(pathMap, src, app));
  let front = strip(head, left[src]);
  if (app > 0 && from.startsWith(join(examples, src) + sep)) {
    const a = APPS[app - 1];
    const rel = relative(join(examples, src), from);
    if (Object.keys(PER_APP).includes(rel.split(sep)[0])) {
      for (const r of repos[src]) {
        const url = new RegExp(`${escape(SOURCES[src])}/${escape(r)}(?![A-Za-z0-9_.-])`, "g");
        body = body.replace(url, `${base}/${a.name}-${r}`).replace(
          new RegExp("\\[`" + escape(r) + "`\\]\\(" + escape(`${base}/${a.name}-${r}`), "g"),
          "[`" + `${a.name}-${r}` + "`](" + `${base}/${a.name}-${r}`);
        front = front.replace(url, `${base}/${a.name}-${r}`)
          .replace(new RegExp(`^(\\s+- )${escape(r)}/`, "gm"), `$1${a.name}-${r}/`);
      }
      front = front.replace(/^repos:\s*\[([^\]]*)\]/m, (m, list) =>
        `repos: [ ${list.split(",").map((s) => s.trim()).filter((s) => s).map((s) => `${a.name}-${s}`).join(", ")} ]`);
      front = front.replace(/^owner:\s*\S+$/m, `owner: ${a.owner}`);
      const label = a.name[0].toUpperCase() + a.name.slice(1);
      body = body.replace(/^# (.+)$/m, `# $1 (${label})`);
    }
  }
  return front + body;
}

function write(dest, text) {
  mkdirSync(dirname(dest), { recursive: true });
  writeFileSync(dest, text);
}

for (const r of plan) write(r.dest, rewrite(readFileSync(r.file, "utf8"), r.src, r.app, r.file, r.dest));

// The type pages and templates, read as fixed records are.
for (const t of TYPES) {
  const src = PAGE_FROM[t];
  const page = join(examples, src, `${t}.md`);
  write(join(out, `${t}.md`), rewrite(readFileSync(page, "utf8"), src, 0, page, join(out, `${t}.md`)));
  cpSync(join(examples, src, t, "_template.md"), join(out, t, "_template.md"));
}

// The framework pages, the framework register, and the files every corpus has, from payments.
const pay = join(examples, "payments");
for (const f of ["knowledge-as-code/contributing.md", "knowledge-as-code/lineage.md", "knowledge-as-code/metadata.md",
  "knowledge-as-code/taxonomy.md", "knowledge-as-code.md", "frameworks.md"]) {
  write(join(out, f), rewrite(readFileSync(join(pay, f), "utf8"), "payments", 0, join(pay, f), join(out, f)));
}
for (const f of ["knowledge-as-code/.order", ".gitattributes", ".gitignore"]) cpSync(join(pay, f), join(out, f));
cpSync(join(pay, ".plugin"), join(out, ".plugin"), { recursive: true });
write(join(out, ".order"), ["README", "knowledge-as-code", ...TYPES, "frameworks", ""].join("\n"));

const readme = readFileSync(join(pay, "README.md"), "utf8");
const block = /<!-- BEGIN GENERATED: types-index -->[\s\S]*?<!-- END GENERATED: types-index -->/.exec(readme)[0];
write(join(out, "README.md"), `# Example Estate

> **Everything here is invented and generated.** \`trials/scale/generate.mjs\` wrote this corpus from the records under
> \`examples/\`, for ${apps} application(s): ${APPS.map((a) => a.name).join(", ")}.

## The knowledge types

${block}
`);

const up = posix(relative(out, root)) || ".";
const descriptor = readFileSync(join(pay, ".corpus.yaml"), "utf8")
  .replace(/^corpus: .*$/m, "corpus: example-estate")
  .replace(/^shortcode: .*$/m, "shortcode: est")
  .replace(/^content-version: .*$/m, 'content-version: "0.1.0"')
  .replace(/^display-name: .*$/m, "display-name: Example Estate")
  .replace(/^description: >\n(?:  .*\n)+/m,
    `description: >\n  A generated corpus of ${apps} application(s) for the scale trial. It binds nobody.\n`)
  .replace(/^  path-prefix: .*$/m, "  path-prefix: examples/estate")
  .replace(/^repos-base: .*$/m, `repos-base: ${base}`)
  .replace(/^    source: .*$/m, `    source: ${up}/examples/engineering/.dist/package`)
  .replace(/^  url: \.\.\/\.\.\/$/m, `  url: ${up}/`)
  .replace(/^types:\n(?:  - .*\n)+/m, `types:\n${TYPES.map((t) => `  - ${t}`).join("\n")}\n`)
  .replace(/^enums:\n(?:  .*\n)+/m, "enums:\n  component-type: [api, asset, cli, website, worker]\n" +
    "  interface-type: [asyncapi, openapi]\n  platform: [azure-function, dotnet, mixed, static, typescript]\n")
  .replace(/^  from: .*$/m, `  from: ${up}/template/.plugin`);
write(join(out, ".corpus.yaml"), descriptor);

const perApp = plan.filter((r) => r.app === 1).length;
console.log(`generate: wrote ${plan.length} record(s) to ${out}: ${plan.length - perApp * apps} fixed, ` +
  `${perApp} for each of ${apps} application(s).`);
