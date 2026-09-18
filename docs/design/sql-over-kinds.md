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
| `?!ReservedWord` in `ActualIdentifier`, `IntroducedName` | over kinds the lexer hands over a word and the parser says whether it is reserved — but a guard is **not** the same thing, see below |
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

### A guard is not a lookahead, and the difference cost 241 lines

The workaround for the first obstacle — reading the word and rejecting it with
`when @(!Reserved.Is(t))` instead of refusing it with `?!ReservedWord` — is not equivalent, and
the experiment proved it by accident. Read the same grammar four ways over one DDL corpus:

| build | lines differing from the hand parser |
| --- | ---: |
| `main`, no edits, over characters | 0 |
| the edits with the guard, over characters | 271 |
| the edits with the lookahead restored, over characters | 30 |
| the edits with the guard, over kinds | 27 |

`Identifier` is written `{ i: ActualIdentifier }`, and an atomic group commits to its first
reading. A lookahead refuses before anything is consumed, so the reading never happens and the
parse backtracks as it should: in `UNIQUE (a, p WITHOUT OVERLAPS)` the column list gives `p`
back and the optional period part takes `, p WITHOUT OVERLAPS`. A guard refuses after
consuming, by which time the group has committed, the failure is hard, and the backtrack never
comes. Twenty-one of those statements are lost over characters and twenty-seven over kinds.

**So over kinds a reserved word wants to be a kind, not a guard.** The lexer knows which words
§5.2 reserves; if it says so in the token, the alternative simply does not match and nothing has
to be taken back. A parser-side guard puts the decision after the commit, which is the one place
it cannot be made.

The table says something else worth keeping. The edits cost 271 lines read over characters and
27 read over kinds: **the token layer repairs what the edits break.** Reading over kinds is not
losing constructs here — it is covering for guards the character automaton needed and the token
layer does not.

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

Measured, it is not a tenth. The stand ran the same grammar over kinds against itself over
characters, seven passes a bucket, tiering off, pinned, accepted and refused apart, and lines the
two builds disagree on excluded from the timing rather than merely counted:

| corpus | over characters | over kinds | |
| --- | ---: | ---: | ---: |
| query expressions, accepted | 27.8 us | 21.3 us | −23% |
| query expressions, refused | 96.6 us | 61.9 us | −36% |
| schema statements, accepted | 13.1 us | 8.5 us | −36% |
| schema statements, refused | 27.9 us | 15.0 us | −46% |
| twenty select items | 114.2 us | 85.8 us | −25% |
| value expressions | 131 ms | 36 ms | **−72%** |

The last row was first reported as the split being four times *slower*, and it is worth keeping
why. The corpus is value expressions and it had been asked for as query expressions, so every
line was refused at its first token and the comparison was between two ways of refusing. Asked
for what it is, it reads — 1,735 lines, both builds agreeing — and it is the largest saving in
the set. The ratio against the hand parser falls from about 20x to about 5x.

**The refusal number is still worth having, relabelled.** Refusing at the first token costs 227 ns
over characters and 905 over kinds, because the token layer lexes before the grammar can say no
and the character reader stops at the first character it cannot use. That is the floor
difference, and it is where the split would lose: a workload of very short inputs refused
immediately. None of these corpora is that; somebody's might be.

A caution on precision that applies to every figure above. The handwritten side drifted by up to
30 per cent between processes on identical code — the same asymmetry the diary records, where
the small parser's number moves and the big one's does not. The generated-side differences are
23 to 72 per cent and the drift is on the other side, so the direction of every row survives it,
but no single figure here should be read to two digits.

## What this does not settle

Whether the *generated* parser can be given a token layer without changing what it accepts is a
different question from whether a handwritten one can. The two parsers agree today because the
handwritten one mirrors the generated one — including two places where the generated parser's
construction finds the parts of an introduced literal by the first period and the first quote in
the token rather than by the BNF. A token layer under the generator would have to keep those, or
they would be corrected on one side only, and the first thing to notice would be the yardstick
disagreeing.

## Moving the product over, and what it costs

Decided: `SqlStandardParser` reads over kinds. Nothing worked around — every guard the experiment
removed has to come back as something, or be shown redundant.

**Done.** `Identifier` loses its atomic braces, and `?!ReservedWord` moves into it. Removing the
braces is not enough on its own: a rule of single-element alternatives carries no trivia and stays
lexical, so the lookahead has to sit where a sequence does. Over kinds it is safe in front of all
three alternatives, since a delimited name is its own kind and no reserved word's. `GluedWord`
carries §5.2's left-hand guard, named once so the rule is reachable — an unreached lexical rule is
`GRAM4018`, an error, and the automaton would not hold the pattern. The guards on the multiplier,
the SQL language identifier, the quantifier's question mark and the simple comment's end are gone,
each because the longest match does their work.

**The introduced literal is the hard one, and it is where two known defects live.** The character
set name sits *inside* the literal's token, so §5.4's rule that a name is no reserved word has to
be applied inside the token — which a lexical pattern cannot do, since `?!` there is not regular.
The mechanism for it is the terminal the lexer begins and a rule ends (`syntax.md` §7): the rule is
a recognizer over characters, lookahead is allowed in it, and its refusal is the token's. It needs
the beginning to belong to that terminal alone, and `_` does: `IdentifierStart` is
`[\p{L} | \p{Nl}]`, so no name begins with one, and every other `_` in the lexical namespace is
inside a token — a digit separator, or the tail of an SQL language identifier.

So the literal splits: a plain one beginning at its quote, and an introduced one beginning at `_`
whose rest a rule reads. The rest **captures its parts** rather than handing the whole text over,
and that is what removes the defects:

- `Nodes.StringLiteral` finds the literal by `text.IndexOf(''')` — the first quote anywhere in the
  token, which for `_u&"'s".x'a'` is the one inside the delimited name;
- `Nodes.CharacterSet` splits the introducer at every period, including the one inside `".s"`.

Both are the same mistake: the parts were searched for in the text after the fact instead of being
read. A rule that captures `set` and `body` has them already, and neither search survives. That is
a change to what `SqlStandardParser` answers, so `HandSqlStandard` — which mirrors both on purpose
— changes in the same commit, and the two shapes get tests asserting the tree the BNF asks for
rather than the one the construction happened to make.

**What it touches, and why it is not a grammar edit.** Thirteen places name the character string
literal, and the value they receive today is the token's text. Splitting the terminal and capturing
the parts changes that shape, so the value model of the introduced literal changes with it —
`Nodes.CharacterSet`, `Nodes.StringLiteral`, and the call sites. Keeping the old construction and
splitting only the terminal would be half the work and would preserve both defects; it is not
worth having.

