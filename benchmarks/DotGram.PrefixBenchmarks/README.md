# Small-grammar prefix-table comparison

This standalone diagnostic harness compares the previous strategy
(`PrefixTables = false`) with the default table strategy (`PrefixTables = true`). It contains six paired synthetic grammars and can load two
saved builds of DotGram.Web as no-op controls. It verifies the option in each
Web assembly's attribute metadata before measuring.

[Results and limitations](results/2026-09-16-small-prefix-tables.md).

## Run

Set `PrefixTables = false` on all thirteen `[Gram]` attributes in src/DotGram.Web,
build in Release/net10.0 and preserve that DLL as the historical default assembly.
Set `PrefixTables = true` explicitly, build again and preserve the table assembly,
then restore the source attributes. Tables are now the compiler default; labels
in the historical results and harness still call the previous strategy `default`.
The harness rejects swapped or incompletely configured assemblies.

```powershell
dotnet build benchmarks/DotGram.PrefixBenchmarks -c Release
$env:DOTNET_TieredCompilation = '0'
$app = './benchmarks/DotGram.PrefixBenchmarks/bin/Release/net10.0/DotGram.PrefixBenchmarks.exe'
& $app <default-web-dll> <table-web-dll> -1 # Validate only.
& $app <default-web-dll> <table-web-dll> 0  # One measured round.
```

Repeat with round numbers 0 through 4 in separate processes. Odd rounds reverse
the variant order. Preserve the same runtime configuration for both variants;
remove DOTNET_TieredCompilation to reproduce the exploratory default-tiering runs.
No profiler, build, or test process should run during timing.
