# The SQL tree: requirements

What the tree every SQL parser builds has to be. Set by Igor on 2026-09-15, when the tree was to be
reshaped to SQL:2023 and then adapted to T-SQL and SQL-92. The starting point is
`src/DotGram.Sql/Standard/Sql2023Ast.cs` with its map `Sql2023Ast.BnfMap.csv`: a blank, not a dogma —
it is changed wherever these requirements or the parsers need it, and each change is recorded below.

`docs/ast.md` describes the tree that is built today; this document is what the reshaped tree is held
to, and every later decision about a node is checked against it.

In one line: **a lossless representation of SQL, flat semantic hierarchies, and composition,
properties, enums and lists in place of the BNF's structure, with validation outside the tree.**

## The requirements

1. **All of SQL:2023's BNF is covered.** Any construct of the specification can be represented. For
   every BNF rule it is clear where it goes: a class, a property, an enum, a list, or nowhere, as a
   purely grammatical rule.

2. **The tree does not repeat the BNF's structure.** Rules such as `<query expression body>`, `<query
   term>`, `<query primary>`, `<simple table>`, the intermediate categories of statements, and the other
   levels that exist for parsing, precedence or validation do not become nodes because the grammar has
   them.

3. **The tree keeps all syntactic information.** No difference is lost: `UNION` / `UNION ALL` /
   `UNION DISTINCT`, `LEFT JOIN` / `LEFT OUTER JOIN`, whether `AS`, `ASC`, `COLUMN`, `ROW` or `ROWS` was
   written, which spelling of a key word. After parsing, the tree holds enough to write the same SQL
   back.

4. **The tree does not validate.** Its structure need not make an invalid combination impossible to
   build. The constraints of SQL:2023 and of each dialect are a separate validation layer, which keeps
   the tree simpler and open to extension.

5. **The first criterion of structure is how simply it represents SQL.** SQL reads straight from the
   object: a `SELECT` is roughly `Statement.Select` with `Items`, `From`, `Where`, `GroupBy`, `Having`,
   `Window`, `SetOperations`, `OrderBy`, `Offset` and `Fetch`, not a chain of the BNF's wrappers.

6. **Class hierarchies are one level deep.** Allowed:

   ```csharp
   public abstract record Statement
   {
       public record Select : Statement;
       public record Insert : Statement;
   }
   ```

   and

   ```csharp
   public abstract record Expression
   {
       public record Binary : Expression;
       public record Literal : Expression;
   }
   ```

   but not `SqlNode → Statement → Select` or `Expression → BooleanExpression → Predicate`. A descendant
   is a direct child of its abstract parent.

7. **Children are nested in their abstract parent** — `Statement.Select`, `Expression.Binary`,
   `TableSource.Join`, `DataType.Character` — and nested no further: no `Statement.Select.Query.Block`.

8. **No common `SqlNode` base class.** A shared technical contract is at most a marker interface such
   as `ISqlNode`. Metadata, source locations, trivia and the like are attached by composition, not by
   another level of inheritance.

9. **Inheritance only for structurally different forms.** `Expression.Binary` and `Expression.Case`
   differ in structure, and a hierarchy is justified there. `ASC`/`DESC`, `ALL`/`DISTINCT`,
   `CASCADE`/`RESTRICT` do not, and are enums or properties.

10. **Enums and properties as widely as possible.** The BNF's closed alternatives — `ASC | DESC`, `LEFT
    | RIGHT | FULL`, `FIRST | NEXT`, `ALL | DISTINCT` — become enums. Independent marks are a `bool`, or
    a nullable enum or property.

11. **A key word left out and a default written out are different states.** `SELECT x`, `SELECT ALL x`
    and `SELECT DISTINCT x` need at least `SetQuantifier? Quantifier`, where `null`, `All` and
    `Distinct` differ.

12. **Lists are kept wherever SQL writes a sequence** — statements, select items, arguments, `ORDER
    BY`, `GROUP BY`, common table expressions, merge clauses, table sources. Neither order nor
    duplicates are lost.

13. **No set-like collections for syntax.** Even where an SQL entity is logically a set, the tree holds
    an `IReadOnlyList<T>`: it represents what was written, not a mathematical model.

14. **A fixed sequence of clauses is properties, not a list of clauses.** `FROM → WHERE → GROUP BY →
    HAVING` is already the shape of `Statement.Select`; an `IReadOnlyList<Clause>` there would only make
    the model worse.

15. **The grammar's lists need no classes of their own.** `<column name list>` is an
    `IReadOnlyList<Identifier>`, `<sort specification list>` an `IReadOnlyList<SortItem>`.

16. **The BNF's alias and role rules usually disappear.** `<point in time 1>`, `<point in time 2>`,
    `<row value predicand 1>` and their like become the names of properties — `From`, `To`, `Left`,
    `Right` — not types.

17. **The expression hierarchy is semantically common.** `<numeric value expression>`, `<character
    value expression>`, `<boolean value expression>`, `<term>`, `<factor>` and `<primary>` do not form
    levels of the tree for the sake of type restrictions or precedence: arithmetic, predicates, casts,
    `CASE`, functions and subqueries are all directly `Expression`.

18. **Recursion only where SQL itself recurses** — binary expressions, joins, nested queries, common
    table expressions, row patterns. Recursion the BNF uses only for parsing or precedence does not
    reach the tree by itself.

19. **An embedded sublanguage may have a flat family of its own.** SQL/JSON paths and row patterns are
    reasonably `JsonPathExpression` and `RowPattern`, not forced into `Expression`.

20. **The tree is open to dialects.** SQL:2023 is the base coverage, and nothing in the structure may
    stand in the way of T-SQL, PostgreSQL, Oracle and their extensions — which is why ISO's constraints
    are especially unwelcome encoded as inheritance.

21. **Every node traces to the BNF.** Classes, enums and significant properties say in a comment which
    rules they correspond to or absorb. A full map, `BNF rule → class / property / enum / list /
    erased`, makes the coverage of the whole specification checkable.

22. **Erased does not mean lost.** A rule may be erased only where it carries no information of its
    own: an alias, a classifier, a precedence or validation layer, a technical grouping of the BNF.
    Where a rule records a choice the author made, that choice stays somewhere in the tree.

## How the blank is adapted

Each change to `Sql2023Ast.cs` made to meet the requirements above or the parsers' needs, and why.
Conflicts with what T-SQL already builds are decided with Igor before they are resolved.

**It compiles, on both frameworks the package ships for** (2026-09-15). The blank did not: 176
errors.

- `required` members need `RequiredMemberAttribute` and `CompilerFeatureRequiredAttribute`, which
  netstandard2.0 does not declare; `DotGram.Sql.csproj` asks its polyfill for both, as it already did for
  `IsExternalInit`. The members stay `required` and the records keep their `init` properties: a node
  with ten optional parts reads better as an object initializer than as ten positional arguments.
- A positional parameter may not share its name with a type nested beside its record, and a record whose
  one parameter is of a type with its own name is its own copy constructor. So `Expression.Name` is
  `Expression.Reference(QualifiedName Name)`, `TableElement.Constraint` is `TableElement.TableConstraint`,
  the enum of `RoutineCharacteristic.SavepointLevel` is `SavepointLevelKind`, and three parameters are
  renamed: `JsonPathAccessor.Method(Value)`, `GrantBody.Roles(Names)` and `RevokeBody.Roles(Names)`,
  `CursorAllocationSource.Routine(Designator)`.
- `SearchClause` and `CycleClause` were named and not declared; they are, from `<search clause>` and
  `<cycle clause>`.
- `CastTarget` was named and not declared. `<cast target> ::= <domain name> | <data type>`, and
  `DataType.Domain` already is the first, so a cast's target is a `DataType`.
- The file is in the repository's format — tabs, CRLF, a byte order mark — and the map has no byte
  order mark, which only C# and project files carry.

**What the grammar already reads has somewhere to go** (2026-09-15). Held against every construct
`SqlStandard.gram` recognizes, the blank had no place for a number of them, or kept them without a
word the author wrote (requirement 3).

- *Expressions.* A multiset operator carries its quantifier, so it is `Expression.MultisetOperation` and
  not three values of `BinaryOperator`. `Invocation` gains an aggregate's quantifier, an `ORDER BY`
  inside the brackets, `LISTAGG`'s overflow, `FROM FIRST|LAST`, `RESPECT|IGNORE NULLS` and a
  navigation's `RUNNING|FINAL`; `COUNT (*)`'s star is `Expression.Asterisk`. The functions whose
  arguments are key words rather than a comma list are records of their own — `Substring`,
  `SubstringSimilar`, `Trim`, `Overlay`, `Position`, `Length`, `Extract` (with its own field enum, which
  has `TIMEZONE_HOUR`), `Normalize`, `TranslateUsing` and `Regex` — since an `Argument` cannot say
  `FROM` or `PLACING`. Added: `Collate`, `AtTimeZone` (`AT LOCAL` has no zone), `IntervalQualified` for
  `(a - b) DAY TO SECOND`, `Generalized` for `(a AS t).m ()`, `Treat`, `New`, `Dereference`, `IsTruth`,
  `Match`, the row predicate `Overlaps`, a JSON accessor's `Wildcard` and an element's `TO`, a host
  parameter's indicator, a `CURRENT_TIME`'s precision, the trigraphs of an array, `LIKE_REGEX`'s flag,
  and the JSON functions — `JsonValue`, `JsonQuery`, `JsonObject`, `JsonArray`, `JsonArrayQuery`, the
  two aggregates, `JsonParse`, `JsonScalar`, `JsonSerialize` — with `JsonOutput`, `JsonQuotes`,
  `JsonMember` (`KEY k VALUE v`, `k VALUE v`, `k : v`), `JsonElement` and `JsonNullHandling`. A static
  method's `::` is a `MemberAccessKind`. `UNIQUE`'s nulls are `NullDistinctness?`, since `NULLS DISTINCT`
  may be written.
- *CASE.* A simple `CASE`'s when clause holds a list, `WHEN 1, 2`, and a when operand may be a predicate
  without its left side, `WHEN < 5`: the missing side is `Expression.CaseOperand`, for which nothing is
  written.
- *Queries.* `Statement.Select` gains `Body`, for a query expression whose first operand is `VALUES`,
  `TABLE t` or a query in brackets, with the specification's properties then empty; `Parentheses`, a
  count rather than a flag, since `((SELECT 1))` is two; and `Updatability`, a cursor specification's
  `FOR UPDATE`. A join's kind is nullable — a bare `JOIN` is not `INNER JOIN` — with `Natural` a flag of
  its own rather than four more values, and a partitioned join's two column lists. `TABLE (f (a))` is
  `TableSource.TableFunction` and `MATCH_RECOGNIZE` after a table is `TableSource.RowPatternRecognition`.
- *Statements.* Added `GetDiagnostics`, with its three forms and both item enums from the BNF, and
  `DeclareLocalTemporaryTable`. A dynamic cursor's `OPEN` takes `DynamicArguments`. A routine's SQL body
  is one statement, as `<SQL routine spec>` is; its parameters and result carry `AS LOCATOR`, its result
  `CAST FROM` and a bare `RETURNS TABLE`, its definition `STATIC DISPATCH` and `SPECIFIC METHOD`; a
  method specification says whether it overrides and has a qualified specific name. `CREATE DOMAIN d AS
  INT` keeps its `AS`, and `ON TABLE t` its `TABLE`.
