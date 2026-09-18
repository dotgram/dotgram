using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class RecoveryBoundaryTests
{
	const string Helpers = """
		static string Text(string text) { return text; }
		static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Text(global::System.ReadOnlySpan<byte> text) { return global::System.Text.Encoding.ASCII.GetString(text.ToArray()); }
		static string Bad(string text) { return "!" + text; }
		static string Bad(global::System.ReadOnlySpan<char> text) { return "!" + Text(text); }
		static string Bad(global::System.ReadOnlySpan<byte> text) { return "!" + Text(text); }
		""";

	[Theory]
	[InlineData(false, "\n")]
	[InlineData(false, "\r\n")]
	[InlineData(true,  "\n")]
	[InlineData(true,  "\r\n")]
	public void Complete_continuation_distinguishes_bad_first_tokens_from_the_trailer(bool buffered, string lineEnding)
	{
		var assembly = Compile("""
			Row : @string = 'R' & text: ['a'..'z']+ & eol => @(Text(text))
			Trailer = 'T' & "END" & eol
			Start : @string[] = Row* recover eol => @(Bad(parserText)) & Trailer & eof
			parse Start
			""", buffered);
		var input = "Rx\nTbad\nwrong\nRy\nTEND\n".Replace("\n", lineEnding);
		var expected = new[] { "x", "!Tbad", "!wrong", "y" };

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value!);
		Assert.Equal(expected, Read(assembly, input, buffered, false));
		if (buffered)
			Assert.Equal(expected, Read(assembly, input, true, true));

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "Rx\n").IsSuccess);
		Assert.Throws<FormatException>(() => Read(assembly, "Rx\n", buffered, false));
	}

	[Theory]
	[InlineData("")]
	[InlineData("x")]
	[InlineData(";")]
	[InlineData("x;Rok;y")]
	public void Empty_input_and_invalid_final_elements_terminate_without_an_atomic_marker(string input)
	{
		var assembly = Compile("""
			Row : @string = 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
			Start : @string[] = Row* recover ';' => @(Bad(parserText))
			parse Start
			""", true);
		var expected = input switch
		{
			""         => Array.Empty<string>(),
			"x"        => new[] { "!x" },
			";"        => new[] { "!" },
			_          => new[] { "!x", "ok", "!y" },
		};

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value!);
		Assert.Equal(expected, Read(assembly, input, true, false));
		Assert.Equal(expected, Read(assembly, input, true, true));
	}

	[Fact]
	public void Nested_recovery_keeps_each_groups_continuation_and_does_not_invent_missing_groups()
	{
		var assembly = Compile("""
			Row : @string = 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
			Group : @string[] = '[' & Row* recover ';' => @(Bad(parserText)) & ']'
			Start : @string[] = value: Group & '!' & eof => @(value)
			parse Start
			""", true);
		const string input = "[x;Ra;y;Rb;]!";
		var expected = new[] { "!x", "a", "!y", "b" };

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value!);
		Assert.Equal(expected, Read(assembly, input, true, false));
		Assert.Equal(expected, Read(assembly, input, true, true));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "[Ra;").IsSuccess);
	}

	[Fact]
	public void Nested_recovering_repetitions_keep_their_own_boundaries()
	{
		var assembly = Compile("""
			Row : @string = 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
			Group : @string = '[' & rows: Row* recover ';' => @(Bad(parserText)) & ']' & eol => @(string.Join(",", rows))
			Start : @string[] = Group* recover eol => @(Bad(parserText))
			parse Start
			""", true);
		const string input = "[x;Ra;]\nbad\n[Rb;y;]\n";
		var expected = new[] { "!x,a", "!bad", "b,!y" };

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value!);
		Assert.Equal(expected, Read(assembly, input, true, false));
		Assert.Equal(expected, Read(assembly, input, true, true));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Atomic_groups_still_close_internal_alternatives(bool atomic)
	{
		var group = atomic ? "{ \"ab\" | \"a\" }" : "(\"ab\" | \"a\")";
		var assembly = Compile("Row : @string = " + group + " & 'b' & ';' => @(\"ok\")\n" +
			"Start : @string[] = Row* recover ';' => @(Bad(parserText))\nparse Start", true);
		var expected = new[] { atomic ? "!ab" : "ok" };

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ab;").Value!);
		Assert.Equal(expected, Read(assembly, "ab;", true, false));
		Assert.Equal(expected, Read(assembly, "ab;", true, true));
	}

	[Fact]
	public void Required_element_at_eof_is_still_missing()
	{
		var assembly = Compile("""
			Row : @string = 'R' & ';' => @("R")
			Start : @string[] = Row+ recover ';' => @(Bad(parserText))
			parse Start
			""", true);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "").IsSuccess);
		Assert.Equal(new[] { "!bad" }, Read(assembly, "bad", true, false));
	}

	[Theory]
	[InlineData("*", "!")]
	[InlineData("*", "?")]
	[InlineData("+", "!")]
	[InlineData("+", "?")]
	public void Committed_turns_preserve_run_backtracking_recovery_and_outer_alternatives(string repetition, string ending)
	{
		var assembly = Compile("""
			Row : @string = text: ['a']+ & 'a' & ';' => @(Text(text))
			""" + "\nRows : @string[] = Row" + repetition + " recover ';' => @(Bad(parserText))\n" + """
			Start : @string[] = rows: Rows & '!' => @(rows)
				| rows: Rows & '?' => @(rows)
			parse Start
			""", true);
		var input = string.Concat(Enumerable.Repeat("aaa;", 12)) + "bad;aaaa;" + ending;
		var expected = Enumerable.Repeat("aa", 12).Concat(new[] { "!bad", "aaa" }).ToArray();

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value!);
		Assert.Equal(expected, Read(assembly, input, true, false));
		Assert.Equal(expected, Read(assembly, input, true, true));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input[..^1]).IsSuccess);
	}

	[Theory]
	[InlineData(false, "!")]
	[InlineData(false, "?")]
	[InlineData(true, "!")]
	[InlineData(true, "?")]
	public void Switch_materializes_direct_and_owned_recoveries_after_prior_fields(bool throughOwner, string ending)
	{
		var capture = throughOwner ? "rows: Group" : "'[' & rows: Row* recover ';' => @(Bad(parserText)) & ']'";
		var assembly = Compile("""
			Row : @string = 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
			""" + (throughOwner ? "\nGroup : @string[] = '[' & Row* recover ';' => @(Bad(parserText)) & ']'" : "") +
			"\nEntry : @string = " + capture + " & " + """
			switch @(rows.Length) {
				case 2: ':' & when @(rows[1].StartsWith("!")) => @(string.Join(",", rows))
				default: '.' => @(string.Join(",", rows))
			}
			Start : @string[] = entries: Entry* & '!' => @(entries)
				| entries: Entry* & '?' => @(entries)
			parse Start
			""", true);
		var input = string.Concat(Enumerable.Repeat("[Ra;bad;]:", 12)) + "[bad;]." + ending;
		var expected = Enumerable.Repeat("a,!bad", 12).Append("!bad").ToArray();

		Assert.Equal(expected, (string[])EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value!);
		Assert.Equal(expected, Read(assembly, input, true, false));
		Assert.Equal(expected, Read(assembly, input, true, true));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "[Ra;Rb;]:!").IsSuccess);
	}

	static Assembly Compile(string grammar, bool buffered)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			BufferedInput = buffered,
			BufferedBytes = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Helpers);
	}

	static string[] Read(Assembly assembly, string input, bool buffered, bool bytes)
	{
		using var reader = new ShortReader(input);
		using var stream = new ShortStream(Encoding.ASCII.GetBytes(input));
		var domain = bytes ? typeof(Stream) : typeof(TextReader);
		var method = assembly.GetType("Grammar")!.GetMethod("ParseStart", buffered ? [domain, typeof(int), typeof(int)] : [domain])!;
		object source = bytes ? stream : reader;

		try
		{
			var result = (IEnumerable)method.Invoke(null, buffered ? [source, 1, 128] : [source])!;

			return result.Cast<string>().ToArray();
		}
		catch (TargetInvocationException error) when (error.InnerException is FormatException inner)
		{
			throw inner;
		}
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
