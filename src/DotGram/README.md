<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — the notation, its seam with C#, and what the generator's diagnostics mean.
  Read it before writing a grammar. In a restored package that is
  ~/.nuget/packages/dotgram/<version>/SKILL.md.
-->

# .Gram

.Gram is a source generator that compiles grammars into strongly typed C# parsers.

The grammar is known at compile time. The generated parser is ordinary C# in your own
assembly — there is no parser engine, grammar graph, or runtime library to interpret, and
nothing to deploy beside your application.

## Getting started

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

A grammar lives in the `[Gram]` attribute of the class the parser is generated into, or in
a `.gram` file listed as `<AdditionalFiles Include="Name.gram" />`. An inline grammar is a
raw string literal, so it needs C# 11; a `.gram` file needs nothing more than the project
already has.

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
right. What separates the three parsers is the publication:

```text
parse Expr with (Value = IntNumber)     as EvaluateInt
parse Expr with (Value = DecimalNumber) as EvaluateDecimal
parse Expr with (Value = NodeNumber)    as BuildTree
```

`with` substitutes a rule through the grammar reachable from that publication, and the
result type follows it: `Expr : Value` means "the type produced by `Value`". The actions
do not change either — `left + right` is C#, and over `Node` it is the operator declared
beside the grammar, which builds a node instead of adding anything.

## Named captures become the result

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
feed.Trailer.Count;

foreach (var row in FeedParser.AllRows(text))
	Console.WriteLine(row.Value.Symbol);
```

There is no generic parse tree and no visitor required to turn it into application data.
Captures can also be matched straight to a constructor or to the `required` properties of
a type you already have, in which case the grammar contains no construction code at all.

## C# where a grammar cannot go

`@` is the boundary. A rule declares its result as a C# type and builds it with an action;
a guard asks a C# question in the middle of a parse:

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

The same boundary calls predicates, external recognizers and constructors.

## What .Gram supports

* literals, element sets, ranges and Unicode categories;
* sequence, ordered choice, `?`, `*`, `+` and bounded repetition;
* lookahead and atomic groups;
* named captures and generated result types;
* existing C# result types, filled by constructor or by `required` properties;
* semantic actions, guards, external C# predicates and recognizers;
* parameterized rules, rule rebinding and parser specialization;
* left recursion, and binding powers for expression grammars;
* grammar namespaces, and grammar libraries that cross a project reference;
* a lexical split: the same notation read over tokens instead of characters;
* `Parse`, `TryParse` and `Find`;
* streaming from a `TextReader`, and recovery inside repetitions.

## No runtime parser library

Everything a generated parser needs is emitted into the consuming assembly as internal C#.
There is no runtime assembly to deploy, and no generator/runtime version pair that can
drift apart.

## Compatibility

The generated parser is C# 8 and targets whatever the project around it targets.
`netstandard2.0`, `net472` and `net10.0` all compile it.

* A grammar written inside `[Gram]` is a raw string literal, which is C# 11. On
  `netstandard2.0` and `net472` the default is C# 7.3, so `<LangVersion>` has to be set.
  A grammar in a `.gram` file asks for nothing.
* `netstandard2.0` and `net472` need `System.Memory`, for the `ReadOnlySpan<char>` the
  generated methods take.

## Documentation

Everything else is at [github.com/dotgram/dotgram](https://github.com/dotgram/dotgram):
the [notation in full](https://github.com/dotgram/dotgram/blob/main/docs/syntax.md), the
[diagnostics](https://github.com/dotgram/dotgram/blob/main/docs/diagnostics.md),
[whole parsers to copy](https://github.com/dotgram/dotgram/tree/main/examples/DotGram.Examples),
and the [benchmarks](https://github.com/dotgram/dotgram/tree/main/benchmarks).

[`DotGram.Parsers`](https://github.com/dotgram/dotgram/tree/main/src/DotGram.Parsers) is a
package of its own: RFC 3986 URIs, a C#-style expression language that builds
`System.Linq.Expressions` trees, and SQL-92 with T-SQL written as a dialect over it. They
are also the largest grammars there are to read.

[MIT](https://github.com/dotgram/dotgram/blob/main/LICENSE)
