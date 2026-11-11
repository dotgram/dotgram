# Compatibility

What a project targeting an older framework or language version needs, in full. The repository
[README](../README.md#compatibility) has the short form.

The generated parser is C# 8 and targets whatever the project around it targets.
[`tests/DotGram.Compatibility`](../tests/DotGram.Compatibility) builds one grammar for
`netstandard2.0`, `net472` and `net8.0` at C# 8 on every build.

One thing is written differently above the floor: where the project's C# is 11 or later, a
lexical grammar's transition table is a UTF-8 literal rather than an array initializer of
every cell, which the compiler reads as one token. The parser it makes is the same.

One thing costs more below C# 11: the table of what each refusal expected is then an ordinary
string rather than a UTF-8 literal, and an ordinary string goes into the assembly's user-string
heap, which holds about 16 MB in all. For the T-SQL grammar that is about 600 KB per parser
class; several classes with different tables add up, while identical ones are stored once. An
assembly compiled at C# 8 to 10 that already holds close to 16 MB of strings fails to compile
(CS8103). From C# 11 the table takes none of that heap.

Three things an older target has to say:

* **A grammar written inside `[Gram]` is a raw string literal, which is C# 11.** On
  `netstandard2.0` and `net472` the default is C# 7.3, so `<LangVersion>` has to be set.
  A grammar in a `.gram` file still needs C# 8: below it the build reports GRAM0010 and
  generates no parser, so set `<LangVersion>8.0</LangVersion>` or later.
* **`netstandard2.0` and `net472` need `System.Memory`**, for the `ReadOnlySpan<char>`
  the generated methods take.
* **A `record` on `netstandard2.0` or `net472` needs `System.Runtime.CompilerServices.IsExternalInit`**, a type
  those frameworks lack (CS0518). Declare it once in the project:
  `namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }`.

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net472' Or '$(TargetFramework)' == 'netstandard2.0'">
  <LangVersion>11.0</LangVersion>
</PropertyGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net472' Or '$(TargetFramework)' == 'netstandard2.0'">
  <PackageReference Include="System.Memory" Version="4.6.3" />
</ItemGroup>
```

The generator is a Roslyn analyzer built against `Microsoft.CodeAnalysis` 4.14 and needs a
compiler at least that new.
