using System;
using System.Linq;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <c>GRAM4016</c> is said by the pass that could not share the operand, and says how many
/// alternatives read it.
/// </summary>
/// <remarks>
/// <para>
/// It used to be said by a check of its own, which had to guess what the folding pass would
/// do: it asked <c>Doors</c> where the fold asks <c>Determinism</c>. Those are not the same
/// question, and the guessing showed — of sixteen sites it named across this repository,
/// nine were the trivia §4.5 weaves between operands, which no author can factor out, and
/// four more were operands the fold went on to share anyway. Three were real.
/// </para>
/// <para>
/// Said from inside the fold there is nothing to guess. The run was found, the proof was
/// asked for, the answer was no — and that is exactly the sentence the author needs, because
/// the remedy is to make the operand's reading provable rather than to rearrange anything.
/// </para>
/// <para>
/// It stays a warning. Sharing an operand that can give back is a different grammar, not a
/// faster spelling of the same one, which is why the compiler declines to do it silently.
/// </para>
/// </remarks>
public sealed class SharedPrefixTests
{
	[Fact]
	public void An_operand_that_cannot_be_shared_is_reported_with_its_count()
	{
		// `Chunk` can give back and a shorter reading of it lets the other tail fit, so the
		// two readings cannot be made one without changing which alternative wins — which
		// `SemanticTests.An_operand_that_can_give_back_is_not_shared` measures on the
		// parser this same grammar produces.
		var said = Warnings(
			"""
			Chunk = 'x'+
			Start : @string
				= a: Chunk & "xy" => @("first:" + a)
				| a: Chunk & "y"  => @("second:" + a)
			parse Start
			""");

		var one = Assert.Single(said);

		Assert.Contains("2 alternatives of 'Start'", one.Message, StringComparison.Ordinal);
		Assert.Equal(GramSeverity.Warning, one.Severity);
	}

	/// <summary>
	/// And it is said whether or not the operand leads back to the rule.
	/// </summary>
	/// <remarks>
	/// The scope this widened from. Reporting only where the cost compounds with nesting
	/// left `ExpressionLanguage`'s `Assignment` unmentioned — eleven alternatives each
	/// reading a non-recursive operand — where a profile then found reading a body of one
	/// identifier costing 18 microseconds, most of what the parse cost.
	/// </remarks>
	[Fact]
	public void A_flat_cost_is_reported_as_well_as_one_that_compounds()
	{
		var said = Warnings(
			"""
			Name     = ['a'..'z']+
			Segments = Name & ('/' & Name)*
			Start    = d: Segments & '/' & f: Name | d: Segments
			parse Start
			""");

		Assert.Single(said);
	}

	/// <summary>
	/// An operand shown to have one reading is shared rather than reported: the compiler
	/// makes the choice where making it changes nothing.
	/// </summary>
	[Fact]
	public void An_operand_with_one_reading_is_shared_and_not_reported()
	{
		var said = Warnings(
			"""
			Set : @string
				= "ab" & "x" => @("first")
				| "ab" & "y" => @("second")
			parse Set
			""");

		Assert.Empty(said);
	}

	static GramDiagnostic[] Warnings(string grammar) =>
		GramCompiler
			.Compile(grammar, new GramCompilerOptions
				{
					ClassName = "G",
					CSharpScanner = DotGram.Generation.RoslynCSharpScanner.Instance,
				})
			.Diagnostics
			.Where(one => one.Id == "GRAM4016")
			.ToArray();
}
