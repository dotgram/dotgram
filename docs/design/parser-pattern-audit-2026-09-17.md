# Parser pattern audit: 2026-09-17

Audit after merging Finance into main at `7c2d8812`. This document identifies
candidates, not implemented features or projected benchmark speedups.
The merged generator builds and all 12 delimiter-scan tests pass.

## Priority 1: recovery synchronization

`Item* recover Separator` still attempts the entire synchronization rule at every
position. `Machine.Recovery.cs`, `CompileRecoveringRepeat`, emits a Choice entry,
tries Sync, and advances by one on failure. For `' '* & '|' & ' '*`, a long run
of spaces followed by an ordinary character is examined repeatedly. The new
`Machine.Delimiter.cs` fast path applies to guarded repetitions, not recovery.

Confirmed using the Finance assembly built from `1986ce43` (included in the merge),
Release, .NET 10.0, string input:

```csharp
var input = "broken" + new string(' ', length) + "x|55=END";
var fields = FixParser.ParseLog(input);
```

Every input produced one Invalid field followed by Symbol("END"). After 128 warmup
parses per input, seven samples of at least 60 ms each were timed; the table shows
median microseconds per parse. This is a diagnostic scaling check, not a
BenchmarkDotNet confidence study.

| Internal spaces | Microseconds |
| ---: | ---: |
| 256 | 34.72 |
| 512 | 59.55 |
| 1024 | 213.03 |
| 2048 | 804.46 |
| 4096 | 3158.01 |

The larger inputs take approximately four times longer when length doubles,
consistent with the repeated suffix scan in the emitted code. Reuse a common
pure-delimiter search plan for normal text and recovery. Preserve the start of
padding, the consumed separator end, invalid raw data, error coordinates and
EOF recovery behavior. Do not skip guards, captures or side effects in Sync.

## Priority 2: an exact count of arbitrary input items

`any{N}` does not require character classification. `Machine.CompileRun` still
emits a loop, and `Machine.Reader.EmitRun` additionally reads each character even
when its test is true. Exact repetitions can use one extent check and position
advance, with the original failure path for precise diagnostics. Buffered sources
must still fetch and retain the required extent; this is not a constant-time I/O
claim. Variable-length or captured-per-item bodies are different cases.

Occurrences: `examples/DotGram.Examples/Formats/FixedWidthExample.cs` (`Raw(n)`)
and the large Fix44 example's Block4096/Block256/Block16 rules. Production Finance
already does this by hand with `ParserInput.TryAdvance` for binary payloads.

## Priority 3: quoted text with escapes

Typical shape: `Quote & (Ordinary | Escape & Escaped)* & Quote`.
Occurrences include `Rfc8259.StringText`, `Rfc9110.QuotedString`,
`Rfc9651.SfString`, and the quoted strings in Rfc6266 and Rfc8288.

The existing emitter has character tests, predictive choices and plain scanners;
this is not a claim that every character currently creates an arena entry.
The additional candidate is scanning an entire ordinary run up to a quote,
escape or invalid character, then handling the exceptional branch once.
Recognition must retain JSON control-character restrictions, Unicode escape
lengths, malformed escape positions and chunk boundaries. Decoding remains a
separate semantic operation. Throughput improvement has not been measured.

## Priority 4: literal terminators on buffered input

`(?!"*/" & any)* & "*/"` appears in the Gram and SQL read-only examples.
`Machine.Scan.EmitScanUntil` already emits `MemoryExtensions.IndexOf` for eligible
contiguous scanners. However, `ScannerOf` returns null for buffered input, and the
new delimiter optimization handles character tests rather than multi-character
literal terminators.

Extend the search plan to literal delimiters across buffer boundaries, retaining
partial matches. Consider a prefix-function search for long/self-overlapping
literals; do not assume repeated candidate matching is linear. Preserve the
first-match rule, backtracking and missing-terminator diagnostics.

## Already optimized or lower priority

- Character runs already have `RunTest`/`CompileRun` and Reader.EmitRun.
- Literal alternatives already have prefix tables and shared-prefix dispatch.
- Deterministic repetitions already have SilentRepeat and NeverGivesBack proofs.
- Separated lists such as `Atext+ & ('.' & Atext+)*` are candidates only where the
  existing deterministic paths still leave measurable overhead. Do not introduce
  another list engine without inspecting the actual emitted path.

Pattern recognition should supply a shared semantic search plan, with separate
emitters for contiguous and buffered sources. Reducing the size of the common
Return/Fail/materialization machinery is a different task: recognizing more text
patterns alone will not remove that infrastructure.
