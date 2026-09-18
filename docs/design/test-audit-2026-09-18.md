# Test suite audit — 2026-09-18

D12 (Igor): the stand measures first, each owner reviews their area by D12's rules after.
This is the measurement — build and run time per project, per class, per test, and a count
of tests too fast to be worth their own name.

Pinned 0-15, high priority. Full data:
[`test-audit-2026-09-18.json`](../../benchmarks/results/test-audit-2026-09-18.json). "Trivial"
means under 1 ms — xunit records to 3 decimals, so that bucket is what shows as 0.000-0.000
either way; it says nothing about whether a test is redundant, only that its own time is not
where a review should start.

## Per project

| project | build | run | tests | classes | trivial (<1ms) |
| --- | ---: | ---: | ---: | ---: | ---: |
| DotGram.Tests | 217.1 s | 220.8 s | 9,008 (1 skipped) | 100 | 6,951 |
| DotGram.Sql.Tests | 3.0 s | 1.8 s | 14,701 | 8 | 14,326 |
| DotGram.Finance.Tests | 115.6 s | 1.7 s | 6,556 | 13 | 6,011 |
| DotGram.Compatibility | 6 s | — (no tests) | — | — | — |
| DotGram.PackageSmoke | 4 s (pack+build) | — (no tests) | — | — | — |

Compatibility and PackageSmoke are build-only assertions (D12's own framing) — Compatibility
targets net8.0, netstandard2.0 and net472 and asserts nothing beyond compiling; PackageSmoke
packs the generator and prints `1 + 41 = 42`. Neither has a class or a test to measure.

**DotGram.Tests is the one slow on both axes.** Its own build (217 s) and its own run (221 s)
are each larger than the other four projects' numbers combined. DotGram.Finance.Tests is slow
to build (115 s) and fast to run (1.7 s) — this is D12's own example: Fix44's 59 MB compiles on
every build because Finance.Tests uses it as an oracle, and the fix already named there
(move the oracle tests to a project of their own) is not measured here since it has not
happened yet. DotGram.Sql.Tests is fast on both axes despite the most tests of the three
(14,701) — TransactSqlTests alone is 12,662 of them, at 0.567 s total.

## DotGram.Tests → performance-3f, expr-2d

Top 10 of 100 classes by total time (json has the full top 30):

| class | total | tests | trivial |
| --- | ---: | ---: | ---: |
| ReaderTests | 143.8 s | 89 | 0 |
| OversizeTests | 114.4 s | 14 | 3 |
| BufferedInputTests | 98.4 s | 55 | 0 |
| SemanticTests | 73.8 s | 408 | 11 |
| FuzzTests | 67.3 s | 12 | 0 |
| GeneratorDriverTests | 61.3 s | 151 | 0 |
| MaterializationPartitionTests | 54.9 s | 3 | 0 |
| CSharpEmitterTests | 49.5 s | 191 | 0 |
| StreamingRetentionTests | 49.2 s | 7 | 1 |
| ReferenceDifferentialTests | 46.5 s | 4 | 0 |

Top 10 individual tests (json has the top 30):

| test | time |
| --- | ---: |
| BufferedInputTests.Large_split_dispatch_keeps_char_and_byte_backtracking_correct | 49.7 s |
| ReaderCoverageTests.Every_parser_and_example_is_read_by_the_reader | 37.4 s |
| OversizeTests.Any_part_size_generates_a_parser_that_parses | 36.9 s |
| OversizeTests.Any_part_size_generates_a_parser_that_parses | 33.5 s |
| RefusalTests.Every_refusal_is_the_one_it_was | 31.8 s |
| MaterializationPartitionTests.Large_factory_choices_preserve_values_and_spans_across_all_input_forms | 29.5 s |
| GeneratorCostTests.And_this_is_where_the_generator_spends_it | 27.0 s |
| ReferenceDifferentialTests.The_automaton_agrees_with_the_semantics | 22.5 s |
| OversizeTests.Any_part_size_generates_a_parser_that_parses | 21.5 s |
| ReaderTests.Every_input_reads_the_same_both_ways | 17.3 s |

`OversizeTests` appearing three times in the top 10 individual tests is the same theory tested
at different sizes, not three different claims — a `[Theory]`'s cases show up as separate rows
here.

## DotGram.Sql.Tests → sql-ff

Fast overall (1.8 s for 14,701 tests); top 8 of 8 classes, all of them:

| class | total | tests | trivial |
| --- | ---: | ---: | ---: |
| SqlWriterTests | 0.977 s | 141 | 113 |
| SqlStandardTreeTests | 0.719 s | 231 | 125 |
| SqlStandardParserTests | 0.644 s | 1,574 | 1,408 |
| TransactSqlTests | 0.567 s | 12,662 | 12,609 |
| SqlWalkerTests | 0.496 s | 10 | 2 |
| SqlStandardAstMapTests | 0.140 s | 4 | 0 |
| Sql92ParserTests | 0.135 s | 75 | 68 |
| AstReferenceTests | 0.119 s | 4 | 1 |

Nothing here is worth a "top 30 individual tests" table read on its own — the slowest single
test is 0.461 s (`SqlWriterTests.What_the_writer_prints_reads_back_the_same`); the full top 30
is in the json for completeness. This project's weight is in its test *count*, not its time.

## DotGram.Finance.Tests → finance-03

Run is fast (1.7 s); the cost here is entirely the 115.6 s build, from Fix44 (D12 already
names the fix). Top 10 of 13 classes:

| class | total | tests | trivial |
| --- | ---: | ---: | ---: |
| FixFlatFieldsTests | 1.478 s | 399 | 257 |
| FixTests | 1.111 s | 212 | 120 |
| HandFixTests | 0.744 s | 2,582 | 2,481 |
| FixFieldGrammarTests | 0.650 s | 206 | 131 |
| FixRetentionTests | 0.575 s | 25 | 18 |
| Fix44StreamingTests | 0.546 s | 189 | 101 |
| Fix44Tests | 0.482 s | 399 | 376 |
| FixRecoveryTests | 0.334 s | 6 | 5 |
| FixLogTests | 0.259 s | 20 | 15 |
| FixConvertFastPathTests | 0.170 s | 86 | 80 |

Top individual tests, for context rather than as a target — none is slow in absolute terms:

| test | time |
| --- | ---: |
| FixFlatFieldsTests.Lazy_streams_match_all_standard_message_fixtures | 0.400 s |
| FixRecoveryTests.Recovery_returns_errors_in_order_with_original_input_and_absolute_positions | 0.334 s |
| FixTests.Malformed_inputs_return_invalid_fields_in_both_parsers | 0.281 s |
| FixFlatFieldsTests.A_recovered_field_is_yielded_without_reading_the_next_field | 0.272 s |
| FixTests.All_fixtures_match_types_values_and_locations_on_all_inputs | 0.224 s |

## Method

`dotnet build <project>.csproj -c Release -m:1 -nodeReuse:false -p:UseSharedCompilation=false`
after removing that project's own `bin`/`obj` (its dependencies were left warm — this is the
cost of rebuilding the project itself, not the whole dependency chain from nothing). Then the
built `.exe` directly (not `dotnet test`, whose MTP double-dash arguments are unreliable in
this SDK) with `-result-xml`, xunit's native flag: plain UTF-8, one `<test time="…">` per test
inside a `<collection>` per class. No JSON schema needed beyond what the XML already carries;
`.work/test-audit.py` (not committed, reproducible from this file) aggregates it into the json.

Not UTF-16, despite the note the order carried — that turned out to be a different, unrelated
`dotnet test` log from an earlier run, not this pipeline (confirmed with architect during
the run).
