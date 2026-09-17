using System;
using System.IO;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class DelimiterScanTests
{
	[Theory]
	[InlineData("'|'")]
	[InlineData("any")]
	[InlineData("['|' | ';']")]
	[InlineData("' '* & '|' & ' '*")]
	[InlineData("[' ' | '\\t']* & ['|' | ';']")]
	public void Scan_matches_the_general_machine_including_backtracking(string separator)
	{
		var grammar = "Separator = " + separator + "\n" +
			"Text = (?!Separator & any)*\n" +
			"Start : @int = value: Text & ('X' | Separator | eof) => @(value.Length)\n" +
			"parse Start stream bytes";
		var fast = Compile(grammar);
		var slow = Compile(grammar.Replace("& any)", "& when @(true) & any)"));
		var inputs = new[] { "", "|", "  |  ", "abc|", "abc ;", "abc", "abcX", "abc  ",
			"a  b | ", "a\t ;", "a|b", "a" + new string(' ', 4096) + "b | " };

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(slow, "Grammar", "TryParseStart", input);
			var actual   = EmittedCode.Match(fast, "Grammar", "TryParseStart", input);
			Assert.Equal(expected.IsSuccess, actual.IsSuccess);
			Assert.Equal(expected.Value, actual.Value);
			Assert.Equal(expected.Position, actual.Position);
			Assert.Equal(expected.Error, actual.Error);

			foreach (var bytes in new[] { false, true })
				Assert.Equal(Read(slow, input, bytes), Read(fast, input, bytes));
		}
	}

	[Theory]
	[InlineData("+")]
	[InlineData("{2,}")]
	[InlineData("{2,4}")]
	public void Minimum_and_maximum_lengths_keep_failure_and_rollback_behavior(string count)
	{
		var grammar = "Separator = ' '* & '|'\nText = (?!Separator & any)" + count +
			"\nStart : @int = value: Text & (Separator | eof) => @(value.Length)\nparse Start stream bytes";
		var fast = Compile(grammar);
		var slow = Compile(grammar.Replace("& any)", "& when @(true) & any)"));
		foreach (var input in new[] { "", "|", " |", "a|", "ab|", "abcdef|", "ab   |", "ab   " })
		{
			Assert.Equal(EmittedCode.Match(slow, "Grammar", "TryParseStart", input),
				EmittedCode.Match(fast, "Grammar", "TryParseStart", input));
			Assert.Equal(Read(slow, input, true), Read(fast, input, true));
		}
	}

	[Fact]
	public void Publication_specialization_selects_the_padded_scan()
	{
		const string grammar = """
			Separator = ';'
			Padded = ' '* & '|'
			Text = (?!Separator & any)+
			Start : @int = value: Text & Separator => @(value.Length)
			parse Start with (Separator = Padded) stream bytes
			""";
		var assembly = Compile(grammar, padded: true);
		Assert.Equal(3, EmittedCode.Match(assembly, "Grammar", "TryParseStart", "abc   |").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "   |").IsSuccess);
	}

	[Theory]
	[InlineData("' '* & when @(true) & '|'")]
	[InlineData("' '* & ' '")]
	[InlineData("' '* & '|' & 'x'")]
	public void Unsupported_delimiters_keep_the_general_machine(string separator)
	{
		var result = GramCompiler.Compile("Separator = " + separator +
			"\nStart = (?!Separator & any)+\nparse Start", Options());
		EmittedCode.Quiet(result.Diagnostics);
		Assert.DoesNotContain("Linear delimiter scan", Assert.Single(result.Sources).Text);
	}

	static GramCompilerOptions Options()
	{
		return new GramCompilerOptions
		{
			Direct = false, BufferedInput = true, BufferedBytes = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		};
	}

	static Assembly Compile(string grammar, bool padded = false)
	{
		var result = GramCompiler.Compile(grammar, Options());
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		if (padded)
			Assert.Contains("Linear delimiter scan", source);

		return EmittedCode.Compile(source);
	}

	static (bool Success, string? Value, long Position) Read(Assembly assembly, string input, bool bytes)
	{
		using var reader = new StringReader(input);
		using var stream = new OneByteStream(Encoding.Latin1.GetBytes(input));
		var result = assembly.GetType("Grammar")!.GetMethod("TryParseStart",
			[bytes ? typeof(Stream) : typeof(TextReader), typeof(int), typeof(int)])!
			.Invoke(null, [bytes ? stream : reader, 1, int.MaxValue])!;
		var type = result.GetType();
		var value = type.GetProperty("Value")!.GetValue(result);

		return ((bool)type.GetProperty("IsSuccess")!.GetValue(result)!,
			value is byte[] data ? Encoding.Latin1.GetString(data) : value?.ToString(),
			(long)type.GetProperty("Position")!.GetValue(result)!);
	}

	sealed class OneByteStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, Math.Min(count, 1));
		}
	}
}
