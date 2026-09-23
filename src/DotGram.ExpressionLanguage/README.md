<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — which entry point to use, who is calling, how names are found, what a text
  may say and where it is not C#. Read it before writing code against the package. In a
  restored package that is ~/.nuget/packages/dotgram.expressionlanguage/<version>/SKILL.md.
-->
# DotGram.ExpressionLanguage

A C#-style expression language written in `.gram`, compiled into `System.Linq.Expressions`
trees.

It is an ordinary C# library. .Gram generated the parser into this assembly when it was
compiled, so nothing here carries a parser runtime, and neither does anything that
references it.

```csharp
using System;

using DotGram.ExpressionLanguage;

var square = ExpressionParser.Compile<Func<int, int>>("(int x) => x * x - 1");

square(3); // 8
```

It reads C#'s operators, at C#'s precedence, and its literals down to the digit separator
and the verbatim string. Past expressions it has typed locals, blocks, `if`, `while`, `do`,
`for`, `switch` — the statement and `x switch { 1 or 2 => …, _ => … }` —
`try`/`catch`/`finally`, `throw`, `break`, `continue` and `return`, and
past the keywords it has members, calls, indexers, `new` with initializers, generic types,
`is`, `as`, casts and `checked`:

```csharp
var calculate = ExpressionParser.Compile<Func<int, int, int>>(
    """
    (int x, int y) =>
    {
        int sum = x + y;
        return sum * sum;
    }
    """);

calculate(2, 3); // 25
```

Or keep the expression tree instead of compiling it:

```csharp
var expression = ExpressionParser.Parse("(double x) => x / 2.0");

Console.WriteLine(expression);   // x => (x / 2)
```

Or ask, rather than catch: `TryParse` answers for everything `Parse` would throw for — text
that is not this language, and text that is and means nothing, such as a name nothing
declares or an operator its operands do not support.

```csharp
var match = ExpressionParser.TryParse("(string s) => s - 1");

if (!match.IsSuccess)                    // there is no minus over a string
    Console.WriteLine(match.Error);      // what Expression.Subtract said about String and Int32
```

A type named rather than spelled as a keyword is found the way C# finds one: written whole,
or through a `using` at the top of the text. Nothing is imported unasked, `System`
included, and a text's `using`s are its own — the next text starts with none. What it can
name is what C# written in the calling assembly could: public types, and that assembly's
own internal types and members.

```csharp
using System.Collections.Generic;

var count = ExpressionParser.Compile<Func<IList<int>, int>>(
    """
    using System.Collections.Generic;

    (IList<int> l) => l.Count
    """);
```

Conversions, operators and overloads follow C#'s rules: `x + 1.5` over an `int` is a
`double`, `byte b = 1` fits, and `Math.Sqrt(x)` finds the `double` overload. A generic
method takes its type arguments from its arguments, an extension method is found through a
`using`, and a lambda that says no types takes them from the overload it is handed to, so
`a.Where(n => n > 1).Sum()` reads as it does in C#. Where it is not C# — a method is never
called with its type arguments written, and a constant is folded only across a minus — is
written down, with the reason for each, at the top of the file below.

The grammar calls `System.Linq.Expressions` factories directly. There is no intermediate
AST specific to .Gram that must later be translated into an expression tree — which also
means a factory that does not exist, or one handed the wrong type, is a C# error on the
line of the grammar that asked for it rather than an exception at run time.

The grammar and the C# it calls are one file,
[`ExpressionParser.cs`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.ExpressionLanguage/ExpressionParser.cs).

## Taking it

```
dotnet add package DotGram.ExpressionLanguage
```

There is no companion runtime package, and no generator to install alongside it: the
parser was generated when this assembly was compiled.
