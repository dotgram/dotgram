# Architecture decisions: generator performance

A log of decisions agreed for bringing generated parsers to the work a handwritten parser
does. Each entry says what was decided, why, and what it binds. Proposals, measurements
and rejected experiments live in their own reports under `docs/design/`; this file only
records the outcome and points at them.

## How a decision is made

Since 2026-09-17 a session named `architect` reviews, and does not write, changes to the
generator's architecture: the renderings (flat, reader, engine), the carriers, the arena
and materialization, what the normalizer proves, the shape of emitted code, and any deep
refactoring. A session proposing such a change sends it before implementing it, with the
problem and the measurement that shows it, the design, the alternatives considered, what
the change touches (other grammars, streaming, recovery, diagnostics, code size), how it
is measured against the hand parser, and how it is undone.

The architect's refusal stands until Igor overrides it. An experiment in scratch needs no
approval; landing it does.

## D1. A handwritten parser reads exactly the grammar it is measured against

Decided 2026-09-17 by Igor.

Every parser in `examples/DotGram.Handwritten` reads the language of the generated parser
it is compared with, as that grammar is implemented now: the same publications, the same
accepted and refused input, the same values (trees, fields) and locations. Error wording
may differ; the position of an error may not.

**Why.** A ratio against a parser that reads less, or reads something else, measures the
difference in language and not the generator. It also lets a generator change that costs
time on an unmeasured construct pass unnoticed.

**What it binds.**

- Conformance is checked by tests that run with the ordinary suite, not only by a
  benchmark's agreement step. A hand parser whose agreement is checked only before timing
  drifts as soon as the grammar grows.
- When a grammar gains a construct, its hand parser gains it in the same change, or the
  change says which comparison is suspended until it does.
- A timing is quoted only for inputs both parsers were shown to agree on in that build.

**State at the time of the decision** (`main` at `4a41ecc9`):

| Hand parser | Generated counterpart | Conforms | Checked by |
| --- | --- | --- | --- |
| `Fix.HandFixParser` | `FixParser` | yes, field model and locations | `HandFixTests` |
| `HandSqlStandard` | `SqlStandardParser`, all 42 publications | yes | `Both` in `DotGram.Sql.Tests`, fuzz corpora |
| `HandExpression` | `ExpressionParser` | no: no interpolated or raw strings; `ParseHole` not offered | `--el` agreement only, 169 shapes |
| `HandSqlTokens` | `Sql92Parser` | no: `SearchCondition` only of four publications; datetime literals not read | `--hand` agreement only, 42 shapes |
| `HandSqlOriginal` | none | no, by its own description | none |

**Decided 2026-09-17 by Igor: `HandSqlTokens` and `HandSqlOriginal` are removed.** SQL is measured
against `HandSqlStandard` alone. Carried out by sql-ff, which owns the SQL benchmarks: the two
files go, and so does every benchmark column and command that compares with them (`--hand`,
`--lexers`, the hand columns of `--bytes`, `--big` and `--spin`, `SqlComparisonBenchmarks`,
`SqlCounters`, `SqlSlope`). Each piece is either deleted or pointed at `HandSqlStandard` and
`SqlStandardParser`; none keeps a comparison with a parser that no longer exists. `docs/next.md`
keeps the ratios measured against them as history.

## D2. The value store clears what was written, not what every machine might write

Decided 2026-09-17 by the architect, on the diagnosis in `docs/next.md` ("Why the shared
machine is not dense", `4a41ecc9`).

SQL:2023's big machine is not dense (`_directBuilds`: forty-five tower guards name a built
value), its `Literal` and `TableName` machines are, and the machines share one
`DirectValues` store. So every materializer call raises 301 high-water marks and `Return`
clears 301 arrays: 20 to 27 per cent of a short select, building nothing.

**Approved:** the store records which types were written where a value is stored, and
`Return` clears only those. The towers stay; the big machine is not forced dense.
**Not approved as the fix:** one shared mark for all types. It makes `Room` O(1) and keeps
the 301 clears.

**Conditions.** Emitted output for grammars that are all dense or all non-dense does not
change, or the difference is shown to cost nothing. The write-site test is timed on a grammar
that stores many values per parse, not only on SQL. Agreement with `HandSqlStandard` holds.

**What it is not.** A fixed cost of about a fifth of a short statement. The per-operand rate,
7.27 us against 0.33 an item, is a different mechanism, and Q1 is where it is decided.

## D3. FIX stays on the engine; the next direction is one-pass construction

Decided 2026-09-17 by the architect, on performance-3f's profiles.

The reader refuses FIX because it recovers (`Machine.Direct.cs:122`). Moving it there
would mean the reader learning recovery and streaming, and nothing measured says that pays:
FIX spends 56 per cent (Order) and 73 per cent (BinaryMany) in recognize plus materialize,
and SQL, on the reader already, spends 53 to 62 per cent in materializing. The two-pass shape
costs on both renderings, so it is not fixed by choosing a rendering.

**Direction approved for design, not for code:** building a value where the recognizer has
committed to it, in regions where that is proved not to change what runs (`syntax.md` §7.3:
a construction runs once per node of the accepted derivation). FIX first.

**Conditions on the design.**

- The proof is one analysis, beside `Replay`, answering per rule or per site; every rendering
  that builds reads its answer. No rendering gets a proof of its own. The generator already
  writes a way in three times; a fourth kind of special case per rendering is refused.
- It says how a construction relates to `recover`: whether the unit that recovery throws away
  can hold an eager construction, and what happens to one already run.
- It says what is left of the tape in a mixed machine, and whether the existing carriers
  (Tape, Immediate, Mixed, Auto) remain four or become a per-rule choice of two.
- It keeps the factories' order and count, the typed caches and rollback invalidation, and
  string, TextReader and Stream input.

**Parked:** narrowing FIX's follow sets for `yield` (six Run records, correctness of the end of
a yield step not established) and removing Run/CaptureOpen records that depends on it. The
whole-field helper stays rejected.

## D4. Timings on the Ryzen machine are pinned

Recorded 2026-09-17 from performance-3f. The machine has two CCDs with the V-cache on one.
Unpinned paired runs varied up to twice batch to batch; pinned to logical processors 0-15 at
high priority, FIX Order's spread fell from 28 to 4 per cent. A timing that was not pinned is
not quoted as a comparison.

## D5. A stream is read without holding it

Decided 2026-09-17 by Igor: streaming exists to process volumes larger than memory, so a
`Stream` or `TextReader` is read through the parser's own buffering and never loaded whole.

**What it binds.**

- No path reaches a contiguous form by reading a stream to its end (`ReadToEnd`, copying into a
  `MemoryStream` or an array). A contiguous overload (Q2) is for data the caller already holds,
  never a shortcut for a stream.
- What a streamed `yield` holds is bounded by the record being read and what the grammar
  cannot yet let go of, not by the input read so far. Arena, value tables, capture buffers and
  input buffers all count. A limit that throws (`IOException` on capacity) is a bound; growth
  proportional to the input is a defect.
- The claim is tested, not argued: a streamed input several times larger than the process is
  allowed to hold, produced on the fly and never resident, read through `yield` with peak live
  memory asserted. The benchmark README says this scenario has not been measured; until it is,
  bounded streaming is a design intent, not a property.
- Any lexical mechanism chosen under Q1 reads tokens lazily over buffered input.

**Measured 2026-09-17 by performance-3f**, at `2f22d5fd`, FIX through `yield`, input made on the
fly under a 32 MB heap limit, live heap after forced collections every 250,000 fields: Stream
9 KB above a 263 KB floor at 16 MB, 256 MB and 1 GB of input (111 million fields); TextReader
29 KB at each size. Flat, and a 1 GB input completed at thirty-two times the heap limit. The
24 MB at 8,000 fields is total allocation of `Parse(string)` with a whole result, not live
memory, and not a D5 case. Scratch harness and results: performance-3f's `.work/streammem`.

**Approved as the standing test:** in `tests/DotGram.Tests`, over a small streaming grammar,
peak live memory at two generated sizes, the larger held to the smaller plus a constant, for
Stream, TextReader and `IEnumerable<string>`, with and without `yield`, and with a bad record
recovered every so often so that recovery is inside the bound. A FIX twin in
`tests/DotGram.Finance.Tests`, agreed with finance-03. The 1 GB heap-limited run stays out of the
suite until it has a place in `benchmarks/`.

**Ruling 2026-09-17, on a whole-result parse of a stream.** performance-3f's test found that a
buffered `parse` without `yield` keeps its whole input to the end: 4,092 bytes a record over
TextReader and 2,016 over Stream, for records of about 1,001 characters and a result of one
`int` each. `syntax.md` §6.3 describes this as the current, conservative release analysis.

Under D5 it is a defect. What a parse may hold grows with its **result**, which the caller asked
for, and not with its **input**: bounded working set plus the result. A root whose value is the
whole matched text holds the input because the input is its result, and that is allowed; a
root of one `int` a record is not. `maxRetained` turning the growth into an `IOException` is a
limit, not a release. The sentence in §6.3 goes when the defect is fixed, not before.

The cause is deferred construction: a capture is built after acceptance from the input it points
into, so the input cannot be let go first. D3's one-pass construction removes exactly that
dependency, so the whole-result memory test is one of D3's acceptance checks. The D3 proposal
compares it with the other way out, copying committed captures into owned storage at release
points: that keeps §7.3's deferral but holds captured text, which here is the whole record, so
it would not pass this test.

The lazy half of the test lands now. The whole-result half lands with its skip reason naming
this ruling, and the skip is removed by the change that fixes it.

**Buffers kept by the shared pool: confirmed.** Test `98d54101` landed as ruled. After one large
whole-result parse, a fresh process keeps about 47 MB (Stream) and 112 MB (TextReader) live when
idle, after a forced compacting collection, while the last input was about 2 MB. Attribution
is by elimination and size (the generated spare parser is not kept past 65,536 entries), not by
a heap dump. The pool trims on its own schedule, so collections do not release it.

**Approved as a change of its own, not folded into D3:** a buffer past a threshold is dropped
rather than returned to `ArrayPool.Shared`, the shape `KeptEntries` already has for the arena.
It stays needed after D3, since D5 allows one record to be large and its buffer would then be
kept the same way. Conditions: the threshold is justified by measurement; the idle figure above
falls to the floor, which also confirms the attribution; the time of small-record parses (FIX
One and Order, pinned) does not move; the retention test gains the idle check after a large
parse.

**For D7:** the test also showed the two mechanisms offering different forms. `IEnumerable<string>`
exists only on the legacy window mechanism, a `yield` publication gets no reader overload
there, and the buffered machines offer TextReader and Stream.
- A speed change to the buffered machines is measured with peak live memory beside time.

## D6. One SQL tree for every SQL parser, handwritten ones included

Decided 2026-09-17 by Igor. It extends what `design/sql-parsers.md` and
`design/sql-tsql-tree.md` decided on 2026-09-15 (T-SQL and `Sql92Parser` move onto the
SQL:2023 tree in `DotGram.Sql.Ast`) to the handwritten parsers.

Today there are two trees: `DotGram.Sql` (`SqlSyntax.cs`), built by `Sql92Parser`,
`TransactSqlParser` and `HandSqlTokens`; and `DotGram.Sql.Ast` (`Sql2023Ast.cs`), built by
`SqlStandardParser` and `HandSqlStandard`.

**Why it matters for performance.** With one tree, the generated and handwritten parsers
make the same allocations for the same text, so the ratio between them is recognition and
construction machinery and nothing else. It is also what D1 needs to hold across dialects.

**What it binds.**

- The public tree holds only the final nodes. A grammar's intermediate carriers (`Nodes.*`,
  `Towers.Typed`) are construction details of that grammar. A hand parser may share what is a
  property of the language, such as the towers' role bits, but not a generated parser's
  intermediate records. A hand parser that allocates the generator's intermediates would
  flatter the generator.
- When `Sql92Parser` and `TransactSqlParser` switch, a handwritten parser measured against
  them builds the new tree in the same change, or is retired in it (see D1's open question on
  `HandSqlTokens`).
- `Both` excludes `Span` from its comparison (`Both.cs:514`). D1 includes locations, so under a
  location-bearing reading both parsers are compared on spans too.
- The switch is measured for allocation per parse and for the number of value types a machine
  stores (D2's 301 grows with the tree), beside `--engine` and `--roundtrip`.

## D7. The input form is a dimension the generator compiles for

Decided 2026-09-17 by Igor: a string, an array of strings, a stream of strings and a stream
of bytes are each a form the generator uses as such, not an adapter onto another form.

This confirms the decision in `design/reader-input-forms.md` (a compile-time input form:
symbol domain, access form, result delivery as independent dimensions, with an emitter-side
abstraction and no runtime interface per symbol) and makes it binding.

**Adapters in place today, each a debt under D7:**

- `IEnumerable<string>` puts the line terminators back and forwards to the `TextReader` path
  (`status.md`, "And from a sequence of lines").
- `FixParser.Parse(byte[])` wraps a `MemoryStream` and runs the buffered byte iterator.
- `FixParser.Parse(ReadOnlySpan<char>)` copies to a string (Q2).

**What it binds.**

- One input abstraction in the emitter: each form supplies the operations recognition needs
  (the `Peek`/`Get`/`Position`/`Restore`/`ReleaseBefore`/`Copy` set of
  `reader-input-forms.md`, and plain indexing for contiguous memory), and every rendering is
  written against it. Recognition and construction are not written again per form.
- A form does not choose the rendering. Today the reader refuses any streamed publication,
  so a stream sends a grammar to the engine; that is where D7 is not yet met, and it is the
  measure of progress on it.
- Two streaming mechanisms exist: `StreamingEmitter`'s window-and-retry wrappers around span
  recognizers, and the buffered machines. D7 converges them into one; a proposal names which
  survives and how the other's cases (retention analysis, stages, `find` over a reader) are
  carried over.
- D5 applies to every streamed form.
- A parse answers the same whichever form the input came in by; a test holds each form to
  the string form.

**Open, for Igor.** Whether an array of strings is lines (a terminator between elements, as
`IEnumerable<string>` is today) or pieces of one text joined as they are; and whether
contiguous bytes (`byte[]`, `ReadOnlyMemory<byte>`) and `ReadOnlySpan<char>` are forms of
their own or the memory side of the byte and string forms.

## Open questions

### Q1. SQL:2023 through a lexical layer

Raised 2026-09-17. Owner: sql-ff, with performance-3f for the generator.

`SqlStandardParser` is `[Gram("SqlStandard.gram")]`, compiled over characters; `Sql92Parser`
and `TransactSqlParser` are `Lexical = true`. `HandSqlStandard` reads through a token cursor
and classifies a word as an integer once (`SqlCursor`, `SqlWords`). Every per-token cost the
SQL timings name — the reserved-word bucket walk behind a first-letter switch, a word costing
1.0 us more than a literal even in an empty bucket, trivia at ten times the hand skip loop per
character, 3.6 us a token overall — is the cost of reading tokens as characters.

The reason recorded for not splitting (`docs/design/sql-parsers.md`): tokens overlap, a date
string is a character string whose meaning depends on the key word before it, and a choice
over characters can go back where one over tokens cannot. The same note says it would be
asked again when the grammar is whole and speed matters. Both are now true.

Before any further per-token optimization of the character reading lands, the answer to Q1
is required:

1. Which constructs of the standard cannot be read over kinds, listed from the grammar, with
   what the hand parser does for each. The hand parser already reads the whole language
   through tokens, which is an existence proof for most of them.
2. Whether those can be expressed with what the split already has (a kind as a set of
   patterns, a valued terminal read twice, a terminal the host measures), or need a new
   mechanism.
3. If a new mechanism: whether it is a lazy token cursor (tokens made as the reader asks, as
   `SqlCursor` does) rather than a lexer over the whole input first. That choice would also
   bear on streamed input, where the engine now reads characters (FIX).

**How large it is.** performance-3f's profile (SELECT a FROM t and twenty items): reading
reserved words is about 8 per cent, and the letters family puts bucket crowding at 7 to 20 per
cent of a line depending on the initial. So the character reading of words is a real cost
but the smaller part; Q1 is required before per-token work so that nothing is built twice
over characters and again over kinds, not because it is where most of the time goes.

**A constraint from FIX.** A lexer over the whole input first would break `stream bytes` and
lazy `yield`, which exist so that a long input is never held whole. Whatever mechanism Q1
chooses, a lazy cursor over buffered input is a supported form.

Q1 and D3 are not rivals. performance-3f attributes 53 to 62 per cent of SQL's time to
materializing, of which D2's store bookkeeping is a large part; the rest of a parse is the
character reading. Each report gives time exclusive of the factories both parsers call, so
that the two causes are sized against each other and not against the work the hand parser
also does.

### Q2. Contiguous input: characters and bytes

Raised 2026-09-17 by performance-3f. Language and public API: Igor's decision first, then
the architect's review.

`FixParser.Parse(byte[])` wraps a `MemoryStream` and runs the buffered iterator; the Bytes
form has the worst ratio to the hand parser in every FIX workload. Proposed: `stream bytes`
also offers a `ReadOnlyMemory<byte>` overload, fed to the existing buffered-byte machine.
The review will ask how much of the Bytes gap is the adapter and copy, which this removes,
and how much is the buffered machine's own checks, which it keeps; the Text form, read from a
contiguous span, is the measure of the second.

**Characters too, and the language already promises them.** Raised 2026-09-17 by finance-03:
`FixParser.Parse(ReadOnlySpan<char>)` copies the input to a string and calls the string form,
and `HandFixParser` does the same. `syntax.md` §6.3 lists `ReadOnlySpan<char>` beside
`string` as an input the generated overloads take; no emitted parser offers it. So the
character half of Q2 is completing the specification, not new API, and is designed with the
byte half as one question: a span for a whole-result `parse`, a memory for anything that
outlives the call (`yield`, a lazy context). Owner: performance-3f.

Until then finance-03 documents the copy on both span overloads, as `FixMessages.Parse`
already does (approved: comments only, no signature or emitted code changes). Removing the
overloads was refused: it would be a breaking change out and another one back.
