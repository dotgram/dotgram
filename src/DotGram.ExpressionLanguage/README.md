# DotGram.ExpressionLanguage

A C#-style expression language written in `.gram`, compiled into `System.Linq.Expressions`
trees.

**ExpressionLanguage is not a sandbox.** Do not compile or execute untrusted expression
text. Expressions run with the permissions of the host process and can call accessible
members, create objects, and access the calling assembly's internal types and members.

It is an ordinary C# library. .Gram generated the parser into this assembly when it was
compiled, so nothing here carries a parser runtime, and neither does anything that
references it.

```csharp
using DotGram.ExpressionLanguage;

var square = ExpressionParser.Compile<Func<int, int>>("(int x) => x * x - 1");

square(3); // 8
```

It reads C#'s operators, at C#'s precedence, and its literals down to the digit separator
and the verbatim string. Past expressions it has typed locals, blocks, `if`, `while`, `do`,
`for`, `switch`, `try`/`catch`/`finally`, `throw`, `break`, `continue` and `return`, and
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

match.IsSuccess;   // false: there is no minus over a string
match.Error;       // what Expression.Subtract said about String and Int32
```

A type can be named in full or through a `using` at the top of the text. Nothing is imported
unasked, `System` included, and a text's `using`s are its own — the next text starts with none.
Resolution uses the calling assembly and its transitive runtime assembly references, loaded
through their owning load contexts on .NET. The caller's own types take precedence; other
assemblies contribute public types. Duplicate public full names in referenced assemblies
are ambiguous. Loading an unrelated plugin does not add names or extension methods.

This runtime reference graph is not the C# compiler's original reference list: a reference
unused by compiled code may not be recorded in assembly metadata. The overloads accepting
an `Assembly` explicitly choose the caller whose graph and internal members are available.

```csharp
var count = ExpressionParser.Compile<Func<IList<int>, int>>(
    """
    using System.Collections.Generic;

    (IList<int> l) => l.Count
    """);
```

Conversions, operators and overloads follow C#'s rules: `x + 1.5` over an `int` is a
`double`, `byte b = 1` fits, and `Math.Sqrt(x)` finds the `double` overload. Generic method
type arguments can be inferred from arguments, and imported extension methods are considered
when no applicable instance method is found, including LINQ calls such as `Where` and `Select`.
The implementation's remaining differences from C# are described in the source below.

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
