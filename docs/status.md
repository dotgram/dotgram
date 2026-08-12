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
| ordered choice `\|` | ✓ | ✓ | ✓ | ✓ | partial — see below |
| quantifiers `? * + {n} {n,m}` | ✓ | ✓ | ✓ | ✓ | partial — see below |
| lookahead `?=` `?!` | ✓ | ✓ | ✓ | ✓ | ✓ |
| rules calling rules, recursion | ✓ | ✓ | ✓ | ✓ | ✓ |
| scopes, `using`, shadowing | ✓ | ✓ | ✓ | ✓ | ✓ |
| standard library `any none eol eof` | ✓ | ✓ | ✓ | ✓ | ✓ |
| `Trivia` by shadowing | ✓ | ✓ | ✓ | ✓ | ✓ |
| publication `parse match find find all` | ✓ | ✓ | ✓ | ✓ | ✓ |
| captures `name:` | ✓ | ✓ | ✓ | dropped | ✗ |
| construction `=>` | ✓ | ✓ | ✓ | dropped | ✗ |
| rule types `: T` | ✓ | partial | ✗ | ✗ | ✗ |
| guards `where` | ✓ | ✓ | ✓ | ignored | ✗ |
| inline C# `@(...)` | ✓ | ✓ | ✓ | ignored | ✗ |
| C# references `@Name` | ✓ | partial | ✗ | ✗ | ✗ |
| parameterized rules `R(n)` | ✗ | ✗ | ✗ | ✗ | ✗ |
| keyword boundaries §4.6 | ✗ | ✗ | ✗ | ✗ | ✗ |
| recovery §6 of the engine plan | ✗ | ✗ | ✗ | ✗ | ✗ |
| streaming input §6.2 | ✗ | ✗ | ✗ | ✗ | ✗ |
| incremental parsing | ✗ | ✗ | ✗ | ✗ | ✗ |

## The one that is not a missing feature but a wrong one

**Backtracking is not full, and the specification says it is.**

`syntax.md` §10 says ordered choice backtracks fully, and rests a design decision on
it — there is no commit point, so a rule means the same thing everywhere. The
generated recognizer does not do this. A choice retries its own alternatives at the
position it started from, and that is all; once an alternative or a repetition has
returned a length, nothing later in the sequence can ask it for another one.

So these fail, and should not:

```dotgram
Start = 'a'? & 'a'          // input "a"
Start = 'a'* & 'a'          // input "a"
Start = ("xy" | "x") & 'y'  // input "xy"
```

The last one is the specification's own counterexample, the one §10 uses to explain
why alternatives may never be reordered.

This is a property of the recognizer's shape — a function returning one end position
has no way to be asked for the next — so fixing it is a decision about that shape, not
a patch. Nothing that depends on execution order should be built until it is settled:
typed values and speculative rollback designed apart do not meet in the middle.

## What the tests cover

`SemanticTests` is a corpus of small grammars run against real input, and grammars
that must be refused. `SnapshotTests` compiles one whole grammar and compares the
generated file with the one checked in, so a change to code generation is a diff.
`GeneratorDriverTests` drives the generator in memory; the test project also has the
generator attached as an analyzer, so it runs over the tests' own sources.

What is missing is differential testing against `System.Text.RegularExpressions` over
the subset where the two agree by design. That would have found the backtracking
defect above in seconds, and it is the next thing to add.
