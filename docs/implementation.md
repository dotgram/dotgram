# .Gram — the engine

Companion to [`syntax.md`](syntax.md). That document describes the language, this one
how the generator executes it. Nothing decided here is a decision about the language: if
something below turns out to be inconvenient, this is what changes, not the notation.

Everything below describes the generator as it is. Where a section names something
not yet built, it says so in as many words; [`status.md`](status.md) holds the rest of
what is still open, and `docs/next.md` how each piece got the shape it has.

Several sections were removed rather than corrected, because what they described was
either never built, or was built and later superseded — and left as prose describing
neither would be worse than a gap in the numbering. Removed early: a fast recursive
parser beside a second one, the shape of the code that fast path would generate,
memoization by input position, and recovery as a search for the cheapest edit over a
whole document. Removed later, once each had either shipped in a different shape than
planned or simply never been reached: a memo-table sketch for incremental parsing, an
unstarted plan to compile this language's own grammar with itself, and a prototype
build-order checklist whose every reachable step is now done. What stands in their place
is §1 to §6: one compiler, an automaton that can resume anything, and cheaper renderings
of the same grammar wherever the compiler can prove the automaton is not needed.

References to **Roc** are to an earlier unpublished project of my own, a BNF macro for
Nemerle. Several ideas in the code came from there — normalization done before anything
else, the normalized grammar rendered back as text so that a change to a fold shows up
as a diff, the element-set merge of §5 — along with one of its mistakes, kept out as a
deliberate non-feature: alternatives are never reordered (§5). The other thing Roc
refused to do, inline a rule reference, is done here wherever it can be proved invisible
(§4). It is named rather than linked because there is nothing to link to.

---

## 0. Diagnostics matter more than speed

This is a priority, not a preference: where the two conflict, diagnostics win. A
generated parser five percent faster that says "could not parse" on bad input is
worse than a slow one that shows where and why.

Several things follow that would otherwise look arbitrary.

**Diagnostics come in two tiers, and both run inside the generated parser itself** —
there is no separate, slower engine behind them. The first is close to free: every place
a literal or an element test fails already knows the position and what it wanted, so
recording the furthest one reached costs one write, live, on the same pass that is
recognizing anyway — in the automaton, in the flat method and in the reader alike (§2).
It yields a message of the form "expected `)`" with an exact place, built once the whole
attempt has failed, from whatever survived — one message per run, since parsing stopped
at the first failure and there is no tree past it.

**A feed needs more than that, and `recover` (`syntax.md` §8.2) is the second tier**:
one bad record must not cost the message for every record after it. It is deliberately
narrow — one repetition, named in the notation, committing what it already took so a
later failure cannot un-take it — not a general repair pass over a whole document.
That case was tried and abandoned: a document-wide search for the cheapest edit that
makes broken input parse is a different kind of engine, one this project does not
build (`syntax.md` §11). A machine that recovers is always compiled on the automaton
(`Machine.Recovery.cs`).

**There is no commit point in an expression**, and the furthest-failure tier is why.
Early commitment existed to keep a real error inside an alternative from being
discarded in favour of "nothing matched" at the top once that alternative backtracked
out — but the furthest position reached is already recorded by then, independently of
which alternative is eventually chosen, so nothing is lost by backtracking past it. An
operator doing the same job inside expressions would cost more than it is worth besides:
the same alternative would mean different things depending on where it sits. `recover`
commits on one repetition instead, which is the case furthest-failure tracking alone
cannot serve: a hundred million records, of which one is bad, needs the parse to
continue past it, not just report where it broke.

**Position mapping is mandatory** (`syntax.md` §7.6). A type error in `=> @Add(l, r)`
must be shown on the grammar's line. Without `#line` in the generated code every C#
diagnostic points into a machine-written file, and the seam between the languages
stops working as a seam.

Obligations on the implementation that follow:

- **every diagnostic has a position and a length** — stages report
  `GramDiagnostic(id, message, position, length)` and the shell turns that into a
  `Location` for the IDE;
- **one error, one message**: a stage must recover and continue, or the first typo
  hides the whole file. The hand-written parser recovers to a declaration boundary;
  the generated one steps over an element that began and then failed, to the next place
  the grammar says a new one can start (`syntax.md` §8.2);
- **every message has an identifier** (`GRAM1001`, `GRAM2001`, …) so it can be
  referred to, suppressed and tested;
- **a message names what was expected**, not only what was found — "expected `)`" is
  worth more than "unexpected token".

**Diagnostics are tested by a corpus, not by comparing implementations.** "Good in
absolute terms" is the requirement, not "the same as some other implementation", so
comparing two parsers against each other cannot test it. What is needed instead is
broken input paired with the expected message identifier, position and text — the same
way compilers are tested, and the only way to keep messages from quietly degrading as
the engine changes. It grows by one rule: whenever a message turns out unclear on a
real grammar, that case goes in, together with what the message should have said.

## 1. From grammar text to a file

`GramCompiler.Compile` is the whole grammar half, a pure function of text with no Roslyn
in it (`src/DotGram/Grammar/`). Each stage is a type of its own, so that it can be
exercised and diffed on its own:

```text
GramLexer.Tokenize        text        -> tokens      the standard library spliced on the end
GramParser.Parse          tokens      -> syntax tree
GrammarBinder.Bind        tree        -> GrammarModel    names resolved, through ISymbolResolver
GrammarNormalizer.Normalize  model    -> RecognitionGraph
Retention.Check, FirstSets.Check                     soundness, only if nothing above failed
LexicalSplit              graph       -> lexer + syntax over kinds     only where asked (§8)
CSharpEmitter.Emit        graph       -> C#          skipped if any stage reported an error
```

Every stage runs even after an earlier one failed — a grammar with one bad rule should
still report what is wrong with the other twelve (§0). What a later stage says about a
declaration the parser could not read whole is dropped, because it is about a guess.
Only emission is skipped, since code built from a broken grammar would bury the real
message under compiler errors in the consumer's build.

**Normalization is where most of the grammar's meaning is settled**, as a fixed sequence
of passes over one graph: `with` and `namespace (...)` specialization, conditions, left
recursion rewritten into repetitions and folds, nullability, types, the implicit
captures of §4.1, trivia woven between operands (§10), transparent rules collapsed,
results computed, the checks, then factoring (§5) and pruning of what nothing reaches.
The checks run on the grammar as it was written, and factoring after them, because the
folded shape is one the author may not write.

**The soundness checks come after normalization and only on a sound grammar.**
`Retention.Check` says which publication gets no reader overload and why (§7);
`FirstSets.Check` names every repetition whose end cannot be told from one more of its
elements. Telling an author what a broken rule will not get is answering a question they
are not asking yet.

### The Roslyn shell

`GramGenerator` is a shell over that call, and its shape is what makes it incremental. A
`Compilation` is a different object after every keystroke, so anything downstream of one
is recomputed for every character typed. Binding genuinely needs it — for declared C#
types, their constructors and their properties — so the dependency is narrowed to what it
is for, in three named stages:

```text
Asked      grammar + host         -> the questions its C# names raise   cached on both
Answered   questions + Compilation -> the answers, as values             re-runs, and is cheap
Compiled   grammar + host + answers -> the file and its diagnostics      cached on all three
```

Editing a C# file re-runs the middle stage — a handful of symbol lookups — and stops
there, because the answers it produces compare equal to the last ones. Every value that
crosses a stage is a value (strings, numbers, equatable arrays); a Roslyn `Diagnostic` is
built only at delivery. The stage names are public because they are what a test reads to
say which stage re-ran. An unexpected exception in any stage becomes a diagnostic
against the host rather than a generator Roslyn disables.

### What is written

One file per compilation of a host, named after the host type — `Namespace.Host.g.cs`,
or `Namespace.Host.Suffix.g.cs` for a second reading (§11). `GramCompiler.Compile` called
directly names its result `Host.gram.g.cs`. Before any grammar is looked at, every
compilation gets `DotGram.Attributes.g.cs`, which declares `[Gram]` and its options,
internal and one copy per compilation.

## 2. One machine per published rule, written one of three ways

`CSharpEmitter.Emit` does not compile a grammar as a whole. It compiles **machines**, and
a machine is a published rule together with everything that rule reaches (`Published`).
`parse R` and `find R` share one machine, entered at two states. Two different rules that
reach each other share one too, since what they reach is the same set — the expression
layer of standard SQL published both `SearchCondition` and `ValueExpression`, and one
machine per publication had compiled the whole of it twice. Once every machine is built,
`Joined` folds a machine whose root another machine already reaches into that one, as an
extra entry — but only where both were written the same way, so that joining changes how
neither publication is read. A grammar that publishes nothing is one machine over every
rule, which is what a caller asking only for recognizers wants.

Each machine is then written in the cheapest of three renderings its rules allow, asked
in this order:

```text
flat      Machine.Flat.cs     one method per publication, the automaton's states laid
                              out in it with no arena and no dispatcher
reader    Machine.Reader.cs   a method per rule, the way a person would write it
engine    Machine.cs          the automaton of §4, which can do anything the notation says
```

**Flat** is for a machine that needs none of the three things the arena is for — no
recursion, no backtracking that has to reach past a local, no deferred construction.
`CanLower` asks it of a valueless publication; `CanLowerValued` of one whose value is a
single construction at the top over spans of the input, which then runs once, when the
parse is accepted. The same states are compiled by the same code and rendered differently
(`RenderFlat`, `RenderFlatValued`); open ways back that no repetition surrounds become
locals. The question is asked of each machine's own publications and of what that machine
reaches: a recovery, a climb or a stream elsewhere in the grammar is some other machine's
business, and no longer costs this one its flat path.

**The reader** is the default for everything else that qualifies — `Direct` is true
unless the host sets it off, which a test of the engine does. `CanDirect` refuses, and
leaves to the engine: a `find`, a publication read from a stream, a recovery, a capture of
what a lookahead saw, a call with arguments, an external recognizer that keeps a value,
and a guard handed a value whose construction asks for the input. It records the refusal
in words (`Refusal`); over kinds, where it changes what the grammar means, that is said as
`GRAM5005` (§8). §6 describes the reader.

**The engine** takes whatever is left, and is the one rendering that serves every
publication kind, every stream and every recovery.

## 3. Nothing is built while matching

**One idea, taken and worth stating on its own: a match allocates nothing, and the
typed result is materialized once it has succeeded.** Speculative parsing then costs
almost nothing — backtracking restores a position and has nothing to undo — which is
what makes ordered choice affordable without a commit point.

On the engine it is in the code as it always was: `ParserEntry` (§4) is an all-integer
struct — positions, indices, no value field — so recognition can run to completion
writing nothing but those. Materialization is a separate pass, over what the arena holds
once a parse has accepted (`syntax.md` §7.3, `Machine.Materialization.cs`): one walk
turns `Capture`/`RuleCapture`/`Construct` entries into the typed values a rule's own `=>`
needs, and calls it exactly once per rule, from what the arena already recorded rather
than by re-deriving it. Values live in a table for each type the grammar can produce
rather than one table of `object?`, so nothing is boxed on the way in or cast on the way
out; the machines of one file agree on one numbering of those tables (`ValueTables`).

The alternative — a flat `int[]` holding the whole raw tree as offsets, materialized
lazily — buys the same property and costs memory proportional to the input, which
line-oriented streaming cannot afford (§7). Not taken.

### Carriers: where the reader keeps a value until it may be built

The reader has the same obligation and more than one way to meet it. What holds the
pieces of a value until the construction runs is its **carrier** (`CarrierKind`,
`Machine.Carrier.cs`), set with `[Gram(Carrier = …)]`:

```text
Tape        records on a tape, built by a walk once the parse has been accepted.
            Keeps §7.3 whole: a construction runs once per node of the accepted derivation
Immediate   no deferral: a => runs the moment its alternative has been read
            (Machine.Immediate.cs). A stack per value type for what a rule gathers,
            registers in the reader itself for what a callee hands its caller
Auto        the default: the generator chooses between Tape and Immediate
```

**Immediate gives up exactly one thing**, and the choice of `Auto` is built around it:
a construction runs once per derivation *tried*, not once per derivation accepted.
`Auto` therefore picks Immediate for a machine only where every rule it builds is read
solely for the derivation that stands or for one on which the whole parse then fails
(`Replay`), and where no rule it reads can be asked for a second answer after giving its
first — which only the reader, once written, can see, so the choice is made after a first
rendering on the tape. What remains given up is a parse that fails having already run
some constructions. `GRAM5012` says which carrier was chosen and, where it was the tape,
which rules kept it there.

A carrier the author named that cannot carry a machine leaves that machine
on the tape, and `GRAM5007` says so. The engine and the flat path have no carrier to
choose: the engine builds from its arena, and the flat path's one construction runs at
accept.

## 4. The engine: one automaton per machine

Every rule a machine reaches is compiled into one C# method. A rule is not a method and a
call is not a call: each place a rule can be in becomes a labelled state, and moving
between them is `goto`. What a rule call leaves behind is an entry in an array — the
arena — saying where to carry on when the called rule is done.

The reason is `syntax.md` §4's promise that a rule call is transparent to backtracking. A C# method
cannot be suspended and resumed, and resuming is exactly what it means to come back into a
rule and take a different alternative. Once the continuation lives in an array rather than
on the machine's stack, that is possible — and so is recursion deeper than the stack
would allow, which is the same fact seen from the other side.

### What the arena holds

Three unlike things, and telling them apart is most of what the engine's correctness rests
on (`ParserEntry` in `Support.cs`):

```text
frames          Call, Completed              where a rule was called from
ways back       Choice, Repeat, Run,         where the parse could return to
                LoopExit, TurnDone,
                Lookahead, Atomic
                Dead                         a way back committed past, left in place
                PendingRecovery              a recovery that may still be taken
derivation      Capture, CaptureOpen,        what was recognized, for building values with
                RuleCapture, Construct,
                StateSet, StateEnd, Recovery
```

A failure unwinds by taking entries off the end until it finds a way back. A commit — what
`{ }` does — turns the ways back inside it into `Dead` entries and leaves everything else.
Materialization walks what is left and runs the constructions; a `Recovery` entry is what a
recovered element leaves for its handler and for the report.

When a guard or computed selector asks for typed captures, the engine marks their
unbuilt values and bounds the materializer's owner/construction scans by the earliest
requested capture. Child calls have later indices, so earlier records need not be
visited on each turn of a repeated field. Link maintenance remains incremental, and
acceptance uses the full range. Recovery grammars retain the full scan; enclosing
state marks still require their complete chain. See
[the value-dependent switch experiment](design/value-dependent-switch-2026-09-16.md).

**An entry's index is its name.** A capture of a rule's value holds the index of the entry
its call completed into; one materialized value names the next. So nothing may renumber the
entries around it: a commit puts a way back out where it lies rather than removing it, and
a repetition compiled without entries keeps its position in a local instead. Two defects
have come from breaking this rule and both looked like something else at first. The same
reasoning is why a turn's count and a capture's start are entries (`TurnDone`,
`CaptureOpen`) rather than values rewritten in place: an in-place rewrite survives
backtracking that the thing it recorded does not.

### The arena is a mechanism, not a tax

Everything above describes what is needed when a parse can be resumed. Most of a grammar
cannot be, and the compiler is expected to prove it and write something cheaper. What it
proves today:

- **A rule outside every call cycle that produces no value** is compiled into its callers.
  Its expansion terminates because the call graph beneath it is a DAG, and what the
  duplication costs is text.
- **A captured call to a rule whose value is one construction over spans of the input**
  is compiled as the callee's body, in place (`Machine.Sites.cs`). The callee's captures
  record into slots of the site's own, and the materializer calls the callee's
  construction over those spans directly. The captures are ordinary arena records, so
  backtracking over the site unwinds them as it would any other, and construction is still
  deferred to accept.
- **A rule that keeps no records and can be read by committing to its first reading**
  becomes a plain scanning method, with no arena at all (`Machine.Scan.cs`). Atomic braces
  say so outright; without them `Scannable` proves it from what may follow the rule at
  every call site. Backtracking state lives in locals, restored on the spot. What this buys
  is the seam: trivia is applied everywhere, and through a scanner each application costs
  one call.
- **A choice whose alternatives cannot begin with the same character** needs no entry: one
  character decides which it is, and having decided there is no second reading. Where only
  some alternatives are ruled out, the entry is written only if one of the rest could still
  match.
- **A repetition whose body matches one way only, and which is followed by something the
  body cannot begin with**, is run to its end and never asked to give any of it back. Every
  place it could stop short is a place a turn began, so the character there is one the body
  starts with, and the continuation cannot start with it. Its one standing way out is a
  single `LoopExit` rather than a `Choice` per turn.
- **Where such a body also writes nothing to the arena**, the whole construct is a loop:
  no entry, no count, no way back, and its required turns written out rather than counted.
- **A character run guarded by negative delimiter lookahead**, `(?!Delimiter & any)*`
  or `+`, has a specialized state-machine path (`Machine.Delimiter.cs`). A pure
  one-character delimiter test becomes a complemented run test. A delimiter shaped as
  `Padding* & Stop & Suffix*` (the suffix is optional in the grammar) becomes a linear
  scan remembering the start of trailing padding. Padding and Stop must be disjoint
  pure character tests; Suffix must also be a pure character test. Named rules are
  resolved after `with` specialization. At EOF unmatched padding remains in the text.
  The padded case accepts unbounded repetitions, including a specified minimum;
  bounded padded runs retain the general machine. Existing run entries preserve
  shortening on backtracking. A minimum-length failure re-enters the general path to
  retain exact diagnostics. Captures, C# guards/actions, recursive aliases, recovery,
  overlapping sets, token-kind and incremental starvation paths are not bypassed.
- **Recovery with a pure character delimiter** reuses that linear search before
  invoking its ordinary synchronization rule. This includes disjoint padding and
  stop sets. The last rejected candidate is checked normally to preserve the
  furthest diagnostic; the separator is consumed by the existing recovery path.
  No per-position synchronization attempts are needed over the skipped text.
  Raw error extents still exclude delimiter padding, while unmatched EOF padding
  remains part of the error. Unsupported synchronization rules keep the original
  per-position search.
- **Text alternatives none of which begins another** are decided where they differ, reading
  what they share once and moving the position only when one has matched whole.

The first sets these rest on are approximate in the direction that says "anything" when
unsure, so a proof that cannot be made is not made and the general machinery stays. What
follows a rule is the union over its call sites, computed as a fixed point over the call
graph (`FollowSets`); a `parse` publication contributes the end of the input, which is a
fact and not a silence.

### What is written out

The state table is planned before a character of it is emitted (`Machine.Layout.cs`).
Compilation reserves a state whenever it needs somewhere to come back to, which leaves
states nothing reaches and states that are nothing but a jump. A signpost is followed to
wherever it ends and not written; everything that pointed at one — a `goto`, a resume
point recorded in the arena, a case of the dispatch — is made to point past it. What is
left is laid out in chains, each state followed by the one it jumps to, so that jump is
dropped and becomes the next line. What cannot be reached from a publication is not
written either — a rule compiled into all of its callers is called from nowhere.

**A large table is written in parts** (`Machine.Parts.cs`). RyuJIT stops optimizing a
method past about two thousand basic blocks, and every real parser here is past it. The
limit is per method, so the states are divided among local functions, each its own method
with its own budget, sharing the enclosing method's variables. Parts are sized evenly
towards `PartSize` — 150 of the generator's own block estimate unless the build sets
`DotGramPartSize`, a wish rather than a constraint — and each cut is moved to a gap between
chains, where no `goto` crosses: control reaches the dispatcher only a few times a parse,
while a crossing jump would run per character. A method that still ends up past the limit
is reported as `GRAM5003`, a warning, since the parser is correct and only slower.

## 5. Filtering alternatives by their first element

Each alternative has the set of what it can begin with computed, and an alternative is
not entered when the current character falls outside it. On the engine the test is
written by `RangesTest` (`Machine.Analysis.cs`) in one of three shapes, by size:
comparisons while the ranges are few enough to read, a table lookup while the set stays
inside ASCII, and a search over an array of bounds for anything wider — which is
what a Unicode category is. Where several alternatives are filtered in a row, each test is
written knowing the ones before it did not fire, so a test that can never fire is not
written. An alternative whose first set is "anything", "nothing", or which can match empty
is not filtered: the approximation has admitted it does not know.

The reader goes one step further: where the first character (or token kind) divides the
alternatives, the choice is a `switch` on it (`Dispatchable`), and an alternative that then
fails is the choice failing, since no other could have matched there.

Cheap, computed when the grammar is built, and it removes most of the cost of ordered
choice — which matters more because ordered choice backtracks fully and no operator cuts
it short (§7). Most alternatives never get tried at all.

What makes it cheap is normalization done first, and Roc's macro is where that came
from, whose fold was: single-character alternatives and ranges are separated out, sorted
by first character, then merged — `'a' | 'b'` into `'a'..'b'`, a range absorbing
anything it contains, duplicates dropped.

**What not to take from there is the reordering.** Roc moves the single-character
alternatives ahead of everything else, which silently changes ordered choice:
`"ab" | 'a'` becomes `'a' | "ab"` and the second is then unreachable. It never bit
because Roc's structural generator was a stub and its character
tests compile to `c == 'a' || …`, where order cannot matter — the multi-character case
was never executed. Merging is safe exactly where the match length is fixed at one
item; beyond that it is a diagnostic, not a rewrite (`syntax.md` §11).

### Shared leading operands are read once

`A & X | A & Y` reads `A` twice when `X` fails, and the doubling compounds through
nesting. Normalization folds it to `A & (X | Y)` (`GrammarNormalizer.Factoring.cs`) — but
only where that cannot be seen from outside. The two are the same grammar exactly when `A`
has one reading where it stands; where it has several, the alternatives prefer a shorter
reading of `A` that lets a tail fit, the folded form does not, and which `=>` runs would
change. `Determinism` is the proof, and where it does not reach nothing is folded and
`GRAM4016` tells the author, whose choice it then is.

## 6. The reader

`Machine.Reader.cs` writes a machine as the methods a person would have written: a rule
is a method returning the position it reached or `-1`, a sequence is statements one after
another, a repetition is a `while`, and a choice is the `switch` of §5 or one attempt after
another. An alternative that can fail halfway and has a sibling after it becomes a method of
its own, because `-1` is how it tells its caller to try the next. A rule that is a token or
a few tokens and nothing else is written inline where it is called.

**Over characters a rule's answer does not stand**, and the reader has to honour that
without a label to jump back to. A rule that can open a way back is written as two
methods: what it is, and the way back into it. Every decision the body takes is recorded
on a tape of ways (`Ways` in `Support.cs`, two integers a way), and the way back calls the
body again after moving the tape on, so that a replay reads the earlier decisions back
rather than taking them again (`Ways.Retry`). A rule that opens no way and calls nothing
that does has one reading and is one method. Over token kinds (§8) a rule's answer stands,
and there is no tape at all.

**Recursion runs on the thread's stack**, which the automaton did not need. A rule that can
reach itself probes the stack once in sixty-four entries; where the runtime says the margin
is gone, the reading carries on over a new thread's stack — the reader is made again there,
the input handed over as `ReadOnlyMemory` — and takes another if it goes deeper still.
`[Gram(Stacks = n)]` bounds how many, after which the parse fails with
`InsufficientExecutionStackException`. A reading over a stream is not carried over.

## 7. Execution modes, and what bounds retention

Which mode a parse runs in is decided by the type of the input, at the call site
(`syntax.md` §6.3). The compiler decides *whether* the reader mode is offered at all,
and both input types that ask for it end up in the same one. (Reading a stream is the
engine's: neither the flat path nor the reader of §6 takes a publication that streams.)

```text
in memory   string / ReadOnlySpan<char>
            full backtracking, the whole input addressable throughout

by reader   TextReader / IEnumerable<string>
            retention is a reused, growable buffer (`Window`), advanced as the
            parse no longer needs what falls behind it. IEnumerable<string> is
            not a second mode: a thin adapter (`Lines`) glues the sequence back
            into an ordinary TextReader — \n reattached, since a grammar's
            `eol` expects it — and everything past that point is the reader
            case, unchanged.
```

**Whether a grammar can stream at all is the retention analysis** (`Retention.cs`):
how far back a pending alternative could still return. Within what the window can be
asked to hold, the reader overloads are emitted; back past the start of the whole
input, they are not, and the method that would have taken a reader simply does not
exist — a call that tried anyway is a C# compile error at the call site, not a runtime
one. Note what this restricts and what it leaves alone: it decides which overloads
exist, and changes the meaning of none of them.

**A grammar that does not get the reader overload is told so.** Not a refusal — the
grammar is fine in memory — but a call that would otherwise just fail to bind with
`cannot convert from TextReader to string`, naming neither the rule responsible nor
anything to do about it:

```text
'ParseLog' gets no overload taking a reader: the alternative at Log:7 may return to
the start of the input. docs/syntax.md §6.3 says which rules get one, and why.
```

`recover` (`syntax.md` §8.2) reaches the same bound from the other side, by being told
rather than by inference: its synchronization expression is a point the parse cannot
return past, so a marked repetition streams by construction and the analysis has
nothing to prove. It commits as well, which the analysis does not — but on one
repetition, named in the notation, and the rules it calls mean the same thing inside it
as anywhere else.

**`Match<T>.Position` is a `long` regardless of mode** — an offset into the whole
input, and an in-memory `string` could in principle be one an `int` cannot index just
as much as a streamed file could. What is mode-specific is what happens *inside*
recognition: an ordinary index into the current window, which is what `ParserEntry`
(§4) actually stores while a parse is running, widened to a `long` only once, at the
one place a position crosses a publication's own boundary out to the caller.

## 8. The lexical split

A grammar is scannerless by default: its terminals are rules over characters, read by the
same machine as everything else. `[Gram(…, Lexical = true)]` asks for it to be cut in two
instead — a generated lexer over characters, and the syntactic machine over the token kinds
the lexer produces (`GramCompiler.Cut`, `LexicalSplit.cs`). Nothing has to be declared about
where the cut goes: a rule is syntactic when it carries trivia (§10) and lexical when it
does not, and a terminal is a call that crosses from the first to the second.

**The machine underneath does not change.** A kind fits in the sixteen bits a character
has, so the syntactic half is an ordinary `RecognitionGraph` whose "characters" are kinds,
and every analysis and every rendering above runs over it unchanged. `TerminalInventory`
numbers the kinds, and a kind is a *set* of patterns, not a pattern: `SELECT` is the
keyword and also a `RegularIdentifier`, so a position that wants an identifier accepts the
keyword's kind, and `?!Reserved` takes it back out as a range test over kinds. The first
version, one kind a pattern, refused `zone` as a column name.

**The lexer is one deterministic automaton over all the patterns at once**
(`LexicalAutomaton`): Thompson and a subset construction, over an alphabet of atoms — the
coarsest partition every pattern's character sets are a union of — so a Unicode category
costs what its intervals cost. Longest match falls out of running to a stop and remembering
the last accepting state. `LexerEmitter` writes it as one method: each state reads from a
row of 128 cells placed where that state's own alphabet lies, and only what the row does
not answer is asked by a chain of tests. There is no arena and no way back, which is the
signal that the boundary is drawn in the right place. Trivia between tokens is skipped by
calling the scanner of §4 for it.

**A terminal that builds a value is read twice.** The lexer finds where it ends; a second
machine over characters, tagged `_Value`, parses exactly that text and builds what the
rule's `=>` says (`LexicalSplit.Valued`). It cannot fail, since the lexer already accepted
the extent. A terminal written as a beginning followed by a host measurement — `'<' & @M`
— is the same second machine entered where the automaton stopped. A terminal that is
nothing but `@M` has no beginning to stop on, so the lexer asks `M` at every token, and
`GRAM5011` says what to write in front of it.

**Where the cut cannot be made, the grammar is compiled over characters and told why**
(`GRAM5004`, a warning, since the author asked for something else): a `find` publication,
which hunts through characters for a place to begin; a terminal that is not a regular
language; no rule carrying trivia; or trivia that is not a scanner. Over kinds,
`FirstSets.Committed` asks the soundness question again, since an overlap there is settled
by the reading that fits rather than by backtracking; and a machine the reader refuses is
reported as `GRAM5005`, because the engine it falls back on backtracks into a rule the
notation promises will stand.

## 9. Operator precedence

Precedence climbing: atoms and prefix operators in one loop, infix and postfix in
another, with a binding power per level.

`syntax.md` §4.3 has two ways to say precedence, and only one of them needs this.

**Levels as rules** needs no engine: direct left recursion is rewritten into a
repetition plus a fold, and an operator costs one iteration of an ordinary repetition.
That is the default, and for the handful of levels most notations have it is also the
faster of the two — nothing to climb, no binding power to compare.

**Binding powers** (§4.3.1) are what this section is for. They admit `E = E & '+' & E`
and an expression language written as one rule, neither of which a rewrite can take,
and they pay for it here: one loop with a binding power rather than a ladder of calls,
which is the trade that wins once the levels are many.

## 10. Trivia and keywords

The usual way to place whitespace is an invariant — every rule consumes the whitespace
after itself and never before — plus rules for where to insert accordingly: after a
literal and after a lexical rule, not after a structural one, once at the start, and
attributes to override either way.

**We need none of it.** `syntax.md` §4.5 requires `trivia` to be nullable, and from
that condition unconditional insertion is safe: a second application consumes nothing,
so nothing is ever doubled. The whole rule collapses into "insert everywhere", with one
insertion at the start of a published rule for leading whitespace, and normalization
drops the insertions entirely when `trivia` is empty.

**Keyword boundaries are declarative the same way** (`syntax.md` §4.6): `wordboundary`
names the characters that continue a word, and once it is not empty, every string
literal whose characters all fall in that class picks up a `& ?!wordboundary` of its
own — decided when the grammar is built, so `"if"` gets the check and `"("` does not.
Whether a literal qualifies never has to be written down; the class alone decides it.
The check goes before the trivia insertion, so it asks whether a letter follows the
keyword rather than whether one follows the whitespace after it.

## 11. Locations, and a second reading of one grammar

**Locations are opt-in.** `[Gram(…, LocationType = typeof(ISqlSpan))]` names an interface
with a settable `Span`; normalization collects the rules whose value is assignable to it
(`RecognitionGraph.Located`), and the reader writes the property once on each such value
as it is made — the rule's own text, without the trivia around it. A grammar that names
nothing is compiled exactly as it would have been.

**A host can carry more than one compilation of its grammar.** Each `[GramOptions(…)]` on
the class is a further reading under different options — another carrier, another
division, locations where the first has none — and each goes into a nested class named by
its `Suffix`, since a file's support types are written once and would otherwise collide
name for name. `TransactSqlParser` is compiled plainly and again with `LocationType` as
`TransactSqlParser.Located`, each in its own file (§1).

## 13. What this engine is not for

Things a grammar engine can reasonably grow, each a good answer to a question this
project is not asking. Listed so that not having them reads as a decision.

- **An AST layer of its own**: a second notation mapping the parse tree onto typed
  nodes, with dependent properties, symbols, scopes and name binding. That is a second
  and a third language on top of the first; here that place is taken by C#
  (`syntax.md` §7).
- **Language composition**: grammars extending other grammars, dynamic extension
  points, resolving ambiguity between extensions. Where most of the runtime complexity
  of such engines comes from.
- **A chain of bootstrap stages.** The `.gram` front end — lexer and parser — is
  hand-written and stays that way; compiling this language's own grammar with its own
  generator was considered as a differential check on the generator, not to replace the
  front end, but was never started.
- **Formatting markers** — soft breaks, indentation, block outlining, so that one
  grammar yields a printer and code folding as well. A good idea that widens the task
  well beyond the current one.
