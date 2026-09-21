using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Sql.TransactSql;

namespace DotGram.Benchmarks;

/// <summary>JSON constructor dispatch, including common SQL, long projections and refusals.</summary>
[MemoryDiagnoser]
public class SqlJsonBenchmarks
{
	public static IEnumerable<string> Inputs =>
	[
		"SELECT 1",
		"SELECT JSON_ARRAY(1, 2, 3)",
		"SELECT JSON_OBJECT('x': 1, 'y': 2)",
		"SELECT jSoN_aRrAy(1, NULL NULL ON NULL)",
		"SELECT " + string.Join(", ", Enumerable.Repeat("JSON_ARRAY(1, 2, 3)", 128)),
		"SELECT " + string.Join(", ", Enumerable.Repeat("JSON_OBJECT('x': 1, 'y': 2)", 128)),
		"SELECT JSON_ARRAY(NULL ON NULL)",
		"SELECT JSON_OBJECT('x': 1",
	];

	[ParamsSource(nameof(Inputs))]
	public string Input { get; set; } = "SELECT 1";

	[Benchmark]
	public bool Generated()
	{
		return TransactSqlParser.TryParseStatement(Input).IsSuccess;
	}
}
