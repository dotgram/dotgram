---
name: dotgram-expression-language
description: Turn C#-style lambda text into System.Linq.Expressions trees or compiled delegates at run time with DotGram.ExpressionLanguage — ExpressionParser.Compile, Parse and TryParse. Use when a project references the DotGram.ExpressionLanguage package, or when asked to evaluate user-written formulas, rules or small code blocks as .NET delegates or expression trees. Not a C# compiler — one lambda per text, no classes, methods, async or query syntax — and not a sandbox.
---

# DotGram.ExpressionLanguage

Reads one lambda written in C#'s syntax — its operators at C#'s precedence, its literals,
locals, blocks and statements — and builds the `System.Linq.Expressions` tree it means, or
compiles that tree to a delegate. Everything is `ExpressionParser`, in the
`DotGram.ExpressionLanguage` namespace. There is no runtime to deploy and nothing to
configure.

The [README][readme] beside this file is the reference, and the comment at the top of
[`ExpressionParser.cs`][source] lists every place the language is not C#, with the reason
for each. This is the order to decide things in, and the mistakes that are easy to make.

[readme]: https://github.com/dotgram/dotgram/tree/main/src/DotGram.ExpressionLanguage
[source]: https://github.com/dotgram/dotgram/blob/main/src/DotGram.ExpressionLanguage/ExpressionParser.cs

## Choose the entry point

```csharp
using System;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;

// A delegate of your own type: the usual case.
var price = ExpressionParser.Compile<Func<decimal, int, decimal>>(
    "(decimal unit, int count) => unit * count * (count > 10 ? 0.9m : 1m)");

// The tree, to inspect, rewrite or hand to something that takes one.
LambdaExpression tree = ExpressionParser.Parse("(double x) => x / 2.0");

// An answer instead of an exception, for text somebody typed.
var match = ExpressionParser.TryParse("(string s) => s - 1");

if (!match.IsSuccess)
    Console.WriteLine($"{match.Position}: {match.Error}");
```

- **`Compile<TDelegate>`** checks the text's parameters against the delegate's and converts
  what the body is worth to what the delegate returns, as C# converts a lambda's body:
  `(int x) => x` compiles as a `Func<int, long>` too. A mismatch is
  `Expression.Lambda`'s `ArgumentException`.
- **`Parse`** throws `FormatException` for text that is not this language or names what is
  not there, `InvalidOperationException` for text that is and means nothing (an operator its
  operands do not have, no overload that fits), `ArgumentException` where the API calls it an
  argument, and `OverflowException` for an integer no integral type holds.
- **`TryParse`** answers for all of those. `Error` says why and `Position` where; where the
  text was read and the tree could not be built, `Position` is 0.

## Say who is calling

Every entry point has a form that takes an `Assembly`. The one that does not asks the stack
which assembly called it — about 300 ns, often more than a short parse — and that assembly
decides what the text may name: public types, and its own internal types and members.

```csharp
static readonly Assembly Caller = typeof(Rules).Assembly;

var rule = ExpressionParser.Compile<Func<Order, bool>>(
    """
    using Warehouse.Orders;

    (Order o) => o.Quantity > 10
    """,
    Caller);
```

The text names the namespace of anything it uses, because nothing is imported for it
(**Names**, below) — the assembly says what may be named, and the text says what is.

Pass it explicitly whenever texts are parsed in a loop, and always when the call is wrapped
in a helper that lives in another assembly: the helper's assembly is the one the stack finds.

## Names

- **Nothing is imported**, `System` included. A type is written whole
  (`System.Math.Max(x, 1)`) or brought in by a `using` at the top of the text, before the
  lambda. A text's `using`s are its own; the next text starts with none.
- A `using` names a namespace: no alias, no `using static`.
- Only **loaded** assemblies are searched. A type in an assembly the process has not loaded
  yet is not there to be named; touch the assembly (`typeof(SomeType)`) before parsing.
- Keyword types (`int`, `string`, …) name their static members as C# does:
  `int.Parse(s)`, `string.Concat(a, b)`.

```csharp
var total = ExpressionParser.Compile<Func<int[], int>>(
    """
    using System.Linq;

    (int[] a) => a.Where(n => n > 1).Sum()
    """);
```

## What a text may say

- **One lambda.** Parameters say their types: `(int x, string s) => …`, or `() => …`. With
  `Compile<TDelegate>` they may be left out and taken from the delegate, as C# takes them
  from the type a lambda is converted to: `(tag, value) => …`, and `value => …` with no
  brackets. `Parse` is handed no delegate, so there they must be written.
- **The body** is an expression, or a block. A block is worth its last expression, and
  `return` leaves the whole lambda from however deep: `{ int sum = x + y; return sum * sum; }`.
- **Locals** say their type or use `var`: `int n = 0;`, `var half = x / 2;`.
- **Statements:** `if`/`else` (also as a value: `int n = if (c) 1 else 2;`), `while`, `do`,
  `for`, `foreach`, `switch` (a label is `case 1:`, `case 1 or 2:`, or several stacked),
  `try`/`catch`/`finally`, `throw`, `break`, `continue`. Loops are worth nothing.
- **`x switch { 1 or 2 => …, _ => … }`** as a value, where C# puts it in the precedence.
  A pattern is constants joined by `or`, or `_`; no other pattern, no `when`. The arms
  share one type as the branches of `?:` do, and where they share none the switch takes
  the type of the place it stands in — the delegate's return, an argument, a variable of a
  declared type — as C# does. Without `_`, an unmatched value throws.
- **Expressions:** members, calls, indexers, `new` with object, collection and array
  initializers, generic types, casts, `is`, `as`, `?.`, `??`, `?:`, `checked(…)` and
  `unchecked(…)`, `typeof(T)`, `default(T)`, `nameof(…)`.
- **Tuples:** `(a, b)`, of any number of elements and nested, which is a `ValueTuple` as it
  is in C#. Read back by position: `(a, b).Item1`.
- **Literals:** every number form (`0x`, `0b`, `_`, suffixes), strings and characters with
  C#'s escapes, verbatim, interpolated (`$"{x,5:D3}"`) and raw strings (`"""…"""`,
  `$$"""{{x}}"""`).
- **Lambdas inside:** `(int y) => y * 2` anywhere a value is wanted; `n => n * 2` with its
  types left out only where it is handed to a call, which gives it the types of the
  overload it chooses — LINQ's methods through `using System.Linq;` read as they do in C#.
- Conversions, operators and overload resolution follow C#'s rules: `x + 1.5` over an `int`
  is a `double`, and `Math.Sqrt(x)` finds the `double` overload.

## Where it is not C#

- A method is never called with its type arguments written (`M<int>(x)`); they are inferred
  or it is refused. No argument is passed by `ref`, `out` or name.
- A constant is folded only across a minus: `byte b = 1;` and `sbyte s = -1;` fit, and
  `byte b = 1 + 1;` is refused where C# would fold it.
- `++`, `--` and compound assignments write to a name or one member of a name, not to an
  element and not to a longer chain.
- `new` may leave its parentheses out before an initializer, as C# may: `new List<int> { 1, 2 }`
  and `new List<int>() { 1, 2 }` both read. `new T` with neither tail is refused. An array is
  `new int[3]` or `new int[] { 1, 2 }`, never `new[] { 1, 2 }`.
- `default` needs its type: `default(int)`, never a bare `default`.
- `nameof` answers with the name as written and checks nothing.
- A tuple element cannot be named: write `(true, x)`, not `(Valid: true, Value: x)`. In C# a
  name is metadata beside the type and is erased from the value, and a compiled expression
  tree carries the type alone — so a name here could be read by nothing that runs.
- A lambda written INSIDE a text that says no types and is handed to no call is refused, as
  C# refuses it (CS8917). The outermost one is different: `Compile<TDelegate>` types it.
- An inner block may declare a name an outer one already has; the nearer one wins.

## Mistakes to avoid

1. Expecting `System` to be there. Write `using System;` or the full name.
2. Parsing in a loop through the forms that ask the stack. Pass the `Assembly`.
3. Wrapping `Parse` in a shared helper library and wondering why the caller's internal types
   are not found. The helper is the caller; pass the assembly through.
4. Catching only `FormatException` around `Parse`. A text that reads and means nothing
   throws `InvalidOperationException` or `ArgumentException`; use `TryParse` for text you do
   not control.
5. Treating the text as sandboxed. It can call any public member of any loaded type — file
   system, process, reflection. Parse only text you would run as code.
