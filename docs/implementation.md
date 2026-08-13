# .Gram — the engine plan

Companion to [`syntax.md`](syntax.md). That document describes the language, this one
how to execute it. Nothing decided here is a decision about the language: if
something below turns out to be inconvenient, this is what changes, not the notation.

Most of it is a plan rather than a report. [`status.md`](status.md) says which parts
are real; of the engines described below, today, none.

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

The second engine starts only if the first returned a negative result or stopped short.
On correct input recovery costs nothing.

The consequence for the language: **repairing a document needs no notation**. The
author writes neither policies nor synchronization points, and the second engine is
reached only when the first has already failed over the whole input.

That covers a source file and not a feed. A hundred million records cannot be held
while the cheapest edit is searched for, and "what did the author most likely mean" is
not the answer wanted for record twelve of them — "it is bad, here is why, carry on"
is. That case is `recover` (`syntax.md` §8.2): it runs inside a **successful** parse,
per element, and never reaches this engine. The two share the word and nothing else.

Both engines are built from one description of the grammar: the fast one as generated
code, the slow one as a state machine.

## 2. The fast path: the shape of generated code

One recognizer per rule, with one signature:

```csharp
static int Recognize_R(ReadOnlySpan<char> text, int pos);   // new position, or -1
```

Flat, no exceptions, no objects, no delegates — what `syntax.md` promises with the
words "code comparable to a careful hand-written parser". That flatness is the idea
worth taking from a generated parser; the shape that achieves it here is our own.

Inside a recognizer there are no nested calls but a state machine: states are `switch`
sections, transitions are `goto case`, and the points a match could have gone another
way are an explicit stack of three-int frames. That is what makes backtracking full
inside a rule, and it is the one part of the engine that is built (`status.md`).

`-1` as the failure signal is a placeholder: the language needs an outcome that tells
"no match" from "error" (`syntax.md` §8.1), and that is what it becomes.

**There is no runtime assembly.** Everything a generated parser needs is emitted into
the same assembly, `internal` (`syntax.md` §6.1). A consumer takes one analyzer
package. It follows that nothing which must be shared between assemblies can appear
here — and if something does, it goes into the optional shared mode
`[assembly: GramRuntime]` rather than into a dependency.

Parameterized rules are specialized per call site (`syntax.md` §4.2), so a recognizer
parameter disappears during generation and becomes a direct call.

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

## 4. Memoization

The table is indexed by **position in the input**; each position holds a linked list of
results for different rules, the head in the table and a `Next` field for the
successor.

This stays an execution strategy rather than a guarantee of the language (`syntax.md`
§7.2: code in `@(...)`, `where` and `=>` must be safe to invoke repeatedly whether or
not anything is cached).

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

## 6. Recovery as the cheapest edit

The slow engine enumerates edits of the input — insert what was expected, delete what
was not — and picks the solution of least total cost. Minimum-distance error correction
is old (Aho and Peterson, 1972); what has to be got right is the bookkeeping.

- the cost is a pair, inserted and deleted;
- a priority queue on cost, then on position;
- the loop: parse to the point of failure → try insertions → try deletions → repeat
  until a solution is found;
- a timeout with graceful degradation: if it does not finish in time, delete the rest
  and stop.

To be judged on a grammar the size of C#, not on the corpus of §11.

## 7. Execution modes, and what bounds retention

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

Note that §4 assumes memory-sized input: a memo table indexed by absolute position
costs tens of gigabytes on a ten-gigabyte feed. In line-oriented mode
both are per-line and are reused, which is what makes that mode cheap rather than
merely possible.

## 8. Incremental parsing

Thirty lines over the memo table of §4:

1. compare old and new text from the start — the length of the common prefix;
2. compare from the end — the length of the common suffix;
3. copy the tail of the memoization table, shifted by the difference in lengths;
4. parse again, landing in ready entries past the edit.

Only the **tail** is worth reusing; the head is recomputed. For an editor that is
enough — an edit is usually in the middle, and the tail is the longer part.

## 9. Operator precedence

Precedence climbing: atoms and prefix operators in one loop, infix and postfix in
another, with a binding power per level.

`syntax.md` §4.3 has the levels written out as rules, which works with no engine at
all, and associativity carried by which side a rule recurses on. Direct left recursion
is rewritten into a repetition plus a fold, which needs no engine either.

The engine is what a precedence *table* would need — and only because a table admits
`E = E & '+' & E`, which the rewrite cannot take. If such a construct appears later it
should be lowered into this shape rather than into a third one.

## 10. Trivia and keywords

The usual way to place whitespace is an invariant — every rule consumes the whitespace
after itself and never before — plus rules for where to insert accordingly: after a
literal and after a lexical rule, not after a structural one, once at the start, and
attributes to override either way.

**We need none of it.** `syntax.md` §4.5 requires `Trivia` to be nullable, and from
that condition unconditional insertion is safe: a second application consumes nothing,
so nothing is ever doubled. The whole rule collapses into "insert everywhere", with one
insertion at the start of a published rule for leading whitespace, and normalization
drops the insertions entirely when `Trivia` is empty.

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

**The first comes out of the fast path**, and it is the standard answer for a parser
that backtracks: remember the furthest position of failure reached and the set of what
was expected there — at that position, try every terminal of the grammar and keep those
that would have fitted. That yields a message of the form "expected `)`" with an exact
place.

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
