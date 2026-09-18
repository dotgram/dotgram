# Finance continuation instructions

Snapshot: 2026-09-17. Read this when continuing the Finance work on another machine.
This is a handoff, not a replacement for the code or language specification.

## Integration update

The user subsequently requested merging all Finance work into local main. Merge
`55b1310d` includes main through `22330721`, the Finance enum/documentation/decimal
changes, and the user's factory formatting commit `196850d7`. Both Finance targets
built and all 6441 Finance tests passed after integration. The earlier branch-status
and pending-merge notes below describe the original handoff snapshot and are superseded
by this update. Also read `docs/design/performance-handoff-2026-09-17.md` for the
new generator optimizations. No remote push is implied by the local integration.

## Start here

1. Read `AGENTS.md`, `CLAUDE.md`, `docs/coding-conventions.md`, and relevant
   `.claude/rules/` instructions before editing.
2. Inspect `git status`, the current branch, and recent commits. Preserve user edits.
3. This work was explicitly assigned to branch `codex/finance` in a separate worktree.
   That user instruction takes precedence over the generic main-only workflow in
   `CLAUDE.md`. Local paths were `P:/dotgram.WorkTrees/finance` and `P:/dotgram` for main;
   choose paths appropriate to the new machine.
4. Ensure the checkout includes this handoff commit, not merely the last merged main.
   At snapshot time main was `b13aac21`; Finance additionally contained `ca66e81c`,
   `dae627bd`, and `af743a0f`. The handoff commit also preserves the user's latest
   formatting changes in `FixConvert.cs` and `FixTypes.cs`.
5. No remote push was performed during this handoff. A local commit is not sufficient
   to make the files available on another device: transfer the branch/commit or the
   repository before continuing there. Do not assume origin already contains it.

The user's latest request was to save progress and plans. The immediately preceding
conversation concerned whether DateOnly/TimeOnly can replace the custom FIX types
on netstandard2.0. No replacement of the temporal types has been approved yet.

## User preferences and decisions

- Russian conversation; English repository text, XML documentation and commits.
- Commit all current changes and leave a clean tree. Preserve intentional user edits;
  do not restore earlier agent formatting or discard unrelated edits.
- Use block-bodied handwritten methods and constructors. Follow tabs, CRLF and UTF-8
  BOM rules for C#; Markdown is UTF-8 without BOM. Always specify encoding explicitly
  in scripts: Python's default encoding on the previous Windows machine was not UTF-8.
- Keep related code together. Avoid helper layers or speculative abstractions.
- The parser returns flat typed fields. Message assembly, groups and validation remain
  separate, explicitly invoked APIs.
- Keep public APIs simple. SOH and log parsing have separate methods; log separators
  accept a pipe with surrounding spaces. The final field may end at EOF.
- Keep the retained FIX definitions under manual maintenance. Do not restore external
  specifications, inventories, generation machinery or provenance materials that the
  user deliberately removed. Existing field cases, factories, schemas and fixtures stay.
- Do not spawn agents unless the user explicitly authorizes delegation.

## Current architecture and files

### Production FIX

`src/DotGram.Finance/Fix/`:

- `FixGrammar.cs`: small production grammar with computed `switch`, typed integer
  tags, `@ReadData` inline, configurable binary pairs and `recover`.
- `FixParser.cs`: `Parse` for SOH and `ParseLog` for padded pipes. String/span/byte-array
  APIs return `FixField[]`; TextReader/Stream APIs return lazy `IEnumerable<FixField>`
  and leave input open.
- `FixOptions.cs`: optional replacement length/data dictionary. A length/data pair is
  returned as one binary data field, with coordinates covering both headers.
- `FixField.cs`: the base, nested `Typed<T>`, `Invalid`, `Unknown`, and all 912 concrete
  cases in one file. `FixField.Cases.cs` was removed. Public members and primary
  constructor parameters have XML comments; binary/length cases have extra remarks.
- `FixFieldType.cs`: 912 named enum members matching the standard field tags.
  `FixField.FieldType` casts `Tag`; custom tags and zero can be unnamed enum values.
- `IFixLocation.cs`: location callback used by generated parsing.
- `FixFactory.cs` and `FixConvert.cs`: typed construction and primitive conversion.
- `FixTypes.cs`: remaining temporal structs only: FixDate, FixTime, FixTimestamp,
  FixMonthYear. FixDecimal has been removed.
- `FixMessages` and the schema/model files: separately invoked semantic assembly.
  Source-backed `FixNumber` still exists; do not confuse it with the removed
  `FixDecimal`. Its TryGetDecimal helper is distinct from a typed field's Value.

The large alternative-per-tag grammar remains in
`examples/DotGram.Examples/Finance/Fix44/` as a comparison and generator stress case.

### Numeric decision just implemented

The user explicitly chose standard `decimal` for now. All decimal field cases use
`FixField.Typed<decimal>`. Integer fields still use BigInteger; changing them was
not requested.

`FixConvert.Decimal` validates the FIX spelling, uses invariant .NET parsing for
characters and Utf8Parser for bytes, then verifies that parsing did not discard any
nonzero fractional digits. Overflow or rounding produces `IsValid == false`; it
must not silently change the value. Trailing fractional zeros are allowed.
`DecimalTests.cs` covers limits, underflow, rounding, signs, separators and both input
representations. `FixFieldGrammarTests` now asserts decimal values directly.

The FIX float requirement discussed was support for 15 significant digits; fractional
scale is based on business needs and counterparty agreement. This did not mandate an
arbitrary-precision CLR type. Do not claim decimal covers every possible negotiated
scale. The chosen implementation explicitly rejects values it cannot represent exactly.

### Handwritten comparison library

`examples/DotGram.Handwritten/` is a non-packable net10.0 library in the solution.
It contains HandExpression, HandSqlTokens, historical HandSqlOriginal, and
`Fix/HandFixParser.cs`. Benchmark runners remain in their benchmark projects.
`benchmarks/DotGram.HandDeferred` remains a separate deferred-construction experiment.

HandFixParser independently recognizes fields and buffers char/byte inputs. It shares
only field models, conversion factories and binary dictionaries through Finance's
friend assembly. It does not call FixParser or the generated grammar. It supports
custom pairs, binary contents including separators, recovery, source coordinates,
lazy enumeration, bounded retention and all production input forms. Error wording is
independent; tests compare error extents and raw contents, not identical prose.

`tests/DotGram.Finance.Tests/HandFixTests.cs` provides differential coverage.
`benchmarks/DotGram.Finance.Benchmarks/HandFixBenchmarks.cs` contains BenchmarkDotNet
and quick comparison/first-call modes. Read the benchmark README for methodology.

## Last validation and performance evidence

The handoff snapshot, including the latest user formatting edits, was rechecked:
both Finance targets built and all 6441 Finance tests passed again.

At `af743a0f`, both Finance targets (netstandard2.0 and net10.0) built, the Finance
benchmark project built, and all 6441 Finance tests passed. This includes 2582
handwritten differential cases and 24 new decimal tests.

Historical handwritten-parser measurements in the benchmark results folder PRECEDE
switching to decimal. They are explicitly marked historical in the README. They found
about 6x on an order string, 3.9x on an order byte stream, much larger gaps on long
contiguous inputs, but slower string recovery and larger handwritten streaming
allocations. Do not quote these as current decimal performance without rerunning.

`TagPrefixLength` is cached in a readonly field. Prior measurements found an 8-byte
per-field allocation increase from object alignment and no convincing general
speedup. The user pointed out binary-heavy processing merits separate measurement;
no removal was authorized and caching remains. Do not silently revert it.

Earlier 12 GRAM4008 failures were fixed in `9eb9eb53`: LowerGroupValues must preserve
trailing switch constructions belonging to the containing rule. Regression tests are
in SwitchTests. The fix is already in main and is unrelated to current temporal work.

## Portable verification commands

Use the .NET SDK selected by `global.json` (10.0.302, latestFeature roll-forward).
Run from the repository root. Do not assume the previous machine's T: artifacts exist.

```powershell
dotnet build tests/DotGram.Finance.Tests/DotGram.Finance.Tests.csproj -c Release --nologo
dotnet tests/DotGram.Finance.Tests/bin/Release/net10.0/DotGram.Finance.Tests.dll
dotnet build src/DotGram.Finance/DotGram.Finance.csproj -c Release --nologo
dotnet build benchmarks/DotGram.Finance.Benchmarks/DotGram.Finance.Benchmarks.csproj -c Release --nologo
git diff --check
```

Use actual output paths printed by MSBuild if output layout is overridden. The test
projects are executable xUnit runners. The previous machine used
`--artifacts-path T:/TEMP/finance-manual-maintenance`; once dependencies were current,
`-p:BuildProjectReferences=false --no-restore` avoided rebuilding enormous examples.
Do not use that shortcut on a fresh checkout or after dependency/API changes.
The large Fix44 example can make a full dependency build take several minutes.

```powershell
dotnet run -c Release --project benchmarks/DotGram.Finance.Benchmarks -- --hand-fix-performance
dotnet run -c Release --project benchmarks/DotGram.Finance.Benchmarks -- --hand-fix-first generated
dotnet run -c Release --project benchmarks/DotGram.Finance.Benchmarks -- --hand-fix-first handwritten
dotnet run -c Release --project benchmarks/DotGram.Finance.Benchmarks -- --filter "*HandFixBenchmarks*"
```

Read `.claude/rules/profiling.md` before performance work. Alternate isolated runs,
keep builds/tests out of timing, distinguish first-call JIT from warmed throughput,
and compare allocations and behavior as well as timing.

XML documentation verification found pre-existing issues outside FixField: malformed
XML in Machine.cs and an unresolved `As` cref in generated DotGram.Attributes.g.cs.
Normal builds are clean. Do not mistake these for newly introduced FixField comments.
Finance-only documentation checks succeeded with BuildProjectReferences=false,
GenerateDocumentationFile=true, NoWarn=1591 and WarningsNotAsErrors=CS1574; these were
command-line checks, not committed project suppressions.

## Next discussion and possible work

1. Resolve temporal representation with the user before changing public types.
   FixDate currently retains year 0000; FixTime retains second 60 and fractional
   digits beyond TimeOnly's 100 ns resolution. DateOnly/TimeOnly cannot express all
   of these. Replacing them requires an explicit decision on rejection or preservation.
2. The installed Meziantou.Polyfill 1.0.165 supplies methods that REQUIRE DateOnly and
   TimeOnly; it does not supply those missing types for netstandard2.0. A different
   backport would require checking public type identity, dependencies and consumer
   compatibility. Do not assume an internal source polyfill solves public API types.
3. The user suggested multiple parser variants via `with`. This remains an idea, not
   implemented work: the ADT currently fixes numeric types, and construction is in
   FixContext/FixFactory. A generic field ADT or replaceable construction policy would
   also be needed. The immediate decision was to use decimal, not implement variants.
4. After representation decisions, rerun handwritten/generated performance comparisons
   against the new decimal model. Binary-heavy semantic processing, contiguous-input
   scaling, recovery and handwritten buffer allocations are useful separate targets.
5. Merge Finance into local main only when requested. The last explicit merge happened
   at b13aac21; the enum, API documentation and decimal changes are newer.
