# FIX engines by their model, and the two things nobody has said yet (2026-09-20)

For Igor, who asked this session to look at what already exists and find something interesting in
it. It extends rather than repeats [`fix-libraries-2026-09-19.md`](fix-libraries-2026-09-19.md),
which compares three .NET engines against ours, and
[`fix-dictionaries-2026-09-19.md`](fix-dictionaries-2026-09-19.md), which reads the two dictionary
formats and draws the build-time/run-time line. Neither looked off this platform, and the
interesting part is off this platform.

**How to read the evidence here.** Claims about a model come from the project's own documentation
as surfaced by search. **No licence below was read from a licence file**: GitHub is not reachable
from this session's fetch, only through search summaries, so every licence line is *reported and
not verified* and must be confirmed from the file before anything rests on it. Where a version
should be named, it is not — the same reason. This is stated once here rather than hedged in every
sentence.

## 1. The split that matters is not speed, it is when the dictionary is read

Three families, and ours is in the second.

**Family A — the dictionary is read at run time.** QuickFIX/n, FIX Antenna, and OnixS in its
dictionary mode. The wire becomes a map from tag to value; the dictionary then checks that map,
and — the fact finance-24 found and the one that reframed our task — in QuickFIX it also tells the
*parser* how to split repeating groups, so it is an input to reading and not only to checking.
What this family gives up is in `fix-libraries-2026-09-19.md` §4 and is not repeated here.

**Family B — the dictionary is read at build time and code is generated from it.** This is our
model, and it is not an unusual one: it is what the fastest open engines do.

- **Artio** (Real Logic, reported Apache-2.0): a `CodecGenerationTool` takes an output directory
  and the path to the XML dictionary and is run from the build — Gradle, before compilation —
  producing encoders and decoders as typed classes.
- **Fix8** (reported LGPL, with a commercial Fix8Pro beside it): "statically compiles your FIX XML
  schema"; a custom message or field means updating the schema and **recompiling**. That is the
  same sentence we would write about ourselves.
- **OnixS .NET** (commercial) ships a generator producing strongly typed message classes *beside*
  its run-time dictionary, and documents both as supported modes.

**Family C — the schema decides the wire itself.** Simple Binary Encoding, from the same body that
publishes FIX and the same authors as Artio: the schema fixes field order, so decoding is
positional and no tag is searched for at all. It is the end of the road our path starts down, and
it shows what that road costs — the wire has to be the one the schema describes.

## 2. What is interesting, ranked by what it is worth to us

### 2.1. Our path is the majority path among the fast engines, and that is worth knowing before we argue about it

The chosen shape — the dictionary as an input to generation rather than a table consulted while
parsing — is what Artio and Fix8 do, and what the commercial .NET engine offers as its typed mode.
Nobody in family B treats the dictionary as a validator bolted on afterwards. **A critic's job is
to say when the path is right**, and the evidence says it is: we are not inventing a shape, we are
joining the one the throughput engines chose.

### 2.2. The second mode is not decoration, and dropping it is a decision rather than an omission

OnixS ships *both* — generated typed classes and a run-time dictionary — and documents them as two
modes. Artio's generation is a build step the consumer runs, which means a consumer who receives a
dictionary they did not have at build time has nothing. **So the question our design has to answer
out loud is which consumer it is refusing**: the one who is handed a counterparty's dictionary at
deployment, or who must read two venues' dialects in one process, has no build-time answer. That
may be entirely the right refusal — it is the same refusal D25 already makes — but it should be
made knowingly, and the engines that kept both kept them for a reason somebody paid for.

### 2.3. The licence obligation moves to the consumer, and nothing we have written says so

This is the part I think is genuinely missed. `fix-dictionaries-2026-09-19.md` §6 settles what *we*
may read and what we must not ship, and it is right. But if the dictionary is an input to the
**consumer's** build, the file is in the **consumer's** repository and their compiler reads it.
Then:

- the obligation attaches to them, not to us — QuickFIX's specification XMLs travel under the
  QuickFIX licence, and the Trading Community's repository and Orchestra files under its own terms;
- our documentation has to say, in the page where the feature is described, that the dictionary is
  the consumer's file to have the rights to, in the same breath as it says we ship none;
- and a diagnostic that quotes a dictionary's text back — a rule name, a message name, an
  enumeration's description — puts somebody else's words in the consumer's build output. That is
  cheap to get right at the start and unpleasant to retrofit.

Nothing in the two existing documents covers the consumer's side of this, because both were written
about what we may read.

### 2.4. On this platform, nobody generates the codec inside the consumer's build

Family B's generation is always a **separate tool**: Artio's is run from Gradle, Fix8's is a
compiler you invoke, OnixS's is a tool in the distribution. A generator that reads the dictionary as
an ordinary input to the consumer's own compilation — no tool to run, no generated files to check
in, no step to forget — is the shape this repository is built around and the one nothing on .NET
appears to offer. *Searched and not found* is weaker evidence than read-and-confirmed, and it is
recorded as such; but if it holds, it is the sentence the package's front page should lead with, and
it is a better claim than any throughput number.

### 2.5. What to take from Artio specifically, if one thing is taken

Its generated decoders carry the checking with them rather than deferring it to a table walk. That
bears directly on the order of work now planned — a validation layer beside the parser, then
construction made unconditional so the modes disappear. **If the checks are generated, the modes do
not need removing later; they never appear.** Worth confirming against Artio's generated output
before it is relied on, which needs a clone this session cannot make.

## 3. What I did not find

- No .NET engine in family B inside the consumer's compilation (see 2.4, and the caveat with it).
- No engine that keeps the wire's order and repeated tags *and* reads a dictionary — the two
  families divide along exactly that line, which is why `fix-libraries-2026-09-19.md` §4.1 had
  nothing to compare against on that point.
- Nothing that answers the boundary case `fix-dictionaries-2026-09-19.md` §5 names — typed fields
  where the consumer has their own type — differently from how we would answer it.

## 4. What I would ask before more reading is done

Whether the second mode (2.2) is refused deliberately. That is a decision about who the package is
for, it is Igor's rather than a design question, and everything else here is stable whichever way
it goes.
