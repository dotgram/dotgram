# FIX read like the hand parser: a design (D13)

For the architect and Igor, before any code. Numbers are in `ANATOMY.md`: of the generated
field's 131 ns, ~47 are the automaton writing its arena and ~38 the second pass reading it
back; the factory is ~20 ns in every parser; hand 51 ns/field, ideal ~33 ns/field.

## 1. The target: what `FixGrammar.g.cs` would say for `Fields`, string form

The grammar, for reference:

```
Fields : @FixField[] =
    (value: Field & end: (Separator | eof) => @(value.WithTerminator(end.Length)))*
    recover Separator => @(new FixField.Invalid(parserText, parserSpan.Start, parserMessage))
Field : @FixField =
    wire: (tag: Tag & '=' & switch @(context.Kind(tag)) {
        case 0: Text
        case 1: size: Size & Separator & dataTag: Tag & '=' & when @(context.BeginData(...)) & @ReadData
    }) => @(context.Create(tag, wire, parserSpan.Start).WithTerminator(0))
Text = (?!Separator & any)+
```

What the reader would emit, beside `IdealFixParser` line for line (`// ideal:` marks the
matching line; `Construct_*` are the helpers the generator writes today, unchanged):

```csharp
static int Read_Fields(ReadOnlySpan<char> text, int p, ref Failure failure, FixContext context, out FixField[] value)
{
    var items = Spare_DotGram<FixField>.Rent();              // ideal: new FixField[8]
    while (p < text.Length)                                   // ideal: while (p < text.Length)
    {
        var start = p;
        var q = Read_Fields_Turn(text, p, ref failure, context, out var item);
        if (q < 0)                                            // recover Separator (§8.2)
        {
            var found = text.Slice(start).IndexOf('\u0001');  // the sync, searched
            q = found < 0 ? text.Length : start + found + 1;
            item = Construct_Fields_Recover(text.Slice(start, q - start), new SourceSpan(start, q - start), Message(failure));
        }
        items.Add(item);                                      // ideal: fields[count++] = field
        p = q;
    }
    value = items.ToArrayAndReturn();                         // ideal: Array.Resize, once
    return p;
}

// One turn: `value: Field & end: (Separator | eof) => …`. Built where the turn commits.
static int Read_Fields_Turn(ReadOnlySpan<char> text, int p, ref Failure failure, FixContext context, out FixField item)
{
    item = null!;
    var q = Read_Field(text, p, ref failure, context, out var tag, out var wireEnd, out var size, out var dataTag);
    if (q < 0) return -1;
    int e;
    if (q < text.Length && text[q] == '\u0001') e = q + 1;   // ideal: p = end + 1
    else if (q == text.Length) e = q;
    else { Refuse_DotGram(ref failure, q, Expected_Separator); return -1; }
    // The turn stands from here (Replay): its constructions run now, once.
    var field = Construct_Field(new SourceSpan(p, q - p), context, text.Slice(p, wireEnd - p), tag, size, dataTag);
    item = Construct_Fields_Turn(new SourceSpan(p, e - p), field, text.Slice(q, e - q));
    return e;
}

// `Field`, recognized: captures come back as positions and scalars, nothing is built but
// what a guard asks for.
static int Read_Field(ReadOnlySpan<char> text, int p, ref Failure failure, FixContext context,
    out int tag, out int wireEnd, out int? size, out int? dataTag)
{
    size = dataTag = null; tag = 0; wireEnd = -1;
    var t = p;                                                // ideal: var start = p
    if (t >= text.Length || text[t] is < '1' or > '9') { Refuse(…); return -1; }
    t++;                                                      // ideal: digits accumulated
    while (t < text.Length && text[t] is >= '0' and <= '9') t++;
    tag = Construct_Tag(text.Slice(p, t - p));                // the guard names it: built now
    if (t >= text.Length || text[t] != '=') { Refuse(…); return -1; }   // ideal: text[p] != '='
    var q = t + 1;
    switch (Guard1(context, tag))                             // ideal: DataTag / IsData
    {
        case 0:                                               // Text = (?!Separator & any)+
        {
            var found = text.Slice(q).IndexOf('\u0001');      // ideal: IndexOf((char)1)
            var e = found < 0 ? text.Length : q + found;
            if (e == q) { Refuse(…); return -1; }             // ideal: end == value
            q = e;
            break;
        }
        case 1:                                               // length/data pair
            … Size, '\u0001', Tag, '=', Guard0 (BeginData), ReadData …
            break;
        default: Refuse(…); return -1;
    }
    wireEnd = q;
    return q;
}
```

What is gone against today's engine: the arena (13 records a field), the four passes of
the construction walk and their link lists, `Reset` and the value tables, the per-call rent
of a parser, the guard's table growth and backward search, the value run character by
character. What is left against the ideal: the `Construct_Tag` call (the author's
`FixConvert.Tag` over digits the reader just scanned), `FixContext.Create` looking for `=`
again (Finance's), `Refuse_DotGram` bookkeeping on the failure path only, and the spare list.

## 2. What stands in the generator's way, and what it needs

**Reader and `recover` (§8.2).** Today `CanDirect` refuses any rule that recovers
(`Machine.Direct.cs`: "it recovers from a bad element"), so FIX never reaches the reader. A
recovering repetition in a reader is a loop whose turn is a method: a turn that returns -1
is the broken element, and the reader searches the sync from the turn's start (the search
the engine already emits: `EmitRecoverySearch`, `PaddedDelimiter`), builds the failure value,
and goes on after the sync. §8.2's semantics carry over as they are: what a turn took is
committed when it returns; a broken turn contributes only the failure value; the furthest
refusal inside it is what `parserMessage` says.
*What a broken element had built:* nothing, if constructions run where the turn commits
(next point). A guard's side effect (`BeginData`) runs in a broken element as it does on the
engine and on the tape today — guards run while reading everywhere.

**Construction as it reads (D3 / Demand / Replay).** Replay's answer for FixGrammar: `Field`
does not *stand* on its own — the turn after it can still fail (`end` is not a separator),
and then recovery replaces the whole turn. So building `Field` where it is read (the
immediate carrier) would run `context.Create` for an element that becomes `Invalid`; Auto
rightly never picks immediate here. What does stand is the **turn**: once `end` is read,
nothing takes it back (the repetition is the whole parse and recovery only replaces turns
that fail). So the construction point is the turn's commit, not the rule's end — the
reader keeps `Field`'s captures as locals (positions and scalars, as the immediate carrier
already keeps them) and calls the factories when the turn returns. That is a new answer
Replay/Demand has to give per site: *the innermost point past which this reading stands*,
with `Stands` meaning "the rule's end" as today. Build there, and the tape's semantics hold
exactly (what the tape builds, once, for the derivation that stood); no author assertion.
Demand already answers what is asked for: `tag` is Always (the guard), `wire`, `size`,
`dataTag` Inherit, and nothing in FIX is Never.

**The guard with a built value.** `switch @(context.Kind(tag))` needs `tag` while reading.
In the reader `tag` is a local built on the spot (Demand: Always), handed to `Guard1` as an
argument — no table, no search. This is what `DirectGuard` does for the immediate carrier
already; it needs doing for a reader whose other constructions wait for the commit point.

**Streaming forms.** FIX publishes every form (`stream bytes`, `yield`). The reader is today
refused for streams (`asMethods` requires no streaming publication in the group). It needs to
read through the one input abstraction D7/Q7.5 is about — the reader over `BufferedText` /
`BufferedBytes` (`Ensure`/`Get` where the span is indexed now, a buffered `IndexOf` that
refills) — and `yield` is the turn method called once a step, which the shape above already
is. Until then the string and `byte[]` forms can move and the streams stay on the engine.

**Per-call setup.** None of it survives: no parser to rent, nothing to reset. What remains
per call is the spare list for the result (thread-static, like the other spares).

## 3. The engine

It stays, for what the reader does not take: `find`, a captured lookahead, a rule called
with arguments, left recursion the climbing cannot do, and — until the steps below land —
the stream forms. For FixGrammar, once every form is on the reader, the engine is not
emitted at all.

## 4. Code size

Today `FixGrammar.g.cs` is 588 KB: buffered/streaming engine variants 47%, the string engine
23%, construction walks 17% — 87% engine and its second pass. A reader for one publication
and one input form is a few kilobytes (three methods like the ones above, the length/data
branch, the log separator's padded variant). Twelve variants (Fields and LogFields × string,
reader, stream × parse and yield) at ~5 KB and the support types: an estimate of **120-150 KB
after the streaming step, about a quarter of today**. Before it, string forms on the reader
and streams still on the engine: slightly larger than today (both are present).

## 5. Steps, each with a number and gates

Gates at every step (D1): `HandFixParser` agrees on every input it is compared on
(Finance.Tests), the Fix44 oracle, refusal positions (`RefusalTests`), the snapshots' diff
read, `Compatibility` at C# 8, stand before/after with hand and ideal beside.

1. **The number first, no generator change.** Write the target code above by hand, as the
   generator would emit it — the same `Construct_*` helpers, `Refuse_DotGram`, the same
   failure — for the string form of `Fields`, in a scratch copy of FixGrammar, and have stand
   time it against hand, ideal and generated. It says what this design is worth before it is
   built, and whether anything in the shape (the spare list, the refusal bookkeeping) costs
   more than expected. Days, not weeks.
2. **Reader with recover** (tape carrier, constructions still from the log): FIX's string and
   `byte[]` forms leave the engine. Removes the automaton and most of the arena; the log's
   walk remains. Number: expected most of the ~47 ns.
3. **Construction at the commit point** (Replay/Demand's new answer; the reader keeps
   captures in locals and builds at the turn's commit): one pass. Number: expected most of
   the ~38 ns. Gate added: factory call counts against the tape (the CarrierDemandTests
   style), since this is where construction moves.
4. **Value runs by search** (IndexOf where the stop set is one to five characters): already
   written for the engine, local; in the reader the same.
5. **Reader over buffered input** (D7/Q7.5): the stream and yield forms leave the engine;
   then FixGrammar emits no engine and the code size is measured.

Steps 2 and 3 are the architecture; 1 decides whether they are worth it, and 4 is local.

## 6. Conditions on steps 2-5 (architect, 2026-09-18)

Steps 2-5 wait for step 1's number and Igor's word. Then:

1. `recover` in the reader is the whole of §8.2, not FIX's case alone: a recovering
   repetition that is not the whole parse tries the full continuation at every boundary
   first (the `Feed` grammar with its `Trailer` is the test), as well as the end-of-input
   case FIX is.
2. "The innermost point past which a reading stands" is one analysis, D3's, beside `Demand`
   and `Replay` in `Grammar/Model`, with its API documented in the file; every rendering that
   builds reads it, none has a check of its own.
3. A guard's side effect in a broken element (`BeginData`) runs as it does today, on the
   engine and on the tape: guards run while reading, whatever the element turns out to be.
4. For a grammar that recovers, Q7.2's quiet first reading stays off, as it is now.
