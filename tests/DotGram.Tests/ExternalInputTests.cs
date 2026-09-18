using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

public sealed class ExternalInputTests
{
	[Theory]
	[InlineData(false, false)]
	[InlineData(true,  false)]
	[InlineData(false, true)]
	[InlineData(true,  true)]
	public void Recognizers_read_exact_extents_across_refills(bool direct, bool typed)
	{
		var assembly = Build(direct, typed);
		var type     = assembly.GetType("Probe")!;

		foreach (var input in new[] { "ab|cd!", "ab|cd", "!", "" })
		foreach (var size in new[] { 0, 5, 6 })
		foreach (var kind in new[] { typeof(string), typeof(TextReader), typeof(Stream) })
		{
			using var reader = new ShortReader(input);
			using var stream = new ShortStream(Encoding.ASCII.GetBytes(input));
			var context = Activator.CreateInstance(type.GetNestedType("Context")!)!;
			context.GetType().GetField("Size")!.SetValue(context, size);
			var source = kind == typeof(string) ? (object)input : kind == typeof(Stream) ? stream : reader;
			var method = type.GetMethods().Single(m => m.Name == "TryParseStart" && m.GetParameters()[0].ParameterType == kind && m.GetParameters().Length == (kind == typeof(string) ? 2 : 4));
			var args   = kind == typeof(string) ? new[] { source, context } : new[] { source, context, (object)1, 64 };
			var match  = method.Invoke(null, args)!;
			var valid  = input.Length == size + 1 && input[size] == '!';

			Assert.Equal(valid, (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
			if (valid)
				Assert.Equal(input.Substring(0, size), match.GetType().GetProperty("Value")!.GetValue(match));

			Assert.InRange((int)context.GetType().GetField("Calls")!.GetValue(context)!, 1, typed ? 2 : 1);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Failed_alternative_keeps_input_available_for_backtracking(bool direct)
	{
		var assembly = GeneratorDriverTests.Build($$"""
			using System;
			[DotGram.Gram("Start = (@Read & 'x') | (@Read & '!')\nparse Start stream bytes", BufferedInput = true, SpanCaptures = true, Direct = {{direct.ToString().ToLowerInvariant()}})]
			public partial class Probe
			{
				static bool Read(ParserInput<char> input, ref int p) { return input.TryAdvance(ref p, 5); }
				static bool Read(ParserInput<byte> input, ref int p) { return input.TryAdvance(ref p, 5); }
			}
			""");

		foreach (var kind in new[] { typeof(TextReader), typeof(Stream) })
		{
			using var reader = new ShortReader("ab|cd!");
			using var stream = new ShortStream(Encoding.ASCII.GetBytes("ab|cd!"));
			var source = kind == typeof(Stream) ? (object)stream : reader;
			var method = assembly.GetType("Probe")!.GetMethod("TryParseStart", [kind, typeof(int?), typeof(int?)])!;
			var match  = method.Invoke(null, [source, 1, 64])!;

			Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Input_view_cannot_bypass_buffer_retention_limits(bool bytes)
	{
		var assembly = Build(false, false);
		var type     = assembly.GetType("Probe")!;
		var context  = Activator.CreateInstance(type.GetNestedType("Context")!)!;

		context.GetType().GetField("Size")!.SetValue(context, 5);

		using var reader = new ShortReader("abcde!");
		using var stream = new ShortStream(Encoding.ASCII.GetBytes("abcde!"));
		var method = type.GetMethod("TryParseStart", [bytes ? typeof(Stream) : typeof(TextReader), context.GetType(), typeof(int?), typeof(int?)])!;
		var error  = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [bytes ? (object)stream : reader, context, 1, 4]));

		Assert.IsType<IOException>(error.InnerException);
	}

	[Fact]
	public void Context_argument_requires_a_context_declaration()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			[DotGram.Gram("Start = @Read\nparse Start stream")]
			public partial class Probe
			{
				static bool Read(ParserInput<char> input, ref int p, object context) { return true; }
			}
			""");

		Assert.Contains(run.Diagnostics, d => d.Id == "GRAM4025" && d.GetMessage().Contains("context"));
		Assert.DoesNotContain(run.Diagnostics, d => d.Id == "GRAM0001");
	}

	[Fact]
	public void Yield_and_named_variants_keep_absolute_positions_and_release_old_input()
	{
		var assembly = GeneratorDriverTests.Build(""""
			using System;
			[DotGram.Gram("""
				Separator = ';'
				CopySeparator = ';'
				Item : @int = @Read & Separator => @(parserSpan.Start)
				Items : @int[] = Item*
				parse Items as ReadItems stream bytes yield : @int
				parse Items with (Separator = CopySeparator) as ReadCopy stream bytes yield : @int
				""", BufferedInput = true)]
			[DotGram.GramOptions(Suffix = "Other", Direct = false)]
			public partial class Probe
			{
				static bool Read(ParserInput<char> input, ref int p)
				{
					return input.Peek(p, out var c) && c == 'a' && input.TryAdvance(ref p, 2);
				}
				static bool Read(ParserInput<byte> input, ref int p)
				{
					return input.Peek(p, out var c) && c == (byte)'a' && input.TryAdvance(ref p, 2);
				}
			}
			"""");

		foreach (var name in new[] { "Probe", "Probe+Other" })
		foreach (var publication in new[] { "ReadItems", "ReadCopy" })
		foreach (var kind in new[] { typeof(TextReader), typeof(Stream) })
		{
			var input = string.Concat(Enumerable.Repeat("ab;", 100));
			using var reader = new ShortReader(input);
			using var stream = new ShortStream(Encoding.ASCII.GetBytes(input));
			var source = kind == typeof(Stream) ? (object)stream : reader;
			var method = assembly.GetType(name)!.GetMethod(publication, [kind, typeof(int?), typeof(int?)])!;
			var values = (System.Collections.Generic.IEnumerable<int>)method.Invoke(null, [source, 1, 8])!;

			Assert.Equal(Enumerable.Range(0, 100).Select(i => i * 3), values);
		}
	}

	[Fact]
	public void Input_view_uses_character_fallback_for_lexical_grammars()
	{
		var assembly = GeneratorDriverTests.Build(""""
			[DotGram.Gram("""
				namespace Lexical
				{
					trivia = none
					Data = @Read
				}
				trivia = [' ']*
				Start = Lexical.Data & '!'
				parse Start
				""", Lexical = true)]
			public partial class Probe
			{
				static bool Read(ParserInput<char> input, ref int p) { return input.TryAdvance(ref p, 2); }
			}
			"""", permittedWarning: "GRAM5004");

		Assert.True(EmittedCode.Match(assembly, "Probe", "TryParseStart", "ab!").IsSuccess);
	}

	static Assembly Build(bool direct, bool typed)
	{
		var body = typed ? "v: @Read & '!' => @(v)" : "v: { @Read } & '!' => @(Text(v))";
		var output = typed ? ", out string value" : "";
		var chars = typed ? "value = input.Ensure(p, context.Size) ? input.Slice(p, context.Size).ToString() : string.Empty;" : "";
		var bytes = typed ? "value = input.Ensure(p, context.Size) ? System.Text.Encoding.ASCII.GetString(input.Slice(p, context.Size)) : string.Empty;" : "";

		return GeneratorDriverTests.Build($$"""
			using System;
			[DotGram.Gram("context : @Context\nStart : @string = {{body}}\nparse Start stream bytes", BufferedInput = true, SpanCaptures = true, Direct = {{direct.ToString().ToLowerInvariant()}})]
			public partial class Probe
			{
				public sealed class Context { public int Size; public int Calls; }
				static string Text(ReadOnlySpan<char> value) { return value.ToString(); }
				static string Text(ReadOnlySpan<byte> value) { return System.Text.Encoding.ASCII.GetString(value); }
				static bool Read(ParserInput<char> input, ref int p{{output}}, Context context)
				{
					context.Calls++;
					{{chars}}
					var start = p;
					var result = input.TryAdvance(ref p, context.Size);
					if (!result && p != start) throw new Exception("Position changed at EOF.");
					return result;
				}
				static bool Read(ParserInput<byte> input, ref int p{{output}}, Context context)
				{
					context.Calls++;
					{{bytes}}
					var start = p;
					var result = input.TryAdvance(ref p, context.Size);
					if (!result && p != start) throw new Exception("Position changed at EOF.");
					return result;
				}
			}
			""");
	}

	sealed class ShortReader(string text) : StringReader(text)
	{
		public override int Read(char[] buffer, int index, int count)
		{
			return base.Read(buffer, index, Math.Min(count, 1));
		}
	}

	sealed class ShortStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, Math.Min(count, 1));
		}
	}
}
