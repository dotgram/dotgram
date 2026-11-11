Since 0.2.0. The same formats, read the same way on accepted input; refusals that cost more than linear time are gone, and a few readers follow their RFC more closely.

BREAKING CHANGES, and what to do

- A Link field is refused where a parameter value breaks the syntax RFC 8288 gives it: a rel or rev that is not relation types separated by spaces, each a registered name or an absolute URI; an anchor that is not a URI-Reference; an hreflang that is not a language tag; a type that is not a type and subtype. Of rel and type only the first is asked. There is no option to accept such a field.
- A Forwarded element read from a position stops earlier, or is refused, where a pair's value is invalid (by, for, host, proto) or a name repeats; a Content-Disposition read from a position refuses a parameter name written twice. Migration: read the whole field (ParseField, TryParseField, Parse, TryParse), which is unchanged.
- An Accept refusal may no longer list Token among what could stand at its position ("Expected Token, ';' or end of input" is now "Expected ';' or end of input"), at the same position.
- JsonPointer.ArrayIndex, JsonPointer.IsPastTheEnd and AddrSpec.IsDotAtom are no longer public. Migration: JsonPointer.Resolve reads an array element the way ArrayIndex did; a token is past the end where it is "-"; a local part is a dot-atom where AddrSpec.ToString writes it unquoted.

NEW

- WebLink.ParseStrictField and TryParseStrictField hold a Link field to the ABNF to the letter, and so refuse a registered relation type that is not lowercase (rel=Next), which ParseField takes in any case, as RFC 8288 compares it.

FIXED

- Refusal time. Content-Type, Accept, Forwarded and Link refuse a field with a long run of spaces in linear time, where it was cubic (2.6 seconds for a Content-Type of 1,600 spaces); the Structured Field readers refuse a long key or token in linear time. An email address with an unclosed comment, quoted string or domain literal or a long run of commas, a Content-Type with many empty parameters and a Structured Field Dictionary with many members are refused in linear time, where it was quadratic (1.3 seconds for an address list of 8 KB). Link, Forwarded, Content-Disposition and Accept refuse a long adversarial header in linear time, where each was quadratic. Values, refusals and messages of these are as before.
- Against the RFCs: an IPvFuture host may write its version flag as V (RFC 3986); a URI Template literal refuses the noncharacters at the end of each supplementary plane and U+E0000 to U+E0FFF (RFC 6570); a Set-Cookie Max-Age within a long's range is read exactly, where the values nearest either end came back as long.MaxValue or long.MinValue; a Forwarded field of semicolons alone (;) is a field with no pairs, where it was refused.

CHANGED

- The URI, cookie-date, Content-Disposition, Forwarded and Link readers are carried immediately, as the JSON, JSON Pointer, URI Template, timestamp and Set-Cookie readers already were. Values, refusals and messages are as before; a text refused part-way allocates more.

FASTER

- Against 0.2.0 on the release benchmark the readers are about 26% faster at the median (a full URL 36%, a JSON object 30%). A full Link field is 16% slower, with 1.9 times the bytes, and a refused one 11 to 12% slower; a refused language tag is 17 to 20% slower, and a few refusals allocate more.