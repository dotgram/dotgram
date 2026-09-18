# Architecture decisions: generator performance

A log of decisions agreed for bringing generated parsers to the work a handwritten parser
does. Each entry says what was decided, why, and what it binds. Proposals, measurements
and rejected experiments live in their own reports under `docs/design/`; this file only
records the outcome and points at them.

## How a decision is made

Since 2026-09-17 a session named `architect` reviews, and does not write, changes to the
generator's architecture: the renderings (flat, reader, engine), the carriers, the arena
and materialization, what the normalizer proves, the shape of emitted code, and any deep
refactoring. A session proposing such a change sends it before implementing it, with the
problem and the measurement that shows it, the design, the alternatives considered, what
the change touches (other grammars, streaming, recovery, diagnostics, code size), how it
is measured against the hand parser, and how it is undone.

The architect's refusal stands until Igor overrides it. An experiment in scratch needs no
approval; landing it does.

## D1. A handwritten parser reads exactly the grammar it is measured against

Decided 2026-09-17 by Igor.

Every parser in `examples/DotGram.Handwritten` reads the language of the generated parser
it is compared with, as that grammar is implemented now: the same publications, the same
accepted and refused input, the same values (trees, fields) and locations. Error wording
may differ; the position of an error may not.

**Why.** A ratio against a parser that reads less, or reads something else, measures the
difference in language and not the generator. It also lets a generator change that costs
time on an unmeasured construct pass unnoticed.

**What it binds.**

- Conformance is checked by tests that run with the ordinary suite, not only by a
  benchmark's agreement step. A hand parser whose agreement is checked only before timing
  drifts as soon as the grammar grows.
- When a grammar gains a construct, its hand parser gains it in the same change, or the
  change says which comparison is suspended until it does.
- A timing is quoted only for inputs both parsers were shown to agree on in that build.

**State at the time of the decision** (`main` at `4a41ecc9`):

| Hand parser | Generated counterpart | Conforms | Checked by |
| --- | --- | --- | --- |
| `Fix.HandFixParser` | `FixParser` | yes, field model and locations | `HandFixTests` |
| `HandSqlStandard` | `SqlStandardParser`, all 42 publications | yes | `Both` in `DotGram.Sql.Tests`, fuzz corpora |
| `HandExpression` | `ExpressionParser` | no: no interpolated or raw strings; `ParseHole` not offered | `--el` agreement only, 169 shapes |
| `HandSqlTokens` | `Sql92Parser` | no: `SearchCondition` only of four publications; datetime literals not read | `--hand` agreement only, 42 shapes |
| `HandSqlOriginal` | none | no, by its own description | none |

**Open, for Igor.** Whether `HandSqlTokens` is brought up to `Sql92Parser`'s four
publications or retired in favour of `HandSqlStandard`, and whether `HandSqlOriginal` is
removed.
