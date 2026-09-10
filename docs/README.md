# The documents, and what each one is allowed to say

There is a lot written down here, which is worth having and is also a hazard: two
documents can disagree, and a reader has no way to tell which one is wrong. So every
document has a role, and the role says what authority it carries and when it is expected
to be out of date.

Read this before believing any of them.

## Normative — what the language *is*

| Document | |
| --- | --- |
| [`syntax.md`](syntax.md) | The specification: the notation and its bond with C#. |

Written in the **present tense whether or not the compiler has caught up**. That is
deliberate and it is the one thing to know about it: a construct described here may not
build. It is the language's definition, not a report on the implementation — which is
what the next document is for.

## Current — what this version actually does

| Document | |
| --- | --- |
| [`status.md`](status.md) | The implementation held against the specification, rule by rule, with what is refused and why. |
| [`diagnostics.md`](diagnostics.md) | Every message the generator reports, by identifier. |
| [`ast.md`](ast.md) | The tree the SQL parsers build, and the specification each node is named from. |

These describe what is true today. A disagreement between one of these and `syntax.md`
is not a contradiction — it is the gap the pair exists to measure. A disagreement between
one of these and the code is a defect, and `ast.md` is held to that by a test that reads
it.

## How it is built

| Document | |
| --- | --- |
| [`development.md`](development.md) | Standing process: build, test, the snapshot baseline, measuring, and the Linux container. |
| [`implementation.md`](implementation.md) | The engine: how the notation is executed. A plan as much as a description, and the one most likely to lag. |
| [`visual-studio.md`](visual-studio.md) | The extension, and the `StringSyntax` annotations. |

## Design — proposals, not descriptions

| Document | |
| --- | --- |
| [`design/lexical-adt-design.md`](design/lexical-adt-design.md) | Separating the lexical and syntactic machines. Proposed, measured, and since built — the measurements are the reason it was. |
| [`design/sql-parsers.md`](design/sql-parsers.md) | The SQL parsers: independent grammars meeting in one tree, the standard's BNF as the reference, and the work that follows — a BNF-to-`.gram` converter among it. |
| [`design/visual-studio-tooling-plan.md`](design/visual-studio-tooling-plan.md) | The living checklist for the extension. |
| [`design/dsl-tooling-design.md`](design/dsl-tooling-design.md) | Tooling for an arbitrary DSL, narrowed from the handoff below into a plan. |
| [`design/DotGram_Tooling_Agent_Handoff.md`](design/DotGram_Tooling_Agent_Handoff.md) | A separate future project: IDE and LSP tooling. Not a description of anything that exists. |

Nothing here is a statement about the current compiler. Where one of them has been
built, `status.md` is what says so.

## Historical

| Document | |
| --- | --- |
| [`next.md`](next.md) | The engineering diary: what was built or found, and why the decision went the way it did. |

**Nothing in it is authoritative about the present**, and it says so itself. It is
written newest last and never revised, so an architecture, a file name or a number read
there may have been replaced the same week. It is kept because the reasoning is worth
more than the conclusion, and it is the only place a decision's alternatives survive.

It stays at the top level rather than moving under a folder that would label it: the
generator's own comments cite it seventy times, and moving it would trade seventy true
citations for a tidier tree.

## For an agent

[`../src/DotGram/SKILL.md`](../src/DotGram/SKILL.md) is how to write a grammar, written
for an agent and shipped inside the NuGet package. It carries what an agent gets wrong
rather than what a reader wants to know, and it is checked the only way that means
anything: by asking one to write a grammar knowing nothing else.
