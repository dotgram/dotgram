# SQL generated source duplication

Analyzed saved generated files from the user's Finance build, matching the supplied
log. No grammar or production code changes. UTF-8 byte totals exclude BOM.
Roslyn member spans provide the same non-overlapping categorization used for FIX.

| Parser | Total MB | Read methods MB | Materializers MB | Lexer MB |
| --- | ---: | ---: | ---: | ---: |
| SQL92 | 0.781 | 0.185 | 0.062 | 0.331 |
| SQL Standard | 13.791 | 10.326 | 0.877 | 0 |
| T-SQL | 22.026 | 11.852 | 3.201 | 2.807 |
| T-SQL Located | 23.034 | 12.498 | 3.835 | 2.844 |

## Repeated publication machines

T-SQL has four direct materializers: Select (246,499 bytes), Statement (986,586),
Sql (981,421), and Script (986,838). Their reader method groups occupy respectively
0.763, 3.726, 3.667 and 3.696 MB. Located repeats this arrangement with location-aware
construction; its larger materializers alone are not evidence of a bug.

Replacing the four machine-name suffixes in T-SQL reader method declaration text
and hashing the resulting text finds 1,805 equal-text groups containing 5,748
methods. Keeping one largest representative per group leaves 3,520,273 bytes of
repeated source. This is a candidate deduplication amount, not a guaranteed saving:
identical text still needs binding/context validation, and stateful materialization
or diagnostics may require different numbering.

Name-only matching finds 2,789 overlapping reader names, with 8,113,469 bytes beyond
one largest representative per name. Unlike the stricter text comparison, this
includes bodies that differ and must not be reported as removable code.

SQL Standard likewise has four groups: Literal, TableName, ValueExpressionTree and
DirectSQLStatement. The expression and statement readers alone occupy 3.451 and
5.840 MB. Name-only overlap is 2,057 groups / 4,459,813 excess bytes, again a lead for
investigation rather than proven semantic equivalence. ReservedWord readers alone
appear four times at approximately 0.18-0.21 MB per copy.

CSharpEmitter.Joined already shares some publications. The subsequent overlap gate
requires direct non-flat machines, at least 128 reachable rules, no streaming,
compatible guard construction, tape eligibility and 90% overlap. The generated
copies show sharing remains incomplete; determining which gate prevents each merge
requires instrumentation, not inference from names alone. Small publications must
not inherit a large sibling's infrastructure merely to reduce source size.

## Lexer tables and alphabet compression

Correction to the earlier discussion: LexerEmitter already computes identical-column
character classes for sufficiently large tables whose rows have a common origin.
Roomy is 131,072 cells. T-SQL's generated Scan_Class maps the 128-entry fast alphabet
to 72 classes; uppercase/lowercase Latin letters share codes. Its generated loop
indexes Scan_Cells through Scan_Class. SQL92 does not emit this class table.

The T-SQL Scan_Cells field occupies 2,411,481 source bytes. Located repeats it as
2,436,183 bytes (different indentation). Scan_Cells, Scan_States, Scan_Class and
Scan_Accepts match between variants after removing whitespace. Sharing immutable
lexer data between variants is therefore a concrete candidate, subject to generated
visibility and initialization design. Source bytes are not the runtime array size.

## Priority

1. Attribute failed publication merges, particularly Sql/Statement/Script.
2. Test shared immutable lexer data between ordinary and Located variants.
3. Consider sharing reader helpers across otherwise separate machines where their
   dependencies and numbering allow it.
4. Revisit alphabet compression only for uncovered cases or improved thresholds;
   the fundamental strategy is already used here.

Compared with FIX, SQL mostly duplicates direct reader methods rather than twelve
large engine recognizers. Both point to code sharing as a major opportunity.
These are source-size measurements; no runtime speedup or compiler-time saving has
been measured in this analysis. Raw categories and artifact hashes are in
benchmarks/results/sql-code-size-2026-09-17.json.
