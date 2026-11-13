# The host class and its attributes

Every parser is generated into a class of yours, the **host**. This page covers what the host has
to be, the attributes that attach a grammar to it — `[Gram]`, `[GramParserVariant]`, `[GramInclude]` — the
options they take, the attributes for editors, and the names of what comes out. What the generated
methods take and answer is the [parser API](../../syntax.md#6-publication); the notation is the
[language guide](../language/README.md).

All the attributes live in the namespace `DotGram` and are written into your project by the
generator itself ([setup](01-setup.md#what-the-generator-adds-to-every-compilation)): `using DotGram;`
is all a file needs.

## What the host has to be

The host is a **class declared `partial`**: the generated part of it is the other half. A nested
host needs every class around it to be `partial` as well. Forgetting it is the first error most
people meet:

```csharp example=not-partial GRAM0002
using DotGram;

[Gram("""
	Digits = ['0'..'9']+

	parse Digits
	""")]
public static class Numbers { }
```

*'Numbers' is marked [Gram] but is not partial; the generated parser has nowhere to go.*

Beyond that the class is yours:

- **`static` or not.** The generated methods are static either way. A `static` host says it is
  nothing but a parser. Without `static` the class can have instances and instance members, and it
  can be the grammar's own `context`: the object a caller hands to every parse and the grammar's
  guards and constructions write into ([language guide](../../syntax.md#77-what-a-parse-works-out-and-the-api-has-nowhere-to-keep)).
- **Accessibility.** `public` or `internal`, as the class says; the generated methods are public
  members of it unless the grammar says otherwise. Whether the host can be named from outside its
  assembly also decides whether its grammar can be included there (`ExportGrammar`, below).
- **Nested, generic.** A host may be nested in other partial classes, and may be generic. A generic
  host `Parser<T>` is looked up by its name without type parameters: `[Gram]` alone on it reads
  `Parser.gram`.

A host that is its own context:

```csharp example=context-host
using DotGram;

namespace Demo;

[Gram("""
	context : @Letters

	Word   = Letter+
	Letter = ['a'..'z'] & when @(context.Seen())

	parse Word
	""")]
public sealed partial class Letters
{
	public int Count { get; private set; }

	public static int CountIn(string text)
	{
		var letters = new Letters();

		ParseWord(text, letters);

		return letters.Count;
	}

	bool Seen()
	{
		Count++;

		return true;
	}
}
```

```text run example=context-host
Letters.CountIn("abc") → 3
```

## `[Gram]`: which grammar

`[Gram]` says which grammar the class is compiled from. It is written once on a class.

| Form | The grammar |
| --- | --- |
| `[Gram]` | the `.gram` file named after the class: `Amount.gram` for `Amount` |
| `[Gram("Grammars/Pair.gram")]` | the file at that path, relative to the project |
| `[Gram("""…""")]` | the text itself |

A single-line argument ending in `.gram` is a path; anything else is grammar text. A file has to be
given to the compiler as an additional file; [grammar files](03-grammar-files.md) has how they are
found and what goes wrong.

```csharp example=forms
using DotGram;

namespace Demo;

// The grammar is the .gram file named after the class.
[Gram]
public static partial class Amount;

// The grammar is the file the path names.
[Gram("Grammars/Pair.gram")]
public static partial class Settings;

// The grammar is the text itself.
[Gram("""
	Hex = ['0'..'9' | 'a'..'f' | 'A'..'F']+

	parse Hex
	""")]
public static partial class HexDigits;
```

```dotgram file=Amount.gram example=forms
@using System.Globalization;

Amount : @decimal = text: Number => @(decimal.Parse(text, CultureInfo.InvariantCulture))
Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?

parse Amount
```

```dotgram file=Grammars/Pair.gram example=forms
Pair = key: Word & '=' & value: Word

Word = ['a'..'z' | '0'..'9']+

parse Pair
```

```text run example=forms
Amount.ParseAmount("12.50") → 12.50
Settings.ParsePair("mode=fast").Key → "mode"
Settings.ParsePair("mode=fast").Value → "fast"
HexDigits.TryParseHex("cafe").IsSuccess → true
```

## The options of a reading

`[Gram]` also says how the grammar is read — compiled into a parser — through named arguments. Every
option has a default, and a grammar that says none gets a parser that is right for it; the options
are for what the defaults cannot know. They change how the parser works, not what it accepts,
except where the table says so.

| Option | Default | What it does |
| --- | --- | --- |
| `Lexical` | `false` | read the input as tokens: the grammar is cut into a lexer and a syntax over its tokens. A request: a grammar that cannot be cut is read over characters and told why (`GRAM5004`). |
| `Carrier` | `GramCarrier.Auto` | when the `=>` constructions run: `Immediate`, as each alternative is read, or `Deferred`, once the parse has accepted. `Auto` takes `Immediate` wherever it can and says where `Deferred` would be safer (`GRAM5016`, `GRAM5012`). |
| `ConstructionsMayThrow` | `false` | a construction may throw on a reading the parse gives up; the immediate carrier then holds what it throws until it is sure. |
| `BufferedText` | `false` | also publish each parse over a `TextReader`, read through a bounded buffer. |
| `BufferedBytes` | `false` | also publish each parse over a `Stream` and bytes, read through a buffer, with no text decoding. |
| `UseSpanCaptures` | `false` | hand character captures to C# as `ReadOnlySpan<char>` rather than `string`. |
| `InitialBufferSize` | `4096` | the capacity a buffered parse starts with, where a call does not say. At least 1 (`GRAM0009`). |
| `MaxBufferedInputLength` | `int.MaxValue`, no limit | the most input, in characters or bytes, a buffered parse holds at once, where a call does not say; past it the parse throws `IOException`. At least 1 (`GRAM0009`). |
| `MaxExtraStacks` | `0`, no limit | how many extra stacks one parse may take when input nests deeper than the thread's stack allows; past it the parse throws `InsufficientExecutionStackException`. |
| `TargetMethodPartSize` | `0`, the measured default | how large the parts of a recognizer too big for one method are aimed to be. A wish: no value fails a build. |
| `LocationType` | none | an interface (or a class) the grammar's values implement to be told where in the input they were read. |
| `Trace` | `false` | a trace build: the parser reports what it reads to a sink, for finding out why an input is refused. Changes no answer. |
| `ExportGrammar` | not set: exported when the class and every class around it are public | carry the grammar's text on the class, so that a grammar in another assembly can include it. Written as `true`, it also marks a grammar that publishes nothing as a library (`GRAM4059`). |
| `Name` | none | `[Gram]` only: compile the first reading into a nested class of that name too ([`[GramParserVariant]`](#gramparservariant-another-reading-of-the-same-grammar)). |
| `DefaultAlias` | the class's name | `[Gram]` only: the name a grammar that includes this one reaches it under ([including](#graminclude-one-grammar-built-on-another)). |

The full semantics of each are in [`syntax.md` §6.3–§6.9](../../syntax.md#68-generation-options).

**Names from before 0.3.0.** Several options were renamed in 0.3.0, with no old spelling kept:
`IncludedAs` is `DefaultAlias`, `Suffix` is `Name`, `[GramOptions(Suffix = "X")]` is
`[GramParserVariant("X")]`, `GramCarrier.Tape` is `GramCarrier.Deferred`, and the rest as the release
notes list them. A host still written with an old name is told what it is called now:

```csharp example=old-name GRAM0012
using DotGram;

[Gram("""
	Letter = ['a'..'z']

	parse Letter
	""", IncludedAs = "Lex")]
public static partial class Lexemes;
```

*'IncludedAs' was renamed to 'DefaultAlias' in 0.3.0.*

### Reading from a `TextReader`

`BufferedText` adds an overload over a `TextReader` beside the one over a string. A file of any
size is read through a buffer of `InitialBufferSize` that grows only as far as one record needs:

```csharp example=buffered
using DotGram;

namespace Demo;

[Gram("""
	Lines = Line*
	Line  = [^ '\n']* & '\n'

	parse Lines
	""", BufferedText = true, InitialBufferSize = 1024)]
public static partial class LineReader;
```

```text run example=buffered
LineReader.TryParseLines(new System.IO.StringReader("one\ntwo\n")).IsSuccess → true
```

`InitialBufferSize` and `MaxBufferedInputLength` are only defaults: each buffered overload also
takes its own `bufferSize` and `maxRetained`. A value below 1 is refused where it is written:

```csharp example=buffer-zero GRAM0009
using DotGram;

[Gram("""
	Digits = ['0'..'9']+

	parse Digits
	""", BufferedText = true, InitialBufferSize = 0)]
public static partial class Numbers;
```

*InitialBufferSize = 0 reads no input; it has to be at least 1.*

### Where a value was read: `LocationType`

Name an interface with a method `void Locate(int at, int length)`, and every rule whose value
implements it is told the range of input it was built from, without the trivia around it. A base
class that declares `Locate` may be named instead of an interface:

```csharp example=located
using DotGram;

namespace Demo;

public interface ILocated
{
	void Locate(int at, int length);
}

public sealed class Word : ILocated
{
	public required string Text { get; init; }

	public int Start { get; private set; }

	public int Length { get; private set; }

	public void Locate(int at, int length)
	{
		Start  = at;
		Length = length;
	}
}

[Gram("""
	trivia = { ' '* }

	Words : @Word[] = items: Item+ => @(items)
	Item  : @Word   = text: { ['a'..'z']+ } => @(new Word { Text = text })

	parse Words
	""", LocationType = typeof(ILocated))]
public static partial class Located;
```

```text run example=located
string.Join(", ", Located.ParseWords("alpha  beta").Select(w => $"{w.Text}@{w.Start}+{w.Length}")) → "alpha@0+5, beta@7+4"
```

A type that declares no such `Locate` is refused where it is named (`GRAM0011`), rather than as an
error in the generated code:

```csharp example=no-locate GRAM0011
using DotGram;

namespace Demo;

public interface ISpanned
{
	int Start { get; set; }
}

[Gram("""
	Word = ['a'..'z']+

	parse Word
	""", LocationType = typeof(ISpanned))]
public static partial class Words;
```

*'Words' names 'Demo.ISpanned' as its LocationType, which is neither an interface nor a class declaring
void Locate(int at, int length).*

A grammar that names no type pays nothing for this. Read from a buffered
input, a place past `int.MaxValue` characters cannot be told, and the parse throws
`InvalidOperationException` where it would tell one.

### Why an input was refused: `Trace`

`Trace = true` makes a trace build of the reading. Its class then also holds a sink to set with
`Tracing(sink)` and three ready-made ones: `GramWhy`, which says why the last call was refused and by
which rules; `GramTraceLog`, an indented log of every rule entered; `GramProfile`, how often each
rule was entered and how long it took. The `DotGramTrace` build property asks the same of every
grammar of a project ([MSBuild properties](04-msbuild.md)).

```csharp example=traced
using DotGram;

namespace Demo;

[Gram("""
	Value : @int = '(' & inner: Value & ')' => @(inner) | digits: ['0'..'9']+ => @(int.Parse(digits))

	parse Value
	""", Trace = true)]
public static partial class Nested
{
	public static string Why(string text)
	{
		var why = new GramWhy();

		using (Tracing(why))
			TryParseValue(text);

		return why.ToString().Split('\n')[0].TrimEnd();
	}
}
```

```text run example=traced
Nested.Why("((1") → "Expected ')'. (line 1, column 4)"
```

The whole of what `GramWhy` says for that input:

```text
Expected ')'. (line 1, column 4)
The rules below were reading there, each wanting what it names.
  wanted ')' in Value 1:1 > Value 1:2
```

## `[GramParserVariant]`: another reading of the same grammar

A class has one grammar, and may have it read in more than one way: as tokens and as characters, on
both carriers, traced and untraced. Each further reading is a `[GramParserVariant("Name")]`,
compiled into a nested class of that name — the one argument it requires. What a variant does not
say it takes from the `[Gram]`:

```csharp example=variant
using DotGram;

namespace Demo;

[Gram("""
	trivia = { ' '* }

	Sum  : @int = left: Sum & '+' & right: Term << 1 => @(left + right)
	            | value: Term                       => @(value)
	Term : @int = digits: { ['0'..'9']+ } => @(int.Parse(digits))

	parse Sum
	""")]
[GramParserVariant("Traced", Trace = true)]
public static partial class Adder;
```

```text run example=variant
Adder.ParseSum("1 + 2 + 3") → 6
Adder.Traced.ParseSum("1 + 2 + 3") → 6
```

- `Adder.ParseSum` is the `[Gram]`'s reading, `Adder.Traced.ParseSum` the variant's: the same grammar,
  with everything the `[Gram]` says, and a trace build.
- Each reading is a compilation of its own, and gets a file of its own: `Demo.Adder.g.cs`,
  `Demo.Adder.Traced.g.cs`. The types the readings share in their signatures — `Match<T>`,
  `SourceSpan`, `ParserInput<T>` — are written once, in the host, in `Demo.Adder.gram-shared.g.cs`.
- The name is one C# identifier (`GRAM0007`), and two readings cannot share one (`GRAM0006`).
- `[Gram(Name = "…")]` puts the first reading into a nested class too; it then needs a name no
  variant takes.

```csharp example=variant-same-name GRAM0006
using DotGram;

[Gram("""
	Digits = ['0'..'9']+

	parse Digits
	""", Name = "Fast")]
[GramParserVariant("Fast", Lexical = true)]
public static partial class Numbers;
```

*'Numbers' asks for two readings of its grammar under the same Name; give one of them a Name of its
own.*

**`UsePerCallLocations`**, an option of `[GramParserVariant]` alone, makes a variant that is not a
second parser: with a `LocationType`, the grammar is compiled once, and the nested class holds
the same methods with locations asked for per call. The interface's method then takes a third
argument, `Locate(int at, int length, int gapStart)`
([`syntax.md`](../../syntax.md#usepercalllocations)).

## `[GramInclude]`: one grammar built on another

`[GramInclude(typeof(X))]` brings the rules of `X`'s grammar into this one, inside a grammar
namespace named after `X`, so that `using X;` and `X.Rule` reach them. Write it as many times as
there are grammars to build on. `X`'s `DefaultAlias` renames that namespace for everybody who
includes it, and the includer's `Alias` renames it for this grammar alone:

```csharp example=include
using DotGram;

namespace Demo;

// A library of rules, published nowhere, reached as Lex.
[Gram("""
	Letter = ['a'..'z' | 'A'..'Z']
	Digit  = ['0'..'9']
	Ident  = Letter & (Letter | Digit)*
	""", DefaultAlias = "Lex")]
public static partial class Lexemes;

[Gram("""
	using Lex;

	Assignment = name: Ident & '=' & value: Lex.Digit+

	parse Assignment
	""")]
[GramInclude(typeof(Lexemes))]
public static partial class Assignments;

[Gram("""
	Label = name: Words.Ident & ':' & value: Words.Digit+

	parse Label
	""")]
[GramInclude(typeof(Lexemes), Alias = "Words")]
public static partial class Labels;
```

```text run example=include
Assignments.ParseAssignment("x1=42").Name → "x1"
Assignments.ParseAssignment("x1=42").Value → "42"
Labels.ParseLabel("port:8080").Name → "port"
```

- The static members of the included class come with its rules, so its `@` calls go on working.
- Two includes under one name are one namespace, and their rules collide: `GRAM0008` says so, and an
  `Alias` for one of them is the cure. An alias is one identifier (`GRAM0005`).
- **Across assemblies**, the grammar travels on the class: a host whose `ExportGrammar` is on — by
  default, a public host — carries its text in a `[GramSource]` the generator writes, and
  `[GramInclude(typeof(X))]` in another project reads it from there. An internal host's grammar
  cannot be included outside its assembly unless it says `ExportGrammar = true`.
- A grammar may also be inherited by deriving from the class that holds it. That is the older
  spelling, and it says less: a class has one base and many attributes.

## Attributes for editors

Two attributes describe a grammar's language to editors. They change nothing the parser reads or
builds.

| Attribute | What it says |
| --- | --- |
| `[GramLanguage(id, Extensions = …)]` | a stable identifier for the language the grammar describes — a reverse domain name is the usual shape — and the file extensions it claims, with the dot |
| `[GramClassify(target, role)]` | a role from `GramClassification` (`Keyword`, `Identifier`, `Type`, `Variable`, `Function`, `Method`, `Property`, `Number`, `String`, `Comment`, `Operator`, `Punctuation`, `Namespace`, `Parameter`, `Label`) for a rule (`"Keyword"`) or one of its captures (`"Declaration.type"`); a capture's role overrides its rule's |

```csharp example=editor
using DotGram;

namespace Demo;

[Gram("""
	Query   = Keyword & ' ' & Field
	Keyword = "select" | "where"
	Field   = ['a'..'z']+

	parse Query
	""")]
[GramLanguage("com.example.query", Extensions = [".query"])]
[GramClassify("Keyword", GramClassification.Keyword)]
[GramClassify("Field", GramClassification.Property)]
public static partial class QueryLanguage;
```

```text run example=editor
QueryLanguage.TryParseQuery("select name").IsSuccess → true
```

With the DotGram extension, a string parameter marked `[StringSyntax("com.example.query")]` is then
coloured as that language wherever a literal is passed to it ([`visual-studio.md`](../../visual-studio.md)).
A target that names no rule or capture is a warning (`GRAM6001`–`GRAM6005`).

The generator declares four more attributes that are not for writing by hand, and hides them from
completion: `[GramSource]` and `[GramLanguageDescriptor]`, which it writes on the classes it compiles,
and `[GramToolingGuard]` and `[GramToolingExternal]`, which are experimental and may change in any
release.

## What comes out

For a host `Demo.Adder` with one `[GramParserVariant("Traced")]`, the project gains:

| File | What it holds |
| --- | --- |
| `DotGram.Attributes.g.cs` | the attributes, once per project |
| `Demo.Adder.g.cs` | the `[Gram]`'s reading: the published methods and everything they need |
| `Demo.Adder.Traced.g.cs` | the variant's reading, in the nested class `Adder.Traced` |
| `Demo.Adder.gram-shared.g.cs` | the types both readings name, written once in `Adder` |

A nested host is named with its classes (`Demo.Formats.HexDigits.g.cs`), a generic one with its type
parameters between underscores (`Demo.Parser_T_.g.cs`). Every type and member the generated class
declares for its own use has `_DotGram` in its name, which no name a grammar writes may contain
(`GRAM1012`), so nothing generated collides with what the host declares.

