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

	[Fact]
	public void Compiler_defaults_to_tables_and_can_select_the_previous_strategy()
	{
		var options = new GramCompilerOptions
		{
			Direct = false, BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		};
		Assert.True(options.PrefixTables);
		var compiled = GramCompiler.Compile(Grammar, options);
		EmittedCode.Quiet(compiled.Diagnostics);
		Assert.Equal(Emit(Grammar, true), Assert.Single(compiled.Sources).Text);
		Assert.DoesNotContain("Prefix_DotGram", Emit(Grammar, false));
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
		var method = parser.GetType("Grammar")!.GetMethod("TryParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;
		var match = method.Invoke(null, [bytes ? stream : reader, 1, 1024])!;
		object? Get(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return ((bool)Get("IsSuccess")!, Get("Value"), Get("Error"), Get("Position"));
	}

	sealed class OneCharReader(string input) : StringReader(input)
	{
		public override int Read(char[] buffer, int index, int count)
		{
			return base.Read(buffer, index, Math.Min(1, count));
		}
	}

	sealed class OneByteStream(byte[] bytes) : MemoryStream(bytes)
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

	[Theory]
	[InlineData("http", "https")]
	[InlineData("same", "same")]
	public void Overlapping_prefixes_keep_the_old_strategy(string first, string second)
	{
		var grammar = $"Start : @int = \"{first}\" & ['x'..'y']+ => @(1) | \"{second}\" & ['x'..'y']+ => @(2) | \"foo\" & ['x'..'y']+ => @(3) | \"bar\" & ['x'..'y']+ => @(4)\nparse Start";
		Assert.Equal(Emit(grammar, false), Emit(grammar, true));
	}

	// Literal-led alternatives that share first characters, as FIX tags do: "9=", "95=",
	// "96=" and "950=" all begin with 9, so a miss after "95" is one the table decides
	// alone, and one after "9" alone or "1" alone goes on to the group. Other takes what
	// Known refuses, so a failure there is followed by a way back.
	const string Tags = """
		Value = ['a'..'z']+
		Known : @int = "1=" & Value => @(1)
			| "10=" & Value => @(10)
			| "198=" & Value => @(198)
			| "55=" & Value => @(55)
			| "54=" & Value => @(54)
			| "9=" & Value => @(9)
			| "95=" & Value => @(95)
			| "96=" & Value => @(96)
			| "950=" & Value => @(950)
		Other : @int = ['0'..'9']+ & ':' & Value => @(-1)
		""";

	const string Alphabet = "109548=a:;";

	static string EmitWith(string grammar, bool tables, bool direct, bool buffered = true)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			PrefixTables = tables, Direct = direct, BufferedInput = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		return Assert.Single(result.Sources).Text;
	}

	/// <summary>
	/// Every input of up to four characters over the grammar's alphabet, and three thousand
	/// longer ones drawn with a fixed seed.
	/// </summary>
	static IEnumerable<string> Inputs()
	{
		var shorter = new List<string> { "" };

		for (var length = 1; length <= 4; length++)
			shorter.AddRange(shorter.Where(text => text.Length == length - 1).SelectMany(text => Alphabet.Select(c => text + c)).ToList());

		var random = new Random(20260918);
		var longer = Enumerable.Range(0, 3000).Select(_ => new string(Enumerable.Range(0, random.Next(5, 14)).Select(_ => Alphabet[random.Next(Alphabet.Length)]).ToArray()));

		return shorter.Concat(longer);
	}

	/// <summary>
	/// A match as everything it could report: outcome, value, where, how much, and the
	/// expected arrays in the order they were written down, besides the message made of them.
	/// </summary>
	static string Describe(object match)
	{
		var type = match.GetType();

		object? Property(string name)
		{
			return type.GetProperty(name)!.GetValue(match);
		}

		object? Field(string name)
		{
			return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(match);
		}

		var value    = Property("Value") is System.Collections.IEnumerable items and not string ? string.Join(",", items.Cast<object>()) : Property("Value")?.ToString();
		var expected = Field("_expected") is string[] first ? string.Join("|", first) : "null";
		// The one known difference, and the reason ties are compared by content. The chain may
		// start a tie list at a shorter literal's failure that a further one then empties, where
		// the fast failure never starts it: an empty list against none. Nothing outside the match
		// can tell them apart — the message is built from the contents — so they count as equal.
		var tied     = Field("_tied") is List<string[]> { Count: > 0 } more ? string.Join(" / ", more.Select(each => string.Join("|", each))) : "none";

		return $"{Property("Outcome")} [{value}] at {Property("Position")}+{Property("Length")} expected {expected} tied {tied} error {Property("Error")} otherwise {Field("_otherwise")}";
	}

	static void Same(string input, string chained, string tabled)
	{
		Assert.True(chained == tabled, "Input: " + input + Environment.NewLine + "chained: " + chained + Environment.NewLine + "tabled:  " + tabled);
	}

	static string Call(Assembly parser, string method, string input, bool reader)
	{
		var type = parser.GetType("Grammar")!;

		if (!reader)
			return Describe(type.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!);

		using var text = new OneCharReader(input);

		return Describe(type.GetMethod(method, [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [text, 1, 1024])!);
	}

	/// <summary>
	/// A miss the table decides fails the choice at once, and reports what trying every
	/// alternative reports: the same outcome and position, the same expected arrays in the
	/// same order, the same message, in a string and read a character at a time.
	/// </summary>
	[Fact]
	public void A_prefix_miss_reports_what_trying_the_alternatives_reports()
	{
		var grammar = Tags + Environment.NewLine + """
			Field : @int = v: Known => @(v) | v: Other => @(v)
			Start : @int = v: Field & ';' => @(v)
			parse Start stream
			""";
		var chained = EmittedCode.Compile(EmitWith(grammar, false, false));
		var tabled  = EmitWith(grammar, true, false);

		Assert.Contains("_Members", tabled);

		var parser = EmittedCode.Compile(tabled);

		foreach (var input in Inputs())
			foreach (var reader in new[] { false, true })
				Same(input, Call(chained, "TryParseStart", input, reader), Call(parser, "TryParseStart", input, reader));
	}

	/// <summary>
	/// Under recovery a failure moves the recovery's reach as well; the fast failure moves it
	/// through the same <c>Fail:</c>, and the fields recovered around it are the same.
	/// </summary>
	[Fact]
	public void A_prefix_miss_under_recovery_recovers_as_before()
	{
		var grammar = Tags + Environment.NewLine + """
			Field : @int = v: Known => @(v) | v: Other => @(v)
			Fields : @int[] = (v: Field & ';' => @(v))* recover ';' => @(-9)
			parse Fields stream
			""";
		var chained = EmittedCode.Compile(EmitWith(grammar, false, false));
		var parser  = EmittedCode.Compile(EmitWith(grammar, true, false));

		foreach (var input in Inputs())
			foreach (var reader in new[] { false, true })
				Same(input, Call(chained, "TryParseFields", input, reader), Call(parser, "TryParseFields", input, reader));
	}

	/// <summary>
	/// A lookahead over the choice records nothing of its own failures, and the fast failure
	/// must not either. Held on both renderings: whichever the generator gives the grammar, with
	/// the reader or on the engine, it answers the same with tables as without.
	/// </summary>
	[Fact]
	public void A_prefix_miss_inside_a_lookahead_reports_nothing_of_its_own()
	{
		var grammar = Tags + Environment.NewLine + """
			Look : @int = (?= Known) & v: Known => @(v) | (?! Known) & v: Other => @(v) | ['0'..'9'] & '!' => @(0)
			parse Look
			""";

		foreach (var direct in new[] { false, true })
		{
			var chained = EmittedCode.Compile(EmitWith(grammar, false, direct, buffered: false));
			var parser  = EmittedCode.Compile(EmitWith(grammar, true, direct, buffered: false));

			foreach (var input in Inputs())
				Same(input, Call(chained, "TryParseLook", input, false), Call(parser, "TryParseLook", input, false));
		}
	}
}
