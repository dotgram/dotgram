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

/// <summary>
/// The value of a whole parse of a rule whose value is the text it matched leaves out the trivia
/// before and after it, in every reading: a string, a reader, a stream and bytes held whole, with
/// the direct reader and without it, on every carrier, and over tokens as over characters
/// (docs/syntax.md §6.3). The trivia is what the parse allows around the rule, not part of the
/// rule's text: what comes back is the value, and neither the trivia before it nor the trivia after.
/// </summary>
/// <remarks>
/// Each grammar reaches the whole parse by another road: a rule lowered to a plain method, one the
/// parse comes back into when the end of the input is not reached the first time, one that calls
/// itself, one whose guard keeps it out of the plain method, one published with <c>stream</c>, and
/// trivia that is a comment and not only spaces.
/// </remarks>
public sealed class WholeValueTriviaTests
{
	const string Spaces = "trivia = { ' '* }\n";

	/// <summary>Each grammar, with inputs and the value each must give.</summary>
	static readonly Dictionary<string, (string Grammar, string Method, (string Input, string Value)[] Rows)> Grammars = new()
	{
		["a run"] = (
			Spaces + "Root = ['a'..'z']+\nparse Root\n",
			"TryParseRoot",
			[("ab", "ab"), ("  ab", "ab"), ("ab  ", "ab"), ("  ab  ", "ab")]),

		["a choice read again"] = (
			Spaces + "Root = 'a' | 'a' & 'b'\nparse Root\n",
			"TryParseRoot",
			[(" a ", "a"), (" a b ", "a b"), ("ab ", "ab"), ("  ab", "ab")]),

		["a recursion"] = (
			Spaces + "Root = '(' & Root & ')' | 'x'\nparse Root\n",
			"TryParseRoot",
			[(" ( x ) ", "( x )"), ("(x)  ", "(x)"), ("  x", "x"), ("x", "x")]),

		["a guard"] = (
			Spaces + "Root = ('a' | 'b')+ & when @(true)\nparse Root\n",
			"TryParseRoot",
			[(" ab ", "ab"), ("ba  ", "ba"), ("  a", "a")]),

		["a stream publication"] = (
			Spaces + "Rows = ('a'+ & ';')*\nparse Rows stream\n",
			"TryParseRows",
			[(" a ; aa ; ", "a ; aa ;"), ("a;  ", "a;"), ("  a;", "a;")]),

		["a comment"] = (
			"trivia = { (' ' | '\\n' | \"--\" & [^ '\\n']*)* }\nRoot = (['a'..'z']+ & '.')* & ['a'..'z']+\nparse Root\n",
			"TryParseRoot",
			[("--x\n ab.cd --y", "ab.cd"), (" ab --y\n", "ab"), ("--x\nab", "ab")]),
	};

	public static TheoryData<string, CarrierKind, bool> Readings
	{
		get
		{
			var data = new TheoryData<string, CarrierKind, bool>();

			foreach (var name in Grammars.Keys)
				foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
					foreach (var direct in new[] { true, false })
						data.Add(name, carrier, direct);

			return data;
		}
	}

	/// <summary>Every input read as a string, through a reader, a stream and bytes, gives the value without the trivia.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_whole_value_leaves_out_the_trivia_around_it(string name, CarrierKind carrier, bool direct)
	{
		var (grammar, method, rows) = Grammars[name];
		var type = Compiled(grammar, carrier, direct).GetType("Grammar")!;

		foreach (var (input, value) in rows)
		{
			Assert.Equal((true, value), Read(type.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!));
			Assert.Equal((true, value), Read(Over(type, method, typeof(TextReader), new StringReader(input))));
			Assert.Equal((true, value), Read(Over(type, method, typeof(Stream), new MemoryStream(Encoding.ASCII.GetBytes(input)))));
			Assert.Equal((true, value), Read(type.GetMethod(method, [typeof(byte[])])!.Invoke(null, [Encoding.ASCII.GetBytes(input)])!));
		}
	}

	/// <summary>
	/// The same grammar over characters and over tokens gives one value: the lexer skips the trivia
	/// between tokens, and the parse over characters leaves it out of the value as well.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_whole_value_over_tokens_is_the_value_over_characters(bool direct)
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Word = ['a'..'z']+
			}
			Root = Lexical.Word & ':' & Lexical.Word
			parse Root
			""";

		var over = Compile(grammar, new GramCompilerOptions { Direct = direct }).GetType("Grammar")!;
		var read = Compile(grammar, new GramCompilerOptions { Direct = direct, Lexical = true }).GetType("Grammar")!;

		foreach (var (input, value) in new[] { ("ab:cd", "ab:cd"), ("  ab:cd", "ab:cd"), ("ab : cd  ", "ab : cd"), ("  ab: cd  ", "ab: cd") })
		{
			Assert.Equal((true, value), Read(over.GetMethod("TryParseRoot", [typeof(string)])!.Invoke(null, [input])!));
			Assert.Equal((true, value), Read(read.GetMethod("TryParseRoot", [typeof(string)])!.Invoke(null, [input])!));
		}
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	static Assembly Compiled(string grammar, CarrierKind carrier, bool direct)
	{
		return Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			BufferedInput = true,
			BufferedBytes = true,
		});
	}

	static Assembly Compile(string grammar, GramCompilerOptions options)
	{
		options.ClassName     = "Grammar";
		options.CSharpScanner = RoslynCSharpScanner.Instance;

		var result = GramCompiler.Compile(grammar, options);

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>A <c>Try…</c> form over a reader or a stream, in pieces of one item, so that a refill meets every boundary.</summary>
	static object Over(Type type, string method, Type over, IDisposable source)
	{
		using (source)
			return type.GetMethod(method, [over, typeof(int?), typeof(int?)])!.Invoke(null, [source, 1, null])!;
	}

	/// <summary>Whether the match read, and its value as text.</summary>
	static (bool IsSuccess, string? Value) Read(object match)
	{
		object? Property(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		var value = Property("Value");

		return ((bool)Property("IsSuccess")!, value is byte[] bytes ? Encoding.ASCII.GetString(bytes) : (string?)value);
	}
}
