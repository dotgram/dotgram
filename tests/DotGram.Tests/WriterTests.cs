using System;
using System.Reflection;

using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

public sealed class WriterTests
{
	static readonly Type WriterType = typeof(CSharpEmitter).Assembly.GetType("DotGram.Grammar.Emit.Writer", throwOnError: true)!;

	[Theory]
	[InlineData("", "\r\n")]
	[InlineData(" \t", "\r\n")]
	[InlineData("a", "\t\ta\r\n")]
	[InlineData("a \t", "\t\ta\r\n")]
	[InlineData("a\r\n", "\t\ta\r\n")]
	[InlineData("a\n", "\t\ta\r\n\r\n")]
	[InlineData("a\nb\r\nc", "\t\ta\r\n\t\tb\r\n\t\tc\r\n")]
	[InlineData("a\r", "\t\ta\r\r\n")]
	[InlineData("a\r\r\n", "\t\ta\r\r\n")]
	[InlineData("a\r\n \t\n", "\t\ta\r\n\r\n\r\n")]
	public void Raw_blocks_keep_line_endings_and_blank_lines(string input, string expected)
	{
		var writer = Create(2);
		Call(writer, "Write", input);
		Assert.Equal(expected, writer.ToString());
	}

	[Theory]
	[InlineData("\n")]
	[InlineData("\r\n")]
	public void Mapped_user_text_keeps_columns_and_trailing_whitespace(string ending)
	{
		var writer = Create(2);
		Call(writer, "Write", string.Join(ending,
			"before ", "#line 7 \"grammar.gram\"", "   value \t", " \t", "#line default", "after "));
		Assert.Equal("\t\tbefore\r\n#line 7 \"grammar.gram\"\r\n   value \t\r\n \t\r\n#line default\r\n\t\tafter\r\n", writer.ToString());
	}

	[Fact]
	public void Indented_writer_preserves_mapped_regions()
	{
		var child = Create(0);
		Call(child, "Write", "before\r\n#line 7\r\n  value \t\r\n#line default\r\nafter");
		var parent = Create(1);
		Call(parent, "AppendIndented", child, 1);
		Assert.Equal("\t\tbefore\r\n#line 7\r\n  value \t\r\n#line default\r\n\t\tafter\r\n", parent.ToString());
	}

	[Theory]
	[InlineData(" \t", "\r\n")]
	[InlineData("value \t", "\tvalue\r\n")]
	[InlineData("value\u00A0", "\tvalue\u00A0\r\n")]
	public void Lines_trim_only_spaces_and_tabs(string input, string expected)
	{
		var writer = Create(1);
		Call(writer, "Line", input);
		Assert.Equal(expected, writer.ToString());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(2)]
	public void Inserted_declarations_preserve_body_and_mapped_columns(int depth)
	{
		var writer = Create(depth);
		Call(writer, "Write", "#line 7\r\n  body \t\r\n#line default\r\nreturn p;");
		var original = writer.ToString();
		var at = Insert(writer, 0, "var p = pos; \t");
		at = Insert(writer, at, " \t");
		Insert(writer, at, "var c = '\\0';");
		var indent = new string('\t', depth);
		Assert.Equal(indent + "var p = pos;\r\n\r\n" + indent + "var c = '\\0';\r\n" + original, writer.ToString());
	}

	[Fact]
	public void Body_search_excludes_inserted_declarations_and_crosses_builder_chunks()
	{
		var writer = Create(0);
		Call(writer, "Line", new string('x', 20000));
		Call(writer, "Line", "accumulator_marker");
		var at = Insert(writer, 0, "header_only");
		Assert.True(Contains(writer, "accumulator_marker", at));
		Assert.True(Contains(writer, "xxx\r\naccumulator", at));
		Assert.False(Contains(writer, "header_only", at));
		Assert.True(Contains(writer, "header_only", 0));
		Assert.False(Contains(writer, "missing", at));
		Assert.True(Contains(writer, "", at));
	}

	[Theory]
	[InlineData(65536 - 9)]
	[InlineData(65536 - 4)]
	[InlineData(65536)]
	[InlineData(2 * 65536 - 7)]
	public void Search_finds_a_match_across_the_blocks_it_copies(int before)
	{
		// The search copies the buffer out in blocks of 64K: a match is found wherever it falls
		// against their edges, and a word is told from part of a longer one on either side of them.
		var writer = Create(0);
		Call(writer, "Exactly", new string('x', before) + " always ways_ ways");
		Assert.True(Contains(writer, " always ways_ ways", 0));
		Assert.True(Contains(writer, "ways", before + 13));
		Assert.False(Contains(writer, "ways", before + 15));
		Assert.False(Contains(writer, "xways", 0));
		Assert.True(Names(writer, "ways"));
		Assert.False(Names(writer, "alway"));

		var lone = Create(0);
		Call(lone, "Exactly", new string('x', before) + " always ways_");
		Assert.False(Names(lone, "ways"));
	}

	[Fact]
	public void Search_of_a_large_buffer_is_linear()
	{
		// The builder's indexer walks its chunks at every call, so a search by index costs the
		// length times the chunk count; over the T-SQL reader that made the generator ten times
		// slower. Sixteen million characters searched that way take minutes; copied out in
		// blocks, a few tens of milliseconds. The bound is far from both, so that load does not
		// fail it and the quadratic search cannot pass it.
		var writer = Create(0);
		var line   = new string('x', 1000);

		for (var i = 0; i < 16_000; i++)
			Call(writer, "Exactly", line);

		var watch = System.Diagnostics.Stopwatch.StartNew();

		Assert.False(Contains(writer, "ref segmentEnd", 0));
		Assert.False(Names(writer, "ways"));
		Assert.True(watch.Elapsed < TimeSpan.FromSeconds(5), $"Searched in {watch.Elapsed}.");
	}

	[Theory]
	[InlineData("Line")]
	[InlineData("Write")]
	[InlineData("Exactly")]
	public void Observation_retains_only_the_match_and_preserves_scope_depth(string method)
	{
		var writer = Activator.CreateInstance(WriterType, [2, "ways.Open("])!;
		var scope = (IDisposable)WriterType.GetMethod("Block")!.Invoke(writer, ["if (p > 0)"])!;
		Assert.Equal(3, WriterType.GetProperty("Depth")!.GetValue(writer));
		Call(writer, method, "ways.Other();");
		Assert.False((bool)WriterType.GetProperty("Observed")!.GetValue(writer)!);
		Call(writer, method, "w0 = ways.Open(1);");
		Call(writer, method, "return p;");
		scope.Dispose();
		Assert.True((bool)WriterType.GetProperty("Observed")!.GetValue(writer)!);
		Assert.Equal(2, WriterType.GetProperty("Depth")!.GetValue(writer));
		Assert.Equal(0, WriterType.GetProperty("Length")!.GetValue(writer));
		Assert.Equal("", writer.ToString());
	}

	static int Insert(object writer, int at, string text)
	{
		return (int)WriterType.GetMethod("InsertLine")!.Invoke(writer, [at, text])!;
	}

	static bool Contains(object writer, string text, int at)
	{
		return (bool)WriterType.GetMethod("Contains")!.Invoke(writer, [text, at])!;
	}

	static bool Names(object writer, string name)
	{
		return (bool)WriterType.GetMethod("Names")!.Invoke(writer, [name])!;
	}

	static object Create(int depth)
	{
		return Activator.CreateInstance(WriterType, [depth])!;
	}

	static void Call(object writer, string method, params object[] arguments)
	{
		WriterType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!.Invoke(writer, arguments);
	}
}
