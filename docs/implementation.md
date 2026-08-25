# .Gram — the engine

Companion to [`syntax.md`](syntax.md). That document describes the language, this one
how to execute it. Nothing decided here is a decision about the language: if
something below turns out to be inconvenient, this is what changes, not the notation.

[`status.md`](status.md) says which parts are real and which are still plans; what is
marked a plan here is marked as one.

Several sections were removed rather than corrected, because what they described was
either never built, or was built and later superseded — and left as prose describing
neither would be worse than a gap in the numbering. Removed early: a fast recursive
parser beside a second one, the shape of the code that fast path would generate,
memoization by input position, and recovery as a search for the cheapest edit over a
whole document. What replaced all of that is this document's own §1 and `syntax.md`
§8.2 — one automaton, and a recovery mechanism scoped to a repetition rather than a
whole document. Removed
later, once each had either shipped in a different shape than planned or simply never
been reached: a memo-table sketch for incremental parsing, an unstarted plan to compile
this language's own grammar with itself, and a prototype build-order checklist whose
every reachable step is now done.

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

**Diagnostics come in two tiers, and both run inside the one automaton** (§1) — there
is no separate, slower engine behind them. The first is close to free: every place a
literal or an element test fails already knows the position and what it wanted, so
recording the furthest one reached costs one array write, live, on the same pass that
is recognizing anyway. It yields a message of the form "expected `)`" with an exact
place, built once the whole attempt has failed, from whatever survived — one message
per run, since parsing stopped at the first failure and there is no tree past it.

**A feed needs more than that, and `recover` (`syntax.md` §8.2) is the second tier**:
one bad record must not cost the message for every record after it. It is deliberately
narrow — one repetition, named in the notation, committing what it already took so a
later failure cannot un-take it — not a general repair pass over a whole document.
That case was tried and abandoned: a document-wide search for the cheapest edit that
makes broken input parse is a different kind of engine, one this project does not
build (`syntax.md` §11).

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

## 1. One automaton over the whole grammar

Every rule of a grammar is compiled into one C# method. A rule is not a method and a call
is not a call: each place a rule can be in becomes a labelled state, and moving between
them is `goto`. What a rule call leaves behind is an entry in an array — the arena — saying
where to carry on when the called rule is done.

The reason is `syntax.md` §4's promise that a rule call is transparent to backtracking. A C# method
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

The direction this points in goes further than removing entries one at a time: a
publication whose whole reachable grammar needs none of the arena's three uses — no
recursion, no backtracking, no deferred construction — is compiled as an ordinary
recursive method instead, with no arena, no state table and no dispatcher at all. A
grammar with even one rule elsewhere that still needs the arena still pays the whole
cost for every rule in it, since one automaton serves the whole grammar (`docs/next.md`
has the mechanism and what does not fit it yet).

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

It is in the code, in a shape of its own: `ParserEntry` (§1) is an all-integer struct —
positions, indices, no value field — so recognition can run to completion writing
nothing but those. Materialization is a separate pass, over what the arena holds once a
parse has accepted (`syntax.md` §7.3): one walk turns `Capture`/`RuleCapture` entries
into the typed values a rule's own `=>` needs, and calls it exactly once per rule, from
what the arena already recorded rather than by re-deriving it.

The alternative — a flat `int[]` holding the whole raw tree as offsets, materialized
lazily — buys the same property and costs memory proportional to the input, which
line-oriented streaming cannot afford (§7). Not taken.

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
(`syntax.md` §6.3). The compiler decides *whether* the reader mode is offered at all,
and both input types that ask for it end up in the same one.

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
(§1) actually stores while a parse is running, widened to a `long` only once, at the
one place a position crosses a publication's own boundary out to the caller.

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
