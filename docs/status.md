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
| standard library `any none eol eof` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `Trivia` by shadowing | ✓ | ✓ | ✓ | ✓ | ✓ |
| publication `parse match find find all` | ✓ | ✓ | ✓ | ✓ | ✓ |
| the position a refusal names | — | — | — | ✓ | ✓ |
| captures `name:` | ✓ | ✓ | ✓ | ✓ | ✓ |
| repeated captures of a rule, `items: Row*` | ✓ | ✓ | ✓ | refused | ✗ |
| construction `=>` | ✓ | ✓ | ✓ | dropped | ✗ |
| rule types `: T` | ✓ | partial | ✗ | ✗ | ✗ |
| guards `where` | ✓ | ✓ | ✓ | ignored | ✗ |
| inline C# `@(...)` | ✓ | ✓ | ✓ | ignored | ✗ |
| C# references `@Name` | ✓ | partial | ✗ | ✗ | ✗ |
| parameterized rules `R(n)` | ✗ | ✗ | ✗ | ✗ | ✗ |
| keyword boundaries §4.6 | ✗ | ✗ | ✗ | ✗ | ✗ |
| `recover` on a repetition §8.2 | ✗ | ✗ | ✗ | ✗ | ✗ |
| value failures `bool M(…, out T)` §8.1 | ✗ | ✗ | ✗ | ✗ | ✗ |
| document repair, §6 of the engine plan | ✗ | ✗ | ✗ | ✗ | ✗ |
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

## What a refusal says

`TryParseR` and its siblings report a position, and until now it was always zero.
It is now the furthest the input could be followed before the match gave up — which,
for a parser that backtracks, is the only position worth naming: the last thing tried
is usually shallower than the best thing tried.

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

Four things a capture can be that are recognized and not built, each `GRAM4006` rather
than a silent drop:

- `items: Row*` — a repeated capture of a rule that builds. §7.3 says `Row[]`, and that
  needs a growable slot with a mark to truncate it to on backtracking. Repeated
  captures of *text* do work: §10 binds a capture tighter than a quantifier, so
  `digits: ['0'..'9']+` is one capture repeated, and §7.3 gives it the text joined —
  which is what it produces, as the extent of the whole run.
- a capture inside a repetition without being the whole of what repeats — the text of
  the iterations could not be told from the text between them.
- a capture inside a lookahead, which is a machine of its own that answers yes or no.
- `GRAM4007`: one name captured twice with different types.

Two deviations from §7.3, both deliberate:

- the generated type is a `sealed class`, not a `record`. A positional record needs
  `IsExternalInit`, which lives in a namespace this generator must not emit into, and
  the consumer's language version is not ours to assume.
- a rule's type is always generated. `: @T` — naming a C# type for a rule to build —
  parses and is not wired up, and neither is `=>`. Both wait on C# name resolution.

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
