# Project instructions

## Coding conventions

Read and follow [docs/coding-conventions.md](docs/coding-conventions.md) before
writing or editing code. It defines file formatting, C# style, naming and code
locality, including the distinction between handwritten and emitted code.
Keep the detailed rules there; this file covers project workflow and architecture.

## Profiling

How the generator is measured and profiled — the harness, the byte-for-byte check against a
baseline, dotTrace with its Reporter, dotMemory — is in `.claude/rules/profiling.md`. Read it
before measuring anything rather than working it out again.

## Git

Commits go straight to `main`. No feature branches — this is a single-user
repository, and branching only adds a merge step.

## Repository layout

```text
docs/
	README.md             the index: what authority each document carries, and when it
	                      is expected to be out of date. Read it before believing one
	syntax.md             the language: notation and its bond with C#
	status.md             what this version does, held against that specification
	diagnostics.md        every message the generator reports, by identifier
	implementation.md     engine plan
	development.md        standing process: build, test, the snapshot baseline, measuring,
	                      and the Linux container
	visual-studio.md      the extension, and the StringSyntax annotations
	ast.md                the tree the SQL parsers build, and where each node comes from
	carriers.md           every grammar's carrier and what holds it on the tape; written
	                      by --carriers, never edited
	coverage.md           how much of Microsoft's T-SQL reference the T-SQL grammar reads;
	                      written by --coverage, never edited
	next.md               the engineering diary, authoritative about nothing present
	design/               proposals and plans; no statement here is about the compiler
src/
	DotGram/              the generator: one analyzer package, no runtime
		Grammar/          pure: a function of grammar text, no Roslyn
		Generation/       the Roslyn shell
		Language/         the editor-neutral analysis of a .gram document — classification,
		                  symbols, diagnostics — that the Visual Studio extension reads
		README.md         the package's own front page, not the repository's: what to
		                  write once it is installed, and links back for the rest
		SKILL.md          how to write a grammar, for an agent. Ships in the package
		                  beside that README, which points at it in a comment nothing
		                  renders
	DotGram.ExpressionLanguage/ the C#-style expression language, a package of its own:
	                      ExpressionParser, in the namespace the project is named for
	DotGram.Finance/      FIX, a package of its own, in the DotGram.Finance.Fix44 namespace:
	                      the wire read into typed fields and the messages built over them,
	                      one door (FixParser), the checks (FixValidators). README.md and
	                      SKILL.md ship on NuGet
	DotGram.Sql/          SQL, a package of its own, with room for other databases: the one
	                      tree the SQL grammars meet in, its writer and walker, in the
	                      DotGram.Sql namespace, and a directory and namespace per dialect
		Standard/         ISO SQL: SqlStandardParser (SQL:2023) and, for now, Sql92Parser;
		                  Specification/ holds the ISO BNF
		TransactSql/      T-SQL, TransactSqlParser; Specification/ holds Microsoft's
		                  published syntax. Each dialect is its own grammar, meeting the
		                  others only in the tree (docs/design/sql-parsers.md)
	DotGram.VisualStudio/ the Visual Studio extension: .Gram language support in the
	                      editor, over DotGram/Language (docs/visual-studio.md)
	DotGram.Web/          the formats of the web, a package of its own, in the DotGram.Web
	                      namespace. A file and an internal grammar class per specification
	                      (Rfc3339 … Rfc9651); what is public is the values, each with its own
	                      Parse and TryParse (JsonValue.Parse, MediaType.Parse, …). README.md
	                      ships on NuGet. Written against whole specifications, not as
	                      teaching material — and, being an ordinary project the generator
	                      runs over, it is where a real symbol resolver is exercised
examples/
	DotGram.Handwritten/  reusable manual parsers for differential tests and benchmarks.
	DotGram.Examples/     whole parsers meant to be copied: a grammar, the class it
	                      attaches to, and the code written against it. No test
	                      framework and no scaffolding — DotGram.Tests runs them.
	                      Grouped by what they read: Formats/, Feeds/, Expressions/,
	                      Languages/, each its own namespace under DotGram.Examples
tests/
	DotGram.Tests/        three levels: direct calls, in-memory generator driver,
	                      and the generator attached as an analyzer
	  Calculators/        one language spelled several ways, held against itself: the
	                      spellings an example would only repeat
	DotGram.Tests.Slow/   what is paid for on request and in CI rather than on every run
	                      of DotGram.Tests (D12): the whole refusal record, of which
	                      DotGram.Tests holds a sample, the memory bounds of streaming,
	                      GRAM5003's parts at every size, and a nine-hundred-rule split
	DotGram.Sql.Tests/    the SQL parsers held to what SQL Server answers, and the tree, its
	                      writer and walker
	DotGram.Finance.Tests/ the FIX package: fields, messages, the streaming forms, and the
	                      hand-written parser beside the generated one
	DotGram.Finance.Fix44/ the FIX 4.4 grammar and its generated parser, the oracle the
	                      package is held against. Not shipped: it is a fixture
	DotGram.Finance.Fix44.Tests/ the comparisons against that oracle, kept out of
	                      Finance.Tests so that the ordinary tests build in seconds (D12)
	DotGram.Compatibility/ the generated code built for netstandard2.0, net472 and
	                      net8.0 at the C# 8 floor. Building it is the assertion
	DotGram.PackageSmoke/ the packed package asked what it promises, under the oldest
	                      Roslyn it supports. Not in the solution; CI runs it after packing
	DotGram.Finance.PackageSmoke/ the same for the FIX package, on both frameworks it
	                      ships for. Not in the solution; CI runs it after packing
	DotGram.VisualStudio.Tests/
	  Playground/         grammars to open in the experimental instance, each with the
	                      manual check beside it (docs/visual-studio.md)
	Snapshots/            a grammar and the file it must compile into, checked in so
	                      a change to code generation shows up as a diff
	Corpus/               somebody else's SQL: SqlScriptDOM's test corpus, copied byte for
	                      byte, that the SQL grammars are read against
benchmarks/
	DotGram.Benchmarks/   BenchmarkDotNet, run by hand and not by CI. Built by the
	                      solution so that it has to keep compiling
	DotGram.Finance.Benchmarks/ the same for FIX
	FirstCall/            what a first call costs: a fresh process reports its phases, the
	                      methods it compiled and their size (Fix/, Sql/)
	DotGram.CodeSize/     the generated assemblies of two checkouts weighed against each other
	Gate-Generation.ps1   the generator's own time on the grammars, held to a base built in
	                      the same run
	README.md             the measuring stand: its rows, the paired form, the windows a
	                      timing run is taken in, and every mode
	results/              what a measurement answered, by date; kept, not maintained
.work/                    scratch, ignored by git
```

Nothing under `examples/` may reference a test framework or be written for one. An
example that needs a fixture to make sense is not an example; assertions about it
belong in `tests/DotGram.Tests/ExampleTests.cs`.

No runtime assembly ships, deliberately: everything a generated parser needs is
emitted into the consumer's own compilation. See `docs/syntax.md` §6.1.

Two seams keep `Grammar/` free of Roslyn — `ISymbolResolver` for `@Name` and
`ICSharpScanner` for `@(...)`. Both are implemented over Roslyn in `Generation/`.
