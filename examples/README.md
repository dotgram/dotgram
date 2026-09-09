# Examples

Whole, working parsers, meant to be copied. No test framework and no scaffolding —
each file is a grammar, the class it attaches to, and the code somebody would write
against it. `DotGram.Tests` runs them; nothing here knows that.

Read them in this order if you are reading rather than looking something up: a format,
then a feed, then an expression language, then one of the larger ones.

## Formats

| | |
| --- | --- |
| [`JsonExample.cs`](DotGram.Examples/Formats/JsonExample.cs) | JSON — recursive structure, and a rule that reads what it built into a type of its own |
| [`IniExample.cs`](DotGram.Examples/Formats/IniExample.cs) | an INI file, read into the shape a caller wants rather than into sections and keys |
| [`TypedCsvExample.cs`](DotGram.Examples/Formats/TypedCsvExample.cs) | a CSV into records, with no `=>` anywhere: captures matched to a constructor by name |
| [`UrlExample.cs`](DotGram.Examples/Formats/UrlExample.cs) | a URL, after RFC 3986 — captures, optional parts, `find` |
| [`HttpHeadersExample.cs`](DotGram.Examples/Formats/HttpHeadersExample.cs) | header fields into a lookup, continuation lines and all |
| [`FixedWidthExample.cs`](DotGram.Examples/Formats/FixedWidthExample.cs) | fields found by counting rather than by looking for a delimiter |
| [`NetstringExample.cs`](DotGram.Examples/Formats/NetstringExample.cs) | netstrings — the one shape a grammar genuinely cannot express, and what to do about it |
| [`FixExample.cs`](DotGram.Examples/Formats/FixExample.cs) | FIX messages, and the C# that checks what the grammar cannot |
| [`XmlExample.cs`](DotGram.Examples/Formats/XmlExample.cs) | XML, cut to elements and attributes — a closing tag checked against its opening one |
| [`MarkdownExample.cs`](DotGram.Examples/Formats/MarkdownExample.cs) | block structure — headings, bullets, fenced code, paragraphs |
| [`YamlExample.cs`](DotGram.Examples/Formats/YamlExample.cs) | nesting by indentation, to any depth |
| [`MetricsLineExample.cs`](DotGram.Examples/Formats/MetricsLineExample.cs) | a line of measurements, and the standard library doing nearly all of it — `using Std;`, comments and numbers said by name |
| [`LocatedConfigExample.cs`](DotGram.Examples/Formats/LocatedConfigExample.cs) | one grammar read twice — `[GramOptions]`, `LocationType`, and `: @SourceSpan` where a rule is simply its own extent |
| [`FileNameExample.cs`](DotGram.Examples/Formats/FileNameExample.cs) | a path split into names the platform would take — `[@M]`, a set of characters that is data rather than syntax |

## Feeds

Record-oriented input, four ways, over one line-oriented format.

| | |
| --- | --- |
| [`FeedExample.cs`](DotGram.Examples/Feeds/FeedExample.cs) | nested rule values, a sequence of records, an envelope checked as a whole |
| [`RecoveringFeedExample.cs`](DotGram.Examples/Feeds/RecoveringFeedExample.cs) | the same feed read past a malformed record — `recover`, and rejections arriving beside the records |
| [`LoggingFeedExample.cs`](DotGram.Examples/Feeds/LoggingFeedExample.cs) | the same again with the rejections sent elsewhere — `recover` with no `=>`, and a `partial void` that vanishes when nobody implements it |
| [`StreamingFeedExample.cs`](DotGram.Examples/Feeds/StreamingFeedExample.cs) | the same feed out of a `TextReader` — a result in parts, a window reused, and a trailer checked against records nobody held |

## Expressions

| | |
| --- | --- |
| [`CalculatorExample.cs`](DotGram.Examples/Expressions/CalculatorExample.cs) | arithmetic in one rule, published three times — `<< n` and `>> n`, `with` over `int`, `decimal` and a tree, and operators declared beside the grammar |
| [`ExpressionTreeExample.cs`](DotGram.Examples/Expressions/ExpressionTreeExample.cs) | the same arithmetic as five rules, building a tree that is somebody else's type |
| [`Expression.cs`](DotGram.Examples/Expressions/Expression.cs) | the tree it builds, and everything the tree can do. No grammar in it, deliberately: what a tree means is not the parser's business |
| [`LocaleNumberExample.cs`](DotGram.Examples/Expressions/LocaleNumberExample.cs) | one decimal-number rule read under two decimal points — `namespace N with (A = B) { ... }` |
| [`ExtensionNodeExample.cs`](DotGram.Examples/Expressions/ExtensionNodeExample.cs) | a `=>` that builds a node `System.Linq.Expressions` has no factory for |

## Languages

| | |
| --- | --- |
| [`FilterExample.cs`](DotGram.Examples/Languages/FilterExample.cs) | the filter language an API puts in a query string — binding powers, and a tree |
| [`SelectorExample.cs`](DotGram.Examples/Languages/SelectorExample.cs) | `orders[2].lines.total(net)` read as the chain of steps it is |
| [`SqlReadOnlyExample.cs`](DotGram.Examples/Languages/SqlReadOnlyExample.cs) | a guard that answers one question: can this statement write anything? |
| [`GramExample.cs`](DotGram.Examples/Languages/GramExample.cs) | the .Gram notation parsed by .Gram itself, building a tree |
| [`TokenizedQueryExample.cs`](DotGram.Examples/Languages/TokenizedQueryExample.cs) | a query read over tokens — `Lexical = true`, a lexer under the rules, and keywords that end where a word does |
| [`LexemeLibraryExample.cs`](DotGram.Examples/Languages/LexemeLibraryExample.cs) | one grammar written to be built on and two written on it — `[GramInclude]`, and why the names cannot collide |
| [`ScopedExpressionExample.cs`](DotGram.Examples/Languages/ScopedExpressionExample.cs) | a language where a name must be declared before it is used — `context`, and what a `when` can do that a `=>` cannot |
| [`CaseRegionExample.cs`](DotGram.Examples/Languages/CaseRegionExample.cs) | a region that changes what the same rule builds inside it — `state`, `with state`, and the nearest mark winning |

## Taking one

Two things, and nothing else:

```xml
<PackageReference Include="DotGram" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

```csharp
[Gram("""
    Digits = ['0'..'9']+
    parse Digits
    """)]
public static partial class Numbers
{
    public static int Sum(string text) => ParseDigits(text).Length;   // ParseDigits is generated here
}
```

The attribute goes on the class you want the parser in, and the methods and types
appear in it — there is nothing to wire up and nothing to name twice. The class must
be `partial`, and every class around it too; `static` is fine. No runtime assembly is
referenced, because there is none: everything the parser needs is generated into your
own compilation.

Give the grammar a class of its own when the generated methods should not be part of
your API — an `internal partial class` beside your public one, which is also the only
way to keep `ParseX` and `TryParseX` out of it.

A grammar long enough to want its own place goes in a `.gram` file instead, named
after the class or given to the attribute, and listed so the generator can see it:

```xml
<AdditionalFiles Include="Numbers.gram" />
```

## Seeing what was generated

Both example projects write it to disk:

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)GeneratedFiles</CompilerGeneratedFilesOutputPath>
```

After a build it is under `obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator`.

## What these deliberately do not show

Things the specification describes and the compiler does not do yet — a rejection
whose `parserMessage` names the set of what could have appeared, rather than only
which rule and where.
[`docs/status.md`](../docs/status.md) is the list; where an example works around a gap,
it says so at that line.
