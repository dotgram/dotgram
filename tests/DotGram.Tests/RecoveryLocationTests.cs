using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where a recovered element is — <c>parserLine</c> and <c>parserColumn</c> — held to a count from
/// the start of the input at the position the factory is also handed, in every rendering that
/// hands them over.
/// </summary>
/// <remarks>
/// The walks count on from the last element asked about (<c>Located_DotGram</c>, and a buffer's
/// own count as it lets input go) rather than from the start each time, which is what made a
/// feed with a bad line in ten quadratic. What is held here is that counting on gives the answer
/// counting from the start gave: several bad elements to a line, a bad first element, line
/// breaks of both kinds.
/// </remarks>
public sealed class RecoveryLocationTests
{
	const string Grammar = """
		Row : @string = eol? & 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
		Rows : @string[] = Row* recover ';' => @(Bad(parserLine, parserColumn, parserPosition))
		parse Rows as All
		parse Rows as Read stream yield : @string
		""";

	const string Members = """
		static string Text(string value) => value;
		static string Text(global::System.ReadOnlySpan<char> value) => value.ToString();
		static string Text(global::System.ReadOnlySpan<byte> value) => global::System.Text.Encoding.ASCII.GetString(value.ToArray());
		static string Bad(int line, int column, long position) => "!" + line + ":" + column + "@" + position;
		""";

	public static TheoryData<string> Inputs => new()
	{
		"Ra;X;Rb;\nRc;Y;Z;\nRd;",
		"X;Ra;\nRb;\n\nW;Rc;",
		"Ra;X;\r\nRb;Y;\r\nZ;",
		"Q;\nQ;\nQ;\nRa;\nQ;",
	};

	[Theory]
	[MemberData(nameof(Inputs))]
	public void A_recovered_element_is_placed_as_counting_from_the_start_would(string input)
	{
		foreach (var direct in new[] { true, false })
		{
			var host = Compile(direct);

			Assert.All(Placed((string[])host.GetMethod("All", [typeof(string)])!.Invoke(null, [input])!), one => Holds(input, one));

			using var reader = new StringReader(input);
			var streamed = (IEnumerable<string>)host.GetMethod("Read", [typeof(TextReader), typeof(int?), typeof(int?)])!
				.Invoke(null, [reader, 1, 64])!;

			Assert.All(Placed([.. streamed]), one => Holds(input, one));
		}
	}

	/// <summary>The recovered elements' answers: line, column and position.</summary>
	static IEnumerable<(int Line, int Column, int Position)> Placed(string[] values)
	{
		foreach (var value in values.Where(value => value.StartsWith("!", StringComparison.Ordinal)))
		{
			var at    = value.IndexOf('@');
			var parts = value.Substring(1, at - 1).Split(':');

			yield return (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(value.Substring(at + 1)));
		}
	}

	static void Holds(string input, (int Line, int Column, int Position) placed)
	{
		var line   = 1;
		var column = 1;

		for (var at = 0; at < placed.Position; at++)
		{
			if (input[at] == '\n')
			{
				line++;
				column = 1;
			}
			else
				column++;
		}

		Assert.Equal((line, column), (placed.Line, placed.Column));
	}

	static Type Compile(bool direct)
	{
		var result = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members).GetType("Grammar")!;
	}
}
