using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The grammar's context and the reading a parser is, named inside a terminal of a grammar cut into a lexer and a
/// syntactic half: the tokenizer and the syntactic half hand both to the machine that reads such a terminal over its
/// text, wherever that machine asks for them.
/// </summary>
/// <remarks>
/// <para>
/// A terminal the automaton only begins — one that binds text, PostgreSQL's dollar quote — is measured by its own rule
/// over characters, and a terminal that builds a value has that value built by the same machine from the token. Every
/// method of that machine takes the context where anything it reads names <c>context</c>, and the reading where a
/// <c>when … is …</c> it reads is answered while the parser runs. The tokenizer measured without either and the
/// syntactic half built values without them (CS7036), so such a grammar did not compile; the located argument had been
/// added to those same calls before, one call at a time.
/// </para>
/// <para>
/// The context is the parse's: every publication of the grammar takes it, the tokens are cut inside that publication,
/// and a cutting made with it is not kept for the next reading of the same string — the same text under another
/// context, or read as another reading, is another cutting.
/// </para>
/// </remarks>
public sealed class LexicalContextTests
{
	static string Source(string carrier, string options, string guard, bool built)
	{
		var number = built ? "Num : @string = d: ['0'..'9']+ => @(context.Ok ? \"y\" : \"n\")" : "";
		var value  = built
			? "Value : @Text = t: Quoted & n: Num => @(new Text(t + n))"
			: "Value : @Text = t: Quoted => @(new Text(t))";

		return $$""""
			using DotGram;

			namespace LexicalContextProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);

					void Locate(int at, int length, int gapStart);
				}

				public sealed class Ctx
				{
					public bool Ok = true;
				}

				public sealed class Text : ILoc
				{
					public Text(string value)
					{
						Value = value;
					}

					public string Value { get; }

					public int At = -1;

					public void Locate(int at, int length)
					{
						At = at;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At = at;
					}
				}

				[Gram("""
					using Lexical;

					context : @Ctx

					trivia = { Lexical.Space* }

					namespace Lexical
					{
						trivia = none

						Space  = ' '
						Quoted = { '$' & $tag: (['a'..'z']*) & '$' {{guard}} & (?!('$' & $tag & '$') & any)* & '$' & $tag & '$' }
						{{number}}
					}

					{{value}}

					parse Value
					""", Lexical = true{{carrier}})]
				{{options}}
				public static partial class P
				{
				}
			}
			"""";
	}

	const string PerCall = "[GramOptions(LocationType = typeof(ILoc), Suffix = \"Located\", PerCall = true)]";

	/// <summary>A guard of the terminal the tokenizer measures names the context: the tokenizer is handed it.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_guard_of_a_measured_terminal_reads_the_context(bool perCall)
	{
		var assembly = GeneratorDriverTests.Build(Source("", perCall ? PerCall : "", "& when @(context.Ok)", built: false));
		var host     = assembly.GetType("LexicalContextProbe.P")!;

		Assert.Equal(("$ab$ x $a$ y $ab$", -1), Read(host, "  $ab$ x $a$ y $ab$", ok: true));
		Assert.Null(Read(host, "  $ab$ x $ab$", ok: false));

		if (perCall)
		{
			var door = host.GetNestedType("Located")!;

			Assert.Equal(("$ab$ x $ab$", 2), Read(door, "  $ab$ x $ab$", ok: true));
			Assert.Null(Read(door, "  $ab$ x $ab$", ok: false));
		}
	}

	/// <summary>
	/// A construction of a terminal the syntactic half has built from its token names the context: the syntactic half
	/// hands it on, under either carrier, and the tokenizer, measuring the other terminal, is handed it too — one
	/// machine reads both, and every method of it takes the context.
	/// </summary>
	[Theory]
	[InlineData("Tape",      false)]
	[InlineData("Immediate", false)]
	[InlineData("Tape",      true)]
	[InlineData("Immediate", true)]
	public void A_construction_of_a_terminal_built_from_its_token_reads_the_context(string carrier, bool perCall)
	{
		var assembly = GeneratorDriverTests.Build(
			Source($", Carrier = GramCarrier.{carrier}", perCall ? PerCall : "", "", built: true), "GRAM5012");
		var host     = assembly.GetType("LexicalContextProbe.P")!;

		Assert.Equal(("$a$ x $a$y", -1), Read(host, "  $a$ x $a$ 12", ok: true));
		Assert.Equal(("$a$ x $a$n", -1), Read(host, "  $a$ x $a$ 12", ok: false));

		if (perCall)
		{
			var door = host.GetNestedType("Located")!;

			Assert.Equal(("$a$ x $a$y", 2), Read(door, "  $a$ x $a$ 12", ok: true));
			Assert.Equal(("$a$ x $a$n", 2), Read(door, "  $a$ x $a$ 12", ok: false));
		}
	}

	/// <summary>
	/// A cutting made with the context is not kept: the same string read from a position again, under a context that
	/// answers otherwise, is cut again and answered otherwise.
	/// </summary>
	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_cutting_made_with_the_context_is_not_kept_for_the_next_reading(string carrier)
	{
		var assembly = GeneratorDriverTests.Build($$""""
			using DotGram;

			namespace LexicalContextKept
			{
				public sealed class Ctx
				{
					public bool Ok = true;
				}

				[Gram("""
					context : @Ctx

					wordboundary = ['a'..'z']
					trivia = { ' '* }

					namespace Tokens
					{
						trivia = none

						Name = { ['a'..'z'] & ['a'..'z']* & when @(context.Ok) }
					}

					Start : @string = t: Tokens.Name+ => @(t)

					parse Start
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
				}
			}
			"""", "GRAM5012");
		var host     = assembly.GetType("LexicalContextKept.P")!;
		var context  = assembly.GetType("LexicalContextKept.Ctx")!;
		var read     = host.GetMethods().Single(static one =>
			one.Name == "TryParseStart" && one.GetParameters() is [_, { ParameterType.IsByRef: true, IsOut: false }, _, { IsOut: true }]);
		var text     = "ab cd";

		// One string, one reading after another: the first cuts it with the context that accepts, and the second,
		// under one that refuses every name, must not be handed that cutting.
		foreach (var ok in new[] { true, false, true })
		{
			var given = Activator.CreateInstance(context)!;

			context.GetField("Ok")!.SetValue(given, ok);

			object?[] arguments = [text, 0, given, null];

			Assert.Equal(ok, (bool)read.Invoke(null, arguments)!);
			Assert.Equal(ok ? text : null, arguments[3]);
		}
	}

	/// <summary>
	/// A <c>when … is …</c> inside a terminal the tokenizer measures, answered while the parser runs because two
	/// publications read the grammar differently: the tokenizer is told which reading it cuts for, and so is the syntactic
	/// half, which builds the terminal's value from the token.
	/// </summary>
	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_test_of_the_reading_inside_a_measured_terminal_is_told_the_reading(string carrier)
	{
		var assembly = GeneratorDriverTests.Build($$""""
			using DotGram;

			namespace LexicalReading
			{
				[Gram("""
					using Lexical;

					Version = "a" | "b"

					trivia = { Lexical.Space* }

					namespace Lexical
					{
						trivia = none

						Space  = ' '
						Quoted : @string = { t: ('$' & $tag: (['a'..'z']*) & '$' & Mark & (?!('$' & $tag & '$') & any)* & '$' & $tag & '$') } => @(t + "!")
						Mark   = 'x' & when Version is "a" | 'y'
					}

					Value : @string = t: Quoted => @(t)

					parse Value with (Version = "a") as A
					parse Value with (Version = "b") as B
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
				}
			}
			"""", "GRAM5012");
		var host     = assembly.GetType("LexicalReading.P")!;

		Assert.Equal("$q$x1$q$!", Reading(host, "TryA", " $q$x1$q$"));
		Assert.Null(Reading(host, "TryB", " $q$x1$q$"));
		Assert.Equal("$q$y1$q$!", Reading(host, "TryA", " $q$y1$q$"));
		Assert.Equal("$q$y1$q$!", Reading(host, "TryB", " $q$y1$q$"));
	}

	/// <summary>What a door reads of the whole input under a context, and where it was told it was; null on a refusal.</summary>
	static (string Text, int At)? Read(Type door, string input, bool ok)
	{
		var context = door.Assembly.GetType("LexicalContextProbe.Ctx")!;
		var given   = Activator.CreateInstance(context)!;

		context.GetField("Ok")!.SetValue(given, ok);

		var parse = door.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(static one =>
			one.Name == "TryParseValue" && one.GetParameters() is [{ ParameterType.Name: "String" }, _, { IsOut: true }]);

		object?[] arguments = [input, given, null];

		if (!(bool)parse.Invoke(null, arguments)!)
			return null;

		var value = arguments[2]!;

		return ((string)value.GetType().GetProperty("Value")!.GetValue(value)!, (int)value.GetType().GetField("At")!.GetValue(value)!);
	}

	/// <summary>What one reading's door reads of the whole input; null on a refusal.</summary>
	static string? Reading(Type host, string method, string input)
	{
		var parse = host.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(one =>
			one.Name == method && one.GetParameters() is [{ ParameterType.Name: "String" }, { IsOut: true }]);

		object?[] arguments = [input, null];

		return (bool)parse.Invoke(null, arguments)! ? (string)arguments[1]! : null;
	}
}
