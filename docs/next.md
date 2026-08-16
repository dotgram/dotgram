# Where this is, and what comes next

A working note rather than documentation: enough to pick the project up on another
machine without reading the whole history. [`status.md`](status.md) says what is built,
[`syntax.md`](syntax.md) says what the language is, [`development.md`](development.md)
says how it is built and checked; this says what to do next, and why in that order.

## State

Everything is on `main`; the build is clean and 712 tests pass. No snapshot has moved
since the restructuring began, which is the standing check that a change altered the
shape and not the output — [`development.md`](development.md) says how that check is run
and what else is owed by a change.

## Next, in order

**1. §8.3 over a streamed parse.** The one gap with no workaround. A `recover` without a
`=>` reports a bad element through the `partial void OnRecovered` hook, and the streaming
driver has no such channel at all — so a feed read from a `TextReader` can step over a
bad record but cannot say which one. That is exactly the case streaming exists for: a
million records, one of them wrong, and the reader needs to know. Part design: what
"report" means when records are handed over one at a time, and whether the hook fires
before or after the element that replaced the broken one.

**2. Indirect left recursion.** `A = B & x`, `B = A & y`. Refused by the normalizer. The
direct form is built and rewritten into a fold; the indirect one needs the cycle found
across rules first. Rare in practice, and the message says so clearly, which is why it
sits below §8.3.

**3. Incremental parsing.** The largest, and not worth starting without deciding what is
incremental — the parse of an edited input, or the generation of an edited grammar. The
second is already largely there (`Questions`/`Answers`); the first is a different project.

**Not worth doing: a second `recover` in one rule.** Refused today, and the diagnostic
tells the author to give the other run its own rule. That workaround was measured against
a real grammar and compiles cleanly — two recovering runs in two rules, called from one —
and it reads better than the original, because each section gains a name. A
convenience, not a gap.

## Held for their own sessions

**Documentation.** To be done deliberately, not in passing. README is ~16 KB and has
become introduction, tutorial, reference, status and rationale at once — it is the first
thing to go stale, and twice already has. The `status.md` table is at 93 rows and some
describe one mechanism from two sides.

**Performance.** Two ideas worth measuring together, both about the backtracking stack:

- one large array used as a stack, rather than a `stackalloc` per recognizer frame — this
  removes both the per-frame cost and the depth limit in one move, and is the better idea
  of the two;
- a depth counter: past a threshold no ordinary grammar reaches, take the buffer from the
  heap. Measured to be worth roughly 4562 → tens of thousands of nesting levels, at the
  price of a parameter on every recognizer.

Already measured and recorded in `status.md`: 48 slots beats 32 and 24, because a smaller
buffer makes `Grow` run on ordinary matches and doubles allocation.
`TryEnsureSufficientExecutionStack` does **not** work as the condition — it answers yes
until the last few hundred frames.

**Tidying.** What is left of the structural review: `Recovery` and `Fold` live in
`CaptureLayout.cs` for historical reasons; `Machine.cs` is ~1600 lines but has one job.

## Findings worth not rediscovering

- A parameter cannot be used inside an element set — `Until(sep) = [^ sep]*` is refused,
  because a parameter names a piece of grammar and a set holds characters and elementary
  rules. One rule per separator instead.
- There is no case-insensitive literal. The SQL guard spells keywords a letter pair at a
  time (`S & E & L & E & C & T`), which is honest but verbose — a candidate feature, and a
  language decision rather than an example's problem.
- `Trivia` is inserted between the operands of a sequence and not between the iterations
  of a repetition, deliberately: a repetition is how a lexeme is written, and inserting
  there would make `1 2` one number. A spaced list names `Trivia` itself. §4.5 says so now.
- An unterminated `/*` in the SQL guard is allowed, and that is correct rather than a
  hole: the comment rule needs its terminator, so what follows is read as ordinary tokens
  and any writing word among them is refused.
- Writing grammars finds defects that reading the compiler does not. JSON found two, the
  typed CSV three, XML two limits; Markdown, HTTP headers and FIX found none, which is
  what saturation looks like.

## Examples, and what each is for

The set now covers, one class of problem each: URL (alternatives, `find`), three
calculators and two tree builders (precedence, associativity, binding powers), four feeds
(records, `recover`, streaming, the §8.3 hook), JSON (recursion), XML (a `where` comparing
two captures), Markdown (line-oriented blocks), typed CSV (§7.3 both ways), INI and HTTP
headers (a lookup rather than a tree), the SQL read-only guard (lexis as proof),
fixed-width (counting rather than delimiters), netstrings (§7.1, the shape a grammar
cannot express), the filter language (heterogeneous literals, an AST), and FIX (ordered
fields, arithmetic over the match).

**YAML was the last of the shortlist, and the reconnaissance is done.** The border is
exactly where it looked:

- *Fixed* depth is expressible by writing the levels out — `L0`, `L1`, `L2`, each with
  its own indent literal. Compiles and works, and is what a config format with two levels
  actually needs.
- *Arbitrary* depth is not. It needs the indent of this line compared against the indent
  of the last one, which is a value carried from one place in the input into the shape of
  another — the same wall netstrings hit.
- And unlike netstrings, §7.1 does not get round it. An external recognizer is handed the
  input and a position and nothing else; a stack of open indent levels has nowhere to
  live between calls, and putting it in a static would make the parser stateful and
  unreentrant, which is a worse trade than not supporting YAML.

So YAML is not an example this project should carry. What it is, is the clearest
statement of the one thing the language cannot do: **carry state across a parse**.
Anything indentation-sensitive — YAML, Python, reStructuredText — is out of reach for the
same reason, and the reason is worth knowing rather than rediscovering.
