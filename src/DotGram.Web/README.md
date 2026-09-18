<!--
  Agents: the skill for this package is SKILL.md, beside this file in the package
  directory — when to use it, the contract every parser shares, and what is easy to
  get wrong. Read it before writing code against the package. In a restored package
  that is ~/.nuget/packages/dotgram.web/<version>/SKILL.md.
-->

# DotGram.Web

[![NuGet](https://img.shields.io/nuget/v/DotGram.Web?logo=nuget)](https://www.nuget.org/packages/DotGram.Web)
[![build](https://img.shields.io/github/actions/workflow/status/dotgram/dotgram/build.yml?branch=main&label=build)](https://github.com/dotgram/dotgram/actions/workflows/build.yml)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet)](https://github.com/dotgram/dotgram#compatibility)
[![License: MIT](https://img.shields.io/github/license/dotgram/dotgram)](https://github.com/dotgram/dotgram/blob/main/LICENSE)

Parsers for the formats of the web — JSON, HTTP header fields, URIs, email addresses,
timestamps and language tags — each written against its specification and held to the test
suite written for it.

The parsers were generated into this assembly by [.Gram](https://github.com/dotgram/dotgram)
when it was compiled, so there is no parser runtime behind them. The one dependency is
`System.Memory`, on `netstandard2.0` only; on `net10.0` there is none.

```
dotnet add package DotGram.Web
```

## What it reads

| Format | Read with | Specification | Held to |
| --- | --- | --- | --- |
| **JSON** | | | |
| JSON | `JsonValue.Parse` | [RFC 8259](https://www.rfc-editor.org/rfc/rfc8259) | [JSONTestSuite](https://github.com/nst/JSONTestSuite) |
| JSON Pointer | `JsonPointer.Parse`, `JsonPointer.ParseFragment` | [RFC 6901](https://www.rfc-editor.org/rfc/rfc6901) | the RFC's examples |
| JSON Patch | `JsonPatch.Parse`, `JsonPatch.Read` | [RFC 6902](https://www.rfc-editor.org/rfc/rfc6902) | [json-patch-tests](https://github.com/json-patch/json-patch-tests) |
| **HTTP** | | | |
| `Content-Type` | `MediaType.Parse` | [RFC 9110 §8.3](https://www.rfc-editor.org/rfc/rfc9110#section-8.3) | the IANA media types registry |
| `Accept` | `MediaRange.ParseAccept` | [RFC 9110 §12.5.1](https://www.rfc-editor.org/rfc/rfc9110#section-12.5.1) | the RFC's examples |
| Structured Fields | `StructuredField.ParseItem`, `ParseList`, `ParseDictionary` | [RFC 9651](https://www.rfc-editor.org/rfc/rfc9651) | [structured-field-tests](https://github.com/httpwg/structured-field-tests) |
| `Link` | `WebLink.ParseField` | [RFC 8288](https://www.rfc-editor.org/rfc/rfc8288) | the RFC's examples |
| `Content-Disposition` | `ContentDisposition.Parse` | [RFC 6266](https://www.rfc-editor.org/rfc/rfc6266) | [tc2231](http://test.greenbytes.de/tech/tc2231/) |
| `Set-Cookie`, `Cookie` | `SetCookie.Parse`, `CookiePair.ParseField`, `CookieDate.Parse` | [RFC 6265](https://www.rfc-editor.org/rfc/rfc6265) | the httpstate working group's cases |
| `Forwarded` | `ForwardedElement.ParseField`, `ForwardedNode.Parse` | [RFC 7239](https://www.rfc-editor.org/rfc/rfc7239) | the RFC's examples |
| **Addresses** | | | |
| URI | `UriReference.Parse`, `UriReference.ParseUri` | [RFC 3986](https://www.rfc-editor.org/rfc/rfc3986) | the RFC's examples |
| URI Template | `UriTemplate.Parse` | [RFC 6570](https://www.rfc-editor.org/rfc/rfc6570) | [uritemplate-test](https://github.com/uri-templates/uritemplate-test) |
| Email address | `AddrSpec.Parse`, `EmailAddress.ParseList` | [RFC 5322](https://www.rfc-editor.org/rfc/rfc5322) | [is_email](https://github.com/dominicsayers/isemail) |
| **Time and language** | | | |
| Timestamp | `Timestamp.Parse`, `FullDate.Parse`, `FullTime.Parse` | [RFC 3339](https://www.rfc-editor.org/rfc/rfc3339) | [JSON Schema test suite](https://github.com/json-schema-org/JSON-Schema-Test-Suite) |
| Language tag | `LanguageTag.Parse` | [RFC 5646](https://www.rfc-editor.org/rfc/rfc5646) (BCP 47) | the IANA language subtag registry |

Every `Parse` throws `FormatException`, its message naming where the text stopped fitting;
every `TryParse` answers `false` instead. A value that parses is a record: equal to another
that means the same, and written back by `ToString` in the form the specification generates.

## JSON

```csharp
using DotGram.Web;

var value = (JsonValue.Object)JsonValue.Parse("""{ "pi": 3.14159265358979323846, "tags": ["a", "b"] }""");

var pi = (JsonValue.Number)value.Members[0].Value;
pi.Text;                  // 3.14159265358979323846
pi.ToDouble();            // 3.141592653589793
value.ToString();         // {"pi":3.14159265358979323846,"tags":["a","b"]}
```

A number keeps the text it was written with, and `ToDouble`, `TryToDecimal` and `TryToInt64`
read it at the precision wanted. An object keeps its members in order, a name written twice
included. Nesting is read as deep as the input goes. The text is characters: decoding bytes,
as UTF-8, is the caller's, and a byte order mark is refused rather than skipped.

### JSON Pointer

```csharp
var document = JsonValue.Parse("""{ "foo": ["bar", "baz"], "a/b": 1 }""");

JsonPointer.Parse("/foo/1").Resolve(document);   // "baz"
JsonPointer.Parse("/a~1b").Resolve(document);    // 1
JsonPointer.Parse("/foo/-").Resolve(document);   // null: the element after the last

var pointer = JsonPointer.Parse("/a~1b/0");
pointer.Tokens;                                  // [a/b, 0]
pointer.ToUriFragment();                         // #/a~1b/0
JsonPointer.ParseFragment("#/c%25d").Tokens;     // [c%d]
```

Escapes are undone in the order the RFC gives, so `~01` is `~1`. `Resolve` answers null where
the pointer refers to nothing — a missing member, an index past the end or with a leading
zero, `-`, or a name an object holds twice — which is not JSON's `null`.

### JSON Patch

```csharp
var patch = JsonPatch.Parse("""
    [
      { "op": "test", "path": "/a/b/c", "value": "foo" },
      { "op": "replace", "path": "/a/b/c", "value": 42 },
      { "op": "copy", "from": "/a/b/c", "path": "/a/b/d" }
    ]
    """);

patch.Apply(JsonValue.Parse("""{ "a": { "b": { "c": "foo" } } }""")).ToString();
// {"a":{"b":{"c":42,"d":42}}}
```

Applying changes nothing it is given: the result is a new document sharing what the patch did
not touch, and a patch that fails throws `JsonPatchException` — or `TryApply` answers false —
with no half-patched document left behind. `test` compares numbers by value at any precision
and objects whatever the order of their members; `JsonPatch.AreEqual` is that comparison.
Removing the whole document is an error.

## HTTP header fields

### Content-Type and Accept

```csharp
var type = MediaType.Parse("Text/HTML; Charset=\"UTF-8\"");

type.Charset;                                        // UTF-8
type == MediaType.Parse("text/html;charset=utf-8");  // true

var accept = MediaRange.ParseAccept("text/*;q=0.3, text/plain;q=0.7, */*;q=0.5");

MediaRange.Quality(accept, MediaType.Parse("text/plain"));   // 0.7
MediaRange.Quality(accept, MediaType.Parse("text/html"));    // 0.3
MediaRange.Quality(accept, MediaType.Parse("image/png"));    // 0.5
```

A type, a subtype, a parameter name and a `charset` value compare without case; other values
compare as written. Empty parameters and empty list elements are accepted, as RFC 9110 asks of
a recipient. A parameter named `q` is the weight wherever it stands, and has to be a qvalue.
`Quality` takes the weight of the most specific matching range, or 0 where none matches.

### Structured Field Values

For the fields HTTP defines this way — `Priority`, `Cache-Status`, `Proxy-Status`, signatures —
as the three types RFC 9651 gives them.

```csharp
var list = StructuredField.ParseList("text/html;q=1.0, (\"a\" \"b\");lvl=5");

var item = (Item)list[0];
item.Value;                    // BareItem.Token { Value = "text/html" }
item.Parameters["q"];          // BareItem.Decimal { Value = 1.0 }

var dictionary = StructuredField.ParseDictionary("u=3, i");
dictionary["i"];               // Item { Value = BareItem.Boolean { Value = true } }

StructuredField.SerializeDictionary(dictionary);   // u=3, i
```

Parameters and Dictionaries are read by position and by key; a key written twice keeps its
first place and its last value. A field in several lines is one value:
`StructuredField.Combine(lines)` joins them. A field that does not parse is refused whole.
`SerializeItem`, `SerializeList` and `SerializeDictionary` write the canonical form, and throw
`ArgumentException` for what has none — a key with an uppercase letter, a String outside
printable ASCII, an Integer of sixteen digits.

### Link

```csharp
var links = WebLink.ParseField(
    "</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel, " +
    "</TheBook/chapter4>; rel=\"next\"; title=\"next chapter\"");

links[0].Target;       // /TheBook/chapter2
links[0].Relations;    // [previous]
links[0].Title;        // letztes Kapitel
links[1].Title;        // next chapter
```

A link-param's value is the same written as a token or a quoted string. Where RFC 8288 allows a
parameter once — `rel`, `title`, `title*`, `media`, `type` — the first counts, and `Title`
prefers a `title*` that decodes, from UTF-8 or ISO-8859-1. A target and an anchor are kept as
written: resolving a relative one needs the URL of the response that carried the field.

### Content-Disposition

```csharp
var field = ContentDisposition.Parse(
    "attachment; filename=\"EURO rates\"; filename*=utf-8''%e2%82%ac%20rates");

field.IsAttachment;               // true
field.Filename;                   // € rates
field.Find("filename")!.Value;    // EURO rates
```

`Filename` prefers a `filename*` that decodes to `filename`. A type other than `inline` is an
attachment. A field the grammar does not make, a parameter named twice included, is refused
whole. The filename is what the sender wrote; stripping its path and making it safe to save is
the caller's.

### Cookies

```csharp
var cookie = SetCookie.Parse("SID=31d4d96e407aad42; Path=/; Max-Age=3600; Secure; HttpOnly");

cookie.Name;                                // SID
cookie.Path;                                // /
cookie.Secure;                              // true
cookie.ExpiryTime(DateTimeOffset.UtcNow);   // an hour from now

CookieDate.Parse("Sun, 06-Nov-94 08:49:37 GMT");      // 1994-11-06 08:49:37 +00:00
CookiePair.ParseField("SID=31d4d96e407aad42; lang=en-US");   // two pairs
```

`SetCookie.Parse` is the user agent's algorithm of RFC 6265 §5.2, which reads nearly anything:
it refuses only a field with no `=` or no name. Every attribute is kept; the properties take the
last one that counts, so a Path that is not absolute means the default path, and Max-Age comes
before Expires. `CookieDate` finds a time, a day, a month and a year among the tokens in any
order, as browsers do. `CookiePair.ParseField` reads the `Cookie` field a server receives.
`SetCookie.DomainMatches`, `DefaultPath` and `PathMatches` decide which request gets a cookie;
the store itself is the caller's.

### Forwarded

```csharp
var elements = ForwardedElement.ParseField(
    "for=192.0.2.43, for=\"[2001:db8:cafe::17]:4711\";by=_hidden;proto=https;host=example.com");

elements[0].For!.Name;         // 192.0.2.43
elements[1].For!.Kind;         // ForwardedNode.Kinds.IPv6
elements[1].For!.PortNumber;   // 4711
elements[1].By!.Kind;          // ForwardedNode.Kinds.Obfuscated
elements[1].Proto;             // https
```

A `by` or `for` value has to be a node identifier, `host` a host and port, and `proto` a URI
scheme; a field where one is not, or where a parameter appears twice in an element, is refused.
Several `Forwarded` fields are one list: join them with commas. Nothing in the field can be
trusted — any proxy on the way may have written it.

## Addresses

### URIs

```csharp
var uri = UriReference.ParseUri("https://user@example.com:8080/a/b?q=1#top");

uri.Scheme;    // https
uri.Host;      // example.com
uri.Port;      // 8080
uri.Path;      // /a/b
uri.Query;     // q=1
uri.Fragment;  // top

UriReference.Parse("../images/logo.png?size=2").Path;   // ../images/logo.png
UriReference.Decode("hello%20world");                    // hello world
```

`Parse` reads a URI reference, relative or not; `ParseUri` asks for a scheme. IPv4, IPv6 and
`IPvFuture` hosts are read as the RFC writes them. Every part comes back undecoded: `%2F` in a
path segment is data, and decoding it while parsing would make it a separator it is not.

### URI Templates

```csharp
var template = UriTemplate.Parse("/users{/id}{?fields,page:3}{&tags*}");

template.Expand(new Dictionary<string, object?>
{
    ["id"]     = "igor",
    ["fields"] = new[] { "name", "email" },
    ["page"]   = "12345",
    ["tags"]   = new[] { "a", "b" },
});
// /users/igor?fields=name,email&page=123&tags=a&tags=b
```

All four levels of RFC 6570. A value is a string, a list or an associative array; a number is
its invariant text, and a missing or null value is undefined. `template.Parts` says which
variables a template asks for. A template using a reserved operator is refused, and a prefix on
a list or an associative array throws on expansion.

### Email addresses

```csharp
var list = EmailAddress.ParseList(
    "\"Joe Q. Public\" <john.q.public@example.com>, jdoe@example.org, Undisclosed recipients:;");

var joe = (EmailAddress.Mailbox)list[0];
joe.DisplayName;          // Joe Q. Public
joe.Address.LocalPart;    // john.q.public
joe.Address.Domain;       // example.com

var group = (EmailAddress.Group)list[2];
group.Members.Count;      // 0

AddrSpec.Parse("(comment)\"john smith\"@example.com").LocalPart;   // john smith
AddrSpec.TryParseStrict("john . smith@example.com", out _);        // false: obsolete syntax
```

A value is what the address means: comments and folding are gone, a quoted local part is its
content, and a display name reads with single spaces. `Parse` and `ParseList` accept the
obsolete syntax RFC 5322 requires a receiver to accept — routes, empty list members, space
around dots, control characters in quoted text; the `Strict` forms accept only what a sender
may write. `ToString` writes the form a sender should. The domain is RFC 5322's; whether mail
could be delivered to it is not asked.

## Time and language

### Timestamps

```csharp
var timestamp = Timestamp.Parse("1996-12-19T16:39:57-08:00");

timestamp.Date;                // FullDate { Year = 1996, Month = 12, Day = 19 }
timestamp.Time.Offset;         // -08:00:00
timestamp.ToDateTimeOffset();  // 12/19/1996 4:39:57 PM -08:00

FullDate.Parse("2020-02-29");             // a leap year
FullDate.TryParse("2021-02-29", out _);   // false
FullTime.Parse("15:59:60-08:00").Second;  // 60: the leap second, at 23:59:60 UTC
```

The date and time format of the Internet, the profile of ISO 8601 that HTTP and JSON Schema
use. A day has to be in its month and a leap second in the last minute of the UTC day. The
fraction of a second is kept as written, however long. `-00:00` is UTC with the local offset
unknown, and `LocalOffsetUnknown` tells it from `Z`. `ToDateTimeOffset()` throws for a leap
second, which `DateTimeOffset` cannot hold.

### Language tags

```csharp
var tag = LanguageTag.Parse("zh-cmn-Hans-CN-u-ca-chinese");

tag.Language;            // zh
tag.ExtendedLanguages;   // [cmn]
tag.Script;              // Hans
tag.Region;              // CN
tag.Extensions[0];       // Extension { Singleton = u, Subtags = [ca, chinese] }

LanguageTag.Parse("EN-latn-us").ToString();   // en-Latn-US
LanguageTag.Parse("i-klingon").Grandfathered; // i-klingon
```

A tag compares without case; its parts are kept as written, and `ToString()` writes the case
BCP 47 recommends. A tag read here is well-formed; whether every subtag is in the IANA
registry is not asked.

## More

The grammars are in the repository, one file per specification, beside the tests that hold
them: [src/DotGram.Web](https://github.com/dotgram/dotgram/tree/main/src/DotGram.Web). How they
are written is [.Gram](https://github.com/dotgram/dotgram)'s documentation.
