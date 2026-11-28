using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Language;

using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a string is over bytes, where the compiler gives a text rule a value of its own: an array that collects text rules
/// collects the bytes they matched, and so does a rule a guard reads a name of.
/// </summary>
public sealed class ByteTextValueTests
{
	const string Host = """
		static string Text(System.ReadOnlySpan<byte> text) { return System.Text.Encoding.ASCII.GetString(text.ToArray()); }
		static string Text(System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Text(string text) { return text; }
		static bool Seen(string v) { return true; }
		static bool Seen(System.ReadOnlySpan<char> v) { return true; }
		static bool Seen(System.ReadOnlySpan<byte> v) { return true; }
		""";

	/// <summary>The bytes of every element, where an array of text rules is read over a stream.</summary>
	static string[] Collected(string grammar, string input)
	{
		var parser = Build(grammar, Host, bytes: true);
		var match  = Match(parser, "TryParseStart", input);

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);

		return ((byte[][])match.GetType().GetProperty("Value")!.GetValue(match)!).Select(static one => Encoding.ASCII.GetString(one)).ToArray();
	}

	[Theory]
	[InlineData("Start : @string[] = Bare & (',' & Bare)*\nBare : @string = ['a'..'z']{2}\nparse Start stream bytes", "ab,cd,ef", "ab|cd|ef")]
	[InlineData("Start : @string[] = '<' & Bare & ',' & Bare & '>'\nBare : @string = ['a'..'z']{2}\nparse Start stream bytes", "<ab,cd>", "ab|cd")]
	[InlineData("Start : @string[] = Mid & ',' & Bare\nMid : Bare = '(' & Bare & ')'\nBare : @string = ['a'..'z']{2}\nparse Start stream bytes", "(ab),cd", "(ab)|cd")]
	[InlineData("Start : @string[] = G & (',' & G)*\nG : @string = g: ['a'..'z']{2} & when @(Seen(g))\nparse Start stream bytes", "ab,cd", "ab|cd")]
	[InlineData("Start : @string[] = '<' & G{0} & '#' & '>'\nG : @string = g: ['a'..'z']{2} & when @(Seen(g))\nparse Start stream bytes", "<#>", "")]
	public void An_array_of_text_rules_collects_the_bytes_they_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, string.Join("|", Collected(grammar, input)));
	}

	[Fact]
	public void An_array_of_text_rules_is_still_strings_over_characters()
	{
		var parser = Build("Start : @string[] = Bare & (',' & Bare)*\nBare : @string = ['a'..'z']{2}\nparse Start\nparse Start as Bytes stream bytes", Host, bytes: false);
		var read   = EmittedCode.Match(parser, "Grammar", "TryParseStart", "ab,cd");

		Assert.True(read.IsSuccess);
		Assert.Equal("ab|cd", string.Join("|", (string[])read.Value!));

		var bytes = Match(parser, "TryBytes", "ab,cd");

		Assert.Equal("ab|cd", string.Join("|", ((byte[][])bytes.GetType().GetProperty("Value")!.GetValue(bytes)!).Select(static one => Encoding.ASCII.GetString(one))));
	}

	/// <summary>
	/// One array holds the bytes of one text and the string another builds only through a decoding, which is not made.
	/// </summary>
	[Fact]
	public void An_array_of_the_bytes_of_a_text_and_a_string_another_builds_is_refused_over_bytes()
	{
		var result = Compiled(
			"Start : @string[] = '<' & Bare & ',' & Word & '>'\nBare : @string = ['a'..'z']{2}\nWord : @string = c: ['a'..'z']{2} => @(\"W:\" + Text(c))\nparse Start stream bytes",
			Host, bytes: true);
		var said = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		Assert.Contains(
			"'Start' collects into one array a piece of the input, from 'Bare', and a built string, from 'Word': over bytes the two " +
			"are not one element type without decoding; build the elements with '=>'",
			said.Message,
			StringComparison.Ordinal);
	}

	/// <summary>The same where the two are read through a rule with no type of its own, on one route of several.</summary>
	[Fact]
	public void An_array_mixing_the_two_through_a_rule_read_in_place_is_refused_over_bytes()
	{
		var result = Compiled(
			"Start : @string[] = '<' & U & '>' | Bare\nU = Bare & ',' & Word\nBare : @string = ['a'..'z']{2}\n" +
			"Word : @string = c: ['a'..'z']{2} => @(\"W:\" + Text(c))\nparse Start stream bytes",
			Host, bytes: true);
		var said = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		Assert.Contains("'Start' collects into one array a piece of the input, from 'Bare', and a built string, from 'Word'", said.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// The same where the built string comes through a call of another array, which gives its elements in place: a rule the
	/// author wrote, or a template's specialization. Over characters the two are one array of strings.
	/// </summary>
	[Theory]
	[InlineData("W : @string[] = Word", "W", "'Start'")]
	[InlineData("L(x) : @string[] = x", "L(Word)", "'Start'")]
	public void An_array_mixing_the_two_through_an_array_it_calls_is_refused_over_bytes(string array, string call, string named)
	{
		var grammar = "Start : @string[] = '<' & Bare & ',' & " + call + " & '>'\n" + array + "\nBare : @string = ['a'..'z']{2}\n" +
			"Word : @string = c: ['a'..'z']{2} => @(\"W:\" + Text(c))\nparse Start";

		var refused = Compiled(grammar + " stream bytes", Host, bytes: true);
		var said    = Assert.Single(refused.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		Assert.Contains(named + " collects into one array a piece of the input, from 'Bare', and a built string, from 'Word'", said.Message, StringComparison.Ordinal);

		var parser = Build(grammar, Host);
		var read   = EmittedCode.Match(parser, "Grammar", "TryParseStart", "<ab,cd>");

		Assert.True(read.IsSuccess);
		Assert.Equal("ab|W:cd", string.Join("|", (string[])read.Value!));
	}

	/// <summary>The same in a specialization of a template: the message names the rules the author wrote, not one made for them.</summary>
	[Fact]
	public void An_array_of_a_template_mixing_the_two_is_refused_over_bytes_by_the_names_written()
	{
		var result = Compiled(
			"Start = xs: L(Word)\nL(x) : @string[] = '<' & Bare & ',' & x & '>'\nBare : @string = ['a'..'z']{2}\n" +
			"Word : @string = c: ['a'..'z']{2} => @(\"W:\" + Text(c))\nparse Start stream bytes",
			Host, bytes: true);
		var said = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		Assert.Contains("'L' collects into one array a piece of the input, from 'Bare', and a built string, from 'Word'", said.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("_Value", said.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A join that a rule the grammar generates a type for does not name is not built over bytes, as a value nothing holds is not
	/// made over characters. A string reads what it calls as it stands, and one that is read is refused as before.
	/// </summary>
	[Fact]
	public void A_join_no_rule_holds_is_not_built_over_bytes()
	{
		const string grammar = "Start = n: Num & '<' & Ow & '>'\nNum : @int = d: ['0'..'9']{2} => @(int.Parse(Text(d)))\nOw : @string = '(' & Word & ')'\nWord : @string = c: ['a'..'z']{2} => @(\"W:\" + Text(c))\nparse Start stream bytes";

		var parser = Build(grammar, Host, bytes: true);
		var match  = Match(parser, "TryParseStart", "03<(cd)>");
		var value  = match.GetType().GetProperty("Value")!.GetValue(match)!;

		Assert.Equal(3, value.GetType().GetProperty("N")!.GetValue(value));

		// A string reads what it calls as it stands, whether it joins it or not.
		foreach (var call in new[] { "Ow", "Ow{0}", "?=(Ow)" })
		{
			var refused = Compiled($"Start : @string = '<' & {call} & '>'\nOw : @string = '(' & Word & ')'\nWord : @string = c: ['a'..'z']{{2}} => @(\"W:\" + Text(c))\nparse Start stream bytes", Host, bytes: true);
			var said    = Assert.Single(refused.Diagnostics, static one => one.Severity == GramSeverity.Error);

			Assert.Equal(GramCompiler.BufferedUnsupported, said.Id);
		}
	}

	[Fact]
	public void A_text_rule_a_guard_reads_a_name_of_gives_its_bytes_over_bytes()
	{
		var parser = Build("Start : @string = '<' & G & '>'\nG : @string = g: ['a'..'z']{2} & when @(Seen(g))\nparse Start stream bytes", Host, bytes: true);
		var match  = Match(parser, "TryParseStart", "<cd>");

		Assert.Equal("<cd>", Encoding.ASCII.GetString((byte[])match.GetType().GetProperty("Value")!.GetValue(match)!));
	}

	static GramCompilation Compiled(string grammar, string members, bool bytes = false, bool spans = false)
	{
		var host = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			SpanCaptures   = spans,
			BufferedBytes  = bytes,
		});
	}

	static Assembly Build(string grammar, string members, bool bytes = false, bool spans = false)
	{
		var result = Compiled(grammar, members, bytes, spans);

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>The match a publication reads from the bytes of <paramref name="input"/>.</summary>
	static object Match(Assembly parser, string method, string input)
	{
		var type = parser.GetType("Grammar")!;
		var read = type.GetMethods().Single(one => one.Name == method && one.GetParameters()[0].ParameterType == typeof(Stream));

		return read.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(input)), null, null])!;
	}
}
