# Materializer sharing candidates — 2026-09-17

## Scope and method

Inspected generated output after `17383239`, with source splitting disabled.
The Examples project includes legacy FIX and ExpressionLanguage; SQL is inspected
separately. Roslyn syntax trees were used only in the offline inventory, not added
to the generator. Local function identifiers beginning with `Materialize_DotGram`
were replaced consistently with ordinal names. Parameter lists and body text were
then compared within the same generated file and enclosing type.

| Project | Materializer methods | Parameter/body characters | Identical complete methods after local-name normalization |
|---|---:|---:|---:|
| Examples | 48 | 5,934,008 | 0 |
| SQL | 32 | 13,627,942 | 0 |

The absence of whole-method matches does not mean the bodies contain no common code.
It rules out simply aliasing one complete materializer to another by this criterion.

## FIX: shared field construction, different roots

Compared `Materialize_DotGram_Buffered_ParseFields` and
`Materialize_DotGram_Buffered_ReadFields` in the legacy FIX parser.
Both take the same buffered character input, parser arena and FIX context.

Of 37 `switch (completed.RuleIndex)` sections per method, 36 have identical
statement text, totaling **467,528 characters**. The first method's sections total
468,681 characters: approximately **99.75%** of that section text is common.
This percentage is not a whole-parser size or performance claim.

The root section differs: parsing builds a `FixField[]`, whereas reading yields a
single `FixField`. Recovery calls also name different helpers, and the array-valued
root needs an additional value table. Replacing either complete method with the
other would change semantics. Common rule construction is the useful sharing unit.

## SQL: common construction with different record numbers

Compared the direct materializers for `CommonValueExpressionTree` and
`NumericValueExpressionTree`. After excluding only outer `case` labels, their
`switch (kind)` sections contain **591 distinct identical statement bodies**,
totaling **204,587 characters**. Each method has 1,080 sections; their statement
text totals 264,264 and 264,265 characters respectively. Duplicate identical bodies
within a single method are counted once in the intersection.

The diff shows different root factories and shifted operation numbers after an
extra construction. Inner case labels, literals and generated statements were not
normalized. Text similarity is not proof of equivalent record layouts: reusing a
helper without coordinating construction IDs would decode the wrong operation.

## Existing mechanism and next experiment

`CSharpEmitter.Joined` already combines compatible publications when one published
root is reachable from another. Sibling root wrappers are not covered merely
because most of their reachable rules overlap. It also deliberately excludes streamed
guests and preserves the flat/direct/engine choice.

The next candidate is to plan a shared machine for compatible large sibling
publications before compiling their rules, giving the union one numbering scheme.
Investigate this before introducing runtime delegates or a new materialization frame.
Preserve each root's EOF requirement, result type, recovery, captures and diagnostics;
retain independent machines when the combined group changes parser strategy.
Small publications must not inherit the cost of a large sibling without measurements.

No production code changed in this investigation. No compile-time, runtime or memory
improvement is claimed. The counts identify an experiment, not an enabled optimization.
