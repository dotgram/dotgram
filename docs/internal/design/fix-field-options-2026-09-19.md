# `FixFieldOptions`: one table, and the question it waits on (2026-09-19)

Three things arrived at this class today: Igor's D25 as reformulated, Igor's refinement that a
consumer's fork should be resolved once into a table rather than asked per field, and critic's
finding that the class's two halves are written to opposite rules. They converge on one change,
except for one promise that is Igor's to make. This prepares both branches of that promise and
everything that is the same either way. No code is changed yet; the one edit I began has been
reverted.

## 1. A correction first

The architect's note says a consumer who adds one counterparty pair loses "all forty-two"
standard pairs. It is **sixteen**, not forty-two. I counted three ways and they agree:
`FixSchema.DataTag` has 16 arms, `FixSchema.LengthTag` has 16, and `TypeCodes` marks exactly 16
tags with the standard type `data`. The pairs are 93/89, 90/91, 95/96, 212/213, 348/349, 350/351,
352/353, 354/355, 356/357, 358/359, 360/361, 362/363, 364/365, 445/446, 618/619 and 621/622.
The trap is real; its size is sixteen.

## 2. What the two halves do today, and what the trap actually costs

```csharp
internal int  DataTag(int tag) => _pairs == null ? FixSchema.DataTag(tag)
                                : _pairs.TryGetValue(tag, out var data) ? data : 0;

internal bool IsData (int tag) => FixSchema.IsData(tag) || _dataTags?.Contains(tag) == true;
```

`IsData` asks the standard first and the consumer second — additive. `DataTag`, the moment a
consumer supplies anything, stops asking the standard at all — replacing. The package's own
documentation says replacing ("A supplied dictionary replaces it and is copied"), so the
documented promise and half the implementation disagree with the other half.

**What it costs is worth stating precisely, because it is not silent corruption and it is not
harmless either.** Take a consumer who supplies one pair for a counterparty's custom tags and a
message carrying a standard `RawDataLength`/`RawData` (95/96):

- `Kind(95)` — `DataTag(95)` is now 0 because 95 is not in the consumer's dictionary, and
  `IsData(95)` is false because 95 is an `int` — so 95 reads as an ordinary text field.
- `Kind(96)` — `DataTag(96)` is 0, but `IsData(96)` is **true**, because that half still asks the
  standard. So `Kind` answers -1, the grammar's `switch` has no arm for it, the field fails, and
  `Fields` recovers at the next separator.

So the message is refused rather than misread — but the payload of a binary field may contain the
separator, so recovery restarts in the middle of it and the refusal arrives somewhere unrelated
to the cause. A consumer would see `FixField.Invalid` at an arbitrary offset and have no way to
connect it to the pair they added. That the two halves disagree is what keeps this from being
silent corruption; it is luck, not design.

## 3. What is the same whichever way the promise goes

Igor's refinement decides the shape, and it is independent of the semantics: **resolve the fork
once, when the options are built, into one table a tag indexes.** Then the guard on every field
is one array read — no dictionary, no set, and no branch on whether the consumer supplied
anything.

```csharp
// 0 ordinary, 1 a length tag, -1 a data tag standing where a length tag should have been.
readonly sbyte[]                 _kinds;   // the tag is the index
readonly Dictionary<int, sbyte>? _far;     // only tags past the array's end
```

`FixContext.Kind` becomes `tag <= 0 ? -1 : options.Kind(tag)`, and `options.Kind` is a bounds
check and a load. The standard's own table is 957 entries, so the array is ~1 KB; a consumer's
tags extend it up to a bound (65,536 covers every realistic FIX tag, including the 20000+ venue
range, for 64 KB built once and shared by every parse those options serve), and anything past
that bound goes in `_far`. When no pairs are supplied the options share one static table, so the
default costs no allocation at all.

`DataTag` stays a lookup, but it leaves the hot path: it is asked only by `BeginData`, which runs
only on a field that is already known to be a length tag — sixteen tags plus the consumer's, not
every field. That is the right split: what is asked per field becomes an index; what is asked per
pair can afford a dictionary.

**Both halves get one shape either way.** That is the part of critic's finding that needs no
decision: whatever "adds" or "replaces" turns out to mean, `IsData` and `DataTag` must mean it
the same way, and after the merge they are two readings of one table rather than two rules.

**A consequence for the package's documentation, and it is a contract, not a note:** the table is
built in the constructor, so changing the pairs means building new options, not mutating old
ones. That is already true — the class is immutable and documented as reusable concurrently —
but after this it is load-bearing.

## 4. Branch A — the promise becomes "adds"

The standard's sixteen pairs always hold; a consumer's pairs fill only what the standard does not
define. The trap disappears by construction: no consumer can lose a standard pair, because no
consumer can touch one.

The question this branch has to answer is what happens to a pair the consumer declares **against**
the standard — mapping a tag the standard already defines. Three possibilities, and I recommend
the first:

1. **Refuse, in the constructor, with a message naming the tag.** In the spirit of add-only: a
   declaration that cannot take effect is a mistake in the caller's data, and the earliest and
   cheapest place to say so is where they hand it over. It also converts today's silent
   replacement into a loud error for exactly the callers whose behaviour would otherwise change
   under them.
2. Accept and ignore. Cheap, and wrong for the same reason it is cheap: the caller believes
   something that is not true, and nothing ever tells them.
3. Accept and let it win. That is branch B wearing branch A's name.

The constructor already refuses a data tag whose standard type is not `data`, so it has the
vocabulary and the precedent for (1); what it would gain is a second check, that neither tag of a
supplied pair is one the standard defines. `FixSchema.TypeCode(tag) != 0` answers that in one
byte, which is the only addition the schema needs for any of this.

Cost of this branch: a consumer who today replaces the standard set on purpose loses the ability.
I can find no legitimate reason to want it — the sixteen pairs are the specification's, not a
default — but it is a real break, and it is loud, which is why it belongs before 0.2.0 rather
than after.

## 5. Branch B — "replaces" stays

If Igor wants the supplied dictionary to remain authoritative, the trap still has to go, because
it is a trap regardless of which promise is in force: nobody who adds one pair means to drop
sixteen.

The way to keep the promise and lose the trap is to make "replace" something the caller asks for
rather than something they fall into:

```csharp
public FixFieldOptions(IReadOnlyDictionary<int,int>? lengthDataPairs = null)              // adds
public FixFieldOptions(IReadOnlyDictionary<int,int> lengthDataPairs, bool replaceStandard) // says which
```

or, better named, two static factories — `FixFieldOptions.Extending(pairs)` and
`FixFieldOptions.Replacing(pairs)` — so that neither behaviour is the one you get by not
thinking. The merged table is built the same way in both; only what is written into it first
differs. And `IsData` must then follow the same rule as `DataTag`, which today it does not: under
"replaces", a supplied set replaces the standard's data tags too.

Cost of this branch: the API grows a second way of saying things, and every reader of the package
has to learn which one they have. It is also the branch where the documentation stays true.

## 6. Where else a consumer's fork is asked during a parse

The architect asked me to look, since the technique is general. I read every call in the parse
path and there is **only this one**. `FixContext` asks `options` in exactly two places —
`Kind(tag)` per field and `DataTag(tag)` per length/data pair — and both are this class. The
other settings are not forks in the parse at all: `FixParseOptions.Mode` is read by the message
layer after recognition, and `Framing` chooses which generated entry point is called before the
parse begins, which is a decision taken once by the caller and not a question asked per field.

So the technique has one subject here. If it becomes a rule, it will find its second subject in
another package rather than in this one.

## 7. What I need

Igor's answer to one question: does a supplied dictionary **add to** the standard sixteen pairs
or **replace** them? Everything else above is ready to be written either way, and section 3 is
ready to be written now, since it holds under both.
