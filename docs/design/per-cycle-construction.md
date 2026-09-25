# Leaving the tape for a recursive cycle

Igor, 2026-09-25 (D142): nested brackets are the case where the reader should leave the tape for
recursion. A `when` that needs a value built inside a recursive cycle makes deferred construction
re-walk the nest at every level; the hand-written parser is linear because each recursive call
simply returns its value. So the construction is to be chosen **per rule cycle**, not per grammar.

This is the design that request asks for. Nothing here is implemented, and no emitter change is
proposed until Igor has agreed.

**Read the soundness section first.** The analysis and the reach came out as expected; the
condition did not. The gate the generator already has for this question answers *no* for every
recursive cycle in every grammar we ship, and it is right to — which moves the whole difficulty
into what replaces it.

---

## 1. What is detected, and where it lives

A **cycle** is a strongly connected component of the rule call graph with more than one rule, or a
single rule that calls itself. `CallGraph` (`Grammar/Model/CallGraph.cs`) already computes these
with an iterative Tarjan and caches them on the graph as `RecognitionGraph.Calls`; `Components`,
`Together` and `Recurses` are its surface. Nothing new is needed for this half.

A cycle is **hot** when some rule in it carries a `when` whose condition names a value built by a
rule *inside the same cycle*.

That is the whole detection, and the narrowing to `when` is checkable rather than a matter of
taste. On the tape an action runs at materialisation, not while the text is read, so the only
thing that can demand a built value mid-parse is a guard — and in the emitter
`Machine.Direct.cs:383` is the **sole** assignment of `_directBuilds`, sitting under
`if (node is Node.Guard guard)`. If a case is ever found where an action's value is demanded
during the parse by something other than a `when`, this section is wrong and the detection widens.

Which members a guard is handed is `Machine.GuardMembers`: the members captured before it whose
parameter name its text contains, `CaptureLayout.Of(...).Before(guard)` deciding "before". It
reads only `RecognitionGraph` — `Bodies`, `Results`, `Types`, `Folds` — plus `CaptureLayout` and
`ResultTypes`. **No Roslyn**, so the analysis belongs in `Grammar/Model` beside `Replay`, and
`GuardMembers` moves there from `Grammar/Emit` with the emitter calling it rather than owning it.

A member with a non-null `Rule` is one built by a rule. The test is that this rule is in the
cycle.

## 2. The reach, measured

Run over every grammar in the repository (`.work/cyclereach`, a console app over the generator's
own sources, since `Calls`, `NodeWalk` and `CaptureLayout` are internal and `DotGram.Tests` has no
`InternalsVisibleTo`):

| grammar | rules | cycles | hot | rules in hot cycles |
| --- | ---: | ---: | ---: | ---: |
| `SqlStandard` | 664 | 7 | **2** | **185** |
| `TransactSql` | 926 | 7 | **2** | 23 |
| `ExpressionParser` | 197 | 5 | **2** | **92** |
| `SqlStandard92` | 90 | 1 | 0 | 0 |
| `Rfc5322` | 126 | 5 | 0 | 0 |
| `Rfc8259` | 16 | 1 | 0 | 0 |
| the other ten Web grammars, `FixGrammar`, `Std` | — | 0 | 0 | 0 |
| ten of the eleven Snapshots | — | 0–1 | 0 | 0 |
| `Snapshots/Twice` | 5 | 1 | **1** | 3 |

**Three shipped grammars are affected and no others.** Every Web grammar and FIX has no hot cycle
at all — several have cycles, none has a guard reading a value built inside one. So this is a
change to SQL:2023, T-SQL and the expression language, and to one snapshot.

The hot cycles themselves:

| cycle | rules | what makes it hot |
| --- | ---: | --- |
| `SqlStandard` `ParenthesizedJoinedTable.f -> TableFactor` | **182** | a bracket reads a table factor |
| `SqlStandard` `JSONTablePlan.p -> JSONTablePlanPrimary` | 3 | |
| `ExpressionParser` `Inferred.value -> Block` | 46 | |
| `ExpressionParser` `ForeachInferred_With1.source -> Assignment_With1` | 46 | |
| `TransactSql` `TriggerStatement.atomic -> AtomicBody` | 18 | |
| `TransactSql` `TSqlInsert.rows -> InsertRows` | 5 | |
| `Snapshots/Twice` `Sum.first -> Term` | 3 | |

**The first number is the one that shapes everything below.** SQL:2023's bracket cycle is 182
rules — 27% of the grammar — and the expression language's two are 92 of 197, 47%. A "cycle" here
is not a small local knot around the bracket; it is everything mutually reachable from a table
factor, which in SQL is most of the query language. Choosing construction per cycle is therefore
not a fine-grained choice in practice: for SQL it is close to choosing it for the query half of
the grammar.

`Snapshots/Twice` being hot is convenient — the snapshot set already contains a witness, and its
cycle is three rules that a person can read.

`Snapshots/Tower`, added yesterday, is **not** hot: eighteen levels of a tower are eighteen
distinct rules in a chain, not a cycle. It exercises the materializer's fast path, which is a
different thing from recursion, and a witness for this design would have to be a new grammar.

## 3. Soundness: the existing gate says no, for every cycle we ship

`Auto` already decides whether a grammar may build where it reads, and its first gate is
`Replay` (`Grammar/Model/Replay.cs`): `Keeps(rule)` is true where the rule `Stands` — every
reading of it is on the derivation that accepted — or is `Losing`, meaning the only readings put
back belong to a parse that fails and hands nothing back. This is D137's condition, and D138 made
forcing the immediate carrier past it a warning rather than an error.

Asked of each hot cycle, of the rules in it that build:

| cycle | build | **not** replay-safe | why |
| --- | ---: | ---: | --- |
| `SqlStandard` `ParenthesizedJoinedTable` | 182 | **182** | `Under` 152, `Follows` 30 |
| `SqlStandard` `JSONTablePlan` | 3 | **3** | `Under` 3 |
| `ExpressionParser` `Inferred` | 46 | **46** | `Under` 40, `Follows` 6 |
| `ExpressionParser` `ForeachInferred_With1` | 46 | **46** | `Under` 40, `Follows` 6 |
| `TransactSql` `TriggerStatement` | 18 | **18** | `Under` 16, `Follows` 2 |
| `TransactSql` `TSqlInsert` | 5 | **5** | `Under` 4, `Follows` 1 |
| `Snapshots/Twice` `Sum` | 3 | 0 | — |

**Every rule of every shipped hot cycle fails it.** Only the toy passes. So if the condition the
analysis proves were "`Replay.Keeps` for every building rule in the cycle", the design would
change nothing at all, and the right answer would be to abandon it.

It is not, and the reason is the point of the whole idea. `Keeps` asks *is this value ever built
and then thrown away*. A bracket cycle is entered speculatively — that is what a bracket is — so
the answer is inevitably no, and the 152 `Under` are simply "reached from something already
speculative". **Igor's proposal takes the discarding as given**: the hand parser discards too, and
is linear because discarding costs it O(1) per level. So the condition cannot be "nothing is
discarded". It has to be the two things the architect named:

1. **Nothing escapes.** D137: no exception escapes a reading the parser abandons. This is exactly
   what EL's `ExpressionParser.Immediate` relies on today — D138 records that it is safe "because
   its grammar puts a `when` in front of every construction that could throw ... not because
   anything checks", with expr's 10,722-text two-carrier comparison as the evidence. For a cycle
   the same argument must be made, and the question is whether it can be *proved* rather than
   argued.
2. **The waste is linear.** A value built on an alternative that is then abandoned must cost O(1)
   per level, not O(depth). This is the property that makes the hand parser linear and is the
   whole point of the change; it is also the property most easily lost by an implementation that
   keeps a per-level list of what it built so it can be released.

**What I cannot yet offer is a proof of (1) that the analysis can carry.** `Replay` is sufficient
and too strong; "every construction behind a `when`" is what EL relies on and is a property of the
grammar that could be computed — for each building alternative in the cycle, is there a guard
before its construction — but it is not the same as "cannot throw", because a factory the author
wrote may throw for reasons no guard covers. The honest options are:

- **(a) Prove the narrow thing.** The analysis proves "every construction in the cycle is preceded
  by a guard in the same alternative", and a cycle that fails it stays on the tape. Computable
  where `GuardMembers` is, and it is the property EL already has. It does not prove the absence of
  exceptions; it proves the author was given the place to prevent them.
- **(b) Warn, as D138 did.** The generator chooses per cycle where it can prove (a), and where it
  cannot, an author who wants it says so and takes the argument, exactly as `GramCarrier.Immediate`
  works now, with GRAM5015's shape.
- **(c) Refuse the roots.** Note that `Under` is transitive: within the cycle the real causes are
  the `Follows` — 30 in SQL, 6 and 6 in EL, 2 and 1 in T-SQL. Whether those roots can be made
  safe individually is a smaller question than the 182, and worth asking before any of the above.

This is the part of the design that needs Igor, because it is his call how much is proved and how
much is the author's to argue.

## 4. The boundary with the tape

**The obstacle is structural, not incidental: a carrier is one per `Machine`.** `Machine` holds
`_carrierKind` and a single `Carrier`, and code throughout the emitter asks
`Carrier is TapeCarrier` / `CarriesImmediately` to decide what to write. "Per cycle" means one
reader in which some rules build as they read and others record on the tape. Two shapes:

- **(A) One machine, two carriers.** Every `Carrier is TapeCarrier` becomes a question about the
  rule being emitted. Touches everything; the interesting cases are the ones where the two meet
  mid-sequence, and every existing invariant about "the tape holds everything" has to be re-read.
- **(B) The cycle as a reader of its own.** The generator already emits several machines per
  compilation — the lexical split and the buffered emitter's memory reader do exactly this
  (`BufferedEmitter.cs` builds three). The cycle becomes a machine whose rules build immediately
  and whose entry point **returns a value**, which is Igor's framing word for word. The tape
  records the call as one record holding the value the cycle returned.

**(B) is the shape to design**, because it matches both the existing structure and the sentence
"each call returning its value as a hand parser does". It raises three questions that (A) does
not, and they are the real content of the next revision of this document:

- **Entering.** The tape-side reader calls the cycle's reader at the call site. It must put back
  the same way on refusal, and the stack guard — `Deepen`, which hands a deep reading to a 16 MB
  thread — is per machine, so a recursion that now lives in a second machine needs its own, or the
  two must share one. The four-term budget in `Machine.Reader.cs` was derived for one.
- **The record.** A value returned by the cycle enters the tape as one record. Materialisation at
  that record is "take the value already built", which is a new kind of record: today a record
  says how to build, not that building is done. `Ways.Built` and the new `AllBuilt` watermark both
  assume flags over records that the walk may build.
- **Leaving.** A rule *inside* the cycle that calls out to a tape rule gets a value that does not
  exist yet. Either such a call forces the outer reading to materialise early — which would undo
  the gain — or the cycle may only call rules whose values it does not name. The second is a
  condition the analysis can check, and it may be what makes the 182 shrink.

## 5. Cost to the generator

The detection costs, measured on the graph a build already has (`.work/cyclereach`):

| grammar | rules | detection | generation | share |
| --- | ---: | ---: | ---: | ---: |
| `TransactSql` | 926 | **4.0 ms** | 4,814 ms | 0.08% |
| `SqlStandard` | 664 | **4.7 ms** | 3,835 ms | 0.12% |
| `ExpressionParser` | 197 | 0.7 ms | 875 ms | 0.08% |
| `Rfc5322` | 126 | 0.2 ms | 184 ms | 0.11% |

The components come from `RecognitionGraph.Calls`, which is already built and cached for other
questions, so the marginal cost is the walk over guards. D11 B took T-SQL from 4 s to 86 s; this
is a tenth of a per cent, and it is measured rather than assumed. **What is not measured is the
cost of the emission change**, which cannot be until there is one.

## 6. How it would be measured

- **Counts before timing.** Rule entries and materialiser calls per character, nested and flat, at
  two sizes, against the hand parser — the instrument from
  `benchmarks/results/hand-against-generated-2026-09-24`. The claim to test is that the nested
  series becomes linear, so a climbing quotient is the failure.
- **The deep rows**, `sql/nested-{100,400}.match` and the EL pair, which exist for this
  (`StandNesting.cs`). SQL:2023 reads 13.4x for four times the depth today and the expression
  language 3.8x; the target is SQL's ratio approaching four.
- **Size-growing rows on every guarded family** — EL, T-SQL, FIX, Web — per the `584a7c1f` lesson,
  even though FIX and Web have no hot cycle and so should not move at all. That they do not move
  is the assertion.
- **Generation time** on `DotGram.Sql` before and after, by `Gate-Generation.ps1`.
- **Generated size**, within one worktree, since `#line` carries absolute paths.
- **Both carriers agreeing**, which is expr's standing comparison: a cycle that changes
  construction must read every text the same as it did.

## 7. Alternatives, and how it is undone

- **Do nothing.** The watermark already removes the scan's triangle; the walk's remains. Nested
  brackets stay quadratic in SQL:2023 and linear in EL. This is the baseline any of the above must
  beat, and it is not obviously wrong — the case is deep nesting of brackets, which is rare in
  written SQL and common in generated SQL.
- **Rework the full walk** (the 60%). Stopped by Igor in D142 as treating the symptom. Recorded
  here so that stopping it is a decision with a reason attached rather than a gap.
- **Fix the grammar instead**, as `3a8d5bcd` did for the expression language: a parenthesis and a
  tuple read once. That commit made EL's nesting linear, which is why EL reads 3.8x and SQL 13.4x
  on the deep rows. **If the same is available for `ParenthesizedJoinedTable`, it is cheaper than
  any carrier change and needs no new machinery.** sql-47's `ScalarSubquery` finding is the
  existing thread; this should be priced before the design is built.
- **Undoing it**: the construction is chosen by an analysis, so it is undone by making the
  analysis answer "no cycle is hot", which leaves every grammar on the tape and every emitted
  reader as it is today. That is the property to keep through the implementation — the choice must
  never be baked into the shape of what is emitted for the rules outside a cycle.

## 8. What I would ask before building it

1. **How much is proved and how much is the author's?** §3's (a), (b) or (c). This is D138's
   question again and it is Igor's.
2. **Is the grammar fix available for the bracket?** §7's third alternative. If
   `ParenthesizedJoinedTable` can read a bracket once the way `3a8d5bcd` made a parenthesis and a
   tuple read once, that is the cheaper path and this design waits behind it.
3. **Does 182 rules change the answer?** The proposal reads as a local choice around a recursive
   knot. In SQL it is 27% of the grammar and in EL 47%. If that is more than was intended, the
   useful question is whether the cycle can be cut smaller — §4's "leaving" condition is the
   candidate — rather than whether to take it whole.
