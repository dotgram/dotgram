using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// How fast the printed TypeScript parser reads JSON: beside <c>JSON.parse</c>, and beside a
/// parser another generator made, where <c>DOTGRAM_IR_OTHER_JSON_PARSER</c> names one (a module
/// that exports <c>parse(text)</c>).
/// </summary>
/// <remarks>
/// A direction, not a measurement: one process, a shared machine, the median of fifteen
/// readings. What it is for is one decision — whether reading through this tree is of the order
/// of what other generators' parsers do in the same language, or far off it.
/// </remarks>
public sealed partial class SpeedTests(ITestOutputHelper output)
{
	[GeneratedRegex(@"printed parser / the other generator's parser = ([0-9.]+)")]
	private static partial Regex Ratio();

	[Fact]
	public void TheTypeScriptParserIsTimedOnJson()
	{
		var other = Environment.GetEnvironmentVariable("DOTGRAM_IR_OTHER_JSON_PARSER");
		var lines = Slices.TypeScriptScript("Speed", Reading.Program(Graphs.WebGrammar("Rfc8259.cs")), "bench.ts", other is null ? [] : [other]);

		foreach (var line in lines)
			output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllLines(Path.Combine(report, "speed.txt"), lines);

		Assert.Contains(lines, static line => line.Contains("printed parser, the tree of json.ts", StringComparison.Ordinal));

		// More than twice the other parser's time is where this way of reading stops being worth taking further.
		foreach (var line in lines)
			if (Ratio().Match(line) is { Success: true } match)
				Assert.InRange(double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture), 0, 2);
	}
}
