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

`@using X.Y;` imports a C# namespace, `using X.Y;` a grammar context (§2). The syntax
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
using Lexical.Numbers;   // import a grammar context

@Encoding.UTF8           // a static C# property
Lexical.Token            // a rule from a grammar context
```

Otherwise `using System;` and `using Lexical;` would look identical while resolving
in different namespaces — exactly the implicit context dependency `@` exists to
remove.

Of all keywords `@` applies only to `using`, and that is not an exception: `context`,
`parse`, `when` and the rest are constructs of `.Gram` with no C# counterpart, so
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
belongs in the partial class next door, where there is a debugger and refactoring.
The reverse transition (`@:`, `<text>`) and the `@@` escape are unnecessary:
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

`any`, `none`, `eol`, `eof` and `trivia` are ordinary standard-library rules rather
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
{ a & b }               atomic group (commit when the group succeeds)
```

A connector between operands is always required. `.Gram` has no juxtaposition: two
expressions cannot sit side by side without `&` or `|`. That is precisely why rules
need no separator (§4.4).

`trivia` is inserted between the operands of a sequence — an empty rule by default,
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

Every quantifier is postfix, and together they form one consistent family. Braces
immediately following an operand with a numeric or parameterized body are a count;
braces in primary position are an atomic group.

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
SmallNumber = n: ?=Number & when @IsSmall(n) & value: Number => value
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
when @Predicate(args)
```

An operand that consumes no input and produces no value: if the predicate returns
`false`, the whole sequence did not match.

The keyword is not ceremony. Syntactic position fixes every C# contract: `@ParseGuid`
as an operand is an external recognizer, `[@IsLetter]` is an element predicate, and
`when @IsSupported(symbol)` is a test on captured values. No C# signature is inspected
to decide which one the author meant.

The word follows C#'s own guards: `case P when condition` and
`catch (Exception e) when condition` make a branch applicable only under an additional
condition. Here it says the same thing about the current recognition path. The guard is
evaluated while that path is being tried, so it may run more than once and on a path
later abandoned by backtracking.

If the guard names a capture whose value is built by `=>`, that value has to exist before
the guard can be called. The parser builds it at that point and caches it on the current
derivation. Acceptance reuses exactly the same value; the construction is not called a
second time. If backtracking abandons the derivation, its cached value is abandoned with
it. Thus choosing to inspect a computed value also chooses to run that computation during
recognition, with the same speculative-effect requirements as the guard itself. A captured
sequence is handled as one array in grammar order, including values supplied by `recover`.

**The guard's position is the author's choice and carries meaning.** A guard has no
outcome of its own — it is an ordinary operand, and its failure means whatever the
failure of any operand in that place means:

```dotgram
SmallNumber = n: ?=Number & when @IsSmall(n) & value: Number => value
```

nothing has been consumed before the guard (lookahead does not move the input), so
`SmallNumber` simply does not begin here and a sibling alternative may be tried;

```dotgram
Row = "D" & '|' & symbol: Text & when @IsSupportedSymbol(symbol) & ...
```

`"D"` has been consumed — the line is plainly a data row, and failure should produce
a diagnostic about the unsupported symbol rather than a silent fall-through to
another alternative.

The first reading is what the language does: ordered choice backtracks fully, and the
only thing that commits is an atomic group (§3.2), which this guard is not inside — so a
failing guard is a non-match and a sibling is tried.

The second is what one would want in the `Row` case, and saying so is the one thing
still missing — see §11. Note that it is a question about diagnostics, not about
parsing: whichever way it is answered, the guard's position stays the author's choice
and still decides how much work is thrown away and where the message points.

### 3.7 Construction

```dotgram
=> value
```

Construction gives the alternative its value. It never touches the input and is deferred
until recognition has selected the accepted derivation, unless a later `when` on that
same path explicitly asks for the computed value. In that case the construction runs for
the guard and its result is cached. An alternative later abandoned by backtracking does
not invoke an unrequested construction.

**It binds to one alternative, not to the rule body** — by §3.8 it sits below `&` and
above `|`. So every branch of a `|` builds its own result, and no parentheses are
needed for that:

```dotgram
Expr = value: Number           => value
     | '(' & inner: Expr & ')' => inner
```

A name, a call or a parenthesized C# expression may stand on the right (§2). Captures
are visible there as ordinary local variables, along with the names the parser supplies
itself — `parserText`, `parserSpan` and the rest of §8.2's table:

```dotgram
Number : int    = ['0'..'9']+ => @int.Parse(parserText)
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

Construction is therefore not speculative like `when`: its value does not need rollback.
A factory runs once for each accepted result it builds; a repeated rule may of course
produce more than one such result.

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

**A rule call is transparent to backtracking.** If a later expression fails, the parser
may resume a choice or repetition inside a called rule just as it may resume one written
inline.

```dotgram
Start = Name & 'y'
Name  = "xy" | "x"
```

matches `xy`: after `Name` first answers `"xy"` and `'y'` finds nothing left, the parser
retries `Name` as `"x"`. The same expressions written inline have the same meaning:

```dotgram
Start = ("xy" | "x") & 'y'
```

Use `{ ... }` where success must commit. For example, `{ "xy" | "x" } & 'y'` fails on
`xy`: once the atomic group succeeds as `"xy"`, later failure cannot reopen choices made
inside that group. Rule extraction by itself never introduces such a commit.

**This is the language, not the engine.** A rule is a function from a position to a
match, and the rest of the language rests on that. What a match records is which
alternative it came through — one number per call — and that is what lets construction
happen after recognition rather than during it (§7.3), what lets a fold collect its steps
(§4.3), and what lets a repetition be committed by `recover` (§8.2). A rule that answered
more than once would be a function from a position to a *sequence* of matches, and every
one of those would have to be built again around it.

None of which costs the freedom to refactor. Lifting part of an expression into a rule
of its own does not change what it matches, and inlining a rule back does not either:
that is what a transparent call means, and it is the property the whole of this section
is about. A rule earns its own commit only by being written inside `{ }`.

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

A name, a call and a parenthesized C# expression are allowed in `=>` and `when`
(§2), with captures visible as ordinary local variables:

```dotgram
Number : int    = ['0'..'9']+ => @int.Parse(parserText)
Point  : @Point = '(' & x: Number & ',' & y: Number & ')' => @(new Point(x, y))
Row             = ... & when @(qty > 0 && symbol.Length == 4)
```

Besides captures, the names the parser supplies are always in scope inside `=>` and
`when`: `parserText` (the matched text, when `TIn` is a character), `parserSpan` (the
current rule's `SourceSpan`), and the rest of §8.2's table. They all begin with
`parser`, which is what that prefix is for — a capture may not take one of those names
(GRAM4012), and every other name in the grammar is the author's to choose.

There is no limit on the size of an expression, but there is a recommendation: once
it stops reading at a glance it is better off as a named method in the partial class
next door, where it has a debugger and refactoring. The generator neither declares nor
resolves that method (§7.4).

### 4.2 Parameters

A parameter is written like a capture — `name` or `name: Type` — because it does the
same thing: binds a name to something that came from outside.

```dotgram
Lex(item)               : item   = trivia & item
List(item, sep)         : item[] = item & (sep & item)*
Padded(item, pad: char) : item   = pad* & value: item & pad* => value
Digits(n: int)          : int    = ['0'..'9']{n} => @int.Parse(parserText)
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

### 4.3 Precedence and associativity

There are two ways, and which to use is a real choice rather than a fallback.

**Levels as rules** is the default. It needs no engine, no notation and no annotation —
a grammar written this way compiles to the same recursive descent as everything else,
and for the handful of levels most notations have it is the faster of the two as well:
an operator costs one iteration of an ordinary repetition. Use it unless something
below says you cannot.

**Binding powers** (§4.3.1) buy the two things levels cannot: an expression language
with many levels written as one rule, and the shapes ordered choice cannot settle. They
cost a precedence-climbing engine at run time, which is why they are not the default.

#### Levels as rules

One rule per level, each calling the next:

```dotgram
Expr   = Term & (['+' | '-'] & Term)*
Term   = Factor & (['*' | '/'] & Factor)*
Factor = Number | '(' & Expr & ')'
```

**Associativity is which side the recursion is on**, and there is no notation for it
because there does not need to be: a grammar that recurses on the left is
left-associative and one that recurses on the right is right-associative, which is
what those shapes have meant since BNF.

```dotgram
Sum   = left: Sum   & op: ['+' | '-'] & right: Product  => @Apply(left, op, right)
      | value: Product                                  => value

Power = left: Unary & '^' & right: Power                => @Raise(left, right)
      | value: Unary                                    => value
```

`1-2-3` groups as `(1-2)-3` and `2^3^2` as `2^(3^2)`, because that is how they are
written.

Associativity therefore belongs to an **alternative**, not to a rule — that is where
the recursion is. A rule ends up with one because a level of precedence is a rule, and
mixing two in one rule is legal and a bad idea, the same way it is in any grammar.

**In a left-recursive alternative, the leading capture is the accumulator.** `left:
Sum` is not a fresh parse of `Sum`; it is the value built so far — the first time from
the alternatives that are not left-recursive, and afterwards from the previous
application of this one. Which is to say `=>` is a fold, and both `=>` in the rule
above are used: the base builds the first value, the recursive alternative applies once
per operator.

A fold is no exception to §7.2: nothing is built while matching. What the match records
is which alternative it came through and, for a chain, which step followed which; the
`=>` of each is applied once the whole match has succeeded, in that order. So a step
tried and given back never ran at all.

Three things are rejected when the grammar is built:

- **indirect left recursion** — `A` reaching itself through `B` without consuming.
  Direct recursion has one shape to rewrite and indirect has arbitrarily many, so it
  is a diagnostic rather than a half-working transform.
- **a rule whose every alternative is left-recursive.** There is nothing to start from.
- **an alternative recursive on both sides**, `E = E & '+' & E`. Ordered choice
  cannot settle it: the leading `E` would be the accumulator and the trailing one
  would take everything to the right, so what is written left-associative would parse
  right-associative. Write the operands at the next level down, or say what you mean
  with §4.3.1.

#### 4.3.1 Binding powers, when levels are not enough

An alternative may state its own strength instead:

```dotgram
Expr : @int = left: Expr & '+' & right: Expr   << 1  => @(left + right)
            | left: Expr & '-' & right: Expr   << 1  => @(left - right)
            | left: Expr & '*' & right: Expr   << 2  => @(left * right)
            | left: Expr & '^' & right: Expr   >> 3  => @Pow(left, right)
            | '-' & operand: Expr              >> 4  => @(-operand)
            | '(' & inner: Expr & ')'                => @(inner)
            | digits: ['0'..'9']+                    => @int.Parse(digits)
```

One expression language, one rule, and `-1-2` is `-3` because unary minus is stronger
than binary minus — which is said here rather than arranged by how the rules are
stacked.

**`<<` and `>>` mean one thing: at what strength the operand to the right is parsed.**
`<< n` parses it one level tighter, which makes the operator left-associative; `>> n`
parses it at `n`, which makes it right-associative. A prefix operator is the same
statement with no left operand, which is why it needs no third marker.

| Written | Is |
| --- | --- |
| `left: E & op & right: E << n` | infix, left-associative, level `n` |
| `left: E & op & right: E >> n` | infix, right-associative, level `n` |
| `op & operand: E >> n` | prefix, level `n` |
| `left: E & op << n` | postfix, level `n` |
| no marker | an atom — a literal, a group, a call |

Higher binds tighter. The numbers are the author's and need not be contiguous: gaps are
where a level is inserted later without renumbering the rest.

A rule uses one convention or the other. Levels as rules and binding powers in one rule
would be two answers to the same question, and the compiler refuses it rather than
choosing.

### 4.4 Rule separator

There is none and none is needed: a connector between operands is mandatory, so an
expression can never be "continued" by the next rule. An identifier followed by `=`,
`:` or `(`, in a position where the current expression is already complete, is always
a new rule. It cannot be a capture: a capture must follow `&` or `|`, and there the
expression does not count as complete.

### 4.5 Trivia — insignificant whitespace and comments

The rule `trivia` is always inserted between the operands of a sequence. It is empty
by default, so by default nothing is inserted:

```dotgram
// standard library
none                  = any{0}                 // zero repetitions: succeeds, consumes nothing
trivia                = none
Whitespace            = ([' ' | '\t'] | eol)*
WhitespaceAndComments = (Whitespace | LineComment | BlockComment)*
```

A grammar to which whitespace is insignificant redefines one rule:

```dotgram
trivia = WhitespaceAndComments
```

No directive, no mode: it is an ordinary rule, and `none` is expressed in the language
itself as `any{0}` rather than by a new primitive.

**Between the operands of a sequence, and nowhere else.** The iterations of a repetition
are not operands of a sequence, so nothing is inserted between them:

```dotgram
trivia = Whitespace

Pair    = Word & Word            // matches "ab cd"
Several = Word*                  // matches "abcd", and stops at the space in "ab cd"
```

That is not an oversight to be worked around but the thing that makes the notation
usable at all. A repetition is how a lexeme is written — `Digits = ['0'..'9']+`,
`Name = Letter+` — and inserting trivia between those iterations would make `1 2` one
number and `a b` one name in every grammar that ignores whitespace. Nothing can tell the
two apart automatically: `Word*` and `Digit*` have the same shape, and only the author
knows which is a list and which is a lexeme.

So the author says which. `trivia` is an ordinary rule and may be named:

```dotgram
Attributes = Attribute & (trivia & Attribute)*     // a list, spaced
Digits     = ['0'..'9']+                           // a lexeme, not
```

The same is true of a run with a separator, where the separator is an operand and the
spacing around it comes for free:

```dotgram
List(item, sep) : item[] = item & (sep & item)*    // "1, 2 , 3" — trivia is inserted
                                                   // either side of `sep`
```

**Switching per block is the shadowing from §5**, not a separate mechanism:

```dotgram
trivia = WhitespaceAndComments

context Lexical
{
    trivia = none                              // whitespace is significant here

    Identifier = ['a'..'z' | '_'] & ['a'..'z' | '0'..'9' | '_']*
    Number     = ['0'..'9']+
}

context Syntax
{
    using Lexical;

    If = "if" & '(' & cond: Expr & ')' & then: Statement
}
```

**Scoping is lexical:** a rule uses the `trivia` visible where it is **declared**, not
where it is called. A rule means the same thing wherever it is used.

**`trivia` must be nullable** — it has to accept empty input, or the build fails. That
condition is what makes unconditional insertion safe: a second application consumes
nothing, so nothing is ever doubled and no rule of the form "insert after a literal
but not after a structural call" is needed. The single exception is one insertion at
the start of a published rule, for leading whitespace.

When `trivia` is empty the insertions are dropped entirely during normalization:
nothing of them survives to run time.

A silent failure is possible here — a lexical rule that ended up by oversight in a
context with non-empty `trivia` will quietly accept `i f` as `if`. No mechanism catches
that, but a warning does: a rule whose operands all test a single input item is
almost certainly a mistake in such a context.

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

Same shape as `trivia` (§4.5), and for the same reason: a rule, ordinary shadowing,
and the insertion dropped entirely while the rule is empty. A regex or a feed grammar
pays nothing; a language grammar pays one line.

The boundary check goes **before** the trivia insertion. The other order would ask
whether a letter follows the whitespace rather than whether it follows the keyword.

---

## 5. Contexts

```dotgram
@using System;

Common = ...

context Lexical
{
    Token = ...
}

context Syntax
{
    using Lexical;              // import a grammar context

    Unit = Token*               // instead of Lexical.Token
}
```

The top of a file is an implicit global context. The `{ }` after `context Name` is a
block of declarations, not an expression; in expression position braces mean a
repetition count and nothing else (§3.3). An inner context sees the outer one; a rule
of the same name shadows the outer one; the qualified name `Context.Rule` is available
from outside.

`using X;` without `@` brings the names of context `X` into the current context
unqualified. Import directives stand at the top of the file or at the top of a
`context` block — where C# expects them. If two imports supply the same name, the
error is raised at the use site rather than at the import, and is settled by
qualification, as in C#.

Every context becomes a nested static class in the generated code.

### 5.1 Contextual rebinding

A `context` may carry a header, `context Name (A = B, ...) { ... }`. This does
something different from an ordinary declaration in the block: it rebinds `A` to `B`
for the *whole call graph reached from what the context declares* — not only for
calls written lexically inside it.

```dotgram
B = 'c'
A = B
F = A

context Ctx (B = D)
{
    E = A
}

D = 'd'
```

`F`, outside the context, still resolves `A` to the ordinary `B`. `E`, inside,
resolves the same `A` through `D` — even though `A` is declared outside the context
and never mentions `D`. `A` itself is untouched: nothing about `F`'s behavior depends
on the context existing.

The contrast with an ordinary declaration of the same name is the whole of what this
section adds:

```dotgram
B = 'c'
A = B

context Ctx
{
    B = 'd'                 // shadowing: a new, unrelated rule named B
    E = A                   // E -> outer A -> outer B -> 'c'
}

context Ctx2 (B = D)
{
    E = A                   // E -> A, with B substituted -> D
}

D = 'd'
```

A binding is not a declaration — it does not introduce a rule named `B` — so it does
not shadow anything and nothing inside the same context, at any nesting depth, may
also *declare* a rule under a name that is actively bound; write a nested
`context (B = ...)` instead of redeclaring `B`. Both sides must already resolve to a
visible, parameterless rule.

Bindings in one header resolve simultaneously, against the context the header itself
is written in: `context (A = B, B = C)` sends a call to `A` all the way to `C`
regardless of which entry is written first. A nested context inherits its enclosing
one's bindings and may replace any of them with its own.

`trivia` is an ordinary rule, so it is an ordinary rebinding target:
`context (trivia = none)` reuses an already-written rule under different whitespace
handling — the same substitution as any other binding, and a different mechanism from
shadowing `trivia` locally (§4.5), which affects only what the block itself declares.

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
skew, and nothing crosses an assembly boundary.

**Nothing is shared, and that is the design rather than a stage of it.** A library
exposing a parser in its public API exposes its own generated types, and a consumer uses
those — exactly as it would use any other type the library declares. There is no mode in
which two assemblies bind to one copy of anything.

There was one, briefly: an assembly could declare `[assembly: GramRuntime]` and publish
four support types as `public` for others to bind to. It bought nothing — three of the
four were used by nothing at all — and it cost the property this section is about, because
an assembly compiled by one version of the generator was then binding to types emitted by
another, with no package or version to say so. Emitting everything `internal` makes the
question unaskable: an internal type cannot be seen across a boundary, so two of them
cannot disagree. When a type genuinely has to be shared, it comes back with a contract to
version and a reason to exist.

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

**What "how far back" means is fixed by §4**, and this is the whole of the retention
rule:

- a call may resume choices in its callee, so rule extraction does not reduce retention;
- an explicit atomic group discards alternatives created inside it when it succeeds;
- recovery and streaming continuation boundaries provide the other points after which
  earlier input can no longer be revisited.

For `parse` that is the whole input, and a grammar of one rule over a whole file streams
no better than reading it. What bounds it is a construct that says the parse will not go
back past a point, and `recover` is one by construction: its synchronization expression
is exactly such a point, so a marked repetition retains one element and not the file.

**A capture is materialized when the rule that owns it accepts** — not when the parse
ends, and not lazily afterwards. Since a rule's extent is also its retention, a captured
string is made while the input it names is still there, and a buffer may be reused the
moment the rule that read it has answered. That is what makes a capture and a reused
buffer compatible at all, and it is why construction runs at the accepting state (§7.3)
rather than at the end of the parse.

**Absolute offsets are `long`; extents are `int`.** A position is into the input, and an
input may be a file larger than an `int` can index. An extent — a span, a length, a
capture — is into a buffer, and a buffer never is. Counts of things, like a line number
or an ordinal, are `int`: nothing has two billion lines.

Positions inside a line are ordinary `int`. What crosses the publication boundary for
a streamed parse is a `long`, so an error at offset 8,432,109,553 can be reported as
such.

---

## 7. The bond with C#

This is the language's other half, not an appendix to it: the grammar describes
structure, C# describes meaning, and the seam between them has to be mechanical.

### 7.1 Recognizer signatures and C# values

Only a method used as a recognizer needs a shape the generator understands, because it
participates in moving through the input. A method or expression used by `when` or `=>`
is emitted as C# and belongs entirely to the consumer's compiler:

| C# signature | Role | Called from the grammar as |
| --- | --- | --- |
| `bool M(char c)` | element predicate | `[@M]` inside an element set |
| `bool M(ReadOnlySpan<char> input, ref int pos)` | external recognizer | bare `@M` as a grammar operand |
| any C# value | construction | `=> @M(a, b)`, `=> @(expr)` |
| any C# `bool` value | guard | `when @M(a)`, `when @(expr)` |

**The arguments are read by §2, with no exception made for being in an argument list.**
A bare name is looked up among the grammar's own — a capture, a rule, a parameter — and
anything of C#'s is reached with `@`:

```dotgram
=> @int.Parse(digits, @CultureInfo.InvariantCulture)     // a capture, then a C# name
=> @(int.Parse(digits, CultureInfo.InvariantCulture))    // or all of it as one expression
```

Both are written the same way in the generated file; which to use is a matter of how
much of the line is C#. A dotted name written without the `@` is the ordinary mistake
here, and the compiler says so by name.

There is one rule to read this by: **syntactic position determines the call shape.**
`[@M]` emits `M(c)`, bare `@M` emits `M(text, ref p)`, and `when` and `=>` emit their C#
values. The generator never inspects a method signature to choose among those roles;
overloads, accessibility, parameter types and result types are C#'s responsibility.

The same C# name may therefore implement both contracts without ambiguity:

```dotgram
One  = [@Foo]
Many = @Foo
```

The first call selects `bool Foo(char)`, the second
`bool Foo(ReadOnlySpan<char>, ref int)` by ordinary C# overload resolution.

The external recognizer's signature is deliberately built from BCL types only: it is
the same whether or not shared mode is on (§6.2), and it needs no interface dispatch.
Its value is the text it covered — the same as any rule that captures nothing.

**A recognizer is trusted absolutely, and that is the bargain.** The `ref` is the method
saying that it moves the position; it is handed the parser's own, and nothing copies it
away, bounds-checks it afterwards, or reasons about what came back. Move it backwards,
move it past the end, leave it somewhere that makes the rest of the grammar nonsense —
all of that is allowed and none of it is diagnosed.

This is deliberate. A seam that second-guessed the code on the other side of it would
still not make a wrong recognizer right. Reaching into the parse means taking the
parse's invariants on with it.

One thing follows from it, and it is arithmetic rather than punishment: a grammar
containing an external recognizer gets no streaming overloads (§6.3). The method is
handed a span and told nothing about where it came from, so it cannot tell the end of a
window from the end of the input, and nothing in its signature lets it say which it hit.
It would read a record cut in half as a record that ended.

An inline `@(...)` expression plays the same role as a value transformation, only
without a name: it receives no input, sees captures as local variables, and is checked
by C#'s type system exactly where the generator placed it.

Overloads, generic methods, extension methods and nullable annotations are resolved by
ordinary C# rules.

### 7.2 What the C# side must guarantee

- A value transformation has no access to the input — it physically never receives it.
- An external recognizer must restore `pos` to its entry value on any outcome other
  than success.
- An external recognizer and a `when` guard execute during recognition. Ordered choice
  and lookahead may invoke them repeatedly or abandon the path on which they ran. Their
  code must therefore be safe for speculative invocation and must not perform effects
  that require rollback.
- A `=>` construction is deferred until the accepted derivation is known unless a `when`
  guard explicitly inspects its value. Captures normally record what matched and the
  chosen factory builds only the accepted path. A value requested by a guard is built
  during recognition, cached, and reused after acceptance; it may therefore have been
  built on a path the guard or later input abandons.

### 7.3 Captures and building the result

Captures are matched to the result type by name, in a fixed order:

1. a constructor whose every parameter is covered by captures;
2. `init`/`required` properties;
3. an explicit `=> @Factory(...)` when neither fits.

Names are matched by one mechanical casing transform: the capture `symbol` fits the
parameter `symbol` and the property `Symbol`.

A handful of names are **supplied rather than captured**: declare a parameter with one
of them and the generator fills it in. They are listed in §8.2, where the same names
serve a rejected element — `parserSpan` and `parserText` for the extent and the input
it covers, `parserOrdinal`, `parserLine`, `parserColumn` and `parserPosition` for where
it was.

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
// generated when no C# type exists
public sealed class Url
{
    public Url(string scheme, Host host) { Scheme = scheme; Host = host; }

    public string Scheme { get; }
    public Host   Host   { get; }
}
```

This is what a regex's named group becomes: a member of a known type, checked at
compile time, rather than `Match.Groups["scheme"].Value` looked up by string at run
time. And it can be typed all the way — `scheme: Scheme` with `Scheme : @UriScheme`
hands back the enum instead of the text.

When no accessible C# type exists for a rule, an ordinary class with the same members
is generated — a constructor and a get-only property per capture, and nothing else. Not
a bespoke node framework, and not a `record`: a positional record needs `IsExternalInit`,
which lives in `System.Runtime.CompilerServices`, and §6.1 is why nothing is ever emitted
into a namespace that is not ours. A consumer targeting an older framework has their own
copy of that type from a polyfill package, and a second one is a compile error in their
build rather than ours.

### 7.4 C# stays C#

The generator does not resolve or declare methods named by `when` and `=>`. It emits
the call exactly as written, under the `#line` mapping of §7.6. A missing name, a wrong
overload, an inaccessible member or an incompatible result is therefore an ordinary C#
diagnostic at the corresponding place in the grammar.

```dotgram
Number : @int = digits: ['0'..'9']+ => @Tini(digits)
```

If the host contains `Tiny` rather than `Tini`, the compiler reports the misspelling.
The generator does not turn an unknown name into a partial-method contract.

### 7.5 Recognition outcomes

Inside the language an outcome is an ordinary value, never an exception. The type is
emitted by the generator into the assembly itself, `internal` like everything else
(§6.2):

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

### 8.1 Recognition failure and C# exceptions

The seam already exists — §7.2: `when` runs **during** the match, `=>` runs **after**
it, once the match is final and from the alternative that actually matched.

A recognition failure is a shape the grammar does not describe. It happens during the
match, and ordered choice may undo it and try something else. Only past a commit point
(§8.2) does it stop being "try something else" and become an error.

Construction runs after recognition. Its C# must produce the rule's declared value;
compile-time mistakes are C# diagnostics, and an exception thrown while constructing a
value leaves the parse. The generator does not infer another parser outcome from a C#
method's signature.

There is also a failure that belongs to no single record: the **envelope**
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

At a boundary between elements, however, the parser first tries the **complete
continuation after the repetition**. If that continuation succeeds, the repetition is
finished. If it fails anywhere along that path, its work is undone and the parser tries
another element. This is what lets a broad row shape coexist with a specific trailer:

```dotgram
Feed = Header & Row* recover eol & Trailer & eof
```

`Trailer & eof` gets first refusal at every boundary. Usually it fails on its first
character and costs almost nothing; at the actual trailer it wins even if `Row` could
also have consumed that line. This is general sequence/repetition behavior, not a
feed-specific rule.

**Nothing is ever caught.** An exception out of a `=>` leaves the parse, inside a marked
repetition as anywhere else. Catching would mean catching `Exception` — there is no way
to tell "this record's quantity is not a number" from `NullReferenceException` by type —
and a parser that turns a bug in the author's own C# into "row 400 was malformed" is
worse than one that stops.

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

**The synchronization expression is one operand, so a choice needs brackets.**

```dotgram
fields: Field* recover ('|' | eol)      // either separator, whichever comes first
fields: Field* recover  '|' | eol       // (fields: Field* recover '|') | eol
```

`recover` binds tighter than `|`, the same way `&` does (§3.8), so the second line is a
choice between a recovering repetition and `eol` rather than a recovery with two ways to
resume. Both are legitimate things to write, which is why this is precedence rather than
an error.

A choice is worth having. Resuming at the next field separator rather than the next line
keeps the rest of a record instead of throwing it away, which is what a recovery *inside*
a record wants — `eol` in the same choice is then the backstop for a record so broken
that no separator is left in it.

**The synchronization expression is also the retention bound.** An element of a marked
repetition cannot reach back past the previous synchronization point, so a streamed
parse holds one element and not the file. This is what §6.3 promises to prove before
emitting a streaming overload; `recover eol` proves it by construction.

#### The failure factory

With a `=>`, a failed element becomes an element of the sequence instead of vanishing
from it:

```dotgram
Row* recover eol => @BadRow(parserOrdinal, parserLine, parserText, parserMessage)
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
| `parserOrdinal` | which element of the repetition this is, counting rejected ones, from 0 |
| `parserLine`, `parserColumn` | where it starts, for a person, from 1 |
| `parserPosition` | absolute offset, `long` — for a machine |
| `parserSpan`, `parserText` | its extent, and the input it covers |
| `parserMessage` | why it was rejected — only here, never in a capture |

Every one of them begins with `parser`, and that prefix is the whole of the collision
story: the supplied names become parameters of the generated factory for a `=>` or a
`when`, sitting in the same scope as the captures, so a capture called `text`
would take a name already spoken for. With the prefix nothing an author would naturally
write collides, and a capture that takes one of these names anyway is refused by name
rather than by a C# error in a file nobody wrote (GRAM4012).

`parserOrdinal` and `parserLine` are not the same number and neither substitutes for
the other: a header shifts the first record off line one, a record may span lines,
`trivia` swallows blank ones, and a recovery skips an unknown number of them. The first
is the key a downstream system joins on, the second is what a person opens the file at.

The same names may be captured on a successful element, which is what lets a record
carry its own position without the grammar saying anything else:

```csharp
public sealed record Row(string Symbol, int Qty, long Ordinal, int Line);
```

Counting lines costs a scan of the text an element consumed, and is done only when a
name that needs it was asked for.

#### Why separate arguments and not one context object

The obvious alternative is a single `parserContext` carrying all of it, which would
end the collision question outright and leave somewhere to put feedback later. It is
refused on performance, which here outranks the convenience:

- **A parameter that is not asked for costs nothing.** The generator sees which names a
  factory's C# mentions and passes only those. `parserLine` is a scan of everything
  consumed so far; on a million-record feed, computing it for every element whether or
  not anybody wanted it is quadratic. A context object has to be filled before it is
  handed over, so either every field is eagerly computed — the quadratic case — or the
  object computes them lazily.
- **Lazily is what it cannot do.** Computing a line number later means holding the
  input, and the input is a `ReadOnlySpan<char>`. A class cannot hold one, so a lazy
  context would force the whole engine onto `string` or `Memory<char>`, which is the
  cost of the feature paid by every parse that never uses it.
- **A container also allocates**, once per rejected element and once per `=>` that asks
  for anything at all, in a design whose whole shape is that nothing is allocated while
  matching (§4).

Feedback out of the parse already goes through `OnRecovered` (§8.3).

What is left is the name collision, and a prefix costs nothing to solve it.

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

`RowOutcome` is generated per grammar, `public`, and nested in the host class, with
members drawn from the BCL and from the grammar's own types. It follows the rule of §7.3
rather than being an exception to it: no C# type declared, so one is generated. Nothing
about it is shared between assemblies, which is what §6.2 says of everything.

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
Row     = "D" & '|' & symbol: Text & when @IsSupportedSymbol(symbol)
        & '|' & qty: Number & '|' & date: Date & eol
Trailer = "T" & '|' & count: Number & eol

Date : @DateOnly =
    y: Digits(4) & '-' & m: Digits(2) & '-' & d: Digits(2)
    => @DateOnly(y, m, d)

Digits(n: int) : int = ['0'..'9']{n} => @int.Parse(parserText)
Number         : int = ['0'..'9']+   => @int.Parse(parserText)
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

Declaration = Context | Publication | Rule
Context     = "context" & Identifier & Rebindings? & '{' & Using* & Declaration* & '}'
Rebindings  = '(' & (Rebinding & (',' & Rebinding)*)? & ')'
Rebinding   = Identifier & '=' & Identifier
Publication = ("parse" | "find") & QualifiedName & ("as" & Identifier)?

Rule        = Identifier & Parameters? & (':' & Type)? & '=' & Body
Parameters  = '(' & (Parameter & (',' & Parameter)*)? & ')'
Parameter   = Identifier & (':' & Type)?
Type        = Reference & "[]"?

Body        = Alternative & ('|' & Alternative)*
Alternative = Sequence & Binding? & ("=>" & Value)?
Binding     = ("<<" | ">>") & Int
Sequence    = Operand & ('&' & Operand)*
Operand     = Guard | Quantified
Guard       = "when" & Value

Quantified  = Prefixed & Quantifier? & Recovery?
Quantifier  = '?' | '*' | '+' | '{' & Count & (',' & Count?)? & '}'
Recovery    = "recover" & Prefixed & ("=>" & Value)?
Count       = Int | Identifier
Prefixed    = ("?=" | "?!")? & Captured
Captured    = (Identifier & ':')? & Primary
Primary     = Char | String | ElementSet | Call | Reference | '(' & Body & ')' | '{' & Body & '}'

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
and `..` are all valid C# operators. On `when @(qty > 0) & b: Y` it would consume
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

None of what follows changes the notation described above.

- **Repairing a document.** When a whole input is one construct — a source file in an
  editor — recovery needs no notation: the engine runs a pass of its own, only after
  ordinary parsing failed, and looks for the cheapest edit that makes the input parse.
  The author writes nothing. Details in `implementation.md` §1 and §6.

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

- **`trivia`** — the mechanism is in §4.5. It needs no notation of its own: an ordinary
  rule and ordinary shadowing.

- **Atomic groups are explicit.** `{ X }` has the same matches and value as `X`, but when
  it succeeds the parser discards alternatives created while recognizing `X`. Failure
  before the closing brace remains an ordinary failure. Rule boundaries never imply
  this behavior; an author chooses it only where the grammar has a real commit point.

- **Keyword boundaries** — §4.6, the same mechanism again.

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

- **A precedence table** separate from the rules, the way `yacc` declares one. What it
  would buy is in §4.3.1 instead, written on the alternatives themselves — an operator
  and its strength in one place rather than two that must be kept in step.

- **Telling an error from a non-match.** A guard is recognition and stays a non-match:
  a failing `when @IsSupportedSymbol(symbol)` is an ordinary non-match, not an error
  carrying a message like "unsupported symbol XYZ". Most of what an author wants to say
  there is not about recognition at all — it is about a value, after a match, which is
  what a transformation that may fail is for (§7.1), or what a repetition's `recover`
  is for (§8.2). Neither changes what a guard means.
