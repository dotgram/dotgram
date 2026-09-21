using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// A few of the examples' entry points, each side's build read in the same rounds (fifteen of about ten milliseconds,
	/// turn and turn about, after three seconds of warm-up each), unpinned. The minimum of the rounds is what is quoted.
	/// </summary>
	public static void RoughExamples(string beforeDir, string afterDir)
	{
		var before = new PairedSide("before", beforeDir);
		var after  = new PairedSide("after", afterDir);

		var quoted = "\"" + string.Concat(Enumerable.Repeat("a \"\"quoted\"\" text ", 20)) + "\"";
		var lines  = string.Concat(Enumerable.Range(0, 20).Select(i => $"name{i} = \"a b {i}\"\nport{i} = {i}\nmode{i} = fast\n"));

		var rows = new List<(string Name, Func<int> Before, Func<int> After)>
		{
			Row("MetricsLine.Read", "MetricsLine", "Read", "cpu=0.75 mem=2048 host=\"db-1\" up=36000 # sampled"),
			Row("MetricsLine.Read, quoted", "MetricsLine", "Read", "name=\"a \"\"quoted\"\" value\" other=\"b\" third=\"c d e\""),
			Row("FilterFile.ParseFilter", "FilterFile", "ParseFilter", "age > 18 and score = 100 and name = \"Bob\""),
			Row("Filters.ParseFilter", "Filters", "ParseFilter", "name = \"Bob\" and ci(city = \"berlin\")"),
			Row("Lexemes.ParseQuoted", "Lexemes", "ParseQuoted", "\"a \"\"quoted\"\" text\""),
			Row("Lexemes.ParseQuoted, long", "Lexemes", "ParseQuoted", quoted),
			Row("SettingsFile.ParseSettings", "SettingsFile", "ParseSettings", "name = \"a b\"\nport = 8080\nmode = fast\n"),
			Row("SettingsFile.ParseSettings, 60 lines", "SettingsFile", "ParseSettings", lines),
		};

		// The fold examples (sql-39's seam): the expressions the examples read, and the benchmarks' Levels where the side has it.
		var sum = "1 + 2 * 3 - (4 / 2) * -5 + 6 * (7 - 8) / 9";

		foreach (var (name, type, method, text) in new[]
		{
			("Calculator.EvaluateInt", "Calculator", "EvaluateInt", sum),
			("Calculator.BuildTree", "Calculator", "BuildTree", sum),
			("ArithmeticTree.Read", "ArithmeticTree", "Read", sum),
			("ClampedExample.Read", "ClampedExample", "Read", "clamp(x + 1, 0, 10) + (x + 2) + clamp(3, x, 9)"),
			("Levels.Levelled", "Levels", "Levelled", sum),
			("JsonParser.Read", "JsonParser", "Read", "{\"id\": 12345, \"name\": \"dotgram\", \"tags\": [\"a\", \"b\", \"c\"], \"nested\": {\"x\": 1.5, \"y\": null, \"z\": true}, \"list\": [1, 2, 3, 4, 5]}"),
			("GramGrammar.ParseFile", "GramGrammar", "ParseFile", "Sum = l: Sum & '+' & r: Product | Product\nProduct = ['0'..'9']+\n\nparse Sum\n"),
			("Levels.Levelled, deep", "Levels", "Levelled", string.Concat(Enumerable.Repeat("(1 + 2 * 3) - ", 20)) + "4"),
		})
		{
			try
			{
				rows.Add(Row(name, type, method, text));
			}
			catch (Exception exception) when (exception is InvalidOperationException or AmbiguousMatchException)
			{
				Console.WriteLine($"<!-- {name} left out: {exception.Message} -->");
			}
		}

		(string, Func<int>, Func<int>) Row(string name, string type, string method, string text)
		{
			return (name, before.Example(type, method, text), after.Example(type, method, text));
		}

		Console.WriteLine("| entry point | before us (min, median) | after us (min, median) | after / before, min | after / before, median |");
		Console.WriteLine("| --- | ---: | ---: | ---: | ---: |");

		foreach (var (name, b, a) in rows)
		{
			try
			{
				if (b() != 1 || a() != 1)
					throw new InvalidOperationException($"{name}: a side returned nothing.");
			}
			catch (System.Reflection.TargetInvocationException exception)
			{
				Console.WriteLine($"| {name} | left out: {exception.InnerException?.Message} | | | |");

				continue;
			}

			foreach (var call in new[] { b, a })
			{
				var warm = Stopwatch.StartNew();

				while (warm.Elapsed < TimeSpan.FromSeconds(2))
					call();
			}

			var probe = Stopwatch.StartNew();
			var n     = 0;

			while (probe.Elapsed < TimeSpan.FromMilliseconds(50))
			{
				b();
				n++;
			}

			var batch  = Math.Max(1, n / 5);
			var rounds = new[] { new List<double>(), new List<double>() };

			for (var round = 0; round < 15; round++)
			{
				for (var k = 0; k < 2; k++)
				{
					var side  = (k + round) % 2;
					var call  = side == 0 ? b : a;
					var watch = Stopwatch.StartNew();

					for (var i = 0; i < batch; i++)
						call();

					rounds[side].Add(watch.Elapsed.TotalMilliseconds * 1000 / batch);
				}
			}

			var x = rounds[0].Order().ToList();
			var y = rounds[1].Order().ToList();

			Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
				$"| {name} | {x[0]:F3}, {x[7]:F3} | {y[0]:F3}, {y[7]:F3} | {y[0] / x[0]:F3} | {y[7] / x[7]:F3} |"));
		}
	}
}
