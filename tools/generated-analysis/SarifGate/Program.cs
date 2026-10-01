// The CI job's own last step -- a count ratchet, not a fixed enforced/report-only split
// (docs/development.md). Reads one or more SARIF errorlogs (one per package, named
// "<Package>.sarif" by the caller) against the checked-in baseline, tools/generated-analysis/
// counts.txt: one line per (package, rule) holding the ceiling "check" holds that pair to.
// Every rule this job tracks is ratcheted the same way; see trackedRules below for why these
// six and not the compiler's own CS-prefixed diagnostics, which are not generated-code-specific
// at all and need no separate harness.
//
// A finding only counts toward a package's rule total when it is (a) really in the emitted
// text prepare-analysis.sh copied into the analysis project's own "gen/" directory -- not the
// grammar author's own file (a `#line`-mapped region reports its SARIF location there
// directly, under the real repository path, never under "gen/") and not the package's
// hand-written sources compiled alongside it for symbol resolution -- and (b) not already
// suppressed in source (a `#pragma warning disable`, which the compiler itself already keeps
// from failing anyone's build). `-warnaserror` cannot tell those apart on its own, which is
// why this reads the SARIF instead of trusting an exit code.
//
// Before any of that: every SARIF is checked for a "level":"error" result first (a compile
// error in the throwaway Analysis.csproj, not a style finding -- TreatWarningsAsErrors is
// false there, so only a genuine error, such as a missing reference, produces one). A broken
// analysis project silently compiles less code and reports FEWER findings, which would read as
// an improvement to the ratchet below rather than the broken run it actually is, so this fails
// loudly, first, before any count is trusted.
//
// Usage:
//   SarifGate check  <counts-file> <sarif-path> [sarif-path ...]
//     The CI job's own step. Fails if any (package, rule) total differs from the checked-in
//     file, in EITHER direction: an increase is the regression the ratchet exists to catch,
//     and a decrease left unrecorded would only let the file go stale, weakening the ratchet
//     the next time something regresses toward the old, looser number. Prints the full
//     measured table either way.
//   SarifGate update <counts-file> <sarif-path> [sarif-path ...] [--allow-increase]
//     Rewrites counts.txt from a fresh measurement, for a developer to review and commit.
//     Refuses (exit 1, no write) if any pair would go UP from what the file already holds,
//     unless --allow-increase is also given -- the one place this tool will knowingly move a
//     number the wrong way, and it says so loudly when it does.

using System;
using System.Text.Json;
using System.Text.Json.Nodes;

if (args.Length < 3)
{
	Console.Error.WriteLine("usage: SarifGate <check|update> <counts-file> <sarif-path> [sarif-path ...] [--allow-increase]");
	return 2;
}

var mode          = args[0];
var countsFile    = args[1];
var allowIncrease = args.Contains("--allow-increase");
var sarifPaths    = args.Skip(2).Where(a => a != "--allow-increase").ToArray();

if (mode != "check" && mode != "update")
{
	Console.Error.WriteLine($"SarifGate: unknown mode '{mode}' -- expected 'check' or 'update'");
	return 2;
}

// IDE-prefixed: the stock style analyzers that skip generated code by design
// (GeneratedCodeAnalysisFlags.None), which is the whole reason this harness exists. The
// compiler's own CS0168/CS0219/CS0162 are NOT generated-code-specific -- they fire on
// generated code exactly as they do anywhere else, so an ordinary -warnaserror build already
// enforces them with no separate harness, and they are not tracked here.
var trackedRules = new[] { "IDE0035", "IDE0059", "IDE0060", "IDE0051", "IDE0052", "IDE0004" };

var measured   = new Dictionary<(string Package, string Rule), int>();
var excluded   = 0;
var suppressed = 0;

foreach (var sarifPath in sarifPaths)
{
	if (!File.Exists(sarifPath))
	{
		Console.Error.WriteLine($"SarifGate: no SARIF at {sarifPath}");
		return 1;
	}

	var package = Path.GetFileNameWithoutExtension(sarifPath);
	var sarif   = JsonNode.Parse(File.ReadAllText(sarifPath))!;
	var run     = sarif["runs"]?[0];
	var results = run?["results"]?.AsArray();

	if (results is null)
	{
		Console.Error.WriteLine($"SarifGate: {sarifPath} has no runs[0].results -- the analysis build did not run as expected");
		return 1;
	}

	var errors = results
		.Where(r => r?["level"]?.GetValue<string>() == "error")
		.Select(r => $"{DescribeLocation(r)}: {r!["ruleId"]?.GetValue<string>()}: {r["message"]?["text"]?.GetValue<string>()}")
		.ToArray();

	if (errors.Length > 0)
	{
		Console.Error.WriteLine($"SarifGate: {package}'s analysis project did not compile ({errors.Length} error-level result(s)); " +
			"its counts are meaningless and are not trusted:");
		foreach (var error in errors)
			Console.Error.WriteLine("  " + error);
		return 1;
	}

	foreach (var result in results)
	{
		var ruleId = result!["ruleId"]?.GetValue<string>();
		if (ruleId is null || !trackedRules.Contains(ruleId))
			continue;

		if (result["suppressions"] is JsonArray { Count: > 0 })
		{
			// Already suppressed in source (a stated-reason #pragma, or the emitter's own
			// suppress/restore fence around a field nothing assigns): the compiler does not
			// fail a build on it today, so this job does not either.
			suppressed++;
			continue;
		}

		var path = ArtifactPath(result);
		if (path is null)
			continue;

		// prepare-analysis.sh always copies the emitted .g.cs this job cares about into
		// "<analysis-dir>/gen/..." -- nowhere else. A result under any other path is either
		// the grammar author's own file (a `#line`-mapped region's SARIF location is already
		// that file, not the copy -- the SARIF errorlog resolves #line itself; there is no
		// further mapping to do here) or the package's hand-written source compiled alongside
		// the copy for symbol resolution, and either way it is not the emitter's text to gate.
		if (!UnderGenDirectory(path))
		{
			excluded++;
			continue;
		}

		var key = (package, ruleId);
		measured[key] = measured.GetValueOrDefault(key) + 1;
	}
}

var baseline = ReadCounts(countsFile);
var packages = measured.Keys.Select(k => k.Package)
	.Union(baseline.Keys.Select(k => k.Package))
	.Distinct()
	.OrderBy(p => p, StringComparer.Ordinal)
	.ToArray();

if (mode == "update")
{
	var regressions = new List<string>();

	foreach (var package in packages)
	{
		foreach (var rule in trackedRules)
		{
			var now = measured.GetValueOrDefault((package, rule));
			var was = baseline.GetValueOrDefault((package, rule));
			if (now > was)
				regressions.Add($"{package}\t{rule}\t{was} -> {now}");
		}
	}

	if (regressions.Count > 0 && !allowIncrease)
	{
		Console.Error.WriteLine("SarifGate update: refusing to write an increase without --allow-increase:");
		foreach (var line in regressions)
			Console.Error.WriteLine("  " + line);
		return 1;
	}

	WriteCounts(countsFile, packages, trackedRules, measured);
	Console.WriteLine($"SarifGate: wrote {countsFile} ({packages.Length} package(s) x {trackedRules.Length} rule(s)).");
	return 0;
}

var mismatches = new List<string>();

foreach (var package in packages)
{
	foreach (var rule in trackedRules)
	{
		var now = measured.GetValueOrDefault((package, rule));
		var was = baseline.GetValueOrDefault((package, rule));
		if (now != was)
			mismatches.Add($"{(now > was ? "REGRESSION" : "IMPROVED  ")} {package}\t{rule}\t{was} -> {now}");
	}
}

Console.WriteLine("Measured counts (package, rule, count):");
foreach (var package in packages)
{
	foreach (var rule in trackedRules)
		Console.WriteLine($"  {package}\t{rule}\t{measured.GetValueOrDefault((package, rule))}");
}

Console.WriteLine();
Console.WriteLine($"SarifGate check: {mismatches.Count} pair(s) differ from {countsFile}, " +
	$"{excluded} finding(s) outside gen/ (grammar-author or hand-written code, never gated), {suppressed} already suppressed in source.");

if (mismatches.Count > 0)
{
	Console.WriteLine("Run the 'update' mode locally and commit the result -- every number must move " +
		"down, never up, or say in the commit message why an increase is accepted:");
	foreach (var line in mismatches)
		Console.WriteLine("  " + line);
	return 1;
}

return 0;

static string? ArtifactPath(JsonNode result)
{
	var uri = result["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]?.GetValue<string>();
	return uri is null ? null : Uri.UnescapeDataString(uri).Replace("file://", "");
}

static bool UnderGenDirectory(string path)
{
	return path.Replace('\\', '/').Split('/').Contains("gen");
}

static string DescribeLocation(JsonNode? result)
{
	if (result is null)
		return "(no location)";

	var path = ArtifactPath(result);
	var line = result["locations"]?[0]?["physicalLocation"]?["region"]?["startLine"]?.GetValue<int>();

	return path is null ? "(no location)" : $"{path}:{line}";
}

static Dictionary<(string Package, string Rule), int> ReadCounts(string path)
{
	var counts = new Dictionary<(string Package, string Rule), int>();
	if (!File.Exists(path))
		return counts;

	foreach (var line in File.ReadAllLines(path))
	{
		if (line.Length == 0 || line.StartsWith('#'))
			continue;

		var parts = line.Split('\t');
		counts[(parts[0], parts[1])] = int.Parse(parts[2]);
	}

	return counts;
}

static void WriteCounts(string path, string[] packages, string[] rules, Dictionary<(string Package, string Rule), int> measured)
{
	using var writer = new StreamWriter(path, false);
	writer.NewLine = "\r\n";
	writer.WriteLine("# The generated-code count ratchet (docs/development.md). One line per (package, rule):");
	writer.WriteLine("# the ceiling SarifGate's 'check' mode holds that pair to. Written by:");
	writer.WriteLine("#   dotnet run --project tools/generated-analysis/SarifGate -- update tools/generated-analysis/counts.txt <sarif...>");
	writer.WriteLine("# Numbers may only go down across commits -- 'update' itself refuses to write an increase");
	writer.WriteLine("# without --allow-increase, and a decrease should be committed as soon as it is measured,");
	writer.WriteLine("# or the ratchet only protects against regressing to a number looser than reality.");
	writer.WriteLine("# package\trule\tcount");

	foreach (var package in packages)
	{
		foreach (var rule in rules)
			writer.WriteLine($"{package}\t{rule}\t{measured.GetValueOrDefault((package, rule))}");
	}
}
