using System;
using System.IO;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The engine's step trace, written into a parser only where the compilation defines
/// <c>DOTGRAM_STEPS</c>.
/// </summary>
/// <remarks>
/// Alone, because the lines go to standard error, which is the process's and not a test's.
/// </remarks>
[Collection(typeof(GeneratorCostTests.Alone))]
public sealed class StepTraceTests
{
	const string Grammar =
		"""
		Start : @string = a: Name & ':' & b: Name & ("q" | "qq") => @(a + b)
		Name  : @string = t: 'a' => @(t) | t: 'b' => @(t)
		parse Start
		""";

	static string Emit(bool steps)
	{
		var result = GramCompiler.Compile(
			Grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Direct = false, Steps = steps });

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}

	[Fact]
	public void Without_the_option_neither_the_calls_nor_the_methods_are_written()
	{
		var source = Emit(steps: false);

		Assert.DoesNotContain("DOTGRAM_STEPS", source);
		Assert.DoesNotContain("Step_DotGram(", source, StringComparison.Ordinal);
	}

	[Fact]
	public void A_host_method_named_Trace_is_not_taken_for_a_step()
	{
		var result = GramCompiler.Compile(
			"Start : @int = 'a' => @(Trace(1))\nparse Start",
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Direct = false });

		Assert.Contains("Trace(1)", Assert.Single(result.Sources).Text, StringComparison.Ordinal);
	}

	[Fact]
	public void With_the_option_every_step_is_reported_and_the_answer_is_the_same()
	{
		var plain   = Emit(steps: false);
		var stepped = Emit(steps: true);

		Assert.Contains("Conditional(\"DOTGRAM_STEPS\")", stepped);
		Assert.Contains("Step_DotGram(\"call Name\"", stepped);

		var before = Console.Error;
		var lines  = new StringWriter();

		try
		{
			Console.SetError(lines);

			var answered = EmittedCode.Match(EmittedCode.Compile(plain), "Grammar", "TryParseStart", "a:bq");

			Assert.Equal("", lines.ToString());

			var traced = EmittedCode.Match(EmittedCode.Compile(stepped, symbols: ["DOTGRAM_STEPS"]), "Grammar", "TryParseStart", "a:bq");

			Assert.True(answered.IsSuccess);
			Assert.True(traced.IsSuccess);
			Assert.Equal(answered.Value, traced.Value);
			Assert.Contains(".Gram call Name", lines.ToString());
		}
		finally
		{
			Console.SetError(before);
		}
	}
}
