using System;
using System.Globalization;
using System.IO;

using DotGram.Web;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// How deep an input may nest. The printed parsers call by plain recursion, so the answer is the
/// stack each language gives them — which this measures, a level being one <c>[</c> of JSON and
/// four calls — and which the shipped C# parser does not depend on: it reads a hundred thousand
/// levels by moving to a new stack when one runs low.
/// </summary>
/// <remarks>
/// Both languages answer a stack that ran out without dying: TypeScript throws a
/// <c>RangeError</c> that the driver catches, and the C driver stands on a second stack for the
/// signal. Neither reads the input, though, and what they need to read it is a setting of the
/// process and not of the parser: that is what explicit frames are for.
/// </remarks>
public sealed class DepthTests(ITestOutputHelper output)
{
	static readonly int[] Ladder = [250, 500, 1_000, 2_000, 5_000, 10_000, 20_000, 50_000, 100_000];

	static IrProgram Json => Reading.Program(Graphs.WebGrammar("Rfc8259.cs"));

	static string Nested(int depth)
	{
		return new string('[', depth) + new string(']', depth);
	}

	/// <summary>The deepest step of the ladder TypeScript reads, each depth in a process of its own.</summary>
	/// <remarks>
	/// A process a depth, because a function V8 has optimized takes a smaller frame than one it
	/// is still interpreting: a parser that has run for a while nests deeper than one that has
	/// just started, and the first answer is the one a caller cannot plan for.
	/// </remarks>
	static int DeepestInTypeScript(int? stackKilobytes, int? systemStackKilobytes)
	{
		var deepest = 0;

		foreach (var depth in Ladder)
		{
			var answer = Slices.TypeScript("Depth", Json, "ParseJson", [Nested(depth)], host: "json.ts",
				stackKilobytes: stackKilobytes, systemStackKilobytes: systemStackKilobytes).Single();

			if (answer.Head == "T stack")
				break;

			Assert.Equal("A " + (depth * 2).ToString(CultureInfo.InvariantCulture) + " " + Nested(depth), answer.Head);

			deepest = depth;
		}

		return deepest;
	}

	static int DeepestInC(int? stackMegabytes)
	{
		var answers = Slices.InC("Depth", Json, "ParseJson", [.. Ladder.Select(Nested)], host: "json.c", stackMegabytes: stackMegabytes);
		var deepest = 0;

		for (var i = 0; i < answers.Count && answers[i].Head != "T stack"; i++)
		{
			Assert.Equal("A " + (Ladder[i] * 2).ToString(CultureInfo.InvariantCulture) + " " + Nested(Ladder[i]), answers[i].Head);

			deepest = Ladder[i];
		}

		return deepest;
	}

	void Report(string line)
	{
		output.WriteLine(line);

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.AppendAllLines(Path.Combine(report, "depth.txt"), [line]);
	}

	[Fact]
	public void TheShippedParserReadsAHundredThousandLevels()
	{
		Assert.Equal(Nested(100_000), JsonValue.Parse(Nested(100_000)).ToString());
	}

	[Fact]
	public void TypeScriptNestsAsDeepAsItsStack()
	{
		var byDefault = DeepestInTypeScript(null, null);
		var widened   = DeepestInTypeScript(7_000, null);

		Report($"TypeScript, node's default stack: reads {byDefault} levels, not the next step of {string.Join(", ", Ladder)}");
		Report($"TypeScript, --stack-size=7000 inside the system's default 8 MB: reads {widened} levels");

		Assert.InRange(byDefault, 250, 50_000);
		Assert.InRange(widened, 2_000, 100_000);

		if (OperatingSystem.IsWindows())
			return;

		// A gigabyte from the system, and V8 told it may use most of it.
		var raised = DeepestInTypeScript(900_000, 1_048_576);

		Report($"TypeScript, ulimit -s 1048576 and --stack-size=900000: reads {raised} levels");

		Assert.Equal(100_000, raised);
	}

	[Fact]
	public void CNestsAsDeepAsItsStack()
	{
		var byDefault = DeepestInC(null);
		var threaded  = DeepestInC(256);

		Report($"C (gcc -O2), the process's default 8 MB stack: reads {byDefault} levels, not the next step of {string.Join(", ", Ladder)}");
		Report($"C (gcc -O2), a thread with a 256 MB stack: reads {threaded} levels");

		Assert.InRange(byDefault, 2_000, 50_000);
		Assert.Equal(100_000, threaded);
	}

	/// <summary>
	/// The suite's two deepest refusals, a hundred thousand <c>[</c> and fifty thousand
	/// <c>[{"":</c> with nothing closing them: refused by the shipped parser, and by the printed
	/// ones once they have the stack to reach the end, where the refusal is.
	/// </summary>
	[Fact]
	public void TheDeepestRefusalsOfTheSuiteAreRefusedAtTheirEnd()
	{
		var directory = Path.Combine(Graphs.Repository, "tests", "DotGram.Tests", "Web", "JsonTestSuite");

		var texts = new[] { "n_structure_100000_opening_arrays.json", "n_structure_open_array_object.json" }
			.Select(file => File.ReadAllText(Path.Combine(directory, file))).ToList();

		foreach (var text in texts)
			Assert.False(JsonValue.TryParse(text, out _));

		var inC = Slices.InC("DeepRefusals", Json, "ParseJson", texts, host: "json.c", stackMegabytes: 256);

		Assert.Equal(2, inC.Count);

		for (var i = 0; i < texts.Count; i++)
			Assert.StartsWith("R " + texts[i].Length.ToString(CultureInfo.InvariantCulture) + " ", inC[i].Head, StringComparison.Ordinal);

		if (OperatingSystem.IsWindows())
			return;

		var inTypeScript = Slices.TypeScript("DeepRefusals", Json, "ParseJson", texts, host: "json.ts",
			stackKilobytes: 900_000, systemStackKilobytes: 1_048_576);

		Assert.Equal(inC.Select(static answer => answer.Head), inTypeScript.Select(static answer => answer.Head));
	}
}
