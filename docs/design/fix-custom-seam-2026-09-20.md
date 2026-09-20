# The seam in the `default` arm (2026-09-20)

Igor's form for fields a consumer defines: a big switch over tags, a tag that matches no arm
falls to `default`, and there the question "is this binary" is asked and there are two branches
of construction — **and all of that is in the consumer's code**. They write their own switch; we
do not hold a directory of theirs. Our switch over 912 tags stays exactly what it is. Only its
`default` arm changes: instead of always building a spare field, it hands the tag and the value
to the consumer's object, if one was supplied.

The architect asked for the design before the code, and said he cares about exactly one place —
that arm — and the name. So this is short, and the part that is not obvious is where the arm
actually is.

## 1. The arm is thirty-three places, not one

`FixFieldFactory` dispatches in two levels. `Value(tag, span)` switches on `tag / 64` into fifteen
`PartN` methods, and each `PartN` switches on the tag within its block of 64. **Both levels have
a default**, because a tag inside a block that the standard does not define — say 700 — falls to
that block's own `default`, not to the outer one. Counting the two spans and the binary form,
there are **33 sites** of `new FixField.Unknown(tag, …)` today.

That is the first thing the design has to answer, and it decides the rest:

- The 33 sites become one call, `Custom(tag, value)`, which is where the seam lives. Less
  duplication than today, and one place to read.
- But `PartN` is `static` and has no way to reach a consumer's object, so **the object has to be
  threaded as a parameter through the two levels**: `Value(int tag, ReadOnlySpan<char> value,
  FixFieldOptions options)` and the same for the byte form and for `Binary`. Thirty-two
  signatures, mechanical, and free at run time — a reference travels in a register, and a known
  tag never looks at it.

The alternative I rejected: have `PartN` return null for a tag it does not know and let the outer
method deal with it. That puts a null check on **every** field, including the 912 known ones, to
save an argument. The whole point of the shape is that a known tag pays nothing.

## 2. Where the seam hangs

On `FixFieldOptions`, the object a parse already carries. It is the consumer's statement about
tags outside the standard — which of them are binary, and now how to build them — and both halves
of that statement are used in the same place at the same moment.

The alternative is a new parameter on the dozen `FixParser.Parse`/`ParseLog` overloads and on
`FixParseOptions`. It separates "what a tag is" from "what to build for it", which is tidier on
paper, and it costs an API change out of proportion to the distinction. I would not.

## 3. The shape of the consumer's object

```csharp
public abstract class FixCustomFields
{
    public abstract FixField Text  (int tag, ReadOnlySpan<char> value);
    public abstract FixField Text  (int tag, ReadOnlySpan<byte> value);
    public abstract FixField Binary(int tag, ReadOnlyMemory<byte> value);
}
```

Three methods because a field is built three ways, and **a consumer who answers only one of them
gets forms that disagree** — the defect we spent a day removing elsewhere. They are abstract
rather than virtual so that the compiler asks the question instead of the consumer discovering it
in production.

Each returns a field rather than null-for-not-mine, so the arm is one virtual call and not a call
plus a test. A consumer whose switch does not recognise a tag calls a `protected` helper that
builds what we build today, so falling through is one line and looks like falling through.

`ReadOnlySpan` in two of the three enforces D5 by its type: a builder cannot store what it is
handed. The binary form takes `ReadOnlyMemory` because that is what a binary pair already is.

Cost, and it is what the architect described: a known tag pays **nothing** — its arm is untouched
and the extra argument is never read. An unknown tag pays a null check and one virtual call, at a
point where a field is being allocated anyway.

## 4. The name, and what renaming costs

Igor's name is `Custom`. So `FixField.Unknown` becomes `FixField.Custom`, and the seam is
`FixCustomFields` on `FixFieldOptions.Custom`.

It is the right name for the seam. For the class it is worth one question, because the class means
two things now: a tag **nobody** declared, which we build ourselves, and a tag the **consumer**
declared, which they build. "Custom" fits the second and is a stretch for the first — a tag in a
message from a counterparty who told us nothing is not custom, it is unknown. Three ways out:

1. `Custom` for both, as Igor said. One name, and the first case is read as "not one of ours".
2. `Custom` for the seam, `Unknown` stays the class. Two names that are honest and one more thing
   to explain.
3. `Custom` the class, and a consumer's own subclass is theirs anyway — which is (1) with the
   observation that the second case never actually produces our class at all, because if they
   declared the tag they returned their own field for it.

**(3) is (1) once you notice that the two meanings never collide in practice**, and I would take
it. Recommending it rather than deciding it: the name is Igor's.

Renaming a public class is a break. `FixField.Unknown` is named in six places outside the package
in this repository — three test files and the generated Fix44 grammars — and by any consumer who
has matched on it. 0.2.0 is not out, so this is the last moment it is free; after it, the same
rename costs an obsolete alias and a major version.

## 5. What this does not do

It does not give a consumer typed **message models** for venue messages, nor schema entries, nor
validation. A custom field built this way is still rejected by the strict mode, because the schema
answers nothing for its tag and the primitive check fails on a null type. That is the next
question and not this one — and it is where `docs/design/fix-dictionaries-2026-09-19.md` lands,
since a dictionary read at build time is what could generate the schema entries too.

Worth saying plainly so nobody expects more from this seam than it gives: **this lets a consumer
build their own field objects; it does not make their tags known to the message layer.**

## 6. Open

1. The name of the class — (1), (2) or (3) of §4. Igor's.
2. Is threading `FixFieldOptions` through 32 factory signatures acceptable, or is the seam better
   placed on a new parameter after all? I recommend threading; it is mechanical and costs nothing
   at run time.
3. Should the strict mode admit a tag the consumer declared, on the grounds that they have said
   what it is? That is §5's question and I would keep it out of this change.
