# DotGram.Web

The formats of the web, written in `.gram` against the specifications that define them.
Where an example shows one feature, a parser here is written against a whole specification.

It is an ordinary C# library. .Gram generates the parsers into this assembly at compile
time, so nothing here carries a parser runtime, and neither does anything that references
it.

## RFC 3986 URI parser

[`Rfc3986`](Rfc3986.cs) follows RFC 3986 closely, including absolute URIs, relative
references, IPv4, IPv6, `IPvFuture`, authority, paths, queries, fragments, and percent
encoding.

```csharp
using DotGram.Web;

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

## RFC 6570 URI Templates

[`Rfc6570`](Rfc6570.cs) reads a template once and expands it as often as there are values,
at all four levels of the RFC.

```csharp
using DotGram.Web;

var template = Rfc6570.ParseTemplate("/users{/id}{?fields,page:3}{&tags*}");

template.Expand(new Dictionary<string, object?>
{
    ["id"]     = "igor",
    ["fields"] = new[] { "name", "email" },
    ["page"]   = "12345",
    ["tags"]   = new[] { "a", "b" },
});
// /users/igor?fields=name,email&page=123&tags=a&tags=b
```

A value is a string, a list (`IEnumerable<string>`) or an associative array
(`IEnumerable<KeyValuePair<string, string>>`, a dictionary among them); a number is its
invariant text, and a missing or null value is undefined. `template.Parts` is what the
template was made of — literals, and expressions with their operator and variables — for a
caller that wants to know which variables a template asks for.

A template that does not follow the grammar is refused whole, including one using an
operator the RFC reserves. A prefix on a list or an associative array throws on
expansion, since the RFC gives it no meaning. It is held to
[the implementers' test suite](https://github.com/uri-templates/uritemplate-test).

## RFC 9651 Structured Field Values

[`Rfc9651`](Rfc9651.cs) reads the fields HTTP defines this way — `Priority`,
`Cache-Status`, `Proxy-Status`, signatures — as the three types the RFC gives them: an Item,
a List and a Dictionary.

```csharp
using DotGram.Web;

var list = Rfc9651.ParseList("text/html;q=1.0, (\"a\" \"b\");lvl=5");

var item = (Item)list[0];
item.Value;                    // BareItem.Token { Value = "text/html" }
item.Parameters["q"];          // BareItem.Decimal { Value = 1.0 }

var inner = (InnerList)list[1];
inner.Items.Count;             // 2
inner.Parameters["lvl"];       // BareItem.Integer { Value = 5 }

var dictionary = Rfc9651.ParseDictionary("u=3, i");
dictionary["i"];               // Item { Value = BareItem.Boolean { Value = true } }
```

Parameters and Dictionaries are read by position and by key, in the order they were
written; a key written twice keeps its first place and its last value. A field that
arrives in several lines is one value: `Rfc9651.Combine(lines)` joins them as §4.2 says.
Every `Parse…` has a `TryParse…` beside it, and a field that does not parse is refused
whole — the RFC allows nothing else.

It is held to [the HTTP working group's test suite](https://github.com/httpwg/structured-field-tests).

## Taking it

```xml
<PackageReference Include="DotGram.Web" Version="0.1.0" />
```

There is no companion runtime package, and no generator to install alongside it: the
parsers were generated when this assembly was compiled.
