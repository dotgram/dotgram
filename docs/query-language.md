# The query language

`DotGram.QueryLanguage` reads a query that somebody else wrote — a client of an API, a user of a
report screen — and turns it into an `IQueryable` over the host's own data. The text can name only
what the host declared, and only where the host allowed it; the host's LINQ provider translates and
runs the result. The package's [README](https://github.com/dotgram/dotgram/blob/main/src/DotGram.QueryLanguage/README.md)
is the overview; this page is the reference.

The package is built and tested with every change and is not yet published. It is tested with
linq2db over SQLite and SQL Server; other providers, EF Core among them, are not tested.

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
| `Describe(text)` | the columns a text returns, the parameters it needs and its warnings |

Each call reads and binds the text (the binding is kept, by the exact text, for the next call),
checks the values, calls the sources with the context and builds a new tree. Nothing runs until
the host enumerates what is returned. A text or a value the language refuses throws
`QlException`; `ToJsonAsync` returns it through its task.

## The language

```text
from orders o
where o.status in @statuses && total between @low and @high
let net = total - total / 6
order by net desc, id
take 20
select id, status, net
```

- **`from <entity> [[as] alias]`** comes first. Then any number of clauses in any order, each
  applied where it is written, as a call of its own:

  | Clause | Does |
  | --- | --- |
  | `where <condition>` | keeps the rows where the condition is true |
  | `let <name> = <value>` | adds a named value to the row |
  | `select <value> [as <name>], …` or `select *` | replaces the row with these columns; after it only they can be named |
  | `order by <value> [asc \| desc] [nulls first \| nulls last], …` | sorts; a second `order by` sorts again |
  | `skip <count>`, `take <count>` | pages, where the query's paging belongs; a count is a number or a parameter |

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
  `order` or `select`; `null`, `true` and `false` are reserved. Names and words ignore case.
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

`QueryJsonRow` and `ToJsonAsync` write a row as an array in column order or as an object keyed by
column name (`QlJsonFormat`):

```json
[1,"Ann \"A\" Lee",1250.50,"2026-11-01T09:30:00.0000000"]
{"id":1,"name":"Ann \"A\" Lee","total":1250.50,"created":"2026-11-01T09:30:00.0000000"}
```

and `ToJsonAsync` the whole result as `{"columns":["id","name","total","created"],"rows":[[…],…]}`
or `[{…},…]`; with no rows, `"rows":[]` or `[]`.

- `bool` is `true`/`false`; numbers are culture-free; a `decimal` has the scale the provider read
  it with (`1250.50` from SQL Server's `decimal(10,2)`); a `long` past 2^53 is written exactly,
  which JavaScript cannot read exactly.
- A `double` is the shortest text that reads back the same value on .NET Core 3.0 and later, and
  seventeen digits on .NET Framework and other consumers of the netstandard2.0 build; `NaN`,
  `Infinity` and `-Infinity` are strings.
- `DateTime` and `DateTimeOffset` are ISO 8601 (`"O"`), a `Guid` is `"D"`, null is `null`.
- Strings escape `"`, `\` and the control characters (with `\n`-style short forms where JSON has
  them), U+2028 and U+2029, `<`, `>`, `&` and `'` (so the text is safe inside HTML), and a
  surrogate without its pair. Everything else is written as it is.
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
| `MaxRows` | 1000 | rows a query returns: a `Take` before the final projection; null for none |
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
a format — is the provider's or the host's own exception.

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
- Only linq2db is tested. A provider that evaluates closed parts of a projection ahead of the rows
  (EF Core does) is not known to keep the JSON writer per row.
