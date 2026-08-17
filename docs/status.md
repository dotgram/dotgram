# Implementation status

[`syntax.md`](syntax.md) describes the language. It is a specification, not a report:
it says what `.Gram` is, in the present tense, whether or not the compiler does it yet.
This file is the report.

Read a row as a pipeline. A construct is only usable when it survives all the way
across — the failures worth knowing about are the ones that parse, bind, normalize and
then quietly mean nothing.

| | Parse | Bind | Normalize | Emit | Runs |
| --- | :-: | :-: | :-: | :-: | :-: |
| character and string literals | ✓ | ✓ | ✓ | ✓ | ✓ |
| element sets, ranges, complement | ✓ | ✓ | ✓ | ✓ | ✓ |
| Unicode categories `\p{Lu}`, groups `\p{L}` | ✓ | ✓ | ✓ | ✓ | ✓ |
| references to elementary rules in a set | ✓ | ✓ | ✓ | ✓ | ✓ |
| sequence `&` | ✓ | ✓ | ✓ | ✓ | ✓ |
| ordered choice `\|` | ✓ | ✓ | ✓ | ✓ | ✓ |
| quantifiers `? * + {n} {n,m}` | ✓ | ✓ | ✓ | ✓ | ✓ |
| lookahead `?=` `?!` | ✓ | ✓ | ✓ | ✓ | ✓ |
| a capture of what a lookahead saw §3.4 | ✓ | ✓ | ✓ | ✓ | ✓ |
| rules calling rules, recursion | ✓ | ✓ | ✓ | ✓ | ✓ |
| scopes, `using`, shadowing | ✓ | ✓ | ✓ | ✓ | ✓ |
| the same rule name in two scopes | ✓ | ✓ | ✓ | ✓ | ✓ |
| standard library `any none eol eof` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `Trivia` by shadowing | ✓ | ✓ | ✓ | ✓ | ✓ |
| publication `parse` and `find` §6 | ✓ | ✓ | ✓ | ✓ | ✓ |
| the position a refusal names | — | — | — | ✓ | ✓ |
| captures `name:` | ✓ | ✓ | ✓ | ✓ | ✓ |
| repeated captures of a rule, `items: Row*` | ✓ | ✓ | ✓ | ✓ | ✓ |
| construction `=>` at the end of a rule | ✓ | ✓ | ✓ | ✓ | ✓ |
| construction `=>` per alternative | ✓ | ✓ | ✓ | ✓ | ✓ |
| rule types `: @T` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `: @string` as the extent §4.1 case 4 | ✓ | ✓ | ✓ | ✓ | ✓ |
| `: @SourceSpan` as the bounds §4.1 case 4 | ✓ | ✓ | ✓ | ✓ | ✓ |
| publishing a rule whose value is one of ours, refused §6.1 | — | — | ✓ | — | — |
| rule types naming another rule §4.1 case 3 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a sequence result `: T[]` §4.1 case 2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| the same collecting operands inside a group §4.1 case 2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| an operand of one captured by hand, reported §4.1 case 2 | — | — | ✓ | — | — |
| guards `when` §8.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| inline C# `@(...)` in `when` and `=>` | ✓ | ✓ | ✓ | ✓ | ✓ |
| C# names inside `@(...)`, e.g. `@int.Parse` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `[@Name]` as an element predicate §7.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| bare `@Name` as a recognizer over a span §7.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| the same handing back a value of its own §7.1 | ✓ | ✓ | refused | ✗ | ✗ |
| direct left recursion §4.3 | ✓ | ✓ | ✓ | ✓ | ✓ |
| binding powers `<< n` `>> n` §4.3.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| indirect left recursion | ✓ | ✓ | refused | ✗ | ✗ |
| parameterized rules §4.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a numeric argument, `Digits(4)` §4.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a value parameter `n: int` given a number §4.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a value parameter given anything else §4.2 | ✓ | ✓ | refused | ✗ | ✗ |
| a sequence result naming a parameter, `: item[]` §4.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| one capture name over rules of one declared type §7.3 | — | — | ✓ | ✓ | ✓ |
| the scalar form of it, `: item` §4.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| keyword boundaries §4.6 | — | ✓ | ✓ | ✓ | ✓ |
| `recover` on a repetition, with `=>` §8.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| the names `recover` supplies §8.2 | — | — | — | ✓ | ✓ |
| offsets are `long`, extents are `int` §6.3 | — | — | — | ✓ | ✓ |
| `recover` without `=>`, dropped and reported §8.3 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a second `recover` in one rule, a stage each | ✓ | ✓ | refused | ✗ | ✗ |
| a `=>` that throws inside `recover` leaves the parse §8.2 | — | — | — | ✓ | ✓ |
| C# recognizer calls emitted without generator resolution §7.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a value type generated for a rule that has none §7.3 | — | — | ✓ | ✓ | ✓ |
| captures matched to an existing type's constructor §7.3 | — | ✓ | ✓ | ✓ | ✓ |
| captures matched to `init`/`required` properties §7.3 | — | ✓ | ✓ | ✓ | ✓ |
| a C# type named beside the grammar, nested in the host | — | ✓ | ✓ | ✓ | ✓ |
| a declared type found under the grammar's `@using` §7.3 | — | — | ✓ | ✓ | ✓ |
| C# in `=>` and `when` emitted without generator resolution §7.4 | ✓ | ✓ | ✓ | ✓ | ✓ |
| `#line` from the generated file back to the grammar §7.6 | — | — | ✓ | ✓ | ✓ |
| `RecognitionResult<T>`, `Outcome`, `Diagnostic` §7.5 | — | — | — | ✗ | ✗ |
| document repair, §6 of the engine plan | ✗ | ✗ | ✗ | ✗ | ✗ |
| leading and trailing `Trivia` §4.5 | — | — | — | ✓ | ✓ |
| `Trivia` between operands and not between iterations §4.5 | — | — | ✓ | ✓ | ✓ |
| a C# name as an argument of `@M(…)`, behind `@` §7.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| retention: what a rule takes, in lines §6.3 | — | — | ✓ | — | — |
| retention: where the window may move §6.3 | — | — | ✓ | — | — |
| `find` over a `TextReader` §6.3 | — | — | ✓ | ✓ | ✓ |
| `parse` over a `TextReader` §6.3 | — | — | ✓ | ✓ | ✓ |
| a streamed feed of records of differing lengths | — | — | — | ✓ | ✓ |
| `recover` stepping over a bad record in a stream | — | — | — | ✓ | ✓ |
| a repetition that cannot tell its own end §6.3 | — | — | ✓ | — | — |
| a repetition of something other than a rule §6.3 | — | — | ✓ | — | — |
| `IEnumerable<string>` input §6.3 | — | — | — | ✓ | ✓ |
| the §8.3 hook over a streamed parse | — | — | — | ✓ | ✓ |
| incremental parsing | ✗ | ✗ | ✗ | ✗ | ✗ |

## Backtracking, and where it stops

Inside a rule, backtracking is full. A rule compiles to a state machine with an
explicit stack of the points that could have gone another way: entering an alternative
records the next one, taking one more repetition records the option of having stopped.
Failing anywhere resumes at the most recent of them, and nothing is given up until the
stack is empty. So `'a'? & 'a'` matches `"a"`, and `("x" | "xy") & 'y'` matches `"xy"`.

**Backtracking does not cross a rule boundary**, and that is now language rather than
implementation — §4 of the specification says so and says why. A call answers once, with
the first match it finds, and cannot be asked for another:

```dotgram
Start = Name & 'y'
Name  = "xy" | "x"
```

does not match `xy`, though `("xy" | "x") & 'y'` does.

The example was wrong here for a long time — it had the alternatives the other way round,
as `"x" | "xy"`, which matches perfectly well because the shorter one wins and `'y'` takes
what is left. Two tests now pin both orderings, which is how the mistake surfaced.

The same boundary shows up in publication: `parse R` asks `R` for a match and then
checks the input ended, and cannot send `R` back for a longer one if it did not.

**Nesting depth is bounded by the process stack, and the bound is about 4560 in
Release.** The
machine takes recursion out of a rule and not out of a grammar: `Expr = '(' & Expr & ')'`
is an ordinary C# call, so a thousand brackets are a thousand frames. Measured on the
default 1 MB stack, `((( … x … )))` survives 4562 levels and overflows by 4625 — bisected
by a child process, because a `StackOverflowException` cannot be caught and takes the
process with it (`benchmarks/ … --depth N`). The 2700 written here before was measured in
Debug, where frames are fatter; both numbers were right about different builds, which is
why the build is now part of the claim.

The number is what it is for a reason worth knowing, because it is a cost of a decision
made elsewhere. Each recognizer opens with

```csharp
global::System.Span<int> bt = stackalloc int[48];
```

— the backtracking stack, sized so that nothing is allocated on the heap in the common
case and `Grow` takes over when 48 is not enough. That is 192 bytes of the C# stack per
rule invocation, and it, not the rest of the frame, is what sets the depth. `Grow` helps
with backtracking *inside* a rule and does nothing for nesting *between* rules.

**48 was tried against 32 and 24**, since the buffer is what a frame mostly costs and a
smaller one buys depth:

| slots | depth | one URL parse | allocated |
| --: | --: | --: | --: |
| 48 | 4562 | 241 ns | 760 B |
| 32 | 5625 | 296 ns | 1448 B |
| 24 | 6421 | 274 ns | 1408 B |

The trade is bad in both directions of reading it. A quarter more depth costs twice the
garbage on every parse, because the URL grammar genuinely needs more than 32 slots and
`Grow` starts running on ordinary matches — the allocation is not the stack buffer, it is
the heap one replacing it. 48 is where an ordinary parse stops spilling, and buying depth
past that means paying for it on every match rather than on the deep ones.

**Spilling the buffer to the heap when the stack runs low was tried and does not work
in the obvious form.** `RuntimeHelpers.TryEnsureSufficientExecutionStack()` asks whether a
fixed reserve is left — on this runtime a hundred kilobytes or so — so it keeps answering
yes until the last few hundred frames and then answers no for the rest. Measured: the
limit did not move. The check is a guard against being about to overflow, not a way of
deciding early that a parse is going deep.

What would work is a depth counter: pass the nesting level down, and past some threshold —
a few hundred, which no ordinary grammar reaches — take the buffer from the heap instead
of the stack. A frame without its buffer is about 40 bytes rather than 230, so the limit
would go from 4562 to something in the tens of thousands, and an ordinary parse would
never test the branch more than a few times. The cost is a parameter on every recognizer
and an increment at every call, which every snapshot would show. Not done yet, and worth
doing before anything parses adversarial input.

So input length and nesting depth are different limits, and only the first is about to
get better: streaming makes a longer file readable and leaves the bracket count exactly
where it is. A grammar meant for adversarial input should bound its own nesting, and a
`StackOverflowException` cannot be caught in .NET — the process goes.

**What `recover` recovers from is an element that started.** A run of `Row*` ends when
`Row` does not begin, and zero further iterations is a legitimate outcome for `*` —
nothing tells that apart from a run that simply finished. So a line the element cannot
start at all ends the run, and what follows is asked to match from there:

```dotgram
Start = rows: Row* recover eol => @(…) & eof
Row   = t: ['a'..'z']+ & eol
```

reads `aa
ab1
cc
` — `ab1` begins as a row and breaks part way through, which is what
a malformed record looks like — and refuses `aa
1bad
cc
`, where `1bad` never began
one. Both are pinned by tests.

This is worth knowing before writing a feed grammar, and it is why the examples give
their records a distinguishing prefix: `Row = "R" & '|' & …` cannot be mistaken for the
end of the run, so anything starting `R|` and failing afterwards is a broken record
rather than a trailer. A grammar whose records begin with the same characters as whatever
follows them has told the parser nothing to recover with.

**A repetition marked `recover` is possessive.** §8.2 calls the mark a commit point,
and this is what that costs: the elements it took are not on offer to what follows.

```dotgram
Row   = name: ['a'..'z']+ & eol
Start = rows: Row*                        & tail: ['a'..'z']+ & eol   // matches "aa\nbb\n"
Start = rows: Row* recover eol => @(…)    & tail: ['a'..'z']+ & eol   // does not
```

Unmarked, the repetition takes both lines, fails on `tail`, and hands the second one
back. Marked, it does not: an element it took was either read or explicitly rejected,
and there is no shorter reading to come back for. That is also what keeps *did an
element begin here* answerable — the question is asked where the repetition would
otherwise have ended, and it is answered by how far the attempt starting there reached.
With the iterations still on the stack, a failure after the repetition would resume at a
position whose element had matched and be told one broke there.

## What a rejection is told

All seven names of §8.2 are supplied. Three differ from the specification in ways worth
knowing:

- **`parserText` and `parserSpan` stop where the synchronization point begins.** `eol`
  separates the elements and is not part of one, so a rejected `b1b\n` is three
  characters, not four.
- **`parserMessage` is not the expected set.** It says which rule the element should
  have been and where the input stopped being one — `Input does not match 'Row' at 43.`
  The set of what could have appeared there would say more, and is not carried yet.

Which of the seven a factory asked for is read out of its C#, because §8.2 has counting
lines cost a scan and only a factory that named `parserLine` should pay for one. The
reading is a whole-word search over the text, so it over-approximates: `parserLine`
inside a string literal counts as asked for. That direction is the safe one — a name that was written is
always found, and a name that was not costs an unused parameter. Reading it exactly means
lexing C#, which is the host's job and not the grammar half's.

**Without a `=>` the element is dropped and reported to a `partial void`**, which is
§8.3's fourth row — successful records only, failures to a log, nothing declared. The
generated class declares the channel and the consumer may implement it:

```csharp
static partial void OnRecovered(
    string rule, string text, int position, int line, int column, int ordinal, string message);
```

The classic C# 3 form, not the C# 9 one: an implementation is optional, and where there
is none the compiler removes the declaration, every call to it, **and everything in the
argument lists**. So the element's text is never materialized and its line never counted
unless somebody is listening — which is why every argument is an expression and none is
computed into a local first, and why `LineAt` and `ColumnAt` are two functions rather
than one method with two `out` parameters. A test compiles a grammar with no implementing
half and asserts the method cannot be found on the type at all, so the erasure is checked
rather than assumed.

It is also why this is a `partial void` and not an event, a delegate or an `ILogger`:
those cost something even when null, and the premise of a streamed feed is that nobody is
usually listening.

The cost is that the hook is static and per host class, so what it reports cannot be
scoped to one call, and one hook serves every recovering rule in the grammar — hence the
`rule` parameter. `LoggingFeedExample` shows the ordinary way round the first: gather into
a `[ThreadStatic]` for the duration of a read.

**A `=>` that throws is not caught, inside a recovering repetition or anywhere else.**
This was the one place the specification and the code disagreed, and it was settled by
changing the specification: §8.2 used to promise the throw would be caught and treated as
a failed parse outcome. Catching would mean catching `Exception`, because no type tells
"this quantity is not a number" from `NullReferenceException`, and a parser that
reports a bug in the author's own C# as "row 400 was malformed" is worse than one that
stops. The generator does not infer a parser outcome from the signature of the C# called
by `=>`.

`DecimalCalculator.Evaluate("1 . 5")` in the examples throws `FormatException` out of a
`decimal.Parse` in a `=>`, and a test asserts exactly that.

## Associativity

Direct left recursion works and means left-associative, as §4.3 says. `R = left: R & op
& right | base` is rewritten into `base & (op & right)*`: the leading self-call is what
makes an alternative recursive and what the rewrite takes away, and `left` stops being
a capture and becomes the value built so far, which that alternative's own `=>`
receives.

The loop is an ordinary repetition, so backtracking, forgetting and the rest apply to
it unchanged.

**Nothing decides associativity, and nothing computes it.** The only structural question
asked of an alternative is the one that has to be asked — does it begin with a call to
its own rule, which cannot be compiled as written. Everything else is an ordinary call,
and how it groups falls out of the order the calls return in. So in a grammar of levels
the author says associativity by choosing which operand is parsed at the rule's own
level:

```dotgram
Sum   = left: Sum     & op  & right: Product   // left at this level  → left-associative
Power = left: Primary & '^' & right: Unary     // right at this level → right-associative
```

`Unary` is the looser level and comes back down to `Power`, so the right operand of `^`
can be another `^` and the left one cannot. There is no "right-recursive" in the
compiler because there is nothing for it to do: `Power` never calls `Power` at all.

**Nothing is built while matching, folds included.** What a match records is a number:
which alternative it came through, and — for a chain — which step followed which. Both
ride on the backtracking frame, so an alternative or a step given back is forgotten with
everything else it did, and the factories run at the accepting state in the recorded
order.

That is what lets a rule have as many recursive alternatives as it likes. Accumulating
built values instead would need one type to hold them all, which would have capped a
rule at one — and a postfix chain wants three: member access, call, index. What is
collected instead is each alternative's own captures, one entry per iteration, which
needs no common type at all.

Refused: indirect left recursion, which has arbitrarily many shapes to rewrite; a rule
whose every alternative is left-recursive, which has nothing to start from; and an
alternative recursive on both sides, which ordered choice cannot settle — `-1-2` under
`E = E & '-' & E` answered 1 rather than -3 until that was checked.

## Binding powers

Built, and they are the same rewrite. `E << 1 | E >> 3 | …` folds into `bases &
(tails)*` exactly as §4.3 does, and two numbers ride beside it: which strength a tail may
be entered at, and which strength its own operand is parsed at. The recognizer takes the
first as a parameter and tests it before matching anything of an alternative; the second
is a constant at the call site, `n + 1` for `<<` and `n` for `>>`. That difference of one
is the whole of left against right.

Only a rule that says `<<` or `>>` takes the parameter. A grammar that never reaches for
them is generated exactly as it was before they existed, which is why no snapshot moved.

**An alternative recursive on both sides is the ordinary case here**, and refused under
levels. The refusal is not about the shape — it is that ordered choice has nothing to
settle the grouping with. A strength is exactly the missing information, so the same
`left: E & op & right: E` is a diagnostic in one convention and the point of the other.

**A strength is not symmetric, and that is what makes one number enough.** It says only
what the operand to the *right* is read at. A prefix has no left operand, so it is a base
— one of the alternatives that start an expression — and a base is entered whatever
strength was asked for, because there is nothing to its left for anything to bind more
tightly than.

So the asymmetry that looks as though it needs two numbers does not. Python's `**` binds
tighter than unary minus on its left (`-2**2` is `-4`) and looser on its right (`2**-1`
parses); levels say that by naming two different rules either side of it, `left: Primary
& '^' & right: Unary`. Strengths say it by giving `^` and unary minus the *same* number.
`examples/` has the same calculator both ways and a test that runs them against each
other expression by expression, that pair included.

Refused with `GRAM4009`: a rule with a strength on one recursive alternative and none on
another (§4.3.1 — one convention or the other), and a strength on an alternative with no
operand of its own to read at it.

## What a publication answers with

`TryParseR` hands back a `Match<T>` — value, error, position, length — and takes no
`out` parameters. `FindR` hands back a lazy `IEnumerable<Match<T>>`, so "the first
one" and "the ones that satisfy this" are LINQ's rather than more directives.
`match` and `find all` are gone: one word meant three different things across
ecosystems, and the other was a sequence method wearing a directive's clothes.

The position it reports was always zero until now. It is now the furthest the input
could be followed before the match gave up — which, for a parser that backtracks, is
the only position worth naming: the last thing tried is usually shallower than the
best thing tried. On a match it is instead where the match began.

Every recognizer takes a `ref Failure` and raises `Position` at the one place a machine
gives up on where it is. Nothing is paid on the path that matches, a rule call carries
its callee's failure out with it, and a lookahead is the one machine that does not take
the state — how far it looked before answering "no" is not how far the parse got.

Two things it does not do yet, and both fit where it stands rather than replacing it:

- **the position is where the failing operand began**, not where its first wrong
  character is. `"abcd"` against `abXY` names 0, not 2. Sharpening it means recording an
  offset at each failing test instead of one position at the point of giving up.
- **nothing says what was expected there.** That is a second field on the same struct,
  which is why the struct is threaded by `ref` rather than returned: `Expected`, and the
  outcome that tells a malformed record from no record (§8.1), go in beside `Position`
  without changing a single signature.

## Captures, and what they build

A rule with captures gets a type of its own — `public sealed class`, nested in the host
class, one get-only property per capture name — and every published method hands that
back instead of a `string`. A rule without captures is unchanged: its value is the text
it matched, and its recognizer has the signature it always had.

A capture holds `(start, end)` into the input, or the value of the rule it names when
that rule builds one. The whole value is constructed once, at the accepting state, from
one expression. Nothing is built during the match, so an attempt that is abandoned
costs nothing to undo — and any C# a grammar supplies will run on the parse that
actually happened rather than once per attempt.

**Backtracking forgets what it captured.** Slots are numbered in the order the notation
writes them, which makes "everything written since this point" a suffix of them; each
state a match can resume at clears that suffix, as literals worked out while
generating. There is no journal and no marks, and the path that does not backtrack pays
nothing.

**A repeated capture collects.** `items: Row*` is a `Row[]`, appended to where each
value is built and truncated back when an attempt is abandoned. The length at the
moment of a push rides on the backtracking frame, which is what makes it exact even for
a repetition inside a repetition: giving back an outer iteration truncates to what the
inner ones had collected before it began. No iterations is an empty array, never null.

A repeated capture of **text** is a different thing and stays one: §10 binds a capture
tighter than a quantifier, so `digits: ['0'..'9']+` is one capture repeated, and §7.3
gives it the text joined — which it produces as the extent of the whole run.

Two things a capture can still be that are recognized and not built, `GRAM4006` rather
than a silent drop:

- a capture of *text* inside a repetition without being the whole of what repeats — the
  text of the iterations could not be told from the text between them.
- a capture inside a lookahead, which is a machine of its own that answers yes or no.

And `GRAM4007`: one name captured twice with different types.

## A rule that names its own type

`R : @T = … => @Expression` works. The type is written into the generated file exactly
as the grammar wrote it, the grammar's `@using` directives are carried in beside it,
and whether the name resolves is C#'s question to answer on the grammar's line — which
is why none of this needed symbol resolution.

The `=>` becomes a method, and the captures are its parameters:

```csharp
static int Construct_Number(string parserText, string digits) =>
    int.Parse(digits, CultureInfo.InvariantCulture);
```

A method rather than an expression written where the value is assigned, and that is
what makes the capture names usable at all: inside a recognizer they would have to
dodge every local it has, and a capture called `p` or `state` would collide with the
machine itself. `parserText` is supplied — the matched extent, §7.3 — and a capture that
takes that name, or any of the other six, is refused (GRAM4012).

**§7.3's first way of filling a type in works**: a rule that declares `: @T` and writes
no `=>` is built by calling `T`'s constructor, with its captures as the arguments.

```dotgram
Row : @Row = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+
```

matched against `Row(string name, string amount)` and called with the two captures, in
the constructor's order. Names are compared without regard to case, which is the
mechanical transform §7.3 describes — the capture `symbol` fits the parameter `symbol`
and, where a record wrote one, the property `Symbol`.

The constructors of a type are the third thing the Roslyn shell answers, alongside
whether a type exists and whether one is assignable to another
(`.claude/rules/grammar-half.md`). It is asked as part of the cached question set, so a
keystroke that changes nothing about the host still costs nothing.

Chosen, not guessed at: the longest constructor every parameter of which is covered by a
capture. Two of the same length both covered is an ambiguity nothing here can resolve, so
none is chosen and the rule is reported unbuilt — calling the wrong constructor silently
is the failure worth avoiding. The types are not checked on this side; whether a capture
goes in that parameter is C#'s question, and §7.6 now asks it on the grammar's own line.

Writing the construction out by hand goes on meaning exactly that. The match is what
happens when the grammar left the question unanswered, so `=> @(new Row(amount, name))`
builds it that way round, arguments swapped and all — a test pins that, because a match
by name could never produce it.

Half an answer is refused. A `=>` on one alternative and not on the next leaves a rule
whose value is built two ways, and the constructor is matched against the rule rather
than against an alternative, so there is no half to complete. The message says which
half: *says how to build its value on 1 of its 2 alternatives and not on the rest*. It
could have been completed instead — filling in the alternatives that stayed quiet — and
that was not done on purpose: a missing `=>` is as likely to be an omission as an
intention, and the silent version of that guess builds the wrong value.

A type written beside the grammar is found by its short name, the same way a method
beside it is. `@Row` used to mean a top-level `Row` only, so a type nested in the host —
which is where a type written for one grammar belongs — could not be named without
spelling out a chain the author writes nowhere else. The host is looked in first, which
is both what C# does and what the generated code needs: it sits inside the host class, so
a short name there binds to the nested type whatever the resolver decided, and deciding
otherwise would check the constructors of one type and call another.

§7.3's second way works too, and is reached when the first cannot answer: the value is
made and its properties written from the captures, in one object initializer because
that is the only place `init` and `required` can be written. Every `required` property
has to be covered — a type saying `required` is saying it will not compile otherwise —
and a property that is neither covered nor required is left alone, keeping whatever
default the type gave it. At least one has to be written, or this would be making an
empty value rather than building one.

The order is §7.3's own: a constructor first, and only what it could not answer is asked
of the properties. A tie between constructors is left as a tie rather than falling
through to them — the grammar did name a way to build this type, and it is the ambiguity
that has to be reported, not a second way found behind it.

Whether the type has a constructor of no parameters to write into is not checked here.
That is C#'s question about the initializer this emits, and §7.6 now asks it on the
grammar's own line.

## A guard asks the values

`when @(…)` runs **during** the match, which is what makes it recognition: saying no
is a non-match and a sibling alternative is tried, exactly as §8.1 has it. It becomes a
method of its own for the same reason a `=>` does, and takes the same `parserText`.

What it may look at is what was captured **before** it. A capture further along has not
been written, so it is not a parameter, and naming it is an ordinary C# error about a
name that is not there. A name captured in more than one alternative is passed as
nullable at the guard, because only the slots behind the guard can have been written
and the generator does not try to prove which.

## Every alternative may build its own way

Which `=>` fired is remembered while matching and undone with everything else an
abandoned attempt did — a `=>` covers a whole alternative, so the only way back past
one is through the choice that offered it, which is where it is forgotten.

A factory sees only what **its own** alternative can have captured. A sibling's
captures are not its parameters, and its own are optional only where that alternative
may skip them — so `digits: […]+ => @(int.Parse(digits))` does not warn about a null
that the alternative it belongs to cannot produce.

Three things are refused rather than quietly ignored, all `GRAM4008`:

- a `=>` on a rule that declares no type. There would be nothing to build.
- a declared type where some alternative has no `=>`. §7.3 would fill that by matching
  captures to a constructor by name, and that does need symbol resolution.
- a `=>` anywhere but on an alternative of the rule — inside a group, say. It builds
  the rule's value, and a group has none.

`: T` naming another rule (§4.1 case 3) is refused with `GRAM4011`. Only `: @T` and the
C# keywords count as a declared type — and until it was refused, the declaration was
dropped in silence and the rule got a type generated from its own captures instead, so
`A : B` compiled, ran, and handed back an `A` with nothing to do with `B`.

Syntactic position now fixes the two recognizer contracts. `[@Name]` is an element
predicate and emits `Name(c)`; bare `@Name` is an input-consuming recognizer and emits
`Name(text, ref p)`. Neither name is looked up by the generator. Missing names, wrong
signatures and overload selection are ordinary diagnostics from the generated C# call.

## One deviation from §7.3, deliberate

The generated type used to be the other one. §7.3 asked for a `record` and a `sealed
class` was emitted, because a positional record needs `IsExternalInit` and that lives in
a namespace this generator must not emit into. That is no longer a deviation: the
specification says class now, and says why. A specification may describe what is not
built yet — that is what this file is for — but not something the project has ruled out
on purpose.

What is left:

- `Match<T>.Value` is `T` rather than `T?`, so a failed match holds `default` and
  `IsSuccess` is what says so. An unconstrained `T?` needs a language version this
  generator may not assume, and `T` has to be unconstrained now that a rule may declare
  itself `: @int`.

## Where the author's C# actually runs

Not inside the machine. Every `=>`, every `when` and every recovery factory becomes a
**method of its own**, and the names it can use — the captures, and the supplied
`parserText`, `parserLine`, `parserColumn`, `parserOrdinal`, `parserMessage` — are that
method's **parameters**:

```csharp
static FeedLine Recognize_Feed_Recover(
    string parserText, int parserOrdinal, int parserLine, string parserMessage) =>
    new RejectedLine(parserOrdinal, parserLine, parserText, parserMessage);

static decimal Construct_Expr(string parserText, decimal operand) =>
    (-operand);
```

That is what makes the names usable at all. Written where the value is assigned they would
have to dodge every local the recognizer has — `p`, `state`, `r`, `saved`, `sp`, `bt`,
`value`, `failure` — and a capture called `p` would collide with the machine itself. As
parameters they are in a scope of their own, named exactly as the grammar named them, and
the machine's internals cannot leak in either direction.

It also means a capture may not be called `parserText`: the parameter is already taken.
That is the whole reason the supplied names carry a prefix — with it, nothing an author
would naturally write collides — and `GRAM4012` is the backstop for a capture that takes
one of the seven anyway. Before it was refused, the generated code simply did not
compile — "no overload takes 2 arguments", in a file the author never wrote, about a
grammar it did not mention. §8.2 of [`syntax.md`](syntax.md) says why the names are
separate arguments rather than one context object.

## C# value calls are passed through

Names used by `when` and `=>` are not looked up or classified by the generator. The
call is emitted as written, and `#line` maps the consumer compiler's diagnostics back to
the grammar. Overloads, generic inference, accessibility and result types consequently
have exactly their C# meaning. A misspelled `@Tini(digits)` remains a missing C# name; it
is not turned into a generated declaration.

## `find` reads from a reader

`find R` now has a second overload taking a `TextReader`, and it is the first thing the
retention analysis actually decides rather than merely computes: a rule gets one when a
single occurrence of it fits in a window, which is `LineExtent` anything but `Beyond`.
`Start = any* & 'z'` gets none — it can give back any of the file, so the window would
be the file.

The window is a buffer that is reused: what is before where the parse is now can never
be returned to (§4, and §6.3 rests on it), so it is dropped by moving what is left to the
front of the same array. A feed of a million occurrences reads through one allocation.
It grows only for an occurrence longer than 4096 characters, which the analysis has
already established is bounded by the grammar.

**It is not a line reader**, and that is the part worth knowing. The analysis measures in
lines because that is what bounds a feed, but nothing in the window knows what a line is:
two occurrences may share one, one may span three, and a grammar with no line terminators
in it streams exactly as well.

**A result that ran into the end of the window is not an answer yet.** This is the whole
of what makes a windowed parse correct rather than nearly correct. `['0'..'9']+` stopped
at a buffer boundary looks exactly like `['0'..'9']+` stopped by a letter, and a failure
that reached the end could have matched with more input. So anything touching the end is
provisional while the reader has more: read, and ask again from the same place. Without
it, digits straddling the boundary come back as two occurrences — or, since the parse
then resumes mid-number, as none at all, which is what the test saw.

It is also what keeps `eof` honest. The end of a full buffer is indistinguishable from
the end of the input to a recognizer, and the only thing that can tell them apart is
whether the reader is finished.

**A grammar that does not get one is told so**, as `GRAM5001`, at the directive that
asked — `'FindStart' gets no overload taking a reader: 'any*' may take more than one
line, …`, naming the innermost part that does not fit rather than the whole body it sits
in, and pointing at §6.3. Information rather than a warning: the grammar is correct and
there is nothing to fix. Without it the author meets a call that does not bind and a
message about converting `TextReader` to `string`, which names neither the rule
responsible nor anything they could do about it.

Only where the grammar is the reason. A `parse` gets no reader overload because the
windowed driver for it is not built, which is a fact about this compiler rather than
about the grammar in front of it — saying that on every build of every grammar would be
noise, and this file is where it belongs. What would let a `parse` window move is a
committed repetition inside it, which means the decomposition `Retention.PlanFor` does
rather than the single question `find` asks.

## `parse` reads from a reader

`parse R` gets a second overload taking a `TextReader` and handing back
`IEnumerable<T>` — the elements of the sequence, one at a time, out of input that is
never all there at once. `Header`, then every `Row`, then `Trailer`: the envelope arrives
in the stream with the records, on its own place, which is what §4.1 case 2 buys.

The stages are run in order rather than compiled into one machine, and that is the whole
difference from the parse over a string. A machine may backtrack anywhere inside a rule;
a stream may not go back past what it has handed over. Each stage reads through the
window by the same provisional rule `find` uses.

**Three conditions, and the second is the interesting one:**

- the result is a sequence (`: @T[]`), because that is the only shape that comes in parts;
- every repetition either ends where the grammar says, or is marked `recover`;
- every part fits a window, a repetition measured by one of its elements.

The second is the requirement itself rather than a stand-in for it: no element handed
over may ever have been wanted back. A repetition whose first set does not meet what
follows it stops where the grammar says and nothing has to commit it — that is the
ordinary feed, and it streams with no mark at all. Where the two do meet, `recover`
settles it, because §8.2 makes a marked repetition possessive: an element it took was
either read or explicitly rejected, and there is no shorter reading to come back for.

It used to demand the mark outright, which was a conservative test standing in for this
one. `recover` is now back to meaning only what it says — survive a bad record — rather
than doubling as permission to stream.

A grammar that declares a sequence and does not get the overload is told why
(`GRAM5001`). One that declares no sequence is told nothing: most grammars are not feeds,
and a note on every build of every one of them is noise.

**A broken record is stepped over in a stream too**, both ways §8.2 offers: with a `=>`
the rejection takes its place in the sequence, and without one it is dropped and told to
the `OnRecovered` hook. Over a string the repetition backtracks out of the bad element
and the machine steps over it; in a stream the driver does the stepping itself, scanning
for the synchronization expression through the window and reading more of it when the
search runs out of what is held.

The names §8.2 supplies come from the window rather than from a span, and that is not
tidiness: a line number counted inside the buffer restarts every time the buffer moves,
so a bad record deep in a large feed would be reported near the top of the file. The
window counts the terminators it drops and where the last one was, which is what makes
`parserLine` and `parserColumn` mean the same thing in both modes. There is a test at two
thousand records — well past the first window — that says so.

## A repetition that cannot tell its own end

`GRAM5002`. `Row* & Trailer` where a trailer also reads as a record parses perfectly
well — the repetition takes the trailer, the rule fails, the repetition gives it back,
and the parse succeeds by the second reading. Nothing is wrong with the answer, and
nothing told the author their rule had two readings and backtracking picked one. First
sets say when it can happen: what the repeated element can begin with, against what
follows the repetition.

**An overlap is not a defect on its own**, which is why this is not raised everywhere.
`'a'+ & 'a'` is the same overlap and is perfectly good `.Gram`: §11 makes backtracking
total and a rule is entitled to lean on it. It becomes a defect exactly where the parse
cannot go back — a rule declaring `: @T[]` is asking to be handed over an element at a
time, and an element handed over cannot be taken back. So the check runs on those rules
and no others.

That measurement is also what the streaming test runs on now. It used to demand a
`recover` outright, because possessiveness is a property that can be checked; this is the
property that actually matters, and having it measured let the requirement become "no
overlap, or marked".

The sets are approximate and in the safe direction: a complement, a Unicode category or
a C# predicate answers "anything", two of those overlap, and the result is a note rather
than a refusal. Being told about an overlap that is not real costs a sentence; missing
one costs the thing this exists to prevent.

## A rule may say out loud that its result is the text

§4.1 case 4: with no `=>` and no captures, "the result is the matched extent: `string`
gives the text, `SourceSpan` gives the bounds". Saying so was refused — and refused with
a message about matching captures to a constructor, of which there were none.

`: @string` now means what the same rule without a type has always meant, and is recorded
as no declared type at all: a declared one is what tells the emitter to expect a value the
machine never builds, which is how this first showed up as generated code that would not
compile.

`: @SourceSpan` is the other half and is still not built — the value would have to be made
where the match accepts, from positions no factory is handed. The message says that now
instead of talking about constructors.

## A lookahead produces what it saw

§3.4 says `?=X` "produces X's value (which can be captured) without moving the input",
and §3.6 writes the example out:

```dotgram
SmallNumber = n: ?=Number & when @IsSmall(n) & value: Number
```

Neither half worked. `n: ?=Number` did not parse at all — a capture read only a primary
expression, and `?=` is a prefix, so the two nested one way round and not the other. And
once it did parse, the capture came back empty: the extent was measured from `p` to `p`,
and `p` is exactly where a lookahead leaves it.

It now takes the extent from what the lookahead returned, which is how far it got before
giving the position back. A negative lookahead still produces nothing, which is not an
oversight — it succeeded because what it looked for was *not* there, so there is nothing
to have seen.

Found by reading §3 with a probe rather than by writing a feature: five claims tested,
one of them false in two ways.

## A rule may take another rule

§4.2, for the arguments that are pieces of grammar:

```dotgram
List(item, sep) = item & (sep & item)*

Start = List(Word, Comma) & ' ' & List(Word, Semi)
```

**By substitution, not dispatch.** `List(Word, Comma)` becomes a rule of its own —
`List_Word_Comma` — whose body is `List`'s with the parameters replaced by what was
passed. A parameter is therefore a compile-time thing entirely, and nothing downstream
ever meets one: the machine, the capture layout and the retention analysis all see an
ordinary rule. That is also what lets a parameter be a *recognizer* at all, since passing
a rule as a value at run time would need a delegate the emitted code deliberately does
not have.

Two calls with the same arguments share one specialization, keyed by what those arguments
lower to, so a grammar naming `List(Word, Comma)` twice gets one recognizer. An argument
need not be a rule — anything that recognizes will do, including a literal or a character
class.

It used to compile and match nothing at all: a parameter lowered to an element set with
nothing in it, which is a rule that runs and refuses every input. The parser and the
binder had understood parameters all along; it was the normalizer that dropped them.

**An argument may also be a number**, which is the other half of §4.2:

```dotgram
Digits(n) = ['0'..'9']{n}

Start = Digits(4) & '-' & Digits(2)
```

A count may name a parameter, and the number the call passed is substituted into the
quantifier. The two kinds of argument are told apart where the call is lowered rather
than by the parameter's declaration: a number is neither a recognizer nor lowerable into
one. An argument that names the caller's own parameter passes the number through, so
`Pair(n) = Digits(n) & '-' & Digits(n)` works as it reads.

The template itself is not in the graph. A parameterized rule is not a rule until it is
called — its body names things only a call gives values to — so what is emitted is the
specializations. Lowering the template would report a count with nothing passed for it,
and emit a recognizer nobody could call.

Refused: a count naming a parameter that was given a piece of grammar rather than a
number, which would otherwise repeat zero times and match nothing (`GRAM4013`). And
`: item`, the result type naming a parameter — §4.1 case 3 said of a parameter, refused
where the rule is declared and not again about the specialization.

**A call that would specialize for ever is refused too**, which §4.2 asks for and which
matters more than it sounds. `Grow(item) = 'x' & Grow(Pair(item))` wraps its own argument
at every call, so there is no repeat to find and no end to the specializing — and the way
that ended was a stack overflow, which is not an exception and takes the process with it.
An author would have watched their IDE lose the compiler rather than read anything about
their grammar. Bounded at 24 nested specializations: generous for a grammar built out of
`Lex(List(Padded(…)))`, and passed almost at once by growth.

**A result type may name a parameter, in the sequence form.** `Many(item) : item[]`
called as `Many(Word)` is an array of what `Word` produces — there are no type parameters
in the language and none are needed, because a specialization has one concrete argument
and so a concrete answer. The pairing is written down where the specialization is made,
because that is where the argument is known, and resolved once every rule's own type has
been worked out. To a fixpoint, since an argument may itself be a specialization of the
same kind; an argument that builds nothing answers `string`, which is what an extent is.

The scalar `: item` works too, and is the same thing as §4.1 case 3 — `A : B` says A's
value is B's — so one rewrite covers both. The operand that produces the value becomes a
capture and the alternative hands it back, which is the sequence rewrite one size down.

Exactly one operand may produce it. Two is a rule with two answers and nothing to say
which, so it is left alone and reported: that is a grammar to rewrite, not a choice for
this compiler to make quietly.

Half built: **a declared parameter type**. §4.2 says a C# type makes the parameter a
value and anything else makes it a recognizer, and the only value that can be passed is a
number — so `Digits(n: int)` called as `Digits(4)` works, and the number reaches the
quantifier.

`Padded(item, pad: char)` handed `' '` does not. It used to: the call judged the argument
by what it turned out to be rather than by what was declared, so a literal became a
recognizer and the parameter meant one thing where it was declared and another where it
was used. It is refused now, which is the whole of the change — a declaration that is
quietly disregarded is worse than one that is turned down, because the grammar goes on
compiling and matching something else.

## A C# method may read the input itself

§7.1's second row works, in the form without a value:

```csharp
static bool Blob(ReadOnlySpan<char> input, ref int pos)
```

Bare `@Blob` stands where an operand goes, reads whatever it likes, and moves the parser's own
position to say how much it took. Saying no is an ordinary non-match: the stack has
somewhere to resume and the grammar carries on. Its value is the text it covered, the same
as any rule that captures nothing — which is why this form needs nothing new from the
host, and why it came first. The form with `out T value` needs the host asked what `T` is,
and is not built.

**It is trusted absolutely.** The `ref` is the method saying it moves a position; it is
handed the parser's own, and nothing copies it away, checks it afterwards, or reasons
about what came back. That is written into §7.1 as a contract rather than left implied: a
seam that second-guessed the code on the other side of it would cost every parse that uses
one and still not make a wrong recognizer right. Reaching into the parse means taking the
parse's invariants on with it.

The one consequence: a grammar containing one gets no streaming overloads. The method is
handed a span and told nothing about where it came from, so it cannot tell the end of a
window from the end of the input — and, unlike a literal, has no way to say which it hit,
which is exactly the distinction `Starved` exists to carry. It would read a record cut in
half as a record that ended.

## A C# predicate stands where an element does

§7.1's first row works: `bool M(char c)` asks the same question about one input item that
a range does, so `[@IsVowel]` is an element set with one C# predicate and merges with
characters and categories in the same brackets:

```dotgram
Start = [@IsVowel | '0'..'9']+ & [@IsStop]
```

The name is written into the generated code as the grammar wrote it, unqualified. The
grammar's own `@using` directives are in that file, which is what they are there for.

Bare `@Name` is the other contract: a recognizer taking the span and a position. The
generator chooses between the two only from brackets versus operand position. It asks
Roslyn about neither method; the emitted `Name(c)` or `Name(text, ref p)` lets C# select
the matching overload and report a missing or incompatible one.

## Where a C# error lands

On the grammar's line. A `=>` or a `when` is the author's own C#, and the C# compiler
will have things of its own to say about it — that a method does not exist, that the
arguments do not fit. Those are said over a `#line` directive naming where the code was
written, so they arrive there rather than inside a machine-written file the author did
not open, cannot edit, and is told is auto-generated. §7.6 calls this a condition of the
seam working at all, and it is: a seam whose errors land on the wrong side of it is not
a seam.

```csharp
static int Construct_Sum(string parserText, int value) =>
#line 28 "…\CalculatorExample.cs"
                                                                       (value);
#line default
```

The padding is deliberate. The directive fixes the line; the column comes from where the
text sits on it, so the line is written out with no indent of its own and padded to the
column the grammar had. An error under one argument of `@Add(l, r)` then lands under that
argument. The `@` is skipped when the position is taken, because it is the grammar saying
that C# follows and is not part of the C#.

Two maps, because the answer depends on how the grammar reached the compiler. A `.gram`
file maps onto itself — `GrammarLineMap`, which is pure and lives on the grammar side. A
grammar inside a `[Gram("…")]` attribute maps into the C# file holding it, and *that*
cannot be computed: what the compiler was handed is the decoded value of a string
literal, and what the author reads is its spelling, with escapes, quoting and a raw
literal's indentation in between. So it is searched for — take the grammar's line, find
it in the spelling, and where it occurs exactly once the offset is known exactly. Found
twice or not at all, there is no answer and no directive: pointing at the wrong line is
worse than pointing nowhere, because the author reads a place with nothing wrong with it
and concludes the message is nonsense.

The plain form of the directive, not the span form C# 10 added — the consumer's language
version is not ours to assume.

Two things this is not. It is not what places DotGram's own GRAM diagnostics: those carry
a position in the grammar and are placed by `Generation/Report.cs`, which solves the same
inline problem by the same search and has done for a while. And it does not extend to the
rest of the generated file, which stays attributed to itself — `#line default` closes
every region, so this compiler's own bugs are reported against this compiler's own
output.

## A method that does not exist

The generator emits calls from `when` and `=>` even when their C# names do not exist.
The consumer compiler then reports the missing name at the mapped grammar location. No
partial helper declaration is generated and no intended signature is guessed.

## A rule can be a sequence of what it is made of

§4.1 case 2 works: `Feed : @FeedItem[] = Header & Row* & Trailer & eof` hands back the
envelope and the records in one array, in the order they were read, with no `=>`
anywhere in `Feed`. Every operand whose value is assignable to `FeedItem` joins;
`Row*` contributes all of its elements; `eof` and anything else that builds no value
contributes nothing. A rule that declares a sequence and has no operand that fits is
refused (`GRAM4008`) rather than generating a method that returns an always-empty array.

**Rewritten into what already worked.** Each operand that fits becomes an ordinary
capture and the alternative gets a `=>` whose text is a marker, so the captures are
numbered, given up on backtracking and rebuilt by exactly the code that does it for a
capture the author wrote. The only new thing is what the factory's body says — a body
rather than an expression, because a repetition contributes an unknown number of
elements and an optional operand contributes none.

The capture goes *inside* a repetition, not around it, which is where one the author
wrote ends up: `rows: Row*` parses as `(rows: Row)*` (§10). The other way round the slot
holds the text of the whole run.

**Assignability is a question for the host**, so `ISymbolResolver` gained `IsAssignable`
— the third thing the grammar half asks about C#, alongside "does this type exist" and
"what shape is this method". It is asked through the same question-and-answer list as
the other two, so nothing downstream of it holds a `Compilation`, and the pairings are
collected as a superset from the grammar's syntax the way §7.1's names are. Roslyn's own
conversion classification answers it, minus numeric widening and user-defined operators:
what joins a sequence is what already *is* the element type.

This is also the shape a streamed `parse` needs, since a sequence is the only result
that can be handed over one element at a time.

**It fixed something else on the way.** `BuildsValue` asked only whether a rule had
captures, so `Header : @Item = 'H' & eol => @(new Head())` — a rule that plainly has a
value and no captures — counted as text, and a capture of it held the characters instead
of the `Item`.

## A recovery that builds needs a sequence to build into

`rows: Row*` where `Row` captures nothing is one string — the run joined, §7.3 — not a
sequence of values, so a `=>` on the recovery has nowhere to put the rejection. It used
to emit a factory call against a list that does not exist, which the consumer's compiler
reported as an undefined name in a file they never wrote. Now `GRAM4010`, saying which
of the two fixes applies: give the repeated rule a capture, or drop the `=>` and report
out of band (§8.3).

Found by writing a test about something else — that an exception out of a `=>` leaves
the parse, which §8.2 decided and nothing had checked. It does.

## And from a sequence of lines

§6.3 lists `IEnumerable<string>` beside `TextReader`, and the difference between them is
one character: a reader carries its terminators, and a sequence of lines has had them
taken off. So they are put back and everything downstream is the reader case unchanged —
one generated method per publication, forwarding.

Which terminator is a decision rather than a detail. `
`, because a grammar's `eol`
matches it, because it is what the lines most often came from, and because putting back
what was actually taken off is not knowable: the sequence does not say, and
`File.ReadLines` would not tell it.

A test gives the same feed both ways and compares what comes back, because that is the
property worth holding: what a parse answers may not depend on which door the input came
in by.

## What the window costs

Measured, because streaming that is slower and heavier than reading the file is a
feature nobody wants. The same feed, the same grammar, the same parts built, given three
ways — `benchmarks/DotGram.Benchmarks/StreamingBenchmarks.cs`, ten thousand records:

| Input | Time | Allocated | Gen2 |
| --- | ---: | ---: | ---: |
| `string` | 719 us | 2653 KB | 249 |
| `TextReader` | 433 us | 1415 KB | 0 |
| `IEnumerable<string>` | 518 us | 1884 KB | 0 |

Streaming is *faster*, which was not the claim being made and is worth understanding:
the string case has to hold the input and every part at once, and the array it grows is
large enough to be collected as one. The `Gen2` column is the whole point — the streamed
cases hold one part at a time and never reach the large object heap at all, while the
string case pays a second-generation collection for every four operations.

At a hundred records the three are within a fifth of each other, which is the other half
of the answer: the window costs nothing worth noticing on input that would have fitted
anyway.

**A wide feed, which is the shape that pays for itself.** Fifty pipe-separated fields a
record, twenty-five of them read out into an object — `long`, `int`, `decimal`,
`DateOnly`, `DateTime`, an enum of the grammar's own and a good deal of text, every one
converted by C# the grammar names. The captured fields are scattered rather than the
first twenty-five, because a skipped field still has to be recognized and every capture
between them is a slot the machine keeps. One million records, about 190 MB:

| Input | Time | Allocated | Gen2 |
| --- | ---: | ---: | ---: |
| `string` | 3164 ms | 3279 MB | 7000 |
| `TextReader` | 1351 ms | 1641 MB | 0 |
| `File.ReadLines` | 1472 ms | 2710 MB | 0 |

Ten million records — 1.9 GB of feed — read through the same 4 KB window in 13.6 seconds,
still with no second-generation collection. The string overload was not asked: 1.9 billion
characters is 3.8 GB in one object, which stops being a choice well before that.

**The defect the benchmark found, and what it was.** Records of *differing lengths* lost
the window: one record per buffer was read as broken and stepped over, silently, until
the trailer turned up where a record was expected.

Running out of input looked exactly like not matching. A literal checks
`p + n > text.Length` and gives up **without moving `p`** — so a record straddling the
end of the window reported its failure at the position the missing character would have
gone in, which is *before* the end of what is held. The driver asks "did this run into
the end of the window?" to decide whether to read more, and the honest answer was no.

`Failure` now carries `Starved`, set exactly where a bounds check gives up, and both
drivers read it beside the position. Only where something streams: over a string the end
of the input is the end of the input, and a rule wanting one more character was simply
wrong.

Every fixed-width test passed throughout, including a megabyte of records through a 4 KB
window — the boundary sat at the same offset inside every record, and that offset
happened to be a safe one. There is now a test whose quantity field is one, two or three
digits, which walks the boundary through every offset there is.

One more thing fixed on the way there: the recovery scan extended the window from
`start` rather than from `from`, which threw away the front of the element about to be
handed over and put `from` before the window — a negative index into a buffer.

## An offset is a `long`, a length is an `int`

§6.3 draws the line and the generated API now sits on the right side of it:
`Match<T>.Position` is a `long`, because it is an offset into the whole input and an
input may be a file no `int` can index, while `Match<T>.Length` beside it stays an `int`,
because it is an extent into a buffer and a buffer never is. `parserPosition` and the
`OnRecovered` hook were already `long`; the published match was the one place still
saying `int`, and streaming is what makes the difference something other than pedantry.

Widening rather than narrowing, so nothing written against it breaks: an `int` variable
reading `match.Position` is the one thing that stops compiling, and `long` is what it
should have said.

## Nothing is shared between assemblies

`.Gram` emits everything a parser needs into the consumer's own compilation, and every
type it puts in a namespace is `internal`. That is what makes the claim in the README
true rather than nearly true: an internal type cannot be seen across an assembly
boundary, so two assemblies that both emit `DotGram.SourceSpan` never collide, never bind
to each other's, and have nothing to version.

There was briefly a shared mode — `[assembly: GramRuntime]` published four support types
as `public` and other assemblies bound to them, having found them by looking up a type by
name. It was removed rather than fixed, for two reasons.

It reintroduced exactly the skew that emitting into the consumer exists to prevent: an
assembly built by one version of the generator would bind to types emitted by another,
with no package, version or metadata anywhere to say so. And it was protecting nothing —
of the four types, `Outcome`, `Diagnostic` and `RecognitionResult<T>` were referenced by
no generated code at all, and `SourceSpan` appears only in the private signature of a
recovery factory, which never crosses a boundary. Three types were deleted; §7.5 still
specifies them and the table above says they are not built, which is this project's
ordinary way of holding a plan.

`GRAM0001` went with it — it reported two assemblies both publishing — so diagnostic
numbering starts at `GRAM0002`. A retired number is not reused: a suppression written
against the old meaning would silently acquire a new one.

Numbers go by the stage that raises them: `GRAM0002`–`GRAM0004` the Roslyn shell,
`GRAM1xxx` the lexer, `GRAM2xxx` the parser, `GRAM3xxx` the binder, `GRAM4xxx` the
normalizer, `GRAM5xxx` the analyses that decide what a grammar gets rather than whether
it is one. `GRAM0001` and `GRAM4004` are retired.

### How a message arrives, rather than what it says

Three of these, of which two are now fixed. They are here and not against a particular
number because none of them is about a particular number.

- **A position in an inline grammar now lands inside the attribute's string**, where it
  can be placed at all. The offset is into the grammar and what the author looks at is
  the spelling of it, so the two have to be lined up — and a C# string knows how to turn
  its spelling into a value, not the other way round. Reversing that properly means
  re-implementing escapes, verbatim doubling and raw-string indent stripping: three sets
  of rules, each with corners, all to place a squiggle.

  So it is done by looking rather than by decoding. Take the line of the grammar the
  diagnostic sits on, find it in the spelling, offset within it. Found once, the place is
  exact. Found twice or not at all — a repeated line, or one whose escapes were written
  differently from what they decode to — there is no answer, and the message lands on the
  attribute as it always did. Never wrong, sometimes silent.
- **An unrecognized character no longer induces a crowd.** A stray `~` used to be
  reported seven times: by the lexer, then by the parser about the same character, twice
  more about where it ended up, and then by the binder and the normalizer describing the
  tree the parser had guessed at — including `No rule, parameter or capture named ''`,
  which is about nothing at all. Now three, and the first one says what happened.

  Two rules do it. At most one **error** per position, because the second thing said
  about a place is the first stage's failure told again by the next one; warnings and
  information are left alone, since two of those in one place can both be true. And
  nothing from a later stage about a declaration the parser could not read whole — what
  it would be describing is a guess. That silence is scoped to the declaration, from
  where it begins to where the next one does, so a rule below it is still checked, which
  is what implementation.md §0 asks for and there is a test for.

  What is left is the parser's own recovery: two of the three remaining messages are it
  finding its feet, and a proper synchronization point would make them one.
- **An ambiguous grammar is only called one where it asks to be streamed.** `GRAM5002`
  compares first sets, and only for a rule declaring `: @T[]` — everywhere else leaning
  on backtracking is legitimate and saying otherwise would be wrong. A grammar that is
  ambiguous and never streamed is still told nothing, and whether that is worth an opt-in
  warning is open.

## What re-runs, and when

Two tests hold this, and they only mean anything as a pair: editing a `.cs` file that no
grammar and no host has anything to do with must leave the output step `Cached`, and
editing the grammar must not. The first alone is satisfied by a generator that does
nothing at all.

It began as one test that failed. Every parser was being regenerated on every keystroke
in every open project, because of one line:

```csharp
context.RegisterSourceOutput(hosts.Combine(files).Combine(context.CompilationProvider), …);
```

An incremental generator caches what each step produced and re-runs the next one only
when that changed. `Host` is carefully reduced to strings for exactly this reason, with a
comment saying that a symbol compares equal to nothing across compilations — and then a
whole `Compilation` was combined into the same input, which compares equal to nothing
either.

**The caching is Roslyn's, not ours.** A generator keeping its own table of grammar text
to generated code would pin compilations, outlive projects and target frameworks, and
leak. The pipeline does this properly; what was needed was to stop defeating it.

The compile now happens in a `Select` rather than in the output step. The transform still
re-runs on every keystroke — resolving `@Name` needs the compilation and there is no way
round that — but what it *produces* is a hint name, a string of C# and a list of reports,
all compared by value. An edit that changes none of them leaves the output step `Cached`,
and the consumer's IDE is not handed a new syntax tree to parse and bind, which for a
fifteen-hundred-line state machine is the greater part of the cost.

Two things had to become values first, and both are the usual way this fix is written and
found not to work:

- a `Diagnostic` holds a `Location`, which holds the syntax tree it came from, so
  carrying one would make the output unequal whenever any tree was reparsed. `Report`
  carries the pieces and builds the `Diagnostic` at delivery.
- `ImmutableArray<T>.Equals` compares the underlying array **by reference**, so a step
  handing one out is unequal to itself every run. Hence `EquatableArray<T>`.

**Then the `Compilation` was narrowed to what it is for**, because the transform it fed
was not cheap: one compile of the URL grammar is 1.5 ms, so twenty grammars in a solution
is thirty milliseconds of a keystroke and a hundred is a sixth of a second.

The compilation answers two questions — does this C# type exist, does this method exist
with this shape — for the handful of `@Name` and `: @T` a grammar mentions. So there are
three stages:

```text
grammar + host  ──►  the questions its C# names raise      cached on the grammar
                              │
Compilation ──────────────────┤  the answers, as values    re-runs, and is a few lookups
                              ▼
grammar + host + answers  ──►  the parser                  cached on all three
```

Editing a C# file re-runs the middle stage and stops there, because the answers it
produces are the same ones. Both ends are checked: `Asked` and `Compiled` must come back
`Cached` after an unrelated edit, and `Compiled` must **not** after a grammar edit — and
the check insists the stage ran at all, because `Assert.All` over nothing passes.

**The questions are collected from the grammar's syntax, and are a superset.** Not by
watching the binder ask, because the binder stops as soon as an answer satisfies it —
`TypeInView` tries the bare name and then each import in turn — so a recording pass would
record a different set from the one the real pass needs. Every C# name crossed with every
`@using` is more questions than are needed and always includes the ones that are.

If it is ever not, the answered resolver throws rather than saying no: a question nobody
foresaw cannot be answered once the compilation is out of reach, and "no" would refuse a
grammar for naming a type that exists. A test drives the four ways a grammar reaches C#
and requires the generator not to fall over.

`RegisterImplementationSourceOutput` — output produced only for real builds, invisible to
IntelliSense — is the mechanism for keeping heavy work off the editor's path, and it does
not apply here: a generated parser *is* the public API a consumer writes against, so it
has to exist in the editor. It suits a generator whose output nothing references by name,
which is why the aspect generator next door uses it for its interceptors and
`RegisterSourceOutput` for its diagnostics.

## Streaming, so far

§6.3 emits the streaming overloads only where the grammar provably works with a reused
buffer, so the analysis comes before anything else. It has two halves and one is built.

**What each rule can take, in lines** — `Retention.ExtentOf`. Three answers, and the
distinction that matters is not whether a terminator is consumed but whether anything
follows it:

| | | fits a line's buffer |
| --- | --- | :-: |
| `None` | no path consumes a terminator — a field | ✓ |
| `AtEnd` | one may be consumed, and nothing follows — a record | ✓ |
| `Beyond` | one may be consumed and the parse goes on | ✗ |

A fixpoint over the graph, like nullability, and for the same reason: a rule's answer
depends on the rules it calls and recursion makes that not a tree. It rests on a second
fixpoint — which rules can consume anything at all — because that is what tells a
terminator at the end of a rule from one in the middle. Guessing that a call consumes
made `eol & eof` two lines, which it plainly is not.

The case it exists for is the field written while thinking about separators:

```dotgram
Text = [^ '|']+                 // Beyond — `+` repeats, so it swallows the file
Text = [^ '|' | '\r' | '\n']+   // None
Row  = "R" & Text & eol         // AtEnd with the second, Beyond with the first
```

A Unicode category is not looked into and is assumed to admit a terminator. That is wrong
in the safe direction: a rule wrongly said to take one loses an overload it could have
had, and one wrongly said not to would lose data.

**Where the window may move** — `Retention.PlanFor`. A published rule breaks into stages,
one per operand of its body, and each is either a piece that fits the window or a
committed repetition of pieces that do. A committed run is measured by **one element**
rather than by the run, because the run's length is what streaming is for.

Two things must hold, and each is a different failure:

```dotgram
Feed = Header & Row* recover eol & Trailer & eof   // streams
Feed = Header & Row*             & Trailer & eof   // 'Row*' may take more than one line
Pair = Header & Trailer & eof                      // every stage fits, none commits
```

The second is not a measuring failure dressed up: with the mark off, the run stops being a
stage boundary and is measured as what it is, every row at once — so it names itself. The
third is the one measuring alone would miss, where each stage fits the window and there is
still no point at which the first may be let go.

Written as a decomposition rather than as "find the recovering repetition" because that
generalizes. Two committed runs in one rule are an ordinary feed —
`Header & Trades* recover eol & Separator & Adjustments* recover eol & Trailer` — and a
stage may itself be a rule with stages of its own. **Neither is built**: one `recover` per
rule is still refused, which is an implementation limit that this shape makes visible, and
it will bite exactly when multi-stage feeds become worth writing.

Nothing is emitted from any of this yet — no overloads, no diagnostic. The analysis is
tested on its own, because one only exercised through the feature it gates is one nobody
can tell is wrong.

## What has been measured

Six of the architecture's claims now have numbers rather than reasoning behind them.

**Against `Regex`.** `benchmarks/` runs the URL grammar against the same language written
as a regular expression, and refuses to time anything until both agree on every part of
every input. Generated parsing comes out 2.3–6.3× faster than interpreted `Regex` and
between 1.1× and 1.9× faster than `RegexOptions.Compiled` — the same order as the best the
BCL does, not a different class. Allocation is at parity, because both materialize the
parts as strings. `benchmarks/README.md` has the table and what not to read into it.

**Throughput on a large feed**, under *What the window costs* above: a million wide
records read in 1351 ms through a 4 KB window against 3164 ms and 3.2 GB for the same
feed as one string, and ten million — 1.9 GB — in 13.6 seconds with no Gen2 collection at
all. That is the streaming claim measured rather than argued.

**Pathological backtracking**, and it is real. §11 says ordered choice backtracks fully
inside a rule, and full backtracking over a repetition whose body can be cut several ways
is exponential — measured here on a match that fails at the very end, so every cutting is
tried:

| grammar | 16 chars | 18 | 20 | 22 |
| --- | --: | --: | --: | --: |
| `(['a']+)+ & 'b'` | 3.5 ms | 14.8 | 62.4 | 258 |
| `("a" \| "aa" \| "aaa")* & 'b'` | 1.2 ms | 4.1 | 14.9 | 57.6 |
| `("a" \| "aa")* & 'b'` | 0.2 ms | 0.4 | 1.2 | 3.8 |
| `Inner+ & 'b'`, `Inner = ['a']+` | 0.09 ms | 0.005 | 0.001 | 0.001 |

Four times the work for two more characters in the first row, which is 2ⁿ; the second is
the tribonacci count of the ways to cut a run, the third the Fibonacci one. Nothing here
memoizes, so a grammar that offers a repetition several ways of consuming the same text
pays for all of them.

The last row is the same shape with a rule boundary in it, and it is the answer rather
than a workaround. **A call answers once** (§4): `Inner` takes the whole run, is never
asked for a shorter one, and the enclosing repetition has nothing to enumerate. So the
engine's one deliberate limitation is also what makes the exponential case avoidable — by
naming the inner run, which is what a reader wants the grammar to say anyway.

**Generated code size**, which is large and is meant to be:

| grammar | its lines | generated | of which support |
| --- | --: | --: | --: |
| `Csv.gram` | 4 | 558 | 19 |
| `Feed.gram` | 13 | 1882 | 281 |
| `Url.gram` | 32 | 3765 | 281 |
| `JsonExample` | 30 | 2103 | 19 |

A hundred lines of C# per line of grammar, and the ratio holds because the machines are
what dominate: one state per position a rule can be in, each with its comment saying
which notation it came from. The support library at the end is a fixed cost and a small
one — 19 lines where nothing streams, 281 where it does — so a second grammar in the same
project pays the machines again and the support again, since both are emitted per host
class rather than shared (§6.1).

Nothing here is optimized for size and it should not be: the file is read by a compiler,
and every line of it exists so that no allocation, no virtual call and no closure exists
at run time. What the number is worth knowing for is compile time in a project with many
grammars — which is measured above, at 1.5 ms each.

**Nesting depth**, above: about 2700 levels, and why.

**One grammar compiled**: 1.5 ms for the URL grammar of `examples/`, in Release. That is
what an editor used to pay per keystroke per grammar, and is why the pipeline was
narrowed rather than left as it was.

Everything the architecture claims now has a number behind it.

## What the tests cover

`SemanticTests` is a corpus of small grammars run against real input, and grammars
that must be refused. `SnapshotTests` compiles one whole grammar and compares the
generated file with the one checked in, so a change to code generation is a diff.
`GeneratorDriverTests` drives the generator in memory; the test project also has the
generator attached as an analyzer, so it runs over the tests' own sources.
`RegexDifferentialTests` runs matched pairs of a `.gram` grammar and a regular
expression over every string of a small alphabet and requires them to agree — the check
that would have found the backtracking defect above in seconds.

`UrlTests` runs the URL grammar of §7.3 and reads the captures back by reflection;
`GeneratedApiTests` asks the compiler the same questions about the same grammar, so a
member that stopped being generated fails the build rather than an assertion.

`ExampleTests` runs everything under `examples/`, and its most useful tests are the ones
that compare two examples rather than either against a written-down answer: the same
expression through a grammar of levels and a grammar of strengths, and the same
expression through five rules and one, compared as whole trees by record equality. A
number in a test is a number somebody decided; two implementations disagreeing is a
defect neither of them can hide.

Every diagnostic the compiler can raise has a test that raises it — all twenty-nine, and
that is checked rather than assumed.
