# Reading SQL:2023 over kinds

Q1: whether the standard's grammar can be read over tokens rather than characters, what it
would cost to say so, and what it would be worth. Written from the two parsers that exist.

## The question is not whether

`HandSqlStandard` reads the whole of SQL:2023 — all forty-two publications — over a token
cursor, and answers what `SqlStandardParser` answers on 14,701 tests and 113,000 corpus lines,
clean and mutated, with nothing differing. Reading this language over kinds is therefore not a
thing to be established. It is done, it is in the repository, and it is the yardstick.

What is worth writing down is what the token layer had to be allowed to do, because those are
exactly the places where a lexer of the ordinary kind would read a different language.

## Three things a plain lexer cannot do

### A literal is written in several quoted runs, with separators between them

`'a' 'b'` is one character string literal of two runs, and what stands between the runs is a
separator — which includes a comment. `X'AB' /* */ 'CD'` is one binary literal. A lexer that
returns a token per quoted run has already lost: the parser would see two literals where the
standard sees one, and `SqlCursor` keeps `Parts` — how many runs a literal was written in —
because the parser needs it to *refuse* the rest: a date, a time and an interval string are one
run each, and `DATE 'a' 'b'` is not a date.

So the token has to absorb separators inside itself, and remember how many runs it ate.

### A key word is guarded on both sides, and the left side is not in the notation

§5.2 makes a key word a whole word. `wordboundary = IdentifierPart` says what may not follow
one, and the grammar has it. Nothing says what may not *precede* one, and the standard needs
that too: in `CHAR(198OCTETS)` the `OCTETS` runs into the number in front of it, so it is no key
word — and it is still a name, which is what lets `2K` be a length and its multiplier. The token
cursor carries `Glued` for it, and `Take(SqlWord)` refuses a glued word while `Identifier` reads
it.

Over characters this falls out of the automaton for free, because nothing ever asked where the
last token ended. Over kinds it has to be said.

### A token's body is narrower than the token

`U&'…' UESCAPE '\'` is one token whose text is the part before `UESCAPE`; no separator may stand
before the `UESCAPE`, and the escape is one character. `SqlCursor` keeps `Body` for it, one past
the literal's last quote, and `Quote`, where the literal's first quote is, because the tree is
built from the text between them and not from the token's extent.

## What the notation already covers, and what it does not

The notation has a terminal the lexer begins and a rule or the host finishes — `'/*' & Nested`,
`'<' & @M`. That mechanism covers the first and third cases as they stand: a literal is a quote
the automaton recognizes and a rule that reads runs and separators until the runs stop, and a
`UESCAPE` tail is the same shape. Neither needs anything new.

The second case does not fit. Gluing is a condition on what stands *before* a terminal, and the
notation has no way to say it. The smallest thing that would work is to let `wordboundary` guard
both sides rather than only the right — the rule is already there, it is already about exactly
this, and reading it as a two-sided guard changes no grammar that does not glue. That is one
extension, not a mechanism.

## Lazy cursor, not a lexer over the whole input

D5 says a stream is never read whole, so a pass that tokenizes the input before parsing begins is
not available to a streaming grammar, and a mechanism that only works where the whole text is in
hand is a mechanism for half the grammars.

The shape that survives D5 is the one `SqlCursor` already has: the token in front is made when
the parser asks for it, nothing is kept behind, and going back is a copy of the cursor struct. It
holds no array, so it costs nothing per token of input that the parse does not reach, and a
backtrack costs a struct copy rather than a re-lex. A lexer over the whole input remains possible
as an additional form for input that is already contiguous — the same way the buffered forms sit
beside the streaming ones — but not as the only one.

## What the compiler says when it is actually asked

The section above was written from the two parsers. Then the grammar was compiled with
`Lexical = true` in a worktree of its own, and the compiler's refusals turned out to be a
better list than the one reasoned out — shorter, and pointing somewhere else.

They come one at a time, each a `GRAM5004`:

| what refuses | what it really is |
| --- | --- |
| `?!ReservedWord` in `ActualIdentifier`, `IntroducedName` | over kinds the lexer hands over a word and the parser says whether it is reserved; a guard after the word says the same thing |
| `?!IdentifierPart` in `wordboundary` | the right-hand word boundary, which a maximal token keeps by itself |
| `?!IdentifierPart` in `SQLLanguageIdentifier`, `Multiplier` | the same again — and the second is the `2K` of the section above |
| `?!eol` in `SimpleComment` | a lookahead written where a negated class says it |
| `?!("?(" | "?)")` in `PatternQuestionMark` | the trigraph brackets, which over kinds are tokens of their own |
| `eof` at the end of a comment | expands to a lookahead for "nothing follows" |
| `BracketedComment` reaches itself | comments nest, and nesting is not regular |

**Seven of the eight are the price of reading over characters, not properties of SQL.** They
are guards the automaton needed because nothing had told it where the last token ended. Only
the last is about the language, and it is the one the notation already has a mechanism for.

The eighth is not a pattern at all: `trivia` must be readable by a scanner that commits to its
first reading. That one cost a wrong turn worth writing down. The nested comment was rewritten
"regularly" as `"/*" & ([^ '*'] | '*'+ & [^ '*' | '/'])* & '*'+ & '/'`, and the scanner refused
it — the language is regular but the reading is not committable, because the repetition and what
follows it both begin with a star. The original shape, `"/*" & (?!"*/" & any)* & "*/"`, is the
one a scanner understands: `Machine.Scan.cs` has a branch for exactly a repetition guarded by
the literal that follows it. So `?!"*/"` was never an obstacle — it is the idiom. Only the
recursion was.

## One number the experiment gives without timing anything

With the eight obstacles handled, the grammar compiles over kinds, and the first thing to look
at needs no window and no corpus:

| | normalized rules | generated | generation |
| --- | ---: | ---: | ---: |
| over characters | 654 | 13,863,417 bytes | 5,309 ms |
| over kinds | 646 | 6,771,203 bytes | 4,621 ms |

**The parser is half the size.** Not a tenth smaller, not a third — 48.8 per cent of what it
was, from the same grammar with the same options, and the generator spends 13 per cent less time
producing it. Code size is not speed, and this says nothing about either yet. But a parser of
6.8 MB where there was 13.9 is a different object to compile, to load and to keep in an
instruction cache, and it is the kind of difference that shows up in places a ratio on one
corpus does not reach.

## What it is worth, measured rather than assumed

This is the part that should decide the order of work, and it argues against doing it first.

The token layer's share of a parse is known, because the two parsers differ by exactly it and the
letters family measures it. Against a name in an empty bucket, the crowding of the reserved-word
lookup costs 0.62 µs an item for a typical initial and 1.95 for the fullest bucket in the grammar
— 7.4 and 20.3 per cent of the line. The whole word layer is wider than the crowding within it: a
name in an empty bucket still costs 1.0 µs an item more than a literal. Trivia between tokens is
0.066 µs a character against the handwritten 0.0067, ten times, but at that size it is small
beside an operand.

Against that, the profile of the generated parser puts materialization at 53 to 62 per cent of
the time, of which the value store's bookkeeping was 20 to 27 — and removing that bookkeeping
(D2) took twenty select items from 327 to 220 ms, a third, without touching a character of the
lexical question.

That comparison is wrong, and it is worth saying why rather than quietly fixing the number.
What it measures is the word layer's own cost — the buckets, the trivia rule — and that is not
what reading over kinds changes. Over kinds the machine itself is different: there is no tape of
ways, no replay, and a choice is a switch on one token. The SQL-92 split measured 1.05 to 2.45
times on accepted conditions and 1.35 to 3.85 on refused ones, which is not a tenth of anything.
The figures above bound one part of the question and were read as bounding all of it.

What the experiment will say, and this document will then carry, is the whole of it: the same
grammar over kinds against itself over characters, and both against the hand parser, on the
corpora — with the inputs the minimal handling changes (nested comments, malformed interval
strings) left out of the measurement and said so.

## What this does not settle

Whether the *generated* parser can be given a token layer without changing what it accepts is a
different question from whether a handwritten one can. The two parsers agree today because the
handwritten one mirrors the generated one — including two places where the generated parser's
construction finds the parts of an introduced literal by the first period and the first quote in
the token rather than by the BNF. A token layer under the generator would have to keep those, or
they would be corrected on one side only, and the first thing to notice would be the yardstick
disagreeing.
