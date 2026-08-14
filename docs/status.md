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
| rule types naming another rule §4.1 | ✓ | ✓ | refused | ✗ | ✗ |
| guards `where` §8.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| inline C# `@(...)` in `where` and `=>` | ✓ | ✓ | ✓ | ✓ | ✓ |
| C# names inside `@(...)`, e.g. `@int.Parse` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `@Name` as an operand or predicate §7.1 | ✓ | partial | refused | ✗ | ✗ |
| direct left recursion §4.3 | ✓ | ✓ | ✓ | ✓ | ✓ |
| binding powers `<< n` `>> n` §4.3.1 | ✓ | ✓ | ✓ | ✓ | ✓ |
| indirect left recursion | ✓ | ✓ | refused | ✗ | ✗ |
| parameterized rules `R(n)` | ✗ | ✗ | ✗ | ✗ | ✗ |
| keyword boundaries §4.6 | ✗ | ✗ | ✗ | ✗ | ✗ |
| `recover` on a repetition, with `=>` §8.2 | ✓ | ✓ | ✓ | ✓ | ✓ |
| the names `recover` supplies §8.2 | — | — | — | ✓ | ✓ |
| `recover` without `=>`, dropped and reported §8.3 | ✓ | ✓ | ✓ | ✓ | ✓ |
| a second `recover` in one rule | ✓ | ✓ | refused | ✗ | ✗ |
| a `=>` that throws inside `recover` §8.2 | — | — | — | ✗ | ✗ |
| value failures `bool M(…, out T)` §8.1 | ✗ | ✗ | ✗ | ✗ | ✗ |
| `RecognitionResult<T>`, `Outcome`, `Diagnostic` §7.5 | — | — | — | ✗ | ✗ |
| document repair, §6 of the engine plan | ✗ | ✗ | ✗ | ✗ | ✗ |
| leading and trailing `Trivia` §4.5 | — | — | — | ✓ | ✓ |
| streaming input §6.2, §8.3 | ✗ | ✗ | ✗ | ✗ | ✗ |
| incremental parsing | ✗ | ✗ | ✗ | ✗ | ✗ |

## Backtracking, and where it stops

Inside a rule, backtracking is full. A rule compiles to a state machine with an
explicit stack of the points that could have gone another way: entering an alternative
records the next one, taking one more repetition records the option of having stopped.
Failing anywhere resumes at the most recent of them, and nothing is given up until the
stack is empty. So `'a'? & 'a'` matches `"a"`, and `("x" | "xy") & 'y'` matches `"xy"`.

**Backtracking does not cross a rule boundary.** A call is a call: it answers once,
with the first match it finds, and cannot be asked for another. So

```dotgram
Start = Name & 'y'
Name  = "x" | "xy"
```

does not match `xy`, though the same expressions written in one rule would. Whether
that stays this way is a language question — PEG answers once at rule boundaries by
design, and .NET regular expressions have no rule boundaries to answer at — and it is
not settled. What is settled is that it is written here rather than discovered.

The same boundary shows up in publication: `parse R` asks `R` for a match and then
checks the input ended, and cannot send `R` back for a longer one if it did not.

**Nesting depth is bounded by the process stack, and the bound is about 2700.** The
machine takes recursion out of a rule and not out of a grammar: `Expr = '(' & Expr & ')'`
is an ordinary C# call, so a thousand brackets are a thousand frames. Measured on the
default 1 MB stack, `((( … x … )))` survives 2600 levels and overflows by 2800.

The number is what it is for a reason worth knowing, because it is a cost of a decision
made elsewhere. Each recognizer opens with

```csharp
global::System.Span<int> bt = stackalloc int[48];
```

— the backtracking stack, sized so that nothing is allocated on the heap in the common
case and `Grow` takes over when 48 is not enough. That is 192 bytes of the C# stack per
rule invocation, and it, not the rest of the frame, is what sets the depth. `Grow` helps
with backtracking *inside* a rule and does nothing for nesting *between* rules.

So input length and nesting depth are different limits, and only the first is about to
get better: streaming makes a longer file readable and leaves the bracket count exactly
where it is. A grammar meant for adversarial input should bound its own nesting, and a
`StackOverflowException` cannot be caught in .NET — the process goes.

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

- **`position` is `int`, not `long`.** §8.2 makes it `long` for a feed larger than an
  `int` can index. Input is a `string` today, so `int` is exact; it widens when
  streaming arrives, and widening a parameter is not a change any factory has to notice.
- **`text` and `span` stop where the synchronization point begins.** `eol` separates the
  elements and is not part of one, so a rejected `b1b\n` is three characters, not four.
- **`message` is not the expected set.** It says which rule the element should have been
  and where the input stopped being one — `Input does not match 'Row' at 43.` The set of
  what could have appeared there would say more, and is not carried yet.

Which of the seven a factory asked for is read out of its C#, because §8.2 has counting
lines cost a scan and only a factory that named `line` should pay for one. The reading is
a whole-word search over the text, so it over-approximates: `line` inside a string
literal counts as asked for. That direction is the safe one — a name that was written is
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

**A `=>` that throws inside a recovering repetition is not caught.** §8.2 says it is, and
treats the throw as a value failure to be recovered from — the element was recognized
whole, so there is nothing to skip and the factory's own rejection stands in for it. What
happens today is that the exception leaves the parse: `DecimalCalculator.Evaluate("1 . 5")`
in the examples throws `FormatException` out of a `decimal.Parse` in a `=>`, and the tests
assert exactly that rather than a recovered element. Worth knowing before writing a `=>`
that can fail.

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
static int Construct_Number(string text, string digits) =>
    int.Parse(digits, CultureInfo.InvariantCulture);
```

A method rather than an expression written where the value is assigned, and that is
what makes the capture names usable at all: inside a recognizer they would have to
dodge every local it has, and a capture called `p` or `state` would collide with the
machine itself. `text` is supplied — the matched extent, §7.3 — and a capture may take
the name instead.

## A guard asks the values

`where @(…)` runs **during** the match, which is what makes it recognition: saying no
is a non-match and a sibling alternative is tried, exactly as §8.1 has it. It becomes a
method of its own for the same reason a `=>` does, and takes the same `text`.

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

`@Name` standing where an operand goes (§7.1) is refused with `GRAM4005`, the same
diagnostic an unbuilt C# predicate inside an element set gets. It used to lower to an
element set with nothing in it: a rule that compiled, ran, and matched nothing whatever
the input was. Only `@(...)` inside a `where` or a `=>` reaches C# today, and the names
inside one — `@int.Parse` and the like — resolve against the host compilation.

## Two deviations from §7.3, both deliberate

- the generated type is a `sealed class`, not a `record`. A positional record needs
  `IsExternalInit`, which lives in a namespace this generator must not emit into, and
  the consumer's language version is not ours to assume.
- `Match<T>.Value` is `T` rather than `T?`, so a failed match holds `default` and
  `IsSuccess` is what says so. An unconstrained `T?` needs a language version this
  generator may not assume, and `T` has to be unconstrained now that a rule may declare
  itself `: @int`.

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

## What has been measured

Two of the architecture's claims now have numbers rather than reasoning behind them.

**Against `Regex`.** `benchmarks/` runs the URL grammar against the same language written
as a regular expression, and refuses to time anything until both agree on every part of
every input. Generated parsing comes out 2.3–6.3× faster than interpreted `Regex` and
between 1.1× and 1.9× faster than `RegexOptions.Compiled` — the same order as the best the
BCL does, not a different class. Allocation is at parity, because both materialize the
parts as strings. `benchmarks/README.md` has the table and what not to read into it.

**Nesting depth**, above: about 2700 levels, and why.

Still unmeasured, and worth knowing before anyone relies on it: throughput on a large
feed, pathological backtracking, generated code size, and how long the generator takes to
re-run when one file of many changes.

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
