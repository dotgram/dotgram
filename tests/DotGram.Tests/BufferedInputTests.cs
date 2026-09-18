using System;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class BufferedInputTests
{
	[Fact]
	public void Large_buffered_parse_and_yield_share_machines_and_preserve_recovery()
	{
		var grammar = string.Join("\n", Enumerable.Range(0, 129)
			.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			"\nR129 : @int = 'a' => @(1) | 'b' => @(2)\n" +
			"Row : @int = n: R0 & ';' => @(n)\n" +
			"Start : @int[] = Row* recover ';' => @(-1)\n" +
			"parse Start stream bytes\nparse Start as ReadRows stream bytes yield : @int";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		var methods = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>();
		Assert.Equal(2, methods.Count(method => method.Identifier.ValueText.StartsWith("Materialize_DotGram_Buffered_")));
		var assembly = EmittedCode.Compile(source, declarationMembers: """
			public static int[] Read(bool bytes, bool yield)
			{
				var input = "a;!;b;";
				using var chars = new System.IO.StringReader(input);
				using var data = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(input));
				if (yield)
					return System.Linq.Enumerable.ToArray(bytes
						? ReadRows(data, bufferSize: 1, maxRetained: 8)
						: ReadRows(chars, bufferSize: 1, maxRetained: 8));
				return bytes ? ParseStart(data, bufferSize: 1) : ParseStart(chars, bufferSize: 1);
			}
			""");
		foreach (var bytes in new[] { false, true })
			foreach (var yield in new[] { false, true })
				Assert.Equal(new[] { 130, -1, 131 }, (int[])assembly.GetType("Grammar")!
					.GetMethod("Read")!.Invoke(null, [bytes, yield])!);
	}
	/// <summary>A record longer than the retention limit says what the limit was and what to change.</summary>
	[Theory]
	[InlineData(false, "characters")]
	[InlineData(true, "bytes")]
	public void Exceeding_the_retention_limit_names_it_and_says_to_raise_it(bool bytes, string unit)
	{
		var result = GramCompiler.Compile("""
			Row : @int = t: ['a'..'z']+ & ';' => @(t.Length)
			Rows : @int[] = Row*
			parse Rows as All stream bytes
			""", new GramCompilerOptions { BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(result.Diagnostics);
		var host   = EmittedCode.Compile(Assert.Single(result.Sources).Text).GetType("Grammar")!;
		var domain = bytes ? typeof(Stream) : typeof(TextReader);
		var input  = bytes
			? (object)new MemoryStream(System.Text.Encoding.ASCII.GetBytes("abcdefghij;"))
			: new StringReader("abcdefghij;");

		var thrown = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
			host.GetMethod("All", [domain, typeof(int?), typeof(int?)])!.Invoke(null, [input, 1, 4]));
		var error = Assert.IsType<IOException>(thrown.InnerException);

		Assert.Contains("more than 4 retained " + unit, error.Message, StringComparison.Ordinal);
		Assert.Contains("maxRetained", error.Message, StringComparison.Ordinal);
	}

	/// <summary>A buffer parameter left null takes the grammar's default; a number is that call's own.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_null_buffer_parameter_takes_the_grammars_default(bool bytes)
	{
		var result = GramCompiler.Compile("""
			Row : @int = t: ['a'..'z']+ & ';' => @(t.Length)
			Rows : @int[] = Row*
			parse Rows as All stream bytes
			""", new GramCompilerOptions
		{
			BufferedInput = true, MaxRetained = 4, BufferSize = 2, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Contains("internal const int DefaultMaxRetained = 4;", source, StringComparison.Ordinal);
		Assert.Contains("internal const int DefaultBufferSize = 2;", source, StringComparison.Ordinal);

		var method = EmittedCode.Compile(source).GetType("Grammar")!
			.GetMethod("All", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;
		object Input() => bytes
			? new MemoryStream(System.Text.Encoding.ASCII.GetBytes("abcdefghij;"))
			: new StringReader("abcdefghij;");

		// Null: the grammar's limit of four, which a record of ten exceeds.
		var thrown = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [Input(), null, null]));
		Assert.Contains("more than 4 retained", Assert.IsType<IOException>(thrown.InnerException).Message, StringComparison.Ordinal);

		// A number: this call's own, and the record fits.
		Assert.Equal(new[] { 10 }, (int[])method.Invoke(null, [Input(), null, 64])!);
	}

	/// <summary>Where the grammar says nothing, there is no limit.</summary>
	[Fact]
	public void Without_the_option_a_buffered_parse_has_no_limit()
	{
		var result = GramCompiler.Compile("Row = 'a'\nparse Row as All stream bytes",
			new GramCompilerOptions { BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Contains("internal const int DefaultMaxRetained = 2147483647;", source, StringComparison.Ordinal);
		Assert.Contains("internal const int DefaultBufferSize = 4096;", source, StringComparison.Ordinal);
	}

	/// <summary>The attribute sets the defaults the generated class holds.</summary>
	[Fact]
	public void The_attribute_sets_the_defaults_the_generated_class_holds()
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"Row = 'a'\\nparse Row\", BufferedInput = true, MaxRetained = 16, BufferSize = 8)]\npublic partial class Limits;");
		var source = string.Join("\n", run.Results.SelectMany(one => one.GeneratedSources).Select(one => one.SourceText.ToString()));
		Assert.Contains("internal const int DefaultMaxRetained = 16;", source, StringComparison.Ordinal);
		Assert.Contains("internal const int DefaultBufferSize = 8;", source, StringComparison.Ordinal);
	}

	/// <summary>A limit or a capacity of nothing is refused where it is written.</summary>
	[Theory]
	[InlineData("MaxRetained = 0")]
	[InlineData("BufferSize = -1")]
	public void A_buffer_option_of_nothing_is_refused_where_it_is_written(string option)
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"Row = 'a'\\nparse Row\", BufferedInput = true, " + option + ")]\npublic partial class Limits;");
		var diagnostic = Assert.Single(run.Diagnostics, one => one.Id == "GRAM0009");
		Assert.Contains(option, diagnostic.GetMessage(), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Buffered_find_matches_contiguous_results_and_releases_completed_occurrences(bool bytes)
	{
		var compilation = GramCompiler.Compile("Item = ['0'..'9']+ & ';'\nfind Item", new GramCompilerOptions
		{
			BufferedInput = true, BufferedBytes = true,
		});
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(compilation.Sources).Text);
		var text = string.Concat(Enumerable.Repeat("x12345;", 100));
		using var reader = new ShortReader(text, 1);
		using var stream = new ShortStream(System.Text.Encoding.ASCII.GetBytes(text));
		var method = assembly.GetType("Grammar")!.GetMethod("FindItem", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;
		var result = (System.Collections.IEnumerable)method.Invoke(null, [bytes ? stream : reader, 2, 16])!;
		var matches = result.Cast<object>().ToArray();
		Assert.Equal(100, matches.Length);
		for (var i = 0; i < matches.Length; i++)
		{
			var match = matches[i];
			Assert.Equal(i * 7 + 1, Convert.ToInt32(match.GetType().GetProperty("Position")!.GetValue(match)));
			var value = match.GetType().GetProperty("Value")!.GetValue(match);
			Assert.Equal("12345;", bytes ? System.Text.Encoding.ASCII.GetString((byte[])value!) : (string)value!);
		}
	}

	[Theory]
	[InlineData("Item = 'a'*\nfind Item stream bytes", "ba", 3)]
	[InlineData("Item : @int = 'a'+ & 'b' => @(1)\nfind Item stream bytes", "aaaaxab", 1)]
	[InlineData("Item = 'a' & eof\nfind Item stream bytes", "aba", 1)]
	[InlineData("Item = ('a' & 'b' & 'c') | 'a'\nfind Item stream bytes", "ababc", 2)]
	public void Buffered_find_handles_empty_matches_eof_and_backtracking(string grammar, string input, int count)
	{
		var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions { BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(compilation.Sources).Text);
		foreach (var bytes in new[] { false, true })
		{
			using var reader = new ShortReader(input, 1);
			using var stream = new ShortStream(System.Text.Encoding.ASCII.GetBytes(input));
			var method = assembly.GetType("Grammar")!.GetMethod("FindItem", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;
			var result = (System.Collections.IEnumerable)method.Invoke(null, [bytes ? stream : reader, 1, 100])!;
			Assert.Equal(count, result.Cast<object>().Count());
		}
	}

	[Fact]
	public void Large_split_dispatch_keeps_char_and_byte_backtracking_correct()
	{
		const int count = 912;
		var rules = Enumerable.Range(0, count).Select(i => $"R{i} : @int = \"{i}=\" & ['0'..'9']+ & ';' => @({i})");
		var grammar = string.Join("\n", rules) + "\nItem : @int = (" +
			string.Join(" | ", Enumerable.Range(0, count).Select(i => $"v: R{i}")) +
			") => @(v)\nStart : @int[] = Item+\nparse Start stream bytes";
		var options = new GramCompilerOptions
		{
			BufferedInput = true, Direct = false, PartSize = 128, CSharpScanner = RoslynCSharpScanner.Instance,
		};
		var single = GramCompiler.Compile(grammar, options);
		EmittedCode.Quiet(single.Diagnostics);
		Assert.Single(single.Sources);
		options.SourceFileSize = 2_000_000;
		var compilation = GramCompiler.Compile(grammar, options);
		EmittedCode.Quiet(compilation.Diagnostics);
		Assert.True(compilation.Sources.Count > 1);
		var source = compilation.Sources[0].Text;
		Assert.Contains("_Dispatch = new int[]", source);
		Assert.All(compilation.Sources, part => Assert.DoesNotContain("switch (chosen)", part.Text));
		var assembly = EmittedCode.Compile(source, sourceParts: compilation.Sources.Skip(1).Select(part => part.Text));
		var expected = Enumerable.Range(0, count).Reverse().ToArray();
		var text = string.Concat(expected.Select(i => $"{i}=123;"));
		Assert.Equal(expected, Assert.IsType<int[]>(EmittedCode.Match(assembly, "Grammar", "TryParseStart", text).Value));
		Assert.Equal(expected, Assert.IsType<int[]>(Read(assembly, new ShortReader(text, 3), 2).Value));
		using var bytes = new ShortStream(text.Select(c => (byte)c).ToArray());
		var method = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)])!;
		var result = method.Invoke(null, [bytes, 2, text.Length + 1])!;
		Assert.True((bool)result.GetType().GetProperty("IsSuccess")!.GetValue(result)!);
		Assert.Equal(expected, Assert.IsType<int[]>(result.GetType().GetProperty("Value")!.GetValue(result)));
		Assert.False(Read(assembly, new ShortReader(text + "599=bad;", 3), 2).Success);
	}

	[Theory]
	[InlineData("Start = \"abcdef\" | \"abcxyz\"", "abcdef", "abcxyz", "abcxef")]
	[InlineData("Start = 'a'* & \"ab\"", "aaaab", "ab", "aaaa")]
	[InlineData("Start = ?=(\"abcdef\") & \"abc\" & \"def\" | \"abcxyz\"", "abcdef", "abcxyz", "abcde")]
	[InlineData("Start = ?!(\"abcdef\") & \"abc\" & any*", "abcxyz", "abcde", "abcdef")]
	[InlineData("Start = '(' & Start & ')' | 'x'", "(((x)))", "x", "((x)")]
	[InlineData("Start = \"ab\" & (\"cd\" | 'c') & 'd'", "abcd", "abcdd", "abc")]
	[InlineData("Start : @string = s: ['a'..'z']+ & '!' => @(s)", "abc!", "z!", "abc")]
	public void Refill_preserves_recognition_and_captures(string grammar, string first, string second, string invalid)
	{
		var compilation = GramCompiler.Compile(grammar + "\nparse Start stream", new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance, Carrier = CarrierKind.Tape,
		});
		EmittedCode.Quiet(compilation.Diagnostics);
		var source = Assert.Single(compilation.Sources).Text;
		var assembly = EmittedCode.Compile(source);
		foreach (var input in new[] { first, second, invalid, "" })
		{
			var expected = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
			for (var split = 1; split <= input.Length + 1; split++)
			{
				using var reader = new ShortReader(input, split);
				var actual = Read(assembly, reader, 2);
				Assert.Equal(expected.IsSuccess, actual.Success);
				Assert.Equal(expected.Position, actual.Position);
				if (expected.IsSuccess) Assert.Equal(expected.Value, actual.Value);
				Assert.False(reader.Disposed);
			}
		}
	}

	[Fact]
	public void Guards_are_not_reexecuted_for_each_refill()
	{
		var compilation = GramCompiler.Compile("""
			Start : @int = x: Item & when @(Check(x)) & "abcdefghijk" => @(x)
			Item : @int = 'x' => @(42)
			parse Start
			""", new GramCompilerOptions { BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(compilation.Sources.Single().Text, declarationMembers:
			"public static int Calls; static bool Check(int value) { Calls++; return value == 42; }");
		Assert.True(Read(assembly, new ShortReader("xabcdefghijk", 1), 2).Success);
		Assert.Equal(1, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Fact]
	public void Limit_is_distinct_from_eof_and_reader_need_not_implement_peek()
	{
		var compilation = GramCompiler.Compile("Start = any*\nparse Start stream");
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(compilation.Sources.Single().Text);
		Assert.True(Read(assembly, new ShortReader("abc", 1), 2, 3).Success);
		var error = Assert.Throws<TargetInvocationException>(() => Read(assembly, new ShortReader("abcd", 1), 2, 3));
		Assert.IsType<IOException>(error.InnerException);
	}

	[Fact]
	public void Default_has_no_buffered_infrastructure()
	{
		var compilation = GramCompiler.Compile("Start = \"abc\"\nparse Start");
		Assert.DoesNotContain("BufferedText", compilation.Sources.Single().Text);
	}

	[Theory]
	[InlineData("Start = any*", false)]
	[InlineData("Start = \"ABC\" & any* | \"XYZ\"", false)]
	[InlineData("Start = any*", true)]
	public void Bytes_are_preserved_without_decoding(string grammar, bool option)
	{
		var compilation = GramCompiler.Compile(grammar + "\nparse Start" + (option ? "" : " stream bytes"),
			new GramCompilerOptions { BufferedBytes = option });
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(compilation.Sources.Single().Text);
		var input = new byte[] { 65, 66, 67 }.Concat(Enumerable.Range(0, 256).Select(i => (byte)i)).ToArray();
		using var stream = new ShortStream(input);
		var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [stream, 2, int.MaxValue])!;
		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		Assert.Equal(input, (byte[])match.GetType().GetProperty("Value")!.GetValue(match)!);
	}

	sealed class ShortStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, 1));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Proven_dead_prefix_is_reused_without_growing_with_input(bool bytes)
	{
		var compilation = GramCompiler.Compile("""
			Start : @int = "ab"* & '!' => @(42)
			parse Start
			""", new GramCompilerOptions
		{
			BufferedInput = !bytes, BufferedBytes = bytes, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compilation.Diagnostics);
		var source = compilation.Sources.Single().Text;
		Assert.Contains("text.ReleaseBefore(p);", source);
		var assembly = EmittedCode.Compile(source);
		foreach (var suffix in new[] { "!", "x", "a!" })
		{
			var text = string.Concat(Enumerable.Repeat("ab", 10000)) + suffix;
			var expected = EmittedCode.Match(assembly, "Grammar", "TryParseStart", text);
			using var input = bytes ? (IDisposable)new ShortStream(System.Text.Encoding.ASCII.GetBytes(text)) : new ShortReader(text, 1);
			var inputType = bytes ? typeof(Stream) : typeof(TextReader);
			var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [inputType, typeof(int?), typeof(int?)])!.Invoke(null, [input, 7, 7])!;
			Assert.Equal(expected.IsSuccess, match.GetType().GetProperty("IsSuccess")!.GetValue(match));
			Assert.Equal(expected.Position, match.GetType().GetProperty("Position")!.GetValue(match));
			if (expected.IsSuccess) Assert.Equal(42, match.GetType().GetProperty("Value")!.GetValue(match));
		}
	}

	[Fact]
	public void Named_compilations_inherit_and_can_disable_buffered_forms()
	{
		var host = """"
			using DotGram;
			[Gram("Start = any*\nparse Start", BufferedInput = true, BufferedBytes = true)]
			[GramOptions(Suffix = "Inherited")]
			[GramOptions(Suffix = "Plain", BufferedInput = false, BufferedBytes = false)]
			public partial class Grammar { }
			"""";
		var assembly = GeneratorDriverTests.Build(host);
		Assert.NotNull(assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)]));
		Assert.NotNull(assembly.GetType("Grammar+Inherited")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)]));
		Assert.Null(assembly.GetType("Grammar+Plain")!.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)]));
		Assert.Null(assembly.GetType("Grammar+Plain")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)]));
	}

	[Fact]
	public void Explicit_buffered_input_materializes_even_when_legacy_streaming_is_available()
	{
		var grammar = """
			Start : @string[] = Row* recover eol => @("!" + parserText)
			Row : @string = text: ['a'..'z']+ & eol => @(text)
			parse Start stream
			""";
		var options = new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance };
		var compilation = GramCompiler.Compile(grammar, options);
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(compilation.Sources.Single().Text, declarationMembers: """
			public static string[] Whole(System.IO.TextReader input) => ParseStart(input, bufferSize: 2);
			public static string[] Default(System.IO.TextReader input) => ParseStart(input);
			""");
		Assert.Null(assembly.GetType("Grammar")!.GetMethod("ParseStart", [typeof(TextReader)]));
		Assert.Equal(typeof(string[]), assembly.GetType("Grammar")!.GetMethod("Default")!.ReturnType);
		var input = "abc\r\n123\r\nxyz\n";
		var expected = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
		var actual = Read(assembly, new ShortReader(input, 1), 2);
		Assert.Equal(expected.IsSuccess, actual.Success);
		Assert.Equal((string[])expected.Value!, (string[])actual.Value!);
		Assert.Equal(new[] { "abc", "xyz" }, (string[])assembly.GetType("Grammar")!.GetMethod("Whole")!
			.Invoke(null, [new StringReader("abc\r\nxyz\n")])!);
		var original = EmittedCode.Compile(GramCompiler.Compile(grammar.Replace(" stream", ""), options).Sources.Single().Text);
		Assert.NotNull(original.GetType("Grammar")!.GetMethod("ParseStart", [typeof(TextReader)]));
		Assert.Equal(new[] { "abc", "xyz" }, (string[])assembly.GetType("Grammar")!
			.GetMethod("Default")!.Invoke(null, [new StringReader("abc\r\nxyz\n")])!);
	}

	[Fact]
	public void Input_type_selects_the_public_overload()
	{
		var compilation = GramCompiler.Compile("Start = any*\nparse Start",
			new GramCompilerOptions { BufferedInput = true, BufferedBytes = true });
		EmittedCode.Quiet(compilation.Diagnostics);
		var assembly = EmittedCode.Compile(compilation.Sources.Single().Text, declarationMembers: """
			public static bool Check()
			{
				using var chars = new System.IO.StringReader("abc");
				using var bytes = new System.IO.MemoryStream(new byte[] { 0, 255 });
				using var tryChars = new System.IO.StringReader("abc");
				using var tryBytes = new System.IO.MemoryStream(new byte[] { 0, 255 });
				return ParseStart("abc") == "abc" && ParseStart(chars) == "abc" &&
					ParseStart(bytes)[1] == 255 && TryParseStart(tryChars).IsSuccess &&
					TryParseStart(tryBytes).Value[0] == 0;
			}
			""");
		Assert.Equal(true, assembly.GetType("Grammar")!.GetMethod("Check")!.Invoke(null, null));
	}

	[Theory]
	[InlineData("Start : @string = 'a' => @(parserInput)\nparse Start stream")]
	[InlineData("Start = '\\u0400'\nparse Start stream bytes")]
	public void Unsupported_forms_report_a_diagnostic(string grammar)
	{
		var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });
		Assert.Contains(compilation.Diagnostics, diagnostic => diagnostic.Id == GramCompiler.BufferedUnsupported && diagnostic.Severity == GramSeverity.Error);
	}

	[Fact]
	public void Byte_machine_materializes_typed_rule_captures()
	{
		var compilation = GramCompiler.Compile("""
			Start : @int[] = items: Item+ => @(items)
			Item : @int = "ab" => @(1) | "cd" => @(2)
			parse Start stream bytes
			""", new GramCompilerOptions { PartSize = 1, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(compilation.Diagnostics);
		var source = compilation.Sources.Single().Text;
		var assembly = EmittedCode.Compile(source);
		using var input = new ShortStream(System.Text.Encoding.ASCII.GetBytes("abcdab"));
		var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [input, 1, 100])!;
		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		Assert.Equal(new[] { 1, 2, 1 }, (int[])match.GetType().GetProperty("Value")!.GetValue(match)!);
	}

	[Theory]
	[InlineData("Start : @int = text: ['0'..'9']+ => @(ToInt(text))", "123", 123)]
	[InlineData("Start : @int = (text: ['0'..'9'] & ','?)+ => @(ToInt(text))", "1,2,3", 123)]
	[InlineData("Start : @int = text: ['0'..'9']* => @(ToInt(text))", "", 0)]
	[InlineData("Start : @int = text: ['0'..'9']+ & ('x' | 'y') => @(ToInt(text))", "123y", 123)]
	[InlineData("Start : @int = (text: ['0'..'9']+)? => @(ToInt(text))", "", 0)]
	[InlineData("Start : @int = ['0'..'9']+ => @(ToInt(parserText))", "123", 123)]
	[InlineData("Start : @int = text: ['0'..'9']+ & when @(ToInt(text) > 0) => @(ToInt(text))", "123", 123)]
	[InlineData("Start : @int = ((text: ['0'..'9']+ & 'x') | (text: ['0'..'9']+ & 'y')) => @(ToInt(text))", "123y", 123)]
	[InlineData("Start : @int = n: Number & '!' => @(n)\nNumber : @int = text: ['0'..'9']+ => @(ToInt(text))", "123!", 123)]
	[InlineData("Start : @int = text: ('\\u0000' & '\\u00ff') => @(ToInt(text))", "\0\u00ff", -273)]
	public void Actions_receive_native_spans(string grammar, string input, int expected)
	{
		foreach (var partSize in new[] { 1, 500 })
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
		{
			var compilation = GramCompiler.Compile(grammar + "\nparse Start stream bytes", new GramCompilerOptions
			{
				BufferedInput = true, SpanCaptures = true, PartSize = partSize, Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
			});
			EmittedCode.Quiet(compilation.Diagnostics);
			var source = compilation.Sources.Single().Text;
			Assert.Contains("ReadOnlySpan<byte>", source);
			Assert.Contains("ReadOnlySpan<char> text", source);
			var assembly = EmittedCode.Compile(source, declarationMembers: """
				static int ToInt(System.ReadOnlySpan<char> text)
				{
					int result = 0;
					for (var i = 0; i < text.Length; i++) result = checked(result * 10 + text[i] - '0');
					return result;
				}
				static int ToInt(System.ReadOnlySpan<byte> text)
				{
					int result = 0;
					for (var i = 0; i < text.Length; i++) result = checked(result * 10 + text[i] - (byte)'0');
					return result + 1000;
				}
				""");
			Assert.Equal(expected, EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).Value);
			Assert.Equal(expected, Read(assembly, new ShortReader(input, 1), 1).Value);
			using var bytes = new ShortStream(input.Select(c => checked((byte)c)).ToArray());
			var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [bytes, 1, 100])!;
			Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
			Assert.Equal(expected + 1000, match.GetType().GetProperty("Value")!.GetValue(match));
		}
	}

	[Fact]
	public void Span_option_is_inherited_and_removes_string_capture_allocations()
	{
		var assembly = GeneratorDriverTests.Build(""""
			using DotGram;
			[Gram("Start : @int = text: ['0'..'9']+ => @(ToInt(text))\nparse Start", SpanCaptures = true, BufferedBytes = true)]
			[GramOptions(Suffix = "Plain", SpanCaptures = false, BufferedBytes = false)]
			public partial class Grammar
			{
				static int ToInt(System.ReadOnlySpan<char> text) => text.Length;
				static int ToInt(System.ReadOnlySpan<byte> text) => text.Length;
				public static long Allocated(bool spans)
				{
					for (var i = 0; i < 1000; i++) { ParseStart("123"); Plain.ParseStart("123"); }
					var before = System.GC.GetAllocatedBytesForCurrentThread();
					var total = 0;
					for (var i = 0; i < 1000; i++) total += spans ? ParseStart("123") : Plain.ParseStart("123");
					var allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
					if (total != 3000) throw new System.InvalidOperationException();
					return allocated;
				}
			}
			"""");
		var measure = assembly.GetType("Grammar")!.GetMethod("Allocated")!;
		Assert.Equal(0L, (long)measure.Invoke(null, [true])!);
		Assert.True((long)measure.Invoke(null, [false])! >= 32000);
	}

	static (bool Success, object? Value, long Position) Read(Assembly assembly, TextReader reader, int capacity, int limit = int.MaxValue)
	{
		var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [reader, capacity, limit])!;
		object? Get(string property) => match.GetType().GetProperty(property)!.GetValue(match);
		return ((bool)Get("IsSuccess")!, Get("Value"), (long)Get("Position")!);
	}

	sealed class ShortReader(string text, int chunk) : TextReader
	{
		int _position;
		public bool Disposed { get; private set; }
		public override int Read(char[] buffer, int index, int count)
		{
			var length = Math.Min(Math.Min(chunk, count), text.Length - _position);
			text.CopyTo(_position, buffer, index, length);
			_position += length;
			return length;
		}
		protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
	}
}
