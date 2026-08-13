# .Gram — the language and its bond with C#

The engine plan lives in [`implementation.md`](implementation.md). Nothing decided
there is a decision about the language.

**This is a specification, not a report.** It says what `.Gram` is, in the present
tense, whether or not the compiler does it yet — and today a good deal of it does not.
[`status.md`](status.md) says which parts are real.

`.Gram` is a typed recognition notation for .NET. A grammar describes how a stream
of `TIn` input items becomes a typed `TOut` result; a source generator turns it into
ordinary C#.

This document covers the notation and its seam with C#, nothing else. Every choice
here answers to one criterion: **each piece of notation means exactly one thing, and
that thing is what the notation already means in C# or in .NET regular expressions.**

---

## 1. The file and the host class

A grammar lives in a `*.gram` file beside a partial class:

```csharp
[Gram]                                  // looks for FeedGrammar.gram
public partial class FeedGrammar;

[Gram("formats/feed.gram")]             // or an explicit path
public partial class FeedGrammar;

[Gram("""                               // or the text right here
    Digits = ['0'..'9']+
    """)]
public partial class Numbers;
```

The grammar file:

```dotgram
@using System;
@using MyApp.Model;
using Lexical;

Header = "H" & '|' & date: Date & '|' & source: Text & eol
```

`@using X.Y;` imports a C# namespace, `using X.Y;` a grammar scope (§2). The syntax
is C#'s own directive, semicolon and all; there is no module system of our own for
C# — project references already are one.

---

## 2. Two namespaces: `@` and its absence

One rule, no exceptions and no clever resolution:

| Written | Looked up |
| --- | --- |
| `Name` | among grammar rules |
| `@Name` | among C# symbols (type, method, property) |
| `string`, `int`, `bool`, `char`, `decimal`, … | always a C# type — these are keywords and cannot name a rule |

`@` never qualifies or overrides — it switches namespace. There is no fallback in
either direction (a bare name that sometimes finds a C# type and sometimes generates
one): that is the single source of ambiguity worth removing outright.

```dotgram
Row = ...                // type Row will be generated
Row : @Row = ...         // use the existing C# type Row
Id  : string = ...       // the result is a string
```

More precisely, `@` is a **transition into C#**, and what it applies to may be a
directive as well as a name:

```dotgram
@using System.Text;      // import a C# namespace
using Lexical.Numbers;   // import a grammar scope

@Encoding.UTF8           // a static C# property
Lexical.Token            // a rule from a grammar scope
```

Otherwise `using System;` and `using Lexical;` would look identical while resolving
in different namespaces — exactly the implicit context dependency `@` exists to
remove.

Of all keywords `@` applies only to `using`, and that is not an exception: `scope`,
`parse`, `where` and the rest are constructs of `.Gram` with no C# counterpart, so
there is nothing to transition into. `using` is the one directive that exists in both
languages. The model is Razor's, where `@using`, `@model` and `@inherits` mean
precisely "C# follows".

A name, a call, or a **parenthesized C# expression** may follow `@`:

```dotgram
@DateOnly                a C# type
@int.Parse(text)         a C# method call
@(y * 10000 + m * 100)   a C# expression
```

Those parentheses are not `.Gram` notation but C#'s own: they are balanced and so
delimit the insertion themselves. The generator never parses C# — it only finds the
matching `)`, minding literals, and hands the text to the compiler.

The block form (Razor's `@{ ... }`) is not supported, and not arbitrarily: the
positions that accept C# accept a **value**, and a block of statements is code. Code
belongs in the partial class next door, where there is a debugger and refactoring
(§7.4). The reverse transition (`@:`, `<text>`) and the `@@` escape are unnecessary:
a grammar is never nested inside C#, and a literal `@` occurs only inside quotes.

---

## 3. Recognition expressions

### 3.1 Elementary

Each tests exactly one input item.

```dotgram
'x'                     character literal
"text"                  string literal (a sequence of characters)
['a'..'z']              element set: a range
[Letter | @IsDigit]     element set: a union
[\p{Lu} | \p{Nd}]       element set: Unicode categories
[^ '|' | '\n']          element set: complement (any item except)
any                     any single item
```

`\p{...}` is the .NET regular-expression spelling, with the same category names
(`Lu`, `Ll`, `Nd`, `Zs`, `Pc`, …). Character input only.

`any`, `none`, `eol`, `eof` and `Trivia` are ordinary standard-library rules rather
than keywords: a rule of the same name shadows them. Whitespace handling rests on
exactly that (§4.5).

Square brackets in an expression are always an element set, testing **one** input
item. Inside them `|` is set union rather than ordered choice, and only ranges,
characters and references to other elementary rules are allowed. The brackets are
required: without them `Letter | @IsDigit` would be indistinguishable from structural
alternation, which means something else.

### 3.2 Composition

```dotgram
a & b                   sequence
a | b                   ordered choice
( a & b )               grouping
```

A connector between operands is always required. `.Gram` has no juxtaposition: two
expressions cannot sit side by side without `&` or `|`. That is precisely why rules
need no separator (§4.4).

`Trivia` is inserted between the operands of a sequence — an empty rule by default,
so by default nothing is inserted (§4.5).

### 3.3 Quantifiers — postfix, as in regular expressions

```dotgram
X?                      zero or one
X*                      zero or more
X+                      one or more
X{3}                    exactly three
X{2,4}                  two to four
X{2,}                   two or more
```

Every quantifier is postfix, and together they form one consistent family. This is
the only place braces appear in an expression, and there they mean a count and
nothing else.

The body of `X*`, `X+` or `X{n,m}` must consume at least one input item per
iteration — a build-time check, not runtime behaviour. `(A?)*` and `(?!A)*` are
rejected by the compiler.

### 3.4 Lookahead

```dotgram
?=X                     positive: X matches here, nothing is consumed
?!X                     negative: X does not match here, nothing is consumed
```

The spelling comes from .NET regular expressions. `?=X` is a recognizer in its own
right and **produces X's value** (which can be captured) without moving the input.
`?!X` produces nothing. Neither is sugar over the other.

Postfix `?` and prefix `?=`/`?!` do not clash: the postfix follows an operand, the
prefix opens one, and between operands there is always `&` or `|`.

```dotgram
SmallNumber = n: ?=Number & where @IsSmall(n) & value: Number => value
Identifier  = ?!Keyword & Letter & LetterOrDigit*
```

### 3.5 Capture

```dotgram
name: X
```

The colon reads "this name — that". What stands on the right is decided by position,
and there are exactly two positions:

| Where | On the right | Example |
| --- | --- | --- |
| in an expression — a **capture** | a recognizer | `symbol: Text`, `value: (A & B)`, `y: Digits(4)` |
| in a rule header or a parameter | a **type** | `Row : @Row = …`, `n: int` |

Usually the difference is invisible, because a rule's name is by default the name of
its type (§4): in `symbol: Text` the right side is the rule `Text`, and the captured
value's type is `Text` too. They part company where a rule declares a type of its
own: after `Date : @DateOnly = …`, the capture `d: Date` names the **rule** `Date`
while the value has type `@DateOnly`.

A capture cannot take a type: a group and a call are allowed on its right
(`value: (A & B)`, `y: Digits(4)`), and those are recognizers, not types.

### 3.6 Guard

```dotgram
where @Predicate(args)
```

An operand that consumes no input and produces no value: if the predicate returns
`false`, the whole sequence did not match.

The keyword is not ceremony. Without it, a call site does not say whether the input
moves: `@ParseGuid` (an external recognizer), `@IsLetter` (an element predicate) and
`@IsSupported(symbol)` (a test on a captured value) look identical and differ only by
a signature living in another file. And `@IsUpper(ch)`, where `ch` is a capture, is
outright ambiguous: a test on the captured character, or on the current input item?

Hence the wider scheme: there are exactly two positions where C# is called for a
value, each with its own marker — `where` asks (`bool`), `=>` answers (a result).
Neither touches the input. Everything else with `@` is a recognizer, and recognizers
move it.

The word is C#'s, where both of its uses are restrictions rather than branches:
`where` filters in LINQ and constrains in a generic declaration. The meaning here is
the same — narrow the set of matches by a condition.

**The guard's position is the author's choice and carries meaning.** A guard has no
outcome of its own — it is an ordinary operand, and its failure means whatever the
failure of any operand in that place means:

```dotgram
SmallNumber = n: ?=Number & where @IsSmall(n) & value: Number => value
```

nothing has been consumed before the guard (lookahead does not move the input), so
`SmallNumber` simply does not begin here and a sibling alternative may be tried;

```dotgram
Row = "D" & '|' & symbol: Text & where @IsSupportedSymbol(symbol) & ...
```

`"D"` has been consumed — the line is plainly a data row, and failure should produce
a diagnostic about the unsupported symbol rather than a silent fall-through to
another alternative.

The first reading is what the language does: ordered choice backtracks fully and there
is no commit point (§11), so a failing guard is a non-match and a sibling is tried.

The second is what one would want in the `Row` case, and saying so is the one thing
still missing — see §11. Note that it is a question about diagnostics, not about
parsing: whichever way it is answered, the guard's position stays the author's choice
and still decides how much work is thrown away and where the message points.

### 3.7 Construction

```dotgram
=> value
```

The other half of the pair from §3.6: `where` asks, `=>` answers. It gives the
alternative its value, never touches the input, and runs once the alternative has
matched in full.

**It binds to one alternative, not to the rule body** — by §3.8 it sits below `&` and
above `|`. So every branch of a `|` builds its own result, and no parentheses are
needed for that:

```dotgram
Expr = value: Number           => value
     | '(' & inner: Expr & ')' => inner
```

A name, a call or a parenthesized C# expression may stand on the right (§2). Captures
are visible there as ordinary local variables, along with two implicit names, `span`
and `text` (§4.1):

```dotgram
Number : int    = ['0'..'9']+ => @int.Parse(text)
Point  : @Point = x: Number & ',' & y: Number => @(new Point(x, y))
Add             = l: Expr & '+' & r: Expr     => Add(l, r)
```

The last line carries an important distinction: `Add` without `@` constructs a type
owned by the grammar, whereas `@Add` would call a host method. Same rule as §2,
nothing new.

**`=>` is not always required.** When it is absent and captures are present, they are
matched to the result type by name — case 3 in §4.1, which covers most rules. `=>` is
for where automatic matching does not suffice, or where being explicit is worth it;
when present it wins over the other cases.

Like `where`, construction may be invoked more than once or discarded on backtracking,
so it is bound by the same obligation in §7.2: no irreversible side effects.

### 3.8 Operator precedence

Highest to lowest:

```text
1.  postfix quantifiers   X?  X*  X+  X{n,m}
2.  prefix lookahead      ?=X  ?!X
3.  capture               name: X
4.  sequence              a & b
5.  construction          => expr
6.  alternation           a | b
```

`=>` binds to a single alternative rather than to the whole rule body, which is what
lets each branch of a `|` construct its own result.

---

## 4. Rules

```dotgram
Name = pattern                          // the result type is named Name
Name : Type = pattern                   // the result type is given explicitly
Name(params) : Type = pattern           // a parameterized rule
```

**A declaration always begins with the rule's name** — the form without a type is not
an exception but its degenerate case, where rule name and type name coincide. The
colon here stands in type position (§3.5): a result type is on its right, not a
recognizer.

```dotgram
Row  = "D" & '|' & symbol: Text & '|' & quantity: Decimal & eol
Feed : FeedItem[] = Header & Row* & Trailer & eof
Id   : string = Letter & LetterOrDigit*
Date : @DateOnly = y: Digit{4} & '-' & m: Digit{2} & '-' & d: Digit{2}
```

`T[]` in type position means "a sequence of T" — the same array notation as C#. In
expression position `[` opens an element set; the positions do not overlap, just as
an array type and an indexer do not overlap in C#.

### 4.1 A rule's result

1. There is `=> expr` — the expression gives the result.
2. The result type is `T[]` — a sequence. Every operand whose value is assignable to
   `T` joins it in order; `X*`, `X+` and `X{n,m}` contribute all of their elements;
   other operands contribute nothing. A rule with no assignable operand is a build
   error.
3. There are captures — they are matched to the result type by name (§7.3).
4. None of the above — the result is the matched extent: `string` gives the text,
   `SourceSpan` gives the bounds. Any other type requires an explicit `=>`.

Separately, the type `void` means "this rule produces no value". Such a rule
recognizes input but contributes nothing to captures or to a sequence — whitespace,
separators and other plumbing are declared this way.

```dotgram
Feed : FeedItem[] = Header & Row* & Trailer & eof
```

`Header`, `Row` and `Trailer` are assignable to `FeedItem` and enter the result in
order; `eof` is not assignable and contributes nothing.

A name, a call and a parenthesized C# expression are allowed in `=>` and `where`
(§2), with captures visible as ordinary local variables:

```dotgram
Number : int    = ['0'..'9']+ => @int.Parse(text)
Point  : @Point = '(' & x: Number & ',' & y: Number & ')' => @(new Point(x, y))
Row             = ... & where @(qty > 0 && symbol.Length == 4)
```

Besides captures, two implicit names are always in scope inside `=>` and `where`:
`span` (the current rule's `SourceSpan`) and `text` (the matched text, when `TIn` is
a character). A capture of the same name shadows them.

There is no limit on the size of an expression, but there is a recommendation: once
it stops reading at a glance it is better off as a named method — the generator will
declare a partial signature for it (§7.4), and the code moves into a `.cs` file with
a debugger and refactoring.

### 4.2 Parameters

A parameter is written like a capture — `name` or `name: Type` — because it does the
same thing: binds a name to something that came from outside.

```dotgram
Lex(item)               : item   = Trivia & item
List(item, sep)         : item[] = item & (sep & item)*
Padded(item, pad: char) : item   = pad* & value: item & pad* => value
Digits(n: int)          : int    = ['0'..'9']{n} => @int.Parse(text)
```

| Declaration | What it is | What is passed at the call site |
| --- | --- | --- |
| `item` | a recognizer; result type comes from the call site | a rule, a literal or a recognition expression |
| `item: Row` | a recognizer constrained to produce `Row` | the same, but obliged to produce `Row` |
| `n: int`, `t: @Tag` | a value | a literal or a previously captured value |

Which kind a parameter is follows from §2, with nothing new needed: **rules** live in
the grammar namespace, **types** in C#'s. That settles an ambiguity otherwise
unresolvable: `item: Row` is a recognizer producing `Row`, while `item: @Row` is a
ready value of type `Row`.

**There are no type parameters in the language.** A parameter's name is allowed in
type position instead: `: item` means "of whatever type `item` produces", `: item[]`
a sequence of those. That covers every known case, and there is nothing to generalize
over at run time anyway: a rule is specialized for each call site, where the argument
type is already concrete.

```dotgram
Numbers = List(Number, ',')      // : int[]
Name    = Lex(Identifier)        // : string
```

A value parameter is allowed anywhere a value is expected: in a quantifier count
(`{n}`), in the arguments of `@Method`, inside `@(...)`.

A recognizer parameter never becomes a delegate — specialization means calling it
costs exactly what calling the rule directly costs. A recursive parameterized call
that would spawn an unbounded number of specializations is rejected when the grammar
is built.

### 4.3 Recursion

Direct and indirect left recursion is rejected when the grammar is built. Loops are
written with quantifiers:

```dotgram
Expr = Term & (['+' | '-'] & Term)*
Term = Factor & (['*' | '/'] & Factor)*
Factor = Number | '(' & Expr & ')'
```

### 4.4 Rule separator

There is none and none is needed: a connector between operands is mandatory, so an
expression can never be "continued" by the next rule. An identifier followed by `=`,
`:` or `(`, in a position where the current expression is already complete, is always
a new rule. It cannot be a capture: a capture must follow `&` or `|`, and there the
expression does not count as complete.

### 4.5 Trivia — insignificant whitespace and comments

**Working proposal.**

The rule `Trivia` is always inserted between the operands of a sequence. It is empty
by default, so by default nothing is inserted:

```dotgram
// standard library
none                  = any{0}                 // zero repetitions: succeeds, consumes nothing
Trivia                = none
Whitespace            = ([' ' | '\t'] | eol)*
WhitespaceAndComments = (Whitespace | LineComment | BlockComment)*
```

A grammar to which whitespace is insignificant redefines one rule:

```dotgram
Trivia = WhitespaceAndComments
```

No directive, no mode: it is an ordinary rule, and `none` is expressed in the language
itself as `any{0}` rather than by a new primitive.

**Switching per block is the shadowing from §5**, not a separate mechanism:

```dotgram
Trivia = WhitespaceAndComments

scope Lexical
{
    Trivia = none                              // whitespace is significant here

    Identifier = ['a'..'z' | '_'] & ['a'..'z' | '0'..'9' | '_']*
    Number     = ['0'..'9']+
}

scope Syntax
{
    using Lexical;

    If = "if" & '(' & cond: Expr & ')' & then: Statement
}
```

**Scoping is lexical:** a rule uses the `Trivia` visible where it is **declared**, not
where it is called. A rule means the same thing wherever it is used.

**`Trivia` must be nullable** — it has to accept empty input, or the build fails. That
condition is what makes unconditional insertion safe: a second application consumes
nothing, so nothing is ever doubled and no rule of the form "insert after a literal
but not after a structural call" is needed. The single exception is one insertion at
the start of a published rule, for leading whitespace.

When `Trivia` is empty the insertions are dropped entirely during normalization:
nothing of them survives to run time.

A silent failure is possible here — a lexical rule that ended up by oversight in a
scope with non-empty `Trivia` will quietly accept `i f` as `if`. No mechanism catches
that, but a warning does: a rule whose operands all test a single input item is
almost certainly a mistake in such a scope.

### 4.6 Keyword boundaries

`KeywordBoundary` is a standard-library rule, `none` by default, naming the characters
that continue a word:

```dotgram
KeywordBoundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']
```

Once it is not empty, every string literal **whose characters all fall in that class**
picks up a `& ?!KeywordBoundary`, so `"if"` no longer matches the start of `iffy`.
Whether a literal qualifies is decided when the grammar is built: `"if"` gets the
check, `"("` does not.

Same shape as `Trivia` (§4.5), and for the same reason: a rule, ordinary shadowing,
and the insertion dropped entirely while the rule is empty. A regex or a feed grammar
pays nothing; a language grammar pays one line.

The boundary check goes **before** the trivia insertion. The other order would ask
whether a letter follows the whitespace rather than whether it follows the keyword.

---

## 5. Scopes

```dotgram
@using System;

Common = ...

scope Lexical
{
    Token = ...
}

scope Syntax
{
    using Lexical;              // import a grammar scope

    Unit = Token*               // instead of Lexical.Token
}
```

The top of a file is an implicit global scope. The `{ }` after `scope Name` is a block
of declarations, not an expression; in expression position braces mean a repetition
count and nothing else (§3.3). An inner scope sees the outer one; a rule of the same
name shadows the outer one; the qualified name `Scope.Rule` is available from outside.

`using X;` without `@` brings the names of scope `X` into the current scope
unqualified. Import directives stand at the top of the file or at the top of a `scope`
block — where C# expects them. If two imports supply the same name, the error is
raised at the use site rather than at the import, and is settled by qualification, as
in C#.

Every scope becomes a nested static class in the generated code.

---

## 6. Publication

A rule on its own creates no public API. A directive does:

```dotgram
parse Feed
find Row
parse Feed as ReadFeed          // an explicit name instead of ParseFeed
```

**There are two, and the whole of the difference is whether input that does not match
is allowed to sit between the matches.**

| Directive | What it says | Generated |
| --- | --- | --- |
| `parse R` | the whole input is an `R` | `R ParseR(input)` — throws `FormatException`<br>`Match<R> TryParseR(input)` |
| `find R` | there are `R`s inside something else | `FindR(input)` — a lazy sequence of `Match<R>` |

`find` is a sequence and needs no companion for "all of them" or "the first one":
`First()`, `Where()`, `Take()` are LINQ's job and it would be strange to reinvent
three of them. It yields occurrences as it finds them, so a document with a million
matches costs one at a time rather than an array of a million.

A rule whose value is a sequence — `Feed : FeedItem[]` — is published with `parse`
like any other, and what comes back is that sequence. Reading a feed is not a third
directive: it is one `parse` of a rule that happens to be a list, and §6.3 decides
whether the list is materialized or walked.

Anything else is a consequence rather than a directive. Where a match may sit is the
grammar's business, how much is held is the input's (§6.3), and picking things out of
a sequence is the caller's.

### 6.1 The result

```csharp
public readonly struct Match<T>
{
    public T?      Value    { get; }   // null when it did not match
    public string? Error    { get; }
    public int     Position { get; }   // where it matched, or where it gave up
    public int     Length   { get; }

    public bool IsSuccess { get; }
}
```

**No `out` parameters anywhere.** `int.Parse` and `int.TryParse` are a pair because an
`int` has no room to carry a failure; a result that has room does not need a second
shape for it, and every later thing a match might want to say — what was expected,
which record it was, whether the record was broken rather than absent — is a field
here instead of another parameter on every signature.

What is left of the pair is a real choice, and it stays: `ParseR` asserts that the
input is an `R` and throws when it is not, `TryParseR` asks and answers. Assertion is
the common case in application code and deserves to stay one line.

`Position` is not only for failures. On a match it is where the match began, which is
what `find` is usually asked for; on a failure it is the furthest the input could be
followed before the match gave up, which for a parser that backtracks is the only
position worth naming.

### 6.2 Why the signatures use BCL types only

`.Gram` ships no runtime assembly: everything a generated parser needs is emitted
beside it, `internal`. A consumer therefore takes one analyzer package, acquires no
dependency, and has nowhere for a "generator of one version, runtime of another" skew
to come from.

The shape of the public API follows: an `internal` type cannot appear in the signature
of a public method. So by default only BCL types face outward — `string`, `int`,
`FormatException` (the very type `int.Parse` throws).

`Match<T>` and a rule's own type are not exceptions to this. They are generated from
one grammar into the assembly that uses it, so there are no two versions of them to
skew, and nothing crosses an assembly boundary. What §6.2 is about is the types that
*would* be shared.

**Shared types on demand.** When typed diagnostics are wanted, or when a parser is
exposed in a library's public API, one assembly declares:

```csharp
[assembly: GramRuntime]
```

and the generator emits `Diagnostic`, `SourceSpan`, `RecognitionResult<T>` and
`Outcome` into it as `public`, while assemblies referencing it bind to those instead
of emitting their own. `Match<T>` gains a typed `Diagnostic` beside its `Error`, additively.

The two modes are **strictly additive**: opting in only adds overloads and never
changes existing ones, so code written before opting in still compiles. If two
referenced assemblies both publish the shared types, that is compile error `GRAM0001`
rather than a silent pick between them.

### 6.3 The input type picks the execution mode

Each directive gains overloads, and which one is called decides how the parse runs.
There is no directive for this and no option: the choice belongs at the call site,
because it is a property of the data rather than of the grammar.

| Input | How it runs | Retains |
| --- | --- | --- |
| `string`, `ReadOnlySpan<char>` | everything in memory | all of it, and the result may be walked again |
| `IEnumerable<string>`, `TextReader` | one line at a time, buffer reused | one line, and the result is walked once |

The shape of what comes back does not change with the input: `parse` of a sequence
rule yields a sequence either way, and `find` yields one either way. What changes is
how much is held while it is walked.

The same grammar serves both. `Feed : FeedItem[] = Header & Row* & Trailer & eof`
checks that there is exactly one header, that the trailer is there and that nothing
follows — which is precisely what is lost when the caller chops the input into records
and parses them one by one.

**The streaming overloads are emitted only when the grammar can stream.** What decides
that is how far back the parser might have to return: a rule whose repeated element
always ends at a line boundary need never hold more than the current line, and the
overloads appear. A grammar where an alternative could reach back to the start of the
input gets no streaming overload, and a message saying which rule is responsible:

```text
'Feed' has no streaming overload — the alternative at Feed:3 may return to the
start of the input, so retention would be the whole file.
```

Which is the shared responsibility: the author picks an overload, and the compiler
offers one only where it provably works.

Positions inside a line are ordinary `int`. What crosses the publication boundary for
a streamed parse is a `long`, so an error at offset 8,432,109,553 can be reported as
such.

---

## 7. The bond with C#

This is the language's other half, not an appendix to it: the grammar describes
structure, C# describes meaning, and the seam between them has to be mechanical.

### 7.1 Classifying C# methods by signature

No attributes on methods are needed — a method's role follows from its signature and
the position it is called from:

| C# signature | Role | Called from the grammar as |
| --- | --- | --- |
| `bool M(char c)` | element predicate | `@M` in recognizer position |
| `bool M(ReadOnlySpan<char> input, ref int pos, out T value)` | external recognizer | `@M` in recognizer position |
| `T M(args…)` | value transformation | `=> @M(a, b)` |
| `bool M(args…, out T value)` | value transformation that may fail | `=> @M(a, b)` |
| `bool M(args…)` | guard | `where @M(a)` |
| — | inline expression | `=> @(expr)`, `where @(expr)` |

There is one rule to read this by: **a method taking the input and a `ref int pos` is
a recognizer; any other method never touches input at all.** `bool M(char)` in
recognizer position is an element predicate and the same method in `where` is a guard;
the positions do not overlap.

The external recognizer's signature is deliberately built from BCL types only: it is
the same whether or not shared mode is on (§6.2), and it needs no interface dispatch.

An inline `@(...)` expression plays the same role as a value transformation, only
without a name: it receives no input, sees captures as local variables, and is checked
by C#'s type system exactly where the generator placed it.

Overloads, generic methods, extension methods and nullable annotations are resolved by
ordinary C# rules.

### 7.2 What the C# side must guarantee

- A value transformation has no access to the input — it physically never receives it.
- An external recognizer must restore `pos` to its entry value on any outcome other
  than success.
- Code in `@Method`, `@(...)`, `where` and `=>` must be safe to invoke more than once:
  ordered choice and lookahead may call it repeatedly or discard its result. This is
  not a demand for mathematical purity — it is a ban on irreversible side effects.
- The same code must not depend on *when* it runs. A capture records where it matched
  and nothing more; the result is built once, after the match is known to have
  succeeded, from the alternative that actually matched. So `=>` and `@(...)` see the
  parse that happened, never a parse that was tried and given back — and how many
  attempts it took to get there is not observable from inside them.

### 7.3 Captures and building the result

Captures are matched to the result type by name, in a fixed order:

1. a constructor whose every parameter is covered by captures;
2. `init`/`required` properties;
3. an explicit `=> @Factory(...)` when neither fits.

Names are matched by one mechanical casing transform: the capture `symbol` fits the
parameter `symbol` and the property `Symbol`.

A handful of names are **supplied rather than captured**: declare a parameter with one
of them and the generator fills it in. They are listed in §8.2, where the same names
serve a rejected element — `span` and `text` for the extent and the input it covers,
`ordinal`, `line`, `column` and `position` for where it was.

A capture's own type follows from what it captures:

| Captured | Type |
| --- | --- |
| a rule reference | that rule's result type |
| a literal, an element set, or a group of those | `string` — the matched text |
| a quantifier over something that yields text | `string`, the text joined — not `char[]` |
| a quantifier over a rule that yields a value | `T[]` |

The two quantifier rows are what makes the regex-shaped case behave:
`scheme: ['a'..'z']+` gives `"http"`, while `items: Row*` gives `Row[]`. Both are the
same principle as §4.1 case 4 — where nothing produces a value of its own, the value
is the matched extent — applied one level down, at the capture.

A capture binds tighter than a quantifier (§10), so `scheme: ['a'..'z']+` is one capture
repeated rather than a capture of a run — and the two rows above are how that is read:
repeated text is the text joined, a repeated rule is an array of its values.

An optional capture and an empty run are different answers and stay different: `(sign:
'-')?` that did not match is null, while `digits: ['0'..'9']*` that matched nothing is
`""` — the text of no iterations.

```dotgram
Url = scheme: ("https" | "http" | "ftp") & "://" & host: Host
```

```csharp
public sealed record Url(string Scheme, Host Host);   // generated when no C# type exists
```

This is what a regex's named group becomes: a member of a known type, checked at
compile time, rather than `Match.Groups["scheme"].Value` looked up by string at run
time. And it can be typed all the way — `scheme: Scheme` with `Scheme : @UriScheme`
hands back the enum instead of the text.

When no accessible C# type exists for a rule, an ordinary `public sealed record` with
the same members is generated — not a bespoke node framework.

### 7.4 The other direction: partial declarations

For every `@Method` a grammar refers to and that is not implemented yet, the generator
emits a partial method declaration with the signature already worked out:

```csharp
// generated
public partial class FeedGrammar
{
    private static partial bool IsSupportedSymbol(string symbol);
}
```

A missing implementation becomes an ordinary C# compile error naming the exact
expected signature, rather than an error from the generator. The developer fills it
in:

```csharp
public partial class FeedGrammar
{
    private static partial bool IsSupportedSymbol(string symbol)
        => Symbols.Contains(symbol);
}
```

This is the mechanism `[GeneratedRegex]` already uses.

### 7.5 Recognition outcomes

Inside the language an outcome is an ordinary value, never an exception. The type is
emitted by the generator into the assembly itself (§6.2), `internal` by default:

```csharp
readonly struct RecognitionResult<T>
{
    public Outcome     Outcome    { get; }   // Success | NoMatch | Error
    public T?          Value      { get; }
    public SourceSpan  Span       { get; }
    public Diagnostic? Diagnostic { get; }
}
```

Exceptions appear only at the publication boundary, and only in the methods without a
`Try` prefix — where a .NET developer expects them. What is thrown is
`FormatException`: the same type as `int.Parse` throws, and one that requires nothing
shared between assemblies.

### 7.6 Mapping positions back

Generated code carries `#line` directives mapping it back into the `.gram` file. This
is not a convenience but a condition of the seam working at all: `=> @Add(l, r)` is
checked by C#'s type system, and when the types do not agree the error must appear on
the grammar's line rather than in a machine-written file. The same goes for
breakpoints and for "go to definition" in both directions.

---

## 8. Failure, recovery and streaming

A grammar that reads a feed has a requirement a grammar that reads a source file does
not: a million records, one of them malformed, and the answer must be a message about
that record and a parse that keeps going. Everything in this section is inert in a
grammar that does not ask for it.

### 8.1 Two kinds of failure, told apart by when they happen

The seam already exists — §7.2: `where` runs **during** the match, `=>` runs **after**
it, once the match is final and from the alternative that actually matched.

- **A recognition failure** is a shape the grammar does not describe. It happens during
  the match, and ordered choice may undo it and try something else. Only past a commit
  point (§8.2) does it stop being "try something else" and become an error.
- **A value failure** is a shape the grammar does describe holding a value it does not
  accept. It happens at construction, after the match is settled, and never backtracks.

`2026-02-31` is both, depending on where the work is done: a recognition failure if
`Date` cannot match it by shape, a value failure if it matches and `DateOnly` refuses
it. Which one an author gets is a design choice, and the second is cheaper — see §8.2.

A value failure is reported without an exception, by the same signature-reading rule as
everything else in §7.1: a transformation that may fail is written `bool M(args…, out T
value)` — the shape the BCL's own `int.TryParse` and `DateOnly.TryParse` already have.

There is a third kind that belongs to neither, and to no single record: the **envelope**
— a missing `Trailer`, input that did not end, a declared count that does not match.
It is settled when the whole parse finishes, which for a streamed parse is after the
records have long since been handed out.

### 8.2 `recover` — a repetition that survives a bad element

```dotgram
Feed : FeedItem[] = Header
                  & Row* recover eol
                  & Trailer & eof
```

`recover` marks one repetition and says three things about it:

1. **Inside it, consuming and then failing is an error, not a non-match.** That is the
   commit point, and it is what makes "this record is malformed" expressible at all:
   without it, a bad row is merely a row that did not match, the repetition ends, and
   the failure surfaces at the top of the file as "the feed does not parse".
2. **On an error the parser skips past the next match of the synchronization
   expression** — `eol` here — and starts the next iteration there. The ordinal
   advances, so a rejected record still occupies its place in the numbering.
3. **What follows the repetition is not tried on the error path.** An error means the
   element was there and was broken, not that the repetition ended.

A value failure inside a marked repetition is recovered from too, and more cheaply: the
element was recognized whole, so the position is already past it and there is nothing
to skip. Inside a marked repetition an exception thrown by `=>` is caught and treated
as a value failure; outside one, nothing is caught.

**It is opt-in because it cannot be the default.** The rule "consumed something, then
failed, therefore malformed" is wrong for ordinary grammars, and this language's own
example proves it:

```dotgram
IPv6 = (H16 & ':'){6} & LS32 | …
```

An iteration matches `H16` and fails on `':'` having consumed four characters. That is
a healthy backtrack, not a broken address. Marking the repetition is how an author says
which reading applies, and it changes nothing outside the repetition it is written on —
a rule still means one thing everywhere it is called.

**The synchronization expression is also the retention bound.** An element of a marked
repetition cannot reach back past the previous synchronization point, so a streamed
parse holds one element and not the file. This is what §6.3 promises to prove before
emitting a streaming overload; `recover eol` proves it by construction.

#### The failure factory

With a `=>`, a failed element becomes an element of the sequence instead of vanishing
from it:

```dotgram
Row* recover eol => @BadRow(ordinal, line, text, message)
```

The factory's result must fit the sequence's element type, exactly as a successful
element must — so this form needs a declared `: T[]` whose type can hold both. Failure
then needs no channel of its own: it arrives in the stream, in its place, and the
question of matching a rejection to the record it came from does not arise.

Without a `=>` the failed element is dropped and reported out of band (§8.3), which is
what a grammar that has no type to spare should do.

#### Positions, and the names that are filled in

The factory's arguments are matched by name, the way §7.3 matches captures, and these
names are supplied rather than captured:

| | |
| --- | --- |
| `ordinal` | which element of the repetition this is, counting rejected ones, from 0 |
| `line`, `column` | where it starts, for a person, from 1 |
| `position` | absolute offset, `long` — for a machine |
| `span`, `text` | its extent, and the input it covers |
| `message` | why it was rejected — only here, never in a capture |

`ordinal` and `line` are not the same number and neither substitutes for the other: a
header shifts the first record off line one, a record may span lines, `Trivia` swallows
blank ones, and a recovery skips an unknown number of them. The first is the key a
downstream system joins on, the second is what a person opens the file at.

The same names may be captured on a successful element, which is what lets a record
carry its own position without the grammar saying anything else:

```csharp
public sealed record Row(string Symbol, int Qty, long Ordinal, int Line);
```

Counting lines costs a scan of the text an element consumed, and is done only when a
name that needs it was asked for.

### 8.3 What a parse hands back

One driver, four surfaces over it. None of them requires the author to declare
anything; declaring is how control is taken, never how it is obtained.

| Wanted | Shape | Declared |
| --- | --- | --- |
| most control, no allocation per record | `Read()` and properties | nothing |
| failures in the stream, LINQ over it | `IEnumerable<RowOutcome>` | nothing |
| failures in the stream as the author's own types | `IEnumerable<FeedItem>` with a factory | `: T[]` |
| successful records only, failures to a log | `IEnumerable<Row>` and a sink | nothing |

The first is the primitive and the other three are built from it:

```csharp
var feed = FeedGrammar.ReadFeed(input);

while (feed.Read())
{
    if (feed.HasError) reject.WriteLine($"{feed.Line}\t{feed.Error}\t{feed.Text}");
    else               Handle(feed.Current);
}
```

Success and failure are the same iteration here, so nothing has to be matched up
afterwards, and there is no element to allocate. It is also the shape a reused record
needs — `Current` valid until the next `Read` — which no compiler-generated iterator
can offer.

`RowOutcome` is generated per grammar, `public`, with members drawn from the BCL and
from the grammar's own types. It follows the rule of §7.3 rather than being an
exception to it: no C# type declared, so one is generated. Nothing is shared between
assemblies, so §6.2 does not apply and `[assembly: GramRuntime]` is not needed; under
it, the outcome gains a typed `Diagnostic`, additively.

**A rejection carries the text it was rejected from.** A position alone is useless in a
streamed parse — a `TextReader` cannot be wound back and the buffer has been reused —
so the text of a failed element is materialized. Only of a failed one: the premise is
that failures are rare, and a grammar that fails on every record has a different
problem.

Exceptions appear only where §7.5 puts them: in the published methods without a `Try`
prefix. `ParseFeed(string)` throws on the first failure of any kind; everything else
answers.

---

## 9. A complete example

```dotgram
@using System;

parse Feed

Feed : FeedItem[] = Header & Row* & Trailer & eof

Header  = "H" & '|' & date: Date & '|' & source: Text & eol
Row     = "D" & '|' & symbol: Text & where @IsSupportedSymbol(symbol)
        & '|' & qty: Number & '|' & date: Date & eol
Trailer = "T" & '|' & count: Number & eol

Date : @DateOnly =
    y: Digits(4) & '-' & m: Digits(2) & '-' & d: Digits(2)
    => @DateOnly(y, m, d)

Digits(n: int) : int = ['0'..'9']{n} => @int.Parse(text)
Number         : int = ['0'..'9']+   => @int.Parse(text)
Text        : string = [^ '|' | '\r' | '\n']+
```

```csharp
[Gram]
public partial class FeedGrammar
{
    private static partial bool IsSupportedSymbol(string symbol)
        => Symbols.Contains(symbol);
}

public abstract record FeedItem;
public sealed record Header(DateOnly Date, string Source)          : FeedItem;
public sealed record Row(string Symbol, int Qty, DateOnly Date)    : FeedItem;
public sealed record Trailer(int Count)                            : FeedItem;
```

```csharp
var feed = FeedGrammar.ParseFeed(text);

if (!FeedGrammar.TryParseFeed(text, out var value, out var error, out var pos))
    Console.WriteLine($"{error} at {pos}");
```

---

## 10. The grammar of `.gram` itself

A consistency check: all the notation above is parsed by this grammar with no more
than two tokens of lookahead.

```dotgram
File        = Using* & Declaration*
Using       = ("@using" | "using") & QualifiedName & ';'

Declaration = Scope | Publication | Rule
Scope       = "scope" & Identifier & '{' & Using* & Declaration* & '}'
Publication = ("parse" | "find") & QualifiedName & ("as" & Identifier)?

Rule        = Identifier & Parameters? & (':' & Type)? & '=' & Body
Parameters  = '(' & (Parameter & (',' & Parameter)*)? & ')'
Parameter   = Identifier & (':' & Type)?
Type        = Reference & "[]"?

Body        = Alternative & ('|' & Alternative)*
Alternative = Sequence & ("=>" & Value)?
Sequence    = Operand & ('&' & Operand)*
Operand     = Guard | Quantified
Guard       = "where" & Value

Quantified  = Prefixed & Quantifier? & Recovery?
Quantifier  = '?' | '*' | '+' | '{' & Count & (',' & Count?)? & '}'
Recovery    = "recover" & Prefixed & ("=>" & Value)?
Count       = Int | Identifier
Prefixed    = ("?=" | "?!")? & Captured
Captured    = (Identifier & ':')? & Primary
Primary     = Char | String | ElementSet | Call | Reference | '(' & Body & ')'

Value       = CsExpr | Call | Reference
CsExpr      = "@(" & Balanced & ')'
Call        = Reference & '(' & (Argument & (',' & Argument)*)? & ')'
Argument    = Value | Char | String | ElementSet
Reference   = '@'? & QualifiedName & TypeArgs?
TypeArgs    = '<' & Type & (',' & Type)* & '>'
ElementSet  = '[' & '^'? & ElemAlt & ('|' & ElemAlt)* & ']'
ElemAlt     = Char & (".." & Char)? | UnicodeCategory | Reference
UnicodeCategory = "\p{" & Identifier & '}'
```

`@(` is the only place in the whole language holding raw C# text, and the only one
needing a foreign lexer. Everything else with `@` (`@Name`, `@Name.Name`, `@Name<T>`,
`@Name(args)`) is parsed by `.gram`'s own parser through the `Reference` and `Call`
productions; the arguments there are `.gram` values rather than C# text, and the names
go to Roslyn later, during symbol resolution.

`Balanced` is the text up to the matching close parenthesis. It is found with C#'s
**tokenizer** (`SyntaxFactory.ParseTokens`) and a depth counter: string, verbatim,
interpolated and raw literals, and comments too, arrive as whole tokens, so a `)`
inside one cannot change the depth.

C#'s parser will not do, tempting though it looks: `SyntaxFactory.ParseExpression` is
greedy and knows nothing of `.gram`'s terminators, while `&`, `|`, `*`, `+`, `?`, `[`
and `..` are all valid C# operators. On `where @(qty > 0) & b: Y` it would consume
`(qty > 0) & b` and stop only at the colon.

Consequences. `.gram`'s lexer is not single-mode: after `@(` it switches to C#
tokenization and returns at the matching `)`, as Razor does. This does not affect the
two-token bound above. And Razor's limits on implicit expressions — no spaces, no
generics — do not carry over: there they exist because the end has to be guessed
inside a stream of markup, whereas here a grammar token marks the end. So
`@ int . Parse ( text )` and `@List<int>` are both fine — `<` and `>` are free, since
the grammar has no comparison operators.

---

## 11. Deliberately out of scope

Below is what needs a working prototype rather than another round of argument on
paper. None of it requires changing the notation above.

**Decided: there will be no notation for it.**

- **Repairing a document.** When a whole input is one construct — a source file in an
  editor — recovery needs no notation: the engine runs a pass of its own, only after
  ordinary parsing failed, and looks for the cheapest edit that makes the input parse.
  The author writes nothing. Details in `implementation.md` §1 and §6.

  This was once written here as covering error recovery entirely, and it does not.
  Cheapest-edit repair answers "what did the author most likely mean", which is the
  right question for one document and the wrong one for a feed of a hundred million
  records: there the answer wanted is "this record is bad, say why and go on", which is
  a policy rather than a repair, and it must hold nothing but the current record. That
  case has notation, and it is §8.2. The two do not overlap — one runs after a failed
  parse over the whole input, the other during a successful one, per element.

- **Alternatives are never reordered.** `|` is ordered choice and stays so, including
  where one literal alternative is a prefix of another. `"http" | "https"` matches
  `https` perfectly well: `"http"` is tried, whatever follows the choice fails, and the
  match returns and tries `"https"`. A prefix does not shadow anything, because
  backtracking is what ordered choice means here.

  Reordering by length looks like the obvious fix to a problem that is not there, and
  would not be one anyway: it produces a different grammar rather than a corrected one.
  In

  ```dotgram
  Rule = part: ("x" | "xy") & 'y'?
  ```

  input `xy` gives `part = "x"` as written and `part = "xy"` reordered — both parses
  succeed, and even full backtracking would not make them agree. Order stops being
  incidental the moment two alternatives can match different lengths, and which of
  "first that matches" and "longest that matches" is wanted is the author's call —
  .NET regular expressions take the first, POSIX the longest.

  Normalization does still merge alternatives automatically where order provably
  cannot matter: single-element sets, where the match is always exactly one item, so
  `'a' | 'b'` becomes `['a'..'b']`.

**Decided in substance, awaiting a prototype.**

- **Trivia** — the mechanism is in §4.5. It needed no notation at all: an ordinary
  rule and ordinary shadowing.

- **There is no commit point in an expression.** Ordered choice backtracks fully, so
  `Call | Index` sharing a leading `Identifier` simply works, and a rule means one thing
  everywhere it is called. There is no cut operator, and an alternative cannot be
  written so as to mean something different from where it sits.

  What was rejected here is a commit point an author scatters through expressions,
  whose cost is exactly that a rule stops meaning one thing. `recover` (§8.2) commits
  too, and pays none of it: it is written on one repetition, it says that *that*
  repetition's elements are records rather than alternatives to try, and the rules it
  calls are unchanged everywhere including inside it. The reason it exists at all is
  the one early commitment was originally wanted for and the recovery engine cannot
  serve — a semantic guard failing on record 12 of a hundred million, where cheapest-edit
  repair is neither affordable nor an answer.

- **Keyword boundaries** — §4.6, the same mechanism again.

**Deferred, with the reason.**

- **`Incomplete`** does not exist. An outcome is `Success`, `NoMatch` or `Error`.
  A source that cannot block — an async socket, where control has to go back to the
  caller mid-parse — is what would need it; a file, however large, is read by a reader
  that simply fetches the next chunk. Adding it means a rule for every construct
  (repetition, both lookaheads, recovery, `find`) plus a resumption model,
  and that is a lot to carry before anything asks for it.
- **A sliding window** over input that is neither memory-sized nor line-oriented.
  Source files fit in memory; feeds are line-oriented; what is left is huge binary
  input, which is out of scope. If it ever arrives, it slots in beside the two modes
  in §6.3 without disturbing them.
- **Operator precedence** as a construct. Levels written as rules (§4.3) work and cost
  nothing, and no grammar has yet made that a burden. Introducing one would also mean
  answering whether it is sugar or a privileged lowering, which is worth doing only
  when something needs it. `implementation.md` §9 records what to lower it into.

**Answered, and where.**

- **How an author says "this is an error, not a mismatch"** was open here, on the
  observation that a failing `where @IsSupportedSymbol(symbol)` is merely a non-match
  and "unsupported symbol XYZ" is therefore never said. It was answered by splitting
  the question in two (§8.1). A guard is recognition and stays a non-match — that was
  never the part that needed changing. What was missing is that most of the checks
  people write are not recognition at all: they run on values, after the match, and
  belong in a transformation that may fail. §7.1 gives that a signature, `recover`
  (§8.2) gives the failure somewhere to go, and neither touches what a guard means.
