#!/bin/sh
# Writes a cut of kac whose last commit regenerated one golden without GITHUB_ACTIONS, so it fails only in CI.
set -eu

mkdir -p .github/workflows tooling/kac tooling/tests/fixtures/validate-clean/corpus \
  tooling/tests/fixtures/validate-broken-link/corpus

cat > README.md <<'EOF'
# knowledge-as-code

The `kac` command line tool, its documentation site, and the corpus that describes them.

Run the golden suite from the root of the repository:

```sh
dotnet run tooling/kac-tests.cs
```
EOF

cat > .github/workflows/kac.yml <<'EOF'
name: kac

on:
  pull_request:
    branches: [main]

permissions:
  contents: read

jobs:
  goldens:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x
      - run: dotnet run tooling/kac-tests.cs
EOF

cat > tooling/kac-tests.cs <<'EOF'
// The golden suite: runs kac over each fixture corpus and compares what it prints with the expected.txt beside it.
using System.Diagnostics;

var update = args.Contains("--update");
var filter = args.FirstOrDefault(a => !a.StartsWith("--"));
var fixtures = Path.Combine("tooling", "tests", "fixtures");
var ran = 0;
var failed = 0;

foreach (var scenario in Directory.GetDirectories(fixtures).OrderBy(d => d, StringComparer.Ordinal))
{
    var name = Path.GetFileName(scenario);
    if (filter is not null && !name.Contains(filter)) continue;
    ran++;

    var actual = Kac("validate", Path.Combine(scenario, "corpus"));
    var expectedPath = Path.Combine(scenario, "expected.txt");
    if (update)
    {
        File.WriteAllText(expectedPath, actual);
        Console.WriteLine($"UPDATE {name}");
        continue;
    }
    if (!File.Exists(expectedPath))
    {
        failed++;
        Console.WriteLine($"MISSING {name}: no golden.");
        continue;
    }

    var expected = File.ReadAllText(expectedPath).Split('\n');
    var got = actual.Split('\n');
    var at = Enumerable.Range(0, Math.Max(expected.Length, got.Length))
        .FirstOrDefault(i => i >= expected.Length || i >= got.Length || expected[i] != got[i], -1);
    if (at < 0)
    {
        Console.WriteLine($"ok {name}");
        continue;
    }
    failed++;
    Console.WriteLine($"FAIL {name}: the output differs from expected.txt at line {at + 1}");
    Console.WriteLine($"    expected: {(at < expected.Length ? expected[at] : "<end of file>")}");
    Console.WriteLine($"    actual:   {(at < got.Length ? got[at] : "<end of output>")}");
}

if (update) return 0;
Console.WriteLine(failed == 0 ? $"all {ran} scenario(s) passed." : $"{failed} of {ran} scenario(s) failed.");
return failed == 0 ? 0 : 1;

static string Kac(params string[] arguments)
{
    var start = new ProcessStartInfo("dotnet", ["run", "tooling/kac/kac.cs", "--", .. arguments])
    {
        RedirectStandardOutput = true,
    };
    using var process = Process.Start(start)!;
    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return output.Replace("\r\n", "\n");
}
EOF

cat > tooling/kac/kac.cs <<'EOF'
// kac validate, cut down to the one check the golden suite pins: every relative link finds a file.
using System.Text.RegularExpressions;

if (args.Length != 2 || args[0] != "validate")
{
    Console.Error.WriteLine("usage: kac validate <corpus>");
    return 2;
}

var corpus = args[1];
// Under GitHub Actions each finding is also written as a workflow annotation, so it shows on the pull request.
var annotate = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
var link = new Regex(@"\]\(([^)#:]+)\)");
var documents = Directory.GetFiles(corpus, "*.md").OrderBy(f => f, StringComparer.Ordinal).ToArray();
var errors = 0;

foreach (var path in documents)
{
    var name = Path.GetFileName(path);
    var lines = File.ReadAllLines(path);
    for (var i = 0; i < lines.Length; i++)
    {
        foreach (Match match in link.Matches(lines[i]))
        {
            var target = match.Groups[1].Value;
            if (File.Exists(Path.Combine(corpus, target))) continue;
            errors++;
            var message = $"the link to {target} points at no file.";
            Console.WriteLine($"{name}:{i + 1}: broken-link: {message}");
            if (annotate) Console.WriteLine($"::error file={name},line={i + 1}::broken-link: {message}");
        }
    }
}

Console.WriteLine($"validated {documents.Length} document(s). {errors} error(s).");
return errors == 0 ? 0 : 1;
EOF

cat > tooling/tests/fixtures/validate-clean/corpus/hours.md <<'EOF'
# Opening hours

The [lending desk](desk.md) keeps the same hours as the branch.
EOF

cat > tooling/tests/fixtures/validate-clean/corpus/desk.md <<'EOF'
# The lending desk

Borrowers collect holds here.
EOF

cat > tooling/tests/fixtures/validate-clean/expected.txt <<'EOF'
validated 2 document(s). 0 error(s).
EOF

cat > tooling/tests/fixtures/validate-broken-link/corpus/hours.md <<'EOF'
# Opening hours

The [lending desk](desk.md) keeps the same hours as the branch.
EOF

cat > tooling/tests/fixtures/validate-broken-link/expected.txt <<'EOF'
hours.md:3: broken-link: the link to desk.md points at no file.
::error file=hours.md,line=3::broken-link: the link to desk.md points at no file.
validated 1 document(s). 1 error(s).
EOF

git init -q -b main
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Add the broken-link check and its goldens"

git checkout -q -b reword-broken-link

sed 's/points at no file\./finds no file. Fix the path, or remove the link./' tooling/kac/kac.cs > tooling/kac/kac.cs.new
mv tooling/kac/kac.cs.new tooling/kac/kac.cs

cat > tooling/tests/fixtures/validate-broken-link/expected.txt <<'EOF'
hours.md:3: broken-link: the link to desk.md finds no file. Fix the path, or remove the link.
validated 1 document(s). 1 error(s).
EOF

git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Say how to fix a broken link"
