# DotGram.Parsers

Parsers for real formats, written in `.gram`. Where an example shows one feature, a parser
here is written against a whole specification.

They are ordinary C# libraries. .Gram generates the parser into this assembly at compile
time, so nothing here carries a parser runtime, and neither does anything that references
it.

## RFC 3986 URI parser

[`Rfc3986`](Uri/Rfc3986.cs) follows RFC 3986 closely, including absolute URIs, relative
references, IPv4, IPv6, `IPvFuture`, authority, paths, queries, fragments, and percent
encoding.

```csharp
using DotGram.Parsers.Uri;

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

## SQL

The SQL parsers are a package of their own now,
[`DotGram.Sql`](https://github.com/dotgram/dotgram/tree/main/src/DotGram.Sql).

## Taking one

```xml
<PackageReference Include="DotGram.Parsers" Version="0.1.0" />
```

There is no companion runtime package, and no generator to install alongside it: the
parsers were generated when this assembly was compiled.
