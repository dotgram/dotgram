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

## One correction before the design

The request says the release notes already promise "a resolver of its own that a host can hand
in". **I cannot find that promise anywhere in this repository** — not in the two
`AnalyzerReleases` files, not in the VSIX notes, not in the package README or SKILL, and the
phrase itself appears nowhere. What does exist is `ISymbolResolver`, in
`src/DotGram/Grammar/ISymbolResolver.cs`: the GENERATOR's seam for `@Name` in a grammar,
implemented over Roslyn at build time. It has nothing to do with how a compiled expression finds
a member at run time.

If the promise is real and published somewhere outside this repository, the obligation is real
and this design should be read as meeting it. If it came from reading `ISymbolResolver` as EL's,
then there is no promise to keep, and the question is whether a scope is worth its price on its
own merits. **It is** — for the determinism, not for the promise — but that changes who decides
and how quickly.

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

## Caching

Every cache must be keyed by the scope, and today none of them is. There are twelve, in two
files:

- **Eight already carry the calling assembly** in their key — `_methods`, `_extensions`,
  `_constructors`, `_indexers`, `_instanceMembers`, `_staticMembers`, `_holdersInside`, `_inside`
  and `_insideNamespaces`. The assembly in the key becomes the scope. Mechanical.
- **Three are global, and are exactly the ones that depend on what is loaded**: `_types` (a full
  name to a type), `_holders` (a namespace to its static classes) and `_namespaces` (which
  namespaces exist). These are the caches the defect lives in, and each must be keyed by scope.
- **One is global and may stay so**: `_operators`, keyed by `(Type, Type)`. Both types are
  already in hand; nothing is searched for.

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

## Order of work, if it is approved

1. `ResolutionScope`, with `Around` interning by assembly.
2. The three scans and the three global caches taking it; the eight caller-keyed caches re-keyed.
3. The three entry-point overloads.
4. The determinism test, and `LoadOrderTests` taught the new expectation in the branch it already
   describes.
5. README and SKILL: what a scope is, and that the default is the caller's closure.
