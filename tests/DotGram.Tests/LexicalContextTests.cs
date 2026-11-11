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
	/// A cutting whose measuring reads the context is not kept: the same string read from a position again, under a
	/// context that answers otherwise, is cut again and answered otherwise — a new object, or the same one changed
	/// between the two readings, which a cache keyed by the object would have answered from the first.
	/// </summary>
	[Theory]
	[InlineData("Tape",      false)]
	[InlineData("Immediate", false)]
	[InlineData("Tape",      true)]
	[InlineData("Immediate", true)]
	public void A_cutting_made_with_the_context_is_not_kept_for_the_next_reading(string carrier, bool same)
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

					positional parse Start
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
		var kept     = Activator.CreateInstance(context)!;

		// One string, one reading after another: the first cuts it with the context that accepts, and the second,
		// under one that refuses every name, must not be handed that cutting.
		foreach (var ok in new[] { true, false, true })
		{
			var given = same ? kept : Activator.CreateInstance(context)!;

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

	/// <summary>
	/// Two readings of one string, from a position, one after another: a cutting whose measuring tests the reading is
	/// kept beside the reading it was cut as, so the other publication cuts its own rather than reading the first one's,
	/// and each is kept: read again, neither is measured again, which a guard of the measured terminal counts.
	/// </summary>
	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_cutting_kept_for_one_reading_is_not_handed_to_another(string carrier)
	{
		var assembly = GeneratorDriverTests.Build($$""""
			using DotGram;

			namespace LexicalReadingKept
			{
				[Gram("""
					Version = "a" | "b"

					wordboundary = ['a'..'z']
					trivia = { ' '* }

					namespace Tokens
					{
						trivia = none

						Quoted = { '$' & $tag: (['a'..'z']*) & '$' & when @(Measured()) & Mark & (?!('$' & $tag & '$') & any)* & '$' & $tag & '$' }
						Mark   = 'x' & when Version is "a" | 'y'
					}

					Start : @string = t: Tokens.Quoted => @(t) | "(" & s: Start & ")" => @(s)

					positional parse Start with (Version = "a") as A
					positional parse Start with (Version = "b") as B
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
					public static int Measurings;

					static bool Measured()
					{
						Measurings++;

						return true;
					}
				}
			}
			"""", "GRAM5012");
		var count    = assembly.GetType("LexicalReadingKept.P")!.GetField("Measurings")!;
		var text     = "$q$x1$q$";
		var cut      = 0;

		// The reading that accepts first, so that what it cut is there to be handed on, and then the other; and then both
		// again, which is answered from what each cut. Start reaches itself so that it is read by the engine, which is
		// where a reading from a position is offered (§6.3).
		foreach (var round in new[] { 0, 1 })
		{
			foreach (var (method, read) in new[] { ("TryA", true), ("TryB", false) })
			{
				var answer = EmittedCode.Answered(assembly, "LexicalReadingKept.P", method, text, 0);

				Assert.Equal(read, answer.Read);
				Assert.Equal(read ? text : null, answer.Value);
			}

			if (round == 0)
				cut = (int)count.GetValue(null)!;
		}

		Assert.True(cut > 0);
		Assert.Equal(cut, (int)count.GetValue(null)!);
	}

	/// <summary>
	/// A construction that names the context builds a value a guard of the measured terminal then reads: the measuring
	/// reads the context through that value, so the cutting is not kept, and the same context object changed between
	/// two readings from a position is answered by a cutting made under it — reached through a nested rule, a
	/// specialization of a rule with a parameter, and a rule of another namespace the terminal's imports.
	/// </summary>
	[Theory]
	[InlineData("nested",    "Tape")]
	[InlineData("nested",    "Immediate")]
	[InlineData("parameter", "Tape")]
	[InlineData("parameter", "Immediate")]
	[InlineData("imported",  "Tape")]
	[InlineData("imported",  "Immediate")]
	public void A_guard_reading_a_value_built_from_the_context_keeps_no_cutting(string through, string carrier)
	{
		// Lines of the grammar, each indented as the grammar's own are inside the host below.
		const string Line = "\n\t\t\t\t\t";

		var flag = "Flag : @bool = '-' => @(context.Ok)";
		var (flags, uses, called, local) = through switch
		{
			"nested"    => ("", "", "Flag", flag),
			"parameter" => ("", "", "Marked('-')", "Marked(mark) : @bool = mark => @(context.Ok)"),
			_           => ("namespace Flags" + Line + "{" + Line + "\ttrivia = none" + Line + Line + "\t" + flag + Line + "}", "using Flags;", "Flag", ""),
		};
		var assembly = GeneratorDriverTests.Build($$""""
			using DotGram;

			namespace LexicalBuiltGuard
			{
				public sealed class Ctx
				{
					public bool Ok = true;
				}

				[Gram("""
					{{uses}}

					context : @Ctx

					wordboundary = ['a'..'z']
					trivia = { ' '* }

					{{flags}}

					namespace Tokens
					{
						trivia = none

						Name : @string = { t: (['a'..'z'] & ['a'..'z']*) & f: {{called}} & when @(f) } => @(t)
						{{local}}
					}

					Start : @string = n: Tokens.Name+ => @(string.Concat(n))

					positional parse Start
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
				}
			}
			"""", "GRAM5012");
		var host     = assembly.GetType("LexicalBuiltGuard.P")!;
		var context  = assembly.GetType("LexicalBuiltGuard.Ctx")!;
		var read     = host.GetMethods().Single(static one =>
			one.Name == "TryParseStart" && one.GetParameters() is [_, { ParameterType.IsByRef: true, IsOut: false }, _, { IsOut: true }]);
		var given    = Activator.CreateInstance(context)!;
		var text     = "ab- cd-";

		foreach (var ok in new[] { true, false, true })
		{
			context.GetField("Ok")!.SetValue(given, ok);

			object?[] arguments = [text, 0, given, null];

			Assert.Equal(ok, (bool)read.Invoke(null, arguments)!);
			Assert.Equal(ok ? "abcd" : null, arguments[3]);
		}
	}

	/// <summary>
	/// A guard that reads a capture's value inside a terminal the lexer measures, with no context anywhere: the machine
	/// that measures it builds that value as it reads, through the walk over values so far the parser it writes into
	/// keeps for it — which that parser used to keep only for a guard of the syntax's own (CS1061).
	/// </summary>
	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_guard_reading_a_captured_value_inside_a_measured_terminal_is_read(string carrier)
	{
		// The same grammar over characters is what the tokens have to answer.
		var split = Read(carrier, lexical: true);
		var whole = Read(carrier, lexical: false);

		Assert.Equal("12,34,7", split("12 34 7"));

		foreach (var input in new[] { "12 34 7", "12 345", "345", "99 100", "100" })
			Assert.Equal(whole(input), split(input));

		static Func<string, string?> Read(string carrier, bool lexical)
		{
			var name     = "LexicalGuardCapture" + (lexical ? "Split" : "Whole");
			var assembly = GeneratorDriverTests.Build($$""""
				using DotGram;

				namespace {{name}}
				{
					[Gram("""
						wordboundary = ['a'..'z' | '0'..'9']
						trivia = { ' '* }

						namespace Tokens
						{
							trivia = none

							Num   : @int = d: (['0'..'9'] & ['0'..'9']*) => @(int.Parse(d))
							Small : @int = { n: Num & when @(n < 100) } => @(n)
						}

						Start : @string = v: Tokens.Small+ => @(string.Join(",", v))

						parse Start
						""", Lexical = {{(lexical ? "true" : "false")}}, Carrier = GramCarrier.{{carrier}})]
					public static partial class P
					{
					}
				}
				"""", "GRAM5012");
			var parse    = assembly.GetType(name + ".P")!.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(static one =>
				one.Name == "TryParseStart" && one.GetParameters() is [{ ParameterType.Name: "String" }, { IsOut: true }]);

			return input =>
			{
				object?[] arguments = [input, null];

				return (bool)parse.Invoke(null, arguments)! ? (string)arguments[1]! : null;
			};
		}
	}

	/// <summary>
	/// A context named only where a terminal's value is built is handed to the tokenizer and read by nothing it measures,
	/// so the cutting is still kept: a host reading one value after another from one text measures each token once,
	/// however many values it reads, rather than the whole text again for every one.
	/// </summary>
	/// <remarks>
	/// Counted rather than timed: a guard of the measured terminal counts its measurings, and the text read a word at a
	/// time is measured once a word — linear in the text, where a cutting made again for every value is the square.
	/// </remarks>
	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_context_named_only_by_a_construction_keeps_the_cutting(string carrier)
	{
		var assembly = GeneratorDriverTests.Build($$""""
			using DotGram;

			namespace LexicalBuiltKept
			{
				public sealed class Ctx
				{
					public string Suffix = "!";
				}

				[Gram("""
					context : @Ctx

					wordboundary = ['a'..'z' | '0'..'9']
					trivia = { ' '* }

					namespace Tokens
					{
						trivia = none

						Name = { ['a'..'z'] & ['a'..'z']* & when @(Measured()) }
						Num : @string = d: (['0'..'9'] & ['0'..'9']*) => @(d + context.Suffix)
					}

					Start : @string = t: Tokens.Name => @(t) | n: Tokens.Num => @(n)

					positional parse Start
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
					public static int Measurings;

					static bool Measured()
					{
						Measurings++;

						return true;
					}
				}
			}
			"""", "GRAM5012");
		var host     = assembly.GetType("LexicalBuiltKept.P")!;
		var context  = assembly.GetType("LexicalBuiltKept.Ctx")!;
		var read     = host.GetMethods().Single(static one =>
			one.Name == "TryParseStart" && one.GetParameters() is [_, { ParameterType.IsByRef: true, IsOut: false }, _, { IsOut: true }]);
		var count    = host.GetField("Measurings")!;
		var given    = Activator.CreateInstance(context)!;

		foreach (var words in new[] { 100, 400 })
		{
			var text   = string.Join(" ", Enumerable.Repeat("abcd 12", words / 2));
			var values = new System.Collections.Generic.List<string>();
			var at     = 0;

			count.SetValue(null, 0);

			while (at < text.Length)
			{
				object?[] arguments = [text, at, given, null];

				Assert.True((bool)read.Invoke(null, arguments)!);

				at = (int)arguments[1]!;
				values.Add((string)arguments[3]!);
			}

			Assert.Equal(words, values.Count);
			Assert.Equal("12!", values[1]);
			Assert.Equal(words / 2, (int)count.GetValue(null)!);
		}
	}

	/// <summary>
	/// A guard of a terminal built from its token that names the context is asked again where the value is built, against
	/// the context as the syntax has written it by then: the generator warns of it (GRAM5019), and where the guard now
	/// refuses the token the parse throws, naming the terminal and where its token stood; where another alternative
	/// takes it, the value is that alternative's (§7.1). A terminal whose value is its token's own text is not read
	/// again: its guard runs once, the value is the token, and nothing is said.
	/// </summary>
	/// <remarks>
	/// Not said either where no guard of a terminal read again names the context: every other test here builds its
	/// grammar refusing any warning, a context named only by a construction among them.
	/// </remarks>
	[Theory]
	[InlineData("Tape",      "refused")]
	[InlineData("Immediate", "refused")]
	[InlineData("Tape",      "other")]
	[InlineData("Immediate", "other")]
	[InlineData("Tape",      "verbatim")]
	[InlineData("Immediate", "verbatim")]
	public void A_guard_asked_again_where_the_value_is_built_sees_the_context_as_written(string carrier, string shape)
	{
		var terminal = shape switch
		{
			"refused"  => "Num : @string = { d: ['0'..'9'] & ['0'..'9']* & when @(context.Ok) } => @(d)",
			"other"    => "Num : @string = { d: (['0'..'9'] & ['0'..'9']*) & when @(context.Ok) | e: (['0'..'9'] & ['0'..'9']*) } => @(d != null ? \"guarded:\" + d : \"other:\" + e)",
			_          => "Num : @string = { d: (['0'..'9'] & ['0'..'9']*) & when @(context.Ok) } => @(d)",
		};
		var source   = $$""""
			using DotGram;

			namespace LexicalWritten
			{
				public sealed class Ctx
				{
					public bool Ok = true;

					public bool Flip()
					{
						Ok = false;

						return true;
					}
				}

				[Gram("""
					context : @Ctx

					wordboundary = ['a'..'z']
					trivia = { ' '* }

					namespace Tokens
					{
						trivia = none

						{{terminal}}
					}

					Start : @string = "go" & when @(context.Flip()) & n: Tokens.Num => @(n ?? "null")

					parse Start
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
				}
			}
			"""";
		var warned   = GeneratorDriverTests.RunGenerator(source).Diagnostics.Where(static one => one.Id == "GRAM5019").ToArray();

		if (shape == "verbatim")
			Assert.Empty(warned);
		else
			Assert.Contains("'Num' is read again from its token", Assert.Single(warned).GetMessage(), StringComparison.Ordinal);

		var assembly = GeneratorDriverTests.Build(source, "GRAM5019");
		var host     = assembly.GetType("LexicalWritten.P")!;
		var context  = assembly.GetType("LexicalWritten.Ctx")!;
		var parse    = host.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(static one =>
			one.Name == "TryParseStart" && one.GetParameters() is [{ ParameterType.Name: "String" }, _, { IsOut: true }]);

		object?[] arguments = ["go 12", Activator.CreateInstance(context)!, null];

		if (shape == "refused")
		{
			var thrown = Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, arguments));
			var inner  = Assert.IsType<InvalidOperationException>(thrown.InnerException);

			Assert.Contains("Num token read at 3", inner.Message, StringComparison.Ordinal);

			return;
		}

		Assert.True((bool)parse.Invoke(null, arguments)!);
		Assert.Equal(shape == "other" ? "other:12" : "12", arguments[2]);
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
