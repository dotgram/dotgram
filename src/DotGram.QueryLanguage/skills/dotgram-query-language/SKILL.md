---
name: dotgram-query-language
description: Let users or clients query a .NET application's data with untrusted query text, safely, through DotGram.QueryLanguage — QlModelBuilder declares the entities and fields a text may name and where, QlModel.QueryData, QueryJsonRow, ToJsonAsync and Describe turn a text into an IQueryable the host's LINQ provider (linq2db) runs. Use when a project references the DotGram.QueryLanguage package, or when asked to expose filtering, sorting and paging over a database to API clients or end users without letting them write SQL or C#. Not a SQL dialect, not a general expression language, and not an executor: the host runs the query.
---

# DotGram.QueryLanguage

A text such as `from orders where total > @min order by created desc take 20 select id, total` is
read, checked against a model the host declared, and turned into an `IQueryable` over the host's
own source. Everything public is in the `DotGram.QueryLanguage` namespace. The library opens no
connection and runs nothing; the host executes what it returns.

The [README][readme] is the overview and the [query language page][page] the reference. This is
the order to decide things in, and the mistakes that are easy to make.

[readme]: https://github.com/dotgram/dotgram/tree/main/src/DotGram.QueryLanguage
[page]: https://github.com/dotgram/dotgram/blob/main/docs/query-language.md

## 1. Declare the model once

```csharp
using DotGram.QueryLanguage;

var builder = new QlModelBuilder<AppDb>();

builder.Entity("orders", db => db.Orders.Where(o => o.TenantId == db.TenantId))
    .Column("id",      o => o.Id)
    .Column("status",  o => o.Status)
    .Column("total",   o => o.Total)
    .Column("created", o => o.Created)
    .Column("margin",  o => o.Margin, QlUse.Filter);

QlModel<AppDb> model = builder.Build();
```

- **Build once, at startup**, and keep the model: it is immutable and thread-safe, and it caches
  what each text binds to. Building one per request repeats all of that.
- **The source is the security policy.** Put the tenant, soft-delete and permission filters in
  the `Func` passed to `Entity`. It is called with the context on every query; a text can only
  narrow what it returns. Never rely on the text to filter by tenant.
- `Entity(db => db.Orders)` without a name takes the member's name and accepts only a bare member
  of the context; anything else needs a name: `Entity("orders", db => …)`.
- **`Column` exposes one mapped member** of a value type, `string` or `byte[]`; a navigation or a
  collection is refused — expose the members of it the text may name. `use` null means `Select | Filter | Sort` and never
  more. Narrow it where a value must not be returned (`QlUse.Filter`) — but `Filter` and `Sort`
  still disclose a value by comparison and order. A field nobody may learn is simply not declared.
- A field used where its `QlUse` does not allow it is reported exactly as an unknown name: same
  code, message and position. Do not "improve" that error on the host side.
- Names are matched ignoring case, and must be `[A-Za-z_][A-Za-z0-9_]*`; `null`, `true` and `false`
  are not names. Conflicts and bad members throw `ArgumentException` from `Build` or the call.
- `builder.Like((s, p) => Sql.Like(s, p, '\\'))` (linq2db; EF Core `EF.Functions.Like(s, p, "\\")`)
  says what `x like p` means: that body with the two strings in place of `s` and `p`. Without it
  the model refuses `like`. Pattern, wildcards, escape, case and collation are the database's.
- `QlOptions` (passed to `Build`) bounds texts. Leave the defaults unless there is a reason; set
  `MaxRows` to what one response may carry.

## 2. Pick the method

```csharp
IQueryable<object?[]> data = model.QueryData(db, text, values);          // values in column order
IQueryable<string>    rows = model.QueryJsonRow(db, text, values);       // one JSON text a row
string                doc  = await model.ToJsonAsync(db, text, values, cancellationToken: ct);
QlQueryInfo           info = model.Describe(text);                       // columns, parameters, warnings
```

- **Nothing runs until the host enumerates.** `QueryData` and `QueryJsonRow` return the provider's
  `IQueryable`: use `ToList()`, linq2db's `ToListAsync()`, or `await foreach`.
- `ToJsonAsync` gives
  `{"status":"started","columns":[…],"rows":[[…],…],"count":N,"truncated":false,"result":"completed"}`,
  the rows as arrays (`QlJsonFormat.Array`, the default) or objects (`QlJsonFormat.Object`), and holds
  the whole document in memory. For large results stream `QueryJsonRow`.
- Read the status from the document, not from its shape: `status` first, `result` last (a document
  without it was cut off), `truncated: true` where `MaxRows` left rows out. An empty `rows` is just
  an empty result.
- **Do not add `Where`, `OrderBy` or paging over the returned `IQueryable`.** Filters and orders over
  the JSON text cannot be translated and throw; paging belongs in the text (`skip`, `take`).
  `Count()` and `Any()` are fine.

## 3. Pass values, not text

Parameters are written `@name` in the text and passed by name without the `@`:

```csharp
var values = new Dictionary<string, object?> { ["min"] = 100, ["statuses"] = new[] { "paid", "new" } };

var rows = model.QueryData(db, "from orders where total > @min && status in @statuses select id", values).ToList();
```

- **Never splice user values into the text.** The value of a parameter is a parameter of the SQL.
- A value converts to the parameter's type only without loss and without culture: any integer for
  an `int` in range, any integer, `decimal` or `double` for a `decimal`, an ISO 8601 string for a
  `DateTime`, a `"D"` string for a `Guid`. Anything else is `QlErrorCode.Parameter`.
- From JSON, read numbers as `long`, then `decimal`, and `double` only as a last resort (the README's
  `Loose`), so that nothing is rounded before the library sees it.
- A list for `in @list` is any enumerable except a string or a `byte[]`, at most `MaxInList` long.
- `Describe(text).Parameters` says each parameter's type, whether it is a list and whether it
  takes null — for building a form, not needed for a call.

## 4. Handle errors in two kinds

```csharp
try
{
    return Results.Text(await model.ToJsonAsync(db, text, values, cancellationToken: ct), "application/json");
}
catch (QlException e)          // the text or a value: the client's error
{
    return Results.Text(e.Error.ToJson(), "application/json", statusCode: 400);
}
```

- `QlException` carries `Error.Code` (`Syntax`, `UnknownName`, `Type`, `Limit`, `Parameter`,
  `DivisionByZero`, `Overflow`), a message meant for whoever wrote the text, and the position in it.
  `Error.ToJson()` is `{"status":"error","error":{"code":"…","message":"…","position":N}}`.
- Everything else — a timeout, a database error, a value the provider cannot read, a host format
  that throws — is the provider's or the host's exception and is not the client's fault.
- `Describe(text).Warnings` lists names declared and never used (`UnusedName`); they never stop a query.

## 5. What the language says

- The head is `from <entity> [[as] alias]` or `from <alias> in <entity>`; the three spellings mean the same, and
  every source has a name: its alias or the entity's name. `orderby` is `order by`, `ascending` is `asc`,
  `descending` is `desc`. Then any number of `where`, `let n = …`, `select`, `order by`, `skip`, `take`,
  in any order; each is applied where it is written. `select` replaces the row: after it, only its
  columns can be named. `*` in the list is the projection of the last method (the row's columns, once);
  a column written by name wins over the field of the same name that `*` would give.
- C# operators: `==` `!=` `<` `<=` `>` `>=` `&&` `||` `!` `+` `-` `*` `/` `%`; and `in (…)`, `in @list`,
  `between … and …`, `is [not] null`, `like` (the host's LIKE, above), each with `not`. Write
  `!(a == b)` or `(!a) == b`: `!a == b` without brackets is refused.
- Strings in `'…'` or `"…"`, escapes `\\`, `\'`, `\"`, `\uXXXX`; `timestamp '2026-01-01 10:00'`,
  `timestamptz '…+02:00'`. Comments `--` and `/* */`.
- Members: `Length`, `Contains`, `StartsWith`, `EndsWith`, `ToUpper()`, `ToLower()`, `Trim()` on
  strings, which may be literals too; `Year` … `Second`, `Date` on dates.
- Aggregates over all rows in a `select` whose columns are all aggregates: `Count(*)`, `Count(x)`,
  `Sum`, `Avg`, `Min`, `Max` of any value, a constant too; no rows give one row with `Count` 0 and the others null. No `group by`,
  `join` or `distinct` yet.
- Null compares as in C#, not as in SQL: `x == null` tests for null, and `x != 1` holds where `x` is null.

## 6. Types and formats

- `bool`, `int`, `long`, `decimal`, `double`, `string`, `DateTime`, `DateTimeOffset`, `Guid` (and
  nullable) are the language's types. Members of other types (enums, `DateOnly`, `byte[]`, NodaTime)
  are fields too: compared with a parameter of the same type, sorted, returned as themselves by
  `QueryData`, printed by `ToString()` in JSON.
- `builder.Format<T>(Func<T, string?>)` prints a type as a JSON string, ahead of the built-in
  print: register one for `byte[]` (Base64), NodaTime types and anything whose `ToString` is not
  what clients want. Exact type only — `Format<int?>`, `Format<string>`, `Format<object>` and
  interfaces are refused — and keep it pure: it runs inside the provider's reading of the rows.
- Whether a string may be null comes from the host's nullable annotations.

## 7. Mistakes worth avoiding

- Putting the tenant filter in the text, or in a `where` the host appends to the result.
- Building a model per request.
- Exposing a field with `Filter` "to hide it": it is disclosed by comparisons.
- Catching `Exception` and returning it to the client: only `QlException` is the client's.
- Expecting `ToJsonAsync` to bound memory: `MaxRows` bounds rows, not their size.
- Passing raw user text as a `like` pattern: its `%` and `_` are wildcards. Search for what a user
  typed with `x.Contains(@q)`, which the provider escapes; `like` is for patterns.
- Expecting EF Core to behave like linq2db: linq2db (SQLite, SQL Server), EF Core (SQLite) and
  LINQ to Objects are tested, and the providers differ where the reference says so.
