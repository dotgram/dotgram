---
name: dotgram-web
description: Read the formats of the web exactly as their RFCs define them with DotGram.Web — JSON without losing a digit, JSON Pointer and Patch, HTTP header fields (Content-Type, Accept, Structured Fields, Link, Content-Disposition, cookies, Forwarded), URIs and URI Templates, email addresses, RFC 3339 timestamps and BCP 47 language tags. Use when a project references the DotGram.Web package, or when a value has to be read the way its specification says rather than approximately. Not a serializer, an HTTP client or a cookie store: it reads values and writes them back in canonical form, and binds nothing to your types.
---

# DotGram.Web

One parser per specification, each held to the test suite written for it. Everything is in
the `DotGram.Web` namespace; there is no runtime behind the parsers and nothing to configure.

The [README][readme] beside this file shows every format with examples. This is what to decide
before using one, the contract they all share, and what is easy to get wrong.

[readme]: https://github.com/dotgram/dotgram/tree/main/src/DotGram.Web

## When this package

Reach for it when the exact reading matters: when a value has to mean what its RFC says, keep
what the text said, and be refused when it is malformed.

- **JSON** — when the text has to survive: a number's every digit, the order of members, a
  name written twice. It gives you a tree to walk, not objects. To bind JSON to your own types,
  use a serializer.
- **URIs** — when parts must come back undecoded and a relative reference is a value of its
  own, not something to resolve on the spot.
- **Email addresses** — when display names, groups and quoted local parts matter, and when
  what a receiver must accept differs from what a sender may write.
- **Timestamps** — when a leap second, a fraction longer than seven digits, or `-00:00` as
  distinct from `Z` must not be lost.
- **HTTP header fields** — when a field has to be read the way its RFC defines it, parameters,
  quoting and extended values included.

## The contract every parser shares

- **Input is a `string` of characters.** Decoding bytes is yours: JSON is UTF-8 on the wire,
  and a byte order mark in front of it is refused, not skipped.
- **`Parse` throws `FormatException`**, its message naming where the text stopped fitting;
  **`TryParse` returns false** instead. `JsonPatch` is the exception: it has no `TryParse`, so
  without exceptions read it with `JsonValue.TryParse` and then `JsonPatch.TryRead`.
- **A result is a record.** Two that mean the same are equal, and `ToString` writes the form
  the specification generates, not the text you gave it.
- **A field is not an element.** Fields that hold a list are read whole with `ParseField`
  (`WebLink`, `CookiePair`, `ForwardedElement`), `ParseAccept` or `StructuredField.ParseList`.
  The same field sent on several lines is one value: join `Forwarded` lines with commas, and
  structured field lines with `StructuredField.Combine`.

```csharp
using DotGram.Web;

var document = (JsonValue.Object)JsonValue.Parse("""{ "price": 19.999999999999999999, "tags": ["a"] }""");
var price    = (JsonValue.Number)document.Members[0].Value;

string exact = price.Text;                          // 19.999999999999999999, as written
double close = price.ToDouble();                    // 20
bool fits    = price.TryToDecimal(out var amount);  // true: twenty digits fit a decimal

if (!JsonValue.TryParse(untrusted, out var value))
    return;                                         // malformed JSON, no exception
```

## What is easy to get wrong

**Security.** Two fields carry what someone else wrote:

- `ContentDisposition.Filename` is **the sender's filename, unchecked**. Strip any path and
  make it safe before it names a file on your disk.
- A `Forwarded` field can have been written by **any proxy on the way**. Nothing in it is
  proof of who the client is.

**JSON.**

- A number keeps its text. `ToDouble` gives the nearest double. `TryToDecimal` **rounds**
  past decimal's 28 or so significant digits and still returns true, rather than refusing.
  `TryToInt64` takes only an integer with no fraction or exponent. When every digit
  matters, keep `Text`.
- An object keeps its members in order, a name written twice included. Decide which one you
  mean when you look one up.
- `JsonPointer.Resolve` answers C# `null` when the pointer refers to nothing — which is not
  JSON's `null`, a `JsonValue.Null`.
- `JsonPatch.Apply` changes nothing it is given. It returns a new document, and a patch that
  fails throws `JsonPatchException` (or `TryApply` returns false) with nothing half-applied.

**Addresses.**

- A URI's parts come back undecoded, because `%2F` in a segment is data and not a separator.
  Split first, then decode each part with `UriReference.Decode`.
- `UriReference.Parse` reads a reference, relative or not; `ParseUri` requires a scheme.
- `AddrSpec.Parse` and `EmailAddress.ParseList` accept the obsolete syntax a receiver must.
  To check an address a user typed, use the `Strict` forms, which accept only what a sender
  may write. Neither asks whether mail could be delivered.

```csharp
bool valid = AddrSpec.TryParseStrict("john . smith@example.com", out _);   // false: obsolete spacing

var accept = MediaRange.ParseAccept("text/*;q=0.3, text/plain;q=0.7, */*;q=0.5");
decimal q  = MediaRange.Quality(accept, MediaType.Parse("text/plain"));     // 0.7
```

**HTTP fields.**

- `MediaType` equality ignores case in the type, subtype, parameter names and `charset`, and
  nowhere else. `MediaRange.Quality` is 0 when nothing in `Accept` matches.
- `SetCookie.Parse` is the user agent's algorithm and accepts nearly anything; it refuses only
  a field with no `=` or no name. Which request gets a cookie is `DomainMatches`, `DefaultPath`
  and `PathMatches`; the store is yours.
- A `Link` target and anchor are kept as written. Resolving a relative one needs the URL of
  the response that carried the field.
- A structured field that does not parse is refused whole. `Serialize*` throws
  `ArgumentException` for a value with no canonical form.

**Time and language.**

- `Timestamp.ToDateTimeOffset()` throws for a leap second, which `DateTimeOffset` cannot hold.
  `timestamp.Time.LocalOffsetUnknown` tells `-00:00` from `Z`.
- A `LanguageTag` is well-formed, not checked against the IANA registry. It compares without
  case, and `ToString` writes the case BCP 47 recommends.
