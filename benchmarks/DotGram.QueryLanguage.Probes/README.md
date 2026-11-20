# Query-language chain probes

Probes of the trees the query-language library would hand to linq2db, run before the library is written. Not part of
the solution, never shipped. `Core/` stands for the library: the row carriers (`QlScalars`, `QlRow<TE>`) and the JSON
writer (`QlJson`) are internal there and the probes reach them only through the trees `Core` builds, so linq2db, in its
own assembly, has to compile calls to internal types, as it would against the library.

```bash
export MSBUILDDISABLENODEREUSE=1
dotnet build benchmarks/DotGram.QueryLanguage.Probes -c Release -nodeReuse:false
cd benchmarks/DotGram.QueryLanguage.Probes
dotnet run --no-build -c Release -- all sqlite        # probes 1-10 and 12; `11` (timing) runs on its own
DOTGRAM_SQLSERVER='Server=…;User Id=…;Password=…;TrustServerCertificate=true' dotnet run --no-build -c Release -- all mssql
DOTGRAM_SQLSERVER='…' dotnet run --no-build -c Release -- drop     # removes the probes' database (QlPchain)
```

`-p:Linq2DbVersion=6.5.0` on the build (and on `dotnet run`) runs them against another linq2db; the default is 6.6.0.
Each probe prints its SQL with parameter values, its rows and one `VERDICT` line per claim; the exit code is 1 when a
claim marked as a stop fails.

| # | Probe |
|---|---|
| 1 | Row cap: `Take(MaxRows)` after a text's own `take`/`skip`, and host paging after the cap |
| 2 | Chains in the bounded carriers and in `object[]` rows |
| 3 | Whole-set aggregates (`GroupBy(r => 0)`) on empty and non-empty input |
| 4 | Values through `StrongBox<T>`, `in @list` |
| 5 | The JSON chain: per-row evaluation, typed reads, allocations, an exception mid-row |
| 6 | Async terminals: `ExecuteReaderAsync` |
| 7 | Host operators over the JSON rows |
| 8 | Two models, one text, under linq2db's query cache |
| 9 | Members linq2db converts (`DateOnly`, an enum mapped to text) |
| 10 | Cost of nested page levels |
| 11 | Allocation and speed, directional |
| 12 | The host's tenant policy under linq2db's query cache |

`Core/Carriers.cs` was written by a script: 32 slots for each of 17 kinds (`Bool`, `BoolN`, `Int`, `IntN`, `Long`,
`LongN`, `Decimal`, `DecimalN`, `Double`, `DoubleN`, `String`, `Timestamp`, `TimestampN`, `TimestampTz`, `TimestampTzN`,
`Guid`, `GuidN`), then eight `object` slots, then `QlRow<TE>`.
