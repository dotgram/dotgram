# Untrusted input

What a generated parser bounds when the text comes from someone else, what it does not bound
today, and what to do about the difference. Nothing here is a sandbox: a parser runs the C# the
grammar names (`=>`, `when`, recognizers of the host) with the caller's rights, and only that code.

## What holds

- **Positions do not wrap.** A position fits an `int`, compaction of a buffer included, and an input
  that would pass it is a resource error rather than a wrong position
  ([§6.3](syntax.md#63-the-input-type-picks-the-execution-mode)).
- **Deep nesting does not overflow the stack.** A rule a cycle re-enters checks the stack as it
  begins, and where the margin is gone the reading goes on, from that rule, on a stack of its own.
  The class's `Limits.MaxExtraStacks` says how many such stacks a reading may take; past that it
  fails with `InsufficientExecutionStackException`
  ([§6.5](syntax.md#65-maxextrastacks-how-deep-a-reading-may-go)). A reading over a window — a
  `TextReader` or a `Stream` — cannot move to another stack, and fails where the stack runs low.
- **A `stream` publication can bound what it holds.** The buffered `TextReader` and `Stream` forms take
  `maxRetained` per call, with the class's `Limits.MaxBufferedInputLength` as the default; input held
  past it throws `IOException`, apart from a mismatch of the grammar
  ([§6.3](syntax.md#63-the-input-type-picks-the-execution-mode)). It bounds the input retained, not
  the parser's whole memory.
- **The limits are the program's to set.** Every generated class holds them in a static `Limits`,
  an immutable `ParserLimits` that starts as what `[Gram]` wrote (`MaxExtraStacks`,
  `MaxBufferedInputLength`) and that a program changes while it runs:
  `MyParser.Limits = MyParser.Limits.WithMaxExtraStacks(4)`. A parse reads it once, as it begins,
  so setting it while other threads parse is safe, and a call's own `maxRetained` still wins
  ([§6.5](syntax.md#limits-at-run-time)). The packages say so in their own words:
  `TransactSqlParser.Limits`, `SqlStandardParser.Limits`, `Sql92Parser.Limits`, `PostgreSqlParser.Limits`
  and `PostgreSqlParser.Located.Limits` in `DotGram.Sql`,
  `ExpressionParser.Limits`, and `WebParsers.Limits` for every parser of `DotGram.Web`.
- **What a streamed parse holds is what it cannot yet let go of.** Where the generator gives a reader
  overload, it has proved that the parse needs nothing a window has moved past: in a well-formed feed
  that is about one record. The values the caller keeps of the records are the caller's.
- **Time.** The generator warns of the shapes it knows to read a refused input more than a linear
  number of times: `GRAM4051` and `GRAM4052` (exponential), `GRAM4016` and `GRAM4054` (a power of
  the nesting depth), `GRAM5016` (memory quadratic in a list's length). Silence is not a proof, as
  those messages say themselves ([diagnostics](diagnostics.md)): no option or check promises linear
  time for a grammar of yours, so test it on the refused inputs you expect.
- **The packages.** `DotGram.Finance` reads a FIX stream a message at a time, each at most the
  context's `MaxMessageLength`, 16 MiB by default, and one field at most `MaxRetained`, also 16 MiB.
  `DotGram.ExpressionLanguage` is [not a sandbox](https://github.com/dotgram/dotgram/blob/main/src/DotGram.ExpressionLanguage/README.md#it-is-not-a-sandbox):
  a text it reads names types and members, and reading it can already run code.

## What does not

Plainly, as of this version:

- **No default limit on anything.** `MaxBufferedInputLength` is `int.MaxValue` and `MaxExtraStacks`
  is `0`, no limit, unless the grammar or the program says otherwise; the packages ship with no
  limit set. A string input is in memory before the parse
  begins, and a parse into memory builds the whole result.
- **The `TextReader` overload of a plain `parse` has no bound on a record.** Its window doubles until
  the longest record fits, and there is no `maxRetained` on it: a file of one line without a line end
  is held whole. A grammar over segments reads its lines through the same window. Only the `stream`
  forms take `maxRetained`.
- **JSON has no depth limit by default.** `DotGram.Web` reads nesting as deep as the input goes, on as
  many stacks as it takes (a test reads 100,000 levels), and the memory of the tree grows with it.
  RFC 8259 §9 lets a parser set a limit; this one leaves it to the caller. `WebParsers.Limits` bounds
  the stacks, which bounds depth coarsely — a stack holds some thousands of levels, how many depending
  on the runtime — and not in levels.
- **No timeout and no cancellation.** A parse runs until it accepts or refuses.
- **A FIX stream stops at the first thing that is not a message.** `ReadMessages` throws
  `FormatException` there: it does not look for the next `8=` and go on.

## What to do

- **Bound the length before the parse.** For text in memory, check its length; for a request body,
  its declared and actual size. That bounds the time and the memory of every grammar the generator
  did not warn about, and JSON's depth with it: a level takes a character at least.
- **Read a stream with `stream` and pass `maxRetained`** where a single record could be arbitrarily
  long, or put a counting `TextReader` in front of the reader overload that refuses past a size.
- **Set `Limits.MaxExtraStacks`** — or `MaxExtraStacks` on `[Gram]` for a grammar of your own — on a
  recursive grammar that faces input it did not write, so a nest past what any real input holds fails
  rather than taking stacks for as long as there is memory. For the packages: `WebParsers.Limits`,
  `TransactSqlParser.Limits` and the like, set once at start-up.
- **Treat every `GRAM4051`, `GRAM4052`, `GRAM4016` and `GRAM4054`** in a grammar that reads untrusted
  text as a defect to fix, not a warning to silence.
- **Keep business checks out of the parser.** What it accepts is what is *written* correctly; whether
  a date exists, a total adds up or an operation is allowed is the program's
  ([Feeds](feeds.md#what-the-parser-checks-and-what-it-leaves-to-you)).
- **Run a text that is code — `DotGram.ExpressionLanguage` — where it can do no harm**: a process of
  its own, with rights you are willing to lose.
