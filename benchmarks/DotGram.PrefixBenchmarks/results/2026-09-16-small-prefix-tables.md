# Prefix tables on small grammars and Web, 2026-09-16

## Conclusion

Do not replace the default strategy globally on this evidence. Shared-prefix
choices benefit even with four alternatives, especially for late branches. A
small choice already separated by its first characters can lose performance.
The relevant inputs include both accepted and refused text, not just grammar size.

All 13 current Web grammars emit byte-identical parser source with PrefixTables
false and true. The enabled assembly's GramAttribute metadata was checked: all
13 hosts explicitly carry PrefixTables=true. The benchmark checks both assemblies'
metadata before running, and checks that each synthetic table variant actually
contains generated transition-table fields. This rules out a missing option or
accidentally measuring an unchanged control grammar.

Web is therefore a no-op control, not evidence for or against the table algorithm.
For example, RFC 6265's twelve month alternatives use case-insensitive literals,
which this prototype excludes. JSON's Value choice mixes three literals with
rule calls, so it also fails the current all-alternatives prefix eligibility test.
No Web grammar or compiler default was changed by this investigation.

## Method

.NET 10.0.12, Release, Windows 11, Ryzen 9 9950X3D. Five fresh-process rounds,
alternating which variant runs first. Each case warms both delegates with 20000
operations. There are 35 synthetic cases and 14 public Web calls per round.
Synthetic cases use 500000 operations per sample; Web uses 100000. Parsing results
are consumed and checked, and every input has an explicit expected outcome.
Binding, reflection, input creation and GC collection are outside timing.
No builds or test jobs ran concurrently with measurements.

The main comparison sets DOTNET_TieredCompilation=0: all JIT compilation is fully
optimized from the start. The exploratory default-tiering runs showed warmup
transitions in these very short methods (for example, the first method case was
far slower than subsequent cases). Both sets are retained in the raw CSV. These
are diagnostic loop timings, not BenchmarkDotNet confidence intervals. Cold start,
stream input and realistic mixed-input frequency distributions were not measured.

Synthetic controls keep Direct=false in both variants. They isolate choice
selection; they are not claims about identical grammars currently shipping in Web:

- Methods8: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS, TRACE; typed integer result.
- Tags4/8/16/32/64: X00=, X01=, ... followed by lowercase text; typed integer result.
  All tag prefixes have equal length and share their initial character.
- Cases include first/middle/last alternatives, unknown prefixes, truncated input,
  and a recognized prefix followed by an invalid value.

## Selected results

Nanoseconds per complete input. Speedup is default divided by tables; below 1
means tables are slower. Medians and ranges come from five runs with FullOpts.

| Case | Default ns | Tables ns | Speedup | Default range | Tables range |
| --- | ---: | ---: | ---: | ---: | ---: |
| Methods8/early | 48.05 | 44.83 | 1.07x | 42.77-48.27 | 44.47-49.47 |
| Methods8/late | 43.87 | 45.72 | 0.96x | 42.95-44.80 | 44.86-46.56 |
| Methods8/near-invalid | 23.65 | 25.88 | 0.91x | 23.08-24.05 | 25.46-26.70 |
| Tags4/early | 61.23 | 61.99 | 0.99x | 60.12-61.97 | 59.99-62.40 |
| Tags4/late | 98.54 | 61.69 | 1.60x | 97.78-100.72 | 60.73-62.42 |
| Tags8/late | 156.65 | 67.65 | 2.32x | 153.73-157.63 | 66.35-68.49 |
| Tags16/late | 262.17 | 68.95 | 3.80x | 258.89-263.78 | 67.91-77.17 |
| Tags32/late | 455.54 | 69.35 | 6.57x | 442.53-464.20 | 68.13-72.21 |
| Tags64/late | 1052.41 | 74.41 | 14.14x | 1033.86-1059.18 | 74.27-76.62 |
| Tags4/unknown | 64.09 | 65.41 | 0.98x | 62.48-64.70 | 64.07-67.17 |
| Tags4/truncated | 60.58 | 63.33 | 0.96x | 60.41-61.86 | 62.23-63.40 |

The first tag stays near the same cost, while late tags stop paying for preceding
alternatives. Fewer choice entries also reduce managed allocations: Tags4/late
falls from 136 to 48 B/op; Tags64/late from 1192 to 48 B/op (see raw CSV for all
cases). On an unknown or truncated prefix the table probe precedes the retained
old failure path, so it cannot remove that old work and may add overhead.

Methods8/late is TRACE: its first character already identifies the branch for the
old strategy. The table adds character transitions there. Small single-digit
percentage differences should not be generalized beyond this experiment; the
large shared-prefix gains are much less sensitive to timing noise.

## What to select automatically later

An automatic strategy should account for first-set overlap, prefix length, the
number of alternatives left after first-character dispatch, table size, and the
cost of its failure path. Preserve the existing predictive/specialized paths where
they already decide cheaply. A threshold on total rule or alternative count alone
misses both the four-alternative win and the eight-method loss.

Extending eligibility to case-insensitive literals, rule calls or mixed choices
would be a separate change with semantic tests and fresh measurements. Enabling
the current flag globally would leave all Web parser bodies unchanged, but that
is not evidence that a broader table strategy should replace their current paths.

## Evidence

- [Raw measurements](2026-09-16-small-prefix-runs.csv): 980 samples, both runtime modes.
- [Web source manifest](2026-09-16-web-generation.json): hashes and sizes of all
  13 byte-identical generated parsers.
- Saved Web assemblies are in T:/TEMP/web-prefix/default and tables. Their hashes:
  - default: `78E0595358CBF75BE90AC3DC32B5F4ED412A61EC0E244B6B496FF2AA4D96430F`
  - tables: `46D2534D9EB54D8A45391C75FF8E704FB8909781468BA79C9909E50EA7BDEDE9`

The new benchmark project and both experimental Web builds succeeded without
warnings or errors. Runtime validation checks all 49 input expectations for both
variants, all 13 Web options, and generated tables for all six enabled controls.
Web source files were restored byte for byte after collecting the two assemblies.

## All FullOpts results

These include the Web no-op controls. Differences between identical generated
Web bodies reflect measurement/JIT placement effects and must not be attributed
to the prefix strategy.

| Case | Default ns | Tables ns | Speedup | Default range | Tables range |
| --- | ---: | ---: | ---: | ---: | ---: |
| Methods8/early | 48.05 | 44.83 | 1.07x | 42.77-48.27 | 44.47-49.47 |
| Methods8/invalid-first | 44.24 | 45.03 | 0.98x | 42.80-45.00 | 43.98-45.38 |
| Methods8/late | 43.87 | 45.72 | 0.96x | 42.95-44.80 | 44.86-46.56 |
| Methods8/near-invalid | 23.65 | 25.88 | 0.91x | 23.08-24.05 | 25.46-26.70 |
| Methods8/truncated | 57.07 | 48.05 | 1.19x | 42.91-58.05 | 46.29-60.06 |
| Tags16/bad-value | 135.63 | 32.80 | 4.13x | 134.18-137.59 | 32.19-33.03 |
| Tags16/early | 64.19 | 64.23 | 1.00x | 62.68-65.06 | 62.65-73.75 |
| Tags16/late | 262.17 | 68.95 | 3.80x | 258.89-263.78 | 67.91-77.17 |
| Tags16/middle | 172.22 | 69.18 | 2.49x | 170.42-173.55 | 67.01-77.07 |
| Tags16/truncated | 211.37 | 213.30 | 0.99x | 208.39-213.84 | 209.49-216.90 |
| Tags16/unknown | 217.22 | 222.41 | 0.98x | 214.27-220.41 | 220.64-231.51 |
| Tags32/bad-value | 283.75 | 33.37 | 8.50x | 274.25-285.29 | 32.52-34.08 |
| Tags32/early | 63.63 | 64.85 | 0.98x | 62.94-64.86 | 64.20-65.55 |
| Tags32/late | 455.54 | 69.35 | 6.57x | 442.53-464.20 | 68.13-72.21 |
| Tags32/middle | 285.93 | 69.50 | 4.11x | 284.23-291.26 | 68.11-71.23 |
| Tags32/truncated | 395.35 | 391.35 | 1.01x | 386.40-404.16 | 386.51-400.85 |
| Tags32/unknown | 400.11 | 409.88 | 0.98x | 395.03-411.44 | 397.06-412.55 |
| Tags4/bad-value | 43.36 | 29.32 | 1.48x | 43.15-44.85 | 28.58-30.01 |
| Tags4/early | 61.23 | 61.99 | 0.99x | 60.12-61.97 | 59.99-62.40 |
| Tags4/late | 98.54 | 61.69 | 1.60x | 97.78-100.72 | 60.73-62.42 |
| Tags4/middle | 97.64 | 61.57 | 1.59x | 94.87-98.54 | 59.69-62.64 |
| Tags4/truncated | 60.58 | 63.33 | 0.96x | 60.41-61.86 | 62.23-63.40 |
| Tags4/unknown | 64.09 | 65.41 | 0.98x | 62.48-64.70 | 64.07-67.17 |
| Tags64/bad-value | 736.67 | 37.95 | 19.41x | 727.77-760.68 | 37.09-38.98 |
| Tags64/early | 66.42 | 68.48 | 0.97x | 65.54-67.89 | 67.65-69.62 |
| Tags64/late | 1052.41 | 74.41 | 14.14x | 1033.86-1059.18 | 74.27-76.62 |
| Tags64/middle | 565.58 | 74.00 | 7.64x | 553.12-573.86 | 73.32-76.52 |
| Tags64/truncated | 861.91 | 892.61 | 0.97x | 844.82-886.86 | 877.61-912.22 |
| Tags64/unknown | 968.49 | 1022.26 | 0.95x | 963.11-983.62 | 1006.90-1041.19 |
| Tags8/bad-value | 67.48 | 30.55 | 2.21x | 66.80-69.47 | 30.28-31.20 |
| Tags8/early | 63.13 | 62.77 | 1.01x | 62.15-64.06 | 61.11-63.02 |
| Tags8/late | 156.65 | 67.65 | 2.32x | 153.73-157.63 | 66.35-68.49 |
| Tags8/middle | 108.75 | 62.07 | 1.75x | 107.47-110.46 | 60.64-63.28 |
| Tags8/truncated | 111.34 | 112.65 | 0.99x | 110.20-115.36 | 111.44-116.61 |
| Tags8/unknown | 116.82 | 116.28 | 1.00x | 114.33-117.03 | 115.90-117.22 |
| Web/JsonValue/invalid | 109.92 | 109.49 | 1.00x | 107.22-113.41 | 109.02-115.02 |
| Web/JsonValue/long | 7238.59 | 7272.74 | 1.00x | 7210.34-7309.57 | 7203.20-7315.57 |
| Web/JsonValue/short | 150.60 | 156.04 | 0.97x | 147.92-191.27 | 153.67-156.98 |
| Web/LanguageTag/invalid | 185.13 | 183.69 | 1.01x | 182.44-190.23 | 181.76-188.07 |
| Web/LanguageTag/long | 547.52 | 545.19 | 1.00x | 537.04-565.41 | 534.99-560.97 |
| Web/LanguageTag/short | 201.39 | 195.25 | 1.03x | 195.31-327.97 | 191.11-428.56 |
| Web/MediaType/invalid | 92.01 | 90.56 | 1.02x | 89.74-139.59 | 89.81-153.56 |
| Web/MediaType/long | 490.69 | 485.66 | 1.01x | 479.56-802.26 | 477.57-740.80 |
| Web/MediaType/short | 125.64 | 127.95 | 0.98x | 123.36-139.17 | 123.42-133.43 |
| Web/Timestamp/invalid | 64.45 | 66.55 | 0.97x | 63.68-118.83 | 64.38-111.45 |
| Web/Timestamp/short | 230.27 | 236.47 | 0.97x | 226.54-435.47 | 224.64-351.11 |
| Web/UriReference/invalid | 300.19 | 298.53 | 1.01x | 293.51-308.55 | 295.65-304.48 |
| Web/UriReference/long | 1624.97 | 1631.24 | 1.00x | 1594.73-1662.02 | 1598.49-1648.64 |
| Web/UriReference/short | 251.45 | 257.77 | 0.98x | 247.21-276.99 | 252.47-265.04 |
