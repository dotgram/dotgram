# Shared beginnings over kinds (Q7.1)

`docs/design/architecture-decisions.md` Q7.1: most rules stay on the tape because of a few
causes, and each cause removed moves a subtree to one-pass construction. The decision asked
for the design to be made over kinds, once SQL:2023 was read that way (7d3ba2d5). This is that
design: what holds SQL:2023's rules on the tape now, what the causes are, and what removes each
kind of them. Nothing here is built yet.

## 1. What the tape is holding, and why

`Replay.Of` over SQL:2023's syntactic graph after the split, counted by a scratch tool that
walks the graph as `Replay.Walk` does and records where each rule is first marked and why:

| | over characters | over kinds |
| --- | ---: | ---: |
| rules | 666 | 573 |
| read where the reading may not stand | 431 | 342 |
| … of them with a cause of their own | 109 | 66 |

The rest are under those 66: a rule read inside a reading that may not stand may not stand
either. `GRAM5012` counts the building ones, 329 of 556. SQL-92, split: 54 of 60, 10 with a
cause of their own.

**One cycle decides most of it.** 184 rules call each other through `ValueExpression`, so a
single cause anywhere in the cycle puts all 184 on the tape, and everything they call with
them. Forty-three causes stand inside it, in about twenty rules.

**What `Auto` needs is none at all.** The carrier is chosen per machine: immediate only where
no rule it builds is replayed (`Machine.Choose`). Removing causes changes nothing a parse does
until the last one goes, unless a carrier is ever chosen per rule. That is D3's question and
not this one. Until then a partial result is worth what it does to the count and nothing more.

**What there is to win.** `ImmediateSql` compiles SQL-92 with the immediate carrier: 1.17 of
the hand parser against 2.45 on the tape (next.md, "The measurement that says so"). It is not
shipped and could not be: an immediate carrier runs a factory once per derivation *tried*, and
the tree's factories being pure makes the comparison fair and not the parser safe. Q7.1 is how
the same carrier gets chosen for a grammar honestly: when the graph proves every building rule
stands.

## 2. The causes, by kind

Of the 66, by what makes the reading replaceable (`[choice]`: an alternative with a sibling
behind it; `[turn]`: inside a repetition or an optional) and what can fail after the call:

| kind | count |
| --- | ---: |
| choice, a token fails after the call | 50 |
| choice, a call fails after it | 14 |
| choice, a guard (`when`) fails after it | 5 |
| turn, a token fails after it | 17 |
| lookahead | 2 |

(The counts are per site; a rule with several sites counts in several rows.)

## 3. Two refinements of the analysis

Both are about what `Replay` already asks, answered more exactly. Neither changes what is
emitted for a grammar the report does not move.

**A. A sibling replaces a reading only where it can begin where the reading began.** Today a
choice whose alternatives overlap anywhere marks every alternative but the last as
replaceable (`Exclusive` is all or nothing). An ordered choice tries later alternatives only;
an alternative whose first set is disjoint from every later one's, and which matches
something, is replaced by nothing when it fails after its first token, and its reading is
`Losing`, not `Follows`. `AggregateCall` is the example: `COUNT(*)` overlaps the general
`ComputationalOperation(…)` alternative, which overlaps nothing after it, so the general
alternative's `ValueNode` stands where the parse succeeds.

Effect: causes 66 → 46 over kinds (112 → 93 over characters, SQL-92 10 → 9).
Six of the cycle's owners go: `AggregateCall`, `WindowedFunction`, `PrimaryBase`,
`PrimaryReading`, `TablePrimary`, `WindowFrameStart`.

**B. A failed turn is replaced only where what follows can begin with it.** A repetition
or an optional whose turn fails after its first token stops, and the parse goes on with what
follows the repetition. Where what follows cannot begin with the turn's first token, that goes
on to fail too, and the reading is `Losing`. `CycleClause`'s `("TO" & v: ValueNode & "DEFAULT"
& d: ValueNode)?` is followed by `USING`, which `TO` is not.

Effect with A: 43 causes. `CycleClause` and `SampleClause` leave the cycle. What the rules held
under the causes do is another matter: the scratch tool's closure (548 → 345) was wider than
`Replay`'s own count and overstated it. Landed (see §7), `GRAM5012` moves from 331 to 327 building
rules on SQL:2023 and not at all on SQL-92 — the cycle is still held, as §1 says it would be
until its last cause goes.

B does not touch `Because.Turn` — a turn that *succeeded* being given back — which is the
reader's and stays unanswered by the graph, as today.

Both belong in `Replay.cs`, with a control grammar each: a choice whose first alternative
overlaps a later one and whose middle one does not, and an optional whose first token is and
is not in its follow.

## 4. What the analysis cannot answer: the grammar has a shared beginning

The 43 that remain are, in the main, alternatives that really do begin alike. They need the
beginning read once.

**C. Factor adjacent alternatives that share a head, not only all of them.**
`FactorCommittedPrefixes` folds a prefix shared by *every* alternative of a choice. Most
shared heads are shared by a run of them:

- `ColumnValueSource`: four of five begin `GENERATED`, three `GENERATED ALWAYS AS`.
- `CollectionValueConstructor`: `ARRAY`/`MULTISET` then a bracket, or then a subquery.
- `ContextuallyTypedRowValueExpression`: two begin `(` and a contextually typed element.
- `CharacterStringType`, `NationalCharacterStringType`, `BinaryStringType`: a type name, then
  a length or a large object's length.
- `IntervalQualifier`, `SingleDatetimeField`, `EndField`: a field, then optional precisions.

Folding a run is the same fold as today, on a run of alternatives rather than all of them,
with the same `Determinism` proof and the same rule that only identical bindings survive. It
keeps the order: a run is adjacent, or is made adjacent only where the alternatives between
cannot begin with the head.

**D. Across rule boundaries, where the head is a call on one side and a literal on the other.**
`COUNT(*)` against `ComputationalOperation`, which may be `COUNT`. A called rule whose body is
a choice of literals can be split by the head: the choice's alternative that shares the
literal is folded with it, the rest stays a call. This is what Q7.1 was named for, and A
answers most of it without a fold. What remains after A and C is small enough to count by
hand before it is built.

**E. Two optionals that begin with the same call.** `JSONValueFunction`'s
`(JSONValueBehavior ON EMPTY)? (JSONValueBehavior ON ERROR)?`, and the column behaviours in
`JSONColumnOnEmpty` / `JSONColumnOnError`. The shape reads the behaviour and then decides by
the word after `ON`. That is a grammar edit: the rule reads `JSONValueBehavior ON` once and
then `EMPTY` or `ERROR`, with a guard for the order. A fold that does this for the author would
be a new transformation (two optionals into a loop with a check), and one case does not ask for
it.

## 5. What the grammar says on purpose

Four causes are the grammar reading something and then looking at it:

- `WhenOperand`: a predicate's second part, kept only where the towers say it is one.
- `InsertValues`: `VALUES …` read as a table value constructor, and refused if a set operator
  or an `ORDER BY` follows, when it was a query.
- `TableContentsSource`: an optional column list before `AS`, against a table element list,
  both beginning `(`.
- `POSITION_REGEX`'s `?=RegexSearch`, which reads a whole search to decide whether `START` or
  `AFTER` is a keyword.

Each has a rewrite that decides at a token instead: the `InsertValues` question moves into the
query expression, which already begins with `VALUES`; the regex lookahead becomes a two-token
one (`START`/`AFTER` followed by what cannot begin a search); the column list is read as a
bracketed list whose contents say which. These are sql's, and each goes with a `Both` test and
the hand parser in the same commit (D1).

## 6. Order

1. A and B in `Replay.cs` (generator; performance-ff's area, coordinated). Each with its
   control grammar. Emitted code moves only for a grammar that reaches zero. SQL does not yet.
2. C in `FactorCommittedPrefixes`. Snapshots and the three renderings. Counted again.
3. The grammar edits of §4E and §5, one commit each, with the hand parser.
4. D, if anything is still left. Counted by hand first.
5. When `Replay` says every building rule of SQL:2023 stands, `Auto` chooses the immediate
   carrier by itself, and the stand measures that against today's tape (SQL rows, first calls,
   allocation). `ImmediateSql` says what to expect: about half.

Each step is counted with the scratch tool (`Replay` per rule, the causes, the cycle) before
and after. The tool goes to `.work/` with its recipe in `docs/development.md` when the first
step lands.

## 7. A and B as landed

`Replay.Walk` carries what follows each node (the rest of the sequence, and past a rest that may
read nothing, the enclosing follow up to the rule's). A choice asks of each alternative whether a
later one can begin where it began (`Replaced`), a nullable one beginning with what follows; a
repetition asks whether what follows can begin where a turn did. What holds either still answers
for it (`elsewhere`), so a failure the choice cannot replace is the enclosing context's, not
`Losing` outright.

Landing it found a defect that was already there: a sequence worked out why a part may be put
back only while nothing was known from outside, so inside a turn of a reading the whole parse
would lose (`Losing`), a part that a failed turn gives up and the parse then goes on without was
left `Losing` and not `Follows` — which says a carrier may build it where it reads, when a
successful parse throws that reading away. The reason from inside is now added to the one from
outside. `ReplayTests` holds both refinements and the defect side by side; no grammar in the
solution changes carrier or emitted text, SQL:2023's count moves 331 → 327, and generation time
is no worse (the solution's 109 grammars 36.9 s → 33.9 s, DotGram.Sql 17.1 → 16.1).

## Appendix: the 43 causes after A and B, over kinds

By owner rule: the rule called, and what fails after it.

```
AllFields: ColumnNameList [choice] token
AlterTableAction: Identifier, SchemaQualifiedName, PeriodForSpecification [choice] call; ColumnDefinition [choice] token
BinaryStringType: LargeObjectLength [choice] token, [turn] token
ChainOrMeasure: Identifier [turn] lookahead
CharacterSetSpecification: Identifier [turn] token
CharacterStringType: CharacterLength, CharacterLargeObjectLength [choice] token, [turn] token
CollectionValueConstructor: ValueNode [choice] token
ColumnValueSource: IdentityGeneration, CommonSequenceGeneratorOptions, ValueNode [choice] token
ContextuallyTypedRowValueExpression: ContextuallyTypedValueSpecification, ContextuallyTypedElement [choice] token
Correlated: DataType [choice] call; ColumnNameList [choice] token
DescribeStatement: SQLStatementName [choice] call
EndField: IntervalFractionalSecondsPrecision [turn] token
FetchStatement: FetchOrientation [turn] token
GrantStatement: Privileges [choice] token
InsertColumnsAndSource: ColumnNameList [turn] token
InsertValues: ContextuallyTypedTableValueConstructor [choice] lookahead
IntervalQualifier: StartField [choice] token
Joins: JoinPartitioning [choice] call
JSONArrayConstructor: JSONInputExpression, JSONConstructorNullClause, JSONOutputClause [choice] token
JSONColumnOnEmpty, JSONColumnOnError: JSONColumnBehavior [choice] token
JSONColumnWrapper: JSONQueryWrapperBehavior [choice] token
JSONNameAndValue: CharacterNode [choice] token
JSONPredicatePrimary: JSONPathPredicate [choice] token
JSONQuery: JSONQueryBehavior [turn] token
JSONTableColumnDefinition: Identifier [choice] call, token
JSONTablePrimitiveColumn: Identifier [choice] token
JSONValueFunction: JSONValueBehavior [turn] token
NationalCharacterStringType: CharacterLength, CharacterLargeObjectLength [choice] token, [turn] token
NumericValueFunction: RegexSearch [lookahead]
PrivilegeAction: ColumnNameList, SpecificRoutineDesignator [choice] token
RevokeStatement: Privileges [choice] token; Grantees, GrantedBy [choice] call
RowPatternPrimary: RowPattern [choice] token
SchemaNameClause: SchemaName [choice] token
SetTargetTail: SimpleValueSpecification [choice] token
SingleDatetimeField: IntervalLeadingFieldPrecision, IntervalFractionalSecondsPrecision [turn] token
SQLArgument: Identifier [choice] token
SQLDiagnosticsInformation: SimpleTargetSpecification [choice] token
SQLDynamicDataStatement: FetchOrientation [turn] token
SQLParameterDeclaration: Identifier [turn] lookahead
TableConstraint: Identifier [turn] lookahead
TableContentsSource: ColumnNameList, Subquery [choice] token
WhenOperand: PredicatePart2 [choice] guard
```
