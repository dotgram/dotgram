# .Gram — the engine plan

Companion to [`syntax.md`](syntax.md). That document describes the language, this one
how to execute it. Nothing decided here is a decision about the language: if
something below turns out to be inconvenient, this is what changes, not the notation.

Almost everything here is borrowed from [Nitra](https://github.com/rsdn/nitra), a
project of the same lineage (RSDN/Nemerle) solving a larger version of the same
problem. File references are given so the working code can be checked against.

---

## 0. Diagnostics matter more than speed

This is a priority, not a preference: where the two conflict, diagnostics win. A
generated parser five percent faster that says "could not parse" on bad input is
worse than a slow one that shows where and why.

Several things follow that would otherwise look arbitrary.

**The two-parser architecture exists for diagnostics, not for speed** (§1). The fast
path is there so the slow one can be afforded: since recovery runs only on broken
input, it is free to be as expensive as it likes — and therefore free to look for the
*right* answer rather than the first one.

**Recovery has no notation** for the same reason. The quality of messages must not
depend on whether the author placed synchronization points; place them wrongly and it
gets worse than having none.

**There is no commit point**, and diagnostics are why. Early commitment existed to stop
a real error inside an alternative from being discarded in favour of "nothing matched"
at the top; the recovery engine does that job better, from the cheapest edit. What is
left over is smaller and still open: how an author says "this is an error, not a
mismatch", so that a failing semantic guard can say "unsupported symbol XYZ" instead of
letting a sibling be tried.

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
  the generated one by the cheapest edit of the input (§6);
- **every message has an identifier** (`GRAM1001`, `GRAM2001`, …) so it can be
  referred to, suppressed and tested;
- **a message names what was expected**, not only what was found — "expected `)`" is
  worth more than "unexpected token".

## 1. Two parsers over one grammar

The main architectural decision, worth taking whole.

```text
fast path      generated recursive descent, no recovery
               success → done
               failure, or not parsed to the end
                   ↓
error path     a separate engine over a state machine built from the same
               grammar, looking for the cheapest edit of the input
```

`ParseResult.Parse()` (`Nitra.Runtime/Parsing/ParseResult.n:124`) starts the second
engine only if the first returned a negative result or stopped short. On correct input
recovery costs nothing.

The consequence for the language: **error recovery needs no notation**. The author
writes neither `Recover` nor synchronization points — the open item in `syntax.md`
§10 is settled by dropping the construct rather than inventing it.

Both engines are built from one description of the grammar: the fast one as generated
code, the slow one as a state machine (`ParsingSequence` with `States`, `StartStates`,
`EndStates`).

## 2. The fast path: the shape of generated code

The parse method's signature
(`Nitra.Compiler/Generation/Parser/ParseMethodEmitter/CompileSequence.n:34`):

```csharp
int Parse(int pos, ReadOnlySpan<char> text, ParseState state);   // new position, or -1
```

Flat, no exceptions, no objects, no delegates. Exactly what `syntax.md` promises with
the words "code comparable to a careful hand-written parser".

Where we differ from Nitra:

- `ReadOnlySpan<TIn>` instead of `string`, with no interface over the input;
- `-1` as the failure signal is replaced by an outcome that tells "no match" from
  "error" — a requirement of the language, not of the implementation.

**There is no runtime assembly.** Everything a generated parser needs is emitted into
the same assembly, `internal` (`syntax.md` §6.1). A consumer takes one analyzer
package. It follows that nothing which must be shared between assemblies can appear
here — and if something does, it goes into the optional shared mode
`[assembly: GramRuntime]` rather than into a dependency.

Parameterized rules are specialized per call site (`syntax.md` §4.2), so a recognizer
parameter disappears during generation and becomes a direct call.

## 3. The parse result: a flat `int[]`

```nemerle
public mutable rawTree : array[int];   // (text.Length + 1) * 10
public mutable memoize : array[int];   // indexed by position in the input
```

(`Nitra.Runtime/Parsing/ParseResult.n:33-34`, `:102-103`)

Not one object is allocated while parsing: a node is an offset into `int[]`, and the
links between nodes are offsets too. The typed result is materialized lazily, after
parsing has succeeded.

For us that means `RecognitionResult<T>` is a struct with a discriminant field, and
records are built on the way out rather than along the way. As a side effect
speculative parsing becomes cheap: backtracking is just restoring a position, with
nothing to undo.

The recovery engine is the one piece that is both grammar-independent and large enough
for duplicating it into every assembly to be noticeable. It is also the only candidate
for someday justifying the shared mode of §6.1 by volume of code rather than by types.

## 4. Memoization

The table is indexed by **position in the input**; each position holds a linked list
of results for different rules (`memoize[pos]` is the head, the `Next` field the
successor; `ParsePrefix.n:75-77`).

This stays an execution strategy rather than a guarantee of the language (`syntax.md`
§7.2: code in `@(...)`, `where` and `=>` must be safe to invoke repeatedly whether or
not anything is cached).

## 5. Filtering alternatives by their first element

Each alternative has the bounds of its first character computed, and an alternative is
not tried at all when the current character falls outside them
(`ParsePrefix.n:85-95`):

```nemerle
when (prefixRule.LowerBound <= c && c <= prefixRule.UpperBound)
```

Cheap, computed when the grammar is built, and it removes most of the cost of ordered
choice — which matters more now that ordered choice backtracks fully and there is no
commit point to cut it short (§7). Most alternatives never get tried at all.

What makes it cheap is normalization done first, and Roc's macro is where to take that
from (`P:\OldProjects\Roc\Macros\BnfMacro.n:550-602`): single-character alternatives
and ranges are separated out, sorted by first character, then merged — `'a' | 'b'`
into `'a'..'b'`, a range absorbing anything it contains, duplicates dropped. After
that an alternative's first-character bounds are already computed.

**What not to take from there is the reordering.** Roc moves the single-character
alternatives ahead of everything else, which silently changes ordered choice:
`"ab" | 'a'` becomes `'a' | "ab"` and the second is then unreachable. It never bit
because Roc's structural generator was a stub (`BnfMacro.n:820`) and its character
tests compile to `c == 'a' || …`, where order cannot matter — the multi-character case
was never executed. Merging is safe exactly where the match length is fixed at one
item; beyond that it is a diagnostic, not a rewrite (`syntax.md` §10).

## 6. Recovery as the cheapest edit

The slow engine enumerates edits of the input — insert what was expected, delete what
was not — and picks the solution of least total cost.

- the cost is a pair, inserted and deleted (`TokenChanges`);
- a priority queue on cost, then on position
  (`Internal/Recovery/RecoveryParser/RecoveryParser.n:27-40`);
- the loop: parse to the point of failure → try insertions → try deletions → repeat
  until a solution is found (`:74-97`);
- a timeout with graceful degradation: if it does not finish in time, delete the rest
  and stop (`:92-96`).

Proven on a C# grammar — Nitra's repository has a complete one.

## 7. Execution modes, and why there is no commit point

Which mode a parse runs in is decided by the type of the input, at the call site
(`syntax.md` §6.2). The compiler decides *how* each mode is implemented.

```text
in memory      string / ReadOnlySpan<char>
               full backtracking, memoization over the whole input

line by line   TextReader / IEnumerable<string>
               retention is one line: read it into a reused buffer, hand the parser
               a ReadOnlySpan<char> over it, parse, discard. No window, no
               cross-chunk logic, no allocation per line.

by window      retention bounded but not by a line — deferred, see below
```

**The line-oriented mode is not an optimization of a windowed one; it is a simpler
implementation.** When every repeated element ends at a line boundary the parser can
never need to look further back than the current line, so the sliding buffer, the
release logic and the position arithmetic all collapse into one reused array.

Detecting it is an analysis of the same nature as nullability: does every path through
the repeated element end with `eol`? It is not particular to feeds — any line-oriented
language lands in the same mode.

**Whether a grammar can stream at all is the retention analysis**: how far back a
pending alternative could return. Bounded by a line, the streaming overloads are
emitted; bounded by the whole input, they are not, and the message names the rule
responsible. That is what replaces a commit point: it restricts what may stream
without changing what anything means, whereas committing would make the same
alternative mean different things depending on where it was written.

**The compiler reports the mode it picked.** Not a warning — a statement of fact, so
that one grammar eating four kilobytes and its neighbour eating a hundred megabytes is
never a mystery:

```text
Feed streams line by line — retention is one line
Log  cannot stream — the alternative at Log:7 may return to the start of the input
```

**The windowed mode is deferred and may never be needed.** Source files fit in memory,
feeds are line-oriented, and what is left — huge input that is neither — is binary
formats, which are out of scope.

Positions follow from the mode: inside a line an ordinary `int` has room to spare,
while what crosses the publication boundary for a streamed parse is a `long`, since a
feed of tens of gigabytes has offsets that do not fit in one.

Note that §3 and §4 assume memory-sized input: a `rawTree` and a memo table indexed by
absolute position cost forty gigabytes on a ten-gigabyte feed. In line-oriented mode
both are per-line and are reused, which is what makes that mode cheap rather than
merely possible.

## 8. Incremental parsing

`Nitra.Runtime/Parsing/IncrementalParser.n:38-57`, about thirty lines:

1. compare old and new text from the start — the length of the common prefix;
2. compare from the end — the length of the common suffix;
3. copy the tail of the memoization table, shifted by the difference in lengths;
4. parse again, landing in ready entries past the edit.

An honest limitation, visible in the code: only the **tail** is reused, the head is
recomputed (the code for the head is commented out). For an editor that is enough —
an edit is usually in the middle, and the tail is the longer part.

## 9. Operator precedence

`ExtensibleRuleParser` is split into `ParsePrefix` (atoms and prefix operators) and
`ParsePostfix` (infix and postfix), with a `BindingPower` — classic precedence
climbing.

`syntax.md` §4.3 currently has the levels written out as rules by hand, which works
with no engine at all. If a precedence construct appears later, it should be lowered
into this shape rather than into a third one.

## 10. Trivia and keywords

Nitra's whitespace insertion is in
`Nitra.Grammar/Typing/TypingUtils-TypeRuleExpression.n:37-81`, on the invariant that
**every rule consumes the whitespace after itself, not before**. Hence: insert `s`
after a literal and after a call to a lexical rule; do not after a structural rule,
which has eaten its own; leading whitespace once, in the start rule; plus attributes
to override either way.

**We do not need any of that.** `syntax.md` §4.5 requires `Trivia` to be nullable, and
from that condition unconditional insertion is safe: a second application consumes
nothing, so nothing is ever doubled. The whole rule collapses into "insert
everywhere", with one insertion at the start of a published rule for leading
whitespace, and normalization drops the insertions entirely when `Trivia` is empty.

Keyword boundaries are declarative in Nitra too — a class of keyword characters plus a
separator rule `!IdentifierPartCharacters s`, after which every string literal falling
into that class gets the boundary check automatically. For us that remains open
(`syntax.md` §10).

## 11. Order of work

The front-end stages are hand-written and already work: the `.gram` lexer and parser in
`Grammar/Syntax`, each with a textual dump the tests are built on. Further along the
pipeline: name binding, normalization, generation.

An engine prototype has to confirm execution rather than notation, hence:

1. **The fast path over a frozen subset**: sequence, ordered choice, quantifiers,
   captures, result construction. No trivia, no precedence, no streaming input.
2. **First-tier diagnostics together with the fast path, not after it.** See below:
   without them there is nothing to show even on a prototype, and by §0 they are a
   requirement of the product.
3. The flat representation and memoization — immediately, because they determine the
   shape of the generated code rather than optimize it afterwards.
4. Filtering by first element — right after, since full backtracking makes it the main
   thing keeping ordered choice cheap (§5).
5. Second-tier diagnostics — the recovery engine (§6), once the fast path works.
6. The line-oriented mode (§7) — retention analysis, then the reused buffer. Feeds do
   not work without it, and it is far simpler than the windowed mode it replaces.
7. Incremental parsing last; it attaches to a finished memoization table and changes
   nothing in it.

Check against the three scenarios with the widest coverage: a calculator (recursion
and levels), a feed (sequence results and recovery), a URL (shared literal prefixes).

### Two tiers of diagnostics

The recovery engine does not have to be pulled forward whole — message quality comes
in two tiers of very different cost, and the first is nearly free.

**The first comes out of the fast path.** Remember the furthest position of failure
reached and the set of what was expected there. In Nitra that is `MaxFailPos` plus
`GetParsingFailureError` (`ParseResult.n:181-205`): at the failure position it tries
every token of the grammar and keeps those that would have fit. That yields a message
of the form "expected `)`" with an exact place.

It costs almost nothing: the position is tracked anyway, and the expected set is known
from the grammar at build time. But it yields **one** message per run — parsing was
abandoned at the first failure, so there is no tree.

**The second is the recovery engine (§6).** It gives what the first cannot:

```text
first tier      one error, parsing abandoned, no tree — so no highlighting,
                no completion, no go-to-definition

second tier     every error in one pass, a complete tree,
                the IDE works on broken input
```

The second is what an editor is for: text being typed is almost always broken. So the
second tier is not "better messages" but the condition of being usable in an IDE.

### Diagnostics are tested by a corpus, not by comparing implementations

The requirement in §0 is "good in absolute terms", not "the same as some other
implementation". Comparing two parsers against each other therefore cannot test it.

What is needed is a corpus: broken input, the expected message identifier, the expected
position, the expected text. That is how compilers are tested, and it is the only way
to keep messages from quietly degrading as the engine changes.

The corpus grows by one rule: **whenever a message turns out to be unclear on a real
grammar, that case goes into the corpus** — together with what the message should have
said.

## 12. `Gram.gram` — the grammar of `.gram` written in `.Gram`

Write the grammar of our own language and compile it with our own generator. Not to
replace the hand-written front end, but for three things, each of which pays for
itself separately.

**An answer to whether the notation can describe itself.** `.gram` is a real grammar:
keywords with boundaries, comments, nested brackets, quantifiers, and exactly one
external recognizer (`@(...)`, where a C# lexer is required). If describing it is
awkward, that is a verdict on the notation, and it should be heard while changing it
is still cheap.

**A differential test.** Two independent implementations compared over a corpus of
grammars by their parse trees. It catches what no snapshot will.

An honest limit: this can compare **valid input only**. On broken input two different
algorithms legitimately differ — the hand-written one recovers to a declaration
boundary, the generated one looks for the cheapest edit. Diagnostics are tested by the
corpus (§11), not by this comparison.

**The most honest check on the code generator.** The diff of the generated file in
review shows exactly what changed in the emitter — on a real grammar rather than a toy.

### What it costs

Compiling `Gram.gram` needs a working `.gram` parser. At build time that is the
hand-written one, so there are two options:

- generate on every build — then the hand-written parser ships anyway;
- **keep the generated source in the repository** and refresh it with a command.

The second is Nitra's bootstrap in its mildest form: one generated `.cs` and a refresh
script, rather than two frozen stages and three `.cmd` files. Manageable, but the cost
should be admitted up front.

### The rule to hold from day one

**On a disagreement, `Gram.gram` is right**, and the hand-written parser is brought
into line with it.

Otherwise the familiar thing happens: the fix goes into C# because that is quicker,
the grammar falls behind, the differential test starts failing "for understandable
reasons" and gets muted. That is how dogfooding dies.

### Order

1. Take the hand-written front end as far as generation — otherwise there is nothing
   to compile `Gram.gram` with.
2. Write `Gram.gram`. The main thing is already here: either the notation describes
   itself comfortably or it does not.
3. A differential test over parse trees on a corpus, `Gram.gram` included.
4. Measure: speed, message quality, size of the code.
5. Only then decide whether to commit the generated parser and switch production to it.

## 13. What we do not take from Nitra

- **The AST layer**: `map syntax`, `ast`, dependent properties, symbols, scopes, name
  binding. That is a second and a third language on top of the first; for us that place
  is taken by C# (`syntax.md` §7).
- **Language composition**: `extend syntax`, dynamic extension points, resolving
  ambiguity between extensions. The source of most of the runtime complexity.
- **The bootstrap machinery**: Nitra's grammar is written in Nitra, and the repository
  holds two frozen stages plus `ShiftBoot.cmd`, `RebuildBoot.cmd` and
  `UpdateStage1Metadata.cmd`. Self-description we do take (§12), the chain of stages we
  do not: the front end stays hand-written and `Gram.gram` serves as a check.
- **Formatting markers** (`sm`, `nl`, indentation, block outlining) — one grammar
  yielding a printer and outlining as well. A good idea, but it widens the task beyond
  the current one.
