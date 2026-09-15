# DotGram.Web

The formats of the web, written in `.gram` against the specifications that define them.
Where an example shows one feature, a parser here is written against a whole specification.

It is an ordinary C# library. .Gram generates the parsers into this assembly at compile
time, so nothing here carries a parser runtime, and neither does anything that references
it.

## RFC 3339 timestamps

[`Rfc3339`](Rfc3339.cs) reads the date and time format of the Internet — the profile of
ISO 8601 that HTTP, JSON Schema and most protocols since use.

```csharp
using DotGram.Web;

var timestamp = Rfc3339.ParseTimestamp("1996-12-19T16:39:57-08:00");

timestamp.Date;                // FullDate { Year = 1996, Month = 12, Day = 19 }
timestamp.Time.Offset;         // -08:00:00
timestamp.ToDateTimeOffset();  // 12/19/1996 4:39:57 PM -08:00

Rfc3339.ParseFullDate("2020-02-29");            // a leap year
Rfc3339.TryParseFullDate("2021-02-29").IsSuccess; // false
Rfc3339.ParseFullTime("15:59:60-08:00").Second;   // 60: the leap second, at 23:59:60 UTC
```

A day has to be in its month and a leap second in the last minute of the UTC day; an hour
is 00 to 23. The fraction of a second is kept as written, however long. `-00:00` is UTC
with the local offset unknown, and `LocalOffsetUnknown` tells it from `Z`.
`ToDateTimeOffset()` throws for a leap second, which `DateTimeOffset` cannot hold. It is
held to the [JSON Schema test suite](https://github.com/json-schema-org/JSON-Schema-Test-Suite)'s
`date`, `time` and `date-time` formats.

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

## RFC 5646 language tags (BCP 47)

[`Rfc5646`](Rfc5646.cs) reads a language tag into the subtags §2.1 gives it.

```csharp
using DotGram.Web;

var tag = Rfc5646.ParseTag("zh-cmn-Hans-CN-u-ca-chinese");

tag.Language;            // zh
tag.ExtendedLanguages;   // [cmn]
tag.Script;              // Hans
tag.Region;              // CN
tag.Extensions[0];       // Extension { Singleton = u, Subtags = [ca, chinese] }

Rfc5646.ParseTag("EN-latn-us").ToString();   // en-Latn-US
Rfc5646.ParseTag("i-klingon").Grandfathered; // i-klingon
```

A tag is case-insensitive: its parts are kept as written, and `ToString()` writes the case
§2.1.1 recommends, which is the registry's. A tag read here is *well-formed*, which needs
nothing but the ABNF; whether every subtag is in the IANA registry — *valid* — is a question
for the registry, and is not asked.

## RFC 6266 Content-Disposition

[`Rfc6266`](Rfc6266.cs) reads the `Content-Disposition` header field.

```csharp
using DotGram.Web;

var field = Rfc6266.ParseContentDisposition(
    "attachment; filename=\"EURO rates\"; filename*=utf-8''%e2%82%ac%20rates");

field.IsAttachment;   // true
field.Filename;       // € rates
field.Find("filename")!.Value;   // EURO rates
```

The type and parameter names compare without case. `Filename` prefers a `filename*` that
decodes — UTF-8 or ISO-8859-1, after RFC 8187 — to `filename`. A type other than `inline` is an
attachment, as §4.2 asks. A field the ABNF does not make, a parameter named twice included, is
refused whole: §3 leaves recovery to the recipient. The filename is what the sender wrote;
stripping its path and making it safe to save is the caller's (§4.3). It is held to Julian
Reschke's [tc2231](http://test.greenbytes.de/tech/tc2231/) test cases.

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

## RFC 6901 JSON Pointer

[`Rfc6901`](Rfc6901.cs) reads a pointer into its reference tokens, with `~1` and `~0` undone
in the order the RFC gives, so `~01` is `~1`.

```csharp
using DotGram.Web;

var pointer = Rfc6901.ParsePointer("/a~1b/0");

pointer.Tokens;          // [a/b, 0]
pointer.ToString();      // /a~1b/0
pointer.ToUriFragment(); // #/a~1b/0

Rfc6901.ParseFragment("#/c%25d").Tokens;   // [c%d]

JsonPointer.ArrayIndex("10");   // 10
JsonPointer.ArrayIndex("01");   // null: a leading zero is no index
JsonPointer.IsPastTheEnd("-");  // true
```

And what it refers to in a document read by `Rfc8259`:

```csharp
var document = Rfc8259.ParseJson("""{ "foo": ["bar", "baz"], "a/b": 1 }""");

Rfc6901.ParsePointer("/foo/1").Resolve(document);   // "baz"
Rfc6901.ParsePointer("/a~1b").Resolve(document);    // 1
Rfc6901.ParsePointer("/foo/-").Resolve(document);   // null: the element after the last
```

`Resolve` answers null where the pointer refers to nothing — a missing member, an index past
the end, `-`, or a name an object holds twice, whose member the RFC calls undefined — which
is not JSON's `null`.

## RFC 6902 JSON Patch

[`Rfc6902`](Rfc6902.cs) reads a JSON Patch document and applies it to a document read by
`Rfc8259`.

```csharp
using DotGram.Web;

var patch = Rfc6902.ParsePatch("""
    [
      { "op": "test", "path": "/a/b/c", "value": "foo" },
      { "op": "replace", "path": "/a/b/c", "value": 42 },
      { "op": "copy", "from": "/a/b/c", "path": "/a/b/d" }
    ]
    """);

var result = patch.Apply(Rfc8259.ParseJson("""{ "a": { "b": { "c": "foo" } } }"""));

result.ToString();   // {"a":{"b":{"c":42,"d":42}}}
```

Applying changes nothing it is given: the result is a new document sharing what the patch did
not touch, and a patch that fails throws `JsonPatchException` — or `TryApply` answers false —
with no half-patched document left behind. `test` compares as §4.6 asks: numbers by value at
any precision, so `1` equals `1.0`, and objects whatever the order of their members;
`Rfc6902.AreEqual` is that comparison. `op`, `path`, and the `value` or `from` an operation
takes are each written once; other members are ignored. Removing the whole document is an
error. It is held to the [JSON Patch test suite](https://github.com/json-patch/json-patch-tests).

## RFC 8259 JSON

[`Rfc8259`](Rfc8259.cs) reads a JSON text into `JsonValue`.

```csharp
using DotGram.Web;

var value = (JsonValue.Object)Rfc8259.ParseJson("""{ "pi": 3.14159265358979323846, "tags": ["a", "b"] }""");

var pi = (JsonValue.Number)value.Members[0].Value;
pi.Text;                  // 3.14159265358979323846
pi.ToDouble();            // 3.141592653589793
value.ToString();         // {"pi":3.14159265358979323846,"tags":["a","b"]}
```

A number keeps the text it was written with, and `ToDouble`, `TryToDecimal` and
`TryToInt64` read it at the precision wanted. An object keeps its members in order, a name
written twice included. Nesting is read as deep as the input goes. The text is characters:
decoding the bytes, as UTF-8, is the caller's, and a byte order mark is refused rather than
skipped. It is held to [JSONTestSuite](https://github.com/nst/JSONTestSuite)'s parsing cases.

## RFC 8288 Link header field

[`Rfc8288`](Rfc8288.cs) reads the `Link` header field into its link-values.

```csharp
using DotGram.Web;

var links = Rfc8288.ParseLinks(
    "</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel, " +
    "</TheBook/chapter4>; rel=\"next\"; title=\"next chapter\"");

links[0].Target;       // /TheBook/chapter2
links[0].Relations;    // [previous]
links[0].Title;        // letztes Kapitel
links[1].Title;        // next chapter
links[1].Parameters;   // every link-param, in order
```

A link-param's value is the same whether written as a token or a quoted string. Where §3
allows a parameter once — `rel`, `title`, `title*`, `media`, `type` — the first counts, and
`Title` prefers a `title*` that decodes. RFC 8187 values are decoded from UTF-8, and from
ISO-8859-1 for older senders. A target and an anchor are kept as written: resolving a
relative one needs the URL of the response that carried the field.

## RFC 9110 media types

[`Rfc9110`](Rfc9110.cs) reads a media type from the `Content-Type` header field and the
media ranges of `Accept`.

```csharp
using DotGram.Web;

var type = Rfc9110.ParseContentType("Text/HTML; Charset=\"UTF-8\"");

type.Charset;                                                  // UTF-8
type == Rfc9110.ParseContentType("text/html;charset=utf-8");   // true

var accept = Rfc9110.ParseAccept("text/*;q=0.3, text/plain;q=0.7, */*;q=0.5");

Rfc9110.Quality(accept, Rfc9110.ParseContentType("text/plain"));   // 0.7
Rfc9110.Quality(accept, Rfc9110.ParseContentType("text/html"));    // 0.3
Rfc9110.Quality(accept, Rfc9110.ParseContentType("image/png"));    // 0.5
```

A type, a subtype and a parameter name compare without case, and so does a `charset` value;
other values compare as written. Empty parameters and empty list elements are accepted, as §5.6
asks of a recipient. A parameter named `q` is the weight wherever it stands, and has to be a
qvalue. `Quality` takes the weight of the most specific matching range, or 0 where none
matches. It is held to every media type the IANA registry holds.

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

And back again, as §4.1 serializes:

```csharp
var priority = new OrderedMap<Member>(
[
    new("u", new Item(new BareItem.Integer(3), new OrderedMap<BareItem>([]))),
    new("i", new Item(BareItem.Boolean.True, new OrderedMap<BareItem>([]))),
]);

Rfc9651.SerializeDictionary(priority);   // u=3, i
```

`SerializeItem`, `SerializeList` and `SerializeDictionary` write the canonical form: a true
Boolean as its key alone, a Decimal rounded half to even to three places. What has no
serialization — a key with an uppercase letter, a String outside printable ASCII, an Integer
of sixteen digits — throws `ArgumentException`. An empty List or Dictionary is the empty
string, and the RFC's advice for it is not to send the field.

It is held to [the HTTP working group's test suite](https://github.com/httpwg/structured-field-tests).

## Taking it

```xml
<PackageReference Include="DotGram.Web" Version="0.1.0" />
```

There is no companion runtime package, and no generator to install alongside it: the
parsers were generated when this assembly was compiled.
