# Documentation

The published site is https://dotgram.github.io/dotgram/, built from these files with docfx
(`docs/docfx.json`; the table of contents is `docs/toc.yml`). Build it locally with
`dotnet tool restore`, then `dotnet docfx docs/docfx.json --serve`.

This folder holds what a user of .Gram reads. Everything aimed at people working on the
repository (the engine plan, the build and test process, coding conventions, design
proposals, the engineering diary, handoffs) is under [`internal/`](internal/README.md), which is
not part of the site, and which says what authority each of its documents carries.

There are three kinds of document here, and they say different things about the compiler.

## The language

| Document | |
| --- | --- |
| [`syntax.md`](syntax.md) | The specification: the notation and its bond with C#. |

Written in the **present tense whether or not the compiler has caught up**. A construct
described here may not build: it is the language's definition, not a report on the
implementation, which is what the next group is for.

## What this version does

| Document | |
| --- | --- |
| [`status.md`](status.md) | The implementation held against the specification, rule by rule, with what is refused and why. |
| [`diagnostics.md`](diagnostics.md) | Every message the generator reports, by identifier. |
| [`ast.md`](ast.md) | The tree the SQL parsers build, and the specification each node is named from. |
| [`carriers.md`](carriers.md) | Which carrier `Auto` took for every grammar of the solution, and for one carried deferred, the gate that kept it and each rule held there with its cause. |

These describe what is true today. A disagreement between one of these and `syntax.md` is
not a contradiction but the gap the pair exists to measure; a disagreement between one of
these and the code is a defect, and `ast.md` is held to that by a test that reads it.
`carriers.md` is written by `--carriers`, never edited, and is a run behind main by
construction.

## Guides, run as written

| Document | |
| --- | --- |
| [`agents.md`](agents.md) | Writing grammars with a coding agent: where each package's `SKILL.md` is, how to point an agent at it, a loop that works and what to check by hand. |
| [`tour.md`](tour.md) | What a grammar can do, by example: feeds written by people, one grammar published three times, versions of one language, typed parsing, streaming and recovery, guards, why an input was refused, grammar libraries, and the four parser packages. |
| [`feeds.md`](feeds.md) | Feeds: trading files in pipes, CSV and fixed width with a header and a trailer, files kept by hand, and a feed of ten million records bulk-loaded into a database. |
| [`guide/language/`](guide/language/README.md) | The `.Gram` language, a page a topic: every feature with a grammar that uses it, the cases worth knowing and the forms refused with their diagnostics. |
| [`cli.md`](cli.md) | `dotnet dotgram`: the generator run outside the build, every command, option and exit code, and what the files it writes promise. |
| [`visual-studio.md`](visual-studio.md) | The Visual Studio extension, and the `StringSyntax` annotations. |
| [`compatibility.md`](compatibility.md) | What a project on an older target framework or C# version needs, and what costs more below C# 11. |

Every example of a guide is compiled and run by a test, its output held to the page's, so a
guide that disagrees with the code is a failing test rather than a stale page. The figures it
quotes from a measurement are the exception: they are what one run on one machine answered.

`tour.md` and `compatibility.md` are the exception for now: they hold what the repository README
used to explain at length, moved here, and only the tour's feeds are run by a test (`FeedsPageTests`).

## For an agent

[`../src/DotGram/SKILL.md`](../src/DotGram/SKILL.md) is how to write a grammar, written
for an agent and shipped inside the NuGet package. It carries what an agent gets wrong
rather than what a reader wants to know. [`agents.md`](agents.md) is the page for the person
driving the agent: where every package's skill is and how to work with one.
