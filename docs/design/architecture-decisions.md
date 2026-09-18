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

**The architect does not measure or write, Igor 2026-09-18.** Its work is plan, delegate, review,
reconcile and decide. Everything about the measuring stand — `Stand.cs`, baseline runs, the
before/after runs a session asks for — belongs to the `stand` session.

**Grammars and the language, Igor 2026-09-17.** The grammar of a concrete parser (SQL, EL,
FIX, Web and the rest) may be improved by the session that owns it without asking: that is
work on a parser, not on the generator. A change to the language itself (`syntax.md`: its
notation, what a construct means, what hooks can see) is discussed with Igor first, before a
design is written, let alone code.

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
| `HandExpression` | `ExpressionParser` | yes, since `85372f80` (expr-2d) | `Both` over 242 call sites of `ExpressionParserTests`, `ExpressionHandTests` over 239 shapes and 14,332 mutations |
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

**Landed 2026-09-17 as `7ed58baa` (sql-ff).** The mark goes up beside the write in record-indexed
machines; `Room` loses its loop; `Return` is unchanged. Only `SqlStandardParser` has
record-indexed machines, so only it gains marks (+76 KB of 13.9 MB); three dense-only grammars lose
a loop that was dead for them; every other grammar is byte-identical. Pinned, paired, both
orders: twenty select items 327 to 220 ms (-32.7%, second run -31.8%), `SELECT a FROM t` -37%, a
literal (dense, the control) within 1%. Twenty items against the hand parser: 26.7x to 17.8x.
Condition 2 is met by construction: the write-site comparison runs only in SQL:2023's machines,
where it was measured.

**Owed:** the change added no test. A missed mark leaves a table uncleared, which is references
kept across parses. A generator test over a small grammar with both dense and record-indexed
machines checks that nothing a finished parse built stays reachable from the store (a weak
reference to a built value, collected after the parse and a collection).

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
- It builds only what is demanded. A value the grammar reads and discards is never built:
  the expression language's `Held` reads a lambda body only to find its end and keeps a span,
  and the body is built later, once its parameters have types. The tape honours this because
  its walk builds on demand; the Immediate carrier does not, and builds the body early,
  where a construction throws (found by expr-2d, 2026-09-17:
  `using System.Linq; (int[] a) => a.Select(n => n * 2)` reads on the tape and throws
  immediately). That is a defect of the Immediate carrier today, and the rule D3 must keep.
  **Fixed 2026-09-18 (performance-3f, up to `610906e3`)**: `Grammar/Model/Demand.cs` answers, per
  call site, whether the tape would build its value — never, with its parent, or during the
  match for a guard — and Immediate builds by that answer. `Demand.Of` is the analysis D3 reads.
  Auto hands no grammar in the repository to Immediate with an unbuilt site; only the expression
  language's explicit Immediate reading changed, and its skipped test is skipped no more.

**Parked:** narrowing FIX's follow sets for `yield` (six Run records, correctness of the end of
a yield step not established) and removing Run/CaptureOpen records that depends on it. The
whole-field helper stays rejected.

## D4. Timings on the Ryzen machine are pinned

Recorded 2026-09-17 from performance-3f. The machine has two CCDs with the V-cache on one.
Unpinned paired runs varied up to twice batch to batch; pinned to logical processors 0-15 at
high priority, FIX Order's spread fell from 28 to 4 per cent. A timing that was not pinned is
not quoted as a comparison.

**A timing window starts after everyone has stopped, not when it is announced.** 2026-09-17, the
first window of expr-2d's `--el` ran over two other sessions' builds that were already running
when the notice arrived (one until 23:53, one until 23:54). The session that measures announces the
window, waits until every other session has answered that nothing of its own is running, and
only then starts; a notice received mid-build is answered with when the build will end.

**A before/after comparison is paired, in one process.** `--stand-paired` (stand, `f39b139f`,
2026-09-18) loads both builds into two `AssemblyLoadContext`s and alternates hand, before and
after in every round, so that what moved the machine moved both sides. Two separate processes
had shown +10-13% where the paired run showed +4% steady, and once the cause was fixed, noise;
the paired form is what a before/after is quoted from. Over SQL it compares acceptance and not
trees, since types from two contexts are never equal.

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

**Threshold, 2026-09-17.** Not returning buffers costs time at every size, because the buffer
grows by doubling from 4,096 and every step past 85,000 bytes is a large-object allocation: one
FIX record through `Parse(Stream)`, pinned, kept against dropped, 4 KB +24%, 64 KB 6.9x, 1 MB
3.2x, 4 MB +62%. FIX One and Order never leave the first 4,096 buffer, so any threshold of 8K
or more leaves them alone. **Approved:** 1,048,576 elements, the bound the generated token and
value stores already use, justified by this table where it is written. **Approved in principle
as a follow-up proposal:** the next parse on a thread rents at the capacity the last one needed,
remembered as one integer, which removes the doubling ladder that is most of both the time and
the idle retention.

The idle figure is explained: measured against a reading taken before the first parse, a parse
with no pool retention holds 4 KB after it, at 1 MB and 4 MB records alike; the rest was the
harness's own input. Returned to the pool, 4 MB records leave 16,385 KB, the doubling ladder.
The idle check runs live, not skipped (it is not blocked by the whole-result defect), with a
slack of the pool's legitimate keep, about two thresholds of elements, and it runs where no
other test shares the process's heap at the same time.

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

**What D7 is about, Igor 2026-09-17.** The forms are already offered, and what matters is that
they are the API the user is given: the caller decides which one to read by, by the type of
what it passes (`syntax.md` §6.3), and some forms are declared by the grammar (`stream`,
`stream bytes` on a publication). How a form is served inside is the generator's business. So
the adapters below are performance debts, paid where a measurement shows the form costs more
than the one it is adapted onto (FIX bytes against FIX text), not a mandate to rewrite every
form natively; and the conditions below bind the generator's inside, not the API.

**Adapters in place today:**

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

**Decided 2026-09-17 by Igor: an array of strings is lines.** The elements are one text with a
line break between them, which is what a feed is, as `IEnumerable<string>` reads today. The
break is `\n`, the one `eol` matches (`status.md`, "And from a sequence of lines"). As a form of
its own it reads the break where an element ends, without copying the elements into a
`TextReader` stream; an element boundary is also a point a feed can release input at.

**Decided 2026-09-17 by Igor: `ReadOnlySpan<char>` is a variant of the text form**, not a form
of its own and nothing to do with bytes: a caller who holds text in a span reads it without
making a string first. A convenience users will likely want rather than a performance
requirement, so it follows the forms above in priority; when it comes it reads the span, not
a copy of it.

**Decided 2026-09-17 by Igor: `byte[]` is a variant of the byte form**, as the span is of the text
form: bytes the caller already holds are read where they lie, not wrapped in a `MemoryStream`.
That retires the `FixParser.Parse(byte[])` adapter above once the variant exists.

**How, decided 2026-09-18 by Igor.** The generator offers `byte[]` (and `ReadOnlyMemory<byte>`) for
every publication with the byte form, not only for FIX. It is served by the existing buffered byte
machine over a source already in memory, which hands over the whole array at once and then
reports its end: no machine of its own, no copy, no `MemoryStream`, and next to no emitted code.
What remains of the gap between bytes and text afterwards is measured; a contiguous byte machine
is weighed against its code size only if that remainder is large. `ReadOnlySequence<byte>` and
`PipeReader`, the networking forms, are candidates for later on the same machine.
**Landed 2026-09-18 as `737376e5` (performance-3f)**: a `BufferedBytes` constructor over memory, one
fill that is the whole input, no copy, no buffer parameters; only FixGrammar and Fix44 differ.
`96d86795` makes the lazy buffered overloads plain methods that validate at the call and call a
private iterator over `int`s, so D9's `int?` costs no allocation (the stand had found +16 bytes a
call). Found by the compatibility build with warnings as errors: an emitted CS0649, which a
consumer building strictly would have failed on; the verification now stops at the first failed
build, since a stale assembly once let tests "pass".

**The remainder, and no separate byte machine (finance-03's report, 2026-09-18).** With bytes
read in place, the generated FIX parser still reads bytes 12-20% slower than text, the hand
parser reads both alike. The remainder is in the buffered rendering, not the input: the scalar
guard is off for buffered input (`Machine.cs`, `!BufferedInput &&`, decided on the stream form
before bytes were in place) and costs 40-45% of the gap, measured with the line removed; text
conversion allocated twice (fixed, `e7d17ceb`); what is left, some 3-6%, is a bounds check through
the buffer object per character and the missing `Scan_*` helpers on the buffered side. A
contiguous byte machine would be a third copy of the recognizer, about +39% source in Finance, for
those last percent. **Decided: no separate byte machine.** The scalar guard for buffered input is
performance-3f's next small proposal, with the stream rows measured before it lands; the rest
comes with Q7.5, where "in memory, never refills" becomes a constant the JIT folds.

**Several forms per parser, and feeds read by line — Igor, 2026-09-17.** A parser offers whichever
forms its grammar asks for, several at once: FIX needs the byte form, and the same parser must
also read a string in memory and a text stream. For a feed, streaming may read the stream a
line at a time, up to `eol`.

What the architect adds to reading by line:

- A line is read by the parser's own buffering and keeps its terminator. `TextReader.ReadLine`
  is not used: it drops `\r\n` and `\n` alike and cannot say whether the last line had one, so
  positions in diagnostics and locations would drift by a character a line on a CRLF file, and
  a grammar that tells the two apart would read something else than was sent (§0: diagnostics
  before speed).
- A line boundary is a release point: what a feed holds is the record being read, which is
  the D5 bound.
- A record that spans lines still reads, since the lines are one text; a single long line is
  held whole, which D5 allows.
- Reading by line is a strategy of the text-stream form for grammars whose records end at `eol`,
  chosen by the generator; the form's answers do not change with it.

## D8. The expression language's hand parser, from expr-2d's inventory

Decided 2026-09-17 by the architect, on expr-2d's inventory of `HandExpression` against
`ExpressionParser` (thirteen grammar commits since the hand parser last changed).

- **A grammar defect is fixed in the grammar first, not copied into the hand parser.** The
  grammar accepts key words where it writes `Word` without `?!Keyword` (`(int if) => 1`,
  `{ var if = 1; x }`, `nameof(if)`, member names after a dot, a foreach variable), which C#
  refuses. expr-2d owns the grammar; the fix (an identifier rule refusing key words at the
  declaration and member sites, keeping `@if` if the grammar reads verbatim identifiers) lands
  before the hand parser is brought up to it. D1 then holds against the corrected grammar.
- **What the hand parser gets wrong is fixed in the hand parser**: escapes, character literals,
  number forms, building on paths the grammar abandons, the order of calls into `State`.
- **Error positions follow the generated parser as it stands**, including two consequences of
  its implementation rather than of the language: a lexer error anywhere in the input wins,
  because the whole input is tokenized first, and a failure counts the positions lookaheads
  reached. Both are recorded as such. If Q1 or D7 makes tokenizing lazy, which D5 requires for
  streams, the first changes, and which position is right is then decided in `syntax.md`, not
  by whichever parser is written second.
- **One corpus.** `ExpressionAgainst`'s corpus moves to `examples/DotGram.Handwritten` as plain
  data, read by the benchmark and by the conformance test alike.
- **The conformance test** compares outcome, position on failure, `State` refusals, exception
  type and the tree by its debug view, over `ExpressionParserTests`, the shared corpus and a
  mutated corpus. It holds the hand parser to the tape, which is what ships.
- The Immediate carrier's defect (D3) goes to the generator's owner; until it is fixed no
  Immediate figure is quoted for untyped lambdas. `ExpressionCarrierTests` gains untyped
  lambdas, interpolated and raw strings, the Immediate half skipped with a reason naming it.

**Done 2026-09-18 (expr-2d).** The hand parser conforms (`85372f80`) and allocates less than the
Immediate reading on every input but one (`b36599bc`). Pinned, over the whole language
(`f88e3216`): common rows tape/hand 1.36-2.68x, immediate/hand 0.92-1.69x; interpolated and raw
strings tape/hand 2.91-3.62x, immediate/hand 2.16-2.67x, the cost being a publication entered
again for every hole (a spare per nesting depth, performance-3f); untyped lambdas about 1.04x,
both parsers spending 50-66 us in overload resolution and inference, which is the host's.

## D9. A parser's retention limit: set by its grammar, overridable by whoever uses it

Decided 2026-09-18 by Igor. The limit on what a streamed parse may retain (`maxRetained`), and
likewise the initial `bufferSize`, is an option of the generated parser, not a constant written
beside it in a package.

- **The grammar sets the default**, as an option on its attribute (`[Gram(…, MaxRetained = …)]`,
  and `BufferSize`). FIX sets 16 MiB in `FixGrammar`, the same as `FixMessages`' message limit.
- **The user of a shipped parser changes it per call**, without rebuilding the package: the
  parameter below. The default is exposed read-only (FIX: `FixParser.DefaultMaxRetained`). No
  settable static: it would be one value for the whole process, which two libraries using the
  same package, or parallel tests, would overwrite for each other (finance-03's objection,
  Igor's choice the same day). An application-wide setting (`AppContext`) is added only if a
  case appears where the call site is out of the user's reach.
- **Per call it stays a parameter**, now `int? maxRetained = null`: `null` means the parser's
  current default, a number overrides it for that call. A caller passing a number compiles as
  before.
- What a grammar gets when its attribute says nothing is decided with D7 (a finite default or
  `int.MaxValue`).

Generator: performance-3f. FIX: finance-03, instead of a constant of its own.

## D10. Faster code on newer targets, and unsafe code, by the user's choice

Decided 2026-09-18 by Igor. A feature that makes generated code faster is not given up because
the default floor cannot have it. Generating on `ref struct` type arguments (`allows ref struct`,
C# 13 on .NET 9 and later), or with `unsafe` code over `char*` and `byte*`, is offered as an
option the user turns on; it is never the default, which stays C# 8 on netstandard2.0 and net472.

**What it binds.**

- The default rendering is the reference: an opt-in rendering answers exactly as it does, and the
  tests compile and run the grammars both ways.
- An option is added only with a measured gain on a shipping grammar, and names what it
  requires of the consumer (target framework, `AllowUnsafeBlocks`); a build that cannot have it
  says so as a diagnostic rather than failing in the compiler.
- It fits Q3's rule, not against it: Q3 removed options that chose between renderings nobody
  needed; these choose what the consumer's build allows.

## D11. A diagnostic that says the grammar reads otherwise than written is a warning

Decided 2026-09-18 by the architect, on sql-ff's finding.

`GRAM5009` says that, over kinds, an optional or a repetition takes what the part after it
needed, so the split reads something other than the grammar as written (`FirstSets.Committed`).
It was Info, which a build does not print, and it had fired eight times on parsers already
shipped over kinds — seven in T-SQL, one in SQL-92 — without anyone knowing, and six more on
SQL:2023 in flight. A diagnostic about the language a parser accepts cannot be one nobody sees.

- `GRAM5009` becomes a Warning. The other Info diagnostics in `Grammar/` are reviewed by the
  same rule: what tells the author about a change in what is read is a Warning; what reports a
  choice the generator made (carrier, rendering) stays Info.
- Order: the fourteen places are fixed in the grammars first (SQL:2023 by sql-ff before its
  split lands; T-SQL and SQL-92 by sql-ff as well, their owner, each fix with a test of the
  input the grammar meant), then performance-3f raises the severity, so that
  `TreatWarningsAsErrors` never meets it red.
- `docs/development.md` says how to see Info diagnostics (`-v:detailed`).

**`Committed` refined (`610906e3`)**: the cures the diagnostic asks for are recognized when an author
writes them — `?=` on what follows at the end of an optional or repetition, also through a call
or a choice; a `?!` of more than one token at the start of what follows, by a second-token
analysis limited to that form; and atomic braces, as before. Repetitions the analysis does not
see (`*`, sql-ff's `UNIQUE (a, p WITHOUT OVERLAPS)`) are the next refinement.

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

**Answered 2026-09-18 by sql-ff** (`docs/design/sql-over-kinds.md`, `030e882a`): nothing in the
standard is unreadable over kinds, since `HandSqlStandard` reads all of it through a token cursor.
Three things the token layer must allow: a literal of several quoted runs with separators between
them, and a token whose body is narrower than itself (`UESCAPE`), both covered by a terminal the
lexer begins and a rule finishes; and a key word glued on its left (`198OCTETS`), which needs
`wordboundary` read as a guard on both sides, a change to the language. Mechanism: a lazy cursor,
with a lexer over the whole input as an additional form for contiguous input. sql-ff's estimate of
the gain: about a tenth of what is left.

**The architect's review.** The estimate counts the word layer's own costs (bucket crowding,
trivia), not what reading over kinds does to the machine: over kinds a rule's answer stands, so
there is no tape of ways and no replay, and each choice is a switch on one token. On SQL-92 the
split measured 1.05-2.45x on accepted conditions and 1.35-3.85x on refused ones, far above a
tenth. And D5 does not stand in the way now: SQL:2023 publishes no stream, so the existing split,
a lexer over the whole input, serves its string input today; the lazy cursor is needed when a
split grammar streams. It also changes Q7.1: over kinds, `Replay`'s causes and the shared
beginnings are other ones. So, before Q7.1's design: SQL:2023 compiled with `Lexical = true` in
scratch, the three cases handled minimally or kept out of the measured corpora, timed against the
character reading and the hand parser. Its result decides the order.

**Measured 2026-09-18 (sql-ff, stand; `036d247e`).** SQL:2023 compiled over kinds against the same
grammar over characters, pinned, tiering off, accepted and refused apart: queries -23% / -36%,
DDL -36% / -46%, twenty select items -25%, value expressions -72%; generated code 6.77 MB against
13.86; generation 13% faster. The one loss is a refusal at the first token: 905 ns over kinds
against 227, the floor a lexer over the whole input pays on very short refused input. Of eight
obstacles to the split, seven were guards the character automaton needed and one is the language
(nesting comments), served by a terminal the lexer begins. **Decided by the architect: SQL:2023
ships over kinds.** sql-ff carries it out, with Q7.1 designed over kinds afterwards, under:

- No divergence from `HandSqlStandard` (D1): every corpus and `Both` agree before the switch lands.
- A reserved word is a token kind, not a guard: `?!ReservedWord` placed where the split already
  reads a lookahead as a range test over kinds (the syntactic rule), never a `when` after an atomic
  identifier, which fixes the first reading and cannot be taken back (271 DDL lines broke that way).
- Nesting comments and the interval string's body are read by a terminal the lexer begins and a rule
  finishes; nothing is left unchecked.
- The glued key word (`198OCTETS`, `2K`) is a question about the language for Igor: `wordboundary`
  guarding both sides, or the split reading a number glued to a word as the standard does not.
  Until answered the grammar refuses what the standard refuses by whatever the notation has, and
  says how.
- The stand's first-call row for SQL is quoted before and after: half the code should show there.

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

### Q3. Options and modes left behind by experiments

**Decided 2026-09-17 by Igor: everything listed under "Remove" and "Beside the options" goes.**
Who and when:

- performance-3f, after the pool threshold lands: `GramCarrier.Mixed` with `Machine.Mixed.cs` and
  `MixedSql`; `PrefixTables = false`; `Direct` out of the user's attribute; the experiment
  benchmark projects and classes and their dated reports.
- sql-ff, with or right after D2, since it changes the same store: `ValueStorage` out of the
  user's attribute, `Paged` and its code, `ValueStorageBenchmarks`.
- finance-03: `Direct = false` out of `FixGrammar` and the Fix44 example.
- performance-3f, with D7's convergence of the two streaming mechanisms: `BufferedInput` and
  `BufferedBytes` as host options.
- `PartSize` and the internal `SourceFileSize`, `SharedTypes`, `Own`, `Inherits` are measured
  first and brought back to the architect.

**Done 2026-09-18:** Mixed and `MixedSql` (`03445198`), Shapes (`fd054645`), the experiment
benchmarks (`ceda9b7a`), `ValueStorage`/`Paged` (`2af82772`, sql-ff), `Direct = false` out of the
FIX grammars (finance-03), `PrefixTables` (`5c6d1bc6`) and `Direct` (`0f3ef41a`) out of the
attribute with an internal lever kept for tests. Emitted parsers byte-identical in every case.
Left: `BufferedInput`/`BufferedBytes` with D7, and the report on `PartSize` and the internals.

**The spare stack by nesting depth landed as `fdea50f1`**: a lazy stack of at most four spares a
pool, bounded by the existing thresholds, depth three by measurement. Paired: EL interpolation
-36% time, -70% allocation, tape/hand 4.25x to 2.71x; untyped lambdas -14%; SQL and FIX
byte-identical.

Raised 2026-09-17 by Igor: the experiments have left options and modes of the generator, some
of which may no longer earn their place; those go. The architect's inventory, by what an
option is for. Usage counts are `[Gram]`/`[GramOptions]` sites in `src`, `examples`, `tests`,
`benchmarks`.

**Keep: they are API a user chooses by, and each changes what the user gets.**
`Suffix` (a second reading), `LocationType`, `Portable`, `Stacks`, `SpanCaptures` (changes the
type a factory receives), `Lexical` (a request, and over kinds a rule's answer stands, which
changes what the grammar means), `Carrier = Tape | Immediate | Auto` (Immediate is the
author's statement that the factories are pure).

**Remove, recommended.**

1. `GramCarrier.Mixed` and `Machine.Mixed.cs` (about 1,170 lines). Measured slower than the
   tape on SQL and recorded as not to be expected to beat a log on a grammar whose rules are
   mostly on cycles. Used by one benchmark (`MixedSql`) and nothing else. D3's design replaces
   the question it asked.
2. `PrefixTables = false` ("use the previous strategy"). Used by tests only. The fallback to
   the ordinary alternative chain inside the prefix tables stays; the switch back to the old
   emission goes.
3. `ValueStorage` as an option: `Flat`, `Adaptive`, `Paged` as author choices. Used by tests and
   `ValueStorageBenchmarks` only; `Auto` already chooses (dense, adaptive). `Paged`, which `Auto`
   never chooses, goes with its code; the rest becomes the generator's choice, not API. After
   D2 lands, since D2 changes the same store.
4. `Direct = false` in `FixGrammar` and the Fix44 example: moot, since the reader refuses a
   recovering grammar anyway (performance-3f). The option stays only if a test of the engine
   needs it, and then it is not offered in the user's attribute.
5. `BufferedInput` / `BufferedBytes` as host options beside `stream` / `stream bytes` on a
   publication: two ways to ask for one thing. Under D7 some forms are declared by the grammar,
   so the publication syntax stays and the host options go. With D7's convergence of the two
   streaming mechanisms, not before.

**Open, measured before deciding.** `PartSize` (a wish; measured flat from 60 to 250, set only
by the Fix44 example at 1,000); the compiler's internal `SourceFileSize` (file split),
`SharedTypes`, `Own`, `Inherits`.

**Beside the options.** Benchmark projects and classes that exist only for a rejected or
finished experiment (`CompilationSplitExperiment`, `DotGram.HandDeferred`,
`DotGram.PrefixBenchmarks`, `MixedSql`, `ValueStorageBenchmarks`, `DeferredShape`), and the dated
reports in `docs/design` whose subject was removed, go with it; `docs/next.md` keeps the
history.

Each removal is one change, by the owner of the area (performance-3f for the generator, the
option's grammar owner for its use), and shows that emitted code for the shipping grammars
(SQL, EL, Web, FIX) is unchanged or explains each difference.

### Q4. What else in the generator can be simpler

Raised 2026-09-17 by Igor. Three candidates go to measurement first, confirmed by Igor the
same day; each comes back to the architect as a report, with no code changed.

1. **The machine that builds a token's value.** Over kinds a terminal with a value is read a
   second time by a `_Value` machine; in the expression language's generated code (snapshot of
   2026-09-14) that machine is on the engine, seventeen parts. It cannot fail, since the lexer
   has accepted the extent. Measure its share of parse time and of code size in EL, T-SQL and
   SQL-92; find why it is on the engine; say whether it can be a reader or be built in the
   lexer. performance-3f.
2. **A second reading only for locations.** `TransactSqlParser.Located` is a whole second parser
   (about 33 MB beside 32 MB, same snapshot) so that a caller who wants no locations does not
   pay about 14 per cent. Prototype one reading generic over a struct parameter that says
   whether locations are offered, which the JIT specializes; measure time without locations,
   time with, JIT and first-call cost when both are used, and the build for netstandard2.0.
   sql-ff.
3. **Flat against reader.** Flat writes a publication as one method of states; reader writes a
   method per rule; flat recompiles the machine to do it, which is a known source of defects.
   Measure both on the Web grammars and the examples. No difference: flat goes. Flat faster: the
   reader takes over what makes it so, and flat goes. performance-3f.

The other candidates are conditions on work already decided: the engine's special paths
(scalar scanners and guards, sites, bounded materializer scans) are reviewed against D3's
analysis once it exists and removed where it covers them; `Auto`'s second rendering goes when
D3's analysis decides the carrier; the value store's remaining modes are revisited after D3;
the legacy `Parse(TextReader)` overload and the diagnostics about removed choices go with D7
and Q3.

### Q5. State that holds while something is read, and that guards read

Raised 2026-09-17 from expr-2d's audit of the expression language's `State`. A change to the
language, so discussed with Igor before any design. It has a twin in the other direction:
SQL:2023's tower guards need a cheap value that rises from a rule while it is read (the roles),
and can only have it by building the node beside it; recognition-time values that guards read,
inherited (this) and synthesized (that), are one subject for that discussion.

The expression language keeps, in its §7.7 `context`, pairs that open before a body and close
after it: lambdas (`Entering`/`Leaves`), loops and breakables (`Opening`/`Breaking`), and
variables whose type is not yet settled (`Awaits`/`Settles`). A reading abandoned between the
open and the close leaves an open extent, and every later position is attributed to it: a
`return`, `break` or `continue` after it can go to the wrong target, and a `var` after it can
read as unsettled (`object`) instead of inferred. The tape is exposed wherever the grammar
abandons a lambda, loop or switch after its opening guard. No minimal reproduction yet.

`syntax.md` says why: §7.7's context is for what can be written more than once without harm,
and nothing in it unwinds; "this holds while that is being read" is §7.8's mark, which is
abandoned with an abandoned reading. But a §7.8 mark is read by constructions after the parse,
and these pairs are read by guards during it: which alternative reads a `var` depends on them.
The language has no state that both holds while something is read and is visible to a `when`.

Options:

1. **Marks readable by guards.** A `when` may name `parserState`, the marks standing over the
   place it runs. The generator already keeps marks in order and drops an abandoned reading's
   (`StateSet`/`StateEnd` on the engine); the reader keeps them on a stack its calls unwind.
   Nothing is undone by the host. The architect's recommendation.
2. **The host undoes.** The generated parser calls the context's mark and rollback at every
   way back. Couples every choice point to a host interface and costs on every backtrack.
3. **Keep the context and make each entry answer for itself**, by position. Not sound: after a
   reading is abandoned the parse can move past the stale opening without opening again.

**Most of it needs no change to the language — Igor agreed 2026-09-17.** `break`, `continue` and
`return` find their target in `=>`, after the parse, where §7.8's marks are already visible and
only the accepted reading's marks stand. So loops, breakables and lambdas move from the
context's open/close pairs to `with state` marks, as an improvement of the expression
language's grammar (expr-2d). What is left for the language discussion is only what a guard
must see while reading: whether a `var` is read as unsettled. The notation offered for that
discussion: marks visible to `when` (downward). A value carried up while reading, for SQL's
towers, was withdrawn the same day: Igor recalled that whatever a `when` asks for may be built
while reading, which is the author's choice; the open problem is reusing what was built (Q6).

**Correction the same day (expr-2d).** A mark's value is a constant of its site: the arena keeps the
site number and the value is written as `site == 0 ? … : …`, with no span, capture or context. So a
mark says "inside a loop" but not which loop; two loops side by side are one `[Loop]` to a `break`
in each. The jumps cannot move onto today's marks after all; the pairs stay until the language
decides. Options, for the discussion with Igor:

- (a) A mark's value may name `parserSpan` and is computed where the mark is placed; the arena
  keeps the value.
- (b) Values stay constants, and a construction is handed where each mark standing over it was
  placed: `parserMarks`, positions parallel to `parserState`. Narrower, and the key a jump needs.
  Recommended by expr-2d and by the architect.

Together with marks visible to `when` (above), (b) covers the whole of Q5.

**Decided 2026-09-18 by Igor: (b).** A construction is handed, beside `parserState`, where each mark
standing over it was placed (`parserMarks`); mark values stay constants of their site. The
expression language's jumps then move from the context's open/close pairs onto marks. Whether
guards see marks while reading (for the unsettled `var`) was not part of the decision and stays
open; until it is decided, `Awaits`/`Settles` stay in the context, where the defect is latent.

**The defect is latent (expr-2d, 2026-09-17).** After every successful tape parse over the corpus
and all its mutations, no open/close pair was left open; every abandoned reading that could be
constructed makes the whole parse fail, and overload resolution catches nothing while building a
candidate's lambda. It shows only if the grammar gains an alternative that succeeds after an
abandoned opening. Q5 is therefore not urgent, and stays a question about the language.

**Landed 2026-09-18 (expr-2d, `c4a79af9`..`f81ad6d2`).** `parserMarks` in the generator with tests over
the three carriers and both symbol domains; §7.8 says what it is; the expression language's jumps
go by marks and its context loses the open/close pairs (the unsettled `var` stays); the hand parser
in the same change. Every grammar but EL emits byte for byte what it did; EL emits 2-3% less.
`GRAM4029` refuses `with state` or a hook naming `parserState`/`parserMarks` where no `state` is
declared, which used to surface as CS0103 in the consumer.

Until decided, the expression language's hand parser uses a checkpoint of its own `State` for
its second, recognize-only reading (a mark at the start of the publication, or of a hole or
body window), which is local to it and changes nothing in the generator.

### Q6. Reusing what a guard built

Raised 2026-09-17 by Igor. Building what a `when` asks for while reading is settled: it is the
author's choice. The problem is that what is built is not reused.

Today, on the reader: a value a guard built is flagged on its record (`values.Built`) and the
walk at the end reuses it, so within the accepted derivation nothing is built twice. But a way
back lowers the watermark (`ways.Built`), because a record's index is its name and the next
derivation writes other records at the same indices; everything built in the abandoned reading
is dropped, and when the grammar reads the same rule at the same place again it builds it again.
That is SQL's case: a `(` read as one thing and then as another, the towers' guards building the
operand's subtree on each reading.

What reuse would be: a value built for a rule read from one position to another, kept by where it
was read rather than by record index, and taken again when the same rule, specialised the same
way and read at the same strength, reads again from that position to the same end. Recognition
is still repeated; construction is not. It holds only where the reading is the same reading,
which a guard or a factory that reads the context or the marks can break.

**First, measured:** in SQL:2023, how many guard builds happen per parse against how many values
the accepted tree holds, on the corpora sql-ff times. If the ratio is near one, there is nothing
to reuse and Q6 closes; if not, the ratio bounds what reuse can give. sql-ff.

**Closed 2026-09-18: nothing to reuse (sql-ff).** No guard or factory in SQL:2023 reads the context
or the marks, so reuse would have been safe; but on accepted parses nothing dropped by a way back
is built again. Per parse, 2,000 parses a corpus: twenty select items 322 builds, 2 dropped, 0
rebuilt; nested brackets at depth 1, 2, 4 and 8 build 54, 67, 93 and 145, drop 2 and rebuild 0 at
every depth; accepted fuzz queries 1.65 rebuilt of 67.5 (2.4%, an upper bound, since the probe's
key could only be the rule and its factory); accepted DDL 0.3%. A refused parse drops everything
it built, which is what refusing means. Counted, not timed; the count needs no quiet window.

### Q7. Further improvements, decided 2026-09-18 by Igor

1. **Factoring shared beginnings across rule boundaries** — go. Most rules stay on the tape
   because of a few causes (SQL-92: 5 direct, 38 under them; T-SQL: 114 and 523); each cause
   removed moves a whole subtree to one-pass construction. Part of D3's track. sql-ff, after Q1,
   design first.
2. **Diagnostics off the hot path** — go. Where recording the furthest failure stands in the way of
   a faster reading, it leaves the fast path: the fast reading records nothing, and a refused
   input is read again with recording on, which gives the same message. Where a second reading is
   impossible (a stream past its retained window), the case comes back to the architect before
   anything is weakened. expr-2d, after `parserMarks` (the expression language is where recording
   costs 15-25 per cent).
   **Design 2026-09-18 (expr-2d, `docs/design/diagnostics-off-the-hot-path-2026-09-18.md`).**
   Recording is a fifth to a quarter of an EL parse (880 sites) and nothing on SQL. Two places
   record for nobody and go first: `find` over a string, and the lexer's value and measure
   re-reads. Then the in-memory `Parse`/`TryParse` forms: a held `bool` gating every
   recording site (one body, C# 8 floor), a refused input read again with recording on; a
   generic-flag reader only if the stand shows the bool costing more than noise. Streams,
   recovery and `yield` keep recording as today. Refused input then reads twice, up to 2x where
   recording was nearly free (FIX, web headers): the stand gets refused-input rows per family.
   Under Immediate a refused input runs constructions up to twice; the carrier's contract says
   so. Implementer: expr-2d, in the generator, coordinating files with performance-3f.

   **For Igor: the context on the second reading.** The generator cannot rewind a type it did
   not write, so a grammar declaring a `context` (the expression language) gets a second reading
   only if the language says how the context is restored: (2) by duck typing, the resolver
   finding `Mark()`/`Rollback(mark)` on the context type as §7.3 finds constructors, with an
   Info diagnostic saying what was found; or (3) by an explicit clause on the `context`
   declaration. Without either, a grammar with a context keeps recording on the hot path.
   The architect recommends (2) with the diagnostic. **Decided 2026-09-18 by Igor: (2), the
   generator does it itself** — it finds `Mark()` and `Rollback(mark)` on the context type through
   the resolver, marks before the quiet reading and rolls back before the recording one, and says
   so in an Info diagnostic; a context type without them keeps one recording reading, and the
   diagnostic says that too.

   **Landed 2026-09-18 (expr-2d).** Step 1, `cf16f1cd`: `find` over text and the split lexer's
   re-reads record nothing. Step 2, `d4f9a45c`: in grammars with no context and no `recover`,
   an in-memory `Parse`/`TryParse` reads quietly and reads again with recording only if it
   refused; equivalent field by field on 2,466 recorded refusals (`RefusalTests.txt`,
   `385f935d`) over engine, tape and immediate. Paired: SQL:2023 accepted -1..-6% time and
   less allocated on every row (the quiet reading makes no tie list); a late refusal +98%
   time; EL and FIX byte-identical, since EL has a context and waits for the decision above.

   **Found beside it (performance-3f):** `Ways.Lookahead` is read by `Refuse_DotGram` and never
   incremented since `487362c5`, so a refusal inside a lookahead is recorded on the reader and
   not on the engine, and the two renderings can report different positions. A defect with a
   test to write: reader and engine agree on the position of a refusal inside a lookahead. **Fixed
2026-09-18 (`c4587e05`, performance-3f)**: the depth is counted on `Failure`, which every reading
carries. The ruling it needed: a refusal's position is where the parse refused, not where a look
got to, as the language says; the expression language's positions moved by one or two on 32
texts, and `HandExpression` followed in the same commit. A lookahead keeps a grammar off the flat
path, so the test covers the reader and the engine.

3. **Large literal sets as tables** — go, generator part after finance-03's Fix44 report.
   **First step landed 2026-09-18 (finance-03, `9a80c42c`): a fast refusal on a prefix-table
   miss.** Where a choice of literals has a prefix table (only Fix44 in this repository, 16
   tables), a miss inside the table no longer replays the whole alternative chain: it records
   the same expected literals at the same position and goes to `Fail`, proven equivalent by a
   test compiling the grammar with and without tables over every short input. Fix44 BinaryMany
   -93% (string) and -85% (stream); Order unchanged once the cold miss block was hoisted out of
   the hot method (in it, +4% on stream). Fix44 itself is not an example but an oracle and the
   stress fixture for literal sets, and moved to `tests/DotGram.Finance.Fix44` as `Fix44Parser`.
4. **One measuring stand** — go. One command for the ratios to the hand parsers of FIX, EL and
   SQL:2023, allocation, peak memory, first call, a control row and the agreement check. Scratch
   and results on `T:\TEMP` (a fast RAM disk; losing it costs nothing), not in `.work`.
5. **Specialization by a struct type parameter instead of copies of code** — possible, with one
   constraint: the generated code's floor is C# 8 on netstandard2.0 and net472
   (`DotGram.Compatibility`), where a `ref struct` cannot be a type argument. That
   is a runtime limit (byref-like generic arguments arrive with .NET 9 and C# 13's
   `allows ref struct`, and the compiler refuses them on older targets), not a compiler one. It
   costs less than it sounds: the reader may itself be a `ref struct` holding the span and be
   generic over a plain struct that says how its source behaves; only the type argument may not
   hold a span. The runtime specializes generic code
   over value types on every target, .NET Framework included. Prototype first: JIT time and first
   call when several forms are used. It is how D7's one input abstraction is implemented, and Q4.2's
   locations prototype uses the same technique.
6. **Generation time and first call as metrics** — lower priority; columns of the stand (4).

**4 and 6 done 2026-09-18 by the architect**: `--stand` and `--stand-compare` in `DotGram.Benchmarks`
(`Stand.cs`, `benchmarks/README.md`). First run, `cd707572`, pinned, control 31.5 ns; generated
time over hand-written:

| family | rows | generated / hand |
| --- | --- | --- |
| FIX, string | One, Order, BinaryMany, 128 orders | 2.5-3.7x |
| FIX, bytes in memory | the same | 4.4-5.3x |
| FIX, stream | the same | 2.1-4.3x |
| EL, tape | seven expressions | 1.5-3.4x (interpolation the worst) |
| EL, immediate | the same | 1.1-2.5x |
| EL, untyped lambda | one | 1.0x (both in the host) |
| SQL:2023 | nine productions | 8.3-21.8x |

First call in a fresh process: FIX 10.7 ms against 7.8, EL 28.1 (tape) and 20.0 (immediate) against
15.2, SQL:2023 26.8 against 4.4 for a literal and 106 against 21 for twenty select items. A lazily
streamed FIX parse of two million fields holds nothing measurable above the floor on either side.

**The input forms, generalized (Igor's question).** The forms are two independent axes, the
symbol (`char` or `byte`) and the source (in memory, or pulled a block at a time), plus what a
source does at element boundaries (lines insert `\n`). String and `ReadOnlySpan<char>` are text
in memory; `IEnumerable<string>`, `string[]` and `TextReader` are text pulled; `byte[]` and
`ReadOnlyMemory<byte>` bytes in memory; `Stream` bytes pulled. So the generalization is one
machine per symbol type, generic over its source (5), and a source per form, rather than one
machine per form.
