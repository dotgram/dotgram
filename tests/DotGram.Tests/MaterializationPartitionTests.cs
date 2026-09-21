using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class MaterializationPartitionTests
{
	static string Alternatives(int count)
	{
		return string.Join("\n | ", Enumerable.Range(0, count)
		.Select(i => $"\"{i}=\" & value: ['a'..'z']+ & ';' => @({i} * 100 + value.Length + parserSpan.Length)"));
	}

	static Assembly Compile(string grammar, bool spanCaptures)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Direct = false, BufferedInput = true, BufferedBytes = true, SpanCaptures = spanCaptures,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Contains("_Construct", source);
		return EmittedCode.Compile(source);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Large_factory_choices_preserve_values_and_spans_across_all_input_forms(bool spanCaptures)
	{
		var grammar = "Start : @int[] = Item+\nItem : @int = " + Alternatives(129) + "\nparse Start";
		var parser = Compile(grammar, spanCaptures);
		var indices = Enumerable.Range(0, 129).Reverse().ToArray();
		var input = string.Concat(indices.Select(i => $"{i}=abc;"));
		var expected = indices.Select(i => i * 100 + 3 + i.ToString().Length + 5).ToArray();
		Assert.Equal(expected, Assert.IsType<int[]>(EmittedCode.Match(parser, "Grammar", "TryParseStart", input).Value));
		foreach (var bytes in new[] { false, true })
		{
			using var reader = new StringReader(input);
			using var stream = new OneByteStream(Encoding.ASCII.GetBytes(input));
			var method = parser.GetType("Grammar")!.GetMethod("ParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;
			Assert.Equal(expected, (int[])method.Invoke(null, [bytes ? stream : reader, 1, 65536])!);
		}
		Assert.False(EmittedCode.Match(parser, "Grammar", "TryParseStart", input + "128=").IsSuccess);
	}

	[Fact]
	public void Partitioned_constructions_remain_available_to_typed_guards_and_backtracking()
	{
		var grammar = "Start : @int = (?!'!') & v: Item & when @(v < 0) => @(v) | v: Item & when @(v > 0) => @(v)\nItem : @int = " + Alternatives(129) + "\nparse Start";
		var parser = Compile(grammar, true);
		foreach (var index in new[] { 0, 63, 64, 127, 128 })
		{
			var result = EmittedCode.Match(parser, "Grammar", "TryParseStart", $"{index}=abc;");
			Assert.True(result.IsSuccess);
			Assert.Equal(index * 100 + 3 + index.ToString().Length + 5, result.Value);
		}
	}

	sealed class OneByteStream(byte[] input) : MemoryStream(input)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, Math.Min(1, count));
		}

		public override int Read(Span<byte> buffer)
		{
			return base.Read(buffer[..Math.Min(1, buffer.Length)]);
		}
	}
}
