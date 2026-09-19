using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where a recovered element is, said by a stream that has let go of everything before it (D5).
/// </summary>
/// <remarks>
/// A buffer counts line breaks as it releases input, and nothing asks it to read behind what it
/// holds: counting from the start of the input once read what had been let go, which is an
/// answer that cannot be given — or, where the buffer had not moved yet, one that cost the whole
/// input a question. Ten thousand lines through a window of a few dozen characters, the bad
/// element on the last of them and not at its start.
/// </remarks>
public sealed class RecoveryLocationTests
{
	[Fact]
	public void A_stream_places_a_bad_element_ten_thousand_lines_in()
	{
		var result = GramCompiler.Compile("""
			Row : @string = eol? & 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
			Rows : @string[] = Row* recover ';' => @(Bad(parserLine, parserColumn))
			parse Rows as Read stream yield : @string
			""", new GramCompilerOptions
		{
			ClassName = "Grammar", BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var host = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: """
			static string Text(string value) => value;
			static string Text(global::System.ReadOnlySpan<char> value) => value.ToString();
			static string Text(global::System.ReadOnlySpan<byte> value) => global::System.Text.Encoding.ASCII.GetString(value.ToArray());
			static string Bad(int line, int column) => "!" + line + ":" + column;
			""").GetType("Grammar")!;

		// No line break at the end: one there would begin an element the input never finishes.
		var input = string.Concat(Enumerable.Repeat("Ra;Rb;\n", 9999)) + "Ra;X;Rc;";

		// Characters only: a line and a column are not asked of bytes (GRAM4026).
		using var reader = new StringReader(input);

		var values = ((IEnumerable<string>)host.GetMethod("Read", [typeof(TextReader), typeof(int?), typeof(int?)])!
			.Invoke(null, [reader, 8, 64])!).ToArray();

		Assert.Equal(20000, values.Count(value => !value.StartsWith("!", StringComparison.Ordinal)));
		Assert.Equal(["!10000:4"], values.Where(value => value.StartsWith("!", StringComparison.Ordinal)));
	}
}
