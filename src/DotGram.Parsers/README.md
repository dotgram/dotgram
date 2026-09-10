# DotGram.Parsers

Parsers for real formats, written in `.gram`. Where an example shows one feature, a parser
here is written against a whole specification.

They are ordinary C# libraries. .Gram generates the parser into this assembly at compile
time, so nothing here carries a parser runtime, and neither does anything that references
it.

## RFC 3986 URI parser

[`Rfc3986`](Rfc3986.cs) follows RFC 3986 closely, including absolute URIs, relative
references, IPv4, IPv6, `IPvFuture`, authority, paths, queries, fragments, and percent
encoding.

```csharp
using DotGram.Parsers;

var uri = Rfc3986.ParseUri("https://user@example.com:8080/a/b?q=1#top");

uri.Scheme;    // https
uri.UserInfo;  // user
uri.Host;      // example.com
uri.Port;      // 8080
uri.Path;      // /a/b
uri.Query;     // q=1
uri.Fragment;  // top
```

URI references can be relative:

```csharp
var reference = Rfc3986.ParseReference("../images/logo.png?size=2");

reference.Scheme;  // null
reference.Path;    // ../images/logo.png
reference.Query;   // size=2
```

Percent decoding is deliberately separate from parsing:

```csharp
Rfc3986.Decode("hello%20world"); // hello world
```

`%2F` inside a path segment is encoded data during parsing; decoding it early would turn
it into a path separator it is not.

## Expression language

[`ExpressionLanguage`](ExpressionLanguage.cs) is a C#-style expression language that
produces `System.Linq.Expressions` trees.

```csharp
using DotGram.Parsers;

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

## SQL

Two grammars, and the second is written as a dialect of the first rather than as a copy
of it.

[`SqlStandard92.gram`](SqlStandard92.gram) is SQL-92 as the standard writes it.
[`TransactSql.gram`](TransactSql.gram) names it — `[GramInclude(typeof(SqlStandard92), As
= "Sql92")]` — and rebinds the rules where T-SQL differs, so what the two languages share
is written once and the dialect is the size of the difference.

Both read through a lexical split (`Lexical = true`): a lexical half makes tokens, and the
syntactic half above it decides each choice by the token in front of it, which is what a
parser written by hand does.

```csharp
using DotGram.Parsers;
using DotGram.Parsers.Sql;

var match = TransactSql.TryParseSelect("select name from Users where id > @id");

var select = (Statement.Select)match.Value;
var query  = (Query.Specification)select.Of;

query.From[0];   // TableReference.Named { Table = "Users" }
```

The tree they build is described in [`docs/ast.md`](https://github.com/dotgram/dotgram/blob/main/docs/ast.md). Both are still
growing: what they read is held against a corpus of somebody else's SQL and against a
round trip — parse, print, and compare the two readings — which catches a parser that
answers yes and builds the wrong thing.

## Taking one

```xml
<PackageReference Include="DotGram.Parsers" Version="0.1.0" />
```

There is no companion runtime package, and no generator to install alongside it: the
parsers were generated when this assembly was compiled.
