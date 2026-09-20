# Validation as a layer over a built message (2026-09-20)

D53: all FIX validation becomes a layer that works on an already-built message. This answers the
four questions the architect put with it. No code is changed; the order stands — the slope first,
then the intermediate array, then this.

It closes the line the dictionary study drew: what is *read* and what is *checked* may both be
run-time tables, and what is *constructed* — a class a tag, a factory arm, a message model — stays
build time, because a type is not data. A counterparty's dictionary is therefore data a validator
loads, and neither the parse nor the construction is touched by it.

## 1. What becomes of `FixParseMode`

The mode is consulted in ten places, and they are not one thing.

**Eight are checks, and they leave.** All in `FixValidation`: `MessageEncoding` required when an
`Encoded*` field is present, group fields in schema order, a duplicate field in a scope, a
duplicate extension field, the primitive and code-set check, a required component, a required
field, and `NumInGroup` against the entries counted. Each becomes a rule of the layer.

**One is a check inside construction, and it leaves too.** `FixSemantics.TryBuild` fails on an
unknown message type in the strict mode. That is a finding about a message, so the layer reports
it, and construction stops caring.

**One is structural, and it cannot leave.** In `Reader.Scope` an unknown tag is admitted to a body
or a group entry only in the lenient mode; in the strict one it ends the scope. This does not
report a message, it *shapes* one: whether tag 9999 is a member of the body or the thing that
ended it is a different tree, not a different verdict.

**So the mode does not survive, and the structural question resolves — but not as simply as the
paragraph above first said.** If validation is a layer over a built message, construction has to
build one: a message it refuses to build is a message the layer never sees, and the consumer gets
a worse answer than today rather than a better one.

The first draft of this section made the rule "an unknown tag inside a body or a group entry
belongs to it". That covers too little, and the stand found the case while checking the agreement
of an unrelated row. A *known* tag in the wrong place is refused too, and by construction rather
than by validation: `Members` maps a group to its **counter** only, so `58 Text` is not a
body-level member of `B News` at all; `Scope` finds no membership, the lenient rescue applies only
to tags the schema does not know, and the scope ends there. `TryBuild` then sees fields left over
and fails with "Field is not permitted in this message scope" — which is one of the very findings
D53 moves to the layer. The same argument therefore applies to it: construction must stop refusing
these too.

**And that is bounded by what a scope needs in order to end.** Admitting everything
unconditionally cannot work, because then the first repeating group swallows the rest of the
message: an entry knows it has ended only by meeting a tag that does not belong to it. So the rule
has to be stated per scope, and this is the shape:

| scope | where it ends |
| --- | --- |
| header | at the first field that is not a header member — unchanged |
| trailer | at `10`, `89`, `93` — unchanged, already special-cased |
| body | nowhere: **every field between the header and the trailer belongs to the body**, known or not, permitted or not |
| group entry | at the delimiter, or at a tag that is neither a member of the entry nor of an enclosing scope — **membership stays**, because there is nothing else that can end an entry |

So construction keeps exactly one use of the schema, and it keeps it where no policy can replace
it. The body's membership check disappears and "this field is not permitted here" becomes a
finding; the group's stays and is not a check but a boundary.

Why this is not two checks in two places: after the split, construction asks *where does this
field go* and the layer asks *should it be here*. Those are different questions with different
answers, and today they are one flag because the answers happened to coincide in the strict mode.

**What it costs.** A message that the strict mode refuses today is now built in full and then
reported on — and so is one that *both* modes refuse today, which is the larger set: every message
carrying a known tag outside its place. For a consumer who parses a stream of bad messages that is one whole construction
they did not pay for before — and by measurement that is ~2,500 B a message of `FixNode[]` alone.
Cheap for a trading session, where a rejected message is an event; not free for a log-scanning
tool, where it may be most of the input. Worth naming rather than discovering.

## 2. A method on the message, or a validator object

**A validator object**, and the dictionary is why. A dictionary is loaded once and read many
times — QuickFIX's own is about a megabyte of XML — so it has to live somewhere that is not a
parameter of every call and is not a static. `FixParseOptions` is already this shape in this
package: immutable, built once, documented as reusable concurrently.

```csharp
var venue   = FixValidator.Load("FIX44-venue.xml");   // or built from our own schema
var wrong   = venue.Validate(message);                 // no findings: valid
```

`message.Validate()` with no argument would have to find a dictionary from somewhere, and the only
somewhere is a static — which is the shape D25 exists to keep out of this package.

**What it costs:** one more type for the consumer to hold, and one more thing to get wrong by
holding it per message instead of per process. The remedy is the one the package already uses:
make it immutable and say so in its first line.

**`FixValidator.Standard`** is a shared instance over the schema we compile in, so the common case
is one word and allocates nothing. See §4.

## 3. What a refusal is: every finding, not the first

**Every finding.** The consumer this layer exists for is someone reconciling a disagreement with a
counterparty, and a layer that stops at the first error makes them run it again for every
subsequent one. That is the opposite of the job.

This is a real departure from parsing, and the difference is principled rather than awkward:
parsing stops at the first error because after it the input's meaning is unknown, while a built
message is fully known and every rule can be asked independently.

```csharp
public readonly struct FixFinding      // tag, position, scope, rule, and what is wrong
public FixFinding[] Validate(FixMessage message)
```

An array, empty when the message is valid, and the empty case returns a shared empty array so a
valid message allocates nothing — which is the case that happens millions of times.

**What it costs:** a message with many findings allocates proportionally, and a caller who only
wants a yes or no pays for a list they discard. If that turns out to matter, the answer is a
second entry point that stops early — but it should be added when something needs it, not
speculatively, and the one that reports everything must stay the one with the short name.

## 4. Validating without a dictionary

**Yes, and it is not a new capability — it is the one being moved.** We compile FIX 4.4's schema
in: types a tag, ~384 code sets, components, groups and their counters, requiredness, message
composition. That is exactly the material the eight checks of §1 read today. So
`FixValidator.Standard` is the strict mode, in its new home, and a consumer migrating writes

```csharp
// was: FixMessages.Parse(wire, FixParseMode.Strict)
var message = FixMessages.Parse(wire);
var wrong   = FixValidator.Standard.Validate(message);
```

That matters beyond convenience: it means D53 loses nothing. Every check that exists today has a
place to be after the move, and the migration is mechanical rather than a judgement call.

A venue's dictionary then *adds* to that picture the same way its length/data pairs add to the
standard sixteen (D27) — the standard's meaning for a tag stands and the dictionary answers where
it is silent. Keeping the two rules the same shape is worth more than either rule on its own.

## 5. What stays where it is

**The envelope's checks are parsing, not validation.** `BeginString` first, then `BodyLength`,
`CheckSum` over the octets between: without them you cannot find where a message ends in a stream,
so they cannot wait for a message to exist. They stay in `FixMessages`, and the layer does not
repeat them.

That line is worth stating because the other engine draws it in the same place and for the same
reason — their `validate: true` checks the frame inside `FromString` and leaves the schema to a
separate call. Two libraries arriving at one split independently is the best evidence available
that the split is in the right place.

## 6. Open

1. Does `FixMessages.Parse` keep an overload that validates, for the caller who wants one call?
   It is convenient and it is also how a layer quietly becomes a mode again. I would not, at
   first, and would let someone ask.
2. What a finding says about a repeating group — the entry's index as well as the tag, presumably,
   since "tag 448 is wrong" is useless when there are nine parties.
3. Conditional rules (`StopPx` required when `OrdType` is `Stop`) are the thing a dictionary buys
   that our schema cannot express. They need an expression, and Orchestra has one. Out of scope
   here, but the finding's shape should not make them impossible to add later.
