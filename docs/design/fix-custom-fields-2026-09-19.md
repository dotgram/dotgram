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

These are not exclusive. **D** is what a consumer can do this evening, **A** or **B** is what makes
the parser do it for them, and **C** is what makes it typed with no seam at run time.

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

## 5. The opening position I would put to Igor

**A** or **B** in the options, add-only, with the message layer left alone in the first step: the
parser builds the consumer's field, the strict mode still refuses an unknown tag inside a message,
and a consumer who wants it there uses the lenient mode or builds the message from their own array.
That is one seam, nothing on the known path, and it leaves questions 2 and 3 — the ones that decide
whether our schema stays authoritative — to be answered on their own rather than by implication.
