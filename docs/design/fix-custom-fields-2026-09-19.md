# Fields a consumer defines: material for a discussion (2026-09-19)

Igor asked whether FIX can already parse fields of the consumer's own, and whether a consumer can
extend `FixField` with classes of their own; and, if not, for the material to discuss it. Nothing
here is a decision. Every fact below was read off the package at `1ea6e5f5`.

## 1. Where the package stands today

- **The hierarchy is already open.** `FixField` is a public abstract class with a `protected`
  constructor, and `FixField.Typed<T>` is a public abstract class with two `protected`
  constructors. A consumer can write `public sealed class MyLimit(decimal v) :
  FixField.Typed<decimal>(5001, v);` today and it compiles.
- **Nothing can make the parser produce it.** Every concrete field the package declares is
  `sealed`, `FixField.Unknown`'s constructor is `internal`, and the one place a tag becomes an
  object — `FixFieldFactory.Value`, a dense switch over 912 tags whose default is
  `new FixField.Unknown(tag, …)` — is an internal static class with no seam of any kind. The
  package has no virtual member, no delegate, no callback and no partial method anywhere.
- **A tag nobody knows is read, not refused.** The grammar is tag-agnostic: it asks the context
  only whether the tag begins a length/data pair, and otherwise reads the value to the separator.
  So 5000-9999 and 20000+ parse, and come back as `FixField.Unknown` carrying the octets.
- **What is configurable is data, not behaviour.** `FixFieldOptions` takes one dictionary of
  length/data pairs — and replaces the standard sixteen rather than extending them.
- **The message layer is stricter than the parser.** An unknown tag is admitted to a body or a
  group entry only in the lenient mode; the strict mode rejects it, because the schema's type
  table answers nothing for it and the primitive check fails on a null type.
- **One door is already open in the other direction.** `FixMessages.Build`/`TryBuild` accept a
  `FixField[]` the caller assembled. Their checks read only base members, so a consumer's own
  subclass, with `Locate` called correctly, does reach `FixNode.TypedValue` and comes back out of
  `message.GetField(tag)?.TypedValue`. It is still subject to the strict mode's rejection.

So the question is not "can a consumer declare a field" — they can — but "can the parser be told to
build it, and will the message layer then treat the tag as known".

## 2. What a design has to preserve

- **The speed just bought.** FIX reads a string at 48.9 ns a field, below the hand-written parser
  and at 1.04x of the ideal reader we built as a floor. Anything that adds work to a *known* tag is
  refused by that alone.
- **The forms.** A field is built from a `ReadOnlySpan<char>`, from a `ReadOnlySpan<byte>`, and a
  binary pair from its data tag. Whatever a consumer supplies has to answer in all three, or the
  forms diverge — the defect we have just spent a day removing elsewhere.
- **D5.** Nothing may retain the input. A builder handed a span cannot store it, which the
  signature itself enforces.
- **The maintenance rule of the package.** Field declarations, the factory, the schema tables and
  the message models are kept in lockstep by hand. A seam that lets a consumer's table drift from
  ours is a seam that produces two answers for one tag.

## 3. Four shapes, with what each costs

**A. A builder table in the options.** `FixFieldOptions` gains a map from tag to a builder, and the
factory's *default* arm consults it — the arm reached only by a tag the package does not know. A
custom delegate type takes the span (`delegate FixField FixFieldBuilder(int tag,
ReadOnlySpan<char> value)`, and a byte twin), so nothing can retain the input.
*Cost:* nothing on a known tag, the dense switch being untouched; one lookup and one delegate call
on an unknown one, where today there is an allocation anyway. *Limit:* a consumer cannot replace a
tag the package knows, which may be a feature rather than a limit (§4).

**B. One interface instead of a table.** The options carry an object with three methods — text,
bytes, binary — and the factory's default arm asks it. The same cost, one seam instead of three,
and the consumer keeps their own dispatch, which for a hundred custom tags is their switch rather
than our dictionary.

**C. The consumer's fields are generated.** The user writes their tags in a grammar, or in a table,
and our generator emits their factory and their field classes beside ours, as `Fix44` is emitted
today. *Cost:* nothing at run time, and the fields are typed, sealed and documented. *Price:* it is
a feature of the generator rather than of the package, and it serves only consumers who can run the
generator over a definition of their own.

**D. Nothing in the parser; a pass afterwards.** Document that a consumer subclasses
`FixField.Typed<T>`, walks the array and replaces the unknown fields they care about before calling
`Build`. *Cost:* a second pass over the fields, and the consumer writes the conversion themselves.
*Price:* nothing in the package — it works today, and owes one paragraph of documentation.

**E. A class the consumer inherits.** The package publishes one class — say `FixFields` — whose
members answer the questions the package answers for its own tags, each with the present answer as
its default:

```csharp
public class FixFields
{
    public static FixFields Standard { get; }

    protected virtual FixField Custom(int tag, ReadOnlySpan<char> value);   // now: the fallback field
    protected virtual FixField Custom(int tag, ReadOnlySpan<byte> value);
    protected virtual FixField Custom(int tag, FixBinaryValue value);
    // and, if question 2 is answered yes, later and without new API:
    // protected virtual string? TypeOf(int tag);   // now: the schema's table
    // protected virtual int      DataTag(int tag); // now: the sixteen standard pairs
}
```

A consumer writes `sealed class MyFields : FixFields`, overrides the three members with a switch of
their own and calls `base` for what they do not claim, and hands an instance to the options. The
package's own switch over its 912 tags stays where it is, static and sealed; the instance is
consulted only where that switch falls through — so a known tag pays nothing, and an unknown one
pays a null check and one virtual call, against an allocation it already pays.

*What inheritance buys over the table and the interface.* The fallback is the language's:
`base.Custom(tag, value)` is how a consumer says "the rest is yours", where a table needs a
convention for absence and an interface needs the consumer to hold our default themselves. One
object carries state — their own schema, their own tables — instead of a delegate per tag. And it
is the only shape that can answer question 2 later *without a second seam*: the day the strict
mode has to accept a consumer's tag, `TypeOf` is one more virtual member on the class that is
already there, rather than a new option beside the old one.

*What it costs.* A protected surface is API: every member we add is a promise, and every member we
change is a consumer's override that no longer overrides what it did. An interface has the same
problem in a harsher form (a new member breaks every implementer), a table has it least. The class
must be constructible by us for the standard case and by them for theirs, so its constructor is
public and its default instance is ours. And it invites the question the table does not: if
`Unknown` is virtual, why not `Text`, why not `Kind` — the answer has to be written down, or the
seam widens by itself until the hot path is virtual.

*Against C.* Where the consumer can run the generator, **C** gives typed sealed classes and no
seam at all; **E** is for the consumer who has a jar of private tags and a NuGet reference.

These are not exclusive. **D** is what a consumer can do this evening, **A**, **B** or **E** is what
makes the parser do it for them, and **C** is what makes it typed with no seam at run time.

| | how a consumer says "the rest is yours" | state | answers question 2 later | new members break |
| --- | --- | --- | --- | --- |
| A, a table | absent from the table | a delegate a tag | a second option | nothing |
| B, an interface | returns null | their object | a second interface, or a breaking member | every implementer |
| E, a base class | `base.Custom(…)` | their object | one more virtual member | only an override of that member |

### The name

Igor's: `Custom` rather than `Unknown`. The package already speaks that way — a message whose type
is not one of the standard 93 comes back as `CustomFixMessage`, built by us, for a type nobody
declared. So `Custom` means in this package "outside the standard set", whoever ends up building
it, and the hook and the fallback are the same thing seen from two sides: the consumer's override
builds their custom field, and the default builds ours. Then the field class is `FixField.Custom`
and `FixField.Unknown` goes with it, which also removes a smaller oddity — today a tag in a
consumer's own 5000-range, which they know perfectly well, is handed back under a name that says
nobody knows it. It costs a rename through the package's documentation (the README and the skill
name `FixField.Unknown` in four places) and it breaks consumers, which at this stage is free.
The alternative is to keep `FixField.Unknown` for the fallback and call only the hook `Custom`,
which reads honestly — unknown is what nobody defined, custom is what you did — at the price of
two words for one shape and of disagreeing with `CustomFixMessage`.

## 4. The questions to settle before any of it is built

1. **Add only, or override?** May a consumer's table claim a tag the package already knows — tag 1,
   say, with a type of their own? Overriding makes our schema advisory, and two readers of the same
   wire then disagree. Refusing it keeps one answer per tag, and costs the consumer who has a
   private meaning for a standard tag.
2. **Does a custom tag become *known* to the message layer?** Today the strict mode rejects what the
   schema does not type. If a consumer declares tag 5001 as a decimal, does strict accept it,
   validate its primitive and let it into a group entry? If yes, the schema's type table needs the
   same seam as the factory, and duplicate detection and group membership follow it.
3. **What happens when the specification catches up?** A consumer claims 5001 and a later version of
   FIX defines it. Does their table still win, does ours, and is the collision a diagnostic?
4. **How far does a custom field reach?** The field array only, or the typed message models too —
   which are generated from the specification and switch on standard tags alone.
5. **Which version promises it?** A seam in the options is public API, and this package's surface
   has just been replaced once (D19). If it is coming, it is cheaper to shape it before 0.2.0 is
   cut than after.
6. **Is a binary pair of the consumer's own in scope?** A length/data pair can already be
   configured; a *typed* custom binary field needs the same builder to be asked for the data tag.

## 5. Igor's decision, 2026-09-19, and its revision the same evening

**Settled the same evening, after the revision below: the shape is E, and the consumer's side
is their program.** Igor drew it: a big switch over tags; the tag that falls through reaches the
default arm, and there a question — is this tag's field binary — opens two ways of building it;
and all of that is in the consumer's code. So the package's switch over its 912 tags is untouched
and its default arm gains one seam, and `IsData(tag)` is a question the consumer's code puts to
us from inside their own default arm, not one the parser puts to them while reading. D25 holds by
the direction of the arrow. Framing is the exception and is folded into one table when the
options are built, where the consumer's pairs *extend* the standard sixteen. All of this is D27
in the decisions journal, which supersedes both sections below; they are kept for their reasoning.

### The revision this replaced

**Revised by D25: the generator decides how to read, and the parser is a machine that does as it
is told.** Asking a consumer's object, while parsing, whether a tag's field is binary is the
parser choosing how to read, and it is out — and with it the shape below, which was approved an
hour earlier. What replaces it is declaration: a consumer's tags are declared where a grammar is
declared, the generator builds their arms and their part of the kind table, and nothing is asked
of anybody at run time. That is option **C** above, which this document had costed as the
heaviest and the only one with no seam while parsing. Inheritance stays in it, carrying the types
and the constructions a consumer writes — not the questions the parser would have asked. The open
question it inherits is how a consumer declares: a grammar of their own, a table beside it, or
attributes on their own classes; and §4's questions 2 to 6 stand unchanged.

The shape it replaces, kept because the reasoning in it is still the reasoning for the parts that
survive:


**The inheritance shape (E), tags added and never overridden, with one more member: is this tag's
field binary.** So the class the consumer inherits answers two questions rather than one — what to
build for a tag the package does not know, and whether that tag begins a length-and-data pair —
and both defaults are the package's present answers.

What the second member changes, and why it has to be designed rather than added: the framing
question is asked *before* the value is read, on every field, through the context's `Kind`. So the
rule that keeps the first member free must hold for it too — the package answers from its own
tables first, and the consumer's object is asked only where those answer nothing. A known tag then
pays what it pays today; an unknown one pays one virtual call before its value is read.

It also decides what becomes of `FixFieldOptions`'s dictionary of pairs, which today replaces the
standard sixteen rather than extending them. Either the dictionary stays as the simple way to say
the same thing and the class is the general one, or the class subsumes it and the dictionary
becomes a convenience built on it. Add-only makes the second easier to explain: nothing a
consumer supplies can take a tag the package already knows, whether it is a type or a pair.

Not started, and not urgent. The design goes to finance-24 as the package's owner when there is
room for it; §4's questions 2 to 6 are still open, and question 1 is now answered.

## 6. The position this replaces

**E** in the options — a class with our answers as its defaults — add-only, with the message layer left alone in the first step: the
parser builds the consumer's field, the strict mode still refuses an unknown tag inside a message,
and a consumer who wants it there uses the lenient mode or builds the message from their own array.
That is one seam, nothing on the known path, and it leaves questions 2 and 3 — the ones that decide
whether our schema stays authoritative — to be answered on their own rather than by implication.
