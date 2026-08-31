using System;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar whose generated method is left past the size the JIT optimizes is told so.
/// </summary>
/// <remarks>
/// The limit is real and measured — past about two thousand basic blocks RyuJIT compiles a
/// method the way it compiles one on its first call, and leaves it that way — and the
/// generator divides its methods to stay under it. Where nothing can divide, the author
/// gets <c>GRAM5003</c>: a warning, because the parser is correct and merely slower, with
/// the numbers it acted under, because the remedy is chosen against them.
/// </remarks>
public sealed class OversizeTests
{
	[Fact]
	public void A_parser_left_past_the_limit_warns_and_still_generates()
	{
		var compiled = GramCompiler.Compile(Huge(literals: 1200), new GramCompilerOptions
		{
			ClassName = "Big",
		});

		Assert.DoesNotContain(compiled.Diagnostics, d => d.Severity == GramSeverity.Error);
		Assert.Single(compiled.Sources);

		var oversized = compiled.Diagnostics.Where(d => d.Id == "GRAM5003").ToArray();

		Assert.NotEmpty(oversized);
		Assert.All(oversized, d => Assert.Equal(GramSeverity.Warning, d.Severity));

		// The message must let the author act: the estimate it measured, the line it
		// measured against, and a remedy.
		Assert.Contains("basic blocks", oversized[0].Message, StringComparison.Ordinal);
		Assert.Contains("2000", oversized[0].Message, StringComparison.Ordinal);
	}

	[Fact]
	public void A_parser_inside_the_limit_does_not_warn()
	{
		var compiled = GramCompiler.Compile(Huge(literals: 20), new GramCompilerOptions
		{
			ClassName = "Small",
		});

		Assert.DoesNotContain(compiled.Diagnostics, d => d.Id == "GRAM5003");
	}

	/// <summary>
	/// How large a part is aimed to be is the author's to change, and no value of it may
	/// fail a build.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The measurements behind the default were taken on two grammars and a synthetic one,
	/// and the basin they found is wide — sixty to two hundred and fifty estimated blocks
	/// all measure alike. Wide is not universal, so the number is settable; and because it
	/// is settable from an attribute in somebody's source, it is a wish rather than a
	/// setting. Below one asks for the finest division there is, past the size of the
	/// recognizer asks for one part, and every value in between is taken at its word.
	/// </para>
	/// <para>
	/// What is held here is that each of them generates a parser and that the parser
	/// parses. A number that divided a recognizer into nothing, or into one part per state
	/// and then failed to name a label, would be a build somebody cannot fix except by
	/// finding this setting and unsetting it.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(null)]
	[InlineData(0)]
	[InlineData(-5)]
	[InlineData(1)]
	[InlineData(40)]
	[InlineData(int.MaxValue)]
	public void Any_part_size_generates_a_parser_that_parses(int? size)
	{
		var compiled = GramCompiler.Compile(Divisible(), new GramCompilerOptions
		{
			ClassName     = "Sized",
			PartSize      = size,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, d => d.Severity == GramSeverity.Error);
		Assert.Single(compiled.Sources);

		// Compiled rather than merely emitted: an unreferenced label or a jump with no
		// target is a build the author cannot fix, and only the C# compiler finds those.
		var assembly = EmittedCode.Compile(compiled.Sources[0].Text);

		// The core, read through the ladder and the parentheses, and something the grammar
		// does not hold.
		Assert.Equal(7, EmittedCode.Match(assembly, "Sized", "TryParseStart", "1+(2+4)").Value);
		Assert.False(EmittedCode.Match(assembly, "Sized", "TryParseStart", "1+").IsSuccess);
	}

	/// <summary>And a size actually changes how many methods come out.</summary>
	[Fact]
	public void A_smaller_part_is_more_methods()
	{
		Assert.True(Methods(400) < Methods(40), "a smaller part should be more methods");

		static int Methods(int size) =>
			GramCompiler.Compile(Divisible(), new GramCompilerOptions
				{
					ClassName     = "Counted",
					PartSize      = size,
					CSharpScanner = RoslynCSharpScanner.Instance,
				})
				.Sources[0].Text
				.Split(["_Part"], StringSplitOptions.None)
				.Length;
	}

	/// <summary>
	/// A recursive core that needs the arena, and enough alternatives beside it to divide.
	/// </summary>
	/// <remarks>
	/// Both halves are load-bearing. A grammar of plain alternatives — <see cref="Huge"/>,
	/// or a choice of four hundred literals — is settled enough to be lowered to one flat
	/// method, which has no parts at all and would measure nothing here whatever the size
	/// said. Recursion with values is what asks for the engine, and the ballast is what
	/// gives the cuts somewhere to fall.
	/// </remarks>
	static string Divisible()
	{
		var text = new StringBuilder();

		text.AppendLine("Start : @int = e: Expr & eof => @(e)");
		text.AppendLine("Expr : @int = l: Expr & '+' & r: Atom => @(l + r) | a: Atom => @(a)");
		text.AppendLine(
			"Atom : @int = d: ['0'..'9']+ => @(int.Parse(d)) " +
			"| '(' & e: Expr & ')' => @(e) | b: Ballast => @(b)");
		text.Append("Ballast : @int = ");

		for (var i = 0; i < 300; i++)
		{
			if (i > 0)
				text.AppendLine().Append("       | ");

			text.Append('"').Append('k').Append(i).Append('"')
				.Append(" & '#' & n").Append(i).Append(": ['0'..'9']+")
				.Append(" => @(int.Parse(n").Append(i).Append("))");
		}

		text.AppendLine();
		text.AppendLine("parse Start");

		return text.ToString();
	}

	/// <summary>
	/// One rule that is a single straight-line sequence of distinct literals: it lowers to
	/// one method, and no cut can divide a method whose whole body is one alternative.
	/// </summary>
	static string Huge(int literals)
	{
		var text = new StringBuilder("Start = ");

		for (var i = 0; i < literals; i++)
			text.Append(i == 0 ? "" : " & ").Append("\"a").Append(i).Append('"');

		text.Append("\nparse Start\n");

		return text.ToString();
	}
}
