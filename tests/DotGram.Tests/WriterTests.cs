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

	static int Insert(object writer, int at, string text) =>
		(int)WriterType.GetMethod("InsertLine")!.Invoke(writer, [at, text])!;

	static bool Contains(object writer, string text, int at) =>
		(bool)WriterType.GetMethod("Contains")!.Invoke(writer, [text, at])!;

	static object Create(int depth) => Activator.CreateInstance(WriterType, [depth])!;

	static void Call(object writer, string method, params object[] arguments) =>
		WriterType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!.Invoke(writer, arguments);
}
