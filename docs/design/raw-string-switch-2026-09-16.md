# Raw-string computed-switch trial, 2026-09-16

## Result

Rejected before runtime benchmarking. A computed switch inside a raw-string terminal
prevents construction of the shared lexical automaton. Building ExpressionLanguage
reports GRAM5004 and falls back to character parsing for the affected publications.
Warnings are errors in this repository, so the Release build fails. The production
grammar was restored; no generator changes are required by this experiment.

## Candidate

At baseline a42ecdd, replace RawText3, RawText4 and RawText5 with:

```dotgram
RawText : @string
    = quotes: '"'{3,5} & switch @(quotes.Length) {
        case 3: [^ '"'] & RawPlain('"'{1,2}) & '"'{3}
        case 4: [^ '"'] & RawPlain('"'{1,3}) & '"'{4}
        case 5: [^ '"'] & RawPlain('"'{1,4}) & '"'{5}
    } => @(ExpressionParser.Raw(parserText, quotes.Length))
```

Replace the three plain alternatives in RawAny with RawText and the body of
RawString with `t: RawText => @(t)`. Interpolated and longer-delimiter forms remain
unchanged. This isolates the smallest value-dependent delimiter selection before
attempting the other forms. Semantic equivalence was not established: the backend
compatibility gate failed first.

Reproduction:

```powershell
dotnet build src/DotGram.ExpressionLanguage -c Release -f net10.0 --no-restore -v:q
```

The candidate reports GRAM5004 twice at ExpressionParser.cs:156. Diagnostic excerpt:
"its terminals cannot all be read together". The failed build took 54.98 seconds;
this is a diagnostic run, not a controlled generation/build benchmark.

## Why this fails

LexicalAutomaton.Gather explicitly rejects Node.Choice with non-null Selection.
TerminalInventory cannot build the combined token recognizer and the requested
lexical backend falls back. Direct-reader support for computed switches in syntax
rules does not provide computed selection inside lexical patterns.

The current separate raw-string patterns already participate in joint lexical
recognition. They are not necessarily sequential attempts over each complete string.
A source-level reduction in rule count therefore does not establish a runtime win.

## Follow-up

Do not suppress GRAM5004 merely to accept this refactoring: that would change the
backend for the large parser and lose the intended local comparison. Runtime,
allocation and generated-source comparisons were not performed because there is no
accepted equivalent lexical candidate.

A future experiment would first need either a terminal-local dynamic recognizer that
preserves shared lexing, or compiler-proven specialization of this bounded delimiter
case into regular patterns. Arbitrary user C# selectors cannot simply be erased or
replaced with the union of their branches: selected failure and selector observations
must remain observable. Such lexer work is a separate feature, not a grammar-only
optimization. Until then, keep the existing raw-string rules.

## Restoration check

The restored Release net10.0 project built with zero warnings and errors. Git confirms
the production ExpressionParser.cs is unchanged from the baseline. No runtime changes
were retained, so no new parser tests were introduced for the rejected candidate.
