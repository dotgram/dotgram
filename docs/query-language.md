# The query language

`DotGram.QueryLanguage` reads a query that somebody else wrote — a client of an API, a user of a
report screen — and turns it into an `IQueryable` over the host's own data. The text can name only
what the host declared, and only where the host allowed it; the host's LINQ provider translates and
runs the result. The package's [README](https://github.com/dotgram/dotgram/blob/main/src/DotGram.QueryLanguage/README.md)
is the overview; this page is the reference.

The package is built and tested with every change and is not yet published. It is tested with
linq2db over SQLite and SQL Server, with EF Core over SQLite, and over LINQ to Objects; other
providers are not tested.

## The model

A model says what may be named. It is built once and is immutable and thread-safe.

```csharp
var builder = new QlModelBuilder<AppDb>();

builder.Entity("orders", db => db.Orders.Where(o => o.TenantId == db.TenantId))
    .Column("id",      o => o.Id)
    .Column("status",  o => o.Status)
    .Column("total",   o => o.Total)
    .Column("margin",  o => o.Margin, QlUse.Filter);

var model = builder.Build();
```

| Call | What it declares |
| --- | --- |
| `Entity(db => db.Orders)` | an entity named after the context member; any other body needs a name |
| `Entity("orders", db => …)` | an entity under a name; the function is its source, called with the context on every query |
| `Column(o => o.Total)` / `Column("total", o => o.Total)` | a field: a member the provider maps, under its own name or the one given; a value type, a `string` or a `byte[]` — a navigation or a collection is refused |
| `Column(…, QlUse.Filter)` | where the field may be named; null means `Select \| Filter \| Sort` |
| `Format<T>(Func<T, string?>)` | how values of `T` are printed in JSON (below) |
| `Like((s, p) => …)` | what `x like p` means: the provider's own LIKE (below); without it `like` is refused |
| `Build(QlOptions?)` | the model, with its bounds |

**`QlUse`** says where a field may be named: `Select` in what a query returns, `Filter` in a value
a `where` tests, `Sort` in a value an `order by` sorts — directly or through a `let` or a `select`
column. `Filter` and `Sort` disclose a value without `Select`, by comparisons against chosen values
and by order; a field nobody may learn is not declared at all. A field named where its use does
not allow it is reported exactly as a name the model does not have — the same code, message and
position — so an error never tells whether a field exists.

**The source is the policy.** Whatever the source's `IQueryable` leaves out, no text can reach:
the tenant, soft-deleted rows, rows the caller may not see. Everything a text writes is applied
after it.

## The methods

| Method | Returns |
| --- | --- |
| `QueryData(context, text, values)` | `IQueryable<object?[]>`: each row's values in column order, as the provider read them |
| `QueryJsonRow(context, text, values, format)` | `IQueryable<string>`: each row as JSON text, written while the provider reads it |
| `ToJsonAsync(context, text, values, format, cancellationToken)` | the whole result as one JSON document |
| `Describe(text)` | the columns a text returns, the parameters it needs and its warnings; `ToJson()` of it is the same as JSON |

Each call reads and binds the text (the binding is kept, by the exact text, for the next call),
checks the values, calls the sources with the context and builds a new tree. Nothing runs until
the host enumerates what is returned. A text or a value the language refuses throws
`QlException`; `ToJsonAsync` returns it through its task, and `QlException.Error.ToJson()` is the
JSON document of the refusal.

## The language

```text
from o in orders
where o.status in @statuses && total between @low and @high
let net = total - total / 6
order by net desc, id
take 20
select id, status, net
```

- **The head** comes first, in any of three spellings that mean the same: `from orders`,
  `from orders o` or `from orders as o` (the entity, then an alias), and `from o in orders` (the name,
  then the entity). Every source has a name: its alias or the entity's name; fields are written after it
  (`o.total`) or alone. Then any number of clauses in any order, each applied where it is written, as a
  call of its own:

  | Clause | Does |
  | --- | --- |
  | `where <condition>` | keeps the rows where the condition is true |
  | `let <name> = <value>` | adds a named value to the row |
  | `select <value> [as <name>], …` | replaces the row with these columns; after it only they can be named |
  | `order by <value> [asc \| desc] [nulls first \| nulls last], …` | sorts; a second `order by` sorts again; `orderby` is `order by`, `ascending` is `asc` and `descending` is `desc` |
  | `skip <count>`, `take <count>` | pages, where the query's paging belongs; a count is a number or a parameter |

- **`*`** — the projection of the last method — is an item of a `select` list like any other: it
  stands for the columns of the row as it stands, the entity's fields with Select, then the `let`s
  after the head, or the columns of the last `select`, then its `let`s. `select 1 as one, *, total` is one
  column, the fields, then `total`. It stands once in a list; `select *` alone returns the row as it is.
  A query with no `select` returns the same row, `let`s included: `from orders let t = total * 2` is
  `from orders let t = total * 2 select *`. A name the list writes itself wins over any column `*` would give — a
  field, a `let`, or a column of an earlier `select` — that column is left out, and the written one keeps its
  place. A field the rest of the text uses without the right it needs (`order by status` where `status` has no
  Sort) is not given by `*` either, and naming it later is refused as a field the model does not have.
  A column that is not a name still needs `as`. `*` stands alone: not `o.*`, not `* as x`.
- **Values**: the entity's fields, by name or after the alias (`o.total`); names of `let`s and
  `select` columns; parameters `@name`; numbers, which take the type of what they meet (`total > 1.5`);
  strings in `'…'` or `"…"` with the escapes `\\`, `\'`, `\"` and `\uXXXX`; `true`, `false`, `null`;
  `timestamp '2026-01-01 10:00:00'` and `timestamptz '2026-01-01 10:00:00+02:00'`.
- **Operators**, at C#'s precedence: `*` `/` `%`, `+` `-`, the comparisons `==` `!=` `<` `<=` `>`
  `>=`, `&&`, `||`, and the unary `!` and `-`. `x [not] in (a, b)` (any values of the type of `x`), `x [not] in @list`,
  `x [not] between a and b`, `x is [not] null`, `x [not] like p` (below). `!` binds as in C#,
  tighter than any binary operator; where C# and SQL's `NOT` would read a text differently — `!a == b`, `!flag is null` —
  the text is refused and asks for `!(a == b)` or `(!a) == b`.
- **Members**: `Length`, `Contains(s)`, `StartsWith(s)`, `EndsWith(s)`, `ToUpper()`, `ToLower()`,
  `Trim()` of a string; `Year`, `Month`, `Day`, `Hour`, `Minute`, `Second`, `Date` of a timestamp. The
  string may be any string, a literal or a `let` too; what a provider cannot compute it refuses itself.
- **Aggregates**, in a `select` whose columns are all aggregates: `Count(*)`, `Count(x)` (the rows
  where `x` is not null), `Sum(x)`, `Avg(x)`, `Min(x)`, `Max(x)`. They aggregate every row at once
  and give one row — with no rows, `Count` is 0 and the others null. `x` is any value, a constant too:
  `Sum(1)` counts the rows. An integer's average is a
  `double`. There is no `group by` yet.
- **Null** compares as in C#, not as in SQL: `x == null` tests for null, `x != 1` holds where `x` is
  null.
- **Words** of the language are words only where a name cannot stand, so a field may be called
  `order` or `select`; `null`, `true` and `false` are reserved. An alias written without `as` is
  never a word that starts a clause (`where`, `let`, `order`, `orderby`, `select`, `skip`, `take`, `join`, `left`, `on`)
  and never `in`; after `as` any name will do. Names and words ignore case.
  Comments are `--` to the end of the line and `/* … */`.
- A name declared and never used is reported in `Describe(text).Warnings` (`UnusedName`) and is
  not built into the query.

### `like`

`title like @pattern`, `title not like 'War%'` match a string against a pattern, at the
level of a comparison: `a like p && b > 1` is `(a like p) && (b > 1)`, and `a like p like q` is not a
text. Both sides are any string: a field, a `let`, a member of one, a literal or a parameter. What a
provider cannot compute it refuses itself; the language asks nothing of the rows.

The language has no LIKE of its own. The host says what `like` means, once, with its provider's own:

```csharp
builder.Like((s, p) => Sql.Like(s, p, '\\'));               // linq2db
builder.Like((s, p) => EF.Functions.Like(s, p, "\\"));      // EF Core
```

and `x like p` becomes that lambda's body with `x` and `p` in place of its parameters (`not like`, the
body under `!`). The wildcards (`%`, `_`, and SQL Server's `[…]`), the escape character, case,
accents and collation, and what a null does under `not like`, are the provider's and the database's;
the language reads nothing in the pattern. A model without `Like` refuses the operator with
`QlErrorCode.Type`. A pattern is a string like any other: `MaxStringLength` bounds its length, and
nothing bounds the wildcards in it: a leading `%` reads every row, as `Contains` does.

The escape is the one the host's lambda names. A backslash is the usual one — without an `ESCAPE` SQLite
has no way to match a literal `%` — and is written doubled inside the language's strings
(`status like '100\\%'`), as it is in any of them; in a parameter's value it is written once.

**Do not pass raw user text as a pattern.** A pattern's wildcards are live wherever it comes from, a
parameter's value too: a user who types `%` or `_` gets every row, and a leading `%` makes the
database read them all. To find text a user typed, as text, write `title.Contains(@q)` (or
`StartsWith`, `EndsWith`), whose value the provider escapes. `like` is for patterns written as
patterns.

## Values

Parameters are passed by name, without the `@`, in a dictionary. A value is converted to the
parameter's type — the type of what it is compared with — without loss and without culture, or
refused with `QlErrorCode.Parameter`:

| Parameter | Takes |
| --- | --- |
| `bool` | `bool` |
| `int`, `long` | any integer in range; a `decimal` or a `double` with an integral value in range |
| `decimal` | any integer; `decimal`; `double`, by its shortest decimal text |
| `double` | any integer up to 2^53; `decimal`; `double` |
| `string` | `string` |
| `DateTime` | `DateTime`; an ISO 8601 string without an offset, or with `Z` |
| `DateTimeOffset` | `DateTimeOffset`; a UTC `DateTime`; an ISO 8601 string with an offset or `Z` |
| `Guid` | `Guid`; a string in the `"D"` format |
| any other type | a value of that type |

A list after `in @list` is any enumerable but a string or a `byte[]`, each item converted the same
way. `null` is taken where the parameter may be null. `Describe(text).Parameters` gives each
parameter's type, whether it is a list and whether it takes null.

## Types outside the language

A member of a type that is not one of the nine — an enum, `DateOnly`, `byte[]`, a NodaTime type —
is a field like any other, reported as `QlType.Other`. It is compared with a
field or a parameter of the same type (the operators its type has), tested with `in` and
`is null`, sorted, and taken by `Count`, `Min` and `Max`. `QueryData` returns it as itself.
Whether the provider can translate what the text does with it is the provider's to say.

## JSON

`QueryJsonRow` writes each row as an array in column order or as an object keyed by column name
(`QlJsonFormat`):

```json
[1,"Ann \"A\" Lee",1250.50,"2026-11-01T09:30:00.0000000"]
{"id":1,"name":"Ann \"A\" Lee","total":1250.50,"created":"2026-11-01T09:30:00.0000000"}
```

`ToJsonAsync` puts the rows in one document, and the document says how it went in members of its
own, never by the shape of what it holds. Its first member is `status`, its last `result`:

```json
{"status":"started","columns":["id","name"],"rows":[[1,"Ann"],[2,"Bob"]],"count":2,"truncated":false,"result":"completed"}
```

The object form is the same document, its rows objects keyed by column name:

```json
{"status":"started","columns":["id","name"],"rows":[{"id":1,"name":"Ann"},{"id":2,"name":"Bob"}],"count":2,"truncated":false,"result":"completed"}
```

Where `MaxRows` left rows out, `truncated` says so:

```json
{"status":"started","columns":["id"],"rows":[[1],[2],…],"count":1000,"truncated":true,"result":"completed"}
```

A refused text or value is a document of its own, `QlError.ToJson()`:

```json
{"status":"error","error":{"code":"UnknownName","message":"Unknown name `totl`.","position":21}}
```

- `status` is `"started"` — the text was accepted and its rows follow — or `"error"` — nothing was
  read, and the document holds the `error` alone. `result` is `"completed"` where every row was
  read; a document without `result` was cut off on its way. `ToJsonAsync` writes its document
  whole or not at all: a failure while it reads the rows fails its task, so `"failed"`, the other
  `result`, is kept for a writer that sends the rows as it reads them.
- `columns` is there in both forms, so a client knows the columns of a result with no rows.
  `count` is the number of rows in `rows`.
- `truncated` is true only where `MaxRows` cut the result: `ToJsonAsync` reads one row past the
  bound to tell, and drops it. That extra row is formatted like the others, so a host `Format<T>`
  that throws on it fails the document, though the row is not written. A `take` of the text is what
  was asked for, not a cut. With `MaxRows` null it is always false.
- `warnings`, after `columns`, only where the text has any:
  ``"warnings":[{"code":"UnusedName","message":"`x` is never used.","position":19}]``. A warning is a
  hint, not a status.
- `error.code` is the name of the `QlErrorCode`. `position` is the zero-based offset in the text, in
  UTF-16 code units as JavaScript counts them, and is left out where there is none. One more code is
  kept for JSON, `Failed`: a failure of the provider after the rows started, which no `QlException`
  carries.
- The provider's own exceptions — a timeout, a lost connection, a format that threw — are not
  written: `ToJsonAsync` passes them on, and their text, which may name tables or show SQL, never
  enters a document. What the host answers with is the host's.
- A client skips members it does not know, so that members can be added without a version.
- The document is not `application/problem+json`, whose `status` is a number. A host that wraps it
  in problem details puts it in a member of its own.

`Describe(text).ToJson()` describes a text in the same envelope: `status` first, `"started"`, and
`result` last, `"completed"`. A description reads no row, so it is whole when it is written, and
its `result` is always `"completed"`.

```json
{"status":"started","columns":[{"name":"id","type":"Int","nullable":false},{"name":"total","type":"Decimal","nullable":true}],"parameters":[{"name":"low","type":"Decimal","nullable":true,"list":false}],"result":"completed"}
```

A `type` is the name of the `QlType`, and `warnings` follow the parameters, before `result`, where
there are any.

Values are written the same way everywhere:

- `bool` is `true`/`false`; numbers are culture-free; a `decimal` has the scale the provider read
  it with (`1250.50` from SQL Server's `decimal(10,2)`); a `long` past 2^53 is written exactly,
  which JavaScript cannot read exactly.
- A `double` is the shortest text that reads back the same value on .NET Core 3.0 and later, and
  seventeen digits on .NET Framework and other consumers of the netstandard2.0 build; `NaN`,
  `Infinity` and `-Infinity` are strings.
- `DateTime` and `DateTimeOffset` are ISO 8601 (`"O"`), a `Guid` is `"D"`, null is `null`.
- Strings escape `"`, `\` and the control characters (with `\n`-style short forms where JSON has
  them), U+2028 and U+2029, `<`, `>`, `&` and `'` (so the text is safe inside HTML), and a
  surrogate without its pair. Everything else is written as it is. The names and messages of a
  document are escaped the same way.
- A type outside the language prints as `ToString(null, CultureInfo.InvariantCulture)` where it is
  `IFormattable`, `ToString()` otherwise: an enum by its name, `DateOnly` as `01/02/2026`, `byte[]` as
  `System.Byte[]`.
- `Format<T>` overrides both: its result is printed as a JSON string, null as `null`. It is matched
  on the exact type (a format for `int` serves `int?`); `Nullable<T>`, `string`, `object`,
  interfaces and abstract types are refused. It runs inside the provider's reading of the rows,
  on whatever threads read them: keep it pure and thread-safe.

The JSON is written while the provider reads each row, from the values as it reads them, with no
boxing and no object between the reader and the text: one string a row. `Count()` and `Any()` may
follow `QueryJsonRow`; paging belongs in the query text (`skip`, `take`). A `Where` or an `OrderBy`
over the JSON text cannot be translated and throws, and `Distinct` or `Union` compare the columns the
provider reads, not the text.
`ToJsonAsync` reads through the provider's `IAsyncEnumerable` where it has one, and holds the whole
document in memory.

## Bounds

`QlOptions` bounds the cost a text can choose. The bounds on the text — its length, nesting, depth,
operands, lists and columns — are counted on the text as written, before anything is looked up.
`MaxNodes` counts it once more as it expands: a name counts at every use, as the size of what it
names.

| Option | Default | Bounds |
| --- | ---: | --- |
| `MaxTextLength` | 8192 | characters of text |
| `MaxSyntaxDepth` | 64 | nesting as written, brackets included |
| `MaxNodes` | 100 | parts of the query, each name counted as what it names |
| `MaxTreeDepth` | 32 | depth of one expression |
| `MaxArithmeticOperands` | 8 | operands of one arithmetic expression |
| `MaxInList` | 100 | values of one `in` list, written or passed |
| `MaxColumns` | 32 | columns of a `select` and of a result |
| `MaxRows` | 1000 | rows a query returns: a `Take` before the final projection; null for none. `ToJsonAsync` takes one more to tell `truncated` |
| `MaxSkip` | 10000 | rows all the `skip`s of a text pass over together |
| `MaxStringLength` | 4096 | characters of one string, literal or value |
| `MaxParameterBytes` | 16384 | bytes of all the values of one call |

The bounds limit the size of what reaches the provider, not the time the database takes: the
command timeout and cancellation are the host's.

## Errors

`QlException.Error` has a `Code`, a `Message` for whoever wrote the text, and a `Position` in it.

| Code | When |
| --- | --- |
| `Syntax` | the text is not a query |
| `UnknownName` | a name the model does not have, or a field where it may not be named |
| `Type` | a value of the wrong type where it stands, an operator its type does not have |
| `Limit` | a bound of `QlOptions` |
| `Parameter` | a value missing, of a type that does not convert, or too large |
| `DivisionByZero`, `Overflow` | arithmetic of literals and parameters, computed before the query runs |

Anything else — a timeout, a database error, a value the provider cannot read, an exception from
a format — is the provider's or the host's own exception. `Error.ToJson()` is the refusal as a JSON
document (see JSON above).

## Providers

- **SQLite** keeps no scale for a `decimal` (`1250.5`, not `1250.50`), compares a stored `decimal`
  with a computed one in its own way, and loses the offset of a `DateTimeOffset`.
- A row of strings only is written over a second read of its first string, which costs a string a
  row; any column of a value type avoids it.
- **`like`** is the provider's LIKE as the host's lambda calls it. Under linq2db, `not like` adds no
  `IS NULL`: a null is in neither `x like p` nor `x not like p`, where `x != 'a'` holds for it. SQL
  Server reads `[…]` in a pattern as a class of characters and SQLite as the characters themselves;
  SQLite ignores the case of ASCII letters only; whether `é` matches `e` is the collation's.
- `x in @list` is written by linq2db as a list of literals: one cached query, but a new SQL text,
  and a new plan in the database, for each set of values.
- linq2db (SQLite, SQL Server), EF Core (SQLite) and LINQ to Objects are tested. EF Core evaluates
  the closed parts of a projection ahead of the rows, and the JSON writer is still called a row at
  a time; its `like` is `EF.Functions.Like(s, p, "\\")`. EF Core over SQLite keeps a `decimal` as
  text, so its scale is the text's.
