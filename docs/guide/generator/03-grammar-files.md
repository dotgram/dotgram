# Grammar files

A grammar can be written into its `[Gram]` attribute or kept in a `.gram` file of its own. This page
covers the files: how the build is given them, how a host finds its file, what a file holds and
what it does not, and when a file is the better place.

## Giving the build its grammar files

The generator sees only the files the compiler is given as **additional files**. A project lists
its grammars the way it lists any other:

```xml
<ItemGroup>
  <AdditionalFiles Include="Grammars/**/*.gram" />
</ItemGroup>
```

A file that is not listed is not there as far as the generator is concerned. The error names the
file it looked for and the line to add:

```csharp example=not-listed GRAM0003
using DotGram;

[Gram]
public static partial class Feed;
```

*No additional file 'Feed.gram' for 'Feed'; add `<AdditionalFiles Include="Feed.gram" />` to the
project.*

Listing a file that no class claims is harmless: it generates nothing. A parser is generated for a
class, never for a file.

## How a host finds its file

| The attribute | The file it reads |
| --- | --- |
| `[Gram]` on `Feed` | an additional file named `Feed.gram`, in any folder |
| `[Gram]` on `Outer.Parser<T>` | `Parser.gram`: the innermost class, without its type parameters |
| `[Gram("feeds/Feed.gram")]` | an additional file whose path ends with `feeds/Feed.gram` |

The path in the attribute is matched against the end of each additional file's full path, at a
folder boundary, so `[Gram("feeds/Feed.gram")]` finds `src/feeds/Feed.gram` and not
`src/oldfeeds/Feed.gram`. `/` and `\` are the same, and case is ignored on every operating system,
so a project finds the same file on Windows, macOS and Linux.

**Exactly one file has to match.** Where two do, the generator does not pick one by order; it says
which, and the cure is a path that names one of them:

```csharp example=ambiguous GRAM0004
using DotGram;

[Gram]
public static partial class Feed;
```

```dotgram file=current/Feed.gram example=ambiguous
Line = ['a'..'z']+

parse Line
```

```dotgram file=archive/Feed.gram example=ambiguous
Line = ['A'..'Z']+

parse Line
```

*'Feed.gram' matches several additional files (/project/current/Feed.gram,
/project/archive/Feed.gram); give [Gram] a path that names one of them.*

```csharp example=disambiguated
using DotGram;

namespace Demo;

[Gram("current/Feed.gram")]
public static partial class Feed;
```

```dotgram file=current/Feed.gram example=disambiguated
Line = ['a'..'z']+

parse Line
```

```dotgram file=archive/Feed.gram example=disambiguated
Line = ['A'..'Z']+

parse Line
```

```text run example=disambiguated
Feed.TryParseLine("abc").IsSuccess → true
Feed.TryParseLine("ABC").IsSuccess → false
```

The same holds for two files whose names differ only in case: they are both candidates, and the path
decides.

## What a file holds

A `.gram` file holds the grammar and nothing else: exactly the text that would otherwise be the
argument of `[Gram("""…""")]`. Its C# imports are the grammar's own `@using` lines; the C# file the
host is declared in lends it nothing.

```dotgram fragment
@using System.Globalization;

Amount : @decimal = text: Number => @(decimal.Parse(text, CultureInfo.InvariantCulture))
Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?

parse Amount
```

What a file does **not** decide:

- **The namespace and the class.** The generated parser goes into the host, in the host's namespace;
  the file's name and folder play no part. `Grammars/Amount.gram` read by `Demo.Amount` generates
  `Demo.Amount.ParseAmount`.
- **The options.** How the grammar is read — `Lexical`, `Carrier`, everything of
  [the previous page](02-hosts-and-attributes.md#the-options-of-a-reading) — is said on the
  attribute, so one file can be read by two hosts in two ways, and each gets a parser of its own.

The standard library needs no file: `using Std;` in any grammar brings in the rules the generator
carries with it (`Std.Digits`, `Std.Spacing`, …), listed in [`syntax.md` §5.2](../../syntax.md#52-the-standard-library-std).

## Where diagnostics point

A message about a grammar in a file points into the file, at the line and column the grammar's
own text has: the Error List opens the `.gram` file there. A message about a grammar written into
the attribute points into the string literal, at the same character. A grammar included from another
class is reported where it is written, in its own file.

A file's messages are filtered like any other: `NoWarn`, `WarningsAsErrors` and a global analyzer
configuration reach them. An `.editorconfig` section for `*.gram` does not, since the compiler applies
those sections only to C# files; a `#pragma` cannot stand in a grammar file either. The
[diagnostics](07-diagnostics.md) page has the details.

## Files of other projects

A grammar included from a class in another project is not read from that project's `.gram` file,
which this project's build is not given: it is read from the class, which carries its grammar's text
when it is exported ([including](02-hosts-and-attributes.md#graminclude-one-grammar-built-on-another)).
A `.gram` file of this project that happens to have the same name changes nothing: the referenced
assembly's grammar is the one compiled and shipped with it, and the build says so where the two
differ (`GRAM0017`, information). A file is preferred only for a class compiled in this same
project, where it is the text being edited.

## A file or the attribute

| | In the attribute | In a `.gram` file |
| --- | --- | --- |
| C# version | 11 (raw string literal) | 8 |
| Listing in the project | none | `AdditionalFiles` |
| Grammar beside the C# it calls | yes, in one file | no, in two |
| Editor support | in the string, with the DotGram extension | in its own editor, with the extension |
| Size that stays readable | tens of rules | any |

The two are the same grammar: moving one from the attribute into a file changes nothing about the
parser. A file wants line endings that do not depend on the machine it was checked out on, since
the generator reads it as it is; `*.gram text eol=lf` in `.gitattributes` does that.

