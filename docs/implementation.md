# .Gram — the engine

Companion to [`syntax.md`](syntax.md). That document describes the language, this one
how to execute it. Nothing decided here is a decision about the language: if
something below turns out to be inconvenient, this is what changes, not the notation.

[`status.md`](status.md) says which parts are real and which are still plans; what is
marked a plan here is marked as one.

Four sections were removed rather than corrected, because what they described was never
built and the engine that exists does the same work differently: a fast recursive parser
beside a second one, the shape of the code that fast path would generate, memoization by
input position, and recovery as a search for the cheapest edit. What replaced all four is
§1.

References to **Roc** are to an earlier unpublished project of my own, a BNF macro for
Nemerle. Most of what is in the code today came from there — normalization done before
anything else, the normalized grammar rendered back as text so that a change to a fold
shows up as a diff, the element-set merge of §5 — along with two of its mistakes, which
are here as deliberate non-features: alternatives are never reordered (§5), and rule
references are never inlined. It is named rather than linked because there is nothing
to link to.

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

**Repairing a document has no notation** for the same reason. The quality of messages
about a source file must not depend on whether the author placed synchronization
points; place them wrongly and it gets worse than having none. A feed is the other
case and does have notation — `recover`, `syntax.md` §8.2 — because there the answer
wanted is not a repair.

**There is no commit point in an expression**, and diagnostics are why. Early
commitment existed to stop a real error inside an alternative from being discarded in
favour of "nothing matched" at the top; the recovery engine does that job better, from
the cheapest edit, and an operator scattered through expressions costs more than it is
worth — the same alternative would mean different things depending on where it sits.
`recover` commits on one repetition instead, which is the case the recovery engine
cannot serve: a hundred million records, of which one is bad.

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

## 1. One automaton over the whole grammar

Every rule of a grammar is compiled into one C# method. A rule is not a method and a call
is not a call: each place a rule can be in becomes a labelled state, and moving between
them is `goto`. What a rule call leaves behind is an entry in an array — the arena — saying
where to carry on when the called rule is done.

The reason is §11's promise that a rule call is transparent to backtracking. A C# method
cannot be suspended and resumed, and resuming is exactly what it means to come back into a
rule and take a different alternative. Once the continuation lives in an array rather than
on the machine's stack, that is possible — and so is recursion deeper than the stack
would allow, which is the same fact seen from the other side.

### What the arena holds

Three unlike things, and telling them apart is most of what the engine's correctness rests
on:

```text
frames          Call, Completed        where a rule was called from
ways back       Choice, Repeat, Run    where the parse could return to
                Lookahead, Atomic
derivation      Capture, RuleCapture   what was recognized, for building values with
                Construct
```

A failure unwinds by taking entries off the end until it finds a way back. A commit — what
`{ }` does — puts out the ways back inside it and leaves everything else. Materialization
walks what is left and runs the constructions.

**An entry's index is its name.** A capture of a rule's value holds the index of the entry
its call completed into; one materialized value names the next. So nothing may renumber the
entries around it: a commit puts a way back out where it lies rather than removing it, and
a repetition compiled without entries keeps its position in a local instead. Two defects
have come from breaking this rule and both looked like something else at first.

### The arena is a mechanism, not a tax

Everything above describes what is needed when a parse can be resumed. Most of a grammar
cannot be, and the compiler is expected to prove it and write something cheaper. What it
proves today:

- **A rule outside every call cycle that produces no value** is compiled into its callers.
  Its expansion terminates because the call graph beneath it is a DAG, and what the
  duplication costs is text.
- **A choice whose alternatives cannot begin with the same character** needs no entry: one
  character decides which it is, and having decided there is no second reading. Where only
  some alternatives are ruled out, the entry is written only if one of the rest could still
  match.
- **A repetition whose body matches one way only, and which is followed by something the
  body cannot begin with**, is run to its end and never asked to give any of it back. Every
  place it could stop short is a place a turn began, so the character there is one the body
  starts with, and the continuation cannot start with it.
- **Where such a body also writes nothing to the arena**, the whole construct is a loop:
  no entry, no count, no way back, and its required turns written out rather than counted.
- **Text alternatives none of which begins another** are decided where they differ, reading
  what they share once and moving the position only when one has matched whole.

The first sets these rest on are approximate in the direction that says "anything" when
unsure, so a proof that cannot be made is not made and the general machinery stays. What
follows a rule is the union over its call sites, computed as a fixed point over the call
graph; a `parse` publication contributes the end of the input, which is a fact and not a
silence.

The direction this points in is worth stating: the arena should be what a grammar gets when
resumability is required, not what every grammar pays for the language having it. The
analyses above remove entries one at a time; the next step is to remove arena-backed
execution from whole regions, and after that from whole parsers.

### Values are built afterwards

Recognition records; construction runs on what was accepted (§3). Values live in a table
for each type the grammar can produce rather than one table of `object?`, so nothing is
boxed on the way in or cast on the way out. An extent — `: @SourceSpan` — is not stored at
all: the entry the rule completed into already holds where it began and where it reached.

### What is written out

The state table is planned before a character of it is emitted. A state whose whole body is
a jump is followed to wherever it ends and not written; everything that pointed at one is
made to point past it. What cannot be reached from a publication is not written either — a
rule compiled into all of its callers is called from nowhere, and its own copy is text
nothing will arrive at. The dispatcher goes before the states rather than after them,
being the block every return and every resumption comes through.

## 3. Nothing is built while matching

**One idea, taken and worth stating on its own: a match allocates nothing, and the
typed result is materialized once it has succeeded.** Speculative parsing then costs
almost nothing — backtracking restores a position and has nothing to undo — which is
what makes ordered choice affordable without a commit point.

It is in the code, in a shape of its own. A capture records a pair of positions into
the input; every state a match can resume at clears the slots an abandoned attempt
could have written, as literals worked out while generating; and the value is built by
one expression at the accepting state. `RecognitionResult<T>` is a struct with a
discriminant field, and records are built on the way out rather than along the way.

The alternative — a flat `int[]` holding the whole raw tree as offsets, materialized
lazily — buys the same property and costs memory proportional to the input, which
line-oriented streaming cannot afford (§7). Not taken.

The recovery engine is the one piece that is both grammar-independent and large enough
for duplicating it into every assembly to be noticeable. It is also the only candidate
for someday justifying the shared mode of §6.1 by volume of code rather than by types.

## 5. Filtering alternatives by their first element

Each alternative has the bounds of its first character computed, and an alternative is
not tried at all when the current character falls outside them:

```csharp
if (lower <= c && c <= upper)
```

Cheap, computed when the grammar is built, and it removes most of the cost of ordered
choice — which matters more now that ordered choice backtracks fully and no operator
cuts it short (§7). Most alternatives never get tried at all.

What makes it cheap is normalization done first, and Roc's macro is where to take that
from — an unpublished Nemerle BNF macro, whose fold was: single-character alternatives
and ranges are separated out, sorted by first character, then merged — `'a' | 'b'`
into `'a'..'b'`, a range absorbing anything it contains, duplicates dropped. After
that an alternative's first-character bounds are already computed.

**What not to take from there is the reordering.** Roc moves the single-character
alternatives ahead of everything else, which silently changes ordered choice:
`"ab" | 'a'` becomes `'a' | "ab"` and the second is then unreachable. It never bit
because Roc's structural generator was a stub and its character
tests compile to `c == 'a' || …`, where order cannot matter — the multi-character case
was never executed. Merging is safe exactly where the match length is fixed at one
item; beyond that it is a diagnostic, not a rewrite (`syntax.md` §11).

## 7. Execution modes, and what bounds retention

Which mode a parse runs in is decided by the type of the input, at the call site
(`syntax.md` §6.2). The compiler decides *how* each mode is implemented.

```text
in memory      string / ReadOnlySpan<char>
               full backtracking, the whole input addressable throughout

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
responsible. Note what this does and does not do — it restricts what may stream, and
changes the meaning of nothing.

`recover` (`syntax.md` §8.2) reaches the same bound from the other side, by being told
rather than by inference: its synchronization expression is a point the parse cannot
return past, so a marked repetition streams by construction and the analysis has
nothing to prove. It commits as well, which the analysis does not — but on one
repetition, named in the notation, and the rules it calls mean the same thing inside it
as anywhere else. That is the whole difference from the commit point `syntax.md` §11
refuses: an operator scattered through expressions would make one alternative mean
different things depending on where it was written.

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

Anything indexed by absolute position assumes memory-sized input: such a table costs tens
of gigabytes on a ten-gigabyte feed. In line-oriented mode what the parser keeps is
per-line and reused, which is what makes that mode cheap rather than merely possible.

## 8. Incremental parsing

Thirty lines over the memo table of §4:

1. compare old and new text from the start — the length of the common prefix;
2. compare from the end — the length of the common suffix;
3. carry over whatever was already established past the edit, shifted by the difference
   in lengths;
4. parse again, landing in what was carried over.

Only the **tail** is worth reusing; the head is recomputed. For an editor that is
enough — an edit is usually in the middle, and the tail is the longer part.

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

Keyword boundaries want to be declarative in the same way — a class of keyword
characters plus a separator rule, after which every string literal falling into that
class gets the boundary check automatically. That remains open (`syntax.md` §11).

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
3. The flat representation — immediately, because it determines the shape of the
   generated code rather than optimizes it afterwards.
4. Filtering by first element — right after, since full backtracking makes it the main
   thing keeping ordered choice cheap (§5).
5. Second-tier diagnostics — recovery that carries on past a bad element, once
   recognition works.
6. The line-oriented mode (§7) — retention analysis, then the reused buffer. Feeds do
   not work without it, and it is far simpler than the windowed mode it replaces.
7. Incremental parsing last; whatever it reuses between runs, it changes nothing about
   how a single run recognizes.

Check against the three scenarios with the widest coverage: a calculator (recursion
and levels), a feed (sequence results and recovery), a URL (shared literal prefixes).

### Two tiers of diagnostics

The recovery engine does not have to be pulled forward whole — message quality comes
in two tiers of very different cost, and the first is nearly free.

**The first comes out of the fast path**, and it is the standard answer for a parser
that backtracks: remember the furthest position of failure reached and the set of what
was expected there — at that position, try every terminal of the grammar and keep those
that would have fitted. That yields a message of the form "expected `)`" with an exact
place.

It costs almost nothing: the position is tracked anyway, and the expected set is known
from the grammar at build time. But it yields **one** message per run — parsing was
abandoned at the first failure, so there is no tree.

**The second is recovery.** It gives what the first cannot:

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

The second is bootstrapping in its mildest form: one generated `.cs` and a refresh
script, rather than a chain of frozen stages. Manageable, but the cost should be
admitted up front.

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
- **A chain of bootstrap stages.** Self-description is taken (§12), the chain is not:
  the front end stays hand-written and `Gram.gram` serves as a check against it.
- **Formatting markers** — soft breaks, indentation, block outlining, so that one
  grammar yields a printer and code folding as well. A good idea that widens the task
  well beyond the current one.
