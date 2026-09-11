# DotGram.ExpressionLanguage

A C#-style expression language written in `.gram`, compiled into `System.Linq.Expressions`
trees.

It is an ordinary C# library. .Gram generated the parser into this assembly when it was
compiled, so nothing here carries a parser runtime, and neither does anything that
references it.

```csharp
using DotGram.Expressions;

var square = ExpressionLanguage.Compile<Func<int, int>>("(int x) => x * x - 1");

square(3); // 8
```

It supports parameters, local variables, blocks, and `return`:

```csharp
var calculate = ExpressionLanguage.Compile<Func<int, int, int>>(
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
var expression = ExpressionLanguage.Parse("(double x) => x / 2.0");

Console.WriteLine(expression);   // x => (x / 2)
```

The grammar calls `System.Linq.Expressions` factories directly. There is no intermediate
AST specific to .Gram that must later be translated into an expression tree — which also
means a factory that does not exist, or one handed the wrong type, is a C# error on the
line of the grammar that asked for it rather than an exception at run time.

The grammar and the C# it calls are one file,
[`ExpressionLanguage.cs`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.ExpressionLanguage/ExpressionLanguage.cs).

## Taking it

```
dotnet add package DotGram.ExpressionLanguage
```

There is no companion runtime package, and no generator to install alongside it: the
parser was generated when this assembly was compiled.
