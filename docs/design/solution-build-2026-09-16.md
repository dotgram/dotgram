# Full solution build profile, 2026-09-16

## Scope

Baseline: `2b58638`, current performance worktree. This is a new diagnostic baseline,
not a measured regression against the earlier four-minute solution build. Grammar,
compiler inputs and environment have changed since that run.

Visual Studio MSBuild 18.9.1 x64, .NET SDK 10.0.400, Debug, one MSBuild node, node reuse
and shared compilation disabled. Existing package restore was used. All solution
projects and both shipping library targets were included, including VSIX and compatibility
projects. Output assemblies were redirected under `.work/solution-profile/output/`;
normal project intermediate directories were used. No tests, builds or profilers were
run concurrently with the measured builds. These are individual observations, not
statistical estimates, and do not reproduce Visual Studio's parallel/shared-server settings.

The initial attempt failed before useful work because the inherited environment contained
both PATH and Path. The successful runs normalized environment key casing in a Python
subprocess. That failed attempt is excluded.

## Whole-solution observations

Times are MSBuild BuildStarted/BuildFinished timestamps from binary logs.

| Operation | Seconds | Csc calls |
|---|---:|---:|
| Rebuild | 632.420 | 21 |
| Build without changes | 2.578 | 0 |
| Append a comment to SqlSpan.cs | 185.207 | 2 |
| Append a comment to TransactSql.gram | 170.858 | 4 |

The C# edit compiled SQL for its two targets only. The grammar edit also compiled
DotGram.Benchmarks and DotGram.Sql.Tests. The difference between 185 and 171 seconds
is not evidence that grammar edits are cheaper: these are single runs with visible
variation in generator timings.

All four builds succeeded with zero warnings/errors. A Build without changes correctly
skips compilation; the solution is not unconditionally regenerating every project.

## Where the rebuild goes

Csc task wall times include source generators and compilation of their output. MSBuild
project/target inclusive durations are not summed.

| Project | Csc calls | Csc seconds | Reported DotGram generator seconds |
|---|---:|---:|---:|
| Finance | 2 | 430.890 | 7.104 |
| SQL | 2 | 148.899 | 54.159 |
| ExpressionLanguage | 2 | 9.664 | 3.321 |
| Web | 2 | 6.352 | 2.503 |
| Other projects | 13 | 32.399 | — |
| Total | 21 | 628.205 | — |

Csc accounts for 99.3% of rebuild wall time. Finance and SQL together account for 91.7%.
Generator numbers are the compiler's `/reportanalyzer` rows for
DotGram.Generation.GramGenerator in the same build, extracted from binlog messages
(the normal text log omits them). Analyzer counters can overlap, so do not add them to
Csc durations or subtract all analyzer totals to claim an exact binding/emission time.

Finance takes 220.696 and 210.195 seconds per target, while its DotGram generator takes
3.796 and 3.308 seconds. This prioritizes profiling the compiler process, including
binding, lowering, emission, debug information and GC, rather than another generator
text-buffer optimization. The counters alone do not identify which compiler phase wins.

Current large generated files include FixGrammar (59,745,055 bytes), SqlStandardParser
(68,248,462), TransactSqlParser (24,930,796 before the probe) and its Located publication
(26,230,181). These are UTF-8 source bytes, not IL or live memory. Generated output
directories contain stale files from earlier experiments, so their total file counts
and directory sizes are not valid compiler-input totals.

## Why a comment can still be expensive

After the C# comment edit, all 35 files in the SQL generated output directory retained
the baseline hashes. Nevertheless both SQL compilations ran. Reported DotGram generator
time was 39.787 + 35.906 = 75.693 seconds in that build.

After the grammar comment edit, exactly one file changed: TransactSqlParser.g.cs.
Removing just the escaped appended comment from its GramSourceAttribute string restores
the exact baseline SHA-256. Parser implementation text is unchanged. The attribute
publishes grammar source for consumers, so its changed metadata also invalidates the
two dependent compilations. Dropping comments from that metadata requires a separate
contract/source-location audit; this experiment does not propose silently stripping it.

An incremental generator can reuse stages in a retained GeneratorDriver, but that is
not a persistent result cache across these separate compiler processes. Earlier driver
reuse measurements must not be treated as evidence that a command-line C# edit skips
all grammar generation. Likewise, a shared compiler server alone is not proof that it
retains a generator's incremental results between builds.

## Next implementation priorities

1. Profile the actual Finance compiler process. Identify expensive methods/constructs
   before changing emitter structure. Compare debug-information settings as an isolated
   experiment only if the profile points there; do not change project defaults blindly.
2. Inventory repeated large generated methods and shared publication machinery in
   Finance and SQL. Reduce the compiler input where this preserves parser semantics and
   runtime costs; validate short and large inputs, allocations and generated source/IL.
3. Investigate reuse across SQL compilations with explicit semantic dependency tracking.
   A file-content cache alone cannot safely ignore host C# type/member changes, target
   frameworks, references or options. Avoid a global unbounded cache.
4. Continue the separate parserState scaling investigation after the dominant build
   cost has a concrete compiler profile. No parser runtime improvement is claimed here.

## Reproduction and artifacts

Use the installed Visual Studio MSBuild executable, not dotnet build, for the complete
solution including VSIX. In an environment without duplicate case variants of variables:

```powershell
& $msbuild DotGram.slnx /t:Rebuild /p:Configuration=Debug /m:1 /nr:false `
  /p:UseSharedCompilation=false /p:ReportAnalyzer=true /p:NuGetAudit=false `
  /p:BaseOutputPath=P:/WorkTrees/2909/dotgram/.work/solution-profile/output/ `
  /bl:.work/solution-profile/rebuild.binlog /v:normal
```

For subsequent cases use `/t:Build`, a separate binlog, and change only the stated input.
The probe was a trailing `// Build-profile probe; no semantic change.` comment.
Restore original bytes afterwards and rebuild to restore matching output metadata.

[Extracted measurements](../../benchmarks/results/solution-build-2026-09-16.json)
retain per-task Csc times and compiler generator/analyzer messages. Full binlogs, text
logs, commands, hashes, run.py, followup.py and the binlog reader are local ignored
artifacts under `.work/solution-profile/`. Binary logs are not published because they
can contain environment details.

## Restoration validation

Original source bytes were restored and their timestamps refreshed before the final
restoration build. It succeeded with zero warnings/errors; all 35 SQL output hashes
matched baseline again. A subsequent unchanged build took 2.497 seconds with zero Csc
calls. An earlier timestamp-preserving scratch no-op was excluded from published
results because output metadata still contained the probe then. Production source is
unchanged, so no new parser tests or runtime benchmarks were required for this audit.

## Concurrent main changes

During the measurement, main advanced to b28acfa. Commit 0635581 promotes the compact
FIX parser and moves the large legacy Fix44 grammar into examples. The timings above
remain measurements of 2b58638; they must not be attributed to the new production
Finance layout. A subsequent profile must locate that large grammar in its new project
and verify whether whole-solution compilation cost moved rather than disappeared.

Main was merged as 7f62902. Release builds and 52 focused guard/switch/recovery tests
passed, followed by all 3,808 Finance tests. The existing largest-tag regression was
updated to the renamed Fix/FixOptions API after the merge. NuGetAudit=false was used
for validation because the vulnerability service was inaccessible; repository settings
were unchanged. No full-solution timing is claimed for the merged layout.

A subsequent [file-splitting experiment](file-split-2026-09-16.md) reduced compilation
time by about 37% in a standalone Roslyn comparison with identical method text.
That result is not yet an end-to-end improvement measured in this solution setup.
