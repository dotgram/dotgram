<!--
  Agents: the skill for this package is skills/dotgram-query-language/SKILL.md in the package
  directory — what the host declares, what a text may say, what the host still owns. Read it before
  writing code against the package.
-->
# DotGram.QueryLanguage

A small query language for text you do not trust. A query names only the entities and fields
the host exposes, each only where the host allows it, and becomes an `IQueryable` that the
host's own LINQ provider runs. The library never opens a connection.

```text
from orders
where status in @statuses && total >= @min
order by created desc, id
take 20
select id, total, created
```

Declare what may be queried, once:

```csharp
var builder = new QlModelBuilder<AppDb>();

builder.Entity("orders", db => db.Orders.Where(o => o.TenantId == db.TenantId))   // the policy lives here
    .Column("id",      o => o.Id)
    .Column("status",  o => o.Status)
    .Column("total",   o => o.Total)
    .Column("created", o => o.Created)
    .Column("margin",  o => o.Margin, QlUse.Filter);   // may be tested in `where`, never returned

var model = builder.Build();   // immutable, thread-safe: keep one
```

Then run what somebody typed:

```csharp
var json = await model.ToJsonAsync(db, "from orders where total > 100 order by total desc take 2 select id, total");

// {"columns":["id","total"],"rows":[[3,250.00],[1,120.50]]}
```

`QueryData` returns the rows as `object?[]`, `QueryJsonRow` as one JSON string a row, and
`Describe` says what a text returns and needs without running it. Each returns an
`IQueryable` the host executes: synchronously, with `ToListAsync`, or row by row.

## A web endpoint

The usual shape: text and values come in as JSON, the result goes out as JSON, and a refused
text is the client's error.

```csharp
app.MapPost("/query", async (QueryRequest request, AppDb db, CancellationToken cancellationToken) =>
{
    var values = request.Values?.ToDictionary(v => v.Key, v => Loose(v.Value));

    try
    {
        return Results.Text(await model.ToJsonAsync(db, request.Text, values, cancellationToken: cancellationToken), "application/json");
    }
    catch (QlException e)
    {
        return Results.BadRequest(new { e.Error.Code, e.Error.Message, e.Error.Position });
    }
});

record QueryRequest(string Text, Dictionary<string, JsonElement>? Values);

// A JSON value as the library takes it: it converts to each parameter's type without loss, so read
// a number as exactly as JSON wrote it — an integer, then a decimal, a double only as a last resort.
static object? Loose(JsonElement value)
{
    switch (value.ValueKind)
    {
        case JsonValueKind.Number:
            if (value.TryGetInt64(out var integer))
                return integer;

            if (value.TryGetDecimal(out var exact))
                return exact;

            return value.GetDouble();

        case JsonValueKind.String: return value.GetString();
        case JsonValueKind.True:   return true;
        case JsonValueKind.False:  return false;
        case JsonValueKind.Array:  return value.EnumerateArray().Select(Loose).ToList();
        default:                   return null;
    }
}
```

## What the host still owns

The library decides what a text may *name*; everything about *running* it stays with the host.

1. **Whose rows.** The entity's source is the policy: `db => db.Orders.Where(o => o.TenantId == db.TenantId)`.
   It is called with the context on every query, and a text cannot get around it.
2. **What each field discloses.** `QlUse.Filter` and `QlUse.Sort` disclose a value as surely as
   `Select` does, by comparison and by order. A field nobody may learn is not exposed at all. A field
   used where it is not allowed is reported exactly as one the model does not have.
3. **Time.** The command timeout, cancellation, and how often a client may ask are the host's.
   `QlOptions` bounds the size of a text and of the tree it becomes, not the time the database takes.
4. **Size.** `MaxRows` (1000) bounds the rows; nothing bounds the size of the values in them.
   `ToJsonAsync` holds the whole document in memory — stream `QueryJsonRow` for large results. Paging
   belongs in the text (`skip`, `take`).
5. **The provider's errors.** A database error, a timeout or a value the provider cannot read comes
   out as the provider's own exception. Only `QlException` is the text's fault.

## Types and formats

The nine types of the language — `bool`, `int`, `long`, `decimal`, `double`, `string`, `DateTime`,
`DateTimeOffset`, `Guid`, nullable or not — are printed in JSON as JSON: numbers culture-free,
`decimal` with the scale the provider read, dates in ISO 8601 (`"O"`), `NaN` and the infinities as
strings. A member of any other value type (and `byte[]`) is an ordinary field: compared with a parameter of its own
type, sorted, returned as itself by `QueryData`, and printed in JSON by its invariant `ToString` — which
for `byte[]` is `"System.Byte[]"` and for `DateOnly` `"01/02/2026"`, not ISO 8601. Give such a type a format,
and override a built-in one the same way:

```csharp
var builder = new QlModelBuilder<AppDb>()
    .Format<Instant>(i => InstantPattern.ExtendedIso.Format(i))   // NodaTime
    .Format<byte[]>(b => Convert.ToBase64String(b))
    .Format<DateTime>(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
```

A format runs inside the provider's reading of the rows, on many threads at once: keep it pure.

## More

The language, every bound, the JSON rules and the differences between providers are on
[the query language page](https://github.com/dotgram/dotgram/blob/main/docs/query-language.md).
Tested with linq2db over SQLite and SQL Server; EF Core is not tested.

The package is built and tested with every change and is not yet published.
