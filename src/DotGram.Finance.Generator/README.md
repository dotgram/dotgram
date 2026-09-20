# DotGram.Finance.Generator

Compiles a FIX dictionary into validation code inside your own build.

`DotGram.Finance` already carries FIX 4.4, and most consumers need nothing else: a message is asked
`message.Validate()` and answers with everything wrong with it. This package is for the other case
— a counterparty's dictionary, a version that is not 4.4, or a venue's own message types.

```xml
<PackageReference Include="DotGram.Finance" Version="0.2.0" />
<PackageReference Include="DotGram.Finance.Generator" Version="0.2.0" PrivateAssets="all" />

<ItemGroup>
  <AdditionalFiles Include="FIX44-venue.xml" />
</ItemGroup>
```

```csharp
using DotGram.Finance.Fix;

[FixDictionaryFile("FIX44-venue.xml")]
public static partial class VenueRules;
```

That is the whole of it. The class is given a `Validator`, ready to be asked:

```csharp
foreach (var finding in message.Validate(VenueRules.Validator))
    Console.WriteLine(finding);   // Body/453[1] tag 452 at 187: InvalidValue: ...
```

and an `Entries` table beside it, for a reader who wants two dictionaries in one validator:

```csharp
var both = new FixValidator();

both.Load(VenueRules.Entries);
both.Load(OtherRules.Entries);   // where they describe the same type, the second wins
```

## What it reads

QuickFIX's format: `<fix>` over `<header>`, `<trailer>`, `<messages>`, `<components>` and
`<fields>`. There is no DTD and no schema anywhere in the QuickFIX tree, so the format is what its
readers accept and they differ from one another; this one refuses what it would otherwise have to
guess at, and says which element and which line.

**No dictionary of anyone's ships in this package.** The file is yours, in your repository, read by
your compiler, and its licence obligations are yours with it. The path is fixed at build time and
is never read at run time.

## The two packages go in step

This fills tables that `DotGram.Finance` reads, so install the two at the same version. The
dictionary reader itself is one source compiled into both, so the reading cannot drift; the rest of
the surface can, and a generator a version behind the library is found in the worst place.

## Where to read further

`SKILL.md` beside this file, which is what an agent working in a project that has restored this
package should read. The library's own page covers parsing, the message model and validating
against FIX 4.4 without a dictionary at all.
