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
/// The outcome, the position and the value of a reading are the same in every form and in both
/// readers (docs/syntax.md §7.5: "The outcome, the position and the value do not vary this way:
/// those are the same in every form, and what a host branches on"). Each row below is one shape
/// on which the direct reader and the engine answered differently; the expected answer is the
/// one the contract names, so a failing row says which reader is wrong.
/// </summary>
public sealed class FormOutcomeTests
{
	const string Host = """
		static bool Odd(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == 'o') { p++; return true; } return false; }
		static bool Odd(System.ReadOnlySpan<byte> input, ref int p) { if (p < input.Length && input[p] == (byte)'o') { p++; return true; } return false; }
		""";

	const string Lexical = """
		trivia = { ' '* }
		namespace Lexical
		{
			trivia = none
			Digits : @int = { ['0'..'9']+ } => @(parserText.Length)
		}

		""";

	sealed record Row(string Grammar, string Input, string Form, int At, string Expected, bool Lexical = false, bool Buffered = true);

	/// <summary>
	/// One row a class of disagreement: the shape, the input and the form it shows on, and what
	/// §7.5 (and §6.3 for a reading from a position) says the answer is.
	/// </summary>
	static readonly Dictionary<string, Row> Rows = new()
	{
		// A lookahead that refuses is a refusal at its own place: the furthest position the parse
		// reached (§7.5). The direct reader records nothing for it and answers from before it.
		["LookRefusedAtItsPlace"] = new("R0 = 'z' & ?!(('a')?) & eof\nparse R0\n", "z", "string", 0, "Starved@1"),

		// The same after an optional: `any+` reads to the end, the look is refused there, and more
		// input could still fit — Starved at 2, not NoMatch at 0.
		["LookRefusedAfterRun"] = new("R0 = 'b' & any+ & ?='b' & eof\nparse R0\n", "ba", "string", 0, "Starved@2"),

		// A positive lookahead the engine keeps on its arena records nothing when it fails, and the
		// alternative before it was dispatched past without recording: the answer is then the
		// initial position, before the place the reading began.
		["LookOnArenaRefused"] = new("R0 : @int = 'd' => @(0) | ?=(['a'..'c']+) => @(1)\nparse R0\n", "yz", "at", 1, "NoMatch@1"),

		// A reading that begins where it is told cannot be refused before that place (§6.3).
		["RefusalNotBeforeStart"] = new("trivia = ' '*\nR0 : @int = ?!('a') & 'b' => @(0)\nparse R0\n", "xa", "at", 1, "NoMatch@1"),

		// A literal the input ends inside of, read inside a lookahead, is the look's business:
		// the parse itself was refused at 0 by input that was there (NoMatch), not starved.
		["RunOutInsideLook"] = new("R0 = ?!(\"kk\") & 'z'\nparse R0\n", "k", "string", 0, "NoMatch@0"),

		// `parserText` is the rule's own extent, from where the rule began to where the parse
		// stands (§4): the trivia after the root rule belongs to the whole reading, not to it.
		["RootTextWithoutTrailingTrivia"] = new("trivia = ' '*\nR0 : @int = 'd' => @(parserText.Length)\nparse R0\n", "d ", "string", 0, "Success@0+2 1"),

		// The trivia a positional reading starts on is skipped, not read: `Position` says where the
		// value began, past it (§6.3). The direct reader gives the trivia back to the rule.
		["LeadingTriviaIsSkipped"] = new("trivia = ' '*\nR0 : @int = any => @(0)\nparse R0\n", " ", "at", 0, "Starved@1"),

		// A recovering repetition tries the complete continuation first (§8.2); from a position the
		// continuation is nothing and succeeds, so a refused element ends the repetition.
		// Read over a span only: a host recognizer over a span is not offered the buffered forms.
		["RecoveryEndsFromAPosition"] = new("R1 : @int = 'z' & ';' => @(0)\nR0 : @int[] = R1* recover @Odd => @(0)\nparse R0\n", "z", "at", 0, "Success@0+0 []", Buffered: false),

		// A literal the input ends inside has run out where every character agrees with it (§7.5):
		// the engine reads the rule as a scan and loses that reach.
		["ScannedLiteralRanOut"] = new("R0 = ((R1 & 'r')*)? & eof\nR1 = (\"ab\")?\nparse R0\n", "a", "string", 0, "Starved@1"),

		// A negative lookahead over a braced choice (`eol` is one) after a repetition that can give
		// back: the body refusing is the look succeeding, and the reading goes on to accept. The
		// engine fails the parse there instead.
		["LookOverBracedChoiceRefused"] = new("R0 = (['a'..'c']+)+ & ?!({ eol })\nparse R0\n", "a", "string", 0, "Success@0+1 a"),

		// Over kinds, a repetition short of its minimum that stopped at the end of the tokens says
		// what it wanted: the direct reader says nothing and answers from the start.
		["TokenRepetitionBelowMinimum"] = new(Lexical + "R0 = Lexical.Digits & ['a'..'c']+\nparse R0\n", "0", "string", 0, "Starved@1", Lexical: true),
	};

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var name in Rows.Keys)
			names.Add(name);

		return names;
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Both_readers_answer_what_the_contract_names(string name)
	{
		var row = Rows[name];

		foreach (var direct in new[] { true, false })
		{
			var parser = Build(row.Grammar, direct, row.Lexical, positionalFollow: false, buffered: row.Buffered);
			var answer = Answer(parser, "R0", row.Form, row.Input, row.At);

			Assert.True(answer == row.Expected, $"{name} ({(direct ? "direct" : "engine")}, {row.Form}): expected {row.Expected}, got {answer}\n{row.Grammar}");
		}
	}

	/// <summary>
	/// A reading from a position gives the rule's first reading (§6.3). These shapes answer so
	/// only when the grammar is compiled knowing it is read from a position
	/// (<see cref="GramCompilerOptions.PositionalFollow"/>); by default each reader cuts one of
	/// them short, and the two disagree.
	/// </summary>
	[Theory]
	[InlineData("R0 : @int = ((\"yz\"){1,2})? => @(0)\nparse R0\n", "ya", "Success@0+0 0")]
	[InlineData("R0 = \"ab\" & ('d' & 'r')* & 'd'\nparse R0\n", "abdrc", "Success@0+3 abd")]
	[InlineData("R0 = Std.Quoted('\"')\nparse R0\n", "\"qa\"\"", "Success@0+4 \"qa\"")]
	public void A_reading_from_a_position_is_the_rules_first_reading_by_default(string grammar, string input, string expected)
	{
		foreach (var direct in new[] { true, false })
		{
			var parser = Build(grammar, direct, lexical: false, positionalFollow: false, buffered: true);
			var answer = Answer(parser, "R0", "at", input, 0);

			Assert.True(answer == expected, $"{(direct ? "direct" : "engine")}: expected {expected}, got {answer}\n{grammar}");
		}
	}

	// --- Differential ---------------------------------------------------------------------

	/// <summary>
	/// Random grammars over the whole syntax, every input up to five characters over a small
	/// alphabet, read whole as a string, from a reader and from a stream: the direct reader and the
	/// engine give one outcome, one position and one value.
	/// </summary>
	[Fact]
	public void Random_grammars_read_alike_whole_in_both_readers()
	{
		var failures = Differential(seed: 20261026, count: 40, forms: ["string", "reader", "stream"], positionalFollow: false);

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	/// <summary>
	/// The same from a position, with the grammar compiled knowing it is read from one: what
	/// remains is not the follow of the whole forms but the readers' own disagreements.
	/// </summary>
	[Fact]
	public void Random_grammars_read_alike_from_a_position_in_both_readers()
	{
		var failures = Differential(seed: 20261026, count: 40, forms: ["at"], positionalFollow: true);

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	static List<string> Differential(int seed, int count, string[] forms, bool positionalFollow)
	{
		var random   = new Random(seed);
		var failures = new List<string>();
		var compiled = 0;

		for (var round = 0; round < count * 3 && compiled < count; round++)
		{
			var grammar = new Generator(random).Build();

			Assembly direct, engine;

			// A host recognizer over a span is not offered the buffered forms.
			var buffered = !grammar.Contains("@Odd", StringComparison.Ordinal);

			try
			{
				direct = Build(grammar, direct: true, lexical: false, positionalFollow, buffered);
				engine = Build(grammar, direct: false, lexical: false, positionalFollow, buffered);
			}
			catch (Xunit.Sdk.XunitException)
			{
				// Refused by the compiler (a shape the generator does not keep clear of), or the
				// emitted source did not compile: not this test's question.
				continue;
			}

			compiled++;

			var found = 0;

			foreach (var input in Inputs("abc; ", 5))
			{
				foreach (var form in forms)
				{
					var at = form == "at" ? Math.Min(1, input.Length) : 0;
					var d  = Answer(direct, "R0", form, input, at);
					var e  = Answer(engine, "R0", form, input, at);

					if (d != e && found++ < 3)
						failures.Add($"round {round}, {form} \"{input}\": direct {d}, engine {e}\n{grammar}");
				}

				if (found >= 3)
					break;
			}
		}

		Assert.True(compiled == count, $"only {compiled} of {count} grammars compiled");

		return failures;
	}

	/// <summary>Random grammars over the syntax the readers disagree on: sequences, choices, repetitions, looks, captures, recovery, guards, trivia.</summary>
	sealed class Generator(Random random)
	{
		readonly List<string?> _types = [];

		int _looking;

		bool Chance(int percent)
		{
			return random.Next(100) < percent;
		}

		T Pick<T>(params T[] items)
		{
			return items[random.Next(items.Length)];
		}

		public string Build()
		{
			var count = 1 + random.Next(3);

			for (var i = 0; i < count; i++)
				_types.Add(Pick<string?>(null, null, "@int", "@int", "@int[]"));

			for (var i = 0; i < count; i++)
				if (_types[i] == "@int[]" && !_types.Skip(i + 1).Contains("@int"))
					_types[i] = "@int";

			var text = new StringBuilder();

			if (Chance(40))
				text.Append(Pick("trivia = ' '*\n", "trivia = { ' '* }\n"));

			for (var i = 0; i < count; i++)
				text.Append(Rule(i)).Append('\n');

			text.Append("parse R0\n");

			return text.ToString();
		}

		string Rule(int index)
		{
			var type = _types[index];
			var head = type is null ? $"R{index}" : $"R{index} : {type}";

			if (type == "@int[]")
			{
				var element = Element(index)!;
				var sync    = Pick("';'", "@Odd", "(';' & when @(true))");

				return $"{head} = {element}{Pick("*", "+")}" + (Chance(70) ? $" recover {sync}" + (Chance(40) ? " => @(0)" : "") : "");
			}

			var alternatives = new List<string>();

			for (var i = 0; i < 1 + (Chance(35) ? 1 : 0); i++)
				alternatives.Add(Alternative(index, type));

			var body = string.Join(" | ", alternatives);

			if (index == 0 && type is null && Chance(40) && alternatives.Count == 1)
				body = $"{body} & eof";

			return $"{head} = {body}";
		}

		string? Element(int index)
		{
			for (var i = index + 1; i < _types.Count; i++)
				if (_types[i] == "@int")
					return $"R{i}";

			return null;
		}

		string Alternative(int index, string? type)
		{
			var operands = new List<string>();
			var captures = 0;
			var length   = 1 + random.Next(3);

			for (var i = 0; i < length; i++)
			{
				var operand = Operand(index, 2);

				if (type == "@int" && Chance(40) && !operand.Contains("@Odd", StringComparison.Ordinal) && !operand.StartsWith('?'))
				{
					operands.Add($"c{captures}: {operand}");

					if (Chance(25))
						operands.Add($"when @(c{captures}.Length >= 0)");

					captures++;
				}
				else
				{
					operands.Add(operand);
				}
			}

			var sequence = string.Join(" & ", operands);

			if (type == "@int")
			{
				var sum = captures == 0 ? Pick("0", "parserText.Length") : string.Join(" + ", Enumerable.Range(0, captures).Select(static one => $"c{one}.Length"));

				return $"{sequence} => @({sum})";
			}

			return sequence;
		}

		string Operand(int index, int depth)
		{
			var roll = random.Next(depth > 0 ? 24 : 12);

			switch (roll)
			{
				case 0: return "'a'";
				case 1: return "'b'";
				case 2: return "\"ab\"";
				case 3: return "['a'..'c']";
				case 4: return "['a'..'c']+";
				case 5: return "[^ ';' | ' ']+";
				case 6: return "any";
				case 7: return "';'";
				case 8: return "@Odd";
				case 9: return Call(index);
				case 10: return Call(index);
				case 11: return "'d' & when @(true)";
			}

			var looks = roll is 18 or 19;

			if (looks)
				_looking++;

			var inner = Operand(index, depth - 1);

			if (looks)
				_looking--;

			var body = Empty(inner) ? $"{inner} & 'r'" : inner;

			switch (roll)
			{
				case 12: return $"({inner})?";
				case 13: return $"({body})*";
				case 14: return $"({body})+";
				case 15: return $"({body}){{1,2}}";
				case 16: return $"{{ {inner} }}";
				case 17: return $"({inner} | {Operand(index, depth - 1)})";
				case 18: return $"?=({inner})";
				case 19: return $"?!({inner})";
				case 20: return "(?!';' & any)* & ';'";
				case 21: return $"({inner} & {Operand(index, depth - 1)})";
				case 22 when _looking == 0: return $"({body})* recover {Pick("';'", "@Odd")}";
				default: return inner;
			}
		}

		static bool Empty(string operand)
		{
			return operand.EndsWith('?') || operand.EndsWith(")*", StringComparison.Ordinal) ||
				operand.StartsWith('?') || operand.Contains("recover", StringComparison.Ordinal) ||
				operand.Contains(" | ", StringComparison.Ordinal) || operand.StartsWith('R') ||
				operand.StartsWith('{') || operand.StartsWith('(') || operand.Contains("when", StringComparison.Ordinal);
		}

		string Call(int index)
		{
			if (index + 1 >= _types.Count)
				return "'z'";

			return $"R{index + 1 + random.Next(_types.Count - index - 1)}";
		}
	}

	// --- Building and asking --------------------------------------------------------------

	static Assembly Build(string grammar, bool direct, bool lexical, bool positionalFollow, bool buffered)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName        = "Grammar",
			Direct           = direct,
			Lexical          = lexical,
			BufferedInput    = buffered,
			BufferedBytes    = buffered,
			PositionalFollow = positionalFollow,
			SymbolResolver   = PermissiveSymbolResolver.Instance,
			CSharpScanner    = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(
			result.Sources[0].Text, declarationMembers: Host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= longest; length++)
		{
			var next = new List<string>(level.Count * alphabet.Length);

			foreach (var head in level)
				foreach (var one in alphabet)
				{
					var input = head + one;

					next.Add(input);

					yield return input;
				}

			level = next;
		}
	}

	/// <summary>
	/// What a publication answers in one form, as one string: the outcome and the position of a
	/// refusal, or where a success began, its length and its value. Null where the form is not
	/// published for this grammar.
	/// </summary>
	static string? Answer(Assembly parser, string rule, string form, string input, int at)
	{
		var type   = parser.GetType("Grammar")!;
		var name   = "TryParse" + rule;
		var method = form switch
		{
			"string" => type.GetMethod(name, [typeof(string)]),
			"at"     => type.GetMethod(name, [typeof(string), typeof(int)]),
			"reader" => type.GetMethod(name, [typeof(TextReader), typeof(int?), typeof(int?)]),
			"stream" => type.GetMethod(name, [typeof(Stream), typeof(int?), typeof(int?)]),
			_        => throw new ArgumentOutOfRangeException(nameof(form)),
		};

		if (method is null)
			return null;

		object match;

		try
		{
			match = form switch
			{
				"string" => method.Invoke(null, [input])!,
				"at"     => method.Invoke(null, [input, at])!,
				"reader" => method.Invoke(null, [new StringReader(input), 1, null])!,
				_        => method.Invoke(null, [new MemoryStream(Encoding.UTF8.GetBytes(input)), 1, null])!,
			};
		}
		catch (TargetInvocationException thrown)
		{
			return "Threw " + thrown.InnerException!.GetType().Name;
		}

		object? Read(string member)
		{
			return match.GetType().GetProperty(member)!.GetValue(match);
		}

		if (!(bool)Read("IsSuccess")!)
			return $"{Read("Outcome")}@{Read("Position")}";

		var value = Read("Value") switch
		{
			int[] many => "[" + string.Join(",", many) + "]",
			null       => "null",
			var one    => one.ToString(),
		};

		return $"Success@{Read("Position")}+{Read("Length")} {value}";
	}
}
