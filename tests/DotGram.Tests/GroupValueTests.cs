using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class GroupValueTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Repeated_group_values_work_with_eager_and_lazy_inputs(bool direct)
	{
		var host = Compile("""
			Number : @int = 'a' => @(1) | 'b' => @(2)
			End = ';'
			LogEnd = '|'
			Rows : @int[] = (value: Number & end: (End | eof) => @(Record(value + end.Length)))*
			parse Rows as All stream bytes
			parse Rows as Items stream bytes yield
			parse Rows with (End = LogEnd) as Logs stream bytes yield : @int
			""", direct);

		foreach (var mode in new[] { "text", "chars", "bytes" })
		{
			Assert.Equal(new[] { 2, 3, 1 }, Read(host, "All", "a;b;a", mode));
			Assert.Equal(new[] { 2, 3, 1 }, Read(host, "Items", "a;b;a", mode));
			Assert.Equal(new[] { 2, 3, 1 }, Read(host, "Logs", "a|b|a", mode));
			Assert.Empty(Read(host, "Items", "", mode));
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Nested_actions_and_alternatives_own_their_values(bool direct)
	{
		var host = Compile("""
			Rows : @int[] =
				(value: ('a' => @(1) | 'b' => @(2)) & ';' => @(value + 10))*
			parse Rows as All stream bytes
			parse Rows as Items stream bytes yield
			""", direct);

		foreach (var mode in new[] { "text", "chars", "bytes" })
		{
			Assert.Equal(new[] { 11, 12 }, Read(host, "All", "a;b;", mode));
			Assert.Equal(new[] { 11, 12 }, Read(host, "Items", "a;b;", mode));
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Abandoned_alternatives_and_lookahead_do_not_run_actions(bool direct)
	{
		var host = Compile("""
			Rows : @int[] =
				((?=('a' => @(Record(9))) & ('a' => @(Record(1))) & 'x')
				| (('a' => @(Record(2))) & 'y'))*
			parse Rows as All stream bytes
			""", direct);

		foreach (var mode in new[] { "text", "chars", "bytes" })
		{
			host.GetField("Calls")!.SetValue(null, 0);
			Assert.Equal(new[] { 2 }, Read(host, "All", "ay", mode));
			Assert.Equal(1, host.GetField("Calls")!.GetValue(null));
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Recover_handles_first_bad_character_and_last_unterminated_field(bool direct)
	{
		var host = Compile("""
			Rows : @int[] = ('a' & (';' | eof) => @(Record(1)))*
				recover ';' => @(-parserText.Length)
			parse Rows as All stream bytes
			parse Rows as Items stream bytes yield
			""", direct);

		foreach (var mode in new[] { "text", "chars", "bytes" })
		{
			Assert.Equal(new[] { 1, -3, 1, -4 }, Read(host, "All", "a;bad;a;tail", mode));
			Assert.Equal(new[] { 1, -3, 1, -4 }, Read(host, "Items", "a;bad;a;tail", mode));
		}
	}

	[Theory]
	[InlineData("text")]
	[InlineData("chars")]
	[InlineData("bytes")]
	public void Lazy_actions_run_once_per_requested_element(string mode)
	{
		var host = Compile("""
			Rows : @int[] = ('a' & ';' => @(Record(parserSpan.Start)))*
			parse Rows as Items stream bytes yield
			""", false);
		using var reader = new StringReader(string.Concat(Enumerable.Repeat("a;", 100)));
		using var stream = new MemoryStream(Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("a;", 100))));
		var source = mode == "text" ? (object)"a;a;a;" : mode == "chars" ? reader : stream;
		var signature = mode == "text" ? new[] { typeof(string) } : new[] { mode == "chars" ? typeof(TextReader) : typeof(Stream), typeof(int), typeof(int) };
		var arguments = mode == "text" ? new[] { source } : new[] { source, 1, 16 };
		var values = (IEnumerable<int>)host.GetMethod("Items", signature)!.Invoke(null, arguments)!;

		Assert.Equal(0, host.GetField("Calls")!.GetValue(null));
		using var iterator = values.GetEnumerator();
		Assert.True(iterator.MoveNext());
		Assert.Equal(0, iterator.Current);
		Assert.Equal(1, host.GetField("Calls")!.GetValue(null));
		Assert.True(iterator.MoveNext());
		Assert.Equal(2, iterator.Current);
		Assert.Equal(2, host.GetField("Calls")!.GetValue(null));
	}

	[Fact]
	public void Legacy_reader_and_yield_share_group_recognizers()
	{
		var host = Compile("""
			End = ';'
			LogEnd = '|'
			Rows : @int[] = ('a' & End => @(1))*
			parse Rows as All
			parse Rows as Items yield
			parse Rows with (End = LogEnd) as Logs yield
			""", false, buffered: false);

		using var reader = new StringReader("a;a;");
		var values = (IEnumerable<int>)host.GetMethod("All", [typeof(TextReader)])!.Invoke(null, [reader])!;

		Assert.Equal(new[] { 1, 1 }, values);
		Assert.Equal(new[] { 1, 1 }, Read(host, "Items", "a;a;", "text"));
		Assert.Equal(new[] { 1, 1 }, Read(host, "Logs", "a|a|", "text"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Parameterized_groups_keep_specialized_types_and_separators(bool lexical)
	{
		var host = Compile("""
			trivia = { ' '* }
			namespace Tokens
			{
				trivia = none
				Number : @int = 'a' => @(1) | 'b' => @(2)
			}
			List(item, end) : item[] = (value: item & end => @(value))*
			Rows : @int[] = value: List(Tokens.Number, ';') => @(value)
			parse Rows as All
			""", false, buffered: false, lexical: lexical);

		Assert.Equal(new[] { 1, 2 }, Read(host, "All", "a;b;", "text"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Group_value_can_backtrack_before_it_is_materialized(bool direct)
	{
		var host = Compile("""
			Rows : @int[] = (("ab" => @(Record(1)) | "a" => @(Record(2))) & 'b')*
			parse Rows as All stream bytes
			""", direct);

		foreach (var mode in new[] { "text", "chars", "bytes" })
		{
			host.GetField("Calls")!.SetValue(null, 0);
			Assert.Equal(new[] { 2 }, Read(host, "All", "ab", mode));
			Assert.Equal(1, host.GetField("Calls")!.GetValue(null));
		}
	}

	static Type Compile(string grammar, bool direct, bool buffered = true, bool lexical = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Direct = direct,
			BufferedInput = buffered,
			BufferedBytes = buffered,
			Lexical = lexical,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text,
			declarationMembers: "public static int Calls; static int Record(int value) { Calls++; return value; }",
			sourceParts: result.Sources.Skip(1).Select(source => source.Text)).GetType("Grammar")!;
	}

	static int[] Read(Type host, string method, string input, string mode)
	{
		using var reader = new StringReader(input);
		using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
		var signature = mode == "text" ? new[] { typeof(string) } : new[] { mode == "chars" ? typeof(TextReader) : typeof(Stream), typeof(int), typeof(int) };
		object source = mode == "text" ? input : mode == "chars" ? reader : stream;
		var arguments = mode == "text" ? new[] { source } : new[] { source, 1, 32 };
		var values = (IEnumerable<int>)host.GetMethod(method, signature)!.Invoke(null, arguments)!;

		return values.ToArray();
	}
}
