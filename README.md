# .Gram

.Gram is a source generator that compiles grammars into strongly typed C# parsers.

The grammar is known at compile time. The generated parser is ordinary C# in your own
assembly — there is no parser engine, grammar graph, or runtime library to interpret.

From a grammar, .Gram can generate:

* `Parse`, `TryParse`, and `Find` APIs;
* strongly typed results from named captures;
* parsers specialized for different versions of the same grammar;
* streaming parsers for `TextReader`;
* error recovery for record-oriented input;
* compile-time diagnostics that point back into the grammar.

Anything with a grammar: data formats and feeds, configuration files, wire protocols,
query and filter languages, markup, and languages of your own — including ones that build
`System.Linq.Expressions` trees or types you already have.

## Getting started

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

A grammar lives in the `[Gram]` attribute of the class the parser is generated into, or in
a `.gram` file listed as `<AdditionalFiles Include="Name.gram" />`. Inline grammars are raw
string literals and so need C# 11; a `.gram` file needs nothing more than the project
already has. [Compatibility](#compatibility) has the rest.

## One grammar, three parsers

A grammar does not have to describe only one parser. The arithmetic below is written once
and published three times: over `int`, over `decimal`, and as a tree.

```csharp
using System;

using DotGram;

[Gram("""
	@using System.Globalization;

	trivia = Std.Spacing?

	Value : @int = d: Std.Digits => @(int.Parse(d))
	Point        = Std.Digits & ('.' & Std.Digits)?

	Expr : Value = left: Expr & '+' & right: Expr  << 1 => @(left + right)
	             | left: Expr & '-' & right: Expr  << 1 => @(left - right)
	             | left: Expr & '*' & right: Expr  << 2 => @(left * right)
	             | left: Expr & '/' & right: Expr  << 2 => @(left / right)
	             | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
	             | '-' & operand: Expr             >> 3 => @(-operand)
	             | '(' & inner: Expr & ')'              => @(inner)
	             | value: Value                         => @(value)

	IntNumber     : @int     = d: Std.Digits => @(int.Parse(d))
	DecimalNumber : @decimal = d: Point      => @(decimal.Parse(d, CultureInfo.InvariantCulture))
	NodeNumber    : @Node    = d: Point      => @(new Node.Number(decimal.Parse(d, CultureInfo.InvariantCulture)))

	parse Expr with (Value = IntNumber)     as EvaluateInt
	parse Expr with (Value = DecimalNumber) as EvaluateDecimal
	parse Expr with (Value = NodeNumber)    as BuildTree
	""")]
public static partial class Calculator
{
	/// <summary>The tree the third parser builds, and the operators that build it.</summary>
	public abstract record Node
	{
		public sealed record Number(decimal Of)                     : Node;
		public sealed record Binary(char Op, Node Left, Node Right) : Node;
		public sealed record Negate(Node Of)                        : Node;

		public static Node operator +(Node left, Node right) => new Binary('+', left, right);
		public static Node operator -(Node left, Node right) => new Binary('-', left, right);
		public static Node operator *(Node left, Node right) => new Binary('*', left, right);
		public static Node operator /(Node left, Node right) => new Binary('/', left, right);
		public static Node operator -(Node of)               => new Negate(of);
	}

	static int     Raise(int     left, int     right) => (int)Math.Pow(left, right);
	static decimal Raise(decimal left, decimal right) => (decimal)Math.Pow((double)left, (double)right);
	static Node    Raise(Node    left, Node    right) => new Node.Binary('^', left, right);
}
```

Three parsers come out of it:

```csharp
Calculator.EvaluateInt("7 / 2");          // 3
Calculator.EvaluateDecimal("7 / 2");      // 3.5
Calculator.EvaluateInt("2 ^ 3 ^ 2");      // 512, since `^` groups to the right
Calculator.EvaluateInt("-2 ^ 2");         // -4, since a prefix is where an expression starts

Calculator.TryEvaluateInt("1.5");         // no match: `Value` is `IntNumber` there

Calculator.BuildTree("1 - 2 - 3");
// case Binary(-, Binary(-, Number(1), Number(2)), Number(3)) :
```

The whole language is one rule. `<< n` reads the operand on its right one strength
tighter, so the operator groups to the left; `>> n` reads it at `n`, so it groups to the
right. Higher binds tighter, and the numbers are the author's: the gaps are where a level
goes in later, as a number, with nothing else touched.

What separates the three parsers is the publication:

```text
parse Expr with (Value = IntNumber)     as EvaluateInt
parse Expr with (Value = DecimalNumber) as EvaluateDecimal
parse Expr with (Value = NodeNumber)    as BuildTree
```

`with` substitutes a rule through the grammar reachable from that publication, and the
result type follows the substitution: `Expr : Value` means "the type produced by `Value`",
so the three parsers return `int`, `decimal` and `Node`.

The actions do not change either. `left + right` is C#, and what it adds up to is the C#
compiler's: over `int` it is addition, and over `Node` it is the operator declared beside
the grammar, which builds a node. The grammar says where an operator goes and which
operands it takes; what it then means is not the grammar's business.

`^` is the exception that shows what the rest rests on. C# has no `^` for `decimal`, and
its `^` on `int` is exclusive-or rather than power, so that one alternative calls a method
— overloaded the three ways the operators are overloaded once.

There is no runtime generic dispatch and no parser configuration object. All three parsers
are specialized when the C# is generated.

## Something much smaller

The example above is deliberately a language. .Gram does not ask for that scale: a grammar
the size of a regular expression stays that size, and is written much like one.

```csharp
using DotGram;

[Gram("""
	Hex   = ['0'..'9' | 'a'..'f' | 'A'..'F']
	Color = '#' & value: Hex{6}

	parse Color
	""")]
public static partial class CssColor;
```

Use it as ordinary C#:

```csharp
var color = CssColor.ParseColor("#12aBcF");

Console.WriteLine(color.Value);       // 12aBcF

var result = CssColor.TryParseColor("#xyz");

Console.WriteLine(result.IsSuccess);  // False
```

The equivalent regular expression would be roughly:

```text
^#(?<value>[0-9a-fA-F]{6})$
```

Ranges, alternatives, `?`, `*`, `+` and `{n}` mean what they mean in a regular expression.

`value:` does more than a regex capture does: it becomes a property of the generated
result type.

Keeping a grammar this size in the attribute puts the parser and its C# API on one screen.
A grammar long enough to want its own place goes in a `.gram` file instead.

## Typed parsing

Named captures define the shape of the result.

```csharp
using DotGram;

[Gram("""
	Feed    = header: Header & rows: Row* & trailer: Trailer & eof

	Header  = "H" & '|' & date: Date & eol
	Row     = "R" & '|' & symbol: Text & '|' & quantity: Digit+ & eol
	Trailer = "T" & '|' & count: Digit+ & eol
	Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	Text  = [^ '|' | '\r' | '\n']+
	Digit = ['0'..'9']

	parse Feed
	find Row as AllRows
	""")]
public static partial class FeedParser;
```

The parser returns that structure directly:

```csharp
var feed = FeedParser.ParseFeed(text);

feed.Header.Date.Year;
feed.Rows[0].Symbol;
feed.Rows[0].Quantity;
feed.Trailer.Count;
```

There is no generic parse tree and no visitor required to turn it into application data.
Captures can also be matched straight to a constructor or to the `required` properties of
a type you already have, in which case the grammar contains no construction code at all.

`find` publishes a rule as a lazy search through the input:

```csharp
foreach (var row in FeedParser.AllRows(text))
	Console.WriteLine(row.Value.Symbol);
```

## Streaming, tokens and recovery

A character parser reads either input that is all there or a reader, and which of the two
runs is a property of the data rather than of the grammar: the overload that was called
settles it at the call site. `Lexical = true` on the `[Gram]` attribute generates a token parser instead
— a lexical half makes the tokens, and the half above it decides each choice by the token
in front of it, which is what a parser written by hand does. A token parse reads from
memory only.

Over characters, a reader overload appears wherever the generator can prove that input may
be released as the parse goes.

```csharp
using DotGram;

[Gram("""
	Text  = [^ '|' | '\r' | '\n']+
	Digit = ['0'..'9']

	Header          = "H" & '|' & Text & eol
	Row   : @string = "R" & '|' & t: Text & eol => @(t)
	Trailer         = "T" & '|' & Digit+ & eol

	Feed : @string[] = Header & Row* & Trailer & eof

	parse Feed
	""")]
public static partial class StreamingFeed;
```

`Feed` collects what its operands produce: `Row` builds a `string`, while the header and
trailer build nothing and so join nothing. Four methods come out of it — `ParseFeed` and
`TryParseFeed` over a `string`, and `ParseFeed` over a `TextReader` and over an
`IEnumerable<string>`, which reuse a buffer rather than hold the input:

```csharp
using var reader = File.OpenText("large.feed");

foreach (var row in StreamingFeed.ParseFeed(reader))
	Handle(row);
```

This is what makes the size of the input stop mattering. The window is bounded and reused
as the parse moves along it, so what is held is the record being read rather than the file,
and each record reaches the caller as it is read rather than in an array of all of them at
the end. Memory does not grow with the file, and a feed of tens of gigabytes costs what one
record costs.

Record-oriented formats can also recover after malformed input:

```csharp
using DotGram;

[Gram("""
	Text = [^ '|' | '\r' | '\n']+

	Row : @string = "R" & '|' & t: Text & eol => @(t)

	Feed : @string[] = Row* recover eol => @(parserText)

	parse Feed
	""")]
public static partial class RecoveringFeed;
```

`recover eol` says where the repetition may pick itself up, and the `=>` says what to make
of what it rejected — here the text of the bad line, which arrives in the sequence beside
the good ones. A rejection can as well become a record of its own, or go to a `partial
void` hook and stay out of the result entirely: a bad record becomes data describing the
rejection instead of ending the feed.

[`docs/syntax.md`](docs/syntax.md) §6.3 says which grammars get a reader overload and why,
and §8.2 what `recover` may promise.

## Grammar and C#

`@` is the boundary between grammar and C#. A rule declares its result as a C# type and
builds it with an action, as the arithmetic above does; and a guard can ask a C# question
in the middle of a parse, which a grammar cannot express on its own:

```csharp
using DotGram;

[Gram("""
	Name = ['a'..'z' | 'A'..'Z']+

	Tag
		= '<' & open: Name & '>'
		& "</" & close: Name & '>'
		& when @(open == close)

	parse Tag
	""")]
public static partial class Tags;
```

The same boundary calls predicates, external recognizers and constructors. The grammar
describes the syntax; the C# beside it does what is easier to write in C#.

## Grammar libraries

A grammar can be written on top of another one. `[GramInclude]` names the class that hosts
it, and the name this grammar will know it by:

```csharp
[Gram("""
	Word   = ['a'..'z']+
	Digits = ['0'..'9']+
	""")]
public static partial class Lexemes;

[GramInclude(typeof(Lexemes), As = "Lex")]
[Gram("""
	trivia = ' '*

	Setting : @string = key: Lex.Word & '=' & value: Lex.Digits => @(key + " is " + value)

	parse Setting
	""")]
public static partial class Settings;
```

Each include is spliced into a namespace of its own, so `Lex.Word` and a `Word` of your
own are different rules and cannot collide by accident.

The include crosses a project reference, which is what makes a grammar a library. What
travels is the grammar rather than a parser: the including assembly generates its own from
it, under its own substitutions.
[`TransactSql`](src/DotGram.Parsers/TransactSql.gram) is built that way on
[`SqlStandard92`](src/DotGram.Parsers/SqlStandard92.gram) — a dialect the size of its
difference, with the standard underneath written once.

## DotGram.Parsers

[`DotGram.Parsers`](src/DotGram.Parsers) is a set of parsers written in .Gram against
published specifications, and a package of its own.

```csharp
var uri = Rfc3986.ParseUri("https://user@example.com:8080/a/b?q=1#top");

uri.Host;   // example.com
uri.Port;   // 8080
uri.Path;   // /a/b
uri.Query;  // q=1
```

The expression language calls the `System.Linq.Expressions` factories directly, so there
is no tree of its own to translate afterwards:

```csharp
var square = ExpressionLanguage.Compile<Func<int, int>>("(int x) => x * x - 1");

square(3);  // 8
```

The SQL parsers build a tree of their own, and read it back as ordinary records:

```csharp
var match  = TransactSql.TryParseSelect("select name from Users where id > @id");
var select = (Statement.Select)match.Value;
var query  = (Query.Specification)select.Of;

query.From[0];  // TableReference.Named { Table = "Users" }
```

| Parser | What it reads |
| --- | --- |
| [`Rfc3986`](src/DotGram.Parsers/Rfc3986.cs) | URIs and relative references after RFC 3986 — authority, IPv4, IPv6, `IPvFuture`, paths, queries, fragments, percent encoding |
| [`ExpressionLanguage`](src/DotGram.Parsers/ExpressionLanguage.cs) | a C#-style expression language that builds `System.Linq.Expressions` trees directly, with parameters, locals, blocks and `return` |
| [`SqlStandard92`](src/DotGram.Parsers/SqlStandard92.gram) | SQL-92, read through a lexical split |
| [`TransactSql`](src/DotGram.Parsers/TransactSql.gram) | T-SQL, written as a dialect over SQL-92 rather than as a copy of it |

[`src/DotGram.Parsers/README.md`](src/DotGram.Parsers/README.md) has what each one parses
and what it hands back.

## Performance

The URL benchmark compares a .Gram URL grammar with the same language transcribed
rule-for-rule into a regular expression. Both are held to agreeing on every tested input,
and on every part they pull out of it, before anything is timed.

| Input | .Gram | `RegexOptions.Compiled` | |
| --- | ---: | ---: | ---: |
| short URL | 133.8 ns | 298.9 ns | 2.23× |
| host as IPv4 | 146.9 ns | 285.4 ns | 1.94× |
| invalid URL | 80.2 ns | 113.5 ns | 1.42× |
| 84-character path | 191.0 ns | 453.0 ns | 2.37× |
| every part named | 274.4 ns | 278.0 ns | 1.01× |

So: from level with `RegexOptions.Compiled` to 2.4× faster, and 2.2× to 6.5× against
interpreted `Regex`. Both sides are asked for the parsed values rather than only whether
the input matched. [`benchmarks`](benchmarks/) has the method and the rest of the numbers.

## Visual Studio

`DotGram.VisualStudio` is an extension for Visual Studio: classification, diagnostics,
Quick Info, navigation, completion, brace matching and folding, for `.gram` files and for
a grammar written inside a `[Gram]` string.

It does the same for the language you generate. A parser host that names its language
lends that name to `StringSyntax`, and a literal passed to such a parameter is edited as
that language rather than as text:

```csharp
[Gram("""
	trivia = ' '*

	Word   = ['a'..'z']+
	Filter = field: Word & '=' & value: Word

	parse Filter
	""")]
[GramLanguage("filter")]
public static partial class FilterLanguage;

void Execute([StringSyntax("filter")] string query);

Execute("status = active");   // classified, checked and completed as Filter
```

[`docs/visual-studio.md`](docs/visual-studio.md) covers the rest, along with building and
installing; the extension targets Visual Studio 18.

## No runtime parser library

`DotGram` is a source-generator package: everything a generated parser needs is emitted
into the consuming assembly as internal C#. There is no runtime assembly to deploy, and no
generator/runtime version pair that can drift apart.

## What .Gram supports

* literals and element sets;
* ranges and Unicode categories;
* sequence and ordered choice;
* `?`, `*`, `+`, and bounded repetition;
* lookahead and atomic groups;
* named captures and generated result types;
* existing C# result types, filled by constructor or by `required` properties;
* semantic actions and guards;
* external C# predicates and recognizers;
* parameterized rules;
* rule rebinding and parser specialization;
* left recursion, and binding powers for expression grammars;
* grammar namespaces, and grammar libraries that cross a project reference;
* a lexical split: the same notation read over tokens instead of characters;
* `Parse`, `TryParse`, and `Find`;
* streaming from `TextReader`;
* recovery inside repetitions;
* parser context and parsing state.

[`docs/status.md`](docs/status.md) lists the features one by one, with what is not
implemented and what is only partly.

## Examples

Complete examples are under [`examples/DotGram.Examples`](examples/DotGram.Examples/).

| Example | What it demonstrates |
| --- | --- |
| [`JsonExample.cs`](examples/DotGram.Examples/JsonExample.cs) | recursive structured data |
| [`XmlExample.cs`](examples/DotGram.Examples/XmlExample.cs) | a closing tag checked against its opening tag |
| [`TypedCsvExample.cs`](examples/DotGram.Examples/TypedCsvExample.cs) | construction of existing C# types |
| [`GramExample.cs`](examples/DotGram.Examples/GramExample.cs) | the .Gram notation parsed by .Gram itself |

[`examples/README.md`](examples/README.md) lists the rest, in the order they are worth
reading.

## Documentation

| Document | Contents |
| --- | --- |
| [`docs/syntax.md`](docs/syntax.md) | grammar notation and generated API |
| [`docs/implementation.md`](docs/implementation.md) | how the generated parser works |
| [`docs/diagnostics.md`](docs/diagnostics.md) | compiler diagnostics |
| [`docs/visual-studio.md`](docs/visual-studio.md) | the Visual Studio extension, and the `StringSyntax` annotations |
| [`docs/status.md`](docs/status.md) | implemented features, limitations, and measurements |

## Compatibility

The generated parser is C# 8 and targets whatever the project around it targets.
[`tests/DotGram.Compatibility`](tests/DotGram.Compatibility) builds one grammar for
`netstandard2.0`, `net472` and `net8.0` at C# 8 on every build.

Two things an older target has to say:

* **A grammar written inside `[Gram]` is a raw string literal, which is C# 11.** On
  `netstandard2.0` and `net472` the default is C# 7.3, so `<LangVersion>` has to be set.
  A grammar in a `.gram` file needs nothing: the generated code is C# 8.
* **`netstandard2.0` and `net472` need `System.Memory`**, for the `ReadOnlySpan<char>`
  the generated methods take.

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net472'">
  <LangVersion>11.0</LangVersion>
</PropertyGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net472'">
  <PackageReference Include="System.Memory" Version="4.5.5" />
</ItemGroup>
```

The generator is a Roslyn analyzer built against `Microsoft.CodeAnalysis` 4.14 and needs a
compiler at least that new.

## Building

```sh
dotnet build DotGram.slnx
dotnet test  DotGram.slnx
```

## License

[MIT](LICENSE)
