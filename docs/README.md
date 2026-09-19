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
| [`coverage.md`](coverage.md) | How much of Microsoft's T-SQL reference the T-SQL grammar reads, page by page, as SQL Server answers every example of it at every level. |
| [`carriers.md`](carriers.md) | Which carrier `Auto` took for every grammar of the solution, and for one on the tape, the gate that kept it and each rule held there with its cause. |

These describe what is true today. A disagreement between one of these and `syntax.md`
is not a contradiction — it is the gap the pair exists to measure. A disagreement between
one of these and the code is a defect, and `ast.md` is held to that by a test that reads
it.

`coverage.md` is a measurement rather than a text: `--coverage` writes it, nobody edits it,
and it is out of date from the first change to the grammar until it is run again.

`carriers.md` is one too: `--carriers` writes it from the reports a build with
`-p:DotGramReportGeneration=true` leaves, nobody edits it, and it is a run behind main by
construction. A change meant to move a grammar off the tape is measured by its difference.

## Repository conventions

| Document | |
| --- | --- |
| [`coding-conventions.md`](coding-conventions.md) | Required file formatting, C# layout, naming and code-locality conventions. The canonical source of coding style. |

## How it is built

| Document | |
| --- | --- |
| [`development.md`](development.md) | Standing process: build, test, the snapshot baseline, measuring, and the Linux container. |
| [`implementation.md`](implementation.md) | The engine: how the notation is executed. A plan as much as a description, and the one most likely to lag. |
| [`visual-studio.md`](visual-studio.md) | The extension, and the `StringSyntax` annotations. |

## Design — proposals, not descriptions

| Document | |
| --- | --- |
| [`design/lexical-adt-design.md`](design/lexical-adt-design.md) | Separating the lexical and syntactic machines. |
| [`design/finance-fix44.md`](design/finance-fix44.md) | FIX 4.4 architecture assessment, definition maintenance and verified coverage matrix. |
| [`design/sql-parsers.md`](design/sql-parsers.md) | The SQL parsers: independent grammars meeting in one tree, the standard's BNF as the reference, and the work that follows — a BNF-to-`.gram` converter among it. |
| [`design/sql-ast.md`](design/sql-ast.md) | The requirements the SQL tree is held to — lossless, flat, composed rather than inherited, validation outside it — and how the SQL:2023 blank is adapted to them. |
| [`design/sql-tsql-tree.md`](design/sql-tsql-tree.md) | T-SQL moved onto the SQL:2023 tree: what is decided, and a numbered proposal for the rest. |
| [`design/sql-tsql-tree-inventory.md`](design/sql-tsql-tree-inventory.md) | Every node of the old SQL tree held against the SQL:2023 tree, the reference the proposal answers to. |
| [`design/visual-studio-tooling-plan.md`](design/visual-studio-tooling-plan.md) | The living checklist for the extension. |
| [`design/dsl-tooling-design.md`](design/dsl-tooling-design.md) | A plan for tooling an arbitrary DSL. |
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

It stays at the top level rather than under a folder that would label it, because the
generator's own comments cite it by that path.

## For an agent

[`../src/DotGram/SKILL.md`](../src/DotGram/SKILL.md) is how to write a grammar, written
for an agent and shipped inside the NuGet package. It carries what an agent gets wrong
rather than what a reader wants to know, and it is checked the only way that means
anything: by asking one to write a grammar knowing nothing else.

## Continuation snapshots

- [`handoffs/finance.md`](handoffs/finance.md): Finance progress, decisions, validation
  commands and open questions for continuing on another device. Verify its recorded
  commits and current working tree before relying on the snapshot.
