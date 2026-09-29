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

> **It is not a sandbox**: do not compile text you do not trust. See
> [It is not a sandbox](#it-is-not-a-sandbox) below.

It reads C#'s operators, at C#'s precedence, and its literals down to the digit separator
and the verbatim string. Past expressions it has typed locals, blocks, `if`, `while`, `do`,
`for`, `switch` — the statement and `x switch { 1 or 2 => …, _ => … }` —
`try`/`catch`/`finally`, `throw`, `break`, `continue` and `return`, and
past the keywords it has members, calls, indexers, `new` with initializers, tuples, generic
types, `is`, `as`, casts and `checked`:

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

`Compile`, `Parse` and `TryParse` may all be called from any number of threads at once — the
generated parser's own contract — and the member-resolution caches behind name and overload
lookups are shared across every call and thread, kept in their own concurrent tables rather
than per call.

A type named rather than spelled as a keyword is found the way C# finds one: written whole,
or through a `using` at the top of the text. Five namespaces are there already — the set a
new project gets: `System`, `System.Collections.Generic`, `System.Linq`, `System.Text` and
`System.Threading.Tasks` — and a text's own `using`s are its own, so the next text starts
with those five again and nothing more. `ResolutionScope.DefaultImports` names them, and
`ResolutionScope.WithoutDefaultImports()` is a scope that leaves them out. A `using` may also
give a name to one type or one namespace (`using L = System.Collections.Generic.List<int>;`),
or bring a type's static members, nested types and extension methods into reach as bare names
(`using static System.Math; () => Abs(-2)`). A name the text itself declares wins over one a
`using static` gives, as in C#; and `static` is a keyword here, as it is in C#, so nothing may
be named it. What a text can name is what C# written in the calling assembly could: public
types, and that assembly's own internal types and members. A full name that two referenced
assemblies declare as two different types is refused where it is used, naming both assemblies,
as C# refuses it (CS0433). One type that several assemblies answer for, as a facade forwards
`System.Object`, is one type; and the calling assembly's own type wins over a reference's, as
the compilation's does in C#.

```csharp
using System.Collections.Generic;

// `System.Collections.Generic` is one of the five, so the text needs no `using` of its own.
var count = ExpressionParser.Compile<Func<IList<int>, int>>("(IList<int> l) => l.Count");

// One it does not get, it says itself.
var named = ExpressionParser.Compile<Func<string>>(
    """
    using System.Globalization;

    () => CultureInfo.InvariantCulture.Name
    """);
```

Conversions, operators and overloads follow C#'s rules: `x + 1.5` over an `int` is a
`double`, `byte b = 1` fits, and `Math.Sqrt(x)` finds the `double` overload. A generic
method takes its type arguments from its arguments, an extension method is found through a
`using`, and a lambda that says no types takes them from the overload it is handed to, so
`a.Where(n => n > 1).Sum()` reads as it does in C#. The lambda a text IS takes them the same
way from the delegate it is compiled to, so `Compile<Func<int, int>>("x => x * x")` reads
with no types written at all. Where it is not C# — a method is never
called with its type arguments written, and a constant is folded only across a minus — is
written down, with the reason for each, at the top of the file below.

Where a name is looked for is the caller's to say. By default it is the calling assembly and
what it references — what a compilation of that assembly would see — so the same text answers the
same whatever else the process has loaded. `ResolutionScope.Of(caller, plugin)` widens that,
`WithoutInternals()` reads as another assembly would, and what a name MEANS stays C#'s either way.
A caller loaded into an `AssemblyLoadContext` of its own has its references found in that
context, as the runtime finds them for its own code.

The grammar calls `System.Linq.Expressions` factories directly. There is no intermediate
AST specific to .Gram that must later be translated into an expression tree — which also
means a factory that does not exist, or one handed the wrong type, is a C# error on the
line of the grammar that asked for it rather than an exception at run time.

The grammar and the C# it calls are one file,
[`ExpressionParser.cs`](https://github.com/dotgram/dotgram/blob/main/src/DotGram.ExpressionLanguage/ExpressionParser.cs).

## It is not a sandbox

**Parse only text you would run as code.** A text can name any type the reading can reach and
call any member on it — the file system, the process, reflection — and what it compiles to runs
with the host's permissions, in the host's process, for as long as the delegate is called. There
is no allow-list, no timeout and no quota, and none is planned: this compiles expressions, it
does not contain them.

That is the same position `System.Linq.Expressions` itself takes, and the same one
`CSharpScript` takes. It matters here because the input LOOKS like data — a formula in a
configuration file, a rule typed into a form — and a formula from somewhere you do not control
is code from somewhere you do not control.

If the text comes from a user, the answer is not to inspect it before parsing. It is to run it
where it can do no harm: a process of its own, with the rights you are willing to lose.

## Taking it

```
dotnet add package DotGram.ExpressionLanguage
```

There is no companion runtime package, and no generator to install alongside it: the
parser was generated when this assembly was compiled.
