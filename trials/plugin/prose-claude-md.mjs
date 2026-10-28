#!/usr/bin/env node
// Writes the prose arm's CLAUDE.md from a corpus export: every record of every type the export contains.
//
//   prose-claude-md.mjs <export dir> <output file>
//
// <export dir> is what `kac export` wrote, or the corpus/ folder of the plugin `kac bundle` built. The types, their
// folders and their parts files come from its manifest.json, so nothing here knows a type by name.
import { existsSync, readdirSync, readFileSync, statSync, writeFileSync, writeSync } from "node:fs";

// Prints a message to stderr and exits. The write is synchronous, so the exit cannot cut the message off.
function stop(status, message) {
  writeSync(2, `${message}\n`);
  process.exit(status);
}

if (process.argv.length !== 4) stop(2, "usage: prose-claude-md.mjs <export dir> <output file>");
const [exportDir, out] = process.argv.slice(2);
const manifestPath = `${exportDir}/manifest.json`;

if (!existsSync(manifestPath) || !statSync(manifestPath).isFile()) {
  stop(1, `prose-claude-md.mjs: ${exportDir} has no manifest.json`);
}

// The keys every line or record has for the tool's own use, which a reader of the prose does not need.
const bookkeeping = new Set(["type", "path", "anchor", "links", "trust", "status", "reviewBy", "review-by", "record",
  "part", "id", "title", "shortcode"]);

// A value as JSON on one line, with DEL written as \u007f.
const json = (v) => (v === undefined ? "null" : JSON.stringify(v).replaceAll("\u007f", "\\u007f"));
// A value as text: a string as it is, anything else as JSON.
const text = (v) => (typeof v === "string" ? v : json(v));
// Whether a value counts as present. Only null and false do not.
const present = (v) => v !== null && v !== undefined && v !== false;
const isObject = (v) => typeof v === "object" && v !== null && !Array.isArray(v);

// A field as one line of prose: a list joined with commas, an object as JSON, a null left out.
function show(v) {
  if (Array.isArray(v) && v.length > 0 && v.every((e) => isObject(e) && "text" in e)) {
    return "\n" + v.map((e) => `- ${text(e.text)}`).join("\n");
  }
  if (Array.isArray(v)) return " " + v.map(text).join(", ");
  return " " + text(v);
}
const fields = (o) => Object.entries(o).filter(([k, v]) => v !== null && !bookkeeping.has(k))
  .map(([k, v]) => `${k}:${show(v)}`);

// The JSON values in a file: one document, or one per line.
function values(path) {
  const body = readFileSync(path, "utf8");
  try {
    return [JSON.parse(body)];
  } catch {
    return body.split("\n").filter((line) => line.trim() !== "").map((line) => JSON.parse(line));
  }
}

// Every .json file below a folder, as paths from it written with forward slashes, in byte order.
function jsonFiles(dir, rel = "") {
  const found = [];
  for (const entry of readdirSync(rel === "" ? dir : `${dir}/${rel}`, { withFileTypes: true })) {
    const path = rel === "" ? entry.name : `${rel}/${entry.name}`;
    if (entry.isDirectory()) found.push(...jsonFiles(dir, path));
    else if (entry.name.endsWith(".json")) found.push(path);
  }
  return rel === "" ? found.sort() : found;
}

const manifest = JSON.parse(readFileSync(manifestPath, "utf8"));
const name = text(present(manifest.about?.displayName) ? manifest.about.displayName : manifest.corpus);

// Every line of the text, and the number of parts written, so a part no record claimed fails the run.
const lines = [`# The knowledge of ${name}`, "",
  "This is what the corpus records, type by type. Each heading gives the id to cite it by."];
let written = 0;

for (const t of manifest.types) {
  const dir = `${exportDir}/${text(t.dir)}`;
  const partsFile = present(t.partsFile) ? text(t.partsFile) : "";
  const recordKey = present(t.recordKey) ? text(t.recordKey) : "";
  const frameworksFile = present(t.frameworksFile) ? text(t.frameworksFile) : "";

  lines.push("", `## ${text(t.type)}`);

  for (const file of jsonFiles(dir)) {
    const record = JSON.parse(readFileSync(`${dir}/${file}`, "utf8"));
    const id = text(record.fields.id);
    const title = present(record.fields.title) ? record.fields.title : record.fields.id;
    lines.push("", `### ${text(title)} (\`${text(record.fields.id)}\`)`, "");
    lines.push(`Status: ${text(present(record.fields.status) ? record.fields.status : "none")}.`);
    lines.push(...fields(record.fields));
    if (present(record.links?.human)) lines.push(`Source: <${text(record.links.human)}>`);
    for (const [heading, body] of Object.entries(present(record.sections) ? record.sections : {})) {
      lines.push("", `#### ${heading}`, "", text(body));
    }

    // A record a layer brought in sits in a folder named for it, and its parts name it as <layer>:<id>.
    const owner = file.includes("/") ? `${file.slice(0, file.indexOf("/"))}:${id}` : id;

    if (partsFile !== "" && recordKey !== "") {
      for (const part of values(`${exportDir}/${partsFile}`).filter((p) => p[recordKey] === owner)) {
        written++;
        const heading = present(part.title) ? `${text(part.title)} (\`${text(part.id)}\`)` : `\`${text(part.id)}\``;
        lines.push("", `#### ${heading}`, "", ...fields(part));
      }
    }
  }

  if (frameworksFile !== "") {
    lines.push("", "### Frameworks this type cites", "");
    for (const framework of values(`${exportDir}/${frameworksFile}`)) lines.push(`- ${fields(framework).join("; ")}`);
  }
}

// Takes out every carriage return, so the prose arm's text is the same on every machine.
writeFileSync(out, (lines.join("\n") + "\n").replaceAll("\r", ""));

const expected = manifest.types.reduce((sum, t) => sum + (t.parts ?? 0), 0);
if (written !== expected) {
  stop(1, `prose-claude-md.mjs: the manifest counts ${expected} parts, and ${written} were written`);
}
