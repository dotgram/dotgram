# A resolution scope for the expression language

**Status: a proposal.** Nothing here is in the compiler. Written 2026-09-25 for the architect
and then Igor, because it is public surface.

## What is being asked

Resolution in `DotGram.ExpressionLanguage` depends on what the process happens to have loaded.
`Names.cs` walks `AppDomain.CurrentDomain.GetAssemblies()` in three places — `Search` (a type by
its full name), `Held` (the static classes of a namespace, which is where extension methods
live) and `Gather` (which namespaces exist at all). A host that hands the same text to the same
call twice can get two answers, if something loaded in between.

That is not hypothetical. It is the defect that cost a day on 2026-09-25: `GeneratorDriverTests`
compiles classes into the GLOBAL namespace and loads them, and from that moment a text naming
`Spans` meant something else. Four rows of one test failed, but only when that other test had run
first, so it read as a property of the worktree.

## The promise this has to keep, and the tension in it

The promise is real and I first reported that it was not. It is in
`src/DotGram.ExpressionLanguage/DotGram.ExpressionLanguage.csproj`, in `PackageReleaseNotes`:

> a resolver of its own that a host can hand in, so what a name means is the host's to decide.

I said it was nowhere in the repository. It is, and my search is why I missed it: I grepped
`--include=*.md --include=*.txt --include=*.cs`, and release notes live in a `.csproj`. A
negative result is only ever as good as the filter that produced it, and I stated it without
one word of hedging.

**Nothing false has shipped.** `Version` is 0.1.0 and `v0.1.0` is tagged, and the note opens
"Since 0.1.0" {D} so it describes the NEXT release. The promise is not yet broken; it is not yet
kept, and it will be published the moment 0.2 is.

**But it does not describe what is designed below, and that has to be said before Igor reads on.**
"What a name means is the host's to decide" is what an INTERFACE gives: the host answers which
member a name is. A `ResolutionScope` deliberately does not give that. With it, what a name means
stays C#'s and stays ours; what the host decides is WHERE it is looked for. That is a smaller
promise and a better one, for the reasons under the next heading {D} but it is not the sentence.

So one of two things has to happen before 0.2 ships, and the choice is Igor's:

* **the scope lands and the sentence changes** to say what it is — "a resolution scope a host
  hands in, so where a name is looked for is the host's to decide" — which is my recommendation;
* **or the interface is built as written**, and the design below is the wrong answer to the
  promise rather than a refinement of it.

What must not happen is the scope landing under the old sentence. A consumer reading it would
expect to decide what a name means, find they can only decide where it is sought, and be right to
call that a broken promise.

## The shape: an object, not an interface

A sealed immutable `ResolutionScope`, holding **where to look** and nothing about **how to
look**.

```csharp
public sealed class ResolutionScope
{
    public static ResolutionScope Around(Assembly caller);        // the caller and what it references
    public static ResolutionScope Of(Assembly caller, params Assembly[] assemblies);

    public ResolutionScope With(params Assembly[] more);          // a new scope; this one is unchanged
    public ResolutionScope WithoutInternals();                    // the caller's internals hidden
}
```

**Why not an interface.** An interface hands the host the SEMANTICS: which of two types a bare
name means, how a dotted name is cut into a prefix and nested types, when an internal type is
nameable, what makes a class an extension holder. Those are C#'s rules, this language's whole
contract is to follow them, and they live in four methods that took two defects this week to get
right — the precedence fix in `4813268b` among them. An implementation that got any of them
subtly wrong would not fail loudly; it would change what a text MEANS. A scope restricts the
search; the search stays ours.

An object is also what the caching answer needs: it can be compared by identity.

**Why the calling assembly stays in it.** Internal visibility is part of where to look, not a
second parameter: a scope says both which assemblies are searched and whose internals answer.

**What I would leave out.** Namespaces. A text's `using`s are its own — the SKILL says so — and a
scope that also imported namespaces would give one text two sources of imports. A scope that
RESTRICTED which namespaces are visible is a different feature, and it can be added later
without changing this shape.

## How the entry points take it

Each of the three public calls has a form taking `Assembly` today. Add a third form taking a
`ResolutionScope`, and leave the other two as they are:

```csharp
LambdaExpression        Parse   (string text, ResolutionScope scope);
Match<LambdaExpression> TryParse(string text, ResolutionScope scope);
TDelegate               Compile<TDelegate>(string text, ResolutionScope scope);
```

The `Assembly` forms become `ResolutionScope.Around(caller)` or today's behaviour, depending on
the default below. The no-argument forms keep asking the stack who called.

## The default, and what each one costs — measured, not guessed

**(a) Today's behaviour**: everything the AppDomain has loaded, at the moment of the call.
Breaks nothing. Keeps the defect: the same text can resolve differently depending on what ran
before.

**(b) The calling assembly's reference closure**: the caller and what it references, walked
transitively.

I prototyped (b) — the three scans reading a per-caller closure instead of the AppDomain, the
three global caches re-keyed by caller — and ran the suite. **It breaks exactly one test of
9,589**: `LoadOrderTests.What_a_text_can_name_is_what_the_probe_recorded`, the witness written
for this very defect, which says in its own failure message:

> CHANGED: a name from an untouched assembly now resolves. If the reader was given the
> retry-after-loading-references behaviour, this mode should now expect success at step 2, and
> this branch is the reminder to change it.

So the only thing (b) breaks is the test whose job is to notice (b). The prototype is not
committed.

**I recommend (b).** A default that depends on what else is in the process is not a default a
consumer can reason about, and the closure is what a C# developer would expect: what my assembly
references is what my code can name. Two costs, stated plainly: it LOADS the referenced
assemblies, so a parse gains that side effect once per caller; and a host that names a type from
an assembly it does not reference must now say so with a scope. The second is a behaviour change
in a package before 0.2, which is the right side of a version boundary to make it.

**What the first of those costs, measured 2026-09-26 and not before.** This section is headed
"measured, not guessed" and that cost was the one thing in it stated without a number. It is
23 ms and 145 assemblies over the benchmark assembly (12 in the process before the call, 157
after); the second call is 0.02 ms, being the same instance. On the stand's first-call rows it is
the whole of the rise between `5f4fbb9f` and the scope: `el/floor` 21.0 ms to 40.7, `el/ladder`
24.8 to 43.7, `el/block` 27.5 to 47.4, on both carriers and on two runs an hour apart, while the
methods the runtime compiled went DOWN (192 to 165 for `el/floor`) — loading, not compiling.

It is paid once per assembly per process, so a host that reads many texts never pays it again.
Where it is felt is a short process that reads one short text: there it roughly doubles the first
reading.

**This leaves a question that is Igor's.** A scope's answer is determined by the closure's NAMES,
not by what has been loaded, so the loading could be deferred to where a name is looked for
without making an answer depend on the process again. That would be a different default, not a
different guarantee. Nothing here proposes it; the number is recorded so the choice is made with
it rather than without it.

## Caching

Every cache must be keyed by the scope, and today none of them is. There are **thirteen** pieces
of cached state in two files — twelve dictionaries and one plain field — and they fall in three
groups:

- **Nine already carry the calling assembly** in their key: `_methods`, `_extensions`,
  `_constructors`, `_indexers`, `_instanceMembers` and `_staticMembers` in `Caches.cs`, and
  `_holdersInside`, `_inside` and `_insideNamespaces` in `Names.cs`. The assembly in the key
  becomes the scope. Mechanical.
- **Three are global, and are exactly the ones that depend on what is loaded**: `_types` (a full
  name to a type), `_holders` (a namespace to its static classes) and `_namespaces` (which
  namespaces exist at all — the field, not a dictionary). These are where the defect lives, and
  each must be keyed by scope.
- **One is global and may stay so**: `_operators`, keyed by `(Type, Type)`. Both types are
  already in hand; nothing is searched for.

Nine and three and one is thirteen, which is the whole of it — an earlier draft of this section
said twelve and eight, and listed nine names under the eight.

`Loaded`'s static constructor subscribes to `AssemblyLoad` and clears the three global caches on
every load. For an explicit scope that subscription can go, which is the point: a scope's answers
cannot change under it. It stays for the ambient default, if (a) is kept.

A scope must therefore work as a dictionary key. Reference identity is enough if a scope is
immutable and a host keeps its own, and that is the usage to document — with `Around(assembly)`
returning the SAME instance for the same assembly, so the common path does not make a new key on
every call.

## Determinism, and the test that proves it

Buildable today, because every piece already exists:

1. take a scope over a fixed set of assemblies;
2. resolve a text naming a type in one of them, and keep the answer;
3. compile a colliding type into the AppDomain and load it — `GeneratorDriverTests` already does
   exactly this, and it is what caused the `Spans` flake;
4. resolve the same text through the same scope; the answer must be identical.

The same pair through the ambient default must show the answer CHANGING, or the first half proves
nothing. That is the whole claim: an explicit scope is deterministic and the ambient one is not.

## What this does not fix

A scope narrows what is searched. It does not change C#'s shadowing: a type in the global
namespace still beats one a `using` brings in, within whatever the scope contains. That is C#'s
rule, it landed in `4813268b`, and a host that puts two same-named types in one scope gets the C#
answer rather than an error.

## Order of work: built, 2026-09-26

All five, in `8b1a41a2`. Written here as done so that nobody reads this page as a proposal that
is still waiting:

1. `ResolutionScope`, with `Around` interning by assembly.
2. The three scans and the three global caches taking it; the eight caller-keyed caches re-keyed.
3. The three entry-point overloads.
4. The determinism test (`ScopeDeterminismTests`), and `LoadOrderTests` taught the new expectation
   in the branch it already describes — it runs in a process of its own and now requires the
   answer NOT to move with load order.
5. README and SKILL: what a scope is, and that the default is the caller's closure.

What it did NOT settle is what this page already said it would not, and what the plan leaves to
Igor: `using static` and aliases (a `using` names a namespace and nothing else), and which
namespaces, if any, are imported by default. The name rules around it landed beside it — CS0104
for two `using`s giving one name, a name resolving as written beating one a `using` brings in
(`4813268b`, checked against Roslyn), and a simple name that binds to a local never asked of the
type tables at all (`723b7af8`).
