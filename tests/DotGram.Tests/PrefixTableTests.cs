using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class PrefixTableTests
{
	const string Grammar = """
		Item : @int = "1=" & 'X'+ => @(1)
			| "100=" & 'X'+ => @(100)
			| "198=" & 'X'+ => @(198)
			| "900=" & 'X'+ => @(900)
		Start : @int = (v: Item | v: Unknown) & '!' => @(v)
		Unknown : @int = ['0'..'9']+ & '=' & 'Y' => @(-1)
		parse Start stream bytes
		""";

	static string Emit(string grammar, bool enabled, int partSize = 1000)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			PrefixTables = enabled, Direct = false, BufferedInput = true,
			PartSize = partSize, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		return Assert.Single(result.Sources).Text;
	}

	[Theory]
	[InlineData(1000, false)]
	[InlineData(3, false)]
	[InlineData(1000, true)]
	[InlineData(3, true)]
	public void Tables_preserve_values_fallback_and_errors_across_input_forms(int partSize, bool atomic)
	{
		var grammar = atomic ? Grammar.Replace("\"1=\" & 'X'+", "{ \"1=\" & 'X'+ }")
			.Replace("\"100=\" & 'X'+", "{ \"100=\" & 'X'+ }")
			.Replace("\"198=\" & 'X'+", "{ \"198=\" & 'X'+ }")
			.Replace("\"900=\" & 'X'+", "{ \"900=\" & 'X'+ }") : Grammar;
		var oldSource = Emit(grammar, false, partSize);
		var newSource = Emit(grammar, true, partSize);
		Assert.DoesNotContain("Prefix_DotGram", oldSource);
		Assert.Contains("Prefix_DotGram", newSource);
		var oldParser = EmittedCode.Compile(oldSource);
		var newParser = EmittedCode.Compile(newSource);
		foreach (var input in new[] { "1=X!", "100=X!", "198=X!", "900=X!", "198=Y!", "777=Y!", "", "1", "19", "198", "198=", "198=Z!", "198=X?", "900=X!extra", "~" })
		{
			Assert.Equal(EmittedCode.Match(oldParser, "Grammar", "TryParseStart", input),
				EmittedCode.Match(newParser, "Grammar", "TryParseStart", input));
			foreach (var bytes in new[] { false, true })
				Assert.Equal(Read(oldParser, input, bytes), Read(newParser, input, bytes));
		}
	}

	static (bool, object?, object?, object?) Read(Assembly parser, string input, bool bytes)
	{
		using var reader = new OneCharReader(input);
		using var stream = new OneByteStream(Encoding.ASCII.GetBytes(input));
		var method = parser.GetType("Grammar")!.GetMethod("TryParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int), typeof(int)])!;
		var match = method.Invoke(null, [bytes ? stream : reader, 1, 1024])!;
		object? Get(string name) => match.GetType().GetProperty(name)!.GetValue(match);
		return ((bool)Get("IsSuccess")!, Get("Value"), Get("Error"), Get("Position"));
	}

	sealed class OneCharReader(string input) : StringReader(input)
	{
		public override int Read(char[] buffer, int index, int count) => base.Read(buffer, index, Math.Min(1, count));
	}

	sealed class OneByteStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(1, count));
		public override int Read(Span<byte> buffer) => base.Read(buffer[..Math.Min(1, buffer.Length)]);
	}

	[Theory]
	[InlineData("http", "https")]
	[InlineData("same", "same")]
	public void Overlapping_prefixes_keep_the_old_strategy(string first, string second)
	{
		var grammar = $"Start : @int = \"{first}\" & ['x'..'y']+ => @(1) | \"{second}\" & ['x'..'y']+ => @(2) | \"foo\" & ['x'..'y']+ => @(3) | \"bar\" & ['x'..'y']+ => @(4)\nparse Start";
		Assert.Equal(Emit(grammar, false), Emit(grammar, true));
	}
}
