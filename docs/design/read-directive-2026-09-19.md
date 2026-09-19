# `read R`: one R from a position (design, 2026-09-19)

For the architect, and through him the spec text for Igor, before any code. Igor has decided the
directive (`read`), the method names (`ReadR`, `TryReadR`), and where a reading ends: at the
first derivation of `R` that succeeds, in the order the alternatives are written, and not the
longest one. A grammar that wants a boundary writes it with `?!`.

## 1. What it publishes

```dotgram
read Value                          // ReadValue, TryReadValue
read Value as Scalar                // Scalar, TryScalar
internal read Value with (Word = AsciiWord) as ReadAscii
read (v: Number => @(v)) as ReadNumber : @int
read Value bytes                    // and the byte forms, section 1.3
```

A directive may carry what `parse` may: an access modifier, `with (...)`, `as`, and a contract
`: @T`, with the same meaning (§6: the modifier covers every method the directive makes, `as` is
required for anything but a bare name, the contract is the result type). `stream` and `yield` are
refused on `read` (a new `GRAM2010`: "`read` reads one value from a position of an input held
whole; a stream is read with `parse … yield`"). `read` is a directive only where an operand
follows it; a rule named `read` is declared with `=` or `:` as any rule is.

### 1.1 The methods, over a string

```csharp
Match<R> ReadR(string input, int start = 0);
R        ReadR(string input, ref int position);                    // throws FormatException
bool     TryReadR(string input, ref int position, out R value);
```

- **`Match<R> ReadR(input, start)`**: `Position` is `start` and `Length` how far the reading got
  from it, leading trivia included, trailing trivia not. A refusal is `NoMatch` at the furthest
  position the input was followed to, or `Starved` where the input ended inside `R` (§7.5, as
  `TryParse` decides it). A `start` outside `0..input.Length` is `NoMatch` with the message the
  positional `TryParse` gives today ("Position … is outside the input."). One reading, quiet where
  the machine can be, and the recording reading again only on a refusal (Q7.2), as `TryParse`.
- **`R ReadR(input, ref position)`**: on success `position` moves to the end of `R`, and the
  value comes back. On a refusal it throws `FormatException` with the message and position the
  match would have carried, and `position` is left as it was.
- **`bool TryReadR(input, ref position, out R value)`**: one quiet reading, no message, as
  `bool TryParseR` (9acc28ac). On `false`, `position` is unchanged and `value` is `default!`.
- In both `ref` forms a position outside the input throws `ArgumentOutOfRangeException`: it is
  the caller's mistake, not the input's content.

**Context.** Where the grammar declares `context` (§7.7), it is the parameter after the input and
the position, as the positional forms place it today, and before `out`:

```csharp
Match<R> ReadR(string input, State context);                  // start = 0
Match<R> ReadR(string input, int start, State context);
R        ReadR(string input, ref int position, State context);
bool     TryReadR(string input, ref int position, State context, out R value);
```

The first line is an overload rather than a default, since an optional `start` cannot stand
before the required context. A context the reading writes into and that rewinds (`Mark`/
`Rollback`) is rewound before the recording reading, as `TryParse` does it.

### 1.2 Over a span

The same three over `ReadOnlySpan<char>`. The positions and the rule are the same. A text a
construction captures is cut from the span (`Slice(…).ToString()`) and not from a string.
They are left out, as §6.3 leaves out forms a machine cannot offer, where the reading needs its
input as memory and not as a span:
- a reading that deepens onto a second thread (`parserWhole`);
- an external recognizer taking `ParserInput<T>`;
- a construction reading `parserInput`.

### 1.3 Over bytes

`read R bytes` adds the three over `byte[]`, `ReadOnlyMemory<byte>` and `ReadOnlySpan<byte>`,
read by the byte machine `stream bytes` already builds (BufferedBytes borrowed in place, which
never refills). No reader or stream forms: a buffer reads ahead of the value it returns, and a
stream is `parse … yield`. **Proposed as a second step**, after the string and span forms.

## 2. The spec text

§6's table gains a row:

| Directive | What it says | Generated |
| --- | --- | --- |
| `read R` | one `R` begins at a given place, and the rest of the input is not its business | `Match<R> ReadR(input, start = 0)`<br>`R ReadR(input, ref position)` — throws `FormatException`<br>`bool TryReadR(input, ref position, out R value)` |

The sentence after the table ("There are two, …") becomes three. A new paragraph, after the one
on `find`:

> **Where a `read` ends.** A `read` reads one `R` beginning at the position it is given, after
> the trivia there, and stops where the first derivation of `R` that succeeds stops, alternatives
> taken in the order they are written (§3). It is not the longest `R` that could begin there:
> `read ('a' | 'ab')` over `ab` reads `a`. Nothing after the rule is asked for, so nothing after it
> can make the reading give anything back. A rule that must not stop inside something longer says
> so itself: `Word = ['a'..'z']+ & ?!['a'..'z']`. The trivia after `R` is not read: the position
> that comes back is where `R` ended, so a host reading several values in a row reads the trivia
> between them as the next `read` begins. Over a grammar cut into tokens (§4), `R` reads tokens
> and ends where its last token ends.

§6.3's sentence on the position forms stays. §2 lists `read` among the directives.

## 3. What the generator does

- **An entry without the end.** A whole `parse` enters its rule through trivia, the rule, trivia,
  and the end of the input. The positional `TryParse` enters through trivia, the rule, trivia.
  `read` enters through trivia and the rule, and nothing more. That is a third entry shape beside
  `Register(rule, whole)`, in each rendering: the reader (`Recognize_R_Lead_Read`), the engine
  (a third registered entry), and a lowered rule, which emits the lead entry too or else leaves
  the rule to be read by methods.
- **The start position** is handed to the entry as `pos`, as the positional form does. Positions
  in everything the reading builds are offsets into the input.
- **The follow it seeds** (FollowSets): `End`, as `parse`, and not `All`, as `find`. Nothing after a
  read refuses, so no reading inside `R` is ever given back for something after it. For every
  question an analysis asks of the rule's follow, End answers as a boundary that nothing reads
  past does. The positional `TryParse` forms already read rules analyzed under End with text
  after them. **Consequence:** adding `read` to a grammar moves no carrier gate: the Replay causes,
  the settled repetitions and the Auto choice are those of the same rule published with `parse`.
  If the architect prefers a seed of its own (`Continuation.Stops`: never refuses, begins nothing),
  it is a few lines in FollowSets. I recommend End.
- **Refusals** are the machine's own, as today: the furthest position, the expected sets, the
  `on fail` a rule says, `Starved` where the input ended inside `R`.
- **Carriers.** The same gates, and both carriers answer alike: the Immediate twin gets the
  `read` methods where it gets `parse`'s. `carriers.md` does not move for a grammar that adds a
  `read` of a rule it already publishes.

## 4. Lexical split

`ReadR` over a split grammar tokenizes from `start`. The lexer begins where the reading begins,
so any start is a token start, and the leading trivia is skipped as the lexer skips it. A
character no token begins with ends the tokens rather than refusing the reading, as the window
form does: what follows the value is commonly not this language at all. The position that comes
back is the end of the last token the reading consumed, which the positional form already
computes (`starts[end - 1] + lengths[end - 1]`).

**What the lexer reads past `R`.** Two things, and both belong in the spec paragraph above:
- Maximal munch decides a token's end by the characters after it. So the last token of `R` is the
  longest token there, and `R` ends where that token ends. Over a grammar cut into tokens that is
  what reading tokens means.
- The lexer tokenizes ahead of the syntactic reading. **First step:** from `start` to the end of
  the input, stopping at the first character no token begins with. That costs time in the rest of
  the input, not in `R`: a host that reads many values from one long text pays the square.
  **Second step, proposed:** tokens made in blocks as the reading asks for them, through the
  reader's text helpers (`ReadAt`, `Short`, `Room`), the seam buffered input already uses
  (fix-reader-buffered §2). Then the lexer reads at most a block past the end of `R`.

## 5. Tests

- Each form over characters, and over a split grammar (EL's `Lambda` through a `read`, and a JSON
  value):
  - the value and the end position, with leading trivia skipped and trailing trivia left;
  - PEG order: `read ('a' | 'ab')` over `ab` reads one character, and `read (Word & ?!['a'..'z'])`
    reads the whole word;
  - `NoMatch` at the furthest position, `Starved` where the input ends inside `R`;
  - a start outside the input (NoMatch in the match form, ArgumentOutOfRangeException in the
    `ref` forms);
  - `position` unchanged on a refusal, in both `ref` forms.
- **A loop of `read`s** across a text with trivia between values ends where a whole `parse` of
  `R*` ends, with the same values.
- **Context** passed and rewound on the recording reading. `with` and `as` naming. The access
  modifier on every method. `stream`/`yield` refused with GRAM2010.
- **Span forms:** the same answers as the string forms. Their absence where the reading needs
  memory.
- **Carriers:** the Immediate twin answers as the tape. `carriers.md` unchanged for a grammar that
  adds a `read` of a rule it already `parse`s.
- **Compatibility:** the new methods at the C# 8 floor (`ref` positions and `out` are C# 1).
- **Snapshots:** a grammar with a `read` is added to tests/Snapshots.
- **Pair:** the refused and accepted rows through `ReadR` against `TryParseR(input, at)` on the
  same text.

## 6. Open, for Igor

1. **The positional `TryParse(input, at)`** reads `R` and the trivia after it, and returns a match.
   `read` reads `R` alone. They overlap, and the window form (`at, length`) does not. My proposal:
   keep all of them now, and say in §6.3 that `read` is the way to read one value from a position.
   Retiring the positional `TryParse` is a separate decision.
2. **Bytes** as a second step (section 1.3), and **lazy tokens** as a second step (section 4).
3. **The seed** End (section 3), unless a `Stops` of its own is wanted for clarity.
