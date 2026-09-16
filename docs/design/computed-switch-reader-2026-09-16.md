# Computed switch in the direct reader and a SQL refactoring experiment

## Implementation

Computed choices now stay in ReaderWriter when their selectors meet the same capture
restrictions as guards. A C# helper evaluates the selector, maps its integral or string
key to a branch, and the reader emits that branch inline. Selected failure never tries
a sibling or default. The implementation retains conservative FIRST information: user
selectors are observations and must not be silently skipped by prediction.

Character readers that can replay retain the selected branch in their existing ways
tape. Retrying inside the case keeps that selection; retrying a preceding alternative
discards it. A committed lexical reader needs no such entry. Grammars with no open
ways do not acquire a replay entry merely because they contain a computed choice.

The single-character repetition shortcut now excludes computed choices. Treating their
bodies as a character-set union would discard both the selector and the case semantics.

Tests cover lexical committed calls, selected failure, outer rollback, inner replay,
typed/text captures, materialization invalidation, observations before failing bodies,
null strings, full-width integral keys, absent defaults, and repetitions. Existing tests
continue to cover character/byte streams and yielded results through the shared engine.

## SQL experiment

The experiment groups the JSON_OBJECT, JSON_OBJECTAGG and JSON_ARRAY alternatives of
TSqlValueFunction. The baseline recognizes each body and repeats the closing parenthesis,
optional CallTail and result construction. Two candidates were built:

- Computed switch: consume the existing name capture and opening parenthesis, then
  select the array body for name length 10 and the object body otherwise. No uppercasing
  or new keyword scan is needed, but handing the string capture to the selector creates
  an additional string before the result factory later takes its own capture.
- Common tail: keep ordinary predictive alternatives for the name and body, then share
  the closing parenthesis, optional CallTail and result construction.

Neither SQL candidate was retained. The common tail reduces code slightly but did not
establish a runtime improvement. The computed switch adds allocations and code. Dispatch
is not automatically an improvement over an already predictive choice. The production
SQL grammar remains unchanged; both candidates are documented below for reproduction.

## Generated source size

UTF-8 bytes, measured on the Release generated files:

| Parser | Baseline | Computed switch | Common tail |
| --- | ---: | ---: | ---: |
| T-SQL | 24,930,796 | 24,938,896 | 24,918,505 |
| T-SQL with locations | 26,230,181 | 26,236,916 | 26,216,226 |

The common tail saves 26,246 bytes across both generated files (about 0.05%). This is
a small local reduction, not a FIX-sized restructuring. Source bytes are not assembly
size, peak memory or JIT code size.

## Runtime and allocations

Exploratory measurements on Windows, .NET 10.0.12, Release, one process per variant
per round, three rounds with rotated order. Compilation and tests completed before
measurement. Tiered compilation was disabled in these runs to avoid changing JIT tiers
during short samples; these are not measurements of the default tiered-PGO steady state.
Each workload ran 2,000 warmup parses, then 30,000 measured parses (1,000 for the two
large inputs). Large projections contain 128 JSON calls. Inputs and validity assertions
match the new SqlJsonBenchmarks workloads. Managed allocations use the current thread's
allocation counter; they are bytes allocated, not peak or retained memory.

Medians across three processes, microseconds per parse:

| Input | Baseline | Computed switch | Common tail |
| --- | ---: | ---: | ---: |
| SELECT 1 | 1.797 | 1.803 | 1.879 |
| JSON_ARRAY, short | 3.468 | 3.471 | 3.593 |
| JSON_OBJECT, short | 4.243 | 4.129 | 4.468 |
| Mixed-case JSON_ARRAY | 3.051 | 3.198 | 3.031 |
| 128 JSON_ARRAY calls | 281.580 | 269.094 | 279.665 |
| 128 JSON_OBJECT calls | 369.715 | 373.272 | 424.183 |
| Invalid array body | 1.600 | 1.633 | 1.583 |
| Truncated object | 1.891 | 1.951 | 1.898 |

There is substantial process-to-process variability: even unchanged SELECT 1 ranges
from 1.763 to 3.101 microseconds in the baseline. These samples do not establish a
speedup, nor a precise regression percentage. The earlier default-tiering exploratory
runs also did not show a consistent switch advantage. No end-to-end SQL-versus-ScriptDom
speedup is claimed.

Allocation differences are deterministic. Baseline/common-tail short array: 1,504 bytes;
switch: 1,552. Short object: 1,720 versus 1,768. Large arrays: 89,896 versus 96,040;
large objects: 117,544 versus 123,688. The extra 48 bytes per JSON call also occur on
invalid/truncated JSON inputs. The common-tail candidate has baseline allocations.

[Raw measurements](../../benchmarks/results/computed-switch-sql-2026-09-16.jsonl).
The checked-in SqlJsonBenchmarks class keeps these workloads available for future
BenchmarkDotNet runs with its normal warmup and statistical reporting:

```powershell
dotnet run -c Release --project benchmarks/DotGram.Benchmarks -- --filter '*SqlJsonBenchmarks*'
```

## Reproducing the grammar candidates

In TSqlValueFunction, replace the JsonObjectName and JSON_ARRAY alternatives with
one of the following. The surrounding alternatives remain unchanged.

Computed selection (the name set is closed; only JSON_ARRAY has length 10):

```dotgram
| name: (JsonObjectName | "JSON_ARRAY"i) & '('
  & switch @(name!.Length) {
    case 10: body: JsonArrayBody?
    default: body: JsonObjectBody?
  } & ')' & tail: CallTail?
    => @(Syntax.Called(new Expression.RoutineInvocation(name, body ?? Expression.None), tail))
```

Shared tail without a selector:

```dotgram
| ( name: JsonObjectName & '(' & body: JsonObjectBody?
  | name: "JSON_ARRAY"i & '(' & body: JsonArrayBody?
  ) & ')' & tail: CallTail?
    => @(Syntax.Called(new Expression.RoutineInvocation(name, body ?? Expression.None), tail))
```

## Validation and next candidates

- 8,277 core tests passed, including 40 switch tests.
- 14,701 SQL tests passed with the common-tail trial and again after restoring the
  original grammar. Both trial grammars and the final SQL build had no warnings or errors.
- After restoration, both generated T-SQL source files match the baseline byte for byte.
- Compatibility built for net8.0, netstandard2.0 and net472.
- The benchmark project built successfully; formatting and git diff checks passed.

Next grammar experiments should use an already available numeric discriminator or a
span capture and eliminate substantial duplicated structure. Arbitrary string selectors
in a predictive language grammar need an allocation audit first. FIX remains the strong
example of that structural reduction; SQL and ExpressionLanguage need individually
measured candidates, preserving contextual keywords, ambiguous prefixes and scope effects.
