# Reading one value from a position: the positional forms finished (design, 2026-09-19)

For the architect, and through him §6.3's text for Igor, before any code. Igor has decided the
way: no third directive. A `parse` already offers a form that begins where it is told and does
not demand the end of the input (§6.3), and that form is how one value is read from a position.
What is left is to finish it. This file replaces the `read R` design of the same day; what that
one said about where a reading ends is kept, because it is a property of reading from a position
and not of a directive.

## 1. What is missing

Today, beside `Match<R> TryParseR(string input)`, a `parse` read by the automaton or by methods
offers:

```csharp
Match<R> TryParseR(string input, int at);              // begins at `at`, need not reach the end
Match<R> TryParseR(string input, int at, int length);  // and sees nothing from `at + length` on
```

Four things are missing, one is wrong, and one place a host is sent away it should not be
(§1.5, sql-39's on real SQL scripts).

### 1.1 The form that moves a position and says nothing

```csharp
bool TryParseR(string input, ref int at, out R value);
bool TryParseR(string input, ref int at, int length, out R value);
```

One quiet reading, no message, no second reading, as `bool TryParseR(input, out R value)` reads
a whole input (9acc28ac) — the shape §6.1 now names as its one exception. On success `at` moves
to the end of what was read; on `false` it is left where it was and `value` is `default!`. This
is the form a loop is written with:

```csharp
var at = 0;
while (Grammar.TryParseValue(text, ref at, out var value))
    values.Add(value);
```

An `at` outside `0..input.Length` throws `ArgumentOutOfRangeException` in these two forms: a
position of the caller's own making is the caller's mistake. The `Match` forms keep what they do
today and answer `NoMatch` with "Position … is outside the input.".

### 1.2 The end of a reading, which is wrong today

The positional and window forms read the trivia after the rule, as the whole form does. So the
position that comes back is past it, and `Match.Length` counts it. That is wrong for the thing
these forms are for: a value's extent ends where the value ends, and a host reading a piece of a
text it holds wants exactly that piece.

**Over a grammar cut into tokens this is already so**: the lexer eats the trivia, the entry is
the rule alone, and the reading ends at the end of its last token. So this is a change over
characters, and it makes the two halves answer alike.

**Proposal: the positional and window forms stop where the rule ends, and read no trivia after
it.** Not an option, not a modifier, not a parameter — the other behaviour is not wanted by
anyone and there is nothing to choose between. A loop reads the trivia between two values as the
leading trivia of the next reading, which every entry already skips, so the loop above is
unaffected. The whole form is untouched: it must reach the end of the input, and the trivia
before the end is part of reaching it.

What this changes for a caller: `Match.Length` and the position that comes back are shorter by
the trivia after the value. In this repository the one consumer is the expression language, which
reads an interpolation hole through the window form and trims what is left over, so it answers
the same either way. It is a behaviour change to a published form and belongs in the release note.

### 1.3 Over bytes already in memory

A publication that takes bytes (`parse R stream bytes`) reads `byte[]` and
`ReadOnlyMemory<byte>` in place today, whole. The same three shapes are missing over them:

```csharp
Match<R> TryParseR(ReadOnlyMemory<byte> input, int at);
Match<R> TryParseR(ReadOnlyMemory<byte> input, int at, int length);
bool     TryParseR(ReadOnlyMemory<byte> input, ref int at, out R value);
```

and the same over `byte[]`. They read through the byte machine that publication already builds,
from `at` instead of from 0. A byte form for a publication that never asked for bytes is a
question about the notation (a `bytes` modifier without `stream`), not about these forms, and is
left out.

### 1.4 Over a span of characters

The same three over `ReadOnlySpan<char>`, where the reading does not need its input as memory —
a reading that deepens onto a second thread, an external recognizer taking `ParserInput<T>`, a
construction reading `parserInput`. Where it does, the span forms are left out, as §6.3 leaves
out the forms a machine cannot offer. Positions and the rule are the same; a captured text is cut
from the span.

### 1.5 Where a reading may begin

Over a grammar cut into tokens the positional form refuses an `at` that is not the start of a
token of the whole text. A host reading a SQL script one statement at a time — sql-39's case —
hands in the position after the previous statement, which is a newline, a line comment, or a
block comment, and is sent away to implement the language's trivia itself: line comments, nested
block comments, and what is a space in this dialect. That is the whole of what the third
directive was wanted for, and the window form does not answer it, because it asks for a length
and the length is what the host is trying to find out.

**Proposal: the reading skips the trivia at `at` and begins at the first token at or after it,
and `Match.Position` says where it really began.** Over characters that is what the entry already
does with the leading trivia; over tokens it falls out of tokenizing from `at` (section 2): the
lexer starts there, skips what is trivia to it, and the first token it makes is the first token
at or after `at`. Where nothing but trivia is left, the reading refuses as an empty input does.

`Match.Position` therefore becomes where the value begins rather than the `at` it was handed, and
`Match.Length` its own extent — with §1.2, exactly the value and neither the trivia before it nor
the trivia after. The `ref` forms move `at` to the end of the value. The whole-input form is
unchanged: its `Position` is 0 and its `Length` the whole input.

## 2. Lazy tokens over a split grammar: an acceptance condition

Over a grammar cut into tokens (§4) the positional form tokenizes the whole input on every call,
and then finds the token that begins at `at`. A loop of readings over a long text therefore costs
its square. That must not ship.

**What the form promises** is that it answers as if the whole input were tokenized: the value it
reads, and where it ends, do not depend on how much of the input the lexer has looked at. Two
cheaper schemes break that promise or the loop, and are written here so they are not tried again:

- **A window that doubles.** Tokenize `[at, at + block)`, read, and where the reading touched the
  edge, tokenize twice as much and read again. The reading is repeated, which is geometric and
  therefore fine, but "touched the edge" cannot be told: a lookahead may peek at the edge without
  consuming it and see a false end of input, and the machine records how far it looked only when
  it refuses. A grammar with `?!any` in it would then answer differently for a window that ends
  where the value does.
- **A tokenization kept between calls.** Tokenize the whole input once and keep it, keyed by the
  string, for the next call. It makes the loop linear and the promise holds, but a single reading
  of a short value from a long text still tokenizes all of it, and something must decide when the
  kept tokenization is dropped. A host that wants that can have it explicitly, as a reading
  session it holds, which is a separate question about the API.

**The proposal: tokens made as they are asked for.** A third buffered source beside `BufferedText`
and `BufferedBytes` (fix-reader-buffered §2): `BufferedKinds`, which holds the text, lexes from
`at` in blocks, and answers the reader's helpers — `Get(p)` the kind of token `p`, `Ensure(p, n)`
whether there are `n` tokens from `p` on (lexing more where there are not), `Slice(p, n)` the
kinds for a literal of several tokens, and the starts and lengths for the positions a refusal and
a capture need. The reading never sees a false end: `Ensure` lexes on, and `false` means the true
end of the input, or the first character no token begins with, which ends the tokens there as the
window form already has it.

- The seam is the one the buffered reader already reads through, so both renderings have it: the
  reader over buffered input (d994009a, c6e0348b) and the engine.
- The whole-input form keeps the eager tokenizer: it reads the whole input by definition, and one
  pass over an array is cheaper than a block-wise one.
- The window form keeps its eager tokenizer too, bounded by the window the caller named.
- A lookbehind that reaches before `at` has nothing to look at, which is what it has today: the
  reading begins at `at` and what is before it was not tokenized. Unchanged, and worth a line in
  §6.3.
- The block is a count of tokens, not of characters, and its size is an implementation choice;
  it never changes an answer.
- Tokenizing from `at` is also what lets a reading begin between tokens of the whole text
  (§1.5): the lexer is asked to begin there, so where the whole text sees one token a positional
  reading may see the beginning of another, which is what a host reading a piece of a text means.

Cost of the loop after this: each reading lexes the tokens it reads, plus at most one block, so a
loop over a text costs the text once.

## 3. What §6.3 says

The paragraph "A position and a window" is rewritten to hold the whole set and what a reading
from a position is. Draft for Igor:

> **Reading one value from a position.** Beside the forms that read a whole input, a `parse` read
> by the shared automaton or by methods offers the forms that begin where they are told:
>
> ```csharp
> Match<R> TryParseR(string input, int at);              // begins at `at`, need not reach the end
> Match<R> TryParseR(string input, int at, int length);  // and sees nothing from `at + length` on
> bool     TryParseR(string input, ref int at, out R value);          // and moves `at` past it
> bool     TryParseR(string input, ref int at, int length, out R value);
> ```
>
> This is how one value is read from a place in a text: the rest of the input is not the reading's
> business. `Position` is where the value begins — the trivia at `at` is skipped — and `Length` is
> its own extent. The `bool` forms are the `bool` form of §6.1 with a position: one quiet reading, no
> message, and `at` moved to the end of what was read, or left where it was and `false`.
>
> **Where such a reading ends.** It reads the trivia at `at`, then one `R`, and stops where the
> first derivation of `R` that succeeds stops — alternatives in the order they are written (§3),
> and not the longest `R` that could begin there: over `ab`, a rule `'a' | 'ab'` reads `a`.
> Nothing after the rule is read, so nothing after it can make the reading give anything back,
> and the trivia after the value is left where it is. A rule that must not stop inside something
> longer says so itself: `Word = ['a'..'z']+ & ?!['a'..'z']`. Over a grammar cut into tokens
> (§4), the reading reads tokens and ends where its last token ends — the longest token that
> begins there, which is what reading tokens means. A lookbehind reaches no further back than
> `at`.
>
> What comes back says where the reading began and how far it got, and every position in it — and
> in whatever the reading builds — is an offset into `input`, so a host reading a piece of a text
> it holds keeps what those positions mean. The first form has to begin where a token of the text
> begins; where `at` is inside trivia — a newline, a comment — the reading begins at the first
> token after it, and `Position` says where. The window form cuts only the window into tokens, so
> it may begin anywhere, and a character no token begins with ends the tokens rather than refusing
> the reading: a hole in an interpolated string, read up to the `:` its format begins with, is the
> shape it is for.
>
> **What `eof` means here.** A reading from a position ends where the rule ends, not where the
> input does, so `eof` written in a rule (§7.4) is still the end of the whole input; inside a
> window it is the end of the window, which is the whole of what a window is. A publication compiled as a plain method, with the one entry a whole parse
> needs, gets none of them: its rules were proved to need nothing else only against the end of
> the input.

The bytes and span forms belong in the same paragraph in one sentence each, once section 1.3 and
1.4 land.

## 4. What the generator does

- **An entry that stops at the rule.** A whole `parse` enters through trivia, the rule, trivia and
  the end of the input; the positional entry enters through trivia, the rule, trivia. It becomes
  trivia and the rule (§1.2), in every rendering: the reader's `Recognize_R_Read_Body`, the
  engine's registered entry, and the lowered form, which has neither today and keeps none.
- **Nothing about the analysis changes.** The positional entry is the one that exists; its rule
  is analyzed with the follow a `parse` seeds (`Continuation.End`), which is what these forms
  already read under — decided, no seed of its own. No carrier gate moves, and `carriers.md` does
  not change.
- **`TokenAt`** (the binary search for a token beginning exactly at `at`) goes: the lexer begins
  at `at` and the first token it makes is the answer (§1.5).
- **The `bool` forms** are the quiet reading of 9acc28ac with `pos` at `at` and the end written
  back, and they recycle the tokens on every path.
- **`BufferedKinds`** (section 2) is the one new piece of support, emitted for a split grammar
  that has a positional form.

## 5. Tests

- The `bool` positional forms: the value and the end; `at` unmoved and `value` null on `false`;
  the loop over a text of several values reading what a whole `parse` of `R*` reads; an `at`
  outside the input throwing.
- The end of a reading: trailing trivia left where it is (`Match.Length` and the moved `at`), in
  the positional, window and `bool` forms; the expression language's hole unchanged.
- PEG order: `'a' | 'ab'` over `ab`; `?!` as the boundary; over kinds the end of the last token.
- Beginning inside trivia: a SQL script of several statements read one at a time, where the
  position handed in is a newline, a line comment and a block comment in turn; `Position` says
  where each statement began; a tail of nothing but trivia refuses.
- `eof` in a rule read from a position is the end of the input, and inside a window the end of the
  window.
- Lazy tokens: a text of N values read in a loop costs time linear in N (the shape of the curve,
  as `--big` does it, not a wall-clock bound); a value at the end of a long text is read without
  tokenizing what is after it (a counter in the test's own lexer, or the block count); the answers
  are those of the eager tokenizer on the same input, for every grammar in the test corpus that is
  split; a lookbehind grammar refused or answering as it does today.
- Bytes and span forms: the same answers as the string forms, and their absence where a machine
  cannot offer them.
- Compatibility at the C# 8 floor; a snapshot grammar with a positional `bool` form; the refusal
  record unchanged.
- A stand pair: the loop row (a text of many values) before and after lazy tokens, and the
  refusal rows through the `bool` positional form.

## 6. Open, for Igor

1. **The trailing trivia** (§1.2): a behaviour change to the positional and window forms over
   characters, with no option offered; over tokens it is already so. I recommend it; the
   alternative is a second pair of names, which is worse.
2. **Where a reading begins** (§1.5): `at` inside trivia is a refusal today and becomes the first
   token after it, with `Position` saying where. §6.3 says "refused elsewhere" today, so this is
   a change to the specification as well as to the forms.
3. **Bytes without `stream`** (§1.3): a notation question, left out of this design.
4. **A reading session** that keeps a tokenization across readings (section 2): an API question,
   left out; `BufferedKinds` makes the loop linear without it.
